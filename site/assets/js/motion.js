// The only animation: a dialog scales in the way a WinUI ContentDialog opens.
// It answers a click, and is skipped when the visitor asks for reduced motion.
export function popIn(element) {
  const anime = globalThis.anime;
  if (!anime || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
  anime.animate(element, {
    opacity: { from: 0, to: 1 },
    scale: { from: 1.05, to: 1 },
    duration: 167,
    ease: 'outQuad',
  });
}
