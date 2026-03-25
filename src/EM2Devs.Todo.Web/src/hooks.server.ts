import { redirect, type Handle } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
	const routeId = event.route.id;

	// Bypass for asset requests and internal SvelteKit routes (no route.id)
	if (routeId === null) {
		return resolve(event);
	}

	const isLoginPage = routeId === '/login';
	const hasCookie = event.cookies.get('demo-user');

	if (!hasCookie && !isLoginPage) {
		throw redirect(303, '/login');
	}

	if (hasCookie && isLoginPage) {
		throw redirect(303, '/');
	}

	return resolve(event);
};
