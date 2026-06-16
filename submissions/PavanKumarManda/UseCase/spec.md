# Flight Status System Specification

## 1. Overview

### Purpose

The SkyRoute Flight Status application provides a unified flight status lookup capability for support agents.

A user enters:

* Flight Number
* Flight Date

The system retrieves flight status information from multiple flight data providers, normalizes the responses into a common format, and returns a single unified result.

The application consists of:

* ASP.NET Core Minimal API Backend
* Angular Frontend
* Provider Integration Layer
* Normalization Layer

---

# 2. Assumptions

## Business Assumptions

1. Flight Number is mandatory.
2. Flight Date is mandatory.
3. Providers return deterministic stubbed data.
4. Provider timestamps are represented in UTC.
5. When multiple provider results exist, the most recently updated result is selected.
6. Provider failures should not stop processing when another provider succeeds.
7. If no provider returns usable data, the response status is Unknown.

## Technical Assumptions

1. Backend is implemented using ASP.NET Core Minimal APIs.
2. Angular is used for the frontend application.
3. Dependency Injection is used throughout the application.
4. Providers are executed asynchronously.
5. Logging is enabled for provider failures and processing events.
6. Unit tests validate business rules and normalization logic.

---

# 3. High-Level Architecture

```text
+----------------------+
|     Angular UI       |
+----------+-----------+
           |
           |
           v
+----------------------+
|  Flight Status API   |
+----------+-----------+
           |
           |
           v
+----------------------+
| FlightStatusService  |
+----------+-----------+
           |
    +------+------+
    |             |
    v             v
+--------+    +---------+
|AeroTrack|    |QuickFlight|
|Provider |    | Provider |
+----+----+    +-----+----+
     |                |
     +-------+--------+
             |
             v
+----------------------+
|Normalize Service     |
+----------+-----------+
           |
           v
+----------------------+
| Unified Response     |
+----------------------+
```

---

# 4. Backend Design

## API Endpoint

### Get Flight Status

```http
GET /flights/status?flightNumber={flightNumber}&flightDate={flightDate}
```

### Query Parameters

| Parameter    | Required | Description   |
| ------------ | -------- | ------------- |
| flightNumber | Yes      | Flight Number |
| flightDate   | Yes      | Flight Date   |

### Success Response

```json
{
  "flightNumber": "BA123",
  "status": "Delayed",
  "scheduledDepartureTime": "2025-07-10T10:00:00Z",
  "actualDepartureTime": "2025-07-10T10:45:00Z",
  "terminal": "T2",
  "gate": "A12",
  "delayReason": "Weather",
  "lastUpdatedTimestamp": "2025-07-10T09:50:00Z"
}
```

---

# 5. Service Layer

## IFlightStatusService

```csharp
public interface IFlightStatusService
{
    Task<IEnumerable<FlightStatusResultResponse>> GetAsync(
        string flightNumber,
        DateTime flightDate);
}
```

### Responsibilities

* Execute provider lookups
* Aggregate provider responses
* Handle provider failures
* Select most recent provider result
* Invoke normalization logic
* Return unified response

---

# 6. Provider Layer

## Generic Provider Contract

```csharp
public interface IFlightStatusProvider<T>
{
    Task<IEnumerable<T>> GetAsync(
        string flightNumber,
        DateTime flightDate);
}
```

### Implementations

#### AeroTrackProvider

Provides:

* Status
* Scheduled Departure
* Scheduled Arrival
* Actual Departure
* Actual Arrival
* Terminal
* Gate
* Delay Reason
* Last Updated Timestamp

#### QuickFlightProvider

Provides:

* Status
* Scheduled Departure
* Scheduled Arrival
* Last Updated Timestamp

---

# 7. Normalization Layer

## IFlightStatusNormalizeService

```csharp
public interface IFlightStatusNormalizeService
{
    Task<FlightStatusResultResponse> NormalizeAsync(
        string flightNumber,
        DateTime flightDate,
        AeroTrackResponse? aeroResponse,
        QuickFlightResponse? quickResponse);
}
```

### Responsibilities

* Convert provider-specific statuses
* Apply unified business rules
* Produce a consistent API response
* Return Unknown when data is unavailable

---

# 8. Domain Models

## UnifiedFlightStatus

```csharp
public enum UnifiedFlightStatus
{
    Unknown,
    OnTime,
    Delayed,
    Cancelled,
    Diverted
}
```

### Status Rules

| Status    | Meaning                                           |
| --------- | ------------------------------------------------- |
| OnTime    | Departure or arrival within 15 minutes            |
| Delayed   | Departure or arrival delayed more than 15 minutes |
| Cancelled | Flight cancelled                                  |
| Diverted  | Flight landed at another airport                  |
| Unknown   | No usable provider response                       |

---

## AeroTrackResponse

Contains:

* FlightNumber
* FlightDate
* ProviderStatus
* ScheduledDepartureTime
* ScheduledArrivalTime
* ActualDepartureTime
* ActualArrivalTime
* Terminal
* Gate
* DelayReason
* LastUpdatedTimestamp

---

## QuickFlightResponse

Contains:

* FlightNumber
* FlightDate
* ProviderStatus
* ScheduledDepartureTime
* ScheduledArrivalTime
* LastUpdatedTimestamp

---

## FlightStatusResultResponse

Unified response returned to the frontend.

Properties include:

