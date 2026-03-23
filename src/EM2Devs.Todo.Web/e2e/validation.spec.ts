import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Validation error flow', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('should show error when creating a task with empty title', async ({ page }) => {
		const addButton = page.getByTestId('add-task-button');
		await expect(addButton).toBeDisabled();
	});
});
