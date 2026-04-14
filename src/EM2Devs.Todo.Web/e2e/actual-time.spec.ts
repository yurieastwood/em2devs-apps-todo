import { test, expect } from '@playwright/test';
import { loginAsDemoUser, createTask, advanceTaskStatus, deleteAllTasks } from './helpers';

test.describe('Actual time recording flow', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
		await deleteAllTasks(page);
	});

	test('should record actual time and display variance for a completed task', async ({
		page
	}) => {
		await createTask(page, 'Estimate me');

		// Open detail page and set estimate to 30 minutes.
		await page.getByTestId('task-title').filter({ hasText: 'Estimate me' }).click();
		await expect(page).toHaveURL(/\/tasks\/[0-9a-f-]+$/);
		await page.getByTestId('task-edit-estimated-minutes').fill('30');
		await page.getByTestId('task-edit-save').click();
		await expect(page).toHaveURL('/');

		// Advance to Done.
		await advanceTaskStatus(page, 'Estimate me', 'InProgress');
		await advanceTaskStatus(page, 'Estimate me', 'Done');

		// Open detail page again, record 45 minutes.
		await page.getByTestId('task-title').filter({ hasText: 'Estimate me' }).click();
		await expect(page.getByTestId('record-actual-time-form')).toBeVisible();

		await page.getByTestId('actual-minutes-input').fill('45');
		await page.getByTestId('record-actual-time-submit').click();

		// Variance block appears with +50% variance.
		const variance = page.getByTestId('task-variance');
		await expect(variance).toBeVisible();
		await expect(variance).toContainText('30 min');
		await expect(variance).toContainText('45 min');
		await expect(variance).toContainText('+50%');
	});
});
