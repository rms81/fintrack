import { test as setup, expect } from '@playwright/test';
import { TEST_USER } from './fixtures/test-fixtures';

const authFile = 'e2e/.auth/user.json';

/**
 * Global setup: Create test user and authenticate
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

  // 200 = created, 400/409 = already exists (both are fine)
  if (registerResponse.status() !== 200 && registerResponse.status() !== 400 && registerResponse.status() !== 409) {
    console.warn(`Registration returned unexpected status: ${registerResponse.status()}`);
  }

  // Navigate to login page
  await page.goto('/login');

  // Fill in credentials
  await page.getByLabel('Email').fill(TEST_USER.email);
  await page.getByLabel('Password').fill(TEST_USER.password);

  // Click login button
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  // Wait for successful navigation
  await expect(page).toHaveURL(/\/(dashboard|profiles|$)/);

  // Save authentication state
  await page.context().storageState({ path: authFile });
});
