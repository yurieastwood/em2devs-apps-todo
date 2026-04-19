import { handleResponse } from './tasks';

export interface CapacityOverview {
	capacityByDay: Record<string, number>;
	mostProductiveDay: string;
	leastProductiveDay: string;
	averageDailyCapacity: number;
	todayCapacity: number;
	todayScheduled: number;
	isOvercommitted: boolean;
	planningRecommendation: string | null;
}

export async function getCapacityOverview(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<CapacityOverview> {
	const url = new URL('/api/capacity', baseUrl);
	const response = await fetch(url);
	return handleResponse<CapacityOverview>(response);
}
