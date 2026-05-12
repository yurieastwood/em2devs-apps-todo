import type { AuthResponse } from './auth';
import { ApiError, type ProblemDetails } from './tasks';

async function readJsonOrThrow<T>(response: Response): Promise<T> {
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
		detail: `Request failed with status ${response.status}`
	});
}

export async function deleteAccount(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	confirmation: string
): Promise<void> {
	const url = new URL('/api/account/delete', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ confirmation })
	});
	if (response.ok) return;
	await readJsonOrThrow(response);
}

export async function recoverAccount(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	email: string,
	password: string
): Promise<AuthResponse> {
	const url = new URL('/api/account/recover', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ email, password })
	});
	return readJsonOrThrow<AuthResponse>(response);
}

export interface ImportResponse {
	recordsImported: number;
}

export async function importData(
	fetch: typeof globalThis.fetch,
	baseUrl: string,
	envelopeJson: string
): Promise<ImportResponse> {
	const url = new URL('/api/data/import', baseUrl);
	const response = await fetch(url, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: envelopeJson
	});
	return readJsonOrThrow<ImportResponse>(response);
}
