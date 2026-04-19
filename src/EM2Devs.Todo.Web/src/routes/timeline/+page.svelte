<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let timeline = $derived(data.timeline);
	let error = $derived(data.error);

	const eventTypeIcons: Record<string, string> = {
		LevelUp: '⬆️',
		QuestCompleted: '🏆',
		EpicCompleted: '⭐',
		BossTaskDefeated: '🗡️',
		TitleEarned: '🎖️',
		SkillTreeUnlocked: '🌳',
		StreakMilestone: '🔥',
		WeeklyReviewStreakMilestone: '📅'
	};

	function formatDate(iso: string): string {
		return new Date(iso).toLocaleDateString('en-GB', {
			day: 'numeric',
			month: 'short',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
</script>

<svelte:head>
	<title>Timeline — Waypoint</title>
</svelte:head>

<div class="timeline-page">
	<h1>Journey Timeline</h1>

	{#if error}
		<p class="error">{error}</p>
	{/if}

	{#if timeline.events.length === 0}
		<div class="empty-state">
			<p class="empty-icon">🗺️</p>
			<p>Your journey is just beginning.</p>
			<p class="empty-hint">
				Complete tasks, earn XP, level up, and hit streak milestones to see events appear
				here.
			</p>
		</div>
	{:else}
		<div class="event-list">
			{#each timeline.events as event (event.id)}
				<div class="event-card">
					<span class="event-icon">{eventTypeIcons[event.eventType] ?? '📌'}</span>
					<div class="event-content">
						<span class="event-type"
							>{event.eventType.replace(/([A-Z])/g, ' $1').trim()}</span
						>
						<p class="event-details">{event.details}</p>
						{#if event.note}
							<p class="event-note">📝 {event.note}</p>
						{/if}
					</div>
					<time class="event-time">{formatDate(event.occurredAt)}</time>
				</div>
			{/each}
		</div>

		{#if timeline.hasMore}
			<p class="load-more-hint">Scroll or navigate to load more events.</p>
		{/if}
	{/if}
</div>

<style>
	.timeline-page {
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

	.event-list {
		display: flex;
		flex-direction: column;
		gap: 0.75rem;
	}

	.event-card {
		display: flex;
		align-items: flex-start;
		gap: 0.75rem;
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
		background: #fafafa;
	}

	.event-icon {
		font-size: 1.5rem;
		flex-shrink: 0;
	}

	.event-content {
		flex: 1;
		min-width: 0;
	}

	.event-type {
		font-weight: 600;
		font-size: 0.85rem;
		color: #374151;
	}

	.event-details {
		margin: 0.25rem 0 0;
		font-size: 0.9rem;
		color: #4b5563;
	}

	.event-note {
		margin: 0.5rem 0 0;
		font-size: 0.8rem;
		color: #6b7280;
		font-style: italic;
	}

	.event-time {
		font-size: 0.75rem;
		color: #9ca3af;
		white-space: nowrap;
		flex-shrink: 0;
	}

	.error {
		color: #dc2626;
		padding: 0.75rem;
		background: #fef2f2;
		border-radius: 4px;
	}

	.load-more-hint {
		text-align: center;
		color: #9ca3af;
		font-size: 0.85rem;
		padding: 1rem;
	}
</style>