* FlightNumber
* Status
* ScheduledDepartureTime
* ScheduledArrivalTime
* ActualDepartureTime
* ActualArrivalTime
* Terminal
* Gate
* DelayReason
* LastUpdatedTimestamp
* Message

---

# 9. Status Normalization Rules

## AeroTrack Status Mapping

| Provider Status | Unified Status |
| --------------- | -------------- |
| ON_TIME         | OnTime         |
| LATE            | Delayed        |
| CANCELLED       | Cancelled      |
| DIVERTED        | Diverted       |
| Unknown Value   | Unknown        |

---

## QuickFlight Status Mapping

| Provider Status | Unified Status |
| --------------- | -------------- |
| ON_SCHEDULE     | OnTime         |
| DELAYED         | Delayed        |
| CANCELED        | Cancelled      |
| REROUTED        | Diverted       |
| Unknown Value   | Unknown        |

---

# 10. Provider Selection Logic

The system queries all configured providers.

Selection Rules:

1. Query AeroTrack.
2. Query QuickFlight.
3. Ignore failed provider responses.
4. Ignore null responses.
5. Select latest record from each provider.
6. Compare LastUpdatedTimestamp.
7. Choose the most recently updated result.
8. Normalize selected data.
9. Return Unknown when no data exists.

### Processing Flow

```text
Request
   |
   v
Call Providers
   |
   v
Collect Responses
   |
   +--> No Valid Response
   |         |
   |         v
   |      Unknown
   |
   v
Compare LastUpdatedTimestamp
   |
   v
Select Latest Response
   |
   v
Normalize
   |
   v
Return Result
```

---

# 11. Frontend Design

## Technology Stack

* Angular
* TypeScript
* Reactive Forms
* Angular HTTP Client

---

## UI Components

### Flight Status Search

Responsibilities:

* Capture Flight Number
* Capture Flight Date
* Validate inputs
* Invoke backend API

Inputs:

* Flight Number
* Flight Date

Action:

* Search Button

---

### Flight Status Result View

Displays:

* Flight Number
* Status
* Scheduled Departure
* Scheduled Arrival
* Actual Departure
* Actual Arrival
* Terminal
* Gate
* Delay Reason
* Last Updated Timestamp

---

# 12. Frontend Models

## FlightStatusRequest

```typescript
export interface FlightStatusRequest {
  flightNumber: string;
  flightDate: string;
}
```

---

## FlightStatusResultResponse

```typescript
export interface FlightStatusResultResponse {
  flightNumber: string;
  status: string;
  scheduledDepartureTime?: string;
  scheduledArrivalTime?: string;
  actualDepartureTime?: string;
  actualArrivalTime?: string;
  terminal?: string;
  gate?: string;
  delayReason?: string;
  lastUpdatedTimestamp?: string;
  message?: string;
}
```

---

# 13. UI Workflow

```text
User Opens Application
          |
          v
Enter Flight Number
          |
          v
Select Flight Date
          |
          v
Click Search
          |
          v
Invoke Backend API
          |
          v
Receive Response
          |
          v
Display Result Card
```

---

# 14. UI Display Rules

## Status Colors

| Status    | Color |
| --------- | ----- |
| OnTime    | Green |
| Delayed   | Amber |
| Cancelled | Red   |
| Diverted  | Red   |
| Unknown   | Grey  |

---

## Conditional Fields

The following fields are displayed only when available:

* Terminal
* Gate
* Delay Reason
* Actual Departure Time
* Actual Arrival Time

These fields are typically available only for AeroTrack responses.

---

# 15. Validation Rules

## Backend Validation

| Validation            | Result          |
| --------------------- | --------------- |
| Missing Flight Number | 400 Bad Request |
| Missing Flight Date   | 400 Bad Request |

---

## Frontend Validation

### Flight Number

* Required

Message:

```text
Flight Number is required.
```

### Flight Date

* Required

Message:

```text
Flight Date is required.
```

---

# 16. Error Handling

## Provider Failure

* Log provider failure
* Continue processing remaining providers

## No Provider Response

Return:

```json
{
  "status": "Unknown",
  "message": "No flight information available."
}
```

## API Failure

UI displays:

```text
Unable to retrieve flight status. Please try again later.
```

---

# 17. Dependency Injection

Registered Services:

```csharp
IFlightStatusProvider<AeroTrackResponse>
IFlightStatusProvider<QuickFlightResponse>
IFlightStatusService
IFlightStatusNormalizeService
```

All services are resolved through ASP.NET Core Dependency Injection.

---

# 18. Testing Strategy

## Unit Tests

### FlightStatusService

Validate:

* Provider execution
* Provider selection
* Latest timestamp wins
* Failure handling

### FlightStatusNormalizeService

Validate:

* AeroTrack mappings
* QuickFlight mappings
* Unknown status handling

### Providers

Validate:

* Stub responses
* Response filtering
* Expected result structure

---

# 19. Non-Functional Requirements

## Reliability

* Provider failures must not crash the API.
* Partial success is supported.

## Maintainability

* New providers can be added without API changes.
* Provider implementations remain isolated.

## Testability

* Services and providers are mockable.
* Business logic is separated from API endpoints.

## Observability

* Structured logging is enabled.
* Provider failures are traceable.

## Scalability

* Additional providers can be integrated using the existing generic provider contract.
* Normalization logic remains centralized.
