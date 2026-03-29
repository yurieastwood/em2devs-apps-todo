import { env } from '$env/dynamic/private';

export function getBaseUrl(): string {
	return env.API_BASE_URL ?? 'http://localhost:5001';
}
