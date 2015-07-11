using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script;
using System.Web.Script.Serialization;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace SampleTwitterCleintAPI.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult TwitterSearch(string twittersearch)
        {
            //string url = "https://api.twitter.com/1.1/search/tweets.json?q=mclarren-honda";
            string url = "https://api.twitter.com/1.1/search/tweets.json?q=" + twittersearch;
            string oauthconsumerkey = "v6DfLPTIeZRmImMRQv8psqR7U";
            string oauthtoken = "122193687-YBbI3iVREllpa4r0gERzjcC19hQJC2DExaGpDeZF";
            string oauthconsumersecret = "kjvx3igIt9ntq60y9H1TaW9mBOGoqv7ch6vZgebsAt8n8bGjcJ";
            string oauthtokensecret = "08KydViQ6UEbRidcyCVkx8EtGpIaEXQAIGkCL1QPdwwf9";
            string oauthsignaturemethod = "HMAC-SHA1";
            string oauthversion = "1.0";
            string oauthnonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            string oauthtimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            SortedDictionary<string, string> basestringParameters = new SortedDictionary<string, string>();
            basestringParameters.Add("q", twittersearch);
            basestringParameters.Add("oauth_version", oauthversion);
            basestringParameters.Add("oauth_consumer_key", oauthconsumerkey);
            basestringParameters.Add("oauth_nonce", oauthnonce);
            basestringParameters.Add("oauth_signature_method", oauthsignaturemethod);
            basestringParameters.Add("oauth_timestamp", oauthtimestamp);
            basestringParameters.Add("oauth_token", oauthtoken);
            //Build the signature string
            StringBuilder baseString = new StringBuilder();
            baseString.Append("GET" + "&");
            baseString.Append(EncodeCharacters(Uri.EscapeDataString(url.Split('?')[0]) + "&"));
            foreach (KeyValuePair<string, string> entry in basestringParameters)
            {
                baseString.Append(EncodeCharacters(Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&")));
            }

            //Remove the trailing ambersand char last 3 chars - %26
            string finalBaseString = baseString.ToString().Substring(0, baseString.Length - 3);

            //Build the signing key
            string signingKey = EncodeCharacters(Uri.EscapeDataString(oauthconsumersecret)) + "&" + EncodeCharacters(Uri.EscapeDataString(oauthtokensecret));

            //Sign the request
            HMACSHA1 hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(signingKey));
            string oauthsignature = Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(finalBaseString)));

            //Tell Twitter we don't do the 100 continue thing
            ServicePointManager.Expect100Continue = false;

            //authorization header
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(@url);
            StringBuilder authorizationHeaderParams = new StringBuilder();
            authorizationHeaderParams.Append("OAuth ");
            authorizationHeaderParams.Append("oauth_nonce=" + "\"" + Uri.EscapeDataString(oauthnonce) + "\",");
            authorizationHeaderParams.Append("oauth_signature_method=" + "\"" + Uri.EscapeDataString(oauthsignaturemethod) + "\",");
            authorizationHeaderParams.Append("oauth_timestamp=" + "\"" + Uri.EscapeDataString(oauthtimestamp) + "\",");
            authorizationHeaderParams.Append("oauth_consumer_key=" + "\"" + Uri.EscapeDataString(oauthconsumerkey) + "\",");
            if (!string.IsNullOrEmpty(oauthtoken))
                authorizationHeaderParams.Append("oauth_token=" + "\"" + Uri.EscapeDataString(oauthtoken) + "\",");
            authorizationHeaderParams.Append("oauth_signature=" + "\"" + Uri.EscapeDataString(oauthsignature) + "\",");
            authorizationHeaderParams.Append("oauth_version=" + "\"" + Uri.EscapeDataString(oauthversion) + "\"");
            webRequest.Headers.Add("Authorization", authorizationHeaderParams.ToString());

            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            //Allow us a reasonable timeout in case Twitter's busy
            webRequest.Timeout = 3 * 60 * 1000;
            string responseFromServer = "";
            //var jSer = new System.Web.Script.Serialization.JavaScriptSerializer();
            JavaScriptSerializer jz = new JavaScriptSerializer();
            try
            {
                //Proxy settings
                //webRequest.Proxy = new WebProxy("enter proxy details/address");
                HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
                Stream dataStream = webResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();
                
            }
            catch (Exception ex)
            {
            }

            var jobj1 = jz.DeserializeObject(responseFromServer);
            var jobj2 = jz.Deserialize<dynamic>(responseFromServer);
            var jobj3 = System.Web.Helpers.Json.Decode(responseFromServer);
            var jobj4 = JsonConvert.DeserializeObject<dynamic>(responseFromServer);
            return Json(jobj1);
            //return Json(responseFromServer);
        }

        private string EncodeCharacters(string data)
        {
            //as per OAuth Core 1.0 Characters in the unreserved character set MUST NOT be encoded
            //unreserved = ALPHA, DIGIT, '-', '.', '_', '~'
            if (data.Contains("!"))
                data = data.Replace("!", "%21");
            if (data.Contains("'"))
                data = data.Replace("'", "%27");
            if (data.Contains("("))
                data = data.Replace("(", "%28");
            if (data.Contains(")"))
                data = data.Replace(")", "%29");
            if (data.Contains("*"))
                data = data.Replace("*", "%2A");
            if (data.Contains(","))
                data = data.Replace(",", "%2C");

            return data;
        }     

    }
}
