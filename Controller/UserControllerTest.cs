using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserMgmnt.Controllers;
using UserMgmnt.Services.Interface;

namespace UserMgmt.Test.Controller
{
    [TestFixture]
    public class UserControllerTest
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<ILogger<UserController>> _loggerMock;
        private UserController _controller;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UserController>>();
            _controller = new UserController(_userServiceMock.Object, _loggerMock.Object);

            // Mock the User property in the controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUserId"),
                new Claim(ClaimTypes.Name, "testUserName")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

        }

        [Test]
        public void GetUserProfileValidUserReturnsOkWithUserName()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("testUserName");

            // Act
            var result = _controller.GetUserProfile() as OkObjectResult;
            var userProfile = result.Value as dynamic;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void GetUserProfileUnauthorizedUserReturnsUnauthorized()
        {
            // Arrange
            _userServiceMock.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns((string)null);  // Simulate unauthorized user

            // Act
            var result = _controller.GetUserProfile() as UnauthorizedResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
        }

        [Test]
        public async Task UpdateProfileValidFileUploadReturnsOk()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Fake file content";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var uploadDateTime = DateTime.Now;

            // Act
            var result = await _controller.UpdateProfile(fileMock.Object, uploadDateTime) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);

            // Verify that the userService's UpdateProfileAsync was called
            _userServiceMock.Verify(x => x.UpdateProfileAsync("testUserId", It.IsAny<string>(), uploadDateTime), Times.Once);
        }

        [Test]
        public async Task UpdateProfileInvalidFileUploadReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();  // Invalid file (null or empty)

            // Act
            var result = await _controller.UpdateProfile(null, DateTime.Now) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }
    }
}
