using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobsQueue
{
    public class User
    {
        public string Mobile { get; set; }

        public string Pwd { get; set; }

        public override string ToString()
        {
            return $"{Mobile},{Pwd}";
        }
    }

    /// <summary>
    /// 双缓冲队列示例
    /// http://www.codeproject.com/Articles/27703/Producer-Consumer-Using-Double-Queues
    /// </summary>
    internal class DoubleBufferQueue
    {
        private ConcurrentQueue<User> _writeQueue;
        private ConcurrentQueue<User> _readQueue;
        private volatile ConcurrentQueue<User> _currentQueue;

        private AutoResetEvent _dataEvent;

        public DoubleBufferQueue()
        {
            _writeQueue = new ConcurrentQueue<User>();
            _readQueue = new ConcurrentQueue<User>();
            _currentQueue = _writeQueue;

            _dataEvent = new AutoResetEvent(false);
            Task.Factory.StartNew(() => ConsumerQueue(), TaskCreationOptions.None);
        }

        public void ProducerFunc(User user)
        {
            _currentQueue.Enqueue(user);
            _dataEvent.Set();
        }

        public void ConsumerQueue()
        {
            ConcurrentQueue<User> consumerQueue;
            User user;
            int allcount = 0;
            Stopwatch watch = Stopwatch.StartNew();
            while (true)
            {
                _dataEvent.WaitOne();
                if (!_currentQueue.IsEmpty)
                {
                    _currentQueue = (_currentQueue == _writeQueue) ? _readQueue : _writeQueue;
                    consumerQueue = (_currentQueue == _writeQueue) ? _readQueue : _writeQueue;
                    while (!consumerQueue.IsEmpty)
                    {
                        while (!consumerQueue.IsEmpty)
                        {
                            if (consumerQueue.TryDequeue(out user))
                            {
                                Console.WriteLine(user.ToString());
                                allcount++;
                            }
                        }
                        Console.WriteLine($"当前个数{allcount}，花费了{watch.ElapsedMilliseconds}ms;");
                        Thread.Sleep(20);
                    }
                }
            }
        }
    }
}
