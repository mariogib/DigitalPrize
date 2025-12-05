# Digital Prize Management System - Functional Specification

**Version:** 1.0  
**Last Updated:** December 5, 2025

---

## 1. Executive Summary

The Digital Prize Management System is a comprehensive web-based platform for managing promotional competitions, prize pools, prize distribution, and redemption workflows. It provides administrators with tools to create competitions, manage prize inventories, award prizes to participants, and track redemptions, while offering end-users a seamless experience for registering for competitions and redeeming their prizes.

---

## 2. System Overview

### 2.1 Architecture

The system follows a modern client-server architecture:

- **Backend API:** ASP.NET Core 8.0 RESTful API
- **Frontend:** React TypeScript Single Page Application (SPA)
- **Database:** Microsoft SQL Server
- **Authentication:** OAuth2/OpenID Connect (JWT Bearer tokens)

### 2.2 Key Components

| Component       | Technology                    | Purpose                                    |
| --------------- | ----------------------------- | ------------------------------------------ |
| API Server      | .NET 8, Entity Framework Core | Business logic, data access, API endpoints |
| Admin Dashboard | React, TypeScript, Vite       | Administrative interface                   |
| Public Portal   | React, TypeScript             | End-user prize redemption                  |
| Database        | SQL Server                    | Persistent data storage                    |
| SMS Gateway     | Configurable provider         | Notification delivery                      |

---

## 3. Domain Model

### 3.1 Core Entities

#### 3.1.1 External User

Represents end-users (participants/winners) identified primarily by cell phone number.

| Field          | Type      | Description                |
| -------------- | --------- | -------------------------- |
| ExternalUserId | Long      | Primary key                |
| CellNumber     | String    | Unique mobile phone number |
| FirstName      | String?   | User's first name          |
| LastName       | String?   | User's last name           |
| Email          | String?   | Email address              |
| IsActive       | Boolean   | Account active status      |
| CreatedAt      | DateTime  | Account creation timestamp |
| UpdatedAt      | DateTime? | Last update timestamp      |

#### 3.1.2 Competition

Promotional campaigns or contests that users can register for.

| Field          | Type      | Description                   |
| -------------- | --------- | ----------------------------- |
| CompetitionId  | Integer   | Primary key                   |
| Name           | String    | Competition name              |
| Description    | String?   | Detailed description          |
| TermsUrl       | String?   | Link to terms and conditions  |
| StartDate      | DateTime  | Competition start date/time   |
| EndDate        | DateTime  | Competition end date/time     |
| BannerImageUrl | String?   | Marketing banner image        |
| IsActive       | Boolean   | Whether competition is active |
| CreatedAt      | DateTime  | Creation timestamp            |
| UpdatedAt      | DateTime? | Last update timestamp         |

**Derived Properties:**

- `IsCurrentlyActive`: True if active and current date is within start/end range

#### 3.1.3 Registration Field

Custom data fields for competition registration forms.

| Field               | Type    | Description                           |
| ------------------- | ------- | ------------------------------------- |
| RegistrationFieldId | Integer | Primary key                           |
| CompetitionId       | Integer | Parent competition                    |
| FieldName           | String  | Field identifier                      |
| FieldLabel          | String  | Display label                         |
| FieldType           | String  | Input type (Text, Number, Date, etc.) |
| IsRequired          | Boolean | Whether field is mandatory            |
| DisplayOrder        | Integer | Field order in form                   |
| ValidationRegex     | String? | Optional validation pattern           |

#### 3.1.4 Registration

User registration for a competition.

| Field            | Type     | Description                                  |
| ---------------- | -------- | -------------------------------------------- |
| RegistrationId   | Long     | Primary key                                  |
| CompetitionId    | Integer  | Associated competition                       |
| ExternalUserId   | Long?    | Associated user (optional)                   |
| CellNumber       | String   | Participant's cell number                    |
| RegistrationDate | DateTime | When registration occurred                   |
| Source           | String?  | Registration channel (Web, Import, API, SMS) |
| ConsentGiven     | Boolean  | Marketing consent status                     |

#### 3.1.5 Registration Answer

