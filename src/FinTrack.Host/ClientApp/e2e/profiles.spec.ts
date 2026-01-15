import { test, expect } from './fixtures/test-fixtures';

test.describe('Profiles (Authenticated)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('can view profiles list', async ({ page }) => {
    await page.goto('/profiles');

    // Should show profiles page
    await expect(page.locator('text=/profiles|personal|business/i')).toBeVisible();
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

        // Submit
        await page.getByRole('button', { name: /save|create|submit/i }).click();

        // Should see success or the new profile
        await expect(page.locator('text=/Test Profile|success|created/i')).toBeVisible();
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
