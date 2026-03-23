import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Navigation', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('should navigate between Tasks and Dashboard pages', async ({ page }) => {
		await expect(page).toHaveURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();

		await page.getByTestId('nav-dashboard').click();
		await expect(page).toHaveURL('/dashboard');
		await expect(page.getByRole('heading', { name: 'Progression Dashboard' })).toBeVisible();

		await page.getByTestId('nav-tasks').click();
		await expect(page).toHaveURL('/');
		await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
	});
});
