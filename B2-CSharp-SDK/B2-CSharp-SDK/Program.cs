using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2_CSharp_SDK
{
    class Program
    {
        static void Main(string[] args)
        {
            string accountId = Console.ReadLine();
            string applicationKey = Console.ReadLine();
            B2SDK sdk = new B2SDK(accountId, applicationKey);
        }
    }
}
