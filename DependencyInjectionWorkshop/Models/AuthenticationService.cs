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

            //return hash.ToString();

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            var expectOneTImePassword = string.Empty;

            if (response.IsSuccessStatusCode)
            {
                expectOneTImePassword = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            if (actualPassword == expectPassword && actualOneTimePassword == expectOneTImePassword)
            {
                var httpClient1 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
                var response2 = httpClient1.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
                if (response2.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api error, accountId:{accountId}");
                }

                return true;
            }
            else
            {
                var httpClient1 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
                var response2 = httpClient1.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
                if (response2.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api error, accountId:{accountId}");
                }

                string temp = response2.Content.ReadAsAsync<string>().Result;

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");

                return false;
            }
        }
    }
}