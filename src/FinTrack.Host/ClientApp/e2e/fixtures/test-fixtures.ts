import { test as base, expect } from '@playwright/test';

/**
 * Test user credentials for E2E tests
 */
export const TEST_USER = {
  email: 'e2e-test@fintrack.local',
  password: 'TestPassword123!',
  displayName: 'E2E Test User',
};

/**
 * Extended test fixture with common utilities
 */
export const test = base.extend<{
  /** Login as the test user */
  loginAsTestUser: () => Promise<void>;
}>({
  loginAsTestUser: async ({ page }, use) => {
    const login = async () => {
      await page.goto('/login');
      await page.getByLabel('Email').fill(TEST_USER.email);
      await page.getByLabel('Password').fill(TEST_USER.password);
      await page.getByRole('button', { name: /sign in|log in/i }).click();
      await expect(page).toHaveURL(/\/(dashboard|profiles)/);
    };
    await use(login);
  },
});

export { expect };
