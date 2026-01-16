import { test, expect } from './fixtures/test-fixtures';

test.describe('Accounts (Authenticated)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('can create a new account', async ({ page }) => {
    // Navigate directly to accounts page (auth setup already created a profile)
    await page.goto('/accounts');
    await page.waitForLoadState('networkidle');

    // Should show accounts page - verify the main heading is visible
    await expect(page.getByRole('heading', { name: 'Accounts', level: 1 })).toBeVisible({ timeout: 10000 });

    // Click "Add Account" button (use first one if multiple)
    const addButton = page.getByRole('button', { name: /add account/i }).first();
    await addButton.click();

    // Should navigate to new account form
    await expect(page).toHaveURL(/\/accounts\/new/);

    // Fill in account details
    await page.getByLabel(/account name|name/i).first().fill('E2E Test Account');

    // Bank name might be optional or named differently
    const bankNameField = page.getByLabel(/bank name|institution/i);
    if (await bankNameField.isVisible({ timeout: 2000 }).catch(() => false)) {
      await bankNameField.fill('Test Bank');
    }

    // Currency selection
    const currencyField = page.getByLabel(/currency/i);
    if (await currencyField.isVisible({ timeout: 2000 }).catch(() => false)) {
      await currencyField.selectOption('USD');
    }

    // Submit the form
    await page.getByRole('button', { name: /create|save|submit/i }).click();

    // Should redirect to accounts list
    await expect(page).toHaveURL('/accounts');

    // Should see the new account in the list (use first in case of duplicates from previous runs)
    await expect(page.getByText('E2E Test Account').first()).toBeVisible({ timeout: 10000 });
  });
});
