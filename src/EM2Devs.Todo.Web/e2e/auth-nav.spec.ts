import { test, expect } from '@playwright/test';
import { loginAsDemoUser, logoutUser } from './helpers';

test.describe('Auth navigation bar', () => {
	test('should show nav bar after login', async ({ page }) => {
		// When
		await loginAsDemoUser(page);

		// Then — nav bar is visible with all links
		await expect(page.getByTestId('nav-tasks')).toBeVisible();
		await expect(page.getByTestId('nav-quests')).toBeVisible();
		await expect(page.getByTestId('nav-dashboard')).toBeVisible();
		await expect(page.getByTestId('user-display-name')).toBeVisible();
		await expect(page.getByTestId('logout-button')).toBeVisible();
	});

	test('should hide nav bar after logout', async ({ page }) => {
		// Given — logged in
		await loginAsDemoUser(page);

		// When
		await logoutUser(page);

		// Then — nav bar is not visible
		await expect(page.getByTestId('nav-tasks')).not.toBeVisible();
		await expect(page.getByTestId('user-display-name')).not.toBeVisible();
		await expect(page.getByTestId('logout-button')).not.toBeVisible();
	});

	test('should persist nav bar after page refresh while logged in', async ({ page }) => {
		// Given — logged in
		await loginAsDemoUser(page);

		// When — refresh the page
		await page.reload();

		// Then — nav bar still shows authenticated state
		await expect(page.getByTestId('nav-tasks')).toBeVisible();
		await expect(page.getByTestId('user-display-name')).toBeVisible();
		await expect(page.getByTestId('logout-button')).toBeVisible();
	});

	test('should revert to unauthenticated state after cookie clearance', async ({
		page,
		context
	}) => {
		// Given — logged in
		await loginAsDemoUser(page);

		// When — clear cookies and navigate
		await context.clearCookies();
		await page.goto('/');

		// Then — redirected to login, nav bar not visible
		await page.waitForURL('/login');
		await expect(page.getByTestId('nav-tasks')).not.toBeVisible();
	});

	test('should display user name in nav bar', async ({ page }) => {
		// When
		await loginAsDemoUser(page);

		// Then — user display name is shown
		const displayName = page.getByTestId('user-display-name');
		await expect(displayName).toBeVisible();
		await expect(displayName).not.toBeEmpty();
	});
});
