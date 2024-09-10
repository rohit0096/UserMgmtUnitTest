using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserMgmnt.Model;
using UserMgmnt.Services;

namespace UserMgmt.Test.service
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private UserService _userService;

        [SetUp]
        public void Setup()
        {
            _userManagerMock = MockUserManager<ApplicationUser>();
            _userService = new UserService(_userManagerMock.Object);
        }

        [Test]
        public void GetUserId_ShouldReturnUserId_WhenClaimExists()
        {
            // Arrange
            var userId = "test-user-id";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, userId)
            }));

            // Act
            var result = _userService.GetUserId(claimsPrincipal);

            // Assert
            Assert.AreEqual(userId, result);
        }
        [Test]
        public void UpdateProfileAsync_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var userId = "non-existent-user-id";
            var fileUploadPath = "/path/to/file";
            var uploadedDateTime = DateTime.UtcNow;

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.UpdateProfileAsync(userId, fileUploadPath, uploadedDateTime));
            Assert.AreEqual("User not found", ex.Message);
        }

        [Test]
        public void UpdateProfileAsync_ShouldThrowException_WhenUpdateFails()
        {
            // Arrange
            var userId = "test-user-id";
            var fileUploadPath = "/path/to/file";
            var uploadedDateTime = DateTime.UtcNow;
            var user = new ApplicationUser { Id = userId };

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                            .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _userService.UpdateProfileAsync(userId, fileUploadPath, uploadedDateTime));
            Assert.AreEqual("Profile update failed", ex.Message);
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
