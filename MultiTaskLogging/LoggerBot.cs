using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTaskLogging
{
    public static class LoggerBot
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static async Task LogAction(int? taskId)
        {
            while (true)
            {
                await _semaphore.WaitAsync();
                await WriteMessageToConsole(taskId);
                _semaphore.Release();
            }
        }

        private static async Task WriteMessageToConsole(int? taskId)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(75));
            var message = GetRandomActionMsg();
            Console.WriteLine($"{message} Task# {taskId}\t\t{DateTime.UtcNow}");
        }
        
        private static string GetRandomActionMsg()
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
    }
}