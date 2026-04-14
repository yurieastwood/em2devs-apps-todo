import { test, expect } from '@playwright/test';
import { loginAsDemoUser, createTask, advanceTaskStatus, deleteAllTasks } from './helpers';

test.describe('Profile expansion dashboard', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
	});

	test('renders XP history, titles, and all seven skill trees after completing tasks', async ({
		page
	}) => {
		// Complete three tasks to generate XP history entries.
		await page.goto('/');
		const titles = [
			'Profile expansion task alpha',
			'Profile expansion task beta',
			'Profile expansion task gamma'
		];
		for (const t of titles) {
			await createTask(page, t);
			await advanceTaskStatus(page, t, 'InProgress');
			await advanceTaskStatus(page, t, 'Done');
		}

		// Navigate to the dashboard.
		await page.goto('/dashboard');

		// XP history section: at least one entry renders.
		const xpSection = page.getByTestId('xp-history-section');
		await expect(xpSection).toBeVisible();
		const historyRows = page.getByTestId('xp-history-row');
		expect(await historyRows.count()).toBeGreaterThan(0);

		// Titles section is present (may be empty depending on qualifying actions).
		await expect(page.getByTestId('titles-section')).toBeVisible();

		// Skill trees section shows all seven cards.
		await expect(page.getByTestId('skill-trees-section')).toBeVisible();
		const treeCards = page.getByTestId('skill-tree-card');
		await expect(treeCards).toHaveCount(7);

		// Clean up
		await page.goto('/');
		await deleteAllTasks(page);
	});
});
