# Phase 3: Dashboard

## Duration: Week 5-6

## Overview
Build the transaction views, dashboard visualizations, and manual categorization features.

## Goals
1. Create transaction list with filters
2. Build dashboard with spending charts
3. Implement manual categorization
4. Add account filtering
5. Create date range picker

---

## Task 3.1: Transaction List Query

### Prompt
```
Create the transaction list query with filters and pagination:

Query: GetTransactions
Parameters:
- ProfileId (required)
- AccountIds (optional, filter by accounts)
- CategoryIds (optional, filter by categories)
- FromDate, ToDate (optional date range)
- Search (optional, fuzzy search on description)
- Tags (optional, array containment)
- IsUncategorized (optional, filter uncategorized)
- Page, PageSize (pagination)
- SortBy (date, amount, description)
- SortOrder (asc, desc)

Response: PagedResult<TransactionDto>
- Items[]
- TotalCount
- Page
- PageSize
- TotalPages

Endpoint:
GET /api/profiles/{profileId}/transactions

Use PostgreSQL features:
- pg_trgm for fuzzy search
- Array operators for tags
- Efficient pagination with keyset where possible
```

### Expected Output
- GetTransactions query and handler
- Filtering logic
- Pagination implementation
- API endpoint

---

## Task 3.2: Transaction Detail

### Prompt
```
Create transaction detail and update endpoints:

Query: GetTransaction
- Get single transaction by ID
- Include category details
- Include matched rule info

Command: UpdateTransaction
- Update CategoryId
- Update Tags
- Set IsManualOverride = true when manually changed
- Option to create rule from this categorization

Endpoints:
- GET /api/profiles/{profileId}/transactions/{id}
- PUT /api/profiles/{profileId}/transactions/{id}
- POST /api/profiles/{profileId}/transactions/{id}/create-rule

The create-rule endpoint should:
- Generate a rule name from description
- Create normalized() match expression
- Add to rules TOML
- Return the created rule
```

### Expected Output
- Transaction detail query
- Update command
- Create rule from transaction

---

## Task 3.3: Bulk Operations

### Prompt
```
Implement bulk transaction operations:

Command: BulkCategorize
- Input: { transactionIds[], categoryId, tags[]? }
- Update all specified transactions
- Set IsManualOverride = true

Command: BulkDelete
- Input: { transactionIds[] }
- Delete transactions
- Return count deleted

Endpoints:
- POST /api/profiles/{profileId}/transactions/bulk-categorize
- POST /api/profiles/{profileId}/transactions/bulk-delete

Include authorization to ensure transactions belong to user's profile.
```

### Expected Output
- Bulk categorize command
- Bulk delete command
- Endpoints with authorization

---

## Task 3.4: Dashboard Summary Query

### Prompt
```
Create dashboard summary query:

Query: GetDashboardSummary
Parameters:
- ProfileId
- AccountIds (optional)
- FromDate, ToDate

Response: DashboardSummaryDto
- TotalIncome (sum of positive amounts)
- TotalExpenses (sum of negative amounts)
- NetBalance
- TransactionCount
- TopCategory (highest spending category)
- ComparedToPreviousPeriod (percentage change)

Endpoint:
GET /api/profiles/{profileId}/dashboard/summary

Calculate previous period based on same duration:
- If date range is 30 days, compare to previous 30 days
- Show percentage change in expenses
```

### Expected Output
- Dashboard summary query
- Period comparison logic
- API endpoint

---

## Task 3.5: Spending by Category Query

### Prompt
```
Create spending by category aggregation:

Query: GetSpendingByCategory
Parameters:
- ProfileId
- AccountIds (optional)
- FromDate, ToDate
- IncludeSubcategories (bool)

Response: CategorySpendingDto[]
- CategoryId
- CategoryName
- CategoryColor
- Amount (absolute value)
- Percentage (of total)
- TransactionCount
- Subcategories[] (if IncludeSubcategories)

Endpoint:
GET /api/profiles/{profileId}/dashboard/spending-by-category

Only include expenses (negative amounts).
Sort by amount descending.
```

### Expected Output
- Category aggregation query
- Percentage calculation
- Nested subcategories

---

## Task 3.6: Spending Over Time Query

### Prompt
```
Create spending over time aggregation:

Query: GetSpendingOverTime
Parameters:
- ProfileId
- AccountIds (optional)
- FromDate, ToDate
- Granularity (day, week, month)

Response: TimeSeriesDto[]
- Date (period start)
- Income
- Expenses
- Net

Endpoint:
GET /api/profiles/{profileId}/dashboard/spending-over-time

Group by period based on granularity.
Return zero for periods with no transactions.
```

### Expected Output
- Time series aggregation
- Granularity handling
- Gap filling for empty periods

