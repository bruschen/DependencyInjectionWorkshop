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
        private const int DefaultFailCount = 1;
        private AuthenticationService _authenticationService;
        private IFailedCounter _failedCounter;
        private IHashedAdapter _hashedAdapter;
        private ILogAdapter _logAdapter;
        private INotifyAdapter _notifyAdapter;
        private IOtpService _otpService;
        private IProfileRepo _profileRepo;

        [SetUp]
        public void Setup()
        {
            _profileRepo = Substitute.For<IProfileRepo>();
            _hashedAdapter = Substitute.For<IHashedAdapter>();
            _otpService = Substitute.For<IOtpService>();
            _logAdapter = Substitute.For<ILogAdapter>();
            _notifyAdapter = Substitute.For<INotifyAdapter>();
            _failedCounter = Substitute.For<IFailedCounter>();

            _authenticationService = new AuthenticationService(_profileRepo, _hashedAdapter, _otpService,
                _failedCounter,
                _logAdapter, _notifyAdapter);
        }

        [Test]
        public void Add_failed_Count_When_invalid()
        {
            WhenInvalid();
            ShouldAddFailCount();
        }

        private void ShouldAddFailCount()
        {
            _failedCounter.Received(1).AddFailCounter(DefaultAccountId);
        }

        [Test]
        public void Is_valid()
        {
            var isValid = WhenVerify(DefaultAccountId, DefaultActualPassword, DefaultOneTimePassword);
            ShouldBeValid(isValid);
        }

        [Test]
        public void Is_valid_When_Wrong_Otp()
        {
            var isValid = WhenVerify(DefaultAccountId, DefaultActualPassword, "Otp Is wrong");
            ShoulbBeInvalid(isValid);
        }

        /// <summary>
        /// 登入失敗後，驗證是否有紀錄log
        /// </summary>
        [Test]
        public void Log_account_failed_count_when_invalid()
        {
            GivenFailCount(DefaultAccountId, DefaultFailCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailCount);
        }

        /// <summary>
        /// 登入失敗，驗證是否有發送通知
        /// </summary>
        [Test]
        public void Notify_User_WhenInvalid()
        {
            var isValid = WhenInvalid();
            ShouldNotifyUser();
        }

        /// <summary>
        /// 登入成功，驗證是否有成功reset fail count
        /// </summary>
        [Test]
        public void Reset_Fail_Count_When_Valid()
        {
            WhenVerify(DefaultAccountId, DefaultActualPassword, DefaultOneTimePassword);
            ShouldResetFailCounter(DefaultAccountId);
        }

        private void ShouldResetFailCounter(string accountId)
        {
            _failedCounter.Received(1).ResetFailCounter(accountId);
        }

        /// <summary>
        ///  驗證是否有發送訊息
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="failCount"></param>
        private void LogShouldContains(string accountId, int failCount)
        {
            _logAdapter.Received(1).LogMessage(Arg.Is<string>(m =>
                m.Contains(accountId) && m.Contains(failCount.ToString())));
        }

        private void GivenFailCount(string accountId, int failCount)
        {
            _failedCounter.GetFailCounterValue(accountId).ReturnsForAnyArgs(failCount);
        }

        private void ShouldNotifyUser()
        {
            _notifyAdapter.Received(1).Notify();
        }

        private bool WhenInvalid()
        {
            var isValid = WhenVerify(DefaultAccountId, DefaultActualPassword, "Otp Is wrong");
            return isValid;
        }

        private static void ShoulbBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string password, string oneTimePassword)
        {
            //設定mock資料
            GivenOtp(DefaultAccountId, DefaultOneTimePassword);
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHashed(DefaultActualPassword, DefaultHashedPassword);

            return _authenticationService.Verify(accountId, password, oneTimePassword);
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