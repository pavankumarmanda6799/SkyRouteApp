# AI Prompts Log

This document captures the significant AI prompts used during the implementation of the SkyRoute Flight Status application. The prompts are organized according to the implementation flow followed during development.

---

# 1. Backend Development

## 1.1 Flight Status Response Model

### Prompt

```text
here is the project Context
We are building a Flight Status application using .NET and Angular.

consider below requirement
Create a response model named `FlightStatusResultResponse` with the following fields:

* FlightNumber
* Status
* ScheduledDepartureTime
* ScheduledArrivalTime
* ActualDepartureTime (optional)
* ActualArrivalTime (optional)
* Terminal (optional)
* Gate (optional)
* DelayReason (optional)
* LastUpdatedTimestamp
* Message (optional)

Use appropriate .NET data types and nullable types for optional fields.
```

### Intent

Create a unified response model that can be returned by the backend API and consumed by the Angular application.

### Outcome

Generated the initial structure for `FlightStatusResultResponse` including nullable properties for optional provider-specific fields.

### Files Impacted

```text
Models/FlightStatusResultResponse.cs
```

---

## 1.2 Provider Models, Status Mapping and Sample Data

### Prompt

```text
Requirement

Create sample provider data files:

* aerospace-data.json
* quickflight-data.json

The JSON structure must match the corresponding provider models located in the Models folder.

Unified Status Requirements

Create a `UnifiedFlightStatus` enum with the following values:

* OnTime
* Delayed
* Cancelled
* Diverted
* Unknown

Provider Status Mapping

AeroTrack:

* ON_TIME - OnTime
* LATE - Delayed
* CANCELLED - Cancelled
* DIVERTED - Diverted
* Any other value - Unknown

QuickFlight:

* ON_SCHEDULE - OnTime
* DELAYED - Delayed
* CANCELED - Cancelled
* REROUTED - Diverted
* Any other value - Unknown

Create data in such a way it covers below Business Scenarios

1. Flight data is sourced from static JSON files only.
2. If multiple providers return a flight, select the record with the most recent LastUpdatedTimestamp.
3. If only one provider returns a flight, return that result.
4. If no provider returns a flight, return a response with status Unknown.
5. Include FlightDate in all provider models and sample data.
```

### Intent

Define unified status mappings and create deterministic provider data for testing business scenarios.

### Outcome

Generated provider-specific sample data, status mapping rules and unified status enumeration.

### Files Impacted

```text
Data/aerospace-data.json
Data/quickflight-data.json
Models/AeroTrackResponse.cs
Models/QuickFlightResponse.cs
Models/UnifiedFlightStatus.cs
```

---

## 1.3 Provider Implementations

### Prompt

```text
Requirement - Implement the interface IFlightStatusProvider.
provide implementations to AeroTrackProvider, QuickFlightProvider which inherits IFlightStatusProvider

requirement:
Read provider-specific data from the corresponding JSON file in the Data folder.
Accept flightNumber and flightDate as input parameters.
Return the matching flight record.
```

### Intent

Implement provider abstraction and data retrieval logic.

### Outcome

Created provider implementations that load data from JSON files and return matching records based on flight number and flight date.

### Files Impacted

```text
Providers/IFlightStatusProvider.cs
Providers/AeroTrackProvider.cs
Providers/QuickFlightProvider.cs
```

---

## 1.4 Flight Status Service and Normalization Service

### Prompt

```text
NormalizationService Responsibilities

Map provider-specific models to FlightStatusResultResponse.
Convert provider statuses to UnifiedFlightStatus.
Contain only normalization and mapping logic.

FlightStatusService Responsibilities

Call all configured providers.
Compare LastUpdatedTimestamp values.
Select the latest provider response.
pass the response to NormalizationService to normalize and have unified status.

Do not place provider lastupdatedtime comparison logic inside NormalizationService.
```

### Intent

Separate business responsibilities between provider orchestration and response normalization.

### Outcome

Implemented:

* FlightStatusService
* FlightStatusNormalizeService

with clear separation of concerns.

### Files Impacted

```text
Services/FlightStatusService.cs
Services/FlightStatusNormalizeService.cs
```

---

## 1.5 Refactoring and Code Cleanup

### Prompt

```text
Refactoring Requirements

1. Replace fully qualified namespace references with appropriate using statements.
2. Reduce duplicate code across services and providers where possible.
3. Preserve existing functionality and business behavior.
4. Improve readability and maintainability.
```

### Intent

Improve code quality and reduce duplication.

### Outcome

Refactored services and providers to improve readability while preserving existing behavior.

### Files Impacted

```text
Services/*
Providers/*
```

---

## 1.6 CORS Configuration

### Prompt

```text
Configure CORS for Angular frontend access.

Allowed Origin - http://localhost:4200

consider below points while adding cors policy -
Store allowed origins in appsettings.json.
Read configuration values through IConfiguration.
Configure the CORS policy in Program.cs.
Avoid hardcoded origins inside Program.cs.
```

### Intent

Enable secure communication between Angular frontend and .NET backend during development.

### Outcome

Configured CORS using application configuration rather than hardcoded values.

### Files Impacted

```text
Program.cs
appsettings.json
```

---

# 2. Frontend Development

## 2.1 Search Screen

### Prompt

```text
Angular UI Requirements

Search Screen
Provide an Angular Material text input for Flight Number, DatePicker for Flight Date.
display validation and API errors in a dedicated error card beneath the inputs.
```

### Intent

Build the flight search experience for support agents.

### Outcome

Implemented Angular Material search form with validation and error handling.

### Files Impacted

```text
flight-search.component.ts
flight-search.component.html
flight-search.component.scss
```

---

## 2.2 Backend API Integration

### Prompt

```text
do api integration for below endpoint
GET /flights/status?flightNumber={flightNumber}&flightDate={flightDate}
add endpoint details in environment.ts - http://localhost:5120

Result Display

display flight details in card-based layout.
Display all fields from FlightStatusResultResponse.
Show Gate, Terminal and DelayReason only when values are available.
```

### Intent

Connect Angular application with backend API and display normalized flight information.

### Outcome

Implemented Angular service integration and result rendering logic.

### Files Impacted

```text
services/flight-status.service.ts
environments/environment.ts
flight-status-result.component.ts
flight-status-result.component.html
```

---

## 2.3 Status Visualization and UI Improvements

### Prompt

```text
Status Colour Coding

OnTime -Green
Delayed -Amber
Cancelled -Red
Diverted -Red
Unknown -Grey

also include below points :
Use exact API response field names when mapping data.
Add a header with the application title "SkyRoute".
Adjust spacing, margins and padding for a clean layout.
Display meaningful error messages for API failures.
```

### Intent

Improve user experience and visual representation of flight statuses.

### Outcome

Implemented:

* Status color coding
* Application header
* Improved layout and spacing
* Enhanced API error messages

### Files Impacted

```text
flight-status-result.component.scss
app.component.html
app.component.scss
```

---

# 3. Unit Testing

## 3.1 FlightStatusService Unit Tests

### Prompt

```text
Generate xUnit test cases for FlightStatusService.

Cover scenarios:

- Both providers return results
- Latest LastUpdatedTimestamp wins
- Only AeroTrack returns a result
- Only QuickFlight returns a result
- No provider returns data
- Provider throws exception
```

### Intent

Validate provider selection and orchestration logic.

### Outcome

Implemented unit tests covering the primary business scenarios for provider selection and fallback handling.

### Files Impacted

```text
FlightStatus.Tests/Services/FlightStatusServiceTests.cs
```

---

## 3.2 FlightStatusNormalizeService Unit Tests

### Prompt

```text
Generate xUnit test cases for FlightStatusNormalizeService.

Validate:

- AeroTrack status mappings
- QuickFlight status mappings
- Unknown status handling
- Missing provider data scenarios
- Response model mapping
```

### Intent

Validate normalization and status conversion logic.

### Outcome

Implemented tests covering status mapping and unified response generation.

### Files Impacted

```text
FlightStatus.Tests/Services/FlightStatusNormalizeServiceTests.cs
```

---

## 3.3 Provider Unit Tests

### Prompt

```text
Generate xUnit test cases for AeroTrackProvider and QuickFlightProvider.

Validate:

- JSON file loading
- Flight filtering by flight number
- Flight filtering by date
- Empty result scenarios
- Invalid flight number scenarios
```

### Intent

Verify provider behavior independently from service-layer logic.

### Outcome

Implemented provider-specific tests to validate deterministic JSON-based data retrieval.

### Files Impacted

```text
FlightStatus.Tests/Providers/AeroTrackProviderTests.cs
FlightStatus.Tests/Providers/QuickFlightProviderTests.cs
```

---

# Summary

AI assistance was used throughout development to accelerate:

* Response model creation
* Provider abstraction implementation
* Sample data generation
* Status mapping design
* Service layer implementation
* Angular UI development
* API integration
* CORS configuration
* Refactoring activities
* xUnit test generation

All generated code and suggestions were reviewed, validated, and adapted before being incorporated into the final solution.
