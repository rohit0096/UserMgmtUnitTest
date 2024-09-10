using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UserMgmnt.Controllers;
using UserMgmnt.Model;
using UserMgmnt.Services.Interface;

namespace UserMgmt.Test.Controller
{
    [TestFixture]
    public class AuthControllerTest
    {
        private Mock<IAuthService> _authServiceMock;
        private Mock<ILogger<AuthController>> _loggerMock;
        private AuthController _controller;

        [SetUp]
        public void SetUp()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
        }


        [Test]
        public async Task RegisterValidModelReturnsOkResult()
        {
            // Arrange
            var model = new Register
            {
                Username = "validUser",
                Password = "ValidPassword123",
                Email = "user@example.com"
            };

            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<Register>()))
               .ReturnsAsync(IdentityResult.Success);
            // Act
            var result = await _controller.Register(model) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task RegisterServiceFailureReturnsBadRequest()
        {
            // Arrange
            var model = new Register
            {
                Username = "testUser",
                Password = "ValidPassword123",
                Email = "user@example.com"
            };

            _authServiceMock.Setup(x => x.RegisterAsync(It.IsAny<Register>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Username already exists" }));

            // Act
            var result = await _controller.Register(model) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public async Task LoginValidModelReturnsOkResultWithToken()
        {
            // Arrange
            var model = new Login
            {
                Username = "validUser",
                Password = "ValidPassword123"
            };

            var fakeToken = "fake-jwt-token";
            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<Login>()))
                .ReturnsAsync(fakeToken);

            // Act
            var result = await _controller.Login(model) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task LoginInvalidModelStateReturnsBadRequest()
        {
            // Arrange
            var model = new Login
            {
                Username = "invalidUser",
                Password = "" 
            };

            _controller.ModelState.AddModelError("Password", "Password is required");

            // Act
            var result = await _controller.Login(model) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }
        
        [Test]
        public async Task LoginInvalidCredentialsReturnsUnauthorized()
        {
            // Arrange
            var model = new Login
            {
                Username = "invalidUser",
                Password = "wrongPassword"
            };

            _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<Login>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _controller.Login(model) as UnauthorizedResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
        }
    }

}
