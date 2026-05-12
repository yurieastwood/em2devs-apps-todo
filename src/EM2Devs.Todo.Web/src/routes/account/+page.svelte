<script lang="ts">
	import { enhance } from '$app/forms';
	import { resolve } from '$app/paths';
	import type { ActionData, PageData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();
	let confirmationInput = $state('');
	let dangerOpen = $state(false);
	let importOpen = $state(false);
	let importConfirmation = $state('');
	let selectedFile = $state<File | null>(null);
	let summary = $state<{
		tasks: number;
		quests: number;
		epics: number;
		weeklyReviews: number;
		insightCards: number;
		timelineEvents: number;
	} | null>(null);

	async function onFileChange(event: Event) {
		const input = event.target as HTMLInputElement;
		const file = input.files?.[0] ?? null;
		selectedFile = file;
		summary = null;
		if (!file) return;
		try {
			const text = await file.text();
			const parsed = JSON.parse(text) as Record<string, unknown>;
			summary = {
				tasks: Array.isArray(parsed.tasks) ? parsed.tasks.length : 0,
				quests: Array.isArray(parsed.quests) ? parsed.quests.length : 0,
				epics: Array.isArray(parsed.epics) ? parsed.epics.length : 0,
				weeklyReviews: Array.isArray(parsed.weeklyReviews)
					? parsed.weeklyReviews.length
					: 0,
				insightCards: Array.isArray(parsed.insightCards) ? parsed.insightCards.length : 0,
				timelineEvents: Array.isArray(parsed.timelineEvents)
					? parsed.timelineEvents.length
					: 0
			};
		} catch {
			summary = null;
		}
	}
</script>

<svelte:head>
	<title>Account | Waypoint</title>
</svelte:head>

<main>
	<h1>Account</h1>

	<section class="card" data-testid="account-info">
		<h2>Profile</h2>
		<dl>
			<dt>Display name</dt>
			<dd>{data.user.displayName}</dd>
			<dt>Email</dt>
			<dd>{data.user.email}</dd>
		</dl>
	</section>

	<section class="card">
		<h2>Data Export</h2>
		<p>
			Download a JSON snapshot of everything in your Waypoint account: tasks, quests, epics,
			progression, weekly reviews, timeline events, and more.
		</p>
		<a
			class="btn btn-primary"
			href={resolve('/account/export.json')}
			data-testid="export-download"
		>
			Download my data (JSON)
		</a>
	</section>

	<section class="card">
		<h2>Data Import</h2>
		<button
			type="button"
			class="btn btn-toggle"
			onclick={() => (importOpen = !importOpen)}
			data-testid="import-toggle"
		>
			{importOpen ? 'Hide' : 'Show'} data import
		</button>

		{#if importOpen}
			<div class="import-body">
				<p class="warning warning-amber">
					<strong>Warning:</strong> Importing will <em>overwrite</em> all your current data
					— every task, quest, epic, weekly review, insight, timeline event, and progression
					state — with the contents of the uploaded file. Export your current data first if
					you want a fallback.
				</p>

				<form
					method="POST"
					action="?/import"
					enctype="multipart/form-data"
					use:enhance={() => {
						return async ({ update }) => {
							await update();
						};
					}}
				>
					<label for="file">Select a Waypoint JSON export file:</label>
					<input
						id="file"
						name="file"
						type="file"
						accept="application/json,.json"
						required
						onchange={onFileChange}
						data-testid="import-file"
					/>

					{#if summary}
						<dl class="summary" data-testid="import-summary">
							<dt>Tasks</dt>
							<dd>{summary.tasks}</dd>
							<dt>Quests</dt>
							<dd>{summary.quests}</dd>
							<dt>Epics</dt>
							<dd>{summary.epics}</dd>
							<dt>Weekly reviews</dt>
							<dd>{summary.weeklyReviews}</dd>
							<dt>Insight cards</dt>
							<dd>{summary.insightCards}</dd>
							<dt>Timeline events</dt>
							<dd>{summary.timelineEvents}</dd>
						</dl>
					{/if}

					<label for="importConfirmation">
						Type <code>OVERWRITE MY DATA</code> to confirm:
					</label>
					<input
						id="importConfirmation"
						name="importConfirmation"
						type="text"
						autocomplete="off"
						required
						bind:value={importConfirmation}
						data-testid="import-confirmation"
					/>

					{#if form?.importError}
						<p class="error" data-testid="import-error">{form.importError}</p>
					{/if}
					{#if form?.importSuccess}
						<p class="success" data-testid="import-success">{form.importSuccess}</p>
					{/if}

					<button
						type="submit"
						class="btn btn-primary"
						disabled={!selectedFile || importConfirmation !== 'OVERWRITE MY DATA'}
						data-testid="import-submit"
					>
						Import &amp; overwrite
					</button>
				</form>
			</div>
		{/if}
	</section>

	<section class="card danger">
		<h2>Danger Zone</h2>
		<button
			type="button"
			class="btn btn-toggle"
			onclick={() => (dangerOpen = !dangerOpen)}
			data-testid="danger-toggle"
		>
			{dangerOpen ? 'Hide' : 'Show'} account deletion
		</button>

		{#if dangerOpen}
			<div class="danger-body">
				<p class="warning">
					<strong>Warning:</strong> Deleting your account permanently erases all your tasks,
					quests, epics, progression, weekly reviews, timeline events, and notifications. Your
					account will be deactivated immediately. You can recover it by signing in within the
					next 30 days; after that, the email is released and your data is gone for good.
				</p>
				<p>
					<strong>Export your data first</strong> if you want to keep a copy — use the button
					in the Data Export section above.
				</p>

				<form
					method="POST"
					action="?/delete"
					use:enhance={() => {
						return async ({ result, update }) => {
							if (result.type === 'redirect') {
								window.location.href = result.location;
								return;
							}
							await update();
						};
					}}
				>
					<label for="confirmation">
						Type <code>DELETE MY ACCOUNT</code> to confirm:
					</label>
					<input
						id="confirmation"
						name="confirmation"
						type="text"
						autocomplete="off"
						required
						bind:value={confirmationInput}
						data-testid="confirmation-input"
					/>

					{#if form?.error}
						<p class="error" data-testid="delete-error">{form.error}</p>
					{/if}

					<button
						type="submit"
						class="btn btn-danger"
						disabled={confirmationInput !== 'DELETE MY ACCOUNT'}
						data-testid="delete-submit"
					>
						Delete my account
					</button>
				</form>
			</div>
		{/if}
	</section>
</main>

<style>
	main {
		max-width: 640px;
		margin: 2rem auto;
		padding: 0 1rem;
		font-family: system-ui, sans-serif;
	}

	h1 {
		font-size: 1.5rem;
		margin-bottom: 1.5rem;
	}

	.card {
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
		padding: 1.25rem;
		margin-bottom: 1rem;
	}

	.card h2 {
		font-size: 1.125rem;
		margin: 0 0 0.75rem;
	}

	dl {
		display: grid;
		grid-template-columns: max-content 1fr;
		column-gap: 1rem;
		row-gap: 0.5rem;
		margin: 0;
	}

	dt {
		font-weight: 500;
		color: #6b7280;
	}

	dd {
		margin: 0;
	}

	.danger {
		border-color: #fecaca;
	}

	.danger h2 {
		color: #b91c1c;
	}

	.danger-body {
		margin-top: 0.75rem;
		padding-top: 0.75rem;
		border-top: 1px dashed #fecaca;
	}

	.warning {
		background: #fef2f2;
		border-left: 3px solid #b91c1c;
		padding: 0.75rem;
		margin-bottom: 1rem;
	}

	.warning-amber {
		background: #fffbeb;
		border-left-color: #d97706;
		color: #78350f;
	}

	.import-body {
		margin-top: 0.75rem;
		padding-top: 0.75rem;
		border-top: 1px dashed #e5e7eb;
	}

	.summary {
		margin: 0.75rem 0;
		padding: 0.5rem 0.75rem;
		background: #f9fafb;
		border-radius: 4px;
	}

	.success {
		color: #065f46;
		margin-top: 0.5rem;
	}

	label {
		display: block;
		margin-top: 1rem;
		font-weight: 500;
	}

	code {
		background: #f3f4f6;
		padding: 0.1rem 0.3rem;
		border-radius: 3px;
		font-family: ui-monospace, monospace;
	}

	input {
		width: 100%;
		padding: 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 4px;
		margin-top: 0.25rem;
	}

	.error {
		color: #b91c1c;
		margin-top: 0.5rem;
	}

	.btn {
		display: inline-block;
		padding: 0.5rem 1rem;
		border-radius: 4px;
		border: 1px solid transparent;
		cursor: pointer;
		font: inherit;
		text-decoration: none;
	}

	.btn-primary {
		background: #2563eb;
		color: white;
	}

	.btn-toggle {
		background: white;
		color: #6b7280;
		border-color: #d1d5db;
	}

	.btn-danger {
		background: #b91c1c;
		color: white;
		margin-top: 1rem;
	}

	.btn-danger:disabled {
		background: #fca5a5;
		cursor: not-allowed;
	}
</style>
