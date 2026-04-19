<script lang="ts">
	import type { PageData } from './$types';

	let { data }: { data: PageData } = $props();
	let wrapped = $derived(data.wrapped);
	let error = $derived(data.error);
	let currentSlide = $state(0);

	function next() {
		if (wrapped && currentSlide < wrapped.slides.length - 1) currentSlide++;
	}
	function prev() {
		if (currentSlide > 0) currentSlide--;
	}
</script>

<svelte:head>
	<title>Wrapped — Waypoint</title>
</svelte:head>

<div class="wrapped-page">
	{#if error}
		<p class="error">{error}</p>
	{/if}

	{#if wrapped}
		<div class="wrapped-container">
			<h1>{wrapped.year} Wrapped {wrapped.isPartialYear ? '(Year So Far)' : ''}</h1>

			{#if wrapped.slides.length > 0}
				{@const slide = wrapped.slides[currentSlide]}
				<div class="slide">
					<h2 class="slide-title">{slide.title}</h2>
					<p class="slide-metric">{slide.metric}</p>
				</div>

				<div class="slide-nav">
					<button onclick={prev} disabled={currentSlide === 0} class="nav-btn"
						>Previous</button
					>
					<span class="slide-counter">{currentSlide + 1} / {wrapped.slides.length}</span>
					<button
						onclick={next}
						disabled={currentSlide === wrapped.slides.length - 1}
						class="nav-btn">Next</button
					>
				</div>
			{:else}
				<p class="empty">No slides available for this year.</p>
			{/if}
		</div>
	{:else if !error}
		<p class="empty">Wrapped not available yet.</p>
	{/if}
</div>

<style>
	.wrapped-page {
		max-width: 480px;
		margin: 2rem auto;
		padding: 0 1rem;
		text-align: center;
	}
	h1 {
		font-size: 1.3rem;
		margin-bottom: 2rem;
		color: #374151;
	}
	.wrapped-container {
		min-height: 400px;
		display: flex;
		flex-direction: column;
		justify-content: center;
	}
	.slide {
		padding: 3rem 2rem;
		border-radius: 16px;
		background: linear-gradient(135deg, #1e3a5f, #2563eb);
		color: white;
		margin-bottom: 1.5rem;
		min-height: 200px;
		display: flex;
		flex-direction: column;
		justify-content: center;
	}
	.slide-title {
		font-size: 1rem;
		font-weight: 400;
		opacity: 0.9;
		margin: 0 0 1rem;
		text-transform: uppercase;
		letter-spacing: 0.1em;
	}
	.slide-metric {
		font-size: 2rem;
		font-weight: 700;
		margin: 0;
	}
	.slide-nav {
		display: flex;
		align-items: center;
		justify-content: space-between;
	}
	.nav-btn {
		padding: 0.5rem 1rem;
		border: 1px solid #d1d5db;
		border-radius: 6px;
		background: white;
		cursor: pointer;
	}
	.nav-btn:disabled {
		opacity: 0.3;
		cursor: not-allowed;
	}
	.slide-counter {
		font-size: 0.85rem;
		color: #6b7280;
	}
	.error {
		color: #dc2626;
		padding: 0.75rem;
		background: #fef2f2;
		border-radius: 4px;
	}
	.empty {
		color: #9ca3af;
		padding: 3rem;
	}
</style>
