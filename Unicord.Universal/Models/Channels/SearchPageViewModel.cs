﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Unicord.Universal.Models.Messages;
using Windows.UI.Xaml.Data;

namespace Unicord.Universal.Models.Channels
{
    public class SearchPageViewModel : ViewModelBase
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ChannelViewModel _channel;
        private readonly ILogger<SearchPageViewModel> _logger
            = Logger.GetLogger<SearchPageViewModel>();

        private int _currentPage = 1;
        private int _totalPages = 1;
        private bool _waitingForIndex;
        private bool _isSearching;
        private int _totalMessages;

        public bool IsSearching
        {
            get => _isSearching;
            set => OnPropertySet(ref _isSearching, value);
        }

        public bool WaitingForIndex
        {
            get => _waitingForIndex;
            set => OnPropertySet(ref _waitingForIndex, value);
        }

        public int TotalMessages
        {
            get => _totalMessages;
            set => OnPropertySet(ref _totalMessages, value, nameof(TotalMessages), nameof(TotalMessagesString), nameof(CanGoBack), nameof(CanGoForward));
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => OnPropertySet(ref _currentPage, value, nameof(CurrentPage), nameof(CanGoBack), nameof(CanGoForward));
        }

        public int TotalPages
        {
            get => _totalPages;
            set => OnPropertySet(ref _totalPages, value, nameof(TotalPages), nameof(CanGoBack), nameof(CanGoForward));
        }

        public string TotalMessagesString =>
            TotalMessages.ToString("N0");

        public bool CanGoForward =>
            CurrentPage < TotalPages;

        public bool CanGoBack =>
            CurrentPage > 1;

        public CollectionViewSource ViewSource { get; set; }

        public SearchPageViewModel(ChannelViewModel channel)
        {
            _channel = channel;
            ViewSource = new CollectionViewSource { IsSourceGrouped = false };
            WaitingForIndex = false;
        }

        public async Task SearchAsync(string content)
        {
            await _semaphore.WaitAsync();

            IsSearching = true;
            ViewSource.Source = Array.Empty<object>();

            try
            {
                DiscordSearchResult result = null;
                if (_channel.Guild != null)
                    result = await discord.SearchAsync(_channel.Guild.Guild, content, offset: (CurrentPage - 1) * 25);
                else
                    result = await discord.SearchAsync(_channel.Channel, content, offset: (CurrentPage - 1) * 25);

                if (!result.IsIndexed)
                {
                    WaitingForIndex = true;
                    TotalMessages = 0;
                }
                else
                {
                    WaitingForIndex = false;
                    TotalMessages = result.TotalResults;
                    TotalPages = (int)Math.Ceiling(result.TotalResults / 25d);
                    ViewSource.Source = result.Messages
                                              .Select(v => new MessageViewModel(v[0]))
                                              .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load search results");
            }
            finally
            {
                _semaphore.Release();
                IsSearching = false;
            }
        }
    }
}
