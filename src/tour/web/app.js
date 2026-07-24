(() => {
  "use strict";

  const frame = document.querySelector("#tour-frame");
  const workspace = document.querySelector("#workspace");
  const divider = document.querySelector("#column-divider");
  const convertButton = document.querySelector("#convert-button");
  const runButton = document.querySelector("#run-button");
  const codeView = document.querySelector("#code-view code");
  const projectView = document.querySelector("#project-view code");
  const sourceTabs = document.querySelector(".source-tabs");
  const outputTabs = document.querySelector(".output-tabs");
  const runtimeSelect = document.querySelector("#runtime-mode");
  const outputScroll = document.querySelector("#output-scroll");
  const outputView = document.querySelector("#output-view");
  const outputTiming = document.querySelector("#output-timing");
  const lessonLabel = document.querySelector("#lesson-label");
  const conversionStatus = document.querySelector("#conversion-status");
  const connectionDot = document.querySelector("#connection-dot");
  const connectionLabel = document.querySelector("#connection-label");

  let goSource = "";
  let runtimeMode = runtimeSelect.value;
  let pagePath = "";
  let conversionID = "";
  let dirty = false;
  let converting = false;
  let running = false;
  let convertController = null;
  let runController = null;
  let autoConvertTimer = null;
  let activeOutput = "transpile";

  const outputs = {
    transpile: idleStage("transpile", "Open a Tour page to convert its Go code."),
    build: idleStage("build", "Run the converted project to see build output."),
    run: idleStage("run", "Run the converted project to see .NET output.")
  };

  function idleStage(id, message) {
    return { id, status: "idle", output: message, durationMs: 0 };
  }

  function setConnection(online, text) {
    connectionDot.className = online ? "online" : "offline";
    connectionLabel.textContent = text;
  }

  async function checkHealth() {
    try {
      const response = await fetch("/api/health");
      const health = await response.json();
      setConnection(health.tour, health.tour ? "Tour of Go connected" : "Tour of Go offline");
      for (const option of runtimeSelect.options) {
        const runtime = health.runtimes?.find(candidate => candidate.value === option.value);
        if (!runtime) continue;
        option.disabled = !runtime.available;
        option.title = runtime.detail || runtime.label;
      }
    } catch {
      setConnection(false, "Local server disconnected");
    }
  }

  function receiveTourMessage(event) {
    if (event.origin !== window.location.origin || event.source !== frame.contentWindow) return;
    if (event.data?.type === "go-tour-theme") {
      document.documentElement.dataset.theme = event.data.theme === "light" ? "light" : "dark";
      return;
    }
    if (event.data?.type !== "go-tour-source") return;

    const nextSource = event.data.source || "";
    const nextPath = event.data.path || "";
    const navigation = event.data.reason === "navigation" || (nextPath && nextPath !== pagePath);
    const sourceChanged = nextSource !== goSource;

    goSource = nextSource;
    pagePath = nextPath;
    lessonLabel.textContent = event.data.title || "Generated .NET";

    if (!goSource) {
      conversionStatus.textContent = "This page has no editable Go source";
      conversionID = "";
      dirty = false;
      renderGeneratedFiles("", "");
      outputs.transpile = idleStage("transpile", "This Tour page has no code to convert.");
      outputs.build = idleStage("build", "Nothing to build.");
      outputs.run = idleStage("run", "Nothing to run.");
      selectOutput("transpile");
      updateButtons();
      return;
    }

    if (navigation) {
      conversionID = "";
      dirty = true;
      conversionStatus.textContent = "Converting page...";
      scheduleAutoConvert();
    } else if (sourceChanged) {
      conversionID = "";
      dirty = true;
      conversionStatus.textContent = "Go source changed -- convert to refresh";
    }
    updateButtons();
  }

  function scheduleAutoConvert() {
    clearTimeout(autoConvertTimer);
    autoConvertTimer = setTimeout(() => {
      if (converting || running) {
        scheduleAutoConvert();
        return;
      }
      convertSource(true);
    }, 220);
  }

  async function convertSource(automatic = false) {
    if (!goSource || converting || running) return;

    convertController?.abort();
    const controller = new AbortController();
    convertController = controller;
    const sourceAtStart = goSource;

    converting = true;
    conversionID = "";
    outputs.transpile = { id: "transpile", status: "running", output: "Transpiling Go to C#...", durationMs: 0 };
    conversionStatus.textContent = automatic ? "Converting page..." : "Converting...";
    selectOutput("transpile");
    updateButtons();

    try {
      const response = await fetch("/api/convert", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ source: sourceAtStart, name: lessonLabel.textContent, runtime: runtimeMode }),
        signal: controller.signal
      });
      const result = await response.json();
      if (!response.ok) throw new Error(result.error || `Conversion failed (${response.status})`);

      outputs.transpile = result.stage;
      renderGeneratedFiles(result.csharp || "", result.project || "");
      conversionID = result.conversionId || "";
      dirty = goSource !== sourceAtStart;

      if (result.successful && !dirty) {
        conversionStatus.textContent = result.projectName ? `Converted  - ${result.projectName}` : "Converted";
      } else if (dirty) {
        conversionID = "";
        conversionStatus.textContent = "Go source changed -- convert to refresh";
      } else {
        conversionStatus.textContent = "Transpile failed";
      }
    } catch (error) {
      if (error.name !== "AbortError") {
        outputs.transpile = { id: "transpile", status: "failed", output: error.message, durationMs: 0 };
        conversionStatus.textContent = "Transpile failed";
      }
    } finally {
      if (convertController === controller) convertController = null;
      converting = false;
      renderOutputState();
      updateButtons();
    }
  }

  async function runDotNet() {
    if (running) {
      killRun();
      return;
    }
    if (!conversionID || dirty || converting) return;

    const controller = new AbortController();
    runController = controller;
    running = true;
    outputs.build = { id: "build", status: "running", output: "Building generated .NET project...", durationMs: 0 };
    outputs.run = idleStage("run", "Waiting for a successful build.");
    conversionStatus.textContent = "Building...";
    selectOutput("build");
    updateButtons();

    try {
      const requestBody = JSON.stringify({ conversionId: conversionID });
      const buildResponse = await fetch("/api/build", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: requestBody,
        signal: controller.signal
      });
      const buildStage = await buildResponse.json();
      if (!buildResponse.ok) throw new Error(buildStage.error || `Build failed (${buildResponse.status})`);
      outputs.build = buildStage;
      renderOutputState();

      if (buildStage.status !== "passed") {
        outputs.run = idleStage("run", "Run was not started because the build failed.");
        conversionStatus.textContent = "Build failed";
        selectOutput("build");
        return;
      }

      outputs.run = { id: "run", status: "running", output: "Running generated .NET program...", durationMs: 0 };
      conversionStatus.textContent = "Running .NET...";
      selectOutput("run");

      const runResponse = await fetch("/api/execute", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: requestBody,
        signal: controller.signal
      });
      const runStage = await runResponse.json();
      if (!runResponse.ok) throw new Error(runStage.error || `.NET run failed (${runResponse.status})`);
      outputs.run = runStage;
      conversionStatus.textContent = runStage.status === "passed" ? ".NET run completed" : ".NET run failed";
      selectOutput("run");
    } catch (error) {
      const active = outputs.run.status === "running" ? "run" : "build";
      if (error.name === "AbortError") {
        outputs[active] = { id: active, status: "killed", output: active === "run" ? "Program exited: killed" : "Killed.", durationMs: 0 };
        if (active === "build") outputs.run = idleStage("run", "Run was not started.");
        conversionStatus.textContent = ".NET process killed";
      } else {
        outputs[active] = { id: active, status: "failed", output: error.message, durationMs: 0 };
        conversionStatus.textContent = active === "build" ? "Build failed" : ".NET run failed";
      }
      selectOutput(active);
    } finally {
      if (runController === controller) runController = null;
      running = false;
      renderOutputState();
      updateButtons();
    }
  }

  function killRun() {
    runController?.abort();
  }

  function updateButtons() {
    convertButton.disabled = !goSource || converting || running || !dirty;
    convertButton.textContent = converting ? "Converting..." : "Convert";

    runButton.disabled = !running && (!conversionID || dirty || converting);
    runButton.textContent = running ? "Kill" : "Run";
    runButton.classList.toggle("kill", running);
    runtimeSelect.disabled = converting || running;
  }

  function renderGeneratedFiles(csharp, project) {
    codeView.innerHTML = highlightCSharp(csharp || "// No C# source was emitted.");
    projectView.innerHTML = highlightXML(project || "<!-- No project file was emitted. -->");
  }

  function selectSourceView(name) {
    for (const button of sourceTabs.querySelectorAll("[data-view]")) {
      const selected = button.dataset.view === name;
      button.setAttribute("aria-selected", String(selected));
      document.querySelector(`#${button.dataset.view}-view`).hidden = !selected;
    }
  }

  function selectOutput(id) {
    activeOutput = id;
    renderOutputState();
  }

  function renderOutputState() {
    const followOutput = outputScroll.scrollTop + outputScroll.clientHeight >= outputScroll.scrollHeight - 4;
    for (const button of outputTabs.querySelectorAll("[data-output]")) {
      const stage = outputs[button.dataset.output];
      button.className = stage.status;
      button.setAttribute("aria-selected", String(button.dataset.output === activeOutput));
    }
    const stage = outputs[activeOutput];
    outputView.textContent = stage.output || "(no output)";
    outputTiming.textContent = stage.durationMs ? `${(stage.durationMs / 1000).toFixed(2)}s` : "";
    if (followOutput) {
      requestAnimationFrame(() => {
        outputScroll.scrollTop = outputScroll.scrollHeight;
      });
    }
  }

  function highlightCSharp(source) {
    const keywords = new Set([
      "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "class",
      "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
      "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
      "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
      "namespace", "new", "null", "object", "operator", "out", "override", "params", "partial",
      "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
      "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
      "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual",
      "void", "volatile", "while"
    ]);
    const token = /\/\/[^\n]*|\/\*[\s\S]*?\*\/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])+'|\b\d+(?:\.\d+)?\b|\b[A-Za-z_@][A-Za-z0-9_@]*\b/g;
    let result = "";
    let index = 0;
    for (const match of source.matchAll(token)) {
      result += escapeHTML(source.slice(index, match.index));
      const value = match[0];
      let kind = "";
      if (value.startsWith("//") || value.startsWith("/*")) kind = "comment";
      else if (value.startsWith('"') || value.startsWith('@"') || value.startsWith("'")) kind = "string";
      else if (/^\d/.test(value)) kind = "number";
      else if (keywords.has(value)) kind = "keyword";
      else if (/^[A-Z]/.test(value)) kind = "type";
      result += kind ? `<span class="tok-${kind}">${escapeHTML(value)}</span>` : escapeHTML(value);
      index = match.index + value.length;
    }
    return result + escapeHTML(source.slice(index));
  }

  function highlightXML(source) {
    const token = /<!--[\s\S]*?-->|<\/?[A-Za-z][^>]*>/g;
    let result = "";
    let index = 0;
    for (const match of source.matchAll(token)) {
      result += escapeHTML(source.slice(index, match.index));
      const kind = match[0].startsWith("<!--") ? "comment" : "xml";
      result += `<span class="tok-${kind}">${escapeHTML(match[0])}</span>`;
      index = match.index + match[0].length;
    }
    return result + escapeHTML(source.slice(index));
  }

  function escapeHTML(value) {
    return value.replace(/[&<>"']/g, character => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;"
    })[character]);
  }

  function setDivider(percent) {
    const bounds = workspace.getBoundingClientRect();
    if (bounds.width < 900) return;
    const minLeft = Math.min(480, bounds.width * .45);
    const minRight = 300;
    const pixels = Math.max(minLeft, Math.min(bounds.width - minRight - 8, bounds.width * percent / 100));
    const actual = pixels / bounds.width * 100;
    workspace.style.setProperty("--left-width", `${pixels}px`);
    divider.setAttribute("aria-valuenow", String(Math.round(actual)));
  }

  divider.addEventListener("pointerdown", event => {
    divider.setPointerCapture(event.pointerId);
    divider.classList.add("dragging");
  });
  divider.addEventListener("pointermove", event => {
    if (!divider.hasPointerCapture(event.pointerId)) return;
    const bounds = workspace.getBoundingClientRect();
    setDivider((event.clientX - bounds.left) / bounds.width * 100);
  });
  divider.addEventListener("pointerup", event => {
    divider.releasePointerCapture(event.pointerId);
    divider.classList.remove("dragging");
  });
  divider.addEventListener("keydown", event => {
    if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
    event.preventDefault();
    const current = Number(divider.getAttribute("aria-valuenow")) || 67;
    setDivider(current + (event.key === "ArrowRight" ? 2 : -2));
  });

  window.addEventListener("message", receiveTourMessage);
  frame.addEventListener("load", () => {
    frame.contentWindow?.postMessage({ type: "go2cs-request-source" }, window.location.origin);
  });
  sourceTabs.addEventListener("click", event => {
    const button = event.target.closest("[data-view]");
    if (button) selectSourceView(button.dataset.view);
  });
  outputTabs.addEventListener("click", event => {
    const button = event.target.closest("[data-output]");
    if (button) selectOutput(button.dataset.output);
  });
  runtimeSelect.addEventListener("change", () => {
    runtimeMode = runtimeSelect.value;
    conversionID = "";
    dirty = Boolean(goSource);
    outputs.build = idleStage("build", "Run the converted project to see build output.");
    outputs.run = idleStage("run", "Run the converted project to see .NET output.");
    conversionStatus.textContent = goSource ? "Switching runtime..." : "Waiting for Go source";
    updateButtons();
    if (goSource) scheduleAutoConvert();
  });
  convertButton.addEventListener("click", () => convertSource(false));
  runButton.addEventListener("click", runDotNet);
  window.addEventListener("keydown", event => {
    if ((event.ctrlKey || event.metaKey) && event.key === "Enter") {
      event.preventDefault();
      convertSource(false);
    } else if (event.shiftKey && event.key === "Enter") {
      event.preventDefault();
      runDotNet();
    }
  });
  window.addEventListener("resize", () => setDivider(Number(divider.getAttribute("aria-valuenow")) || 67));

  renderOutputState();
  checkHealth();
  setInterval(checkHealth, 15000);
})();
