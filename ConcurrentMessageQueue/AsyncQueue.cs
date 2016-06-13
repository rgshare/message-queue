using System;
using System.Collections.Generic;
using ConcurrentMessageQueue.Logging;

namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 异步队列工厂类
    /// </summary>
    /// <typeparam name="TMessage">消息对象</typeparam>
    public interface IAsyncQueueFactory<TMessage>
    {
        /// <summary>
        /// 设置消息的来源，队列会不断的从该来源拉取消息
        /// </summary>
        /// <param name="source">消息的来源</param>
        /// <returns>主动拉取消息异步队列</returns>
        IPullAsyncQueue<TMessage> From(IMessageSource<TMessage> source);
        
        /// <summary>
        /// 决定如何获取消息，队列会按照指定的时间间隔来去消息
        /// </summary>
        /// <param name="source">消息的来源</param>
        /// <param name="interval">获取消息间隔</param>
        /// <returns>主动拉取消息异步队列</returns>
        IPullAsyncQueue<TMessage> From(IMessageSource<TMessage> source, TimeSpan interval);

        /// <summary>
        /// 决定如何获取消息，队列会按照指定的时间间隔来去消息
        /// </summary>
        /// <param name="source">消息的来源</param>
        /// <param name="interval">获取消息间隔</param>
        /// <returns>主动拉取消息异步队列</returns>
        IPullAsyncQueue<TMessage> From(Func<int, IEnumerable<TMessage>> source, TimeSpan interval);
    }

    /// <summary>
    /// 主动拉取消息异步队列
    /// </summary>
    /// <typeparam name="TMessage">消息对象</typeparam>
    public interface IPullAsyncQueue<TMessage>
    {
        /// <summary>
        /// 去除重复的消息对象
        /// </summary>
        /// <param name="messageEqualityComparer">消息应该如何进行相等比较</param>
        /// <returns></returns>
        IPullAsyncQueue<TMessage> Distinct(IEqualityComparer<TMessage> messageEqualityComparer);
        /// <summary>
        /// 设置自定义的如何获取消息和如何处理消息
        /// </summary>
        /// <param name="messageHandler">处理消息对象</param>
        /// <returns></returns>
        IPullAsyncQueue<TMessage> Handle(IMessageHandler<TMessage> messageHandler);
        /// <summary>
        /// 设置自定义的如何获取消息和如何处理消息
        /// </summary>
        /// <param name="messageHandler">处理消息对象</param>
        /// <returns></returns>
        IPullAsyncQueue<TMessage> Handle(Action<IMessageContext<TMessage>> messageHandler);
        /// <summary>
        /// 开始获取和处理消息，该方法仅能被调用一次
        /// </summary>
        IAsyncQueueStop Start();
    }
    
    /// <summary>
    /// 停止队列
    /// </summary>
    public interface IAsyncQueueStop
    {
        /// <summary>
        /// 停止队列
        /// </summary>
        void Stop();
    }

    internal sealed class AsyncQueue<TMessage> : IAsyncQueueFactory<TMessage>, IPullAsyncQueue<TMessage>, IAsyncQueueStop
    {
        private readonly AsyncQueueSetting _setting;
        private readonly ILogger _logger;
        private IMessageSource<TMessage> _source;
        private IMessageHandler<TMessage> _handler;
        private TimeSpan _interval;
        private IEqualityComparer<TMessage> _messageEqualityComparer;

        private MessageQueue<TMessage> _messageQueue; 

        internal AsyncQueue(AsyncQueueSetting settings, ILogger logger)
        {
            this._setting = settings;
            this._logger = logger;
        }

        IPullAsyncQueue<TMessage> IAsyncQueueFactory<TMessage>.From(IMessageSource<TMessage> source)
        {
            return ((IAsyncQueueFactory<TMessage>)this).From(source, TimeSpan.Zero);
        }

        IPullAsyncQueue<TMessage> IAsyncQueueFactory<TMessage>.From(Func<int, IEnumerable<TMessage>> source, TimeSpan interval)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            var anonymousMessageSource = new AnonymousMessageSource<TMessage>(source);
            return ((IAsyncQueueFactory<TMessage>)this).From(anonymousMessageSource, interval);
        }

        IPullAsyncQueue<TMessage> IAsyncQueueFactory<TMessage>.From(IMessageSource<TMessage> source, TimeSpan interval)
        {
            this._source = source;
            this._interval = interval;
            return this;
        }
        
        IPullAsyncQueue<TMessage> IPullAsyncQueue<TMessage>.Distinct(IEqualityComparer<TMessage> messageEqualityComparer)
        {
            this._messageEqualityComparer = messageEqualityComparer;
            return this;
        }

        IPullAsyncQueue<TMessage> IPullAsyncQueue<TMessage>.Handle(Action<IMessageContext<TMessage>> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            var anonymousMessageHande = new AnonymousMessageHandle<TMessage>(messageHandler);
            return ((IPullAsyncQueue<TMessage>)this).Handle(anonymousMessageHande);
        }

        IPullAsyncQueue<TMessage> IPullAsyncQueue<TMessage>.Handle(IMessageHandler<TMessage> messageHandler)
        {
            this._handler = messageHandler;
            return this;
        }
        
        IAsyncQueueStop IPullAsyncQueue<TMessage>.Start()
        {
            var queue = new PullMessageQueue<TMessage>(this._setting, this._logger, this._messageEqualityComparer);
            queue.SetMessageSource(this._source, this._interval);
            queue.SetMessageHandle(this._handler);
            queue.Start();
            this._messageQueue = queue;
            return this;
        }

        void IAsyncQueueStop.Stop()
        {
            if (this._messageQueue != null)
            {
                this._messageQueue.Stop();
            }
        }
    }

    /// <summary>
    /// AsyncQueue工厂类
    /// </summary>
    public sealed class AsyncQueue
    {
        /// <summary>
        /// 创建AsyncQueue
        /// </summary>
        /// <typeparam name="TMessage">消息类型</typeparam>
        /// <param name="settings">队列的配置信息</param>
        /// <param name="logger">日志对象</param>
        public static IAsyncQueueFactory<TMessage> Create<TMessage>(AsyncQueueSetting settings, ILogger logger)
        {
            return new AsyncQueue<TMessage>(settings, logger);
        }
        
    }
    
}
