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
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNet.Identity.Owin;

namespace Findparts.Services.Services
{
    public class WeavyService : IWeavyService
    {
        private readonly FindPartsEntities _context;
        private static string _accessToken;
        private static object _locker = new object();
        public WeavyService (FindPartsEntities context)
        {
            _context = context;
        }

        public async Task<int> GetCollabChannel(int vendorId, string userId)
        {
            var userProfile = _context.UserGetByProviderUserKey2(new Guid(userId)).FirstOrDefault();
            var vendorUsers = _context.UserGetByVendorID(vendorId).ToList();
            var vendor = _context.VendorGetByID(vendorId).FirstOrDefault();

            var createUserId = await CreateOrRetriveMemberFromWeavy(userProfile.Email);

            var spaceInfo = await CreateOrRetrieveCollabSpaceFromWeavy($"collab_{vendorId}_{userId}", $"{vendor.VendorName} & {userProfile.Email}", createUserId); // Item1: spaceId, Item2: isNew
            if (spaceInfo.Item2 == true)
            {
                var memberIds = await BulkRegisterOrRetrieveUsersFromWeavy(vendorUsers.Select(x => x.Email).ToList());

                memberIds.Add(createUserId);

                await AddMembersToSpace(spaceInfo.Item1, memberIds);
            }

            return spaceInfo.Item1;
        }
        public async Task<int> GetServiceRequestChannel(int vendorId, string userId)
        {
            var userProfile = _context.UserGetByProviderUserKey2(new Guid(userId)).FirstOrDefault();
            var vendorUsers = _context.UserGetByVendorID(vendorId).ToList();
            var vendor = _context.VendorGetByID(vendorId).FirstOrDefault();

            var createUserId = await CreateOrRetriveMemberFromWeavy(userProfile.Email);

            var spaceInfo = await CreateOrRetrieveServiceRequestSpaceFromWeavy($"gigs_{vendorId}_{userId}", $"{vendor.VendorName} & {userProfile.Email}", createUserId); // Item1: spaceId, Item2: isNew
            if (spaceInfo.Item2 == true)
            {
                var memberIds = await BulkRegisterOrRetrieveUsersFromWeavy(vendorUsers.Select(x => x.Email).ToList());

                memberIds.Add(createUserId);

                await AddMembersToSpace(spaceInfo.Item1, memberIds);
            }

            return spaceInfo.Item1;
        }
        public async Task<int> GetMemberId(int vendorId)
        {
            var user = _context.UserGetByVendorID(vendorId).FirstOrDefault();
            if (user == null) return 0;

            return await CreateOrRetriveMemberFromWeavy(user.Email);            
        }

        private async Task<bool> ValidateAccessToken()
        {
            bool needRefresh = false;
            if (string.IsNullOrEmpty(_accessToken))
                needRefresh = true;

            if (!needRefresh)
            {
                var jwtToken = new JwtSecurityToken(_accessToken);
                if ((jwtToken == null) || (jwtToken.ValidFrom > DateTime.UtcNow) || (jwtToken.ValidTo < DateTime.UtcNow))
                    needRefresh = true;
            }

            if (needRefresh)
            {
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
                    _accessToken = jobject.Value<string>("access_token");
                }
            }
            return true;
        }
        private async Task<HttpClient> CreateWeavyHttpClient()
        {

            await ValidateAccessToken();
            
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            return client;
        }
        private async Task<Tuple<int, bool>> CreateOrRetrieveCollabSpaceFromWeavy(string key, string name, int createUser)
        {   
            using (var client = await CreateWeavyHttpClient())
            {
                StringContent payload = null;
                payload = new StringContent(JsonConvert.SerializeObject(new { key = key, name = name, user = createUser }), Encoding.UTF8, "application/json");
                var spaceResponse = await client.PostAsync($"{Config.WeavyUrl}/api/collab-spaces", payload);
                if (spaceResponse.IsSuccessStatusCode)
                {
                    var content = await spaceResponse.Content.ReadAsStringAsync();

                    var jobject = JObject.Parse(content);
                    return new Tuple<int, bool>(jobject.Value<int>("id"), spaceResponse.StatusCode == System.Net.HttpStatusCode.Created);
                }
               
            }
            return new Tuple<int, bool>(0, false);
        }
        private async Task<Tuple<int, bool>> CreateOrRetrieveServiceRequestSpaceFromWeavy(string key, string name, int createUser)
        {
            using (var client = await CreateWeavyHttpClient())
            {
                StringContent payload = null;
                payload = new StringContent(JsonConvert.SerializeObject(new { key = key, name = name, user = createUser }), Encoding.UTF8, "application/json");
                var spaceResponse = await client.PostAsync($"{Config.WeavyUrl}/api/service-request-spaces", payload);
                if (spaceResponse.IsSuccessStatusCode)
                {
                    var content = await spaceResponse.Content.ReadAsStringAsync();

                    var jobject = JObject.Parse(content);
                    return new Tuple<int, bool>(jobject.Value<int>("id"), spaceResponse.StatusCode == System.Net.HttpStatusCode.Created);
                }

            }
            return new Tuple<int, bool>(0, false);
        }
        private async Task<List<int>> BulkRegisterOrRetrieveUsersFromWeavy(List<string> emails)
        {
            var res = new List<int>();
            using (var client = await CreateWeavyHttpClient())
            {
                var str = JsonConvert.SerializeObject(emails.Select(x => new { email = x, user_name = x.Split('@')[0].Replace(".", "") }).ToList());                
                var content = new StringContent(str, Encoding.UTF8, "application/json");
                var usersResponse = await client.PostAsync($"{Config.WeavyUrl}/api/users/bulk-register", content);
                var response = await usersResponse.Content.ReadAsStringAsync();
                if (usersResponse.IsSuccessStatusCode)
                {
                    
                    var jArray = JArray.Parse(response);
                    foreach (var obj in jArray)
                    {
                        res.Add(obj.Value<int>("id"));
                    }
                }
            }
            return res;
        }
        private async Task<int> CreateOrRetriveMemberFromWeavy(string email)
        {
            using (var client = await CreateWeavyHttpClient())
            {
                string response;
                JObject jobject;

                var userResponse = await client.GetAsync($"{Config.WeavyUrl}/api/users/byemail?email={email}");
                if (userResponse.IsSuccessStatusCode)
                {
                    response = await userResponse.Content.ReadAsStringAsync();
                    jobject = JObject.Parse(response);
                    var id = jobject.Value<int>("id");
                    return id;
                }
                else if (userResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    StringContent content = null;
                    content = new StringContent(JsonConvert.SerializeObject(new { email = email, username = email.Split('@')[0].Replace(".", "") }), Encoding.UTF8, "application/json");

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
        private async Task AddMembersToSpace(int spaceId, List<int> members)
        {
            using (var client = await CreateWeavyHttpClient ())
            {
                StringContent content = null;
                content = new StringContent(JsonConvert.SerializeObject(members), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{Config.WeavyUrl}/api/spaces/{spaceId}/members", content);
            }
        }
        public string GetWeavyToken(string userId, string email)
        {
            if (string.IsNullOrEmpty(userId))
            {
                var userManager = System.Web.HttpContext.Current.Request.GetOwinContext()
                                .GetUserManager<ApplicationUserManager>();
                var userTask = userManager.FindByEmailAsync(email);
                Task.WaitAll(userTask);
                userId = userTask.Result.Id;
            }

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