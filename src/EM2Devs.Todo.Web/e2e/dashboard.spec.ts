import { test, expect } from '@playwright/test';
import { loginAsDemoUser } from './helpers';

test.describe('Progression dashboard', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('should display XP, level, and streak on dashboard', async ({ page }) => {
		await page.getByTestId('nav-dashboard').click();
		await expect(page).toHaveURL('/dashboard');

		await expect(page.getByTestId('level-badge')).toBeVisible();
		await expect(page.getByTestId('level-number')).toBeVisible();
		await expect(page.getByTestId('xp-total')).toBeVisible();
		await expect(page.getByTestId('current-streak')).toBeVisible();
		await expect(page.getByTestId('longest-streak')).toBeVisible();
	});

	test('should show increased XP after completing a task', async ({ page }) => {
		// Read initial XP
		await page.getByTestId('nav-dashboard').click();
		const initialXpText = await page.getByTestId('xp-total').textContent();
		const initialXp = parseInt(initialXpText?.replace(/[^0-9]/g, '') ?? '0');

		// Create and complete a task
		await page.getByTestId('nav-tasks').click();
		await page.getByTestId('task-title-input').fill('E2E XP test task');
		await page.getByTestId('add-task-button').click();
		await page.getByTestId('task-title').filter({ hasText: 'E2E XP test task' }).waitFor();

		const taskItem = page.getByTestId('task-item').filter({ hasText: 'E2E XP test task' });
		// Todo → InProgress
		await taskItem.getByTestId('task-advance-button').click();
		await expect(taskItem.getByTestId('task-status')).toHaveText('InProgress');
		// InProgress → Done
		await taskItem.getByTestId('task-advance-button').click();
		await expect(taskItem.getByTestId('task-status')).toHaveText('Done');

		// Check XP increased
		await page.getByTestId('nav-dashboard').click();
		const updatedXpText = await page.getByTestId('xp-total').textContent();
		const updatedXp = parseInt(updatedXpText?.replace(/[^0-9]/g, '') ?? '0');

		expect(updatedXp).toBeGreaterThan(initialXp);

		// Clean up
		await page.getByTestId('nav-tasks').click();
		await taskItem.getByTestId('task-delete-button').click();
	});
});
