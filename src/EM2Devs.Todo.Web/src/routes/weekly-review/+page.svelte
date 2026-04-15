<script lang="ts">
	import { enhance } from '$app/forms';
	import type { PageData, ActionData } from './$types';

	let { data, form }: { data: PageData; form: ActionData } = $props();

	let review = $derived(data.review);
	let existingReflection = $derived(review?.reflection ?? null);

	// Prefer the latest saved reflection returned by the form action, then the
	// persisted one from the loader, then the in-progress form values.
	let justSaved = $derived(form && 'reflection' in form ? form.reflection : null);
	let effectiveReflection = $derived(justSaved ?? existingReflection);

	let wentWell = $state('');
	let dragged = $state('');
	let adjustment = $state('');

	$effect(() => {
		if (effectiveReflection) {
			wentWell = effectiveReflection.whatWentWell;
			dragged = effectiveReflection.whatDragged;
			adjustment = effectiveReflection.adjustment;
		} else if (form && 'whatWentWell' in form) {
			wentWell = (form.whatWentWell as string) ?? '';
			dragged = (form.whatDragged as string) ?? '';
			adjustment = (form.adjustment as string) ?? '';
		}
	});
</script>

<svelte:head>
	<title>Weekly review</title>
</svelte:head>

<main>
	<h1>Weekly review</h1>

	{#if data.error}
		<p class="error" data-testid="weekly-review-error">{data.error}</p>
	{:else if review}
		<section class="summary" data-testid="weekly-review-summary">
			<h2>Week of {review.weekOf}</h2>
			<dl>
				<div>
					<dt>Tasks completed</dt>
					<dd data-testid="summary-tasks-completed">{review.tasksCompleted}</dd>
				</div>
				<div>
					<dt>XP earned</dt>
					<dd data-testid="summary-xp-earned">{review.xpEarned}</dd>
				</div>
				<div>
					<dt>Streak start → end</dt>
					<dd data-testid="summary-streak">{review.streakStart} → {review.streakEnd}</dd>
				</div>
			</dl>

			{#if review.notableEvents.length > 0}
				<h3>Notable events</h3>
				<ul data-testid="notable-events">
					{#each review.notableEvents as event (event)}
						<li>{event}</li>
					{/each}
				</ul>
			{/if}
		</section>

		<form method="POST" action="?/save" use:enhance data-testid="weekly-review-form">
			<input type="hidden" name="weekOf" value={review.weekOf} />

			<label>
				What went well?
				<textarea
					name="whatWentWell"
					rows="4"
					bind:value={wentWell}
					data-testid="field-went-well"
					required
				></textarea>
			</label>

			<label>
				What dragged?
				<textarea
					name="whatDragged"
					rows="4"
					bind:value={dragged}
					data-testid="field-dragged"
					required
				></textarea>
			</label>

			<label>
				One thing to adjust?
				<textarea
					name="adjustment"
					rows="4"
					bind:value={adjustment}
					data-testid="field-adjustment"
					required
				></textarea>
			</label>

			{#if form && 'saveError' in form && form.saveError}
				<p class="error" data-testid="save-error">{form.saveError}</p>
			{/if}

			<button type="submit" data-testid="save-button">
				{effectiveReflection ? 'Update reflection' : 'Save reflection'}
			</button>

			{#if effectiveReflection}
				<p class="saved" data-testid="saved-at">
					Last saved {new Date(effectiveReflection.savedAt).toLocaleString()}
				</p>
			{/if}
		</form>
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

	.summary {
		background: #f9fafb;
		border: 1px solid #e5e7eb;
		border-radius: 0.5rem;
		padding: 1rem 1.25rem;
		margin-bottom: 1.5rem;
	}

	.summary h2 {
		margin-top: 0;
		font-size: 1.125rem;
	}

	.summary dl {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
		gap: 0.75rem;
		margin: 0.75rem 0;
	}

	.summary dt {
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.05em;
		color: #6b7280;
	}

	.summary dd {
		margin: 0;
		font-size: 1.5rem;
		font-weight: 600;
		color: #111827;
	}

	form {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
		font-weight: 500;
	}

	textarea {
		font: inherit;
		padding: 0.5rem;
		border: 1px solid #d1d5db;
		border-radius: 0.25rem;
		resize: vertical;
	}

	button {
		align-self: flex-start;
		padding: 0.5rem 1rem;
		background: #2563eb;
		color: white;
		border: none;
		border-radius: 0.25rem;
		cursor: pointer;
	}

	button:hover {
		background: #1d4ed8;
	}

	.error {
		color: #b91c1c;
	}

	.saved {
		font-size: 0.875rem;
		color: #6b7280;
	}
</style>
