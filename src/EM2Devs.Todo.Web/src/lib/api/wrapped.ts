import { handleResponse } from './tasks';

export interface WrappedSlide {
	title: string;
	metric: string;
	visualizationType: string;
	isShareable: boolean;
}

export interface AnnualWrapped {
	year: number;
	isPartialYear: boolean;
	slides: WrappedSlide[];
}

export async function getWrapped(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	year?: number
): Promise<AnnualWrapped> {
	const url = new URL('/api/wrapped', baseUrl);
	if (year) url.searchParams.set('year', String(year));
	const response = await fetch(url);
	return handleResponse<AnnualWrapped>(response);
}
