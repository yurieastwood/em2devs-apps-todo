<script lang="ts">
	import { enhance } from '$app/forms';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let epic = $derived(data.epic);
	let availableQuests = $derived(data.availableQuests);

	let actionInFlight = $state<string | null>(null);
	let notification = $state<string | null>(null);
	let selectedQuestId = $state('');

	let actionError = $derived(form?.error ? String(form.error) : null);

	$effect(() => {
		if (actionError) {
			notification = actionError;
			const timer = setTimeout(() => (notification = null), 4000);
			return () => clearTimeout(timer);
		}
	});
</script>

<svelte:head>
	<title>{epic.title} | Epics | EM2Devs Todo</title>
</svelte:head>

<main>
	{#if notification}
		<div class="notification" role="alert">
			{notification}
			<button type="button" onclick={() => (notification = null)}>Dismiss</button>
		</div>
	{/if}

	<header>
		<h1 class:completed={epic.isCompleted}>{epic.title}</h1>
		{#if epic.description}
			<p class="description">{epic.description}</p>
		{/if}
		<div class="meta">
			<div class="progress-section">
				<span class="progress-label">{Math.round(epic.progress)}% complete</span>
				<div class="progress-bar">
					<div
						class="progress-fill"
						style:width="{epic.progress}%"
						class:complete={epic.isCompleted}
					></div>
				</div>
			</div>
			{#if epic.targetDate}
				<span class="target-date">Target: {epic.targetDate}</span>
			{/if}
		</div>
	</header>

	<section>
		<h2>Quests ({epic.quests.length})</h2>

		{#if availableQuests.length > 0}
			<form
				method="POST"
				action="?/assignQuest"
				use:enhance={() => {
					actionInFlight = 'assign';
					return async ({ update, result }) => {
						try {
							await update();
						} finally {
							actionInFlight = null;
							if (result.type === 'success') selectedQuestId = '';
						}
					};
				}}
				class="assign-form"
			>
				<select
					name="questId"
					bind:value={selectedQuestId}
					disabled={actionInFlight === 'assign'}
				>
					<option value="">Select a quest to assign...</option>
					{#each availableQuests as quest (quest.id)}
						<option value={quest.id}>{quest.title} ({quest.progress}%)</option>
					{/each}
				</select>
				<button
					type="submit"
					disabled={actionInFlight === 'assign' || !selectedQuestId}
					data-testid="assign-quest-button"
				>
					{actionInFlight === 'assign' ? 'Assigning...' : 'Assign'}
				</button>
			</form>
		{/if}

		{#if epic.quests.length === 0}
			<p class="empty">No quests assigned yet.</p>
		{:else}
			<ul class="quest-list">
				{#each epic.quests as quest (quest.id)}
					<li class="quest-item">
						<div class="quest-info">
							<span class="quest-title">{quest.title}</span>
							<div class="quest-progress-bar">
								<div
									class="quest-progress-fill"
									style:width="{quest.progress}%"
								></div>
							</div>
							<span class="quest-progress-text">{quest.progress}%</span>
						</div>
						<form
							method="POST"
							action="?/removeQuest"
							use:enhance={() => {
								actionInFlight = quest.id;
								return async ({ update }) => {
									try {
										await update();
									} finally {
										actionInFlight = null;
									}
								};
							}}
						>
							<input type="hidden" name="questId" value={quest.id} />
							<button
								type="submit"
								class="btn-remove"
								disabled={actionInFlight === quest.id}
							>
								{actionInFlight === quest.id ? '...' : 'Remove'}
							</button>
						</form>
					</li>
				{/each}
			</ul>
		{/if}
	</section>

	<footer class="actions">
		{#if !epic.isCompleted}
			<form
				method="POST"
				action="?/complete"
				use:enhance={() => {
					actionInFlight = 'complete';
					return async ({ update }) => {
						try {
							await update();
						} finally {
							actionInFlight = null;
						}
					};
				}}
			>
				<button
					type="submit"
					class="btn-complete"
					disabled={actionInFlight === 'complete'}
					data-testid="complete-epic-button"
				>
					{actionInFlight === 'complete' ? 'Completing...' : 'Complete Epic'}
				</button>
			</form>
		{/if}
		<form
			method="POST"
			action="?/delete"
			use:enhance={() => {
				actionInFlight = 'delete';
				return async ({ update }) => {
					try {
						await update();
					} finally {
						actionInFlight = null;
					}
				};
			}}
		>
			<button
				type="submit"
				class="btn-delete"
				disabled={actionInFlight === 'delete'}
				data-testid="delete-epic-button"
			>
				{actionInFlight === 'delete' ? 'Deleting...' : 'Delete Epic'}
			</button>
		</form>
	</footer>
</main>

<style>
	main {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}

	.notification {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		margin-bottom: 1rem;
		background: #fef2f2;
		border: 1px solid #fca5a5;
		border-radius: 0.25rem;
		color: #991b1b;
	}

	.notification button {
		background: none;
		border: none;
		color: #991b1b;
		cursor: pointer;
		font-weight: 600;
	}

	header {
		margin-bottom: 2rem;
	}

	h1 {
		margin-bottom: 0.5rem;
	}

	h1.completed {
		text-decoration: line-through;
		color: #9ca3af;
	}

	.description {
		color: #6b7280;
		margin-bottom: 1rem;
	}

	.meta {
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.progress-section {
		display: flex;
		align-items: center;
		gap: 0.75rem;
	}

	.progress-label {
		font-size: 0.875rem;
		font-weight: 600;
		color: #374151;
		min-width: 6rem;
	}

	.progress-bar {
		flex: 1;
		height: 0.75rem;
		background: #e5e7eb;
		border-radius: 0.375rem;
		overflow: hidden;
	}

	.progress-fill {
		height: 100%;
		background: #2563eb;
		border-radius: 0.375rem;
		transition: width 0.3s ease;
	}

	.progress-fill.complete {
		background: #059669;
	}

	.target-date {
		font-size: 0.875rem;
		color: #6b7280;
	}

	h2 {
		margin-bottom: 1rem;
	}

	.assign-form {
		display: flex;
		gap: 0.5rem;
		margin-bottom: 1rem;
	}

	.assign-form select {
		flex: 1;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 0.875rem;
	}

	.assign-form button {
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.assign-form button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.empty {
		color: #6b7280;
		font-style: italic;
	}

	.quest-list {
		list-style: none;
		padding: 0;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.quest-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
	}

	.quest-info {
		display: flex;
		align-items: center;
		gap: 0.75rem;
		flex: 1;
		min-width: 0;
	}

	.quest-title {
		font-weight: 500;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.quest-progress-bar {
		width: 60px;
		height: 0.375rem;
		background: #e5e7eb;
		border-radius: 0.25rem;
		overflow: hidden;
		flex-shrink: 0;
	}

	.quest-progress-fill {
		height: 100%;
		background: #2563eb;
		border-radius: 0.25rem;
	}

	.quest-progress-text {
		font-size: 0.75rem;
		color: #6b7280;
		flex-shrink: 0;
	}

	.btn-remove {
		padding: 0.25rem 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		cursor: pointer;
		font-size: 0.75rem;
		background: white;
		margin-left: 0.75rem;
	}

	.btn-remove:hover {
		background: #fef2f2;
		border-color: #dc2626;
		color: #dc2626;
	}

	.btn-remove:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.actions {
		display: flex;
		gap: 0.75rem;
		margin-top: 2rem;
		padding-top: 1.5rem;
		border-top: 1px solid #e5e7eb;
	}

	.btn-complete {
		padding: 0.5rem 1rem;
		background: #059669;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.btn-complete:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.btn-delete {
		padding: 0.5rem 1rem;
		background: white;
		color: #dc2626;
		border: 1px solid #dc2626;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.btn-delete:hover {
		background: #fef2f2;
	}

	.btn-delete:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
</style>
