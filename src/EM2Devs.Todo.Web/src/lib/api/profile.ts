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
}

export async function getProfile(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<PlayerProfile> {
	const url = new URL('/api/profile', baseUrl);
	const response = await fetch(url);
	return handleResponse<PlayerProfile>(response);
}
