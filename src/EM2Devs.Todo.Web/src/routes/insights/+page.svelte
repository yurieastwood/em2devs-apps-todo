<script lang="ts">
	import { enhance } from '$app/forms';
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let insights = $derived(data.insights);
	let error = $derived(data.error);
</script>

<svelte:head>
	<title>Insights — Waypoint</title>
</svelte:head>

<div class="insights-page">
	<h1>Insight Cards</h1>

	{#if error}
		<p class="error">{error}</p>
	{/if}

	{#if insights.length === 0}
		<div class="empty-state">
			<p class="empty-icon">💡</p>
			<p>No insight cards yet.</p>
			<p class="empty-hint">
				Keep completing tasks and building patterns — insights will appear as we learn your
				habits.
			</p>
		</div>
	{:else}
		<div class="insight-list">
			{#each insights as insight (insight.id)}
				<div
					class="insight-card"
					class:read={insight.status === 'Read'}
					class:saved={insight.status === 'Saved'}
				>
					<div class="insight-header">
						<span class="insight-type"
							>{insight.type.replace(/([A-Z])/g, ' $1').trim()}</span
						>
						<span class="insight-status">{insight.status}</span>
					</div>
					<p class="insight-message">{insight.message}</p>
					{#if insight.supportingData}
						<p class="insight-data">{insight.supportingData}</p>
					{/if}
					<div class="insight-actions">
						{#if insight.status === 'Unread'}
							<form method="POST" action="?/read" use:enhance>
								<input type="hidden" name="id" value={insight.id} />
								<button type="submit" class="action-btn">Mark read</button>
							</form>
						{/if}
						{#if insight.status !== 'Saved'}
							<form method="POST" action="?/save" use:enhance>
								<input type="hidden" name="id" value={insight.id} />
								<button type="submit" class="action-btn save">Save</button>
							</form>
						{/if}
						<form method="POST" action="?/dismiss" use:enhance>
							<input type="hidden" name="id" value={insight.id} />
							<button type="submit" class="action-btn dismiss">Dismiss</button>
						</form>
					</div>
				</div>
			{/each}
		</div>
	{/if}
</div>

<style>
	.insights-page {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
	}
	h1 {
		font-size: 1.5rem;
		margin-bottom: 1.5rem;
	}
	.empty-state {
		text-align: center;
		padding: 3rem 1rem;
		color: #666;
	}
	.empty-icon {
		font-size: 3rem;
		margin-bottom: 0.5rem;
	}
	.empty-hint {
		font-size: 0.85rem;
		color: #999;
	}
	.insight-list {
		display: flex;
		flex-direction: column;
		gap: 0.75rem;
	}
	.insight-card {
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
		background: #fafafa;
	}
	.insight-card.saved {
		border-color: #2563eb;
		background: #eff6ff;
	}
	.insight-card.read {
		opacity: 0.8;
	}
	.insight-header {
		display: flex;
		justify-content: space-between;
		margin-bottom: 0.5rem;
	}
	.insight-type {
		font-weight: 600;
		font-size: 0.85rem;
		color: #374151;
	}
	.insight-status {
		font-size: 0.75rem;
		color: #9ca3af;
	}
	.insight-message {
		font-size: 0.9rem;
		color: #4b5563;
		margin: 0;
	}
	.insight-data {
		font-size: 0.8rem;
		color: #6b7280;
		margin: 0.5rem 0 0;
	}
	.insight-actions {
		display: flex;
		gap: 0.5rem;
		margin-top: 0.75rem;
	}
	.action-btn {
		padding: 0.25rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 4px;
		background: white;
		cursor: pointer;
		font-size: 0.8rem;
	}
	.action-btn:hover {
		background: #f3f4f6;
	}
	.action-btn.save {
		border-color: #2563eb;
		color: #2563eb;
	}
	.action-btn.dismiss {
		color: #9ca3af;
	}
	.error {
		color: #dc2626;
		padding: 0.75rem;
		background: #fef2f2;
		border-radius: 4px;
	}
</style>
