import { fail, redirect } from '@sveltejs/kit';
import { deleteAccount, importData } from '$lib/api/account';
import { me } from '$lib/api/auth';
import { ApiError } from '$lib/api/tasks';
import { getBaseUrl } from '$lib/server/config';
import { TOKEN_COOKIE } from '../../hooks.server';
import type { Actions, PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ fetch }) => {
	const profile = await me(fetch, getBaseUrl());
	if (!profile) {
		throw redirect(303, '/login');
	}
	return { user: profile };
};

export const actions: Actions = {
	delete: async ({ request, fetch, cookies }) => {
		const formData = await request.formData();
		const confirmation = formData.get('confirmation')?.toString() ?? '';

		if (confirmation !== 'DELETE MY ACCOUNT') {
			return fail(400, { error: 'Confirmation phrase must be "DELETE MY ACCOUNT".' });
		}

		try {
			await deleteAccount(fetch, getBaseUrl(), confirmation);
		} catch (e) {
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Failed to delete account.')
					: 'Failed to delete account.';
			return fail(500, { error: message });
		}

		cookies.delete(TOKEN_COOKIE, { path: '/' });
		throw redirect(303, '/login?deactivated=1');
	},

	import: async ({ request, fetch }) => {
		const formData = await request.formData();
		const file = formData.get('file');
		const confirmation = formData.get('importConfirmation')?.toString() ?? '';

		if (confirmation !== 'OVERWRITE MY DATA') {
			return fail(400, {
				importError: 'Confirmation phrase must be "OVERWRITE MY DATA" to confirm overwrite.'
			});
		}

		if (!(file instanceof File) || file.size === 0) {
			return fail(400, { importError: 'A JSON export file is required.' });
		}

		const text = await file.text();

		// Quick sanity check so we surface a clear error before posting bytes upstream.
		try {
			const parsed = JSON.parse(text);
			if (typeof parsed !== 'object' || parsed === null || !('meta' in parsed)) {
				return fail(400, {
					importError:
						'File does not look like a Waypoint export (missing "meta" section).'
				});
			}
		} catch {
			return fail(400, { importError: 'File is not valid JSON.' });
		}

		try {
			const result = await importData(fetch, getBaseUrl(), text);
			return { importSuccess: `Restored ${result.recordsImported} record(s).` };
		} catch (e) {
			const message =
				e instanceof ApiError
					? (e.problem.detail ?? 'Import failed.')
					: 'Import failed. Please try again.';
			return fail(500, { importError: message });
		}
	}
};
