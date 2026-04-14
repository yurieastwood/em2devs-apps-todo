import { test, expect, type Page } from '@playwright/test';
import { loginAsDemoUser, createTask, deleteAllTasks, advanceTaskStatus } from './helpers';

test.describe('Task views (Inbox/Today/Upcoming/Completed)', () => {
	test.beforeEach(async ({ page }) => {
		await loginAsDemoUser(page);
		// Switch to the Inbox view so new tasks (which have no scheduled date) are visible
		// immediately after creation — the default Today view would hide them.
		await page.goto('/?view=inbox');
		await deleteAllTasks(page);
	});

	test('should render all four view tabs and reflect the active view via ?view=', async ({
		page
	}) => {
		// Landing on "/" defaults to the Today view.
		await expect(page).toHaveURL(/\/(?:\?.*)?$/);
		await expect(page.getByTestId('view-tabs')).toBeVisible();

		for (const view of ['inbox', 'today', 'upcoming', 'completed'] as const) {
			await expect(page.getByTestId(`view-tab-${view}`)).toBeVisible();
		}

		// Clicking each tab updates the URL and sets aria-current on the active tab.
		for (const view of ['inbox', 'upcoming', 'completed', 'today'] as const) {
			await page.getByTestId(`view-tab-${view}`).click();
			await expect(page).toHaveURL(new RegExp(`[?&]view=${view}(?:&|$)`));
			await expect(page.getByTestId(`view-tab-${view}`)).toHaveAttribute(
				'aria-current',
				'page'
			);
		}
	});

	test('should bucket tasks into the appropriate view', async ({ page }) => {
		const runId = Date.now();
		const inboxTitle = `Inbox-only task ${runId}`;
		const completedTitle = `Completed task ${runId}`;

		// Inbox task — no schedule, no quest.
		await createTask(page, inboxTitle);

		// Completed task: create + walk to Done. Final "Done" state disappears from the
		// Inbox view (which excludes Done), so we explicitly jump to the Completed view
		// after advancing to verify completion instead of asserting on the Inbox item.
		await createTask(page, completedTitle);
		await advanceTaskStatus(page, completedTitle, 'InProgress');
		await page
			.getByTestId('task-item')
			.filter({ hasText: completedTitle })
			.getByTestId('task-advance-button')
			.click();
		await gotoView(page, 'completed');
		await expect(
			page.getByTestId('task-title').filter({ hasText: completedTitle })
		).toBeVisible({ timeout: 10_000 });

		// Inbox view: only the untouched open task is visible; completed task is not.
		await gotoView(page, 'inbox');
		await expect(page.getByTestId('task-title').filter({ hasText: inboxTitle })).toBeVisible();
		await expect(
			page.getByTestId('task-title').filter({ hasText: completedTitle })
		).toHaveCount(0);

		// Completed view: only the Done task is visible, inside a group.
		await gotoView(page, 'completed');
		await expect(page.getByTestId('task-group-header').first()).toBeVisible();
		await expect(
			page.getByTestId('task-title').filter({ hasText: completedTitle })
		).toBeVisible();
		await expect(page.getByTestId('task-title').filter({ hasText: inboxTitle })).toHaveCount(0);

		// Today view: neither of these appear (no scheduled date set, completed excluded).
		await gotoView(page, 'today');
		await expect(page.getByTestId('task-title').filter({ hasText: inboxTitle })).toHaveCount(0);
		await expect(
			page.getByTestId('task-title').filter({ hasText: completedTitle })
		).toHaveCount(0);
		await expect(page.getByTestId('view-empty')).toBeVisible();

		// Upcoming view: empty (no tasks with a scheduled date in next 14 days).
		await gotoView(page, 'upcoming');
		await expect(page.getByTestId('view-empty')).toBeVisible();
	});
});

async function gotoView(page: Page, view: 'inbox' | 'today' | 'upcoming' | 'completed') {
	await page.getByTestId(`view-tab-${view}`).click();
	await expect(page).toHaveURL(new RegExp(`[?&]view=${view}(?:&|$)`));
}
