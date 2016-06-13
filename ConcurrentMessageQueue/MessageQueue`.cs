using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConcurrentMessageQueue.Logging;
using ConcurrentMessageQueue.Scheduling;

namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 内存并发处理消息队列
    /// </summary>
    /// <typeparam name="TMessage">消息对象</typeparam>
    internal class MessageQueue<TMessage>
    {
        private readonly BlockingCollection<MessageWrapper<TMessage>> _consumingMessageQueue;
        private readonly ConcurrentDictionary<MessageId, MessageWrapper<TMessage>> _handlingMessageDict;
        private readonly AsyncQueueSetting _settings;
        private readonly Interval _scheduleService;
        private readonly WorkerManager _workerManager;
        private readonly ILogger _logger;

        private int _running;
        private IMessageHandler<TMessage> _messageHandler;

        /// <summary>
        /// 构建MessageQueue
        /// </summary>
        /// <param name="settings">消息队列设置</param>
        /// <param name="logger">日志对象</param>
        public MessageQueue(AsyncQueueSetting settings, ILogger logger)
        {
            var queue = new ConcurrentQueue<MessageWrapper<TMessage>>();
            this._consumingMessageQueue = new BlockingCollection<MessageWrapper<TMessage>>(queue, settings.MaxConsumeringMessageCount);
            this._handlingMessageDict = new ConcurrentDictionary<MessageId, MessageWrapper<TMessage>>();
            this._settings = settings;
            this._scheduleService = new Interval(logger);
            this._workerManager = new WorkerManager(logger, this._settings.WorkThreadCount);
            this._logger = logger;
        }

        /// <summary>
        /// 开始获取和处理消息，该方法仅能被调用一次
        /// </summary>
        /// <returns><see cref="MessageQueue{TMessage}"/></returns>
        public virtual MessageQueue<TMessage> Start()
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 1)
            {
                throw new InvalidOperationException("Start()方法只能被调用一次.");
            }

            this._workerManager.Start("Queue.ConsumingMessage", HandleMessage);
            return this;
        }

        /// <summary>
        /// 停止获取和处理消息
        /// </summary>
        public virtual void Stop()
        {
            if (Interlocked.CompareExchange(ref _running, 0, 1) == 1)
            {
                this._scheduleService.StopTask("Queue.PullMessage");
                this._workerManager.Stop();
            }
        }

        /// <summary>
        /// 设置自定义的如何获取消息和如何处理消息
        /// </summary>
        /// <param name="messageHandler">处理消息对象</param>
        /// <returns></returns>
        internal MessageQueue<TMessage> SetMessageHandle(IMessageHandler<TMessage> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            if (this._running == 1)
            {
                throw new InvalidOperationException("队列正在运行，不能进行此操作.");
            }
            this._messageHandler = messageHandler;
            return this;
        }

        internal void Add(TMessage message, MessageId messageId)
        {
            var messageWrapper = new MessageWrapper<TMessage> {MessageId = messageId, Message = message};
            this._consumingMessageQueue.Add(messageWrapper);
        }

        protected int GetQueueCount()
        {
            return this._consumingMessageQueue.Count;
        }

        protected List<TMessage> GetProcessingMessageSnapshot()
        {
            var processingItems = this._consumingMessageQueue.ToList();
            var handlingItems = this._handlingMessageDict.Values.ToList();
            return processingItems.Concat(handlingItems).Select(wrapper => wrapper.Message).ToList();
        }

        private void HandleMessage()
        {
            var message = this._consumingMessageQueue.Take();
            var messageId = message.MessageId;

            if (!this._handlingMessageDict.TryAdd(messageId, message))
            {
                this._logger.Warn("ignore to handle message [MessageId:{0}]", messageId);
                return;
            }

            try
            {
                this._messageHandler.Handle(new MessageContext<TMessage>(message.Message, message.MessageId));
                this.RemoveHandledMessage(message);
            }
            catch (Exception ex)
            {
                LogMessageHandlingException(message, ex);
            }
        }

        private void LogMessageHandlingException(MessageWrapper<TMessage> message, Exception exception)
        {
            this._logger.Error(string.Format(
                "Message handling has exception, message info:[MessageId={0}]", message.MessageId
                ), exception);
        }

        private void RemoveHandledMessage(MessageWrapper<TMessage> consumingMessage)
        {
            MessageWrapper<TMessage> consumedMessage;
            if (!_handlingMessageDict.TryRemove(consumingMessage.MessageId, out consumedMessage))
            {
                //
            }
        }
        
    }

}
