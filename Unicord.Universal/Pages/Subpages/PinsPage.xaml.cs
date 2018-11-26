﻿using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unicord.Universal.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Unicord.Universal.Pages.Subpages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PinsPage : Page
    {
        public PinsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is DiscordChannel channel)
            {
                try
                {
                    messages.Children.Clear();
                    noMessages.Visibility = Visibility.Collapsed;
                    ratelimited.Visibility = Visibility.Collapsed;
                    progress.IsActive = true;

                    var pins = await channel.GetPinnedMessagesAsync();
                    if (pins.Any())
                    {
                        var messageViewerFactory = MessageViewerFactory.GetForCurrentThread();

                        foreach (var message in pins.Reverse())
                        {
                            var viewer = messageViewerFactory.GetViewerForMessage(message, channel);
                            //viewer.AutoSize = false;
                            messages.Children.Add(viewer);
                        }
                    }
                    else
                    {
                        noMessages.Visibility = Visibility.Visible;
                    }
                }
                catch (RateLimitException)
                {
                    ratelimited.Visibility = Visibility.Visible;
                }
                catch (Exception)
                {

                }
            }

            progress.IsActive = false;
        }
    }
}
