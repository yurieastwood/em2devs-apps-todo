<script lang="ts">
	import { enhance } from '$app/forms';
	import { resolve } from '$app/paths';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let quests = $derived(data.quests);
	let loadError = $derived(data.error);
	let newTitle = $state('');
	let newDescription = $state('');
	let creating = $state(false);

	let createError = $derived(
		form?.action === 'create' && form?.error ? String(form.error) : null
	);
</script>

<svelte:head>
	<title>Quests | EM2Devs Todo</title>
</svelte:head>

<main>
	<h1>Quests</h1>

	<form
		method="POST"
		action="?/create"
		use:enhance={() => {
			creating = true;
			return async ({ update, result }) => {
				try {
					await update();
				} finally {
					creating = false;
					if (result.type === 'success') {
						newTitle = '';
						newDescription = '';
					}
				}
			};
		}}
		class="create-form"
	>
		<input
			type="text"
			name="title"
			bind:value={newTitle}
			placeholder="Quest title"
			disabled={creating}
			maxlength={200}
		/>
		<input
			type="text"
			name="description"
			bind:value={newDescription}
			placeholder="Description"
			disabled={creating}
		/>
		<button type="submit" disabled={creating || !newTitle.trim()}>
			{creating ? 'Creating...' : 'Create Quest'}
		</button>
		{#if createError}
			<p class="form-error" role="alert">{createError}</p>
		{/if}
	</form>

	{#if loadError}
		<p class="error" role="alert">{loadError}</p>
	{:else if quests.length === 0}
		<p class="empty">No quests yet. Create your first quest to group related tasks!</p>
	{:else}
		<ul class="quest-list">
			{#each quests as quest (quest.id)}
				<li class="quest-item">
					<a href={resolve(`/quests/${quest.id}`)}>
						<div class="quest-info">
							<span class="quest-title">{quest.title}</span>
							<span class="quest-desc">{quest.description}</span>
						</div>
						<div class="quest-progress">
							<span class="progress-text">{quest.progress}%</span>
							<div class="progress-bar">
								<div class="progress-fill" style="width: {quest.progress}%"></div>
							</div>
						</div>
					</a>
				</li>
			{/each}
		</ul>
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

	.create-form {
		display: flex;
		flex-wrap: wrap;
		gap: 0.5rem;
		margin-bottom: 1.5rem;
	}

	.create-form input {
		flex: 1;
		min-width: 150px;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		font-size: 1rem;
	}

	.create-form button {
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
		font-weight: 500;
	}

	.create-form button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.form-error {
		width: 100%;
		color: #dc2626;
		font-size: 0.875rem;
		margin: 0;
	}

	.error {
		color: #dc2626;
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

	.quest-item a {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.25rem;
		text-decoration: none;
		color: inherit;
	}

	.quest-item a:hover {
		border-color: #2563eb;
	}

	.quest-info {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}

	.quest-title {
		font-weight: 500;
	}

	.quest-desc {
		font-size: 0.875rem;
		color: #6b7280;
	}

	.quest-progress {
		text-align: right;
		min-width: 80px;
	}

	.progress-text {
		font-size: 0.875rem;
		font-weight: 600;
		color: #374151;
	}

	.progress-bar {
		height: 4px;
		background: #e5e7eb;
		border-radius: 2px;
		margin-top: 0.25rem;
	}

	.progress-fill {
		height: 100%;
		background: #2563eb;
		border-radius: 2px;
		transition: width 0.3s;
	}
</style>
