import { test, expect } from '@playwright/test';

test.describe('Ask (NLQ)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('shows page title and input form', async ({ page }) => {
    await page.goto('/ask');

    await expect(page.getByRole('heading', { name: /ask your data/i })).toBeVisible();
    await expect(page.getByRole('textbox')).toBeVisible();
    await expect(page.getByRole('button', { name: /send/i })).toBeVisible();
  });

  test('can type a query and submit button becomes enabled', async ({ page }) => {
    await page.goto('/ask');

    await expect(page.getByRole('heading', { name: /ask your data/i })).toBeVisible();

    // Send button should be disabled initially
    const sendButton = page.getByRole('button', { name: /send/i });
    await expect(sendButton).toBeDisabled();

    // Fill in a question
    await page.getByRole('textbox').fill('How much did I spend last month?');
    
    // Send button should now be enabled
    await expect(sendButton).toBeEnabled();
  });
});
