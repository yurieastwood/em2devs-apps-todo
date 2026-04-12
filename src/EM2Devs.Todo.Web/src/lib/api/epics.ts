import { handleResponse, throwIfError } from './tasks';

export interface EpicQuest {
	id: string;
	title: string;
	progress: number;
}

export interface Epic {
	id: string;
	title: string;
	description: string;
	targetDate: string | null;
	progress: number;
	isCompleted: boolean;
	quests: EpicQuest[];
}

export async function listEpics(fetch: typeof globalThis.fetch, baseUrl: string): Promise<Epic[]> {
	const url = new URL('/api/epics', baseUrl);
	const response = await fetch(url);
	return handleResponse<Epic[]>(response);
}

export async function createEpic(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	title: string,
	description: string
): Promise<Epic> {
	const url = new URL('/api/epics', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ title, description })
	});
	return handleResponse<Epic>(response);
}

export async function getEpic(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	epicId: string
): Promise<Epic> {
	const url = new URL(`/api/epics/${epicId}`, baseUrl);
	const response = await fetch(url);
	return handleResponse<Epic>(response);
}

export async function assignQuestToEpic(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	epicId: string,
	questId: string
): Promise<Epic> {
	const url = new URL(`/api/epics/${epicId}/quests`, baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ questId })
	});
	return handleResponse<Epic>(response);
}

export async function removeQuestFromEpic(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	epicId: string,
	questId: string
): Promise<Epic> {
	const url = new URL(`/api/epics/${epicId}/quests/${questId}`, baseUrl);
	const response = await fetch(url, { method: 'DELETE' });
	return handleResponse<Epic>(response);
}

export async function completeEpic(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	epicId: string
): Promise<Epic> {
	const url = new URL(`/api/epics/${epicId}/complete`, baseUrl);
	const response = await fetch(url, { method: 'POST' });
	return handleResponse<Epic>(response);
}

export async function deleteEpic(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	epicId: string
): Promise<void> {
	const url = new URL(`/api/epics/${epicId}`, baseUrl);
	const response = await fetch(url, { method: 'DELETE' });
	if (response.ok) return;
	await throwIfError(response);
}
