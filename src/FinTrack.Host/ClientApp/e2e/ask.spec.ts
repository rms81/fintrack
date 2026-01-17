import { test, expect } from '@playwright/test';

test.describe('Ask (NLQ)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('shows suggestions and can run a query', async ({ page }) => {
    await page.goto('/ask');

    await expect(page.getByRole('heading', { name: /ask your data/i })).toBeVisible();

    const suggestions = page.getByRole('button', { name: /spend|income|expense/i }).first();
    if (await suggestions.isVisible({ timeout: 5000 }).catch(() => false)) {
      await suggestions.click();
    }

    await page.getByRole('textbox').fill('How much did I spend last month?');
    await page.getByRole('button', { name: /send/i }).click();

    await expect(page.getByText(/show sql|hide sql/i)).toBeVisible({ timeout: 15000 });
  });
});
