import { initRelease } from './release.js';
import { initLightbox } from './lightbox.js';

// Module scripts run after the deferred CDN scripts, so lucide and anime are ready (or blocked).
globalThis.lucide?.createIcons();
initLightbox();
initRelease();
