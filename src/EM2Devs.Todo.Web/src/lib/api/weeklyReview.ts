import { handleResponse } from './tasks';

export interface WeeklyReflection {
	whatWentWell: string;
	whatDragged: string;
	adjustment: string;
	savedAt: string;
}

export interface WeeklyReview {
	weekOf: string;
	tasksCompleted: number;
	xpEarned: number;
	streakStart: number;
	streakEnd: number;
	notableEvents: string[];
	reflection: WeeklyReflection | null;
}

export interface SaveWeeklyReviewInput {
	whatWentWell: string;
	whatDragged: string;
	adjustment: string;
	weekOf?: string | null;
}

export async function getWeeklyReview(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	weekOf?: string | null
): Promise<WeeklyReview> {
	const url = new URL('/api/weekly-review', baseUrl);
	if (weekOf) {
		url.searchParams.set('weekOf', weekOf);
	}
	const response = await fetch(url);
	return handleResponse<WeeklyReview>(response);
}

export async function saveWeeklyReview(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	input: SaveWeeklyReviewInput
): Promise<WeeklyReflection> {
	const url = new URL('/api/weekly-review', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(input)
	});
	return handleResponse<WeeklyReflection>(response);
}
