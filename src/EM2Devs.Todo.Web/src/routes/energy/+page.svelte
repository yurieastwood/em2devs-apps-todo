<script lang="ts">
	import { enhance } from '$app/forms';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();
	let profile = $derived(data.profile);
	let error = $derived(data.error);

	const levels = ['Low', 'Medium', 'High', 'Peak'];

	const levelEmoji: Record<string, string> = {
		Low: '🔋',
		Medium: '⚡',
		High: '🔥',
		Peak: '💥'
	};
</script>

<svelte:head>
	<title>Energy — Waypoint</title>
</svelte:head>

<div class="energy-page">
	<h1>Energy Check-in</h1>

	{#if error}
		<p class="error">{error}</p>
	{/if}

	{#if form && 'success' in form && form.success}
		<p class="success">Energy set to {form.result?.level}!</p>
	{/if}

	<section class="checkin-section">
		<h2>How's your energy right now?</h2>
		<form method="POST" action="?/checkin" use:enhance>
			<div class="level-grid">
				{#each levels as level (level)}
					<button type="submit" name="level" value={level} class="level-btn">
						<span class="level-emoji">{levelEmoji[level]}</span>
						<span class="level-label">{level}</span>
					</button>
				{/each}
			</div>
		</form>
	</section>

	{#if profile}
		<section class="profile-section">
			<h2>Energy Profile</h2>
			<div class="profile-grid">
				<div class="profile-card">
					<span class="card-value">{profile.currentLevel ?? '—'}</span>
					<span class="card-label">Current Level</span>
				</div>
				<div class="profile-card">
					<span class="card-value">{profile.dataPoints}</span>
					<span class="card-label">Check-ins</span>
				</div>
				<div class="profile-card">
					<span class="card-value">{profile.confidenceLevel}</span>
					<span class="card-label">Confidence</span>
				</div>
			</div>
			{#if profile.insufficientDataMessage}
				<p class="hint">{profile.insufficientDataMessage}</p>
			{/if}
		</section>
	{/if}
</div>

<style>
	.energy-page {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
	}

	h1 {
		font-size: 1.5rem;
		margin-bottom: 1.5rem;
	}

	.checkin-section {
		margin-bottom: 2rem;
	}

	.checkin-section h2 {
		font-size: 1.1rem;
		margin-bottom: 1rem;
	}

	.level-grid {
		display: grid;
		grid-template-columns: repeat(4, 1fr);
		gap: 0.75rem;
	}

	.level-btn {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.25rem;
		padding: 1rem 0.5rem;
		border: 2px solid #e5e7eb;
		border-radius: 12px;
		background: white;
		cursor: pointer;
		transition: all 0.2s;
	}

	.level-btn:hover {
		border-color: #2563eb;
		background: #eff6ff;
	}

	.level-emoji {
		font-size: 1.5rem;
	}

	.level-label {
		font-size: 0.85rem;
		font-weight: 600;
		color: #374151;
	}

	.profile-section h2 {
		font-size: 1.1rem;
		margin-bottom: 1rem;
	}

	.profile-grid {
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		gap: 0.75rem;
		margin-bottom: 1rem;
	}

	.profile-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
	}

	.card-value {
		font-size: 1.2rem;
		font-weight: 700;
		color: #111827;
	}

	.card-label {
		font-size: 0.75rem;
		color: #6b7280;
	}

	.hint {
		font-size: 0.85rem;
		color: #6b7280;
		font-style: italic;
		text-align: center;
	}

	.success {
		color: #16a34a;
		padding: 0.75rem;
		background: #f0fdf4;
		border-radius: 4px;
		margin-bottom: 1rem;
	}

	.error {
		color: #dc2626;
		padding: 0.75rem;
		background: #fef2f2;
		border-radius: 4px;
	}
</style>
