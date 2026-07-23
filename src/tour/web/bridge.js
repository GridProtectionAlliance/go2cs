(() => {
  "use strict";

  let currentEditor = null;
  let lastSource = null;
  let lastPath = "";
  let pendingNavigation = null;

  function editor() {
    const element = document.querySelector(".CodeMirror");
    return element?.CodeMirror || null;
  }

  function currentPath() {
    return location.pathname + location.hash;
  }

  function lessonTitle() {
    const candidates = [".lesson-title", ".page h2", "article h1", "article h2", "h1", "h2"];
    for (const selector of candidates) {
      const value = document.querySelector(selector)?.textContent?.trim();
      if (value && value.length < 120) return value;
    }
    return "Generated .NET";
  }

  function publish(reason, force = false) {
    const instance = editor();
    const source = instance ? instance.getValue() : "";
    const path = currentPath();
    if (!force && source === lastSource && path === lastPath) return;
    lastSource = source;
    lastPath = path;
    window.parent.postMessage({
      type: "go-tour-source",
      source,
      title: lessonTitle(),
      path,
      reason
    }, window.location.origin);
  }

  function attach() {
    const instance = editor();
    if (!instance || instance === currentEditor) return;

    currentEditor = instance;

    // Configure each editor once. Reapplying these options mutates CodeMirror's
    // DOM, which would wake the MutationObserver and create a feedback loop.
    if (instance.getOption("mode") !== "text/x-go") {
      instance.setOption("mode", "text/x-go");
    }
    if (!instance.getOption("lineNumbers")) {
      instance.setOption("lineNumbers", true);
    }

    instance.on("change", () => publish("edit"));
    publish("navigation", true);
  }

  function inspectNavigation() {
    attach();
    if (currentPath() === lastPath) return;
    clearTimeout(pendingNavigation);
    pendingNavigation = setTimeout(() => publish("navigation", true), 100);
  }

  window.addEventListener("message", event => {
    if (event.origin !== window.location.origin) return;
    if (event.data?.type === "go2cs-request-source") {
      attach();
      publish(lastPath ? "sync" : "navigation", true);
    }
  });
  window.addEventListener("hashchange", inspectNavigation);
  window.addEventListener("popstate", inspectNavigation);
  new MutationObserver(inspectNavigation).observe(document.documentElement, { childList: true, subtree: true });
  setInterval(inspectNavigation, 400);
  attach();
})();
