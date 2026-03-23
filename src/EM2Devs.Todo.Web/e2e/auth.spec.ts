import { test, expect } from '@playwright/test';

test.describe('Demo auth flow', () => {
	test('should sign in as demo user and redirect to task list', async ({ page }) => {
		await page.goto('/login');
		await expect(page.getByRole('heading', { name: 'Welcome to Waypoint' })).toBeVisible();

		await page.getByTestId('demo-login-button').click();
		await page.waitForURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
	});

	test('should logout and redirect to login page', async ({ page }) => {
		// Login first
		await page.goto('/login');
		await page.getByTestId('demo-login-button').click();
		await page.waitForURL('/');

		// Logout
		await page.getByTestId('logout-button').click();
		await page.waitForURL('/login');
		await expect(page.getByTestId('demo-login-button')).toBeVisible();
	});
});
