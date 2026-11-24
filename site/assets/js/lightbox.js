import { popIn } from './motion.js';

// Any [data-zoom] button opens its screenshot full size in a native modal <dialog>.
// The dialog gives Esc to close, focus trapping and focus return for free.
export function initLightbox() {
  const dialog = document.getElementById('lightbox');
  const big = dialog.querySelector('img');

  document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-zoom]');
    if (!button) return;
    const small = button.querySelector('img');
    big.src = small.currentSrc || small.src;
    big.alt = small.alt;
    dialog.showModal();
    popIn(dialog);
  });

  // A click on the backdrop lands on the dialog itself, not on its content.
  dialog.addEventListener('click', (event) => { if (event.target === dialog) dialog.close(); });
}
