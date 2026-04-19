import { handleResponse } from './tasks';

export interface EnergyProfile {
	currentLevel: string | null;
	hasSufficientData: boolean;
	confidenceScore: number;
	confidenceLevel: string;
	dataPoints: number;
	insufficientDataMessage: string | null;
}

export interface EnergyCheckInResult {
	level: string;
	isUpdate: boolean;
	hasFluctuated: boolean;
}

export async function getEnergyProfile(
	fetch: typeof globalThis.fetch,
	baseUrl: string
): Promise<EnergyProfile> {
	const url = new URL('/api/energy/profile', baseUrl);
	const response = await fetch(url);
	return handleResponse<EnergyProfile>(response);
}

export async function checkInEnergy(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	level: string
): Promise<EnergyCheckInResult> {
	const url = new URL('/api/energy/check-in', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ level })
	});
	return handleResponse<EnergyCheckInResult>(response);
}
