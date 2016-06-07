using System;

namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 消息队列相关参数设置
    /// </summary>
    public sealed class MessageQueueSetting
    {
        private int _workThreadCount;
        private int _maxConsumeringMessageCount;
        private int _minConsumeringMessageCount;

        /// <summary>
        /// 构建一个<see cref="MessageQueueSetting"/>对象
        /// </summary>
        /// <param name="workThreadCount">工作线程数量</param>
        /// <param name="maxConsumeringMessageCount">可处理消息最大阈值</param>
        /// <param name="minConsumeringMessageCount">可处理消息最小阈值</param>
        public static MessageQueueSetting Create(int workThreadCount, int maxConsumeringMessageCount, int minConsumeringMessageCount)
        {
            return new MessageQueueSetting
            {
                WorkThreadCount = workThreadCount,
                MaxConsumeringMessageCount = maxConsumeringMessageCount,
                MinConsumeringMessageCount = minConsumeringMessageCount
            };
        }

        /// <summary>
        /// 工作线程数量
        /// 队列在开始工作时候会初始化这个数值的线程数，用于处理队列中的消息。当队列中没有任何数据时，这些线程将会被挂起
        /// </summary>
        public int WorkThreadCount
        {
            get
            {
                return this._workThreadCount;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "工作线程数量只能是大于0的整数");
                }
                this._workThreadCount = value;
            }
        }

        /// <summary>
        /// 可处理消息最大阈值
        /// 当队列中待处理的消息数量等于这个数值时，队列会暂停拉取消息
        /// </summary>
        public int MaxConsumeringMessageCount
        {
            get { return this._maxConsumeringMessageCount; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "可处理消息最大阈值只能是大于0的整数");
                }
                this._maxConsumeringMessageCount = value;
            }
        }

        /// <summary>
        /// 可处理消息最小阈值
        /// 当队列中待处理的消息数量小于这个数值时，队列会不间断的拉取消息
        /// </summary>
        public int MinConsumeringMessageCount
        {
            get { return this._minConsumeringMessageCount; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "可处理消息最小阈值只能是大于0的整数");
                }
                this._minConsumeringMessageCount = value;
            }
        }
    }
}
