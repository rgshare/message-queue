using System;
using System.Linq;

namespace ConcurrentMessageQueue.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = MessageQueueSetting.Create(5, 50, 10);
            var logger = new ConsoleLogger();

            var queue = AsyncQueue.Create<string>(settings)
                                  .From(count => Enumerable.Repeat("", count), TimeSpan.Zero)
                                  //.Distinct()
                                  .Handle(Console.WriteLine)
                                  .Start();

            Console.WriteLine("队列已开始运行，按任意键退出");
            Console.ReadKey();
            queue.Stop();
        }
    }
}
