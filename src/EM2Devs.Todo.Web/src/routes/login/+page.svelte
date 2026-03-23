<script lang="ts">
	import { enhance } from '$app/forms';
</script>

<svelte:head>
	<title>Sign In | EM2Devs Todo</title>
</svelte:head>

<main>
	<div class="login-card">
		<h1>Welcome to Waypoint</h1>
		<p>Your personal task management companion.</p>
		<form method="POST" use:enhance>
			<button type="submit" class="btn-demo" data-testid="demo-login-button"
				>Sign in as Demo User</button
			>
		</form>
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
		text-align: center;
		padding: 2rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
		max-width: 400px;
		width: 100%;
	}

	h1 {
		margin-bottom: 0.5rem;
	}

	p {
		color: #6b7280;
		margin-bottom: 1.5rem;
	}

	.btn-demo {
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

	.btn-demo:hover {
		background: #1d4ed8;
	}
</style>
