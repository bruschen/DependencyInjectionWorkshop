using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "bruschen";
        private const string DefaultActualPassword = "pw";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultOneTimePassword = "123456";
        private IProfileRepo _profileRepo;
        private IHashedAdapter _hashedAdapter;
        private IOtpService _otpService;
        private ILogAdapter _logAdapter;
        private INotifyAdapter _notifyAdapter;
        private IFailedCounter _failedCounter;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _profileRepo = Substitute.For<IProfileRepo>();
            _hashedAdapter = Substitute.For<IHashedAdapter>();
            _otpService = Substitute.For<IOtpService>();
            _logAdapter = Substitute.For<ILogAdapter>();
            _notifyAdapter = Substitute.For<INotifyAdapter>();
            _failedCounter = Substitute.For<IFailedCounter>();

            _authenticationService = new AuthenticationService(_profileRepo, _hashedAdapter, _otpService, _failedCounter,
                _logAdapter, _notifyAdapter);
        }

        [Test]
        public void is_valid()
        {
            //設定mock資料
            GivenOtp(DefaultAccountId, DefaultOneTimePassword);
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHashed(DefaultActualPassword, DefaultHashedPassword);

            var isValid = _authenticationService.Verify(DefaultAccountId, DefaultActualPassword, DefaultOneTimePassword);
            Assert.IsTrue(isValid);
        }

        private void GivenHashed(string password, string hashedPassword)
        {
            _hashedAdapter.GetHashedActualPassword(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPassword(string accountId, string hashedPassword)
        {
            _profileRepo.GetUserPassword(accountId).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenOtp(string accountId, string oneTimePassword)
        {
            _otpService.GetOneTimePassword(accountId).ReturnsForAnyArgs(oneTimePassword);
        }
    }
}