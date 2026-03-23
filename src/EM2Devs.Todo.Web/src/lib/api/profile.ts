import { handleResponse } from './tasks';

export interface XpBreakdown {
	baseXp: number;
	deadlineModifier: number;
	streakMultiplier: number;
	finalXp: number;
}

export interface PlayerProfile {
	totalXp: number;
	level: number;
	xpToNextLevel: number;
	currentStreak: number;
	longestStreak: number;
	lastXpBreakdown: XpBreakdown | null;
}

export async function getProfile(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<PlayerProfile> {
	const url = new URL('/api/profile', baseUrl);
	const response = await fetch(url);
	return handleResponse<PlayerProfile>(response);
}
