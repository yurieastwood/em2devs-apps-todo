<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let capacity = $derived(data.capacity);
	let error = $derived(data.error);

	const dayOrder = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

	let maxCap = $derived(capacity ? Math.max(...Object.values(capacity.capacityByDay), 1) : 1);

	function barHeight(value: number, max: number): number {
		return max > 0 ? (value / max) * 100 : 0;
	}
</script>

<svelte:head>
	<title>Capacity — Waypoint</title>
</svelte:head>

<div class="capacity-page">
	<h1>Weekly Capacity</h1>

	{#if error}
		<p class="error">{error}</p>
	{/if}

	{#if capacity}
		{#if capacity.isOvercommitted}
			<div class="warning">
				You have {capacity.todayScheduled} tasks scheduled today but typically complete {capacity.todayCapacity}.
				Consider reprioritising.
			</div>
		{/if}

		<section class="chart-section">
			<div class="bar-chart">
				{#each dayOrder as day (day)}
					{@const value = capacity.capacityByDay[day] ?? 0}
					<div class="bar-column">
						<div class="bar-wrapper">
							<div
								class="bar"
								class:most={day === capacity.mostProductiveDay}
								class:least={day === capacity.leastProductiveDay}
								style="height: {barHeight(value, maxCap)}%"
							></div>
						</div>
						<span class="bar-label">{day.slice(0, 3)}</span>
						<span class="bar-value">{value}</span>
					</div>
				{/each}
			</div>
		</section>

		<section class="stats-section">
			<div class="stat-grid">
				<div class="stat-card">
					<span class="stat-value">{capacity.averageDailyCapacity}</span>
					<span class="stat-label">Avg daily</span>
				</div>
				<div class="stat-card">
					<span class="stat-value">{capacity.todayCapacity}</span>
					<span class="stat-label">Today's capacity</span>
				</div>
				<div class="stat-card">
					<span class="stat-value">{capacity.todayScheduled}</span>
					<span class="stat-label">Today scheduled</span>
				</div>
			</div>
		</section>

		{#if capacity.planningRecommendation}
			<section class="recommendation">
				<p>{capacity.planningRecommendation}</p>
			</section>
		{/if}
	{:else if !error}
		<p class="empty">
			No capacity data yet. Complete tasks over a few days to build your model.
		</p>
	{/if}
</div>

<style>
	.capacity-page {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
	}
	h1 {
		font-size: 1.5rem;
		margin-bottom: 1.5rem;
	}
	.warning {
		padding: 0.75rem 1rem;
		background: #fef3c7;
		border: 1px solid #f59e0b;
		border-radius: 8px;
		color: #92400e;
		margin-bottom: 1.5rem;
		font-size: 0.9rem;
	}
	.chart-section {
		margin-bottom: 2rem;
	}
	.bar-chart {
		display: flex;
		justify-content: space-between;
		align-items: flex-end;
		height: 160px;
		padding: 0 0.5rem;
		gap: 0.25rem;
	}
	.bar-column {
		display: flex;
		flex-direction: column;
		align-items: center;
		flex: 1;
	}
	.bar-wrapper {
		width: 100%;
		height: 120px;
		display: flex;
		align-items: flex-end;
		justify-content: center;
	}
	.bar {
		width: 70%;
		border-radius: 4px 4px 0 0;
		background: #93c5fd;
		transition: height 0.3s ease;
		min-height: 2px;
	}
	.bar.most {
		background: #2563eb;
	}
	.bar.least {
		background: #d1d5db;
	}
	.bar-label {
		font-size: 0.7rem;
		color: #6b7280;
		margin-top: 0.25rem;
	}
	.bar-value {
		font-size: 0.7rem;
		color: #374151;
		font-weight: 600;
	}
	.stats-section {
		margin-bottom: 1.5rem;
	}
	.stat-grid {
		display: grid;
		grid-template-columns: repeat(3, 1fr);
		gap: 0.75rem;
	}
	.stat-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
	}
	.stat-value {
		font-size: 1.5rem;
		font-weight: 700;
		color: #111827;
	}
	.stat-label {
		font-size: 0.75rem;
		color: #6b7280;
	}
	.recommendation {
		padding: 1rem;
		background: #f0fdf4;
		border: 1px solid #86efac;
		border-radius: 8px;
		font-size: 0.9rem;
		color: #166534;
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
