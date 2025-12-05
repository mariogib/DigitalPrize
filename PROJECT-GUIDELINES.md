# DigitalPrizes Project Guidelines

## Overview

DigitalPrizes is a full-stack application with:

- **Frontend**: React with TypeScript (Vite)
- **Backend**: C# ASP.NET Core Web API (IIS hosted)

## Repository Structure

```
DigitalPrizes/
├── frontend/                 # React TypeScript application
│   ├── src/
│   │   ├── components/       # Reusable UI components
│   │   ├── pages/            # Page-level components
│   │   ├── hooks/            # Custom React hooks
│   │   ├── services/         # API clients and external services
│   │   ├── types/            # TypeScript type definitions
│   │   ├── utils/            # Utility functions
│   │   ├── context/          # React context providers
│   │   ├── assets/           # Static assets (images, fonts)
│   │   └── styles/           # Global styles
│   ├── public/               # Static public files
│   └── tests/                # Test files
├── backend/
│   └── DigitalPrizes.Api/    # ASP.NET Core Web API
│       ├── Controllers/      # API controllers
│       ├── Models/           # Domain models and DTOs
│       ├── Services/         # Business logic services
│       ├── Repositories/     # Data access layer
│       ├── Middleware/       # Custom middleware
│       └── Configuration/    # App configuration
├── docs/                     # Additional documentation
├── .vscode/                  # VS Code workspace settings
├── .github/                  # GitHub Actions workflows
├── FRONTEND-STANDARDS.md     # Frontend coding standards
├── BACKEND-STANDARDS.md      # Backend coding standards
└── PROJECT-GUIDELINES.md     # This file
```

## Development Environment Setup

### Prerequisites

- Node.js 20+ (LTS recommended)
- .NET 8 SDK
- VS Code with recommended extensions
- Git

### Getting Started

1. Clone the repository
2. Open in VS Code - accept recommended extensions prompt
3. Frontend setup:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
4. Backend setup:
   ```bash
   cd backend/DigitalPrizes.Api
   dotnet restore
   dotnet run
   ```

## Standards Documents

| Document                                         | Purpose                           |
| ------------------------------------------------ | --------------------------------- |
| [FRONTEND-STANDARDS.md](./FRONTEND-STANDARDS.md) | React/TypeScript coding standards |
| [BACKEND-STANDARDS.md](./BACKEND-STANDARDS.md)   | C# API coding standards           |

## Branching Strategy

- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `hotfix/*` - Production hotfixes

## Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

Types:

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:

```
feat(api): add prize redemption endpoint
fix(frontend): resolve date picker timezone issue
docs: update API documentation
```

## Code Review Guidelines

1. All changes require a pull request
2. At least one approval required before merging
3. CI must pass before merging
4. No direct commits to `main` or `develop`

## Testing Requirements

### Frontend

- Unit tests for utilities and hooks
- Component tests for UI components
- Integration tests for critical user flows
- Minimum 80% coverage for new code

### Backend

- Unit tests for services and business logic
- Integration tests for API endpoints
- Minimum 80% coverage for new code

## API Communication

- All API calls go through the `services/` layer
- Use typed request/response objects
- Handle errors consistently with custom error types
- Base URL configured via environment variables

## Security Guidelines

- Never commit secrets or API keys
- Use environment variables for configuration
- Validate all user input on both frontend and backend
- Follow OWASP security best practices

## Questions?

If anything is unclear, ask the team before making assumptions!
