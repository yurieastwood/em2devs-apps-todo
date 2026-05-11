import { error, type RequestHandler } from '@sveltejs/kit';
import { getBaseUrl } from '$lib/server/config';

/**
 * Proxies the JSON data export from the backend so the browser receives a file
 * download with the correct Content-Disposition header. The cookie/JWT is
 * already attached by the wrapped fetch in hooks.server.ts.
 */
export const GET: RequestHandler = async ({ fetch }) => {
	const url = new URL('/api/data/export?format=json&scope=all', getBaseUrl());
	const response = await fetch(url);

	if (!response.ok) {
		throw error(response.status, `Export failed (${response.status})`);
	}

	const headers = new Headers();
	const contentType = response.headers.get('content-type');
	if (contentType) headers.set('content-type', contentType);

	const contentDisposition = response.headers.get('content-disposition');
	headers.set(
		'content-disposition',
		contentDisposition ?? `attachment; filename="waypoint-export-${Date.now()}.json"`
	);

	return new Response(response.body, { status: 200, headers });
};
