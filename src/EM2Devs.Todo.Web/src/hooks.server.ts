import { redirect, type Handle } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
	const isLoginPage = event.url.pathname === '/login';
	const hasCookie = event.cookies.get('demo-user');

	if (!hasCookie && !isLoginPage) {
		throw redirect(303, '/login');
	}

	if (hasCookie && isLoginPage) {
		throw redirect(303, '/');
	}

	return resolve(event);
};
