import { handleResponse } from './tasks';

export interface SeasonalQuestLine {
	totalStages: number;
	currentStage: number;
	tasksCompletedInStage: number;
	tasksRemaining: number;
	isCompleted: boolean;
}

export interface CosmeticItem {
	name: string;
	rarity: string;
	requiredStage: number;
	isEarned: boolean;
}

export interface CurrentSeason {
	name: string;
	theme: string;
	startDate: string;
	endDate: string;
	daysRemaining: number;
	isActive: boolean;
	questLine: SeasonalQuestLine;
	availableCosmetics: CosmeticItem[];
}

export async function getCurrentSeason(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<CurrentSeason> {
	const url = new URL('/api/seasons/current', baseUrl);
	const response = await fetch(url);
	return handleResponse<CurrentSeason>(response);
}
