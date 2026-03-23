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

	test('should show progression after completing a task', async ({ page }) => {
		// Read initial progression from a fresh dashboard load
		await page.goto('/dashboard');
		await expect(page.getByTestId('xp-total')).toBeVisible();
		const initialXpText = await page.getByTestId('xp-total').textContent();
		const initialXp = parseInt(initialXpText?.replace(/[^0-9]/g, '') ?? '0');
		const initialLevelText = await page.getByTestId('level-number').textContent();
		const initialLevel = parseInt(initialLevelText ?? '0');

		// Create and complete a task
		await page.goto('/');
		await createTask(page, 'E2E XP test task');
		await advanceTaskStatus(page, 'E2E XP test task', 'InProgress');
		await advanceTaskStatus(page, 'E2E XP test task', 'Done');

		// Read updated progression from a fresh dashboard load
		await page.goto('/dashboard');
		await expect(page.getByTestId('xp-total')).toBeVisible();
		const updatedXpText = await page.getByTestId('xp-total').textContent();
		const updatedXp = parseInt(updatedXpText?.replace(/[^0-9]/g, '') ?? '0');
		const updatedLevelText = await page.getByTestId('level-number').textContent();
		const updatedLevel = parseInt(updatedLevelText ?? '0');

		// XP should increase, or level should increase (XP resets on level-up)
		const progressed = updatedLevel > initialLevel || updatedXp > initialXp;
		expect(progressed).toBe(true);

		// Clean up
		await page.goto('/');
		await deleteAllTasks(page);
	});
});
