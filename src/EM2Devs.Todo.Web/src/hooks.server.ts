import { redirect, type Handle } from '@sveltejs/kit';

export const TOKEN_COOKIE = 'wp_token';

const PUBLIC_ROUTES = new Set(['/login', '/register']);

export const handle: Handle = async ({ event, resolve }) => {
	const routeId = event.route.id;

	// Bypass for asset requests and internal SvelteKit routes (no route.id)
	if (routeId === null) {
		return resolve(event);
	}

	const token = event.cookies.get(TOKEN_COOKIE);
	const isPublic = PUBLIC_ROUTES.has(routeId);

	if (!token && !isPublic) {
		throw redirect(303, '/login');
	}

	if (token && isPublic) {
		throw redirect(303, '/');
	}

	// Wrap event.fetch so all server-side API calls carry the bearer token,
	// and so a 401 response surfaces as a redirect to /login with the cookie cleared.
	if (token) {
		const originalFetch = event.fetch;
		event.fetch = async (input, init) => {
			const headers = new Headers(init?.headers);
			if (!headers.has('Authorization')) {
				headers.set('Authorization', `Bearer ${token}`);
			}
			const response = await originalFetch(input, { ...init, headers });
			if (response.status === 401) {
				event.cookies.delete(TOKEN_COOKIE, { path: '/' });
			}
			return response;
		};
	}

	return resolve(event);
};
