using System.Collections.Generic;

namespace WorstBlockchainEver.Helper
{
    public class BalanceDictionary<K, V>
    {
        private readonly Dictionary<K, V> balances;
        private readonly V defaultValue;

        public BalanceDictionary(V defaultValue)
        {
            balances = new Dictionary<K, V>();
            this.defaultValue = defaultValue;
        }

        public void Add(K key, V value)
        {
            if (balances.ContainsKey(key))
            {
                balances[key] = value;
            }
            else
            {
                balances.Add(key, value);
            }
        }

        public void Remove(K key)
        {
            if (balances.ContainsKey(key))
            {
                balances.Remove(key);
            }
        }

        public void Modify(K key, V value)
        {
            if (balances.ContainsKey(key))
            {
                balances[key] = value;
            }
            else
            {
                Add(key, value);
            }
        }

        public V Get(K key)
        {
            if (!balances.ContainsKey(key))
            {
                Add(key, defaultValue);
            }

            return balances[key];
        }
    }
}