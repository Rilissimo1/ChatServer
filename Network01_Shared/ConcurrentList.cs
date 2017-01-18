using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Network01_Shared
{
    public class ConcurrentList<T> : ConcurrentContainer
    {
        private List<T> m_hList;

        public ConcurrentList() : base()
        {
            m_hList = new List<T>();
        }

        public void Add(T item)
        {
            this.SpinWait(() => m_hList.Add(item));
        }


        public bool Remove(T item)
        {
            bool bResult = false;

            this.SpinWait(() => 
            {
                bResult = m_hList.Remove(item);
            });

            return bResult;
        }



    }
}
