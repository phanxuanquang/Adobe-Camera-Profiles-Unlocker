// Fills the download buttons from the latest GitHub release.
// Every link already points at /releases/latest, so a failure here never blocks a download.
const API = 'https://api.github.com/repos/phanxuanquang/Adobe-Camera-Profiles-Unlocker/releases/latest';
const PAGE = 'https://github.com/phanxuanquang/Adobe-Camera-Profiles-Unlocker/releases/latest';

const sizeFormat = new Intl.NumberFormat('en', { style: 'unit', unit: 'megabyte', maximumFractionDigits: 0 });
const dateFormat = new Intl.DateTimeFormat('en', { dateStyle: 'long' });

// Only Chromium exposes the CPU architecture; everyone else gets x64, the common case.
async function detectArch() {
  try {
    const { architecture } = await navigator.userAgentData.getHighEntropyValues(['architecture']);
    return architecture === 'arm' ? 'arm64' : 'x64';
  } catch {
    return 'x64';
  }
}

function setStatus(status, text, link) {
  status.querySelector('.spinner-border')?.remove();
  const span = status.querySelector('span');
  span.textContent = text;
  if (link) {
    const a = Object.assign(document.createElement('a'), { href: PAGE, textContent: link });
    span.append(a, '.');
  }
}

export async function initRelease() {
  const section = document.getElementById('download');
  const status = document.getElementById('release-status');
  const [arch, release] = await Promise.all([
    detectArch(),
    fetch(API, { headers: { Accept: 'application/vnd.github+json' }, signal: AbortSignal.timeout(8000) })
      .then((r) => (r.ok ? r.json() : Promise.reject(new Error(`HTTP ${r.status}`))))
      .catch((error) => ({ error })),
  ]);

  // Mark the recommended button and keep it first.
  const recommended = document.querySelector(`#downloads [data-download="${arch}"]`);
  recommended.classList.replace('btn-light', 'btn-primary');
  recommended.querySelector('[data-recommended]').hidden = false;
  recommended.parentElement.prepend(recommended);

  const assets = release.error ? [] : release.assets ?? [];
  const asset = (a) => assets.find((x) => x.name.endsWith(`win-${a}.zip`));

  for (const link of document.querySelectorAll('[data-download]')) {
    const file = asset(link.dataset.download);
    if (!file) continue;
    link.href = file.browser_download_url;
    const size = link.querySelector('[data-size]');
    if (size) size.textContent = `(${sizeFormat.format(Math.round(file.size / 1e6))} zip)`;
  }

  if (release.error || !asset('x64') || !asset('arm64')) {
    setStatus(status, "Couldn't load the file list. The buttons below open the ", 'latest release on GitHub');
  } else {
    const version = release.tag_name.replace(/^v/, '');
    setStatus(status, `Version ${version}, released ${dateFormat.format(new Date(release.published_at))}.`);
  }
  section.setAttribute('aria-busy', 'false');
}
