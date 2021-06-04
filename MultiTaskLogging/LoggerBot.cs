using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTaskLogging
{
    public class LoggerBot : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly NameValueCollection _configurations = ConfigurationManager.AppSettings;
        
        private readonly List<string> _actionLogs;
        private readonly int _minLogsBound;
        private readonly string _filePath;

        public LoggerBot()
        {
            _semaphore = new SemaphoreSlim(1, 1);
            int.TryParse(_configurations.Get("min-log-per-proc"), out var minLog);
            _minLogsBound = minLog;
            _filePath = _configurations.Get("file-path");
            _actionLogs = new List<string>();
        }

        // ~LoggerBot()
        // {
        //     // when use this component as disposable which means use in using block then use commented code below
        //     // and comment out ReleaseUnmanagedResources();
        //     //Dispose(false);
        //     
        //     ReleaseUnmanagedResources();
        // }
        
        public async Task LogAction(int? taskId)
        {
            while (true)
            {
                var message = GetActionMessage(taskId);
                await _semaphore.WaitAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(2));
                if (message != string.Empty)
                {
                    _actionLogs.Add(message);
                }
                _semaphore.Release();
            }
        }

        private string GetActionMessage(int? taskId)
        {
            var message = GetRandomActionMsg();

            if (taskId == null)
            {
                Console.WriteLine($"\t\tLogger doesn't catch that who log this message: '{message}'");
                return string.Empty;
            }
            
            var printedMessage = $"{message} Task# {taskId.ToString().PadRight(15, ' ')} {DateTime.UtcNow.ToLocalTime()}";
            // Console.WriteLine(printedMessage);
            return printedMessage;

        }

        public async Task WriteToFile(int? taskId)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(3));
                var writtenLogs = string.Empty;
                var writtenLogsCount = 0;
                var canBeAccessFile = false;
                await _semaphore.WaitAsync();
                if (_actionLogs.Count > _minLogsBound)
                {
                    canBeAccessFile = true;
                    writtenLogs = string.Join(Environment.NewLine, _actionLogs);
                    writtenLogsCount = _actionLogs.Count;
                    _actionLogs.Clear();
                }
                _semaphore.Release();

                if (canBeAccessFile)
                {
                    await using var writer = new StreamWriter(_filePath ?? string.Empty, true);
                    try
                    {
                        await writer.WriteLineAsync(writtenLogs);
                        Console.WriteLine($"\n\t{writtenLogsCount} files added to {Path.GetFileName(_filePath)} by Task# {taskId} (Writer-Task)\n");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        await writer.DisposeAsync();
                    }
                }
            }
        }

        private string GetRandomActionMsg()
        {
            var actionsCount = Enum.GetNames(typeof(Actions)).Length;
            var customLogMessages = new List<string>(actionsCount * 3);
            for (var i = 0; i < actionsCount; i++)
            {
                var commonMessage = $"Item (x) is {Enum.GetName(typeof(Actions), i)}: ";
                customLogMessages.AddRange(new string[]
                {
                    commonMessage + "SUCCESS.",
                    commonMessage + "FAIL!",
                    commonMessage + "PENDING.."
                });
            }
            var random = new Random();
            var maxMsgSize = customLogMessages.Select(m => m.Length).Max();
            var returnedMsg = customLogMessages[random.Next(0, customLogMessages.Count)];
            return returnedMsg.PadRight(maxMsgSize + 15, ' ');
        }

        private void ReleaseUnmanagedResources()
        {
            if (_actionLogs.Count > 0)
            {
                // WriteToFile();
            }
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _semaphore?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}