<script lang="ts">
	import { enhance } from '$app/forms';
	import { resolve } from '$app/paths';
	import type { ActionData } from './$types';

	let { form }: { form: ActionData } = $props();
</script>

<svelte:head>
	<title>Create Account | EM2Devs Todo</title>
</svelte:head>

<main>
	<div class="register-card">
		<h1>Create your account</h1>
		<p>Sign up for a Waypoint account.</p>
		<form
			method="POST"
			use:enhance={() => {
				return async ({ update }) => {
					await update({ invalidateAll: true });
				};
			}}
		>
			<label for="displayName">Display name</label>
			<input
				id="displayName"
				name="displayName"
				type="text"
				autocomplete="name"
				required
				minlength="1"
				maxlength="100"
				value={form?.displayName ?? ''}
				data-testid="displayname-input"
			/>

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
				autocomplete="new-password"
				required
				minlength="8"
				data-testid="password-input"
			/>

			{#if form?.error}
				<p class="error" data-testid="register-error">{form.error}</p>
			{/if}

			<button type="submit" class="btn-primary" data-testid="register-button"
				>Create account</button
			>
		</form>
		<p class="hint">
			Already have an account? <a href={resolve('/login')} data-testid="login-link">Sign in</a
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

	.register-card {
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
