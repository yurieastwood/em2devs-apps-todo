import { describe, it, expect, beforeEach } from 'vitest';
import { get } from 'svelte/store';
import { currentUser } from './auth';

const demoUser = {
	userId: '00000000-0000-0000-0000-000000000001',
	displayName: 'Demo User',
	email: 'demo@waypoint.dev'
};

describe('auth store', () => {
	beforeEach(() => {
		currentUser.set(null);
	});
	it('Should_BeNull_When_NoUserLoggedIn', () => {
		expect(get(currentUser)).toBeNull();
	});

	it('Should_HoldUser_When_UserSetsStore', () => {
		currentUser.set(demoUser);

		expect(get(currentUser)).toEqual(demoUser);
	});

	it('Should_ClearUser_When_LogoutSetsNull', () => {
		currentUser.set(demoUser);
		currentUser.set(null);

		expect(get(currentUser)).toBeNull();
	});
});