Responses to custom registration fields.

| Field                | Type    | Description           |
| -------------------- | ------- | --------------------- |
| RegistrationAnswerId | Long    | Primary key           |
| RegistrationId       | Long    | Parent registration   |
| RegistrationFieldId  | Integer | Field being answered  |
| Value                | String? | User's response value |

#### 3.1.6 Prize Pool

Logical grouping of prizes for organizational purposes.

| Field       | Type     | Description            |
| ----------- | -------- | ---------------------- |
| PrizePoolId | Integer  | Primary key            |
| Name        | String   | Pool name              |
| Description | String?  | Pool description       |
| IsActive    | Boolean  | Whether pool is active |
| CreatedAt   | DateTime | Creation timestamp     |

#### 3.1.7 Prize Type

Classification of prizes (e.g., Voucher, Cash, Physical Item).

| Field       | Type    | Description      |
| ----------- | ------- | ---------------- |
| PrizeTypeId | Integer | Primary key      |
| Name        | String  | Type name        |
| Description | String? | Type description |

#### 3.1.8 Prize

Individual prize definition with inventory tracking.

| Field             | Type      | Description                 |
| ----------------- | --------- | --------------------------- |
| PrizeId           | Long      | Primary key                 |
| PrizePoolId       | Integer   | Parent prize pool           |
| PrizeTypeId       | Integer   | Prize classification        |
| Name              | String    | Prize name                  |
| Description       | String?   | Detailed description        |
| MonetaryValue     | Decimal?  | Monetary worth              |
| TotalQuantity     | Integer   | Total available             |
| RemainingQuantity | Integer   | Remaining inventory         |
| ExpiryDate        | DateTime? | When prize expires          |
| ImageUrl          | String?   | Prize image                 |
| MetadataJson      | String?   | Additional metadata as JSON |
| IsActive          | Boolean   | Whether prize is active     |
| CreatedAt         | DateTime  | Creation timestamp          |

**Derived Properties:**

- `IsExpired`: True if past expiry date
- `HasAvailableQuantity`: True if RemainingQuantity > 0
- `IsCurrentlyValid`: True if active, has quantity, and not expired

#### 3.1.9 Prize Award

Record of a prize being awarded to a user.

| Field               | Type      | Description                           |
| ------------------- | --------- | ------------------------------------- |
| PrizeAwardId        | Long      | Primary key                           |
| PrizeId             | Long      | Awarded prize                         |
| CompetitionId       | Integer?  | Associated competition (optional)     |
| ExternalUserId      | Long?     | Recipient user                        |
| CellNumber          | String    | Recipient's cell number               |
| AwardedAt           | DateTime  | Award timestamp                       |
| AwardedBySubjectId  | String?   | Admin who awarded                     |
| AwardMethod         | String?   | Manual, Bulk, or Auto                 |
| NotificationChannel | String?   | SMS, WhatsApp, Email                  |
| NotificationStatus  | String?   | Pending, Sent, Failed                 |
| Status              | String    | Awarded, Redeemed, Expired, Cancelled |
| ExpiryDate          | DateTime? | Award expiration                      |
| ExternalReference   | String?   | External system reference             |

**Status Values:**

- `Awarded`: Prize has been assigned but not yet claimed
- `Redeemed`: Prize has been claimed/collected
- `Expired`: Award period has passed
- `Cancelled`: Award has been revoked

**Derived Properties:**

- `IsRedeemable`: True if status is "Awarded" and not expired

#### 3.1.10 Prize Redemption

Record of a prize being redeemed/claimed.

| Field             | Type     | Description                |
| ----------------- | -------- | -------------------------- |
| PrizeRedemptionId | Long     | Primary key                |
| PrizeAwardId      | Long     | Parent award               |
| RedeemedAt        | DateTime | Redemption timestamp       |
| RedeemedChannel   | String?  | WebPortal, Kiosk, API, POS |
| RedeemedFromIp    | String?  | Client IP address          |
| RedemptionStatus  | String?  | Pending, Completed, Failed |
| Notes             | String?  | Additional notes           |

#### 3.1.11 OTP (One-Time Password)

