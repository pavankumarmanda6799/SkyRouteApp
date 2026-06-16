using System;

namespace FlightStatus.Api.Validators
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ParsedDate { get; set; }
    }
}
