using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web.Script.Serialization;

namespace OktaCLIDemo
{
    public class RestClient
    {
        public CookieContainer Cookies { get; set; }
        public string Endpoint { get; set; }
        public string SessionId { get; set; }
        public string TenantId { get; set; }
        public dynamic ChallengeCollection { get; set; }
        public string SocialIdpRedirectUrl { get; set; }
        public bool AllowPasswordReset { get; set; }

        private const string DEFAULT_ENDPOINT = "https://dev-993565.okta.com";
        private JavaScriptSerializer m_jsSerializer = new JavaScriptSerializer();

        public RestClient()
        {
            Endpoint = DEFAULT_ENDPOINT;
        }

        public string BearerToken
        {
            get; set;
        }

        public RestClient(string podEndpointName)
        {
            Endpoint = podEndpointName;
        }

        public Dictionary<string, dynamic> CallApi(string method, Dictionary<string, dynamic> payload)
        {
            return m_jsSerializer.Deserialize<Dictionary<string, dynamic>>(Call(method, payload));
        }

        public string Call(string method, Dictionary<string, dynamic> payload)
        {
            return Call(method, m_jsSerializer.Serialize(payload));
        }

        public string Call(string method, string jsonPayload)
        {
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(Endpoint + method);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; //Added to support all TLS types required by remote server

            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/x-www-form-urlencoded";
            //request.Headers.Add("X-CENTRIFY-NATIVE-CLIENT", "1");
            request.Timeout = 120000;

            if (BearerToken != null)
            {
                request.Headers.Add("Authorization", "Bearer " + BearerToken);
            }

            // Carry cookies from call to call
            if (Cookies == null)
            {
                Cookies = new CookieContainer();
                Cookies.PerDomainCapacity = 300;
            }

            request.CookieContainer = Cookies;

            // Add content if provided
            if (!string.IsNullOrEmpty(jsonPayload))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                request.ContentLength = bytes.Length;

                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            using (var response = (System.Net.HttpWebResponse)request.GetResponse())
            {
                string responseValue = null;

                // If call did not return 200, throw exception
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException(string.Format("Request failed. Received HTTP {0}", response.StatusCode));
                }

                // If response is non-empty, read it all out as a string
                if (response.ContentLength > -2)
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var reader = new System.IO.StreamReader(responseStream))
                            {
                                responseValue = reader.ReadToEnd();
                            }
                        }
                    }
                }

                return responseValue;
            }
        }
    }
}
