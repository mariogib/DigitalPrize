# Backend Standards (C# / ASP.NET Core)

## Overview

This document defines coding standards for the DigitalPrizes C# Web API backend.

## Folder Structure

```
backend/DigitalPrizes.Api/
├── Controllers/              # API endpoints
├── Models/
│   ├── Domain/               # Domain entities
│   ├── Dtos/                 # Data transfer objects
│   └── Requests/             # Request models
├── Services/
│   ├── Interfaces/           # Service contracts
│   └── Implementations/      # Service implementations
├── Repositories/
│   ├── Interfaces/           # Repository contracts
│   └── Implementations/      # Repository implementations
├── Middleware/               # Custom middleware
├── Extensions/               # Extension methods
├── Configuration/            # App configuration classes
├── Validators/               # FluentValidation validators
├── Filters/                  # Action/Exception filters
└── Data/                     # EF Core DbContext, migrations
```

## Naming Conventions

### General Naming

| Type           | Convention                   | Example            |
| -------------- | ---------------------------- | ------------------ |
| Classes        | PascalCase                   | `PrizeService`     |
| Interfaces     | PascalCase with `I` prefix   | `IPrizeService`    |
| Methods        | PascalCase                   | `GetPrizeAsync`    |
| Properties     | PascalCase                   | `PrizeName`        |
| Private fields | camelCase with `_` prefix    | `_prizeRepository` |
| Parameters     | camelCase                    | `prizeId`          |
| Constants      | PascalCase                   | `MaxPrizeValue`    |
| DTOs           | PascalCase with `Dto` suffix | `PrizeDto`         |

### Async Methods

Always suffix async methods with `Async`:

```csharp
// ✅ Good
public async Task<Prize> GetPrizeAsync(Guid id)
public async Task CreatePrizeAsync(CreatePrizeDto dto)

// ❌ Bad
public async Task<Prize> GetPrize(Guid id)
public async Task CreatePrize(CreatePrizeDto dto)
```

### File Naming

One type per file, file name matches type name:

- `PrizeController.cs`
- `IPrizeService.cs`
- `PrizeService.cs`
- `PrizeDto.cs`

## Controller Patterns

### Standard Controller Template

```csharp
using Microsoft.AspNetCore.Mvc;

namespace DigitalPrizes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PrizesController : ControllerBase
{
    private readonly IPrizeService _prizeService;
    private readonly ILogger<PrizesController> _logger;

    public PrizesController(
        IPrizeService prizeService,
        ILogger<PrizesController> logger)
    {
        _prizeService = prizeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all prizes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PrizeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrizeDto>>> GetAllAsync()
    {
        var prizes = await _prizeService.GetAllAsync();
        return Ok(prizes);
    }

    /// <summary>
    /// Gets a prize by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PrizeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizeDto>> GetByIdAsync(Guid id)
    {
        var prize = await _prizeService.GetByIdAsync(id);

        if (prize is null)
        {
            return NotFound();
        }

        return Ok(prize);
    }

    /// <summary>
    /// Creates a new prize.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrizeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrizeDto>> CreateAsync(
        [FromBody] CreatePrizeDto createDto)
    {
        var prize = await _prizeService.CreateAsync(createDto);
        return CreatedAtAction(
            nameof(GetByIdAsync),
            new { id = prize.Id },
            prize);
    }

    /// <summary>
    /// Updates an existing prize.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PrizeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrizeDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdatePrizeDto updateDto)
    {
        var prize = await _prizeService.UpdateAsync(id, updateDto);

        if (prize is null)
        {
            return NotFound();
        }

        return Ok(prize);
    }

    /// <summary>
    /// Deletes a prize.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var success = await _prizeService.DeleteAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
```

### Controller Rules

1. **Thin controllers** - Controllers should only handle HTTP concerns
2. **Inject services** - Business logic belongs in services
3. **Use `[ApiController]`** - Enables automatic model validation
4. **Use `[ProducesResponseType]`** - Document all response types
5. **Return `ActionResult<T>`** - For proper response typing
6. **Use route constraints** - e.g., `{id:guid}`

## Service Layer Pattern

### Interface Definition

