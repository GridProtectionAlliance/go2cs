(() => {
  "use strict";

  const frame = document.querySelector("#tour-frame");
  const runButton = document.querySelector("#run-button");
  const copyButton = document.querySelector("#copy-button");
  const code = document.querySelector("#csharp-code code");
  const consoleTabs = document.querySelector("#console-tabs");
  const consoleOutput = document.querySelector("#console-output");
  const elapsed = document.querySelector("#elapsed-label");
  const lesson = document.querySelector("#lesson-label");
  const syncState = document.querySelector("#sync-state");
  const connectionDot = document.querySelector("#connection-dot");
  const connectionLabel = document.querySelector("#connection-label");
  const projectLabel = document.querySelector("#project-label");

  let goSource = "";
  let latestCSharp = "";
  let stages = [];
  let activeTab = "welcome";
  let running = false;

  const stageNodes = new Map(
    [...document.querySelectorAll(".pipeline-stage")].map(node => [node.dataset.stage, node])
  );

  function setConnection(online, label) {
    connectionDot.className = `connection-dot ${online ? "online" : "offline"}`;
    connectionLabel.textContent = label;
  }

  async function checkHealth() {
    try {
      const response = await fetch("/api/health");
      const health = await response.json();
      setConnection(health.tour, health.tour ? "Official Tour connected" : "Official Tour is offline");
    } catch {
      setConnection(false, "Local server disconnected");
    }
  }

  function receiveTourMessage(event) {
    if (event.origin !== window.location.origin || event.source !== frame.contentWindow) return;
    if (!event.data || event.data.type !== "go-tour-source") return;

    goSource = event.data.source || "";
    lesson.textContent = event.data.title || event.data.path || "Tour lesson";
    syncState.textContent = goSource ? "Source synchronized" : "No editable source";
    runButton.disabled = !goSource || running;
  }

  function resetPipeline() {
    stageNodes.forEach(node => { node.className = "pipeline-stage"; });
    stages = [];
  }

  function markRunning() {
    resetPipeline();
    stageNodes.get("go")?.classList.add("running");
    elapsed.textContent = "Pipeline running…";
    consoleTabs.innerHTML = `<button type="button" role="tab" aria-selected="true" data-tab="running">Live</button>`;
    consoleOutput.textContent = "Running Go, transpiling the same source, then building and executing .NET…";
  }

  async function runPipeline() {
    if (!goSource || running) return;
    running = true;
    runButton.disabled = true;
    runButton.innerHTML = `<span class="play-icon">●</span> Running…`;
    markRunning();
    const started = performance.now();

    try {
      const response = await fetch("/api/pipeline", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ source: goSource, name: lesson.textContent })
      });
      const result = await response.json();
      if (!response.ok) throw new Error(result.error || `Pipeline failed (${response.status})`);
      renderResult(result, performance.now() - started);
    } catch (error) {
      resetPipeline();
      consoleTabs.innerHTML = `<button type="button" role="tab" class="failed" aria-selected="true">Request</button>`;
      consoleOutput.textContent = error.message;
      elapsed.textContent = "Pipeline request failed";
    } finally {
      running = false;
      runButton.disabled = !goSource;
      runButton.innerHTML = `<span class="play-icon">▶</span> Convert &amp; run`;
    }
  }

  function renderResult(result, totalMS) {
    stages = result.stages || [];
    latestCSharp = result.csharp || "";
    code.textContent = latestCSharp || "// No C# source was emitted. Open the Transpile output for details.";
    copyButton.disabled = !latestCSharp;
    projectLabel.textContent = result.project || "main.cs";

    stageNodes.forEach(node => { node.className = "pipeline-stage"; });
    for (const stage of stages) {
      stageNodes.get(stage.id)?.classList.add(stage.status);
    }

    consoleTabs.innerHTML = "";
    for (const stage of stages) {
      const button = document.createElement("button");
      button.type = "button";
      button.role = "tab";
      button.dataset.tab = stage.id;
      button.className = stage.status;
      button.textContent = stage.label;
      button.setAttribute("aria-selected", "false");
      consoleTabs.append(button);
    }

    const failed = stages.find(stage => stage.status === "failed");
    const preferred = failed || stages.find(stage => stage.id === "run") || stages[0];
    selectTab(preferred?.id);
    elapsed.textContent = `${result.successful ? "Pipeline passed" : "Pipeline completed with errors"} · ${(totalMS / 1000).toFixed(1)}s`;
  }

  function selectTab(id) {
    const stage = stages.find(item => item.id === id);
    if (!stage) return;
    activeTab = id;
    for (const button of consoleTabs.querySelectorAll("button")) {
      button.setAttribute("aria-selected", String(button.dataset.tab === id));
    }
    const timing = stage.durationMs ? `\n\n[${stage.status} in ${(stage.durationMs / 1000).toFixed(2)}s]` : "";
    consoleOutput.textContent = (stage.output || "(no output)") + timing;
  }

  async function copyCSharp() {
    if (!latestCSharp) return;
    await navigator.clipboard.writeText(latestCSharp);
    const original = copyButton.textContent;
    copyButton.textContent = "Copied";
    setTimeout(() => { copyButton.textContent = original; }, 1200);
  }

  window.addEventListener("message", receiveTourMessage);
  frame.addEventListener("load", () => {
    frame.contentWindow?.postMessage({ type: "go2cs-request-source" }, window.location.origin);
  });
  runButton.addEventListener("click", runPipeline);
  copyButton.addEventListener("click", copyCSharp);
  consoleTabs.addEventListener("click", event => {
    const button = event.target.closest("button[data-tab]");
    if (button) selectTab(button.dataset.tab);
  });
  window.addEventListener("keydown", event => {
    if ((event.ctrlKey || event.metaKey) && event.shiftKey && event.key === "Enter") {
      event.preventDefault();
      runPipeline();
    }
  });

  checkHealth();
  setInterval(checkHealth, 15000);
})();
