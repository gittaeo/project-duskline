mergeInto(LibraryManager.library, {
  DusklineTestReport: function(message) {
    var panel = document.getElementById('duskline-test-report');
    if (!panel) {
      panel = document.createElement('pre');
      panel.id = 'duskline-test-report';
      panel.setAttribute('role', 'status');
      panel.style.cssText = 'position:fixed;z-index:999;top:60px;left:20px;max-width:90vw;padding:18px;background:#082722;color:#8affe0;white-space:pre-wrap;font:16px monospace';
      document.body.appendChild(panel);
    }
    panel.textContent = UTF8ToString(message);
  }
});
