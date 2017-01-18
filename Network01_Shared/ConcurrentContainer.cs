using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Network01_Shared
{
    public abstract class ConcurrentContainer
    {
        private volatile int m_iTokenCounter;
        private volatile int m_iServedToken;

        public ConcurrentContainer()
        {
            m_iServedToken = m_iTokenCounter + 1;
        }


        protected void SpinWait(Action hUserCode)
        {
            int iToken = Interlocked.Increment(ref m_iTokenCounter);

            //Let's the thread spin for a while
            while (true)
            {
                if (Interlocked.CompareExchange(ref iToken, m_iServedToken + 1, m_iServedToken) != iToken)
                {
                    try
                    {
                        hUserCode.Invoke(); //in questo modo l'invocazione sarà thread-safe
                    }
                    finally
                    {
                        //In questo modo siamo sicuri che anche in caso di eccezioni, il contatore sarà aggiornato                        
                        m_iServedToken = iToken; //Cessione del token al thread successivo
                    }

                    break;
                }
            }
        }
    }
}
