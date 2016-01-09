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
            // This is all just testing code, it doesn't really matter what happens here
            B2SDK sdk = new B2SDK(accountId, applicationKey);
            string bucketID = sdk.b2_create_bucket("BalajisAwesomeBucket", "allPrivate");

            Console.WriteLine(sdk.b2_list_buckets()); 
          }
    }
}
