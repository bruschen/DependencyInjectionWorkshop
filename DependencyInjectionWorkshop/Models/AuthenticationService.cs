using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileRepo _profileRepo;
        private readonly IHashedAdapter _sha256Adapter;
        private readonly IOtpService _otpService;
        private readonly ILogAdapter _nLogAdapter;
        private readonly INotifyAdapter _slackAdapter;
        private readonly IFailedCounter _failedCounter;

        public AuthenticationService(IProfileRepo profileRepo, IHashedAdapter sha256Adapter, IOtpService otpService, IFailedCounter failedCounter, ILogAdapter nLogAdapter, INotifyAdapter slackAdapter)
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
                _failedCounter.ResetFailCounter(accountId);

                return true;
            }
            else
            {
                _failedCounter.AddFailCounter(accountId);
                var failCount = _failedCounter.GetFailCounterValue(accountId);
                _nLogAdapter.LogMessage($"Fail Count:{failCount}");

                _slackAdapter.Notify();
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