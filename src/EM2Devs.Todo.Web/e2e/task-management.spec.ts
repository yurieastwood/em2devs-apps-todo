import { test, expect } from '@playwright/test';
import { loginAsDemoUser, createTask, deleteAllTasks } from './helpers';

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

		const taskItem = page.getByTestId('task-item').filter({ hasText: 'Read a book' });
		await taskItem.getByTestId('task-advance-button').click();
		await expect(taskItem.getByTestId('task-status')).toHaveText('InProgress');
	});

	test('should complete a task and change status to Done', async ({ page }) => {
		await createTask(page, 'Write tests');

		const taskItem = page.getByTestId('task-item').filter({ hasText: 'Write tests' });
		// Todo → InProgress
		await taskItem.getByTestId('task-advance-button').click();
		await expect(taskItem.getByTestId('task-status')).toHaveText('InProgress');
		// InProgress → Done
		await taskItem.getByTestId('task-advance-button').click();
		await expect(taskItem.getByTestId('task-status')).toHaveText('Done');
	});

	test('should delete a task and remove it from the list', async ({ page }) => {
		await createTask(page, 'Task to delete');

		const taskItem = page.getByTestId('task-item').filter({ hasText: 'Task to delete' });
		await expect(taskItem).toBeVisible();

		await taskItem.getByTestId('task-delete-button').click();
		await expect(taskItem).not.toBeVisible();
	});
});
