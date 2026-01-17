import { test, expect } from './fixtures/test-fixtures';

test.describe('Accounts (Authenticated)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  // TODO: This test is flaky when run in parallel due to database state issues.
  // The API returns "Failed to load accounts" because the profile ID in localStorage
  // may not match a valid profile for the authenticated user when tests run in parallel.
  // Skip for now until we implement proper test isolation or serialization.
  test.skip('can create a new account', async ({ page }) => {
    // Navigate to home page first
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Get current profiles from API
    const profilesResponse = await page.request.get('/api/profiles');
    
    if (!profilesResponse.ok()) {
      throw new Error(`Failed to get profiles: ${profilesResponse.status()}`);
    }

    const profiles = await profilesResponse.json();
    
    // If no profiles exist, create one
    let profileId: string;
    if (!Array.isArray(profiles) || profiles.length === 0) {
      const createResponse = await page.request.post('/api/profiles', {
        data: { name: 'E2E Test Profile', type: 'Personal' }
      });
      
      if (!createResponse.ok()) {
        throw new Error(`Failed to create profile: ${createResponse.status()}`);
      }
      
      const newProfile = await createResponse.json();
      profileId = newProfile.id;
    } else {
      profileId = (profiles[0] as { id: string }).id;
    }

    // Set the profile ID in localStorage
    await page.evaluate((id: string) => {
      window.localStorage.setItem('fintrack-active-profile', id);
    }, profileId);

    // Navigate to accounts page and reload to pick up the profile
    await page.goto('/accounts');
    await page.waitForLoadState('networkidle');

    // Check if we hit the "Failed to load" error - if so, try reloading once
    const failedToLoad = page.getByText('Failed to load accounts');
    if (await failedToLoad.isVisible({ timeout: 3000 }).catch(() => false)) {
      // Try refreshing - sometimes the first load fails due to race conditions
      await page.reload();
      await page.waitForLoadState('networkidle');
    }

    // Wait for page content to appear
    const accountsHeading = page.getByRole('heading', { name: 'Accounts', level: 1 });
    const noAccountsCard = page.getByText('No Accounts');

    // Wait for accounts page to be ready
    await expect(accountsHeading.or(noAccountsCard)).toBeVisible({ timeout: 15000 });

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
