using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileRepo _profileRepo;
        private readonly IHashedAdapter _hashedAdapter;
        private readonly IOtpService _otpService;
        private readonly ILogAdapter _logAdapter;
        private readonly INotifyAdapter _notifyAdapter;
        private readonly IFailedCounter _failedCounter;

        public AuthenticationService(IProfileRepo profileRepo, IHashedAdapter hashedAdapter, IOtpService otpService, IFailedCounter failedCounter, ILogAdapter logAdapter, INotifyAdapter notifyAdapter)
        {
            _profileRepo = profileRepo;
            _hashedAdapter = hashedAdapter;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _logAdapter = logAdapter;
            _notifyAdapter = notifyAdapter;
        }

        public bool Verify(string accountId, string actualPassword, string actualOneTimePassword)
        {
            _failedCounter.GetAccountIsLock(accountId);

            string expectPassword = _profileRepo.GetUserPassword(accountId);

            actualPassword = _hashedAdapter.GetHashedActualPassword(actualPassword);

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
                _logAdapter.LogMessage($"{accountId}-Fail Count:{failCount}");

                _notifyAdapter.Notify();
                return false;
            }
        }
    }
}