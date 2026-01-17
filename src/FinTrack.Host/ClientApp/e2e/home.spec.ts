import { test, expect } from '@playwright/test';

test.describe('Home Page (Authenticated)', () => {
  test.use({ storageState: 'e2e/.auth/user.json' });

  test('displays the application title', async ({ page }) => {
    await page.goto('/');

    // Check for FinTrack branding in header
    await expect(page.getByRole('banner').getByText('FinTrack')).toBeVisible();
  });

  test('has navigation elements', async ({ page }) => {
    await page.goto('/');
    
    // Wait for loading to finish and layout to render
    await page.waitForLoadState('networkidle');

    // Should have the layout with navigation links
    // Check for dashboard link in navigation
    await expect(page.getByRole('link', { name: 'Dashboard' })).toBeVisible();
  });
});

test.describe('Home Page (Unauthenticated)', () => {
  test('redirects unauthenticated users to login', async ({ browser }, testInfo) => {
    // Create a fresh context without any stored authentication
    const context = await browser.newContext({
      storageState: undefined,
      baseURL: testInfo.project.use.baseURL,
    });
    const page = await context.newPage();
    
    // Visit the protected home route (index /)
    await page.goto('/');

    // Should redirect to login page
    await expect(page).toHaveURL(/\/login/);
    
    await context.close();
  });
});
