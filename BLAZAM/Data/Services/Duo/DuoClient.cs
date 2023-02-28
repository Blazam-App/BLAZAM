using BLAZAM.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Packaging.Signing;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BLAZAM.Server.Data.Services.Duo
{
    public class DuoClient
    {
        public RestResponse Response { get; private set; }


        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private string _clientId;
        private string _apiHost;
        private string _clientSecret;

        const string preAuthUri = "/auth/v2/preauth";
        const string authUri = "/auth/v2/auth";
        const string pingUri = "/auth/v2/ping";
        const string checkUri = "/auth/v2/check";
        string BaseUri =>  "https://"+_apiHost;



        public string RequestTimestamp { get; private set; }

        string CurrentEnpoint = "";
        HttpMethod Method = HttpMethod.Get;


        private RestRequest NewRequest(string uri)
        {
            var request = new RestRequest(uri);
            request.AddHeader("Date", RequestTimestamp)
                .AddHeader("Host", _apiHost);
            CurrentEnpoint = uri;

            return request;
        }

        private RestClient NewClient(string endpoint, string data = "")
        {
            RequestTimestamp = Timestamp;
            CurrentEnpoint = endpoint;
            return new RestClient(BaseUri)
            {
                Authenticator = new HttpBasicAuthenticator(_clientId, HmacSign(data))
            };
        }


        public DuoClient(string clientId, string apiHost, string clientSecret)
        {
            _clientId = clientId;
            _apiHost = apiHost;
            _clientSecret = clientSecret;
        }
        string Timestamp
        {
            get
            {
                var date = DateTime.UtcNow;
                // Can't use the "zzzz" format because it adds a ":"
                // between the offset's hours and minutes.
                string date_string = date.ToString(
                    "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                int offset = 0;
                // set offset if input date is not UTC time.
                if (date.Kind != DateTimeKind.Utc)
                {
                    offset = TimeZoneInfo.Local.GetUtcOffset(date).Hours;
                }
                string zone;
                // + or -, then 0-pad, then offset, then more 0-padding.
                if (offset < 0)
                {
                    offset *= -1;
                    zone = "-";
                }
                else
                {
                    zone = "+";
                }
                zone += offset.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
                date_string += " " + zone.PadRight(5, '0');
                return date_string;
            }
        }
        string HeaderDate;
        private string signaturePlainText;
        private string signaturePlainText2;
        private DuoResponseData? PreAuthResponse;
        private DuoResponseData? AuthResponse;

        protected string CanonicalizeRequest(string canon_params)
        {
            string[] lines = {
                RequestTimestamp,
                Method.ToString().ToUpperInvariant(),
                _apiHost.ToLower(),
                CurrentEnpoint,
                canon_params,
            };
            string canon = String.Join("\n",
                                       lines);
            return canon;
        }
        private string HmacSign(string data = "")
        {
            data = CanonicalizeRequest(data);
            byte[] key_bytes = ASCIIEncoding.ASCII.GetBytes(_clientSecret);
            HMACSHA512 hmac = new HMACSHA512(key_bytes);

            byte[] data_bytes = ASCIIEncoding.ASCII.GetBytes(data);
            hmac.ComputeHash(data_bytes);

            string hex = BitConverter.ToString(hmac.Hash);
            return hex.Replace("-", "").ToLower();
        }
     
        /// <summary>
        /// Checks if the Duo API is reachable
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Ping()
        {
            try
            {
                Method = HttpMethod.Get;

                RestClient client = NewClient(pingUri);

                var request = NewRequest(pingUri);
                Response = await client.ExecuteAsync(request);
                return Response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {

                return false;
            }
        }

        /// <summary>
        /// Checks if the Duo authentication settings are correct
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Check()
        {
            try
            {
                Method = HttpMethod.Get;


                RestClient client = NewClient(checkUri);

                var request = NewRequest(checkUri);
                Response = await client.ExecuteAsync(request);
                return Response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {

                return false;
            }
        }
        public static string FinishCanonicalize(string p)
        {
            // Signatures require upper-case hex digits.
            p = Regex.Replace(p,
                            "(%[0-9A-Fa-f][0-9A-Fa-f])",
                            c => c.Value.ToUpperInvariant());
            // Escape only the expected characters.
            p = Regex.Replace(p,
                            "([!'()*])",
                            c => "%" + Convert.ToByte(c.Value[0]).ToString("X"));
            p = p.Replace("%7E", "~");
            // UrlEncode converts space (" ") to "+". The
            // signature algorithm requires "%20" instead. Actual
            // + has already been replaced with %2B.
            p = p.Replace("+", "%20");
            return p;
        }
        public static string CanonicalizeParams(Dictionary<string, string> parameters)
        {
            var ret = new List<String>();
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                string p = String.Format("{0}={1}",
                                         HttpUtility.UrlEncode(pair.Key),
                                         HttpUtility.UrlEncode(pair.Value));

                p = FinishCanonicalize(p);
                ret.Add(p);
            }
            ret.Sort(StringComparer.Ordinal);
            return string.Join("&", ret.ToArray());
        }
        /// <summary>
        /// Checks that the user is authorized, if they are in Duo
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PreAuth(string username)
        {
            try
            {
                Method = HttpMethod.Post;

                var data = CanonicalizeParams(new() { { "username", username } });

                RestClient client = NewClient(preAuthUri, data);

                var request = NewRequest(preAuthUri);
                request.Method = RestSharp.Method.Post;
                request.AddParameter("username", username);
                PreAuthResponse = (await client.ExecuteAsync<DuoResponseData>(request)).Data;

                if (PreAuthResponse.Response.Result == "auth" || PreAuthResponse.Response.Result == "allow")
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {

                return false;
            }
        }

        /// <summary>
        /// Checks that the user is authorized, if they are in Duo
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Auth(string username)
        {
            try
            {
                Method = HttpMethod.Post;

                var data = CanonicalizeParams(new() { { "username", username },{ "factor","auto"},{"device","auto" } });

                RestClient client = NewClient(authUri, data);

                var request = NewRequest(authUri);
                request.Method = RestSharp.Method.Post;
                request.AddParameter("username", username);
                request.AddParameter("factor", "auto");
                request.AddParameter("device", "auto");
                 AuthResponse =(await client.ExecuteAsync<DuoResponseData>(request,CancellationTokenSource.Token)).Data;
                if (AuthResponse.Response.Result == "allow") return true;
                return false;
            }
            catch (Exception e)
            {

                return false;
            }
        }

    }
}
