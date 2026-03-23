import { test, expect } from '@playwright/test';
import { loginAsDemoUser, createTask, advanceTaskStatus, deleteAllTasks } from './helpers';

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

	test('should show XP after completing a task', async ({ page }) => {
		// Create and complete a task
		await createTask(page, 'E2E XP test task');
		await advanceTaskStatus(page, 'E2E XP test task', 'InProgress');
		await advanceTaskStatus(page, 'E2E XP test task', 'Done');

		// Navigate to dashboard and verify XP is positive
		await page.getByTestId('nav-dashboard').click();
		await expect(page.getByTestId('xp-total')).toBeVisible();
		const xpText = await page.getByTestId('xp-total').textContent();
		const xp = parseInt(xpText?.replace(/[^0-9]/g, '') ?? '0');
		expect(xp).toBeGreaterThan(0);

		// Clean up
		await page.getByTestId('nav-tasks').click();
		await deleteAllTasks(page);
	});
});
