using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profileRepo = Substitute.For<IProfileRepo>();
            var hashedAdapter = Substitute.For<IHashedAdapter>();
            var otpService = Substitute.For<IOtpService>();
            var logAdapter = Substitute.For<ILogAdapter>();
            var notifyAdapter = Substitute.For<INotifyAdapter>();
            var failedCounter = Substitute.For<IFailedCounter>();

            var authenticationService = new AuthenticationService(profileRepo, hashedAdapter, otpService, failedCounter,
                logAdapter, notifyAdapter);

            //設定mock資料
            otpService.GetOneTimePassword("brus").ReturnsForAnyArgs("123456");
            profileRepo.GetUserPassword("bruschen").ReturnsForAnyArgs("my hashed password");
            hashedAdapter.GetHashedActualPassword("pw").ReturnsForAnyArgs("my hashed password");

            var isValid = authenticationService.Verify("bruschen", "pw", "123456");
            Assert.IsTrue(isValid);
        }
    }
}