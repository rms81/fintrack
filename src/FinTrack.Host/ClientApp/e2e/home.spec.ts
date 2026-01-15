import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('displays the application title', async ({ page }) => {
    await page.goto('/');

    // Check for FinTrack branding
    await expect(page.locator('text=FinTrack')).toBeVisible();
  });

  test('has navigation elements', async ({ page }) => {
    await page.goto('/');

    // Should have some form of navigation
    const nav = page.locator('nav, header, [role="navigation"]');
    await expect(nav.first()).toBeVisible();
  });

  test('redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/dashboard');

    // Should redirect to login page
    await expect(page).toHaveURL(/\/login/);
  });
});
