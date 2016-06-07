using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConcurrentMessageQueue.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = MessageQueueSetting.Create(5, 50, 10);
            var logger = new ConsoleLogger();
            var handle = new MessageQueueClient();
            var queue = new MessageQueue<Message>(settings, logger);
            queue.SetMessageHandle(handle, handle);
            queue.Start();

            Console.WriteLine("队列已开始运行，按任意键退出");
            Console.ReadKey();
            queue.Stop();
        }
    }
}
