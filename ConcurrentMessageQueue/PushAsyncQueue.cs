using System;
using JobsQueue.Logging;

namespace JobsQueue
{
    /// <summary>
    /// 手动往队列中推送数据
    /// </summary>
    /// <typeparam name="TMessage">消息</typeparam>
    public sealed class PushAsyncQueue<TMessage>
    {
        private readonly AsyncQueueSetting _setting;
        private readonly ILogger _logger;
        private IMessageHandler<TMessage> _handler;

        private MessageQueue<TMessage> _messageQueue;

        /// <summary>
        /// 构造一个异步队列
        /// </summary>
        /// <param name="settings">配置信息</param>
        public PushAsyncQueue(AsyncQueueSetting settings)
            :this(settings, null)
        {
        }

        /// <summary>
        /// 构造一个异步队列
        /// </summary>
        /// <param name="settings">配置信息</param>
        /// <param name="logger"></param>
        public PushAsyncQueue(AsyncQueueSetting settings, ILogger logger)
        {
            this._setting = settings;
            this._logger = logger ?? new NullLogger();
            this._messageQueue = new MessageQueue<TMessage>(this._setting, this._logger);
        }

        /// <summary>
        /// 设置自定义的如何获取消息和如何处理消息
        /// </summary>
        /// <param name="messageHandler">处理消息对象</param>
        /// <returns></returns>
        public PushAsyncQueue<TMessage> SetMessageHandle(Action<IMessageContext<TMessage>> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            var anonymousMessageHande = new AnonymousMessageHandle<TMessage>(messageHandler);
            return this.SetMessageHandle(anonymousMessageHande);
        }

        /// <summary>
        /// 设置自定义的如何获取消息和如何处理消息
        /// </summary>
        /// <param name="messageHandler">处理消息对象</param>
        /// <returns></returns>
        public PushAsyncQueue<TMessage> SetMessageHandle(IMessageHandler<TMessage> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            this._messageQueue.SetMessageHandle(messageHandler);
            return this;
        }

        /// <summary>
        /// 向队列中添加消息
        /// </summary>
        /// <param name="message">消息对象</param>
        /// <returns>队列</returns>
        public PushAsyncQueue<TMessage> Add(TMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            var messageId = new MessageId(Guid.NewGuid().ToString("N"));
            this._messageQueue.Add(message, messageId);
            return this;
        }

        /// <summary>
        /// 开始获取和处理消息，该方法仅能被调用一次
        /// </summary>
        public PushAsyncQueue<TMessage> Start()
        {
            this._messageQueue.Start();
            return this;
        }

        /// <summary>
        /// 停止队列
        /// </summary>
        public void Stop()
        {
            if (this._messageQueue != null)
            {
                this._messageQueue.Stop();
            }
        }
    }
}
