using System;
using System.Collections.Generic;
using System.IO;
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
            B2Bucket newBucket = sdk.b2_create_bucket("ThisIsAnAwesomeBucket", "allPrivate");
            Console.WriteLine(newBucket.accountId + newBucket.bucketId + newBucket.bucketName + newBucket.bucketType);

            B2Bucket deletedBucket = sdk.b2_delete_bucket(newBucket.bucketId);
            if (deletedBucket.bucketId == newBucket.bucketId)
            {
                Console.WriteLine("Delete Successful");
            }

            // fun test code that will delete all files in our buckets!!
            List<B2Bucket> buckets = sdk.b2_list_buckets();

            foreach(B2Bucket bucket in buckets)
            {
                foreach(B2File file in sdk.b2_list_file_versions(bucket.bucketId, "").files)
                {
                    sdk.b2_delete_file_version(file.fileName, file.fileId);
                }
            }
        }
    }
}
