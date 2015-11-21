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
                downloadURL = jsonData.downloadURL;
                authorized = true;
            }
        }
    }
}