Verification codes for secure operations.

| Field           | Type      | Description                                   |
| --------------- | --------- | --------------------------------------------- |
| OtpId           | Long      | Primary key                                   |
| CellNumber      | String    | Target cell number                            |
| Code            | String    | Generated OTP code                            |
| Purpose         | String    | Redemption, Registration, Login, Verification |
| RelatedEntityId | Long?     | Related entity ID                             |
| CreatedAt       | DateTime  | Creation timestamp                            |
| ExpiresAt       | DateTime  | Expiration timestamp                          |
| IsUsed          | Boolean   | Whether OTP has been used                     |
| UsedAt          | DateTime? | When OTP was used                             |
| AttemptCount    | Integer   | Failed verification attempts                  |
| MaxAttempts     | Integer   | Maximum allowed attempts (default: 3)         |

**Derived Properties:**

- `IsExpired`: True if current time > ExpiresAt
- `IsValid`: True if not used, not expired, and attempts < max

#### 3.1.12 SMS Message

Audit log for sent SMS messages.

| Field             | Type      | Description                      |
| ----------------- | --------- | -------------------------------- |
| SmsMessageId      | Long      | Primary key                      |
| CellNumber        | String    | Recipient number                 |
| MessageText       | String    | Message content                  |
| MessageType       | String    | Message classification           |
| Status            | String    | Pending, Sent, Delivered, Failed |
| SentAt            | DateTime? | Send timestamp                   |
| DeliveredAt       | DateTime? | Delivery confirmation            |
| ErrorMessage      | String?   | Error details if failed          |
| ExternalReference | String?   | Provider reference ID            |
| CreatedAt         | DateTime  | Creation timestamp               |

#### 3.1.13 Audit Log

System activity audit trail.

| Field      | Type     | Description               |
| ---------- | -------- | ------------------------- |
| AuditLogId | Long     | Primary key               |
| Action     | String   | Action performed          |
| EntityType | String   | Affected entity type      |
| EntityId   | String   | Affected entity ID        |
| SubjectId  | String?  | User who performed action |
| Details    | String?  | Additional details        |
| IpAddress  | String?  | Client IP                 |
| Timestamp  | DateTime | When action occurred      |

---

## 4. Functional Requirements

### 4.1 Competition Management

#### FR-COMP-001: Create Competition

**Description:** Administrators can create new competitions with configurable parameters.

**Inputs:**

- Name (required)
- Description
- Start/End dates (required)
- Terms URL
- Banner image URL
- Active status

**Process:**

1. Validate required fields
2. Ensure start date is before end date
3. Create competition record
4. Log audit trail

**Outputs:**

- Created competition with ID
- Success/failure response

#### FR-COMP-002: Configure Registration Fields

**Description:** Define custom form fields for competition registration.

**Field Types Supported:**

- Text
- Number
- Date
- Email
- Phone
- Select (dropdown)
- Checkbox
- TextArea

#### FR-COMP-003: List/Search Competitions

**Description:** View and search competitions with filtering.

**Filters:**

- Status (Active, Completed, etc.)
- Date range
- Search term (name/description)

**Pagination:** Configurable page size with total count

#### FR-COMP-004: Update Competition

**Description:** Modify competition details.

**Constraints:**

- Cannot change dates if registrations exist (configurable)
- Status changes trigger validation

#### FR-COMP-005: Deactivate Competition

**Description:** Mark competition as inactive.

**Effects:**

- Prevents new registrations
- Awards can still be redeemed

### 4.2 Prize Pool & Prize Management

#### FR-POOL-001: Create Prize Pool

**Description:** Create logical groupings for prizes.

#### FR-POOL-002: Manage Prizes

**Description:** CRUD operations for prizes within pools.

**Prize Operations:**

- Create with inventory quantity
- Update details (preserves award history)
- Adjust remaining quantity
- Deactivate (soft delete)

#### FR-POOL-003: Inventory Tracking

**Description:** Track prize availability.

**Metrics:**

- Total quantity
- Remaining quantity
- Awarded quantity (derived)
- Redeemed quantity

### 4.3 Registration Management

#### FR-REG-001: User Registration

