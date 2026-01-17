import { test, expect } from '@playwright/test';

const CSV_CONTENT = `date,description,amount
2024-01-01,Grocery Store,-45.32
2024-01-02,Salary,2500.00
`;

test.describe('Import Flow', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('can upload CSV and preview transactions', async ({ page }) => {
    await page.goto('/import');

    // Select the first account card (or create a new one if needed)
    const accountCard = page.locator('button').filter({ hasText: /account/i }).first();
    if (await accountCard.isVisible({ timeout: 5000 }).catch(() => false)) {
      await accountCard.click();
    }

    // Upload CSV via file input
    const fileInput = page.locator('input[type="file"]');
    await fileInput.setInputFiles({
      name: 'transactions.csv',
      mimeType: 'text/csv',
      buffer: Buffer.from(CSV_CONTENT),
    });

    // Should reach preview step
    await expect(page.getByText(/preview import/i)).toBeVisible({ timeout: 10000 });
    await expect(page.getByText(/grocery store/i)).toBeVisible();
    await expect(page.getByText(/salary/i)).toBeVisible();
  });
});
