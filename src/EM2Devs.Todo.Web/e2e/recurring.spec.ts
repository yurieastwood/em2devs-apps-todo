import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Recurring task UI', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('should navigate to recurring page, create a daily template, and show it in the list', async ({
		page
	}) => {
		await page.getByTestId('nav-recurring').click();
		await expect(page).toHaveURL('/recurring');
		await expect(page.getByRole('heading', { name: 'Recurring Tasks' })).toBeVisible();

		const title = `Daily ritual ${Date.now()}`;
		await page.getByTestId('recurring-title-input').fill(title);
		await page.getByTestId('recurring-pattern-select').selectOption('Daily');
		await page.getByTestId('recurring-create-button').click();

		const item = page.getByTestId('recurring-item').filter({ hasText: title });
		await expect(item).toBeVisible({ timeout: 10_000 });
		await expect(item.getByTestId('recurring-pattern')).toHaveText('Daily');

		// Cleanup so repeated runs stay deterministic.
		await item.getByRole('button', { name: 'Delete' }).click();
		await expect(item).not.toBeVisible({ timeout: 10_000 });
	});
});