**Description:** End-users register for competitions.

**Channels:**

- Web portal
- API
- Bulk import
- SMS

**Process:**

1. Validate competition is active
2. Check for duplicate registration (by cell number)
3. Validate required fields
4. Create/update ExternalUser
5. Store registration and answers
6. Send confirmation (optional)

#### FR-REG-002: Bulk Registration Import

**Description:** Import multiple registrations from file.

**Supported Formats:**

- CSV

**Process:**

1. Parse file
2. Validate each row
3. Process in batches
4. Return summary (success/failure counts)

#### FR-REG-003: View Registrations

**Description:** List and search registrations.

**Filters:**

- Competition
- Date range
- Cell number search

### 4.4 Prize Awarding

#### FR-AWARD-001: Manual Award

**Description:** Administrator awards prize to specific user.

**Inputs:**

- Prize ID
- Cell number
- Competition ID (optional)
- Notification preference

**Process:**

1. Validate prize availability
2. Decrement prize quantity
3. Create award record
4. Send notification (if enabled)
5. Log audit trail

#### FR-AWARD-002: Bulk Award

**Description:** Award prizes to multiple users.

**Inputs:**

- Prize ID
- List of cell numbers
- Competition ID (optional)

**Process:**

- Same as manual but batched
- Return summary with individual results

#### FR-AWARD-003: Award Cancellation

**Description:** Cancel an existing award.

**Constraints:**

- Cannot cancel redeemed awards
- Must provide reason

**Effects:**

- Status set to "Cancelled"
- Prize quantity restored

#### FR-AWARD-004: Extend Award Expiry

**Description:** Extend the expiration date of an award.

### 4.5 Prize Redemption

#### FR-RED-001: Initiate Redemption

**Description:** Start redemption process for a user.

**Process:**

1. User provides cell number
2. System retrieves redeemable prizes
3. User selects prize to redeem
4. OTP sent to cell number

#### FR-RED-002: OTP Verification

**Description:** Verify user identity via OTP.

**Security:**

- OTP expires after configurable time (default: 5 minutes)
- Maximum 3 attempts per OTP
- Rate limiting on OTP requests

#### FR-RED-003: Complete Redemption

**Description:** Finalize prize redemption.

**Process:**

1. Verify OTP
2. Update award status to "Redeemed"
3. Create redemption record
4. Send confirmation SMS
5. Log audit trail

**Outputs:**

- Redemption code
- Confirmation details

#### FR-RED-004: View Redemptions

**Description:** List and search redemptions.

**Filters:**

- Competition
- Prize pool
- Date range
- Status

### 4.6 Notifications

#### FR-NOT-001: SMS Notifications

**Description:** Send SMS messages to users.

**Notification Types:**

- Award notification
- Redemption confirmation
- OTP codes
- Custom messages

#### FR-NOT-002: Resend Notification

**Description:** Retry failed or resend notifications.

### 4.7 Reporting & Analytics

#### FR-RPT-001: Dashboard

**Description:** Overview dashboard with key metrics.

**Metrics:**

- Total competitions (active/total)
- Registration counts
- Prize statistics (awarded/redeemed/pending)
- Recent activity

#### FR-RPT-002: Awards Report

**Description:** Detailed awards reporting.

**Filters:**

- Competition
- Prize pool
- Date range
- Status

**Export:** CSV format

#### FR-RPT-003: Redemption Analytics

**Description:** Redemption trends and statistics.

**Metrics:**

- Redemption rate
- By channel breakdown
- Daily/weekly trends

---

## 5. API Endpoints

### 5.1 Admin API (Authenticated)

