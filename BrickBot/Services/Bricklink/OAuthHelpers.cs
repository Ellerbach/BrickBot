using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BrickBot.Services.Bricklink
{
    public class OAuthHelpers
    {
        private const string Digit = "1234567890";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string High = "ABCDEFGHIJKLMNOPQRSTUVW";

        private static readonly Random _random = new Random();
        private static readonly object _randomLock = new object();

        /// <summary>
        /// Generates a random 16-byte lowercase alphanumeric string. 
        /// </summary>
        /// <seealso href="http://oauth.net/core/1.0#nonce"/>
        /// <returns></returns>
        public static string GetNonce()
        {
            const string chars = (Lower + Digit + High);

            var nonce = new char[16];
            lock (_randomLock)
            {
                for (var i = 0; i < nonce.Length; i++)
                {
                    nonce[i] = chars[_random.Next(0, chars.Length)];
                }
            }
            return new string(nonce);
        }

        public static string NormalizeRequestParameters(WebParameterCollection parameters)
        {
            var copy = SortParametersExcludingSignature(parameters);
            var concatenated = Concatenate(copy, "=", "&");
            return concatenated;
        }

        public static string Concatenate(ICollection<WebParameter> collection, string separator, string spacer)
        {
            var sb = new StringBuilder();

            var total = collection.Count;
            var count = 0;

            foreach (var item in collection)
            {
                sb.Append(item.Name);
                sb.Append(separator);
                sb.Append(item.Value);

                count++;
                if (count < total)
                {
                    sb.Append(spacer);
                }
            }

            return sb.ToString();
        }

        public static WebParameterCollection SortParametersExcludingSignature(WebParameterCollection parameters)
        {
            var copy = new WebParameterCollection(parameters);
            var exclusions = copy.Where(n => EqualsIgnoreCase(n.Name, "oauth_signature"));

            copy.RemoveAll(exclusions);

            foreach (var parameter in copy)
            {
                parameter.Value = Uri.EscapeUriString(parameter.Value);
            }

            copy.Sort((x, y) => x.Name.Equals(y.Name) ? x.Value.CompareTo(y.Value) : x.Name.CompareTo(y.Name));
            return copy;
        }

        private static bool EqualsIgnoreCase(string left, string right)
        {
            return String.Compare(left, right, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string GetAuthorizationToken(string url, WebParameterCollection param = null)
        {
            string TimeInSecondsSince1970 = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            string SHA1HASH = "";
            string Nonce = "";
            do
            {
                Nonce = GetNonce();

                string consumer_secret = Uri.EscapeDataString(ConfigurationManager.AppSettings["consumer_secret"]);
                string token_secret = Uri.EscapeDataString(ConfigurationManager.AppSettings["token_secret"]);

                string signature_base_string = GetSignatureBaseString(url, TimeInSecondsSince1970, Nonce, param);
                SHA1HASH = GetSha1Hash(consumer_secret + "&" + token_secret, signature_base_string);
            } while (SHA1HASH.Contains("+") || SHA1HASH.Contains("/"));

            string Header =
               $"OAuth oauth_consumer_key=\"{ConfigurationManager.AppSettings["oauth_consumer_key"]}\",oauth_nonce=\"{Nonce}\",oauth_timestamp=\"{TimeInSecondsSince1970}\",oauth_token=\"{ConfigurationManager.AppSettings["oauth_token"]}\",oauth_signature=\"{SHA1HASH}\",oauth_signature_method=\"HMAC-SHA1\",oauth_version=\"1.0\"";

            Debug.WriteLine($"{ Header}");
            return Header;
        }

        public static string GetSha1Hash(string key, string message)
        {
            var encoding = new System.Text.ASCIIEncoding();

            byte[] keyBytes = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(message);

            string Sha1Result = string.Empty;

            using (HMACSHA1 SHA1 = new HMACSHA1(keyBytes))
            {
                var Hashed = SHA1.ComputeHash(messageBytes);
                Sha1Result = Convert.ToBase64String(Hashed);
            }

            return Sha1Result;
        }

        public static string GetSignatureBaseString(string url, string TimeStamp, string Nonce, WebParameterCollection param)
        {
            //1.Convert the HTTP Method to uppercase and set the output string equal to this value.
            string Signature_Base_String = "GET";
            Signature_Base_String = Signature_Base_String.ToUpper();

            //2.Append the ‘&’ character to the output string.
            Signature_Base_String = Signature_Base_String + "&";

            //3.Percent encode the URL and append it to the output string.
            string PercentEncodedURL = Uri.EscapeDataString(url);
            Signature_Base_String = Signature_Base_String + PercentEncodedURL;

            //4.Append the ‘&’ character to the output string.
            Signature_Base_String = Signature_Base_String + "&";

            //5.append parameter string to the output string.
            // a bit crap but should work for 1 param. Let see :)

            var authParameters = new WebParameterCollection
                                     {
                                         new WebParameter("oauth_consumer_key", ConfigurationManager.AppSettings["oauth_consumer_key"]),
                                         new WebParameter("oauth_nonce", Nonce),
                                         new WebParameter("oauth_signature_method", "HMAC-SHA1"),
                                         new WebParameter("oauth_timestamp", TimeStamp),
                                         new WebParameter("oauth_version", "1.0"),
                                         new WebParameter("oauth_token", ConfigurationManager.AppSettings["oauth_token"])
                                     };
            if (param != null)
                foreach (var authParameter in param)
                {
                    authParameters.Add(authParameter);
                }

            Signature_Base_String += Uri.EscapeDataString(NormalizeRequestParameters(authParameters));

            //if (param.Length > 0)
            //    if (string.Compare(param.Substring(0, 1), "o") < 0)
            //    { Signature_Base_String = Signature_Base_String + Uri.EscapeDataString(param + "&"); }
            //Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("oauth_consumer_key=" + ConfigurationManager.AppSettings["oauth_consumer_key"]);
            //Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_nonce=" + Nonce);
            //Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_signature_method=" + "HMAC-SHA1");
            //Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_timestamp=" + TimeStamp);
            //Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_token=" + ConfigurationManager.AppSettings["oauth_token"]);
            //Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&oauth_version=" + "1.0");
            //if (param.Length > 0)
            //    if (string.Compare(param.Substring(0, 1), "o") >= 0)
            //    { Signature_Base_String = Signature_Base_String + Uri.EscapeDataString("&" + param); }
            return Signature_Base_String;
        }
    }

}