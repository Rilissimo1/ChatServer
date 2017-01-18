using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network01_Shared
{
    public class ConcurrentDictionary<TKey, TValue> : ConcurrentContainer
    {
        private Dictionary<TKey, TValue> m_hDictionary;

        public ConcurrentDictionary()
        {
            m_hDictionary = new Dictionary<TKey, TValue>();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            try
            {
                this.SpinWait(() => m_hDictionary.Add(key, value));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value       = default(TValue);
            TValue res  = default(TValue);
            bool bres   = false;

            this.SpinWait(() =>
            {
                bres = m_hDictionary.TryGetValue(key, out res);
            });

            return bres;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            value       = default(TValue);
            TValue res  = default(TValue);
            bool bres   = false;

            this.SpinWait(() =>
            {
                if (m_hDictionary.ContainsKey(key))
                {
                    res  = m_hDictionary[key];
                    bres = m_hDictionary.Remove(key);
                }                
            });

            value = res;
            return bres;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue res = default(TValue);

            this.SpinWait(() =>
            {
                if (m_hDictionary.ContainsKey(key))
                {
                    res = updateValueFactory.Invoke(key, addValue);
                    m_hDictionary[key] = res;
                }
                else
                {
                    res = addValue;
                    m_hDictionary.Add(key, addValue);
                }
            });

            return res;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue res;
                this.TryGetValue(key, out res);
                return res;
            }

            set
            {
                this.AddOrUpdate(key, value, (k, v) => v);
            }
        }
    }
}