| Method | Endpoint                                  | Description               |
| ------ | ----------------------------------------- | ------------------------- |
| GET    | /api/competitions                         | List competitions         |
| POST   | /api/competitions                         | Create competition        |
| GET    | /api/competitions/{id}                    | Get competition details   |
| PUT    | /api/competitions/{id}                    | Update competition        |
| DELETE | /api/competitions/{id}                    | Delete competition        |
| GET    | /api/prizepools                           | List prize pools          |
| POST   | /api/prizepools                           | Create prize pool         |
| GET    | /api/prizepools/{id}                      | Get prize pool details    |
| PUT    | /api/prizepools/{id}                      | Update prize pool         |
| GET    | /api/prizes                               | List prizes               |
| POST   | /api/prizes                               | Create prize              |
| PUT    | /api/prizes/{id}                          | Update prize              |
| GET    | /api/registrations                        | List registrations        |
| POST   | /api/registrations/bulk                   | Bulk import registrations |
| GET    | /api/prizeawards                          | List awards               |
| POST   | /api/prizeawards                          | Award prize               |
| POST   | /api/prizeawards/bulk                     | Bulk award prizes         |
| PUT    | /api/prizeawards/{id}/cancel              | Cancel award              |
| POST   | /api/prizeawards/{id}/resend-notification | Resend notification       |
| PUT    | /api/prizeawards/{id}/extend-expiry       | Extend expiry             |
| GET    | /api/redemptions                          | List redemptions          |
| GET    | /api/reports/dashboard                    | Dashboard data            |
| GET    | /api/reports/awards                       | Awards report             |

### 5.2 Public API

| Method | Endpoint                             | Description                 |
| ------ | ------------------------------------ | --------------------------- |
| POST   | /api/public/redemption/initiate      | Start redemption            |
| POST   | /api/public/redemption/verify-otp    | Verify OTP                  |
| POST   | /api/public/redemption/complete      | Complete redemption         |
| GET    | /api/public/redemption/status/{code} | Get redemption status       |
| POST   | /api/public/registration             | Register for competition    |
| GET    | /api/public/competitions/{id}        | Get public competition info |

---

## 6. Security Requirements

### 6.1 Authentication

- OAuth2/OpenID Connect for admin users
- JWT Bearer tokens for API authentication
- OTP verification for public redemption

### 6.2 Authorization

- Role-based access control (RBAC)
- Resource-level permissions

### 6.3 Data Protection

- All API communication over HTTPS
- Sensitive data encryption at rest
- Cell numbers partially masked in logs
- Audit logging for all operations

### 6.4 Rate Limiting

- OTP requests: Max 3 per cell number per 5 minutes
- API requests: Configurable per-endpoint limits

---

## 7. User Interface

### 7.1 Admin Dashboard

- **Dashboard:** Overview metrics and recent activity
- **Competitions:** List, create, edit, manage registration fields
- **Prize Pools:** Manage pools and prizes
- **Registrations:** View and search registrations
- **Awards:** Award prizes, view award history
- **Redemptions:** View redemption activity

### 7.2 Public Portal

- **Redemption Flow:** Cell number entry → OTP verification → Prize selection → Confirmation

---

## 8. Integration Points

### 8.1 SMS Gateway

- Configurable SMS provider integration
- Support for multiple providers
- Delivery status tracking

### 8.2 Authentication Provider

- OAuth2/OIDC compliant identity provider
- User management external to system

### 8.3 Export/Import

- CSV import for bulk registrations
- CSV export for reports

---

## 9. Non-Functional Requirements

### 9.1 Performance

- API response time < 500ms for standard operations
- Support for concurrent users: 100+ simultaneous

### 9.2 Availability

- Target uptime: 99.5%
- Graceful degradation for SMS failures

### 9.3 Scalability

- Horizontal scaling via containerization
- Database connection pooling

### 9.4 Audit & Compliance

- Complete audit trail for all operations
- Data retention policies (configurable)

---

## 10. Glossary

| Term          | Definition                                     |
| ------------- | ---------------------------------------------- |
| Award         | Instance of a prize being allocated to a user  |
| Competition   | Promotional campaign or contest                |
| External User | End-user/participant identified by cell number |
| OTP           | One-Time Password for verification             |
| Prize         | Rewardable item with inventory                 |
| Prize Pool    | Logical grouping of prizes                     |
| Redemption    | Act of claiming an awarded prize               |
| Registration  | User's enrollment in a competition             |

---

## 11. Revision History

| Version | Date       | Author | Changes               |
| ------- | ---------- | ------ | --------------------- |
| 1.0     | 2025-12-05 | System | Initial specification |
