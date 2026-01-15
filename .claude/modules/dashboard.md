# Dashboard Module

> **Phase:** 3 (Dashboard & Visualizations)
> **Status:** Planned

## Overview
Interactive dashboards showing spending patterns, trends, and analytics.

## User Stories

| ID | As a... | I want to... | So that... |
|----|---------|--------------|------------|
| US-D1 | User | see spending breakdown by category | I understand where my money goes |
| US-D2 | User | view monthly income vs expenses trend | I can track my financial health over time |
| US-D3 | User | see daily spending for the current month | I can monitor day-to-day spending patterns |
| US-D4 | User | view my top merchants by spend | I know which businesses I frequent most |
| US-D5 | User | see account balances at a glance | I have a quick overview of my finances |
| US-D6 | User | compare spending to previous periods | I can identify changes in habits |
| US-D7 | User | drill down into category details | I can explore specific spending areas |
| US-D8 | User | see count of uncategorized transactions | I know how much cleanup work remains |

## Dashboard Components

### Spending by Category (Pie/Donut Chart)
- Current month breakdown
- Drill-down into subcategories
- Compare to previous period

### Monthly Trend (Bar Chart)
- Income vs Expenses
- 12-month rolling view
- Stacked by category

### Daily Spending (Area Chart)
- Current month daily spending
- Running balance line
- Highlight unusual days

### Top Merchants (Table)
- Most frequent transaction descriptions
- Total spent per merchant
- Category breakdown

### Account Summary (Cards)
- Current balance per account
- Month-to-date spending
- Pending categorization count

## Aggregation Queries

<!-- TODO: Phase 3 - Implement efficient aggregations -->

```csharp
public record SpendingByCategory(
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Amount,
    int TransactionCount
);

public record MonthlyTrend(
    int Year,
    int Month,
    decimal Income,
    decimal Expenses,
    decimal Net
);
```

## Endpoints

<!-- TODO: Phase 3 -->

### GET /api/profiles/{profileId}/dashboard/summary
Overall dashboard summary.

### GET /api/profiles/{profileId}/dashboard/spending-by-category
Category breakdown for period.

### GET /api/profiles/{profileId}/dashboard/monthly-trend
Monthly income/expense trend.

### GET /api/profiles/{profileId}/dashboard/daily-spending
Daily spending for current month.

## React Components

<!-- TODO: Phase 3 -->

Using Recharts:
- `SpendingPieChart`
- `MonthlyTrendChart`
- `DailySpendingChart`
- `DashboardSummaryCards`

## Caching Strategy

<!-- TODO: Phase 3 -->

Consider caching aggregations:
- Daily recalculation for historical data
- Real-time for current month
- Invalidate on transaction changes
