import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright config for running with pre-started servers
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,
  reporter: [['list']],

  use: {
    baseURL: 'http://host.docker.internal:5173',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
      dependencies: ['setup'],
    },
  ],

  timeout: 30 * 1000,
  expect: { timeout: 5 * 1000 },
});
