<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let season = $derived(data.season);
	let error = $derived(data.error);
</script>

<svelte:head>
	<title>Seasons — Waypoint</title>
</svelte:head>

<div class="seasons-page">
	<h1>Current Season</h1>

	{#if error}
		<p class="error">{error}</p>
	{/if}

	{#if season}
		<div class="season-header">
			<h2>{season.name}</h2>
			<p class="theme">{season.theme}</p>
			<div class="season-meta">
				<span class="days-remaining">{season.daysRemaining} days remaining</span>
				<span class="date-range">{season.startDate} — {season.endDate}</span>
			</div>
		</div>

		<section class="quest-line">
			<h3>Seasonal Quest Line</h3>
			<div class="quest-progress">
				<div class="progress-bar">
					<div
						class="progress-fill"
						style="width: {((season.questLine.currentStage - 1) /
							season.questLine.totalStages) *
							100}%"
					></div>
				</div>
				<p>
					Stage {season.questLine.currentStage} of {season.questLine.totalStages}
					{#if season.questLine.isCompleted}
						<span class="completed-badge">Completed!</span>
					{/if}
				</p>
			</div>
		</section>
	{:else if !error}
		<p class="empty">No active season.</p>
	{/if}
</div>

<style>
	.seasons-page {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
	}

	h1 {
		font-size: 1.5rem;
		margin-bottom: 1.5rem;
	}

	.season-header {
		padding: 1.5rem;
		border: 1px solid #e5e7eb;
		border-radius: 12px;
		background: linear-gradient(135deg, #f0f9ff, #e0f2fe);
		margin-bottom: 1.5rem;
	}

	.season-header h2 {
		font-size: 1.3rem;
		margin: 0 0 0.25rem;
	}

	.theme {
		color: #4b5563;
		margin: 0 0 1rem;
	}

	.season-meta {
		display: flex;
		justify-content: space-between;
		font-size: 0.85rem;
		color: #6b7280;
	}

	.days-remaining {
		font-weight: 600;
		color: #2563eb;
	}

	.quest-line {
		padding: 1.25rem;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
	}

	.quest-line h3 {
		margin: 0 0 1rem;
		font-size: 1.1rem;
	}

	.progress-bar {
		height: 12px;
		background: #e5e7eb;
		border-radius: 6px;
		overflow: hidden;
		margin-bottom: 0.5rem;
	}

	.progress-fill {
		height: 100%;
		background: #2563eb;
		border-radius: 6px;
		transition: width 0.3s ease;
	}

	.completed-badge {
		color: #16a34a;
		font-weight: 600;
	}

	.error {
		color: #dc2626;
		padding: 0.75rem;
		background: #fef2f2;
		border-radius: 4px;
	}

	.empty {
		color: #9ca3af;
		text-align: center;
		padding: 3rem;
	}
</style>
