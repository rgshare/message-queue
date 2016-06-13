using System;
using System.Collections.Generic;

namespace ConcurrentMessageQueue
{
    internal class AnonymousMessageSource<TMessage> : IMessageSource<TMessage>
    {
        private readonly Func<int, IEnumerable<TMessage>> _source;

        public AnonymousMessageSource(Func<int, IEnumerable<TMessage>> source)
        {
            _source = source;
        }

        public IEnumerable<TMessage> GetList(int count)
        {
            return this._source(count);
        }
    }
}
