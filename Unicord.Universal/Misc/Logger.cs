﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Unicord.Universal
{
    internal static class Logger
    {
#if !STORE
        private static ConcurrentQueue<LogMessage> _messages = new ConcurrentQueue<LogMessage>();
        private static Task _loggerThread;
        private static bool _logging;

        static Logger()
        {
            _logging = true;
            _loggerThread = Task.Run(LoggerLoopAsync);
        }

        private static async Task LoggerLoopAsync()
        {
            while (_logging)
            {
                while (_messages.TryDequeue(out var message))
                {
                    var prefix = $"[{message.Source}:{message.DateTime}:{message.LineNumber}] ";
                    foreach (var str in message.Message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Debug.WriteLine($"{prefix}{str}");
                    }
                }

                await Task.Delay(1000);
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message, [CallerMemberName] string source = "General", [CallerLineNumber] int line = 0)
        {
#if !STORE
            _messages.Enqueue(new LogMessage() { Message = message.ToString(), Source = source, LineNumber = line, DateTime = DateTimeOffset.Now });
#endif
        }
    }

#if !STORE
    internal struct LogMessage
    {
        public string Message;
        public string Source;
        public int LineNumber;
        public DateTimeOffset DateTime;
    }
#endif

}

