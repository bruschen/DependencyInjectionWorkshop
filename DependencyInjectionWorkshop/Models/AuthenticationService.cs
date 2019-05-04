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
            _failedCounter.GetAccountIsLock(accountId);

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
    }
}