﻿using System;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktaCLIDemo
{
    public class OAuthClient
    {
         public string Call(string method, string headers, string jsonPayload)
        {
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(method);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; //Added to support all TLS types required by remote server

            request.Method = "POST";
            request.ContentLength = 0;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Authorization", "Basic " + headers);
            request.Timeout = 120000;

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

        public RestClient Centrify_OAuthClientCredentials(string hosturl, string clientid, string clientsecret, string scope, string username, string password)
        {
            //Centrify Uri for oAuth 
            string api = hosturl + "/oauth2/v1/token";
            //Web request body
            string body = "grant_type=password&username=" + username + "&password=" + password + "&scope=" + scope;
            //Authorization header "Basic base64encoded id:secret"
            string basic = InternalMakeClientAuth(clientid, clientsecret);

            //Response from oAuth web request
            string response = Call(api, basic, body);
            //Parse JSON response into Dictionary
            JavaScriptSerializer m_jsSerializer = new JavaScriptSerializer();
            Dictionary<string, dynamic> responseDict = m_jsSerializer.Deserialize<Dictionary<string, dynamic>>(response);

            RestClient client = new RestClient();
            client.BearerToken = responseDict["access_token"].ToString();
            client.Endpoint = hosturl;
            //Return the Access Token
            return client;
        }

        public string InternalMakeClientAuth(string id, string secret)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(id + ":" + secret);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
