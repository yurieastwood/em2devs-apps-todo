export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical';
export type TaskDifficulty = 'Trivial' | 'Easy' | 'Normal' | 'Hard' | 'Epic';
export type TaskStatus = 'Todo' | 'InProgress' | 'Done' | 'Skipped';

export interface Task {
	id: string;
	title: string;
	description: string | null;
	status: TaskStatus;
	difficulty: TaskDifficulty;
	priority: TaskPriority;
	estimatedMinutes: number | null;
	dueDate: string | null;
	completedAt: string | null;
	actualMinutes?: number | null;
	variancePercent?: number | null;
}

export interface UpdateTaskFields {
	title?: string;
	description?: string | null;
	difficulty?: TaskDifficulty;
	priority?: TaskPriority;
	estimatedMinutes?: number | null;
	clearEstimatedTime?: boolean;
	dueDate?: string | null;
	clearDueDate?: boolean;
}

export interface ProblemDetails {
	type: string;
	title: string;
	status: number;
	detail: string;
	traceId?: string;
	errors?: Record<string, string[]>;
}

export class ApiError extends Error {
	constructor(public readonly problem: ProblemDetails) {
		super(problem.detail ?? problem.title);
		this.name = 'ApiError';
	}
}

export async function throwIfError(response: Response): Promise<void> {
	const contentType = response.headers.get('content-type') ?? '';
	if (
		contentType.includes('application/problem+json') ||
		contentType.includes('application/json')
	) {
		const problem: ProblemDetails = await response.json();
		throw new ApiError(problem);
	}

	throw new ApiError({
		type: 'https://tools.ietf.org/html/rfc9457',
		title: response.statusText,
		status: response.status,
		detail: `Request failed with status ${response.status}`
	});
}

export async function handleResponse<T>(response: Response): Promise<T> {
	if (response.ok) {
		return response.json();
	}
	return throwIfError(response) as never;
}

export async function listTasks(fetch: typeof globalThis.fetch, baseUrl: string): Promise<Task[]> {
	const url = new URL('/api/tasks', baseUrl);
	const response = await fetch(url);
	return handleResponse<Task[]>(response);
}

export async function getTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	taskId: string
): Promise<Task> {
	const url = new URL(`/api/tasks/${taskId}`, baseUrl);
	const response = await fetch(url);
	return handleResponse<Task>(response);
}

export async function createTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	title: string
): Promise<Task> {
	const url = new URL('/api/tasks', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ title })
	});
	return handleResponse<Task>(response);
}

export async function updateTaskStatus(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	taskId: string,
	status: Task['status']
): Promise<Task> {
	const url = new URL(`/api/tasks/${taskId}/status`, baseUrl);
	const response = await fetch(url, {
		method: 'PATCH',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ status })
	});
	return handleResponse<Task>(response);
}

export async function updateTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	taskId: string,
	fields: UpdateTaskFields
): Promise<Task> {
	const url = new URL(`/api/tasks/${taskId}`, baseUrl);
	const response = await fetch(url, {
		method: 'PATCH',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(fields)
	});
	return handleResponse<Task>(response);
}

export async function reopenTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	taskId: string
): Promise<Task> {
	const url = new URL(`/api/tasks/${taskId}/reopen`, baseUrl);
	const response = await fetch(url, { method: 'PATCH' });
	return handleResponse<Task>(response);
}

export async function recordActualTime(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	taskId: string,
	actualMinutes: number
): Promise<Task> {
	const url = new URL(`/api/tasks/${taskId}/actual-time`, baseUrl);
	const response = await fetch(url, {
		method: 'PATCH',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ actualMinutes })
	});
	return handleResponse<Task>(response);
}

export async function deleteTask(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	taskId: string
): Promise<void> {
	const url = new URL(`/api/tasks/${taskId}`, baseUrl);
	const response = await fetch(url, { method: 'DELETE' });
	if (response.ok) return;
	await throwIfError(response);
}
