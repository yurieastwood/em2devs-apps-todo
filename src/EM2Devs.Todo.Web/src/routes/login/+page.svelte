<script lang="ts">
	import { enhance } from '$app/forms';
	import { resolve } from '$app/paths';
	import { page } from '$app/state';
	import type { ActionData } from './$types';

	let { form }: { form: ActionData } = $props();
	let deactivatedBanner = $derived(page.url.searchParams.get('deactivated') === '1');
</script>

<svelte:head>
	<title>Sign In | EM2Devs Todo</title>
</svelte:head>

<main>
	<div class="login-card">
		<h1>Welcome to Waypoint</h1>
		<p>Sign in to your account to continue.</p>

		{#if deactivatedBanner}
			<div class="banner" data-testid="deactivated-banner">
				Your account has been deactivated. Sign in within 30 days to recover it.
			</div>
		{/if}

		<form
			method="POST"
			action={form?.deactivated ? '?/recover' : ''}
			use:enhance={() => {
				return async ({ update }) => {
					await update({ invalidateAll: true });
				};
			}}
		>
			<label for="email">Email</label>
			<input
				id="email"
				name="email"
				type="email"
				autocomplete="email"
				required
				value={form?.email ?? ''}
				data-testid="email-input"
			/>

			<label for="password">Password</label>
			<input
				id="password"
				name="password"
				type="password"
				autocomplete="current-password"
				required
				data-testid="password-input"
			/>

			{#if form?.error}
				<p class="error" data-testid="login-error">{form.error}</p>
			{/if}

			{#if form?.deactivated}
				<p class="hint-inline" data-testid="recover-hint">
					This account is deactivated. Submit the same credentials to recover it.
				</p>
				<button type="submit" class="btn-primary" data-testid="recover-button">
					Recover account
				</button>
			{:else}
				<button type="submit" class="btn-primary" data-testid="login-button">Sign In</button
				>
			{/if}
		</form>
		<p class="hint">
			New here? <a href={resolve('/register')} data-testid="register-link"
				>Create an account</a
			>.
		</p>
	</div>
</main>

<style>
	main {
		display: flex;
		justify-content: center;
		align-items: center;
		min-height: 100vh;
		font-family: system-ui, sans-serif;
	}

	.login-card {
		text-align: left;
		padding: 2rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
		max-width: 400px;
		width: 100%;
	}

	h1 {
		margin-bottom: 0.5rem;
		text-align: center;
	}

	p {
		color: #6b7280;
		margin-bottom: 1.5rem;
		text-align: center;
	}

	.banner {
		background: #fef3c7;
		border: 1px solid #fcd34d;
		color: #92400e;
		padding: 0.75rem;
		border-radius: 0.25rem;
		margin-bottom: 1rem;
		font-size: 0.875rem;
		text-align: left;
	}

	label {
		display: block;
		font-size: 0.875rem;
		font-weight: 500;
		margin-top: 1rem;
		margin-bottom: 0.25rem;
		color: #374151;
	}

	input {
		width: 100%;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
		box-sizing: border-box;
	}

	.error {
		color: #b91c1c;
		margin: 0.75rem 0 0;
		font-size: 0.875rem;
		text-align: left;
	}

	.hint-inline {
		margin: 0.5rem 0 0;
		font-size: 0.875rem;
		text-align: left;
		color: #92400e;
	}

	.btn-primary {
		margin-top: 1.25rem;
		width: 100%;
		padding: 0.75rem 1.5rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 1rem;
		font-weight: 500;
	}

	.btn-primary:hover {
		background: #1d4ed8;
	}

	.hint {
		margin-top: 1rem;
		font-size: 0.875rem;
		color: #6b7280;
		text-align: center;
	}

	.hint a {
		color: #2563eb;
		text-decoration: none;
	}
</style>
