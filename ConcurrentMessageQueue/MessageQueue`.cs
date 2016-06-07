using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public sealed class MessageQueue<TMessage>
    {
        private readonly BlockingCollection<TMessage> _consumingMessageQueue;
        private readonly ConcurrentDictionary<string, TMessage> _handlingMessageDict;
        private readonly MessageQueueSetting _settings;
        private readonly Interval _scheduleService;
        private readonly WorkerManager _workerManager;
        private readonly ILogger _logger;

        private int _running;
        private Func<TMessage, string> _keySelector;
        private IMessageSource<TMessage> _messageFactory;
        private IMessageHandler<TMessage> _messageHandler;

        /// <summary>
        /// 构建MessageQueue
        /// </summary>
        /// <param name="settings">消息队列设置</param>
        /// <param name="logger">日志对象</param>
        public MessageQueue(MessageQueueSetting settings, ILogger logger)
        {
            this._consumingMessageQueue = new BlockingCollection<TMessage>(new ConcurrentQueue<TMessage>());
            this._handlingMessageDict = new ConcurrentDictionary<string, TMessage>();
            this._settings = settings;
            this._scheduleService = new Interval(logger);
            this._workerManager = new WorkerManager(logger, this._settings.WorkThreadCount);
            this._logger = logger;

            InitKeySelector();
        }

        /// <summary>
        /// 开始获取和处理消息，该方法仅能被调用一次
        /// </summary>
        /// <returns><see cref="MessageQueue{TMessage}"/></returns>
        public MessageQueue<TMessage> Start()
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 1)
            {
                throw new InvalidOperationException("Start() must only be called once.");
            }

            //源源不断的读取数据
            var pullMessageInterval = 0;
            this._scheduleService.StartTask("Consumer.PullMessage", PullMessage, pullMessageInterval);
            this._workerManager.Start("Consumer.ConsumingMessage", HandleMessage);
            return this;
        }

        /// <summary>
        /// 停止获取和处理消息
        /// </summary>
        public void Stop()
        {
            this._scheduleService.StopTask("Consumer.PullMessage");
            this._scheduleService.StopTask("Consumer.SendHeartbeat");
            this._workerManager.Stop();
        }

        /// <summary>
        /// 设置自定义的如何获取消息和如何处理消息
        /// </summary>
        /// <param name="messageFactory">获取消息对象</param>
        /// <param name="messageHandler">处理消息对象</param>
        /// <returns></returns>
        public MessageQueue<TMessage> SetMessageHandle(IMessageSource<TMessage> messageFactory, IMessageHandler<TMessage> messageHandler)
        {
            if (messageFactory == null)
            {
                throw new ArgumentNullException("messageFactory");
            }
            if (messageHandler == null)
            {
                throw new ArgumentNullException("messageHandler");
            }
            this._messageFactory = messageFactory;
            this._messageHandler = messageHandler;
            return this;
        }

        /// <summary>
        /// 决定如何查找{TMessage}对象的主键属性，这个会用于稍后的重复消息过滤
        /// </summary>
        private void InitKeySelector()
        {
            var isMessageType = typeof(IMessage).IsAssignableFrom(typeof(TMessage));

            if (isMessageType)
            {
                this._keySelector = message => ((IMessage)message).MessageId;
                return;
            }

            var keys = (from prop in typeof(TMessage).GetProperties()
                        where prop.GetCustomAttributes(typeof(KeyAttribute), true).Length > 0
                        select prop).ToArray();

            if (!keys.Any())
            {
                var error = string.Format("无法找到对象`{0}`的主键，请为该对象的主键属性添加`KeyAttribute`或实现`IMessage`接口", typeof(TMessage));
                throw new InvalidOperationException(error);
            }

            this._keySelector = message =>
            {
                return keys.Select(p => p.GetValue(message, null)).Aggregate(string.Empty, (a, b) => a + b);
            };
        }


        private void PullMessage()
        {
            var min = this._settings.MinConsumeringMessageCount;
            var max = this._settings.MaxConsumeringMessageCount;
            var len = this._consumingMessageQueue.Count;
            if (len < min)
            {
                try
                {
                    var processingMessageIds = GetProcessingMessageIdsSnapshot();//在某些情况下，这里返回的ids是不全面的
                    var prepareEnqueueMessages = (_messageFactory.GetList(max - len) ?? new TMessage[0]).ToArray();
                    var equeuedCount = 0;

                    if (prepareEnqueueMessages.Length == 0) return;

                    foreach (var message in prepareEnqueueMessages)
                    {
                        var messageId = this._keySelector(message);
                        if (processingMessageIds.Contains(messageId))
                        {
                            this._logger.Warn("the meesage is processing, ignore to equeue [MessageId:{0}]", messageId);
                            continue;
                        }

                        this._consumingMessageQueue.Add(message);
                        equeuedCount++;
                    }
                    this._logger.Info("pull {0} messages from data source, enqueue {1}, ignore {2}",
                        prepareEnqueueMessages.Length,
                        equeuedCount,
                        prepareEnqueueMessages.Length - equeuedCount
                        );
                }
                catch (Exception ex)
                {
                    this._logger.Error("pull message error:" + ex.Message, ex);
                }
            }
        }

        private List<string> GetProcessingMessageIdsSnapshot()
        {
            var processingItems = this._consumingMessageQueue.Select(this._keySelector).ToList();
            var handlingItems = this._handlingMessageDict.Keys.ToList();
            return processingItems.Concat(handlingItems).ToList();
        }

        private void HandleMessage()
        {
            var message = this._consumingMessageQueue.Take();
            var messageId = this._keySelector(message);

            if (!this._handlingMessageDict.TryAdd(messageId, message))
            {
                this._logger.Warn("ignore to handle message [MessageId:{0}]", messageId);
                return;
            }

            try
            {
                this._messageHandler.Handle(new MessageContext<TMessage>(message, messageId));
                this.RemoveHandledMessage(message);
            }
            catch (Exception ex)
            {
                LogMessageHandlingException(message, ex);
            }
        }

        private void LogMessageHandlingException(TMessage message, Exception exception)
        {
            this._logger.Error(string.Format(
                "Message handling has exception, message info:[MessageId={0}]", this._keySelector(message)
                ), exception);
        }

        private void RemoveHandledMessage(TMessage consumingMessage)
        {
            TMessage consumedMessage;
            if (!_handlingMessageDict.TryRemove(this._keySelector(consumingMessage), out consumedMessage))
            {
                //
            }
        }
    }

}
