// Minimal browser interop for Stringy. No analytics, no network calls — everything stays local.
window.stringy = {
  getTheme: function () {
    try { return localStorage.getItem('stringy-theme'); } catch (e) { return null; }
  },
  setTheme: function (theme) {
    try { localStorage.setItem('stringy-theme', theme); } catch (e) { /* private mode */ }
    document.documentElement.setAttribute('data-theme', theme);
  },
  prefersDark: function () {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
  },
  paste: async function () {
    try {
      // Race a timeout: some contexts leave the permission promise pending forever,
      // which would hang the caller. A slow "Allow" still works on the next click.
      return await Promise.race([
        navigator.clipboard.readText(),
        new Promise((resolve) => setTimeout(() => resolve(null), 5000))
      ]);
    } catch (e) {
      // Blocked by permissions or unsupported (e.g. Firefox without a paste prompt).
      return null;
    }
  },
  copy: async function (text) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch (e) {
      // Fallback for browsers/contexts without the async clipboard API.
      try {
        const ta = document.createElement('textarea');
        ta.value = text;
        ta.style.position = 'fixed';
        ta.style.opacity = '0';
        document.body.appendChild(ta);
        ta.select();
        const ok = document.execCommand('copy');
        document.body.removeChild(ta);
        return ok;
      } catch (e2) {
        return false;
      }
    }
  }
};
