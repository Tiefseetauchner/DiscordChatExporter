using System.Collections.Generic;

namespace DiscordChatExporter.Stats
{
    class LongCounter<T> : Dictionary<T, long>
    {
        public void Add(T key)
        {
            if (!ContainsKey(key))
            {
                base.Add(key, 1);
            }
            else
            {
                this[key] += 1;
            }
        }


        public new void Add(T key, long value)
        {
            if (!ContainsKey(key))
            {
                base.Add(key, value);
            }
            else
            {
                this[key] += value;
            }
        }
    }
}