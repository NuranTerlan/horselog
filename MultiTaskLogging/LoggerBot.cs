using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTaskLogging
{
    public static class LoggerBot
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        public static async Task LogAction(int? taskId)
        {
            while (true)
            {
                await Semaphore.WaitAsync();
                var message = await WriteActionToConsole(taskId);
                await WriteToFile(message);
                Semaphore.Release();
            }
        }

        private static async Task<string> WriteActionToConsole(int? taskId)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            var message = GetRandomActionMsg();
            var printedMessage = $"{message} Task# {taskId}\t\t{DateTime.UtcNow}";
            Console.WriteLine(printedMessage);
            return printedMessage;
        }

        private static async Task WriteToFile(string line)
        {
            await using var writer = new StreamWriter("F:\\Simbrella-Tasks\\MultiTaskLogging\\logs\\log.txt", true);
            await writer.WriteLineAsync(line);
            await writer.DisposeAsync();
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