using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using UserMgmnt.Model;
using UserMgmnt.Services;

namespace UserMgmt.Test.service
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private Mock<ILogger<AuthService>> _loggerMock;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            _userManagerMock = MockUserManager<ApplicationUser>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _jwtSettingsMock.Setup(x => x.Value).Returns(new JwtSettings { Secret = "YourSuperSecretKey" });

            _authService = new AuthService(_userManagerMock.Object, _jwtSettingsMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task UserCreationIsSuccessfulReturnsSucceededResult()
        {
            // Arrange
            var registerModel = new Register { Username = "testUser", Email = "test@test.com", Password = "Test@123" };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(registerModel);

            // Assert
            Assert.IsTrue(result.Succeeded);
        }
        private Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();

            return new Mock<UserManager<TUser>>(
                store.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<TUser>>().Object,
                new IUserValidator<TUser>[0],
                new IPasswordValidator<TUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<TUser>>>().Object);
        }
    }

}
