using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class LogDecorator: IAuthenticationService
    {
        private IAuthenticationService _authenticationService;
        private ILogAdapter _logAdapter;
        private IFailedCounter _failedCounter;

        public LogDecorator(IAuthenticationService authenticationService, ILogAdapter logAdapter, IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _logAdapter = logAdapter;
            _failedCounter = failedCounter;
        }

        public bool Verify(string accountId, string actualPassword, string actualOneTimePassword)
        {
            var isVerify = _authenticationService.Verify(accountId, actualPassword, actualOneTimePassword);

            if (isVerify==false)
            {
                LogVerify(accountId);
            }

            return isVerify;
        }

        private void LogVerify(string accountId)
        {
            var failCounterValue = _failedCounter.GetFailCounterValue(accountId);
            _logAdapter.LogMessage($"{accountId} - fail login count {failCounterValue}");
        }
    }
}
