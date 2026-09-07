import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  retries: process.env.CI ? 1 : 0,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      // Backend .NET único (Fase 8 — Node ya no existe, ver vite.config.ts proxy).
      command: 'dotnet run --no-launch-profile --project backend-dotnet/src/Quincena.Api --urls http://localhost:5080',
      url: 'http://localhost:5080/api/health',
      reuseExistingServer: !process.env.CI,
      timeout: 60_000,
      env: { ASPNETCORE_ENVIRONMENT: 'Development' },
    },
    {
      command: 'npm run dev --prefix frontend',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 30_000,
    },
  ],
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
})
