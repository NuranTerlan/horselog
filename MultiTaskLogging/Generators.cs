using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTaskLogging
{
    public static class Generators
    {
        public static IEnumerable<Task> GenerateNLoggerBot(int n)
        {
            var returnedTasks = new List<Task>();

            var loggerBot = new LoggerBot();
            
            for (var i = 0; i < n; i++)
            {
                returnedTasks.Add(Task.Run(async () => await loggerBot.LogAction(Task.CurrentId)));
            }
            returnedTasks.Add(Task.Run(async () => await loggerBot.WriteToFile(Task.CurrentId)));

            return returnedTasks;
        }
    }
}