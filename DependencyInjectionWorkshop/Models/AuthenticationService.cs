using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IProfileRepo _profileRepo;
        private readonly IHashedAdapter _hashedAdapter;
        private readonly IOtpService _otpService;
        private readonly INotifyAdapter _notifyAdapter;

        public AuthenticationService(IProfileRepo profileRepo, IHashedAdapter hashedAdapter, IOtpService otpService, INotifyAdapter notifyAdapter)
        {
            _profileRepo = profileRepo;
            _hashedAdapter = hashedAdapter;
            _otpService = otpService;
            _notifyAdapter = notifyAdapter;
        }

        public bool Verify(string accountId, string actualPassword, string actualOneTimePassword)
        {
            string expectPassword = _profileRepo.GetUserPassword(accountId);

            actualPassword = _hashedAdapter.GetHashedActualPassword(actualPassword);

            var expectOneTImePassword = _otpService.GetOneTimePassword(accountId);

            if (actualPassword == expectPassword && actualOneTimePassword == expectOneTImePassword)
            {
                return true;
            }
            else
            {
                _notifyAdapter.Notify();
                return false;
            }
        }
    }
}