import { handleResponse, throwIfError } from './tasks';

export interface RecurringTask {
	id: string;
	title: string;
	pattern: 'Daily' | 'Weekly' | 'Monthly';
	isActive: boolean;
}

export async function listRecurringTasks(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<RecurringTask[]> {
	const url = new URL('/api/recurring-tasks', baseUrl);
	const response = await fetch(url);
	return handleResponse<RecurringTask[]>(response);
}

export async function createRecurringTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	title: string,
	pattern: string
): Promise<RecurringTask> {
	const url = new URL('/api/recurring-tasks', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ title, pattern })
	});
	return handleResponse<RecurringTask>(response);
}

export async function pauseRecurringTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	id: string
): Promise<RecurringTask> {
	const url = new URL(`/api/recurring-tasks/${id}/pause`, baseUrl);
	const response = await fetch(url, { method: 'PATCH' });
	return handleResponse<RecurringTask>(response);
}

export async function resumeRecurringTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	id: string
): Promise<RecurringTask> {
	const url = new URL(`/api/recurring-tasks/${id}/resume`, baseUrl);
	const response = await fetch(url, { method: 'PATCH' });
	return handleResponse<RecurringTask>(response);
}

export async function deleteRecurringTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	id: string
): Promise<void> {
	const url = new URL(`/api/recurring-tasks/${id}`, baseUrl);
	const response = await fetch(url, { method: 'DELETE' });
	if (response.ok) return;
	await throwIfError(response);
}