```csharp
// Services/Interfaces/IPrizeService.cs
namespace DigitalPrizes.Api.Services.Interfaces;

public interface IPrizeService
{
    Task<IEnumerable<PrizeDto>> GetAllAsync();
    Task<PrizeDto?> GetByIdAsync(Guid id);
    Task<PrizeDto> CreateAsync(CreatePrizeDto createDto);
    Task<PrizeDto?> UpdateAsync(Guid id, UpdatePrizeDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
```

### Service Implementation

```csharp
// Services/Implementations/PrizeService.cs
namespace DigitalPrizes.Api.Services.Implementations;

public class PrizeService : IPrizeService
{
    private readonly IPrizeRepository _prizeRepository;
    private readonly ILogger<PrizeService> _logger;

    public PrizeService(
        IPrizeRepository prizeRepository,
        ILogger<PrizeService> logger)
    {
        _prizeRepository = prizeRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PrizeDto>> GetAllAsync()
    {
        var prizes = await _prizeRepository.GetAllAsync();
        return prizes.Select(MapToDto);
    }

    public async Task<PrizeDto?> GetByIdAsync(Guid id)
    {
        var prize = await _prizeRepository.GetByIdAsync(id);
        return prize is null ? null : MapToDto(prize);
    }

    public async Task<PrizeDto> CreateAsync(CreatePrizeDto createDto)
    {
        var prize = new Prize
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            Value = createDto.Value,
            Status = PrizeStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _prizeRepository.AddAsync(prize);
        _logger.LogInformation("Created prize {PrizeId}", prize.Id);

        return MapToDto(prize);
    }

    private static PrizeDto MapToDto(Prize prize)
    {
        return new PrizeDto
        {
            Id = prize.Id,
            Name = prize.Name,
            Description = prize.Description,
            Value = prize.Value,
            Status = prize.Status.ToString(),
            CreatedAt = prize.CreatedAt,
            UpdatedAt = prize.UpdatedAt
        };
    }
}
```

## Model Definitions

### Domain Models

```csharp
// Models/Domain/Prize.cs
namespace DigitalPrizes.Api.Models.Domain;

public class Prize
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public PrizeStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum PrizeStatus
{
    Active,
    Inactive,
    Redeemed
}
```

### DTOs (Data Transfer Objects)

```csharp
// Models/Dtos/PrizeDto.cs
namespace DigitalPrizes.Api.Models.Dtos;

public class PrizeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Models/Dtos/CreatePrizeDto.cs
public class CreatePrizeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

// Models/Dtos/UpdatePrizeDto.cs
public class UpdatePrizeDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Value { get; set; }
}
```

### DTO Rules

1. **Suffix with `Dto`** - e.g., `CreatePrizeDto`, `PrizeDto`
2. **Separate Create/Update DTOs** - Different validation needs
3. **Initialize strings** - Use `string.Empty` to avoid nulls
4. **Flatten for API responses** - Avoid nested objects when possible

## Repository Pattern

### Interface

```csharp
// Repositories/Interfaces/IPrizeRepository.cs
namespace DigitalPrizes.Api.Repositories.Interfaces;

public interface IPrizeRepository
{
    Task<IEnumerable<Prize>> GetAllAsync();
    Task<Prize?> GetByIdAsync(Guid id);
    Task AddAsync(Prize prize);
    Task UpdateAsync(Prize prize);
    Task<bool> DeleteAsync(Guid id);
}
```

### Implementation

```csharp
// Repositories/Implementations/PrizeRepository.cs
namespace DigitalPrizes.Api.Repositories.Implementations;

public class PrizeRepository : IPrizeRepository
{
    private readonly ApplicationDbContext _context;

    public PrizeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Prize>> GetAllAsync()
    {
        return await _context.Prizes.ToListAsync();
    }

    public async Task<Prize?> GetByIdAsync(Guid id)
    {
        return await _context.Prizes.FindAsync(id);
    }

    public async Task AddAsync(Prize prize)
    {
        await _context.Prizes.AddAsync(prize);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Prize prize)
    {
        _context.Prizes.Update(prize);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var prize = await _context.Prizes.FindAsync(id);
        if (prize is null)
        {
            return false;
        }

        _context.Prizes.Remove(prize);
        await _context.SaveChangesAsync();
        return true;
    }
}
```

## Dependency Injection

### Registration

