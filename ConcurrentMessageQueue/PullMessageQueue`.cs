using System;
using System.Collections.Generic;
using System.Linq;
using ConcurrentMessageQueue.Logging;
using ConcurrentMessageQueue.Scheduling;

namespace ConcurrentMessageQueue
{
    internal class PullMessageQueue<TMessage> : MessageQueue<TMessage>
    {
        private readonly MessageQueueSetting _settings;
        private readonly ILogger _logger;
        private readonly IEqualityComparer<TMessage> _messageEqualityComparer;
        private readonly Interval _pullMessageService;

        private Func<TMessage, MessageId> _keySelector;
        private IMessageSource<TMessage> _messageSource;
        private TimeSpan _pullMessageInterval;
        
        public PullMessageQueue(MessageQueueSetting settings, ILogger logger, IEqualityComparer<TMessage> messageEqualityComparer)
            :base(settings, logger)
        {
            this._settings = settings;
            this._logger = logger;
            this._messageEqualityComparer = messageEqualityComparer;

            this._keySelector = DefaultKeySelector;
            this._pullMessageInterval = TimeSpan.Zero;
            this._pullMessageService = new Interval(logger);
        }

        public PullMessageQueue<TMessage> SetMessageSource(IMessageSource<TMessage> messageSource, TimeSpan interval)
        {
            this._messageSource = messageSource;
            this._pullMessageInterval = interval;
            return this;
        }

        public PullMessageQueue<TMessage> SetKeySelector(Func<TMessage, MessageId> keySelector)
        {
            this._keySelector = keySelector;
            return this;
        }

        private MessageId DefaultKeySelector(TMessage message)
        {
            return new MessageId(Guid.NewGuid().ToString("N"));
        }

        public override MessageQueue<TMessage> Start()
        {
            this._pullMessageService.StartTask("Queue.PullMessage", PullMessage, (int)this._pullMessageInterval.TotalSeconds);

            return base.Start();
        }

        public override void Stop()
        {
            this._pullMessageService.StopTask("Queue.PullMessage");
            base.Stop();
        }
        
        private void PullMessage()
        {
            var min = this._settings.MinConsumeringMessageCount;
            var max = this._settings.MaxConsumeringMessageCount;
            var len = this.GetQueueCount();
            if (len < min)
            {
                try
                {
                    var processingMessages = GetProcessingMessageSnapshot();
                    var prepareEnqueueMessages = (this._messageSource.GetList(max - len) ?? new TMessage[0]).ToArray();
                    var equeuedCount = 0;

                    if (prepareEnqueueMessages.Length == 0) return;
                    
                    foreach (var message in prepareEnqueueMessages)
                    {
                        var messageId = this._keySelector(message);
                        
                        if (this._messageEqualityComparer != null && processingMessages.Contains(message, this._messageEqualityComparer))
                        {
                            this._logger.Warn("ignore to equeue [MessageId:{0}]", messageId);
                            continue;
                        }

                        this.Add(message, messageId);
                        processingMessages.Add(message);
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
                    this._logger.Error("pull message error", ex);
                }
            }
        }
        
    }
}
