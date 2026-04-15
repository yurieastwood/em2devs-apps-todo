import { test, expect } from '@playwright/test';
import { loginAsDemoUser, deleteAllTasks } from './helpers';

test.describe('Task create polish — date + tags + quick-add', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
		await deleteAllTasks(page);
	});

	test('should create a task with scheduled date and tags via structured form', async ({
		page
	}) => {
		const today = new Date();
		const iso = today.toISOString().slice(0, 10);

		await page.getByTestId('task-title-input').fill('ship demo');
		await page.getByTestId('task-scheduled-date-input').fill(iso);
		await page.getByTestId('task-tags-input').fill('work, milestone');
		await page.getByTestId('add-task-button').click();

		// Today view is the default — task should be visible with chips.
		const taskItem = page.getByTestId('task-item').filter({ hasText: 'ship demo' });
		await expect(taskItem).toBeVisible({ timeout: 10_000 });

		const chips = taskItem.getByTestId('task-tag-chip');
		await expect(chips).toHaveCount(2);
		await expect(chips.nth(0)).toHaveText('#work');
		await expect(chips.nth(1)).toHaveText('#milestone');
	});

	test('should create a task via the quick-add bar', async ({ page }) => {
		await page.getByTestId('toggle-quick-add').click();
		await page.getByTestId('quick-add-input').fill('pay rent #personal !High ^tomorrow');
		await page.getByTestId('quick-add-button').click();

		// Switch to Upcoming view and verify the task is there.
		await page.getByTestId('view-tab-upcoming').click();
		const taskItem = page.getByTestId('task-item').filter({ hasText: 'pay rent' });
		await expect(taskItem).toBeVisible({ timeout: 10_000 });
		await expect(taskItem.getByTestId('task-tag-chip')).toHaveText('#personal');
	});
});
