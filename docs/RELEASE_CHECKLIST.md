# FinTrack Pre-Release Checklist

Complete this checklist before tagging a new release.

## Code Quality

- [ ] All unit tests pass: `dotnet test`
- [ ] All integration tests pass: `dotnet test`
- [ ] Frontend tests pass: `pnpm test` (in ClientApp)
- [ ] E2E tests pass: `pnpm e2e` (in ClientApp)
- [ ] No TypeScript errors: `pnpm tsc --noEmit` (in ClientApp)
- [ ] No ESLint errors: `pnpm lint` (in ClientApp)
- [ ] Solution builds without warnings: `dotnet build -warnaserror`

## Security

- [ ] No secrets committed to repository
- [ ] `pnpm audit` shows no critical vulnerabilities
- [ ] `dotnet list package --vulnerable` shows no critical vulnerabilities
- [ ] Security headers are configured (CSP, X-Frame-Options, etc.)
- [ ] Authentication endpoints are protected
- [ ] SQL injection prevented (parameterized queries)

## Documentation

- [ ] README.md is up to date
- [ ] ARCHITECTURE.md reflects current structure
- [ ] API documentation (OpenAPI/Swagger) is accessible
- [ ] CHANGELOG.md updated with release notes
- [ ] Any breaking changes documented

## Database

- [ ] All migrations are committed
- [ ] Migrations apply cleanly to fresh database
- [ ] No pending model changes
- [ ] Indexes are defined for common queries

## Performance

- [ ] Frontend bundle size is reasonable (< 500KB gzipped initial)
- [ ] Route-based code splitting is working
- [ ] Database queries are optimized (no N+1)
- [ ] Caching is configured for hot paths

## Infrastructure

- [ ] Docker image builds successfully
- [ ] Docker Compose starts all services
- [ ] Health endpoints respond correctly:
  - [ ] GET /health returns 200
  - [ ] GET /alive returns 200
  - [ ] GET /ready returns 200
- [ ] Environment variables documented in README
- [ ] Production configuration is correct

## Testing

- [ ] Manual testing checklist completed (TESTING_CHECKLIST.md)
- [ ] Tested on latest Chrome/Firefox/Safari
- [ ] Tested responsive design on mobile viewport
- [ ] Tested with screen reader (basic accessibility)

## Release Process

1. [ ] Merge all PRs to main
2. [ ] Pull latest main locally
3. [ ] Run full test suite
4. [ ] Update version in relevant files
5. [ ] Update CHANGELOG.md
6. [ ] Create git tag: `git tag -a v1.0.0 -m "Release v1.0.0"`
7. [ ] Push tag: `git push origin v1.0.0`
8. [ ] Build and push Docker image (if applicable)
9. [ ] Create GitHub release with notes

---

## Sign-off

| Role | Name | Date | Approved |
|------|------|------|----------|
| Developer | | | [ ] |
| Reviewer | | | [ ] |
