using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace B2_CSharp_SDK
{
    class B2SDK
    {
        static string BASE_BACKBLAZE_URL = "https://api.backblaze.com/b2api/v1/";

        string accountID;
        string apiUrl;
        string authorizationToken;
        string downloadURL;
        bool authorized = false;
        Dictionary<string,string> uploadUrls = new Dictionary<string, string>();

        public B2SDK(string accountId, string applicationKey)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BASE_BACKBLAZE_URL + "b2_authorize_account");
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(accountId + ":" + applicationKey));
            request.Headers.Add("Authorization", "Basic " + credentials);
            request.ContentType = "application/json; charset=utf-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();

                dynamic jsonData = JsonConvert.DeserializeObject(responseString);

                // set Data
                accountID = accountId;
                apiUrl = jsonData.apiUrl;
                authorizationToken = jsonData.authorizationToken;
                downloadURL = jsonData.downloadUrl;
                authorized = true;
            }
        }

        /// <summary>
        /// <param name="bucketName"> Name for the new bucket(unique)</param>
        /// <param name="bucketType">"allPrivate" - or - "allpublic"</param>
        /// </summary>
        /// <returns> string bucketID</returns>
        public string b2_create_bucket (string bucketName, string bucketType)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_create_bucket");
            string body =
                    "{\"accountId\":\"" + accountID + "\",\n" +
                    "\"bucketName\":\"" + bucketName + "\",\n" +
                    "\"bucketType\":\"" + bucketType + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                dynamic jsonData = JsonConvert.DeserializeObject(responseString);
                return jsonData.bucketId;
            } else
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketId">ID of bucket to be deleted</param>
        /// <returns>Bool success/failure</returns>
        public bool b2_delete_bucket (string bucketId)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_delete_bucket");
            string body =
                "{\"accountId\":\"" + accountID + "\",\n" +
                "\"bucketId\":\"" + bucketId + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// Lists all buckets associated with current instance of the SDK
        /// </summary>
        /// <returns> JSON string of buckets as returned from backblaze API</returns>
        public string b2_list_buckets()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_list_buckets");
            string body = "{\"accountId\":\"" + accountID + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            return responseString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketId"> ID for bucket to get files from</param>
        /// <param name="startFileName"> StartFileName to start with for next request</param>
        /// <returns>JSON list of files in bucket specified</returns>
        public string b2_list_file_names(string bucketId, string startFileName)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_list_file_names");
            string body = "{\"bucketId\":\"" + bucketId + "\","
                +         "\"startFileName\":\"" + startFileName + "\"}";
            var data = Encoding.UTF8.GetBytes(body);

            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using(var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets versions of every file in a bucket starting with the start file name
        /// </summary>
        /// <param name="bucketId"> id of bucket to search</param>
        /// <param name="startFileName"> starting file name for files</param>
        /// <returns> JSON string of all version of files </returns>
        public string b2_list_file_versions(string bucketId, string startFileName)
        {
            if (bucketId == null)
            {
                return "";
            }
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_list_file_versions");
            string body = "{\"bucketId\":\"" + bucketId + "\","
                + "\"startFileName\":\"" + startFileName + "\"}";
            var data = Encoding.UTF8.GetBytes(body);

            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Deletes a specific version of a file given a filename and id
        /// </summary>
        /// <param name="fileName"> Name of file</param>
        /// <param name="fileId"> Id of file to delete</param>
        /// <returns> True if successful, false if unsuccessful</returns>
        public bool b2_delete_file_version(string fileName, string fileId)
        {
            if (fileName == null || fileId == null)
            {
                return false;
            }
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_delete_file_version");
            string body =
               "{\"fileName\":\"" + fileName + "\",\n" +
               "\"fileId\":\"" + fileId + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.Close();
                return true;
            }
            else
            {
                response.Close();
                return false;
            }
        }

        /// <summary>
        /// Downloads a file from b2 given a fileId
        /// </summary>
        /// <param name="fileId"> ID of file to download</param>
        /// <returns>byte array of download data</returns>
        public byte[] b2_download_file_by_id(string fileId)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(downloadURL + "/b2api/v1/b2_download_file_by_id");
            string body = "{\"fileId\":\"" + fileId + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream responseStream = response.GetResponseStream();
            byte[] fileBytes;
            using (BinaryReader br = new BinaryReader(responseStream))
            {
                fileBytes = br.ReadBytes(500000);
                br.Close();
            }

            responseStream.Close();
            response.Close();

            return fileBytes;
        }

        /// <summary>
        /// Downloads a file from b2 given a fileName and Bucket to download from
        /// </summary>
        /// <param name="fileName"> Name of file to download</param>
        /// <param name="bucketName"> Name of bucket that file is stored in</param>
        /// <returns> Byte array of downloaded file</returns>
        public byte[] b2_download_file_by_name(string fileName, string bucketName)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(String.Format("{0}/file/{1}/{2}", downloadURL, bucketName, fileName));
            webRequest.Method = "GET";
            webRequest.Headers.Add("Authorization", authorizationToken);
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            Stream responseStream = response.GetResponseStream();
            byte[] fileBytes;
            using (BinaryReader br = new BinaryReader(responseStream))
            {
                fileBytes = br.ReadBytes(500000);
                br.Close();
            }
            responseStream.Close();
            response.Close();

            return fileBytes;
        }

        /// <summary>
        /// Returns file info in string form for a given fileId
        /// </summary>
        /// <param name="fileId"> string file id </param>
        /// <returns> string json data of file info</returns>
        /// NOTE: this api isn't currently working (I think due to an error in Backblaze -- getting info in their web console returns a bad request as this does
        public string b2_get_file_info(string fileId)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_get_file_info");
            string body = "{\"fileId\":\"" + fileId + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets Upload Url for pushing files to a specific bucket.
        /// </summary>
        /// <param name="bucketId"> Id for bucket to get upload url for</param>
        /// <returns>string upload url</returns>
        public string b2_get_upload_url(string bucketId)
        {
            if (uploadUrls.ContainsKey(bucketId))
            {
                return uploadUrls[bucketId];
            }

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_get_upload_url");
            string body = "{\"bucketId\":\"" + bucketId + "\"}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", authorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            WebResponse response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();
            dynamic jsonData = JsonConvert.DeserializeObject(responseString);
            uploadUrls[bucketId] = jsonData.uploadUrl;
            Console.WriteLine(uploadUrls[bucketId]);
            return uploadUrls[bucketId];
        }
    }
}