---

## Task 3.7: Top Merchants Query

### Prompt
```
Create top merchants aggregation:

Query: GetTopMerchants
Parameters:
- ProfileId
- AccountIds (optional)
- FromDate, ToDate
- Limit (default 10)

Response: MerchantSpendingDto[]
- Merchant (normalized description)
- Amount (total)
- TransactionCount
- LastTransactionDate
- CategoryName (most common category)

Endpoint:
GET /api/profiles/{profileId}/dashboard/top-merchants

Group by normalized description.
Return top N by total amount.
```

### Expected Output
- Merchant aggregation
- Most common category detection
- API endpoint

---

## Task 3.8: Transaction List UI

### Prompt
```
Create the Transaction List UI:

Page: /transactions

Components:
1. TransactionFilters
   - Date range picker (presets: This Month, Last Month, This Year, Custom)
   - Account multi-select
   - Category multi-select
   - Search input (debounced)
   - Tags filter
   - "Show uncategorized only" toggle

2. TransactionTable
   - Columns: Date, Description, Category, Amount
   - Row click to expand details
   - Checkbox for bulk selection
   - Sort by clicking column headers

3. TransactionRow
   - Category badge with color
   - Tags as small badges
   - Amount colored (green/red)
   - Manual override indicator

4. TransactionDetail
   - Full details in side panel or modal
   - Edit category dropdown
   - Edit tags
   - "Create Rule" button
   - Show matched rule name

5. BulkActions
   - Appears when items selected
   - Categorize selected
   - Delete selected

Features:
- Infinite scroll or pagination
- URL state for filters (shareable links)
- Loading skeletons
```

### Expected Output
- Transaction list page
- Filter components
- Detail panel
- Bulk action bar

---

## Task 3.9: Dashboard UI

### Prompt
```
Create the Dashboard UI:

Page: / (home/dashboard)

Layout:
- Header with profile name and date range picker
- Grid of cards and charts

Components:
1. SummaryCards (grid of 4)
   - Total Income (with comparison)
   - Total Expenses (with comparison)
   - Net Balance
   - Transaction Count

2. SpendingByCategory (Pie/Donut chart)
   - Using Recharts PieChart
   - Legend with category names
   - Click to filter transactions

3. SpendingOverTime (Area/Line chart)
   - Using Recharts AreaChart
   - Toggle income/expenses/net
   - Granularity selector (day/week/month)

4. TopMerchants (Bar chart or list)
   - Horizontal bars
   - Show amount and count

5. RecentTransactions
   - Last 5-10 transactions
   - Quick link to full list

6. AccountBalances (if multiple accounts)
   - Card per account
   - Show calculated balance

Features:
- Responsive grid layout
- Loading states for each card
- Date range affects all components
- Account filter in header
```

### Expected Output
- Dashboard page layout
- All chart components
- Summary cards
- Filter integration

---

## Task 3.10: Category Management UI

### Prompt
```
Create Category Management UI:

Page: /categories

Components:
1. CategoryTree
   - Hierarchical list
   - Drag and drop reordering
   - Expand/collapse subcategories
   - Show transaction count per category

2. CategoryForm
   - Name input
   - Parent selector
   - Color picker
   - Icon selector (optional, use emoji or Lucide icons)

3. CategoryCard
   - Category color and icon
   - Name and transaction count
   - Edit and delete buttons

Features:
- Create category modal
- Edit inline or in modal
- Confirm before delete
- Show warning if category has transactions
```

### Expected Output
- Category tree view
- CRUD modals
- Reordering

---

## Task 3.11: Date Range Picker

### Prompt
```
Create a reusable Date Range Picker component:

Features:
- Presets: Today, Yesterday, This Week, Last Week, This Month, Last Month, This Year, Last Year, All Time
- Custom range with calendar
- Show selected range as text
- Keyboard navigation

Props:
- value: { from: Date, to: Date }
- onChange: (range) => void
- presets: Preset[] (customizable)

Use:
- Tailwind for styling
- Native date inputs or small calendar library
- Store selection in URL params

Component should be used in:
- Dashboard header
- Transaction list filters
- Export dialogs
```

### Expected Output
- DateRangePicker component
- Presets logic
- URL integration

---

## Completion Criteria

Phase 3 is complete when:
- [ ] Transaction list loads with pagination
- [ ] All filters work correctly
- [ ] Transaction detail shows and edits
- [ ] Bulk operations work
- [ ] Dashboard summary calculates correctly
- [ ] Pie chart shows category breakdown
- [ ] Line chart shows spending over time
- [ ] Top merchants display
- [ ] Date range picker works globally
- [ ] Category management works
- [ ] All components are responsive

## Next Phase
Proceed to Phase 4: NLQ & Export
