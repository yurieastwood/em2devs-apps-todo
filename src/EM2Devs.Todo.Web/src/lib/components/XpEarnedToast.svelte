<script lang="ts">
	let { delta }: { delta: number } = $props();
	let visible = $state(false);

	$effect(() => {
		if (delta > 0) {
			visible = true;
			const timer = setTimeout(() => (visible = false), 2500);
			return () => clearTimeout(timer);
		}
	});
</script>

{#if visible}
	<div class="toast" role="status" aria-live="polite" data-testid="xp-toast">
		+{delta.toLocaleString()} XP
	</div>
{/if}

<style>
	.toast {
		position: fixed;
		bottom: 2rem;
		left: 50%;
		transform: translateX(-50%);
		padding: 0.75rem 1.5rem;
		background: linear-gradient(135deg, #2563eb, #7c3aed);
		color: white;
		border-radius: 9999px;
		font-weight: 700;
		font-size: 1.125rem;
		box-shadow: 0 4px 12px rgba(37, 99, 235, 0.35);
		animation: rise 2.5s ease-out forwards;
	}

	@keyframes rise {
		0% {
			opacity: 0;
			transform: translate(-50%, 1rem);
		}
		15% {
			opacity: 1;
			transform: translate(-50%, 0);
		}
		85% {
			opacity: 1;
			transform: translate(-50%, 0);
		}
		100% {
			opacity: 0;
			transform: translate(-50%, -1rem);
		}
	}
</style>
