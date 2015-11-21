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
            string bucketID = sdk.b2_create_bucket("BalajisAwesomeBucket", "allPrivate");
            if (bucketID != "")
            {
                Console.WriteLine("Bucket Created");
                if (sdk.b2_delete_bucket(bucketID))
                {
                    Console.WriteLine("Delete successful");
                }
            }
        }
    }
}
