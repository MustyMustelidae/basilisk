using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
         //   NullTest(null);
        }

        static void NullTest(string nullString)
        {
            Contract.Requires(nullString != null);
        }
    }
}
