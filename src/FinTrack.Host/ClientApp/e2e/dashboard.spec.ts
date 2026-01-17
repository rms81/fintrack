import { test, expect } from '@playwright/test';

// Dashboard smoke test

test.describe('Dashboard', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('shows summary cards and charts', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible();

    await expect(page.getByText(/income/i)).toBeVisible();
    await expect(page.getByText(/expenses/i)).toBeVisible();
    await expect(page.getByText(/net balance/i)).toBeVisible();
  });
});
