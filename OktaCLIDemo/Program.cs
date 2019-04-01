using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OktaCLIDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 5)
            {
                string TenantUrl = args[0]; //"https://dev-993565.okta.com"
                string ClientId = args[1]; //"0oaeakh7vWAg4d9ap356"
                string ClientSecret = args[2]; //"PaznJnTxVBxMv4Q5Y5nXyHyyhcyLGCmu4dHYhX-c"
                string Scope = args[3]; //"openid"
                string Username = args[4]; //"nickgamb@gmail.com"
                string Password = args[5]; //"Fread111"

                Console.WriteLine("Okta CLI Demo \n");
                Console.WriteLine("Getting accesstoken for {0}...\n", Username);
                RestClient authenticationClient = new OAuthClient().Centrify_OAuthClientCredentials(TenantUrl, ClientId, ClientSecret, Scope, Username, Password);
                Console.WriteLine("Access token is: {0} \n", authenticationClient.BearerToken.ToString());
                Console.WriteLine("Getting user info...\n");

                string api = "/oauth2/v1/userinfo";
                string response = authenticationClient.Call(api, "");
                //Parse JSON response into Dictionary
                JavaScriptSerializer m_jsSerializer = new JavaScriptSerializer();
                Dictionary<string, dynamic> responseDict = m_jsSerializer.Deserialize<Dictionary<string, dynamic>>(response);
                foreach (KeyValuePair<string, dynamic> kvp in responseDict)
                {
                    Console.WriteLine("{0} = {1}", kvp.Key, kvp.Value);
                }
                Console.ReadLine();
            }

            else
            {
                Console.WriteLine("No command line arguments found.");
                Console.WriteLine("Usage:");
                Console.WriteLine("OktaCLIDemo.exe [TenantUrl ClientId ClientSecret Scope Username Password]");
            }

  
        }
    }
}
