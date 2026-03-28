import { env } from '$env/dynamic/private';

export function getBaseUrl(): string {
	const url = env.API_BASE_URL;
	if (!url) {
		throw new Error(
			'API_BASE_URL environment variable is not set. ' +
				'Start the app via Aspire or set API_BASE_URL explicitly.'
		);
	}
	return url;
}