```csharp
// Extensions/ServiceCollectionExtensions.cs
namespace DigitalPrizes.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Services
        services.AddScoped<IPrizeService, PrizeService>();

        // Repositories
        services.AddScoped<IPrizeRepository, PrizeRepository>();

        return services;
    }
}
```

### Usage in Program.cs

```csharp
// Program.cs
builder.Services.AddApplicationServices();
```

## Error Handling

### Global Exception Handler

```csharp
// Middleware/ExceptionMiddleware.cs
namespace DigitalPrizes.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An internal server error occurred.",
            Detail = exception.Message // Remove in production
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

## Validation

### Using FluentValidation

```csharp
// Validators/CreatePrizeDtoValidator.cs
using FluentValidation;

namespace DigitalPrizes.Api.Validators;

public class CreatePrizeDtoValidator : AbstractValidator<CreatePrizeDto>
{
    public CreatePrizeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required and must be 100 characters or less");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must be 500 characters or less");

        RuleFor(x => x.Value)
            .GreaterThan(0)
            .WithMessage("Value must be greater than zero");
    }
}
```

## Testing Expectations

### Test Project Structure

```
backend/DigitalPrizes.Api.Tests/
├── Controllers/              # Controller integration tests
├── Services/                 # Service unit tests
├── Repositories/             # Repository tests
├── Validators/               # Validator tests
└── Fixtures/                 # Test fixtures and helpers
```

### Test Naming Convention

```csharp
[Fact]
public async Task GetByIdAsync_WhenPrizeExists_ReturnsPrize()
{
    // Arrange
    // Act
    // Assert
}

[Fact]
public async Task GetByIdAsync_WhenPrizeNotFound_ReturnsNull()
{
    // Arrange
    // Act
    // Assert
}
```

Pattern: `MethodName_StateUnderTest_ExpectedBehavior`

### What to Test

- ✅ Service business logic
- ✅ Controller response types
- ✅ Validation rules
- ✅ Repository queries
- ✅ Edge cases and error handling

## Patterns to AVOID

```csharp
// ❌ Don't use async void (except event handlers)
public async void ProcessAsync() { }

// ❌ Don't block on async code
var result = GetDataAsync().Result;

// ❌ Don't expose domain entities directly
[HttpGet]
public async Task<ActionResult<Prize>> Get() { }

// ❌ Don't put business logic in controllers
[HttpPost]
public async Task<ActionResult> Create(CreatePrizeDto dto)
{
    // Complex business logic here is wrong!
}

// ❌ Don't use magic strings
if (status == "active") { }
```

## Patterns to PREFER

```csharp
// ✅ Use async/await properly
public async Task ProcessAsync() { }

// ✅ Use DTOs for API responses
[HttpGet]
public async Task<ActionResult<PrizeDto>> Get() { }

// ✅ Use enums instead of magic strings
if (status == PrizeStatus.Active) { }

// ✅ Use pattern matching with is null
if (prize is null) { return NotFound(); }

// ✅ Use file-scoped namespaces
namespace DigitalPrizes.Api.Controllers;

// ✅ Use primary constructors (C# 12+)
public class PrizeService(IPrizeRepository repository) : IPrizeService
{
    // ...
}
```

## Logging

```csharp
// ✅ Use structured logging
_logger.LogInformation(
    "Prize {PrizeId} created by user {UserId}",
    prize.Id,
    userId);

// ✅ Use appropriate log levels
_logger.LogDebug("Processing request...");
_logger.LogInformation("Prize created successfully");
_logger.LogWarning("Prize {PrizeId} not found", id);
_logger.LogError(ex, "Failed to create prize");
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Configuration Classes

```csharp
// Configuration/CorsSettings.cs
namespace DigitalPrizes.Api.Configuration;

public class CorsSettings
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
```

## Formatting & Linting

- **.NET Analyzers** enabled in project file
- **StyleCop Analyzers** for consistent style
- **.editorconfig** defines C# formatting rules
- **`dotnet format`** enforces formatting

## Quick Reference

| Rule          | Standard                     |
| ------------- | ---------------------------- |
| Indentation   | 4 spaces                     |
| Braces        | Allman style (new line)      |
| Line length   | 120 characters               |
| Namespaces    | File-scoped                  |
| Null checking | Pattern matching (`is null`) |
| Async suffix  | Always use `Async`           |
| DTOs          | Suffix with `Dto`            |
| Interfaces    | Prefix with `I`              |
