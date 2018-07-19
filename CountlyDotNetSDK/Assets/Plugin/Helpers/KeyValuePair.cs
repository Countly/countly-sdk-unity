using System;

namespace Assets.Plugin.Helpers
{
    [Serializable]
    public struct KeyValuePair<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

        public KeyValuePair(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}
