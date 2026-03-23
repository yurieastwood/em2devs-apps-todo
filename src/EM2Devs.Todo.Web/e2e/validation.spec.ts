import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Validation error flow', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('should disable add button when title is empty', async ({ page }) => {
		const addButton = page.getByTestId('add-task-button');
		await expect(addButton).toBeDisabled();
	});
});
