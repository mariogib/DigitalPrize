# Frontend Standards (React + TypeScript)

## Overview

This document defines coding standards for the DigitalPrizes React frontend application.

## Folder Structure

```
frontend/src/
├── components/           # Reusable UI components
│   ├── common/           # Generic components (Button, Input, Modal)
│   ├── layout/           # Layout components (Header, Footer, Sidebar)
│   └── features/         # Feature-specific components
├── pages/                # Route-level page components
├── hooks/                # Custom React hooks
├── services/             # API clients and external integrations
│   └── api/              # REST API client modules
├── types/                # TypeScript type definitions
│   ├── models/           # Domain model types
│   └── api/              # API request/response types
├── utils/                # Pure utility functions
├── context/              # React context providers
├── assets/               # Static assets
│   ├── images/
│   └── fonts/
├── styles/               # Global styles and themes
├── constants/            # Application constants
└── config/               # Configuration files
```

## Naming Conventions

### Files and Folders

| Type             | Convention                  | Example              |
| ---------------- | --------------------------- | -------------------- |
| Components       | PascalCase                  | `PrizeCard.tsx`      |
| Hooks            | camelCase with `use` prefix | `usePrizes.ts`       |
| Services         | camelCase                   | `prizeService.ts`    |
| Types/Interfaces | PascalCase                  | `Prize.ts`           |
| Utils            | camelCase                   | `formatDate.ts`      |
| Constants        | SCREAMING_SNAKE_CASE        | `API_ENDPOINTS.ts`   |
| Test files       | Same as source + `.test`    | `PrizeCard.test.tsx` |

### Code Naming

```typescript
// ✅ Good
interface PrizeDto { }           // Interfaces: PascalCase
type PrizeStatus = 'active';     // Types: PascalCase
const MAX_PRIZES = 100;          // Constants: SCREAMING_SNAKE_CASE
const prizeCount = 5;            // Variables: camelCase
function calculateTotal() { }    // Functions: camelCase
const PrizeCard: React.FC = ()   // Components: PascalCase

// ❌ Bad
interface prizeDto { }           // lowercase interface
const prize_count = 5;           // snake_case variable
function CalculateTotal() { }    // PascalCase function
```

## Component Patterns

### Preferred Pattern: Functional Components with TypeScript

```tsx
import React from "react";

interface PrizeCardProps {
  id: string;
  name: string;
  onSelect?: (id: string) => void;
}

export const PrizeCard: React.FC<PrizeCardProps> = ({ id, name, onSelect }) => {
  const handleClick = () => {
    onSelect?.(id);
  };

  return (
    <div className="prize-card" onClick={handleClick}>
      <h3>{name}</h3>
    </div>
  );
};
```

### Component Rules

1. **One component per file** - Keep components focused
2. **Props interface above component** - Named `ComponentNameProps`
3. **Export named exports** - Avoid default exports for better refactoring
4. **Destructure props** - In function signature or at top of function
5. **Keep components small** - If > 150 lines, consider splitting

### Patterns to AVOID

```tsx
// ❌ Don't use class components
class PrizeCard extends React.Component {}

// ❌ Don't use inline object styles excessively
<div style={{ margin: 10, padding: 20 }} />;

// ❌ Don't use index as key (unless list is static)
{
  items.map((item, index) => <Item key={index} />);
}

// ❌ Don't nest ternaries
{
  isLoading ? <Loader /> : hasError ? <Error /> : <Content />;
}
```

### Patterns to PREFER

```tsx
// ✅ Use early returns for cleaner rendering
if (isLoading) return <Loader />;
if (hasError) return <Error />;
return <Content />;

// ✅ Use CSS modules or styled-components
import styles from "./PrizeCard.module.css";
<div className={styles.card} />;

// ✅ Use stable keys
{
  items.map((item) => <Item key={item.id} />);
}
```

## Hooks

### Custom Hook Pattern

```typescript
// hooks/usePrizes.ts
import { useState, useEffect } from "react";
import { prizeService } from "../services/prizeService";
import type { Prize } from "../types/models/Prize";

interface UsePrizesResult {
  prizes: Prize[];
  isLoading: boolean;
  error: Error | null;
  refetch: () => void;
}

export const usePrizes = (): UsePrizesResult => {
  const [prizes, setPrizes] = useState<Prize[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchPrizes = async () => {
    try {
      setIsLoading(true);
      const data = await prizeService.getAll();
      setPrizes(data);
      setError(null);
    } catch (err) {
      setError(err as Error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPrizes();
  }, []);

  return { prizes, isLoading, error, refetch: fetchPrizes };
};
```

### Hook Rules

