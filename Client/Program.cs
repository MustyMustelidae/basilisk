using System.Diagnostics.Contracts;

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
