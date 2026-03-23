import { ApiError, type ProblemDetails } from './tasks';

export interface PlayerProfile {
	totalXp: number;
	level: number;
	xpToNextLevel: number;
	currentStreak: number;
	longestStreak: number;
}

export async function getProfile(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<PlayerProfile> {
	const url = new URL('/api/profile', baseUrl);
	const response = await fetch(url);

	if (response.ok) {
		return response.json();
	}

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
		detail: `Failed to fetch profile: ${response.status}`
	});
}
