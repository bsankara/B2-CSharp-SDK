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
            string bucketID = sdk.b2_create_bucket("BalajisAwesomeBucket", "allPrivate");

            // random test code to make sure these functions don't break everything
            sdk.b2_update_bucket(bucketID, "allPublic");
            sdk.b2_update_bucket(bucketID, "allPrivate");
            // this is actually a private bucket so it won't work for anyone other than someone with my auth token...
            byte[] fileBytes = sdk.b2_download_file_by_name("experience.html", "Somebuckethere");


            string downloadsFolder = @"C:\Users\balaji\downloads\experience.html";
            FileStream saveFile = new FileStream(downloadsFolder, FileMode.Create);
            BinaryWriter writeFile = new BinaryWriter(saveFile);
            try
            {
                writeFile.Write(fileBytes);
            }
            finally
            {
                saveFile.Close();
                writeFile.Close();
            }

            Console.WriteLine(sdk.b2_list_buckets()); 
        }
    }
}
