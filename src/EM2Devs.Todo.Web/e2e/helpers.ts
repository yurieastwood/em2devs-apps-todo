import { expect, type Page } from '@playwright/test';

export async function loginAsDemoUser(page: Page) {
	await page.goto('/login');
	await page.getByTestId('login-button').click();
	await page.waitForURL('/');
	await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();
}

export async function createTask(page: Page, title: string) {
	await page.getByTestId('task-title-input').fill(title);
	await page.getByTestId('add-task-button').click();
	await expect(page.getByTestId('task-title').filter({ hasText: title })).toBeVisible({
		timeout: 10_000
	});
}

export async function advanceTaskStatus(page: Page, taskTitle: string, expectedStatus: string) {
	const taskItem = page.getByTestId('task-item').filter({ hasText: taskTitle });
	await taskItem.getByTestId('task-advance-button').click();
	await expect(taskItem.getByTestId('task-status')).toHaveText(expectedStatus, {
		timeout: 10_000
	});
}

export async function deleteAllTasks(page: Page) {
	let count = await page.getByTestId('task-delete-button').count();
	while (count > 0) {
		await page.getByTestId('task-delete-button').first().click();
		await expect(page.getByTestId('task-delete-button')).toHaveCount(count - 1, {
			timeout: 10_000
		});
		count--;
	}
}
