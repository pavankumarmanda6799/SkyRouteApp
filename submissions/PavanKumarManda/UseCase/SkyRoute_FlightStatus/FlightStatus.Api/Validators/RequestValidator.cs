using System;
using System.Globalization;

namespace FlightStatus.Api.Validators
{
    public static class RequestValidator
    {
        // Only strict date format (yyyy-MM-dd) is required per requirements.

        /// <summary>
        /// Validates query parameters for the flight status endpoint.
        /// Returns ValidationResult with ParsedDate when valid; otherwise returns error message.
        /// </summary>
        public static ValidationResult ValidateFlightStatusRequest(string? flightNumber, string? flightDate)
        {
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "flightNumber is required" };
            }

            if (string.IsNullOrWhiteSpace(flightDate))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "flightDate is required" };
            }

            // Parse date in strict yyyy-MM-dd format (UTC assumed)
            var trimmedDate = flightDate!.Trim();
            if (!DateTime.TryParseExact(
                trimmedDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var dateUtc))
            {
                return new ValidationResult { IsValid = false, ErrorMessage = "date must be in yyyy-MM-dd format." };
            }

            var result = new ValidationResult { IsValid = true, ParsedDate = dateUtc };

            return result;
        }

    }
}
