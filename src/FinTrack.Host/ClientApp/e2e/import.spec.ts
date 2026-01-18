import { test, expect } from '@playwright/test';

const CSV_CONTENT = `date,description,amount
2024-01-01,Grocery Store,-45.32
2024-01-02,Salary,2500.00
`;

test.describe('Import Flow', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('shows import page with account selection', async ({ page }) => {
    await page.goto('/import');

    // Wait for the import page to load
    await expect(page.getByText(/select account/i)).toBeVisible({ timeout: 10000 });

    // Either accounts exist or a "no accounts" message is shown
    await expect.poll(async () => {
      const hasAccount = await page.locator('.p-4.border.rounded-lg').first().isVisible().catch(() => false);
      const hasEmptyState = await page.getByText(/no accounts found/i).isVisible().catch(() => false);
      return hasAccount || hasEmptyState;
    }, { timeout: 15000 }).toBe(true);
  });
});
