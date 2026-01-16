import { test as setup, expect } from '@playwright/test';
import { TEST_USER } from './fixtures/test-fixtures';

const authFile = 'e2e/.auth/user.json';

/**
 * Global setup: Create test user, authenticate, and create initial profile
 * This runs once before all tests and saves auth state
 */
setup('authenticate', async ({ page, request }) => {
  // Try to register the test user (ignore if already exists)
  const registerResponse = await request.post('/api/auth/register', {
    data: {
      email: TEST_USER.email,
      password: TEST_USER.password,
      displayName: TEST_USER.displayName,
    },
  });

  // 200/201 = created, 400/409 = already exists (both are fine)
  if (registerResponse.status() !== 200 && registerResponse.status() !== 201 && registerResponse.status() !== 400 && registerResponse.status() !== 409) {
    console.warn(`Registration returned unexpected status: ${registerResponse.status()}`);
  }

  // Navigate to login page
  await page.goto('/login');

  // Fill in credentials
  await page.getByLabel('Email').fill(TEST_USER.email);
  await page.getByLabel('Password').fill(TEST_USER.password);

  // Click login button
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  // Wait for successful navigation (may go to profiles/new if no profile exists)
  await expect(page).toHaveURL(/\/(dashboard|profiles|$)/);

  // Check if we need to create a profile
  const welcomeHeading = page.getByRole('heading', { name: /welcome to fintrack/i });
  if (await welcomeHeading.isVisible({ timeout: 3000 }).catch(() => false)) {
    // We're on the welcome/create profile page, create a profile
    const profileNameInput = page.getByLabel(/profile name/i);
    const createButton = page.getByRole('button', { name: /create profile/i });

    // Wait for the form to be interactive
    await expect(profileNameInput).toBeEnabled({ timeout: 5000 });
    await expect(createButton).toBeEnabled({ timeout: 5000 });

    // Fill the form
    await profileNameInput.fill('Test Profile');

    // Wait a bit for React to process the input
    await page.waitForTimeout(500);

    // Click the button and wait for network request
    const [response] = await Promise.all([
      page.waitForResponse(response =>
        response.url().includes('/api/profiles') && response.request().method() === 'POST'
      ),
      createButton.click(),
    ]);

    // Verify profile creation succeeded
    if (!response.ok()) {
      throw new Error(`Profile creation failed with status ${response.status()}`);
    }

    // Wait for navigation away from the welcome page
    await expect(welcomeHeading).not.toBeVisible({ timeout: 10000 });
  }

  // Ensure we have a profile - get existing profiles and set one as active if needed
  await page.goto('/profiles');
  await page.waitForLoadState('networkidle');

  // Get the profiles from the API and set the first one as active
  const profileCard = page.locator('[class*="card"]').filter({ hasText: /profile/i }).first();
  if (await profileCard.isVisible({ timeout: 5000 }).catch(() => false)) {
    // Get profile ID from the card if possible, or use API
    const context = page.context();

    // Get current localStorage to check if activeProfileId is already set
    const localStorage = await page.evaluate(() => {
      return window.localStorage.getItem('fintrack-active-profile');
    });

    if (!localStorage) {
      // Fetch profiles via API to get the profile ID
      const cookies = await context.cookies();
      const profilesResponse = await page.request.get('/api/profiles');
      if (profilesResponse.ok()) {
        const profiles = await profilesResponse.json();
        if (profiles && profiles.length > 0) {
          // Set the first profile as active
          await page.evaluate((profileId: string) => {
            window.localStorage.setItem('fintrack-active-profile', profileId);
          }, profiles[0].id);
        }
      }
    }
  }

  // Save authentication state (includes localStorage)
  await page.context().storageState({ path: authFile });
});
