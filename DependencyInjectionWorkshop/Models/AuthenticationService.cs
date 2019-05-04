using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
            _slackAdapter = new SlackAdapter();
        }

        public AuthenticationService(ProfileRepo profileRepo, Sha256Adapter sha256Adapter, OtpService otpService, FailedCounter failedCounter, NLogAdapter nLogAdapter, SlackAdapter slackAdapter)
        {
            _profileRepo = profileRepo;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _nLogAdapter = nLogAdapter;
            _slackAdapter = slackAdapter;
        }

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