using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
        string uploadAuthToken = "";
        bool authorized = false;
        Dictionary<string,string> uploadUrls = new Dictionary<string, string>();

        private bool checkStringParamsNotEmpty(string[] parameters)
        {
            foreach(string param in parameters) {
                if (param == "" || param == null)
                {
                    return false;
                }
            }
            return true;
        }

        public B2SDK(string accountId, string applicationKey)
        {
            if (!checkStringParamsNotEmpty(new string[] { accountId, applicationKey })) {
                return;
            }
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
        /// <returns> B2Bucket object with created bucket. Returns null for failure.</returns>
        public B2Bucket b2_create_bucket (string bucketName, string bucketType)
        {
            if (!checkStringParamsNotEmpty(new string[] { bucketName, bucketType }) || !authorized)
            {
                return null;
            }
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
                B2Bucket returnData = new B2Bucket((String)jsonData.accountId, (String)jsonData.bucketId, (String)jsonData.bucketName, (String)jsonData.bucketType);
                return returnData;
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketId">ID of bucket to be deleted</param>
        /// <returns>Object for bucket that was deleted</returns>
        public B2Bucket b2_delete_bucket (string bucketId)
        {
            if (!checkStringParamsNotEmpty(new string[] { bucketId }) || !authorized)
            {
                return null;
            }
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
            try
            {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                dynamic jsonData = JsonConvert.DeserializeObject(responseString);
                B2Bucket returnData = new B2Bucket((String)jsonData.accountId, (String)jsonData.bucketId, (String)jsonData.bucketName, (String)jsonData.bucketType);
                return returnData;
            }
            // we catch ex in case we have a better way of logging errors to the user in the future
            catch (Exception ex)
            {
                return null;
            };
        }
        
        /// <summary>
        /// Lists all buckets associated with current instance of the SDK
        /// </summary>
        /// <returns> JSON string of buckets as returned from backblaze API</returns>
        public List<B2Bucket> b2_list_buckets()
        {
            if (!authorized)
            {
                return null;
            }
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
            B2BucketList bucketData = (B2BucketList)Newtonsoft.Json.JsonConvert.DeserializeObject(responseString, typeof(B2BucketList));
            return bucketData.buckets;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketId"> ID for bucket to get files from</param>
        /// <param name="startFileName"> StartFileName to start with for next request</param>
        /// <returns>B2FileList -- list of files in bucket</returns>
        public B2FileList b2_list_file_names(string bucketId, string startFileName)
        {
            if (!checkStringParamsNotEmpty(new string[] { bucketId }) || !authorized)
            {
                return null;
            }
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
            try {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                B2FileList fileList = (B2FileList)Newtonsoft.Json.JsonConvert.DeserializeObject(responseString, typeof(B2FileList));
                return fileList;
            }
            catch (Exception ex)
            {
                return null;
            };

        }

        /// <summary>
        /// Gets versions of every file in a bucket starting with the start file name
        /// </summary>
        /// <param name="bucketId"> id of bucket to search</param>
        /// <param name="startFileName"> starting file name for files</param>
        /// <returns> B2FileList -- list of files in bucket </returns>
        public B2FileList b2_list_file_versions(string bucketId, string startFileName)
        {
            if (!checkStringParamsNotEmpty(new string[] { bucketId }) || !authorized)
            {
                return null;
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

            try
            {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                B2FileList fileList = (B2FileList)Newtonsoft.Json.JsonConvert.DeserializeObject(responseString, typeof(B2FileList));
                return fileList;
            }
            catch (Exception ex)
            {
                return null;
            };
        }

        /// <summary>
        /// Deletes a specific version of a file given a filename and id
        /// </summary>
        /// <param name="fileName"> Name of file</param>
        /// <param name="fileId"> Id of file to delete</param>
        /// <returns> True if successful, false if unsuccessful</returns>
        public bool b2_delete_file_version(string fileName, string fileId)
        {
            if (!checkStringParamsNotEmpty(new string[] { fileName, fileId }) || !authorized)
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
        /// <returns>Byte array of download data. Returns null if invalid fileId</returns>
        public byte[] b2_download_file_by_id(string fileId)
        {
            if (!checkStringParamsNotEmpty(new string[] { fileId }) || !authorized)
            {
                return null;
            }
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
        /// <returns> Byte array of downloaded file. Returns null if fileName/bucketname combo are invalid.</returns>
        public byte[] b2_download_file_by_name(string fileName, string bucketName)
        {
            if (!checkStringParamsNotEmpty(new string[] { fileName, bucketName }) || !authorized)
            {
                return null;
            }
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
        /// NOTE: this api isn't currently working (I think due to an error in Backblaze -- get_file_info in their web console returns a bad request as this does
        /// Currently holding off on putting this into an object until the errors are fixes and I can actually test
        public string b2_get_file_info(string fileId)
        {
            if (!checkStringParamsNotEmpty(new string[] { fileId }) || !authorized)
            {
                return "";
            }
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
            if (!checkStringParamsNotEmpty(new string[] { bucketId }) || !authorized)
            {
                return "";
            }
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
            uploadAuthToken = jsonData.authorizationToken;
            return uploadUrls[bucketId];
        }

        /// <summary>
        /// Hides a file by its name. It will still exist and can be downloaded by version.
        /// </summary>
        /// <param name="bucketId"> Id of the bucket</param>
        /// <param name="fileName"> Id of the file</param>
        /// <returns> Bool True if successful, false if any error</returns>
        public bool b2_hide_file(string bucketId, string fileName)
        {
            if (!checkStringParamsNotEmpty(new string[] { fileName, bucketId }) || !authorized)
            {
                return false;
            }
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_hide_file");
            string body =
            "{\"bucketId\":\"" + bucketId + "\",\n" +
            "\"fileName\":\"" + fileName + "\"}";
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
        /// Updates bucket information
        /// </summary>
        /// <param name="bucketId"> Id of bucket to update</param>
        /// <param name="bucketType"> "allPrivate" or "allPublic" for private or public bucket</param>
        /// <returns> B2Bucket -- the updated bucket after applying the changes</returns>
        public B2Bucket b2_update_bucket(string bucketId, string bucketType)
        {
            if (!checkStringParamsNotEmpty(new string[] { bucketType, bucketId }) || !authorized)
            {
                return null;
            }
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(apiUrl + "/b2api/v1/b2_update_bucket");
            string body =
            "{\"accountId\":\"" + accountID + "\",\n" +
            "\"bucketId\":\"" + bucketId + "\",\n" +
            "\"bucketType\":\"" + bucketType + "\"}";
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
            try
            {
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                dynamic jsonData = JsonConvert.DeserializeObject(responseString);
                B2Bucket returnData = new B2Bucket((String)jsonData.accountId, (String)jsonData.bucketId, (String)jsonData.bucketName, (String)jsonData.bucketType);
                return returnData;
            }
            // we catch ex in case we have a better way of logging errors to the user in the future
            catch (Exception ex)
            {
                return null;
            };
        }

        public string b2_upload_file(byte[] bytes, string fileName, string bucketId)
        {
            if (!checkStringParamsNotEmpty(new string[] { fileName, bucketId }) || !authorized)
            {
                return "";
            }
            SHA1CryptoServiceProvider sh = new SHA1CryptoServiceProvider();
            sh.ComputeHash(bytes);
            byte[] hash = sh.Hash;
            StringBuilder sb = new StringBuilder();
            foreach(byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            string sha1 = sb.ToString();

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(b2_get_upload_url(bucketId));
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", uploadAuthToken);
            webRequest.Headers.Add("X-Bz-File-Name", fileName);
            webRequest.ContentType = "b2/x-auto";
            webRequest.ContentLength = bytes.Length;
            webRequest.Headers.Add("X-Bz-Content-Sha1", sha1);
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
                return responseString;
            }
            else
            {
                response.Close();
                return "";
            }
        }
    }
}
