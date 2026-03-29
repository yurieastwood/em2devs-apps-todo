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
});
