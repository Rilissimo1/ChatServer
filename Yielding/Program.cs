using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yielding
{
    class UnknowEnumerator : IEnumerator<int>
    {
        private int i;
        private int a;
        private int b;

        public UnknowEnumerator(int a, int b)
        {
            this.a = a;
            this.b = b;
            i = a;
        }
        public bool MoveNext()
        {
            if (i < b)
            {
                i++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Current
        {
            get
            {
                return i;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return i;
            }
        }

        public void Dispose()
        {
            
        }

        public void Reset()
        {
            i = a;
        }
    }

    class UnknowObject : IEnumerable<int>
    {
        int a, b;

        public UnknowObject(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new UnknowEnumerator(a, b);            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new UnknowEnumerator(a, b);
        }
    }

    class Program
    {
        static IEnumerable<int> GetValues(int a, int b)
        {
            Console.WriteLine("GetValues");

            for (int i = a; i < b; i++)
            {
                yield return i;
            }

            Console.WriteLine("GetValues Terminata");
        }




        static void Main(string[] args)
        {

            IEnumerable<int> myEnum = GetValues(10, 20);

            foreach (var item in myEnum)
            {
                Console.WriteLine(item);
            }
        
        }
    }
}
