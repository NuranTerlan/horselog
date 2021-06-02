using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTaskLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            var taskList = Generators.GenerateNLoggerBot(20);
            Task.WaitAll(taskList.ToArray());
        }
    }
}