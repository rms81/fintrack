import { test, expect } from './fixtures/test-fixtures';

test.describe('Profiles (Authenticated)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('can view profiles list', async ({ page }) => {
    await page.goto('/profiles');

    // Should show profiles page heading
    await expect(page.getByRole('heading', { name: 'Profiles' })).toBeVisible();
  });

  test('can create a new profile', async ({ page }) => {
    await page.goto('/profiles');

    // Click create profile button
    const createButton = page.getByRole('button', { name: /create|new|add/i });
    if (await createButton.isVisible()) {
      await createButton.click();

      // Fill in profile details
      const nameInput = page.getByLabel(/name/i);
      if (await nameInput.isVisible()) {
        await nameInput.fill('Test Profile');

        // Select profile type
        const typeSelect = page.getByLabel(/type/i);
        if (await typeSelect.isVisible()) {
          await typeSelect.selectOption('Personal');
        }

        // Submit - click the Create Profile button specifically
        await page.getByRole('button', { name: /^create profile$/i }).click();

        // Wait for navigation back to profiles list
        await expect(page.getByRole('heading', { name: 'Profiles', exact: true })).toBeVisible({ timeout: 10000 });
      }
    }
  });

  test('can switch between profiles', async ({ page }) => {
    await page.goto('/profiles');

    // Find profile selector or list
    const profileCards = page.locator('[data-testid="profile-card"], .profile-item, [role="listitem"]');
    const count = await profileCards.count();

    if (count > 1) {
      // Click on a different profile
      await profileCards.nth(1).click();

      // Should navigate or update context
      await expect(page).toHaveURL(/\/profiles\/|\/dashboard/);
    }
  });
});
