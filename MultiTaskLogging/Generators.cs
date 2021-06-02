using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTaskLogging
{
    public static class Generators
    {
        public static IEnumerable<Task> GenerateNLoggerBot(int n)
        {
            var returnedTasks = new List<Task>();
            for (var i = 0; i < n; i++)
            {
                returnedTasks.Add(Task.Run(async () => await LoggerBot.LogAction(Task.CurrentId)));
            }
        
            return returnedTasks;
        }
    }
}