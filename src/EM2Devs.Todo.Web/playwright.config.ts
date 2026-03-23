import { defineConfig } from '@playwright/test';

export default defineConfig({
	testDir: 'e2e',
	timeout: 30_000,
	retries: 1,
	use: {
		baseURL: 'http://localhost:5173',
		trace: 'on-first-retry'
	},
	webServer: {
		command: 'npm run dev',
		port: 5173,
		timeout: 15_000,
		reuseExistingServer: true,
		env: {
			API_BASE_URL: 'http://localhost:5001'
		}
	}
});
