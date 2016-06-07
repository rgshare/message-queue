using System.Collections.Generic;

namespace ConcurrentMessageQueue
{
    /// <summary>
    /// 决定如何获取消息
    /// </summary>
    /// <typeparam name="TMessage">消息类型</typeparam>
    public interface IMessageSource<TMessage>
    {
        /// <summary>
        /// 批量获取消息
        /// </summary>
        /// <param name="count">消息数量</param>
        /// <returns>消息集合</returns>
        IEnumerable<TMessage> GetList(int count);
    }
}
