import { test, expect } from '@playwright/test';
import { DEMO_EMAIL, DEMO_PASSWORD, loginAsDemoUser } from './helpers';

test.describe('Auth flow', () => {
	test('should sign in with demo credentials and redirect to task list', async ({ page }) => {
		await page.goto('/login');
		await expect(page.getByRole('heading', { name: 'Welcome to Waypoint' })).toBeVisible();

		await page.getByTestId('email-input').fill(DEMO_EMAIL);
		await page.getByTestId('password-input').fill(DEMO_PASSWORD);
		await page.getByTestId('login-button').click();
		await page.waitForURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
		await expect(page.getByTestId('nav-tasks')).toBeVisible();
	});

	test('should show error on invalid credentials', async ({ page }) => {
		await page.goto('/login');
		await page.getByTestId('email-input').fill(DEMO_EMAIL);
		await page.getByTestId('password-input').fill('wrong-password');
		await page.getByTestId('login-button').click();
		await expect(page.getByTestId('login-error')).toBeVisible();
	});

	test('should allow re-login after cookies are cleared', async ({ page, context }) => {
		await loginAsDemoUser(page);

		// Clear cookies to simulate session expiry
		await context.clearCookies();

		// Re-login should work
		await page.goto('/login');
		await page.getByTestId('email-input').fill(DEMO_EMAIL);
		await page.getByTestId('password-input').fill(DEMO_PASSWORD);
		await page.getByTestId('login-button').click();
		await page.waitForURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
	});
});
