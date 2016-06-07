namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 消息队列设置
    /// </summary>
    public sealed class MessageQueueSetting
    {
        /// <summary>
        /// 工作线程数量
        /// 队列在开始工作时候会初始化这个数值的线程数，用于处理队列中的消息。当队列中没有任何数据时，这些线程将会被挂起
        /// </summary>
        public int WorkThreadCount { get; set; }
        /// <summary>
        /// 可处理消息最大阈值
        /// 当队列中待处理的消息数量等于这个数值时，队列会暂停拉取消息
        /// </summary>
        public int MaxConsumeringMessageCount { get; set; }
        /// <summary>
        /// 可处理消息最小阈值
        /// 当队列中待处理的消息数量小于这个数值时，队列会不间断的拉取消息
        /// </summary>
        public int MinConsumeringMessageCount { get; set; }
    }
}
