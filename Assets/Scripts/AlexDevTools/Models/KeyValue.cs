using System;

namespace AlexDevTools.Models
{
    [Serializable]
    public class KeyValue<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }
}