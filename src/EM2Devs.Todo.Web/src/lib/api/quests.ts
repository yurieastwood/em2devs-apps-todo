export interface QuestTask {
	id: string;
	title: string;
	status: 'Todo' | 'InProgress' | 'Done';
}

export interface Quest {
	id: string;
	title: string;
	description: string;
	dueDate: string | null;
	progress: number;
	tasks: QuestTask[];
}

async function handleResponse<T>(response: Response): Promise<T> {
	if (response.ok) {
		return response.json();
	}
	throw new Error(`Request failed with status ${response.status}`);
}

export async function listQuests(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<Quest[]> {
	const url = new URL('/api/quests', baseUrl);
	const response = await fetch(url);
	return handleResponse<Quest[]>(response);
}

export async function createQuest(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	title: string,
	description: string
): Promise<Quest> {
	const url = new URL('/api/quests', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ title, description })
	});
	return handleResponse<Quest>(response);
}

export async function getQuest(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	questId: string
): Promise<Quest> {
	const url = new URL(`/api/quests/${questId}`, baseUrl);
	const response = await fetch(url);
	return handleResponse<Quest>(response);
}

export async function deleteQuest(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	questId: string
): Promise<void> {
	const url = new URL(`/api/quests/${questId}`, baseUrl);
	const response = await fetch(url, { method: 'DELETE' });
	if (!response.ok) {
		throw new Error(`Delete failed with status ${response.status}`);
	}
}
