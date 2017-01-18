using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Network01_Shared
{    
    public class ConcurrentQueue<T> : ConcurrentContainer
    {
        private Queue<T> m_hQueue;


        public ConcurrentQueue() : base()
        {
            m_hQueue = new Queue<T>();
        }

        public bool TryAdd(T item)
        {
            this.SpinWait(() => m_hQueue.Enqueue(item));

            return true;
        }


        public bool TryTake(out T item)
        {
            T result    = default(T);
            bool bRes   = false;

            this.SpinWait(() => 
            {
                if (m_hQueue.Count > 0)
                {
                    result = m_hQueue.Dequeue();
                    bRes = true;
                }
            });

            item = result;
            return bRes;
        }

        public int Count
        {
            get
            {
                int iResult = 0;

                this.SpinWait(() => iResult = m_hQueue.Count);

                return iResult;
            }
        }

    }
}
