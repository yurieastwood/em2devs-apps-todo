import { handleResponse } from './tasks';

export interface XpBreakdown {
	baseXp: number;
	deadlineModifier: number;
	streakMultiplier: number;
	finalXp: number;
}

export interface XpHistoryEntry {
	date: string;
	xpEarned: number;
	source: string;
	cumulativeTotal: number;
}

export interface Title {
	type: string;
	displayName: string;
	earnedOn: string;
}

export interface TitleProgress {
	type: string;
	progressPercentage: number;
	remainingDescription: string;
}

export interface ProfileTitles {
	earned: Title[];
	active: string | null;
	progress: TitleProgress[];
}

export interface SkillTreePerk {
	tier: number;
	perkType: string;
	description: string;
}

export interface SkillTree {
	type: string;
	tier: number | null;
	tasksCompletedInTier: number | null;
	tasksToNextTier: number | null;
	unlockHint: string | null;
	perks: SkillTreePerk[];
}

export interface StreakFreeze {
	frozenAt: string;
	days: number;
	expiresAt: string;
}

export interface PlayerProfile {
	totalXp: number;
	level: number;
	xpToNextLevel: number;
	currentStreak: number;
	longestStreak: number;
	lastXpBreakdown: XpBreakdown | null;
	xpHistory: XpHistoryEntry[];
	titles: ProfileTitles;
	skillTrees: SkillTree[];
	streakFreeze: StreakFreeze | null;
}

export async function getProfile(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<PlayerProfile> {
	const url = new URL('/api/profile', baseUrl);
	const response = await fetch(url);
	return handleResponse<PlayerProfile>(response);
}

export type EstimationCalibrationState = 'NotEnoughData' | 'Calibrated';

export interface EstimationBias {
	biasFactor: number;
	sampleSize: number;
	calibrationState: EstimationCalibrationState;
}

export async function getEstimationBias(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<EstimationBias> {
	const url = new URL('/api/profile/estimation-bias', baseUrl);
	const response = await fetch(url);
	return handleResponse<EstimationBias>(response);
}

export async function freezeStreak(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	days: number
): Promise<PlayerProfile> {
	const url = new URL('/api/profile/streak/freeze', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ days })
	});
	return handleResponse<PlayerProfile>(response);
}
