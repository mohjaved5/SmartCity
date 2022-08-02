using Newtonsoft.Json;
using PSSK_POC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PSSK_POC.Services
{
    public class AuthenticationService
    {
        readonly string clientId = "l3jHAmQz86pM0S1inXhbT3TyEKOwrOqu";
        readonly string clientSecret = "o6YB7WHCNoi_fRrUb5UN9G39ul46reSO92X6f6Tmp4yEbqdHAn877CTPmgW9ltu1";
        readonly string audience = "https://dev-c1w9u7zc.jp.auth0.com/api/v2/";
        readonly string connection = "TestConnection";
        readonly string redirectUrl = "{RedirectUrl}";

        public AuthenticationService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public TokenResponse GetToken()
        {
            var body = new TokenRequest() { client_id = clientId, client_secret = clientSecret, audience = audience, grant_type = "client_credentials" };
            var json = JsonConvert.SerializeObject(body);

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = HttpClient.PostAsync("https://dev-c1w9u7zc.jp.auth0.com/oauth/token", httpContent).Result;
            if (!response.IsSuccessStatusCode)
                return new TokenResponse();

            var result = JsonConvert.DeserializeObject<TokenResponse>(response.Content.ReadAsStringAsync().Result);
            return result;
        }

        public ProfileResponse Signup(ProfileRequest profile)
        {
            var token = GetToken();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"{token.token_type} {token.access_token}");

            profile.connection = connection;
            profile.client_id = clientId;

            var json = JsonConvert.SerializeObject(profile);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = client.PostAsync("https://dev-c1w9u7zc.jp.auth0.com/dbconnections/signup", httpContent).Result;
            if (!response.IsSuccessStatusCode)
            {
                var a = response.Content.ReadAsStringAsync().Result;
                return new ProfileResponse();
            }
            var result = JsonConvert.DeserializeObject<ProfileResponse>(response.Content.ReadAsStringAsync().Result);
            return result;
        }

        public ProfileResponse GetProfile(string Access_Key)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Access_Key}");

            var response = client.GetAsync($"https://dev-c1w9u7zc.jp.auth0.com/UserInfo").Result;
            if (!response.IsSuccessStatusCode)
            {
                var a = response.Content.ReadAsStringAsync().Result;
                return new ProfileResponse();
            }
            ProfileResponse result = JsonConvert.DeserializeObject<ProfileResponse>(response.Content.ReadAsStringAsync().Result);
            return result;
        }

        public ProfileResponse GetProfile1(string Access_Key)
        {
            var a = Access_Key.Split('&')[0];
            var b = a.Split('=')[1];
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {b}");

            var response = client.GetAsync($"https://dev-c1w9u7zc.jp.auth0.com/UserInfo").Result;
            if (!response.IsSuccessStatusCode)
            {
                var abc = response.Content.ReadAsStringAsync().Result;
                return new ProfileResponse();
            }
            ProfileResponse result = JsonConvert.DeserializeObject<ProfileResponse>(response.Content.ReadAsStringAsync().Result);
            return result;
        }

        public string GetLoginUrl(int type)
        {
            string connection = type switch
            {
                1 => "TestConnection",
                2 => "google-auth2",
                3 => "facebook",
                4 => "siwe",
                5 => "windowslive",
                _ => string.Empty,
            };
            var url = $"https://dev-c1w9u7zc.jp.auth0.com/dbconnections/authorize?response_type=token" 
                + (!string.IsNullOrEmpty(connection) ? $"&connection={connection}" : string.Empty) 
                + $"&client_id={clientId}&redirect_uri={redirectUrl}&scope=openid%20profile%20email&state=STATE";
            return url;
        }

        public string GetLogoutUrl()
        {
            return $"https://dev-c1w9u7zc.jp.auth0.com/v2/logout?client_id={clientId}&returnTo={redirectUrl}";
        }


        public ProfileResponse GetUserMetadata(string userId)
        {
            ProfileResponse result = null;
            var token = GetToken();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", "Bearer " + token.access_token);

            var response = client.GetAsync($"https://dev-c1w9u7zc.jp.auth0.com/api/v2/users/{userId}").Result;
            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ProfileResponse>(response.Content.ReadAsStringAsync().Result);
                result.sub = userId;
            }
            else
            {
                if (response.ReasonPhrase == "Not Found")
                    throw new Exception("User Not Found");
            }
            return result;
        }

        public bool UpdateUserMetadata(UserMetadataRequest metadata, string userId)
        {
            var token = GetToken();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("authorization", "Bearer " + token.access_token);

            var json = JsonConvert.SerializeObject(metadata);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = client.PatchAsync($"https://dev-c1w9u7zc.jp.auth0.com/api/v2/users/{userId}", httpContent).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return true;
        }
    }
}
