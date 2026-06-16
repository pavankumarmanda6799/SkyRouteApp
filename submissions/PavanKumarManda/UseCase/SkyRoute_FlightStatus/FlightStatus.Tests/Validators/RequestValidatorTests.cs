using System;
using FlightStatus.Api.Validators;
using Xunit;

namespace FlightStatus.Tests.Validators
{
    public class RequestValidatorTests
    {
        [Fact]
        public void ValidateFlightStatusRequest_NullFlightNumber_ReturnsInvalidWithErrorMessage()
        {
            // Arrange
            string? flightNumber = null;
            string? flightDate = "2023-01-01";
            // Act
            var result = RequestValidator.ValidateFlightStatusRequest(flightNumber, flightDate);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("flightNumber is required", result.ErrorMessage);
            Assert.Null(result.ParsedDate);
        }

        [Fact]
        public void ValidateFlightStatusRequest_WhitespaceFlightNumber_ReturnsInvalidWithErrorMessage()
        {
            // Arrange
            string? flightNumber = "   ";
            string? flightDate = "2023-01-01";
            // Act
            var result = RequestValidator.ValidateFlightStatusRequest(flightNumber, flightDate);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("flightNumber is required", result.ErrorMessage);
            Assert.Null(result.ParsedDate);
        }

        [Fact]
        public void ValidateFlightStatusRequest_NullFlightDate_ReturnsInvalidWithErrorMessage()
        {
            // Arrange
            string? flightNumber = "AB123";
            string? flightDate = null;
            // Act
            var result = RequestValidator.ValidateFlightStatusRequest(flightNumber, flightDate);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("flightDate is required", result.ErrorMessage);
            Assert.Null(result.ParsedDate);
        }

        [Fact]
        public void ValidateFlightStatusRequest_InvalidDateFormat_ReturnsInvalidWithFormatError()
        {
            // Arrange
            string? flightNumber = "AB123";
            // wrong format (dd-MM-yyyy)
            string? flightDate = "01-01-2023";
            // Act
            var result = RequestValidator.ValidateFlightStatusRequest(flightNumber, flightDate);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("date must be in yyyy-MM-dd format.", result.ErrorMessage);
            Assert.Null(result.ParsedDate);
        }

        [Fact]
        public void ValidateFlightStatusRequest_ValidDateWithWhitespace_ReturnsValidAndParsesDateAsUtc()
        {
            // Arrange
            string? flightNumber = "AB123";
            // include surrounding whitespace to ensure Trim() is used
            string? flightDate = " 2023-06-15 ";
            // Act
            var result = RequestValidator.ValidateFlightStatusRequest(flightNumber, flightDate);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.ParsedDate);

            // The parsed date should be the UTC date corresponding to the provided yyyy-MM-dd
            var parsed = result.ParsedDate!.Value;
            Assert.Equal(2023, parsed.Year);
            Assert.Equal(6, parsed.Month);
            Assert.Equal(15, parsed.Day);

            // When DateTimeStyles.AssumeUniversal is used, Kind may be Utc or Unspecified depending on ParseExact behavior,
            // but the UTC-equivalent instant should match the date provided. Ensure the date portion in UTC matches.
            var utc = parsed.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc) : parsed.ToUniversalTime();
            Assert.Equal(new DateTime(2023, 6, 15, 0, 0, 0, DateTimeKind.Utc), utc);
        }
    }
}
