import { fail } from '@sveltejs/kit';
import {
	listTasks,
	createTask,
	quickAddTask,
	updateTaskStatus,
	reopenTask,
	deleteTask,
	ApiError,
	type TaskView
} from '$lib/api/tasks';
import {
	listRecurringTasks,
	createRecurringTask,
	pauseRecurringTask,
	resumeRecurringTask,
	deleteRecurringTask
} from '$lib/api/recurring';
import { getProfile } from '$lib/api/profile';
import { getBaseUrl } from '$lib/server/config';
import type { Actions, PageServerLoad } from './$types';

const VALID_STATUSES = ['Todo', 'InProgress', 'Done'];
const VALID_VIEWS: readonly TaskView[] = ['inbox', 'today', 'upcoming', 'completed'];
const DEFAULT_VIEW: TaskView = 'today';

function parseView(raw: string | null): TaskView {
	if (raw === null) return DEFAULT_VIEW;
	const lower = raw.toLowerCase();
	return (VALID_VIEWS as readonly string[]).includes(lower) ? (lower as TaskView) : DEFAULT_VIEW;
}

function failFromError(e: unknown, fallbackMessage: string, action: string) {
	const status = e instanceof ApiError ? (e.problem.status ?? 500) : 500;
	const message = e instanceof ApiError ? e.problem.detail : fallbackMessage;
	return fail(status, { action, error: message });
}

export const load: PageServerLoad = async ({ fetch, url }) => {
	const view = parseView(url.searchParams.get('view'));
	try {
		const [tasks, profile, recurringTasks] = await Promise.all([
			listTasks(fetch, getBaseUrl(), { view }),
			getProfile(fetch, getBaseUrl()).catch(() => null),
			listRecurringTasks(fetch, getBaseUrl()).catch(() => [])
		]);
		return { tasks, profile, recurringTasks, view, error: null };
	} catch (e) {
		const message = e instanceof Error ? e.message : 'An unexpected error occurred';
		return { tasks: [], profile: null, recurringTasks: [], view, error: message };
	}
};

export const actions: Actions = {
	create: async ({ request, fetch }) => {
		const formData = await request.formData();
		const title = formData.get('title')?.toString()?.trim() ?? '';
		const scheduledDateRaw = formData.get('scheduledDate')?.toString()?.trim() ?? '';
		const tagsRaw = formData.get('tags')?.toString()?.trim() ?? '';

		if (!title) {
			return fail(400, { action: 'create', error: 'Title is required.' });
		}

		const tags = tagsRaw
			? tagsRaw
					.split(',')
					.map((t) => t.trim())
					.filter((t) => t.length > 0)
			: undefined;

		try {
			await createTask(fetch, getBaseUrl(), {
				title,
				scheduledDate: scheduledDateRaw || undefined,
				tags
			});
			return { action: 'create', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to create task', 'create');
		}
	},

	quickAdd: async ({ request, fetch }) => {
		const formData = await request.formData();
		const input = formData.get('input')?.toString()?.trim() ?? '';

		if (!input) {
			return fail(400, { action: 'quickAdd', error: 'Input is required.' });
		}

		try {
			await quickAddTask(fetch, getBaseUrl(), input);
			return { action: 'quickAdd', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to quick-add task', 'quickAdd');
		}
	},

	updateStatus: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';
		const status = formData.get('status')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'updateStatus', error: 'Task ID is required.' });
		}
		if (!VALID_STATUSES.includes(status)) {
			return fail(400, { action: 'updateStatus', error: `Invalid status: ${status}` });
		}

		try {
			await updateTaskStatus(
				fetch,
				getBaseUrl(),
				taskId,
				status as 'Todo' | 'InProgress' | 'Done'
			);
			return { action: 'updateStatus', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to update status', 'updateStatus');
		}
	},

	reopen: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'reopen', error: 'Task ID is required.' });
		}

		try {
			await reopenTask(fetch, getBaseUrl(), taskId);
			return { action: 'reopen', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to reopen task', 'reopen');
		}
	},

	delete: async ({ request, fetch }) => {
		const formData = await request.formData();
		const taskId = formData.get('taskId')?.toString() ?? '';

		if (!taskId) {
			return fail(400, { action: 'delete', error: 'Task ID is required.' });
		}

		try {
			await deleteTask(fetch, getBaseUrl(), taskId);
			return { action: 'delete', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to delete task', 'delete');
		}
	},

	createRecurring: async ({ request, fetch }) => {
		const formData = await request.formData();
		const title = formData.get('title')?.toString()?.trim() ?? '';
		const pattern = formData.get('pattern')?.toString() ?? '';

		if (!title) return fail(400, { action: 'createRecurring', error: 'Title is required.' });
		if (!pattern)
			return fail(400, { action: 'createRecurring', error: 'Pattern is required.' });

		try {
			await createRecurringTask(fetch, getBaseUrl(), title, pattern);
			return { action: 'createRecurring', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to create recurring task', 'createRecurring');
		}
	},

	pauseRecurring: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id')?.toString() ?? '';
		try {
			await pauseRecurringTask(fetch, getBaseUrl(), id);
			return { action: 'pauseRecurring', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to pause', 'pauseRecurring');
		}
	},

	resumeRecurring: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id')?.toString() ?? '';
		try {
			await resumeRecurringTask(fetch, getBaseUrl(), id);
			return { action: 'resumeRecurring', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to resume', 'resumeRecurring');
		}
	},

	deleteRecurring: async ({ request, fetch }) => {
		const formData = await request.formData();
		const id = formData.get('id')?.toString() ?? '';
		try {
			await deleteRecurringTask(fetch, getBaseUrl(), id);
			return { action: 'deleteRecurring', success: true };
		} catch (e) {
			return failFromError(e, 'Failed to delete recurring task', 'deleteRecurring');
		}
	}
};
