<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();

	let profile = $derived(data.profile);
	let error = $derived(data.error);

	let progressPercent = $derived(
		profile && profile.totalXp + profile.xpToNextLevel > 0
			? Math.round((profile.totalXp / (profile.totalXp + profile.xpToNextLevel)) * 100)
			: 0
	);
</script>

<svelte:head>
	<title>Dashboard | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Progression Dashboard</h1>

	{#if error}
		<p class="error" role="alert">{error}</p>
	{:else if !profile}
		<div class="loading" aria-label="Loading profile">
			<div class="skeleton skeleton-level"></div>
			<div class="skeleton skeleton-bar"></div>
			<div class="skeleton skeleton-stats"></div>
		</div>
	{:else}
		<section class="level-section">
			<div class="level-badge" aria-label={`Level ${profile.level}`}>
				<span class="level-number">{profile.level}</span>
				<span class="level-label">Level</span>
			</div>
		</section>

		<section class="xp-section">
			<div class="progress-header">
				<span class="xp-total">{profile.totalXp.toLocaleString()} XP</span>
				<span class="xp-next"
					>{profile.xpToNextLevel.toLocaleString()} XP to next level</span
				>
			</div>
			<div
				class="progress-bar"
				role="progressbar"
				aria-valuenow={progressPercent}
				aria-valuemin={0}
				aria-valuemax={100}
			>
				<div class="progress-fill" style:width="{progressPercent}%"></div>
			</div>
			<p class="progress-label">{progressPercent}% to Level {profile.level + 1}</p>
		</section>

		<section class="streaks-section">
			<div class="streak-card">
				<span class="streak-value">{profile.currentStreak}</span>
				<span class="streak-label">Current Streak</span>
				<span class="streak-unit">days</span>
			</div>
			<div class="streak-card">
				<span class="streak-value">{profile.longestStreak}</span>
				<span class="streak-label">Longest Streak</span>
				<span class="streak-unit">days</span>
			</div>
		</section>
	{/if}
</main>

<style>
	main {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}

	h1 {
		margin-bottom: 1.5rem;
	}

	.error {
		color: #dc2626;
		padding: 1rem;
		border: 1px solid #dc2626;
		border-radius: 0.25rem;
	}

	.loading {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.skeleton {
		background: linear-gradient(90deg, #e5e7eb 25%, #f3f4f6 50%, #e5e7eb 75%);
		background-size: 200% 100%;
		animation: shimmer 1.5s infinite;
		border-radius: 0.5rem;
	}

	.skeleton-level {
		width: 100px;
		height: 100px;
		border-radius: 50%;
		margin: 0 auto;
	}

	.skeleton-bar {
		height: 1.5rem;
	}

	.skeleton-stats {
		height: 5rem;
	}

	@keyframes shimmer {
		0% {
			background-position: -200% 0;
		}
		100% {
			background-position: 200% 0;
		}
	}

	.level-section {
		display: flex;
		justify-content: center;
		margin-bottom: 2rem;
	}

	.level-badge {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		width: 100px;
		height: 100px;
		border-radius: 50%;
		background: linear-gradient(135deg, #2563eb, #7c3aed);
		color: white;
	}

	.level-number {
		font-size: 2.5rem;
		font-weight: 700;
		line-height: 1;
	}

	.level-label {
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		opacity: 0.9;
	}

	.xp-section {
		margin-bottom: 2rem;
	}

	.progress-header {
		display: flex;
		justify-content: space-between;
		margin-bottom: 0.5rem;
	}

	.xp-total {
		font-weight: 600;
		font-size: 1.125rem;
	}

	.xp-next {
		color: #6b7280;
		font-size: 0.875rem;
	}

	.progress-bar {
		height: 1.5rem;
		background: #e5e7eb;
		border-radius: 0.75rem;
		overflow: hidden;
	}

	.progress-fill {
		height: 100%;
		background: linear-gradient(90deg, #2563eb, #7c3aed);
		border-radius: 0.75rem;
		transition: width 0.3s ease;
		min-width: 0;
	}

	.progress-label {
		text-align: center;
		color: #6b7280;
		font-size: 0.875rem;
		margin-top: 0.5rem;
	}

	.streaks-section {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 1rem;
	}

	.streak-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding: 1.5rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
	}

	.streak-value {
		font-size: 2rem;
		font-weight: 700;
		color: #2563eb;
	}

	.streak-label {
		font-size: 0.875rem;
		font-weight: 500;
		margin-top: 0.25rem;
	}

	.streak-unit {
		font-size: 0.75rem;
		color: #6b7280;
	}
</style>
