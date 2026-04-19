import { handleResponse } from './tasks';

export interface InsightCard {
	id: string;
	type: string;
	message: string;
	supportingData: string;
	status: string;
	generatedAt: string;
}

export async function listInsights(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	options?: { includeRead?: boolean }
): Promise<InsightCard[]> {
	const url = new URL('/api/insights', baseUrl);
	if (options?.includeRead) url.searchParams.set('includeRead', 'true');
	const response = await fetch(url);
	return handleResponse<InsightCard[]>(response);
}

export async function markInsightRead(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	insightId: string
): Promise<void> {
	const url = new URL(`/api/insights/${insightId}/read`, baseUrl);
	const response = await fetch(url, { method: 'POST' });
	if (!response.ok) throw new Error('Failed to mark insight as read');
}

export async function saveInsight(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	insightId: string
): Promise<void> {
	const url = new URL(`/api/insights/${insightId}/save`, baseUrl);
	const response = await fetch(url, { method: 'POST' });
	if (!response.ok) throw new Error('Failed to save insight');
}

export async function dismissInsight(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	insightId: string
): Promise<void> {
	const url = new URL(`/api/insights/${insightId}/dismiss`, baseUrl);
	const response = await fetch(url, { method: 'POST' });
	if (!response.ok) throw new Error('Failed to dismiss insight');
}
