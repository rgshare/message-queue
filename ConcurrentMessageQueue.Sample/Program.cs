using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConcurrentMessageQueue.Sample
{
    class Program
    {
        private static int _index;

        static void Main()
        {
            var settings = AsyncQueueSetting.Create(2, 50, 10);
            var logger = new ConsoleLogger();

            var queue = AsyncQueue.Create<string>(settings, logger)
                                  .From(CreateMessage, TimeSpan.Zero)
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .Handle(context => Console.WriteLine(context.Message))
                                  .Start();

            Console.WriteLine("队列已开始运行，按任意键退出");
            Console.ReadKey();
            queue.Stop();
        }

        static IEnumerable<string> CreateMessage(int count)
        {
            var start = _index;
            _index += count + 1;
            return Enumerable.Range(start, count).Select(i => i.ToString());
        }
    }
}
