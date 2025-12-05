# DigitalPrizes

A full-stack digital prizes management application with a React TypeScript frontend and C# ASP.NET Core Web API backend.

## Quick Start

### Prerequisites

- Node.js 20+ (LTS recommended)
- .NET 8 SDK
- VS Code with recommended extensions

### Setup

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd DigitalPrizes
   ```

2. **Open in VS Code**

   - Accept the recommended extensions prompt
   - VS Code settings will auto-configure formatting and linting

3. **Frontend Setup**

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

   Frontend runs at: http://localhost:5173

4. **Backend Setup**
   ```bash
   cd backend/DigitalPrizes.Api
   dotnet restore
   dotnet run
   ```
   API runs at: http://localhost:5000
   Swagger UI: http://localhost:5000/swagger

## Project Structure

```
DigitalPrizes/
├── frontend/                 # React TypeScript application
├── backend/
│   └── DigitalPrizes.Api/    # ASP.NET Core Web API
├── .vscode/                  # VS Code workspace settings
├── .github/                  # GitHub Actions workflows
├── PROJECT-GUIDELINES.md     # Project guidelines
├── FRONTEND-STANDARDS.md     # Frontend coding standards
├── BACKEND-STANDARDS.md      # Backend coding standards
└── .editorconfig             # Editor formatting rules
```

## Standards & Tooling

### Frontend

- **Formatter**: Prettier
- **Linter**: ESLint
- **Test Runner**: Vitest
- **Pre-commit**: Husky + lint-staged

### Backend

- **Analyzers**: .NET Analyzers + StyleCop
- **Formatter**: dotnet format
- **Validation**: FluentValidation

### Shared

- **EditorConfig**: Consistent formatting across editors
- **VS Code Settings**: Shared workspace configuration
- **CI Pipeline**: GitHub Actions

## Development Commands

### Frontend

```bash
npm run dev          # Start development server
npm run build        # Build for production
npm run lint         # Run ESLint
npm run lint:fix     # Fix ESLint issues
npm run format       # Format with Prettier
npm run test         # Run tests
npm run test:coverage # Run tests with coverage
```

### Backend

```bash
dotnet run           # Start the API
dotnet build         # Build the project
dotnet test          # Run tests
dotnet format        # Format code
```

## API Endpoints

| Method | Endpoint         | Description        |
| ------ | ---------------- | ------------------ |
| GET    | /api/prizes      | Get all prizes     |
| GET    | /api/prizes/{id} | Get prize by ID    |
| POST   | /api/prizes      | Create a new prize |
| PUT    | /api/prizes/{id} | Update a prize     |
| DELETE | /api/prizes/{id} | Delete a prize     |

## Documentation

- [Project Guidelines](./PROJECT-GUIDELINES.md)
- [Frontend Standards](./FRONTEND-STANDARDS.md)
- [Backend Standards](./BACKEND-STANDARDS.md)

## License

MIT
