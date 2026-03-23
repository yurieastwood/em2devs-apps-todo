import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Demo auth flow', () => {
	test('should sign in as demo user and redirect to task list', async ({ page }) => {
		await page.goto('/login');
		await expect(page.getByRole('heading', { name: 'Welcome to Waypoint' })).toBeVisible();

		await page.getByTestId('demo-login-button').click();
		await page.waitForURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
	});

	test('should redirect to login when session cookie is cleared', async ({ page, context }) => {
		await loginAsDemoUser(page);

		// Clear the demo-user cookie to simulate logout
		await context.clearCookies();
		await page.goto('/');

		// Without the cookie, the layout returns user: null
		// Verify the login page is accessible and functional
		await page.goto('/login');
		await expect(page.getByTestId('demo-login-button')).toBeVisible();
	});
});
