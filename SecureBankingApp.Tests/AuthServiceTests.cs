using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using Xunit;

namespace SecureBankingApp.Tests
{
    public class AuthServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void VerifyPassword_ValidCredentials_ReturnsTrueAndSetsSession()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();

            var authService = new AuthService(db, mockEmailService.Object, mockSessionService.Object);

            var plainPassword = "TestPassword123!";
            var hash = authService.HashPassword(plainPassword);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = hash,
                Role = UserRole.Customer
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Act
            var result = authService.VerifyPassword("testuser", plainPassword, out var returnedUser);

            // Assert
            Assert.True(result);
            Assert.NotNull(returnedUser);
            Assert.Equal(0, returnedUser.FailedLoginCount);
            Assert.Null(returnedUser.LockoutEnd);
            mockSessionService.Verify(s => s.SetToken(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void VerifyPassword_InvalidCredentials_IncrementsFailedLoginCount()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();

            var authService = new AuthService(db, mockEmailService.Object, mockSessionService.Object);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = authService.HashPassword("CorrectPassword"),
                Role = UserRole.Customer
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Act
            var result = authService.VerifyPassword("testuser", "WrongPassword", out var returnedUser);

            // Assert
            Assert.False(result);
            Assert.NotNull(returnedUser);
            Assert.Equal(1, returnedUser!.FailedLoginCount);
        }

        [Fact]
        public void VerifyPassword_FifthFailedAttempt_LocksAccount()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();

            var authService = new AuthService(db, mockEmailService.Object, mockSessionService.Object);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = authService.HashPassword("CorrectPassword"),
                FailedLoginCount = 4, // 4 failed attempts previously
                Role = UserRole.Customer
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Act
            var result = authService.VerifyPassword("testuser", "WrongPassword", out var returnedUser);

            // Assert
            Assert.False(result);
            Assert.NotNull(returnedUser);
            Assert.Equal(5, returnedUser.FailedLoginCount);
            Assert.NotNull(returnedUser.LockoutEnd);
            Assert.True(returnedUser.LockoutEnd.Value > DateTime.UtcNow);
        }

        [Fact]
        public void VerifyPassword_LockedAccount_RefusesValidLogin()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();

            var authService = new AuthService(db, mockEmailService.Object, mockSessionService.Object);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = authService.HashPassword("CorrectPassword"),
                FailedLoginCount = 5,
                LockoutEnd = DateTime.UtcNow.AddMinutes(10), // Currently locked out
                Role = UserRole.Customer
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Act
            var result = authService.VerifyPassword("testuser", "CorrectPassword", out var returnedUser);

            // Assert
            Assert.False(result); // Should still return false despite correct password
        }
    }
}
