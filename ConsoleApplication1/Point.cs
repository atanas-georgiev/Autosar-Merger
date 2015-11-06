using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Point<T, K>
    {
        private T X;

        private K Y;

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() % 1000;
        }
    }
}