1. Always prefix with `use`
2. Return typed objects, not arrays (except for simple state)
3. Handle loading and error states
4. Memoize callbacks and values when appropriate

## API Communication

### Service Pattern

```typescript
// services/api/apiClient.ts
const API_BASE_URL = import.meta.env.VITE_API_URL;

interface RequestOptions {
  method: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  headers?: Record<string, string>;
}

export const apiClient = {
  async request<T>(endpoint: string, options: RequestOptions): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: options.method,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
      body: options.body ? JSON.stringify(options.body) : undefined,
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.status}`);
    }

    return response.json();
  },

  get<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: "GET" });
  },

  post<T>(endpoint: string, body: unknown): Promise<T> {
    return this.request<T>(endpoint, { method: "POST", body });
  },

  put<T>(endpoint: string, body: unknown): Promise<T> {
    return this.request<T>(endpoint, { method: "PUT", body });
  },

  delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: "DELETE" });
  },
};
```

### Domain Service

```typescript
// services/prizeService.ts
import { apiClient } from "./api/apiClient";
import type { Prize, CreatePrizeDto } from "../types";

export const prizeService = {
  getAll(): Promise<Prize[]> {
    return apiClient.get<Prize[]>("/api/prizes");
  },

  getById(id: string): Promise<Prize> {
    return apiClient.get<Prize>(`/api/prizes/${id}`);
  },

  create(prize: CreatePrizeDto): Promise<Prize> {
    return apiClient.post<Prize>("/api/prizes", prize);
  },

  update(id: string, prize: Partial<Prize>): Promise<Prize> {
    return apiClient.put<Prize>(`/api/prizes/${id}`, prize);
  },

  delete(id: string): Promise<void> {
    return apiClient.delete<void>(`/api/prizes/${id}`);
  },
};
```

## TypeScript Guidelines

### Type Definitions

```typescript
// types/models/Prize.ts
export interface Prize {
  id: string;
  name: string;
  description: string;
  value: number;
  status: PrizeStatus;
  createdAt: Date;
  updatedAt: Date;
}

export type PrizeStatus = "active" | "inactive" | "redeemed";

// types/api/PrizeDto.ts
export interface CreatePrizeDto {
  name: string;
  description: string;
  value: number;
}

export interface UpdatePrizeDto extends Partial<CreatePrizeDto> {}
```

### Type Rules

1. **Prefer interfaces for objects** - Use `type` for unions/primitives
2. **Suffix DTOs with `Dto`** - e.g., `CreatePrizeDto`
3. **No `any`** - Use `unknown` if truly unknown
4. **Export types explicitly** - From a central `types/index.ts`

## Testing Expectations

### Test Structure

```
tests/
├── components/           # Component tests
├── hooks/                # Hook tests
├── services/             # Service tests
└── utils/                # Utility tests
```

### Test Naming

```typescript
describe("PrizeCard", () => {
  it("should render prize name", () => {});
  it("should call onSelect when clicked", () => {});
  it("should not throw when onSelect is undefined", () => {});
});
```

### What to Test

- ✅ Component rendering with different props
- ✅ User interactions (clicks, inputs)
- ✅ Hook state changes
- ✅ API service methods
- ✅ Utility function edge cases

### Testing Tools

- Vitest for test runner
- React Testing Library for component tests
- MSW for API mocking

## Import Order

```typescript
// 1. React and framework imports
import React, { useState, useEffect } from "react";

// 2. Third-party libraries
import { format } from "date-fns";

// 3. Internal modules (absolute paths)
import { prizeService } from "@/services/prizeService";
import type { Prize } from "@/types";

// 4. Relative imports
import { PrizeItem } from "./PrizeItem";
import styles from "./PrizeList.module.css";
```

## Environment Variables

```bash
# .env.local (not committed)
VITE_API_URL=http://localhost:5000

# .env.example (committed)
VITE_API_URL=
```

Access in code:

```typescript
const apiUrl = import.meta.env.VITE_API_URL;
```

## Formatting & Linting

- **Prettier** handles all formatting (configured in `.prettierrc`)
- **ESLint** handles code quality (configured in `.eslintrc.cjs`)
- **Format on Save** is enabled in VS Code settings
- **Pre-commit hooks** run linting automatically

No debates about formatting - just follow the tools!

## Quick Reference

| Rule            | Standard           |
| --------------- | ------------------ |
| Indentation     | 2 spaces           |
| Quotes          | Single quotes      |
| Semicolons      | Yes                |
| Line length     | 100 characters     |
| Trailing commas | ES5 style          |
| Props           | Destructured       |
| Exports         | Named (no default) |
