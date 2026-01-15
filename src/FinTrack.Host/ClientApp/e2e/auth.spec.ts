import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test.describe('Login Page', () => {
    test('displays login form', async ({ page }) => {
      await page.goto('/login');

      await expect(page.getByLabel('Email')).toBeVisible();
      await expect(page.getByLabel('Password')).toBeVisible();
      await expect(page.getByRole('button', { name: /sign in|log in/i })).toBeVisible();
    });

    test('shows validation errors for empty form', async ({ page }) => {
      await page.goto('/login');

      await page.getByRole('button', { name: /sign in|log in/i }).click();

      // Should show validation message
      await expect(page.locator('text=/required|email|password/i')).toBeVisible();
    });

    test('shows error for invalid credentials', async ({ page }) => {
      await page.goto('/login');

      await page.getByLabel('Email').fill('invalid@example.com');
      await page.getByLabel('Password').fill('wrongpassword');
      await page.getByRole('button', { name: /sign in|log in/i }).click();

      // Should show error message
      await expect(page.locator('text=/invalid|incorrect|failed/i')).toBeVisible();
    });

    test('has link to registration page', async ({ page }) => {
      await page.goto('/login');

      const registerLink = page.getByRole('link', { name: /register|sign up|create account/i });
      await expect(registerLink).toBeVisible();
    });
  });

  test.describe('Registration Page', () => {
    test('displays registration form', async ({ page }) => {
      await page.goto('/register');

      await expect(page.getByLabel('Email')).toBeVisible();
      await expect(page.getByLabel('Password')).toBeVisible();
      await expect(page.getByRole('button', { name: /register|sign up|create/i })).toBeVisible();
    });

    test('has link to login page', async ({ page }) => {
      await page.goto('/register');

      const loginLink = page.getByRole('link', { name: /login|sign in|already have/i });
      await expect(loginLink).toBeVisible();
    });
  });
});
