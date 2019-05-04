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
    public class NLogAdapter
    {
        public void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class SlackAdapter
    {
        public void NotifyBySlack()
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
        }
    }

    public class FailedCounter
    {
        public int GetFailCounterValue(string accountId)
        {
            var httpClient3 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response4 = httpClient3.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            response4.EnsureSuccessStatusCode();
            int failCount = response4.Content.ReadAsAsync<int>().Result;
            return failCount;
        }

        public void AddFailCounter(string accountId)
        {
            var httpClient3 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response3 = httpClient3.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            response3.EnsureSuccessStatusCode();
            if (response3.IsSuccessStatusCode == false)
            {
            }
            else
            {
                throw new Exception($"web api FailCounter Add error, accountId:{accountId}");
            }
        }
    }

    public class OtpService
    {
        public string GetOneTimePassword(string accountId)
        {
            var httpClient1 = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var httpClient1Response = httpClient1.PostAsJsonAsync("api/otps", accountId).Result;
            httpClient1Response.EnsureSuccessStatusCode();

            if (httpClient1Response.IsSuccessStatusCode)
            {
                return httpClient1Response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api get opt error, accountId:{accountId}");
            }
        }
    }

    public class ProfileRepo
    {
        public string GetUserPassword(string accountId)
        {
            string expectPassword;
            using (var connection = new SqlConnection("my connection string"))
            {
                expectPassword = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return expectPassword;
        }
    }

    public class Sha256Adapter
    {
        public string GetHashedActualPassword(string actualPassword)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(actualPassword));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            actualPassword = hash.ToString();
            return actualPassword;
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo = new ProfileRepo();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string actualPassword, string actualOneTimePassword)
        {
            GetAccountIsLock(accountId);

            string expectPassword = _profileRepo.GetUserPassword(accountId);

            actualPassword = _sha256Adapter.GetHashedActualPassword(actualPassword);

            var expectOneTImePassword = _otpService.GetOneTimePassword(accountId);

            if (actualPassword == expectPassword && actualOneTimePassword == expectOneTImePassword)
            {
                F.ResetFailCounter(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailCounter(accountId);
                var failCount = _failedCounter.GetFailCounterValue(accountId);
                _nLogAdapter.LogMessage($"Fail Count:{failCount}");

                _slackAdapter.NotifyBySlack();
                return false;
            }
        }

        private static void GetAccountIsLock(string accountId)
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
        }
    }
}