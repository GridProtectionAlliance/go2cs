(() => {
  "use strict";

  let currentEditor = null;
  let lastSource = null;
  let scheduled = false;

  function editor() {
    const element = document.querySelector(".CodeMirror");
    return element && element.CodeMirror ? element.CodeMirror : null;
  }

  function lessonTitle() {
    const candidates = [
      ".lesson-title",
      ".page h2",
      "article h1",
      "article h2",
      "h1",
      "h2"
    ];
    for (const selector of candidates) {
      const value = document.querySelector(selector)?.textContent?.trim();
      if (value && value.length < 120) return value;
    }
    return "Tour lesson";
  }

  function publish(force = false) {
    scheduled = false;
    const instance = editor();
    if (!instance) return;
    const source = instance.getValue();
    if (!force && source === lastSource) return;
    lastSource = source;
    window.parent.postMessage({
      type: "go-tour-source",
      source,
      title: lessonTitle(),
      path: location.pathname + location.hash
    }, window.location.origin);
  }

  function schedulePublish() {
    if (scheduled) return;
    scheduled = true;
    setTimeout(publish, 80);
  }

  function attach() {
    const instance = editor();
    if (!instance) return;
    if (instance !== currentEditor) {
      currentEditor = instance;
      instance.on("change", schedulePublish);
      publish(true);
    }
  }

  window.addEventListener("message", event => {
    if (event.origin !== window.location.origin) return;
    if (event.data?.type === "go2cs-request-source") publish(true);
  });
  window.addEventListener("hashchange", () => setTimeout(() => publish(true), 100));
  new MutationObserver(() => {
    attach();
    schedulePublish();
  }).observe(document.documentElement, { childList: true, subtree: true });
  setInterval(attach, 500);
  attach();
})();
