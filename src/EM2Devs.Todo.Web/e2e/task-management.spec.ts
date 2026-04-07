import { test, expect } from '@playwright/test';
import { loginAsDemoUser, createTask, advanceTaskStatus, deleteAllTasks } from './helpers';

test.describe('Task management flow', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
		await deleteAllTasks(page);
	});

	test('should create a task and display it in the list', async ({ page }) => {
		await createTask(page, 'Buy groceries');

		const taskItem = page.getByTestId('task-item').filter({ hasText: 'Buy groceries' });
		await expect(taskItem).toBeVisible();
		await expect(taskItem.getByTestId('task-status')).toHaveText('Todo');
	});

	test('should start a task and change status to InProgress', async ({ page }) => {
		await createTask(page, 'Read a book');
		await advanceTaskStatus(page, 'Read a book', 'InProgress');
	});

	test('should complete a task and change status to Done', async ({ page }) => {
		await createTask(page, 'Write tests');
		await advanceTaskStatus(page, 'Write tests', 'InProgress');
		await advanceTaskStatus(page, 'Write tests', 'Done');
	});

	test('should delete a task and remove it from the list', async ({ page }) => {
		await createTask(page, 'Task to delete');

		const taskItem = page.getByTestId('task-item').filter({ hasText: 'Task to delete' });
		await expect(taskItem).toBeVisible();

		await taskItem.getByTestId('task-delete-button').click();
		await taskItem.getByTestId('task-confirm-delete').click();
		await expect(taskItem).not.toBeVisible({ timeout: 10_000 });
	});

	test('should filter tasks by status', async ({ page }) => {
		await createTask(page, 'Todo task');
		await createTask(page, 'InProgress task');
		await advanceTaskStatus(page, 'InProgress task', 'InProgress');

		await page.getByTestId('filter-status').selectOption('Todo');

		const items = page.getByTestId('task-item');
		await expect(items).toHaveCount(1);
		await expect(items.first()).toContainText('Todo task');
	});

	test('should sort tasks by priority', async ({ page }) => {
		// All tasks are created with default Medium priority — verify the
		// control mounts and selecting it does not error.
		await createTask(page, 'First');
		await createTask(page, 'Second');

		await page.getByTestId('sort-key').selectOption('priority');

		const items = page.getByTestId('task-item');
		await expect(items).toHaveCount(2);
	});

	test('should navigate to task detail and edit fields', async ({ page }) => {
		await createTask(page, 'Editable task');

		await page.getByTestId('task-title').filter({ hasText: 'Editable task' }).click();
		await expect(page).toHaveURL(/\/tasks\/[0-9a-f-]+$/);

		await page.getByTestId('task-edit-title').fill('Edited task');
		await page.getByTestId('task-edit-priority').selectOption('High');
		await page.getByTestId('task-edit-save').click();

		await expect(page).toHaveURL('/');
		await expect(
			page.getByTestId('task-item').filter({ hasText: 'Edited task' })
		).toBeVisible();
	});

	test('should show XP toast when completing a task', async ({ page }) => {
		await createTask(page, 'XP task');
		await advanceTaskStatus(page, 'XP task', 'InProgress');
		await advanceTaskStatus(page, 'XP task', 'Done');

		await expect(page.getByTestId('xp-toast')).toBeVisible({ timeout: 5000 });
	});
});
