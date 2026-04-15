import { handleResponse } from './tasks';
import type { TaskDifficulty, TaskPriority } from './tasks';

export type DailyBriefStatus = 'Available' | 'InsufficientTasks';

export interface DailyBriefTask {
	id: string;
	title: string;
	difficulty: TaskDifficulty;
	priority: TaskPriority;
	estimatedMinutes: number | null;
	scheduledDate: string | null;
}

export interface DailyBrief {
	date: string;
	greeting: string;
	currentStreakDays: number;
	corePlanCount: number;
	ifTimeAllowsCount: number;
	overdueCount: number;
	corePlan: DailyBriefTask[];
	ifTimeAllows: DailyBriefTask[];
	overdue: DailyBriefTask[];
	status: DailyBriefStatus;
}

export async function getDailyBrief(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<DailyBrief> {
	const url = new URL('/api/daily-brief', baseUrl);
	const response = await fetch(url);
	return handleResponse<DailyBrief>(response);
}
