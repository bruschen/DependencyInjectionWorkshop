using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class FailedCounterDecorator:IAuthenticationService
    {
        private IFailedCounter _failedCounter;
        private IAuthenticationService _authenticationService;

        public FailedCounterDecorator(IAuthenticationService authenticationService, IFailedCounter failedCounter)
        {
            _failedCounter = failedCounter;
            _authenticationService = authenticationService;
        }

        public bool Verify(string accountId, string actualPassword, string actualOneTimePassword)
        {
            _failedCounter.GetAccountIsLock(accountId);

            var isVerify = _authenticationService.Verify(accountId, actualPassword, actualOneTimePassword);

            if (isVerify == true)
            {
                _failedCounter.ResetFailCounter(accountId);
            }
            else
            {
                _failedCounter.AddFailCounter(accountId);
            }

            return isVerify;
        }
    }
}
