# Phase 5: Polish - Task Tracker

## Overview
Final polish phase focusing on error handling, performance optimization, documentation, and user experience improvements.

## Task Priority Order

| # | Task | Status | Branch | PR |
|---|------|--------|--------|-----|
| 1 | [5.1 Global Error Handling](#51-global-error-handling) | Pending | | |
| 2 | [5.2 Loading States](#52-loading-states) | Pending | | |
| 3 | [5.7 UI Polish](#57-ui-polish) | Pending | | |
| 4 | [5.9 Security Headers](#59-security-review) | Pending | | |
| 5 | [5.8 Testing Coverage](#58-testing-coverage) | Pending | | |
| 6 | [5.3 Database Performance](#53-database-performance) | Pending | | |
| 7 | [5.4 Frontend Performance](#54-frontend-performance) | Pending | | |
| 8 | [5.5 Logging & Monitoring](#55-logging-and-monitoring) | Partial | | |
| 9 | [5.6 Documentation](#56-documentation) | Done | | |
| 10 | [5.10 Final Testing](#510-final-testing) | Pending | | |

---

## 5.1 Global Error Handling

**Status:** Pending

### Backend
- [ ] Create custom exception types:
  - [ ] `NotFoundException`
  - [ ] `ValidationException`
  - [ ] `ConflictException`
- [ ] Implement global exception handler middleware
- [ ] Return Problem Details (RFC 7807) format
- [ ] Log errors with correlation ID

### Frontend
- [ ] Global error boundary component
- [ ] Toast notification system
- [ ] Retry logic for transient failures

---

## 5.2 Loading States

**Status:** Pending

### Components
- [ ] Skeleton loaders for lists
- [ ] Skeleton loaders for cards
- [ ] Skeleton loaders for charts
- [ ] Button loading state improvements

### Optimistic Updates
- [ ] Transaction categorization
- [ ] Profile/Account updates

### Progress Indicators
- [ ] File upload progress
- [ ] Import progress

### Suspense
- [ ] Add React Suspense boundaries
- [ ] Fallback components

---

## 5.7 UI Polish

**Status:** Pending

### Dark Mode
- [ ] Implement dark mode toggle
- [ ] Persist preference to localStorage
- [ ] System preference detection

### Empty States
- [ ] No transactions empty state
- [ ] No accounts empty state
- [ ] No profiles empty state
- [ ] No rules empty state

### Accessibility
- [ ] Keyboard navigation audit
- [ ] Focus indicators
- [ ] Color contrast (WCAG AA)

---

## 5.9 Security Review

**Status:** Pending

### Security Headers
- [ ] Content-Security-Policy
- [ ] X-Content-Type-Options
- [ ] X-Frame-Options
- [ ] Referrer-Policy

### Input Validation
- [ ] Review all endpoints for validation
- [ ] Add FluentValidation or similar

### Audit
- [ ] Run `dotnet audit`
- [ ] Run `pnpm audit`
- [ ] Update vulnerable packages

---

## 5.8 Testing Coverage

**Status:** Pending

### Backend
- [ ] Unit tests for services
- [ ] Integration tests for all endpoints
- [ ] Target: >80% coverage

### Frontend
- [ ] Component tests with Testing Library
- [ ] Hook tests
- [ ] Target: >70% coverage

### E2E
- [ ] Fix flaky accounts test
- [ ] Add import flow test
- [ ] Add dashboard test

---

## 5.3 Database Performance

**Status:** Pending

- [ ] Review and optimize slow queries
- [ ] Implement caching (IMemoryCache)
- [ ] Add compiled queries for hot paths
- [ ] Configure connection pooling
- [ ] Create performance tests

---

## 5.4 Frontend Performance

**Status:** Pending

- [ ] Route-based code splitting (React.lazy)
- [ ] Virtualize long lists (react-virtual)
- [ ] Bundle size analysis
- [ ] Lighthouse audit and improvements

---

## 5.5 Logging and Monitoring

**Status:** Partial (health checks done via Aspire)

- [ ] Add correlation ID middleware
- [ ] Ensure structured logging for key events
- [ ] Add /health/ready endpoint

---

## 5.6 Documentation

**Status:** Done

- [x] README.md
- [x] ARCHITECTURE.md
- [x] OpenAPI/Swagger

---

## 5.10 Final Testing

**Status:** Pending

- [ ] Manual testing checklist
- [ ] Pre-release checklist
- [ ] Release notes (CHANGELOG.md)
- [ ] Docker image verification

---

## Notes

- Each task gets its own feature branch
- Run tests before committing
- Create PR for review before merging
