import { test, expect } from '@playwright/test';

// Dashboard smoke test

test.describe('Dashboard', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('shows summary cards and charts', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible();

    // Use exact match for summary card headings to avoid matching chart titles
    await expect(page.getByRole('heading', { name: 'Income', exact: true })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Expenses', exact: true })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Net Balance', exact: true })).toBeVisible();
  });
});
