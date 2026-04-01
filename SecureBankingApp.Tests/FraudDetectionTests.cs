using System;
using Microsoft.EntityFrameworkCore;
using SecureBankingApp.Database;
using SecureBankingApp.Models;
using SecureBankingApp.Services;
using Xunit;

namespace SecureBankingApp.Tests
{
    public class FraudDetectionTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void CalculateRiskScore_LocationMismatch_Adds50Points()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new FraudDetectionService(db);
            
            var user = new User 
            { 
                Username = "test", 
                HomeLocation = "London, UK",
                FailedLoginCount = 0 
            };

            // Act - Logging in from a completely different location
            var score = service.CalculateRiskScore(user, "192.168.1.1", "Tokyo, Japan");

            // Assert
            Assert.Equal(50, score);
        }

        [Fact]
        public void CalculateRiskScore_HighFailedLogins_Adds30Points()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new FraudDetectionService(db);
            
            var user = new User 
            { 
                Username = "test", 
                HomeLocation = "London, UK",
                FailedLoginCount = 4 // Exceeds threshold of 3
            };

            // Act - Logging in from home
            var score = service.CalculateRiskScore(user, "192.168.1.1", "London, UK");

            // Assert
            Assert.Equal(30, score);
        }

        [Fact]
        public void CalculateRiskScore_TotalAnomalousLogin_Scores80AndLogsFraud()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new FraudDetectionService(db);
            
            var user = new User 
            { 
                Username = "hacker_target", 
                HomeLocation = "New York, USA",
                FailedLoginCount = 5 
            };

            // Act - Logging in from Moscow after 5 failed attempts
            var score = service.CalculateRiskScore(user, "10.0.0.1", "Moscow, Russia");

            // Assert
            Assert.Equal(80, score);
            
            var logs = db.FraudLogs.AsyncEnumerable();
            Assert.Single(db.FraudLogs); // Ensure the automatic DB log was triggered
        }

        [Theory]
        [InlineData("London, UK", "london, uk", true)]
        [InlineData("New York, NY", "New York", true)]
        [InlineData("Paris, France", "Berlin, Germany", false)]
        public void LocationsAreSimilar_EvaluatesCorrectly(string locA, string locB, bool expected)
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new FraudDetectionService(db);

            // Act
            var result = service.LocationsAreSimilar(locA, locB);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
