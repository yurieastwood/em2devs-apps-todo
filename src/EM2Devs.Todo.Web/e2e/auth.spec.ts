import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Demo auth flow', () => {
	test('should sign in as demo user and redirect to task list', async ({ page }) => {
		await page.goto('/login');
		await expect(page.getByRole('heading', { name: 'Welcome to Waypoint' })).toBeVisible();

		await page.getByTestId('login-button').click();
		await page.waitForURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
	});

	test('should allow re-login after cookies are cleared', async ({ page, context }) => {
		await loginAsDemoUser(page);

		// Clear cookies to simulate session expiry
		await context.clearCookies();

		// Re-login should work
		await page.goto('/login');
		await expect(page.getByTestId('login-button')).toBeVisible();
		await page.getByTestId('login-button').click();
		await page.waitForURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
	});
});
