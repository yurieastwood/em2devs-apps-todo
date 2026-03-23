import { type Page } from '@playwright/test';

export async function loginAsDemoUser(page: Page) {
	await page.goto('/login');
	await page.getByTestId('demo-login-button').click();
	await page.waitForURL('/');
}

export async function createTask(page: Page, title: string) {
	await page.getByTestId('task-title-input').fill(title);
	await page.getByTestId('add-task-button').click();
	await page.getByTestId('task-title').filter({ hasText: title }).waitFor();
}

export async function deleteAllTasks(page: Page) {
	while ((await page.getByTestId('task-delete-button').count()) > 0) {
		await page.getByTestId('task-delete-button').first().click();
		await page.waitForTimeout(300);
	}
}
