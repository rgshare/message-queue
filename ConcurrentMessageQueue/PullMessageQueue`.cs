using System;
using System.Collections.Generic;
using System.Linq;
using JobsQueue.Scheduling;

namespace JobsQueue
{
    internal class PullMessageQueue<TMessage> : MessageQueue<TMessage>
    {
        private readonly AsyncQueueSetting _settings;
        private readonly ILogger _logger;
        private readonly IEqualityComparer<TMessage> _messageEqualityComparer;
        private readonly Interval _pullMessageService;

        private Func<TMessage, MessageId> _keySelector;
        private IMessageSource<TMessage> _messageSource;
        private TimeSpan _pullMessageInterval;
        
        public PullMessageQueue(AsyncQueueSetting settings, ILogger logger, IEqualityComparer<TMessage> messageEqualityComparer)
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
            var interval = (int) this._pullMessageInterval.TotalMilliseconds;
            this._pullMessageService.StartTask("Queue.PullMessage", PullMessage, 0, interval);

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
                    var count = max - len;
                    var equeuedCount = 0;
                    var prepareEnqueueMessages = count == 0 //count=0时无需从数据源拉取消息了
                                                ? new TMessage[0]
                                                : (this._messageSource.GetList(count) ?? new TMessage[0]).ToArray();

                    if (prepareEnqueueMessages.Length == 0) return;
                    
                    foreach (var message in prepareEnqueueMessages)
                    {
                        var messageId = this._keySelector(message);
                        
                        if (this._messageEqualityComparer != null && processingMessages.Contains(message, this._messageEqualityComparer))
                        {
                            this._logger.Log(LogLevel.Warning,  "ignore to equeue [MessageId:{0}]", messageId);
                            continue;
                        }

                        this.Add(message, messageId);
                        processingMessages.Add(message);
                        equeuedCount++;
                    }
                    
                    this._logger.Log(LogLevel.Info, "pull {0} messages from data source, enqueue {1}, ignore {2}",
                        prepareEnqueueMessages.Length,
                        equeuedCount,
                        prepareEnqueueMessages.Length - equeuedCount
                        );
                }
                catch (Exception ex)
                {
                    this._logger.Log(LogLevel.Error, "pull message error " + Environment.NewLine + ex);
                }
            }
        }
        
    }
}
