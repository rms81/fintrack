# FinTrack Manual Testing Checklist

Use this checklist before each release to verify all features work correctly.

## Prerequisites

- [ ] Application is running (`dotnet run` from AppHost or Docker)
- [ ] Database is accessible and migrated
- [ ] Frontend is accessible at http://localhost:5173 (dev) or served by backend (prod)

---

## 1. Authentication

### Registration
- [ ] Can register with valid email, name, and password
- [ ] Registration fails with duplicate email (shows error)
- [ ] Registration fails with invalid email format
- [ ] Registration fails with weak password
- [ ] After registration, user is logged in and redirected to dashboard
- [ ] Default "Personal" profile is created automatically

### Login
- [ ] Can login with valid credentials
- [ ] Login fails with invalid credentials (shows error)
- [ ] "Remember me" persists session
- [ ] Logout clears session and redirects to login

---

## 2. Profile Management

- [ ] Can create a new profile (Personal or Business type)
- [ ] Can rename an existing profile
- [ ] Can delete a profile (with confirmation dialog)
- [ ] Profile switcher in header shows all profiles
- [ ] Switching profiles updates dashboard and all data views
- [ ] Default categories are seeded for new profiles

---

## 3. Account Management

- [ ] Can create a new account with name, bank name, and currency
- [ ] Can edit account details
- [ ] Can delete an account (with confirmation dialog)
- [ ] Accounts list shows all accounts for current profile
- [ ] Empty state shown when no accounts exist

---

## 4. Transactions

### Viewing
- [ ] Transactions list displays with pagination
- [ ] Can search transactions by description
- [ ] Can filter by account
- [ ] Can filter by category
- [ ] Can filter by date range
- [ ] Can filter by amount range
- [ ] Can filter uncategorized only
- [ ] Clear filters button works
- [ ] Pagination controls work correctly

### Editing
- [ ] Can edit transaction category inline
- [ ] Can delete a transaction (with confirmation dialog)
- [ ] Category changes persist after refresh

---

## 5. Import

### CSV Upload
- [ ] Can upload CSV file via drag-and-drop
- [ ] Can upload CSV file via file picker
- [ ] Shows error for invalid file types
- [ ] Shows preview of detected transactions

### Format Detection
- [ ] Auto-detects common CSV formats
- [ ] Can select from saved import formats
- [ ] Can adjust column mappings manually
- [ ] Can save new import format for reuse

### Import Confirmation
- [ ] Preview shows transactions to be imported
- [ ] Shows duplicate detection (skipped transactions)
- [ ] Import summary shows new/duplicate/error counts
- [ ] Imported transactions appear in transactions list
- [ ] Rules are applied to newly imported transactions

---

## 6. Categories

- [ ] Default categories exist (Food, Transport, Shopping, etc.)
- [ ] Can view all categories for current profile
- [ ] Categories display with icons/colors in transaction list
- [ ] Uncategorized transactions show "Uncategorized" label

---

## 7. Rules

### TOML Editor
- [ ] Can view existing rules in TOML format
- [ ] Syntax highlighting works in editor
- [ ] Can edit and save rules
- [ ] Validation errors shown for invalid TOML syntax

### Rule Application
- [ ] "Apply Rules" button categorizes uncategorized transactions
- [ ] Rules match in priority order
- [ ] Manual overrides are preserved (not re-categorized)
- [ ] Rule templates can be inserted

---

## 8. Dashboard

### Summary Cards
- [ ] Total income displays correctly
- [ ] Total expenses displays correctly
- [ ] Net balance displays correctly
- [ ] Transaction count is accurate

### Charts
- [ ] Spending by category pie chart renders
- [ ] Spending over time area chart renders
- [ ] Charts update when date filters change

### Recent Activity
- [ ] Recent transactions list shows latest 5 transactions
- [ ] Top merchants list shows correct merchants

---

## 9. Natural Language Queries (Ask)

- [ ] Can enter a question in natural language
- [ ] Query suggestions are clickable
- [ ] Results display in appropriate format (table/scalar)
- [ ] "Show SQL" toggle reveals generated SQL
- [ ] Invalid questions show helpful error message
- [ ] Works without LLM configured (stub mode)

---

## 10. Export/Import

### Export
- [ ] Can export transactions to CSV
- [ ] Can export transactions to JSON
- [ ] Export respects current filters
- [ ] Downloaded file contains correct data

### Import
- [ ] Can import JSON backup file
- [ ] Import preview shows what will be imported
- [ ] Imported data appears correctly

---

## 11. Settings

- [ ] Dark mode toggle works
- [ ] Theme preference persists across sessions
- [ ] System preference detection works

---

## 12. Error Handling

- [ ] API errors show toast notifications
- [ ] Network errors show appropriate message
- [ ] 404 pages show helpful message
- [ ] Error boundary catches React errors

---

## 13. Accessibility

- [ ] Skip link visible on keyboard focus
- [ ] All interactive elements are keyboard accessible
- [ ] Tab order is logical
- [ ] Focus indicators are visible
- [ ] Screen reader announces important changes

---

## 14. Performance

- [ ] Initial page load is under 3 seconds
- [ ] Dashboard charts render smoothly
- [ ] Large transaction lists paginate without lag
- [ ] No visible layout shifts during loading

---

## 15. Health Endpoints

- [ ] GET /health returns 200 OK
- [ ] GET /alive returns 200 OK
- [ ] GET /ready returns 200 OK (database connected)
- [ ] X-Correlation-ID header present in responses

---

## Sign-off

| Tester | Date | Environment | Status |
|--------|------|-------------|--------|
| | | | |

**Notes:**
