using Findparts.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Findparts.Core;
using DAL;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using JWT.Builder;
using JWT.Algorithms;

namespace Findparts.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly FindPartsEntities _context;
        public ChatService (FindPartsEntities context)
        {
            _context = context;
        }

        public async Task<int> GetMemberId(int vendorId)
        {
            var user = _context.UserGetByVendorID(vendorId).FirstOrDefault();
            if (user == null) return 0;

            using (var client = new HttpClient())
            {
                var authContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("client_id", Config.WeavyClientId),
                    new KeyValuePair<string, string>("client_secret", Config.WeavyClientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                });
                var authResponse = await client.PostAsync($"{Config.WeavyUrl}/api/auth", authContent);
                var response = await authResponse.Content.ReadAsStringAsync();
                var jobject = JObject.Parse(response);
                var access_token = jobject.Value<string>("access_token");

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

                var userResponse = await client.GetAsync($"{Config.WeavyUrl}/api/users/byemail?email={user.Email}");
                if (userResponse.IsSuccessStatusCode)
                {
                    response = await userResponse.Content.ReadAsStringAsync();
                    jobject = JObject.Parse(response);
                    var id = jobject.Value<int>("id");
                    return id;
                } else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    StringContent content = null;
                    content = new StringContent(JsonConvert.SerializeObject(new { email = user.Email, username = user.Email.Split('@')[0].Replace(".", "") }), Encoding.UTF8, "application/json");
                    
                    userResponse = await client.PostAsync($"{Config.WeavyUrl}/api/users", content);
                    response = await userResponse.Content.ReadAsStringAsync();
                    if (userResponse.IsSuccessStatusCode)
                    {   
                        jobject = JObject.Parse(response);
                        var id = jobject.Value<int>("id");
                        return id;
                    } 
                }
            }
            return 0;
        }

        public string GetWeavyToken(string userId, string email)
        {
            return new JwtBuilder()
               .WithAlgorithm(new HMACSHA256Algorithm())
               .AddClaim("exp", DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds())
               .AddClaim("iss", "findparts")
               .AddClaim("sub", userId)
               .AddClaim("email", email)
               .AddClaim("client_id", Config.WeavyClientId)
               .WithSecret(Config.WeavyClientSecret)
               .Encode();
        }
    }
}