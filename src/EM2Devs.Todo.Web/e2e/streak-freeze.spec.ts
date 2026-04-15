import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Streak freeze', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('should freeze the streak and persist the banner across reload', async ({ page }) => {
		await page.goto('/dashboard');

		const form = page.getByTestId('freeze-form');
		const banner = page.getByTestId('freeze-banner');

		// Either the user already has a freeze from a previous run, or the form is shown.
		// If the form is shown, submit it.
		if (await form.isVisible().catch(() => false)) {
			await page.getByTestId('freeze-button').click();
			await expect(banner).toBeVisible();
		}

		// Banner must now be visible.
		await expect(banner).toBeVisible();
		await expect(banner).toContainText('Streak frozen');

		// Reload — banner must persist, proving backend persistence.
		await page.reload();
		await expect(page.getByTestId('freeze-banner')).toBeVisible();
		await expect(page.getByTestId('freeze-banner')).toContainText('Streak frozen');
	});
});
