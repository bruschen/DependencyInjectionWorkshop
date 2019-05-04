using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string actualPassword, string actualOneTimePassword)
        {
            var httpClient4 = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var httpClient4Response = httpClient4.PostAsJsonAsync("api/Account/IsLock", accountId).Result;
            httpClient4Response.EnsureSuccessStatusCode();
            if (httpClient4Response.IsSuccessStatusCode)
            {
                if (Boolean.Parse(httpClient4Response.Content.ReadAsAsync<string>().Result) == true)
                {
                    throw new Exception($"Account Fail too many time , accountId:{accountId}");
                }
            }
            else
            {
                throw new Exception($"web api fail too many error, accountId:{accountId}");
            }

            string expectPassword = string.Empty;

            using (var connection = new SqlConnection("my connection string"))
            {
                expectPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(actualPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            expectPassword = hash.ToString();

            var httpClient1 = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var httpClient1Response = httpClient1.PostAsJsonAsync("api/otps", accountId).Result;
            httpClient1Response.EnsureSuccessStatusCode();
            var expectOneTImePassword = string.Empty;

            if (httpClient1Response.IsSuccessStatusCode)
            {
                expectOneTImePassword = httpClient1Response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api get opt error, accountId:{accountId}");
            }

            if (actualPassword == expectPassword && actualOneTimePassword == expectOneTImePassword)
            {
                var httpClient2 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
                var response2 = httpClient2.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
                if (response2.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api FailCounter reset, accountId:{accountId}");
                }

                return true;
            }
            else
            {
                var httpClient3 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
                var response3 = httpClient3.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
                if (response3.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api FailCounter Add error, accountId:{accountId}");
                }

                string temp = response3.Content.ReadAsAsync<string>().Result;

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");

                return false;
            }
        }
    }
}