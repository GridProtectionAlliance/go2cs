// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Standalone runner for the go2cs behavioral suite. Mirrors the four MSTest phases
// (Transpile -> Compile -> TargetComparison -> OutputComparison) but as a plain console app that is
// NOT hosted in testhost.exe, removing the self-lock failure mode. It also collapses the per-project
// "dotnet build" churn (180 invocations) into a single parallel MSBuild call (Tier 2a).

using System.Diagnostics;
using System.Text;
using BehavioralRunner;

return Runner.Main(args);

namespace BehavioralRunner
{
    internal enum Phase { Transpile, Compile, Target, Output }

    internal enum Status { Pass, Fail, Skip }

    internal sealed class ProjectResult
    {
        public required string Name { get; init; }
        public Dictionary<Phase, Status> Phases { get; } = new();
        public List<string> Messages { get; } = new();

        public bool Failed => Phases.Values.Any(s => s == Status.Fail);
    }

    internal static class Runner
    {
        // Timeouts (ms). A build/transpile/run that exceeds these is treated as hung and killed with
        // its whole process tree -- the runner never blocks indefinitely the way the MSTest Exec did.
        private const int BuildAllTimeoutMs = 300_000;  // one-shot parallel build of every C# target
        private const int BuildOneTimeoutMs = 180_000;  // per-project fallback build / go build
        private const int TranspileTimeoutMs = 60_000;
        private const int RunTimeoutMs = 30_000;

        private const string Config = "Release";
        private const string NetVersion = "net9.0";

        private static string s_repoRoot = null!;
        private static string s_srcRoot = null!;
        private static string s_behavioralDir = null!;
        private static string s_converterSrc = null!;
        private static string s_go2csExe = null!;

        // Go-build failures are tracked separately (a Go build failing is not a C# compile failure, but
        // it must still surface and fail the run, since it blocks output comparison).
        private static readonly List<string> s_goBuildFailures = new();

        // Newest shared-dependency output (golib, the analyzer, core/* and any redirected package) as of
        // the pre-build. A target assembly older than this predates its own dependencies, so it cannot be
        // treated as evidence that the target still compiles -- see SuspectProjects.
        private static DateTime s_sharedDepStamp = DateTime.MinValue;

        public static int Main(string[] args)
        {
            // ----- argument parsing -----
            string? filter = null;
            bool updateTargets = false;
            bool listOnly = false;
            HashSet<Phase> phases = new() { Phase.Transpile, Phase.Compile, Phase.Target, Phase.Output };

            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                switch (a)
                {
                    case "--filter" when i + 1 < args.Length:
                        filter = args[++i];
                        break;
                    case "--phase" when i + 1 < args.Length:
                        phases = ParsePhases(args[++i]);
                        break;
                    case "--update-targets":
                        updateTargets = true;
                        break;
                    case "--list":
                        listOnly = true;
                        break;
                    case "--help" or "-h":
                        PrintUsage();
                        return 0;
                    default:
                        Console.Error.WriteLine($"Unknown argument: {a}");
                        PrintUsage();
                        return 2;
                }
            }

            // ----- resolve paths -----
            // Runner lives at src\Tests\Behavioral\BehavioralRunner; behavioral dir is its parent.
            s_behavioralDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\.."));
            s_srcRoot = Path.GetFullPath(Path.Combine(s_behavioralDir, @"..\.."));
            s_repoRoot = Path.GetFullPath(Path.Combine(s_srcRoot, ".."));
            s_converterSrc = Path.Combine(s_srcRoot, "go2cs");
            s_go2csExe = Path.Combine(s_converterSrc, "bin", "go2cs.exe");

            // ----- discover projects -----
            // A behavioral test project is a folder with both a .csproj and Go source. This naturally
            // excludes the BehavioralTests (MSTest) runner and this BehavioralRunner utility (no .go),
            // and any future utility folder, without brittle name checks.
            List<string> projects = Directory.GetDirectories(s_behavioralDir)
                .Where(d => Directory.GetFiles(d, "*.csproj").Length > 0)
                .Where(d => Directory.GetFiles(d, "*.go").Length > 0)
                .Select(Path.GetFileName)
                .Where(n => filter is null || n!.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            if (listOnly)
            {
                foreach (string p in projects)
                    Console.WriteLine(p);
                Console.WriteLine($"({projects.Count} projects)");
                return 0;
            }

            if (projects.Count == 0)
            {
                Console.Error.WriteLine("No behavioral projects matched.");
                return 2;
            }

            Console.WriteLine($"go2cs behavioral runner: {projects.Count} project(s), phases [{string.Join(", ", phases)}]");
            Stopwatch total = Stopwatch.StartNew();

            Dictionary<string, ProjectResult> results = projects.ToDictionary(p => p, p => new ProjectResult { Name = p });

            // ----- Phase: Transpile -----
            if (phases.Contains(Phase.Transpile) || phases.Contains(Phase.Target) || phases.Contains(Phase.Compile) || phases.Contains(Phase.Output))
            {
                if (!EnsureConverterBuilt())
                    return 1;

                RunTranspile(projects, results);

                if (updateTargets)
                {
                    UpdateTargets(projects, results);
                    Console.WriteLine($"Updated .cs.target goldens for {projects.Count} project(s). Done.");
                    return 0;
                }
            }

            // ----- Phase: Target comparison (pure file compare; no build) -----
            if (phases.Contains(Phase.Target))
                RunTargetComparison(projects, results);

            // ----- Phase: Compile (one-shot C# build-all + per-project go build) -----
            bool needRun = phases.Contains(Phase.Output);

            if (phases.Contains(Phase.Compile) || needRun)
            {
                RunCompileCSharp(projects, results);

                if (needRun)
                    RunCompileGo(projects, results);
            }

            // ----- Phase: Output comparison -----
            if (phases.Contains(Phase.Output))
                RunOutputComparison(projects, results);

            total.Stop();

            // ----- summary -----
            return Report(results.Values, total.Elapsed);
        }

        private static bool EnsureConverterBuilt()
        {
            if (File.Exists(s_go2csExe))
            {
                DateTime exe = File.GetLastWriteTimeUtc(s_go2csExe);

                bool stale = Directory.GetFiles(s_converterSrc, "*.go")
                    .Any(go => File.GetLastWriteTimeUtc(go) > exe);

                if (!stale)
                    return true;
            }

            Console.WriteLine("Building go2cs.exe (converter sources changed)...");
            ProcResult r = Exec("go", $"build -o \"{s_go2csExe}\"", s_converterSrc, BuildOneTimeoutMs);

            if (r.ExitCode != 0)
            {
                Console.Error.WriteLine($"go build of converter failed ({r.ExitCode}):\n{r.StdErr}");
                return false;
            }

            return true;
        }

        private static void RunTranspile(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Transpile] {projects.Count} project(s)... ");
            int failed = 0;

            foreach (string p in projects)
            {
                string projPath = Path.Combine(s_behavioralDir, p);

                if (UpToDate(projPath))
                {
                    results[p].Phases[Phase.Transpile] = Status.Pass;
                    continue;
                }

                ProcResult r = Exec(s_go2csExe, $"\"{projPath}\"", projPath, TranspileTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Phases[Phase.Transpile] = Status.Pass;
                }
                else
                {
                    results[p].Phases[Phase.Transpile] = Status.Fail;
                    results[p].Messages.Add($"transpile exit {r.ExitCode}: {Truncate(r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        // A project is up to date when every .cs is newer than BOTH its matching .go source and the
        // converter binary that produced it. The converter must be part of this comparison: converter
        // work is the normal case where the .go files DON'T change, so a .go-only check leaves every
        // project "up to date", skips transpilation entirely, and lets the Target/Output phases validate
        // the PREVIOUS converter's output against goldens that same converter generated -- a false green
        // that guards nothing. (check-no-regression.ps1 re-transpiles unconditionally and is immune.)
        private static bool UpToDate(string projPath)
        {
            DateTime exe = File.GetLastWriteTimeUtc(s_go2csExe);

            foreach (string go in Directory.GetFiles(projPath, "*.go"))
            {
                string cs = Path.ChangeExtension(go, ".cs");

                if (!File.Exists(cs))
                    return false;

                DateTime csTime = File.GetLastWriteTimeUtc(cs);

                if (csTime <= File.GetLastWriteTimeUtc(go) || csTime <= exe)
                    return false;
            }

            return true;
        }

        private static void RunTargetComparison(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Target]   byte-comparing goldens... ");
            int failed = 0;

            foreach (string p in projects)
            {
                if (results[p].Phases.TryGetValue(Phase.Transpile, out Status t) && t == Status.Fail)
                {
                    results[p].Phases[Phase.Target] = Status.Skip;
                    continue;
                }

                string projPath = Path.Combine(s_behavioralDir, p);
                bool ok = true;

                foreach (string go in Directory.GetFiles(projPath, "*.go"))
                {
                    string cs = Path.ChangeExtension(go, ".cs");
                    string target = cs + ".target";

                    if (!File.Exists(cs) || !File.Exists(target) || !FilesEqual(cs, target))
                    {
                        ok = false;
                        results[p].Messages.Add($"target mismatch: {Path.GetFileName(cs)}");
                    }
                }

                results[p].Phases[Phase.Target] = ok ? Status.Pass : Status.Fail;
                if (!ok) failed++;
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static void RunCompileCSharp(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            string go2csPathArg = s_srcRoot.Replace('\\', '/').TrimEnd('/') + "/";

            // Pre-build the shared dependencies (golib, the go2cs-gen analyzer, and the core/* packages
            // the targets reference) SEQUENTIALLY first. The one-shot parallel build of 180 targets that
            // each pull these shared projects otherwise races on their obj/bin output (intermittent
            // MSB3026/MSB3027 "file in use" under heavy parallelism). Building them to completion up front
            // means they are up to date during the fan-out, so no node writes to them and the race is gone.
            PreBuildSharedDeps(projects, go2csPathArg);

            // Generate a traversal project that builds every target csproj in a single parallel MSBuild
            // invocation -- replacing 180 sequential "dotnet build" calls (Tier 2a). go2csPath is pinned
            // to the src root so each target's golib/analyzer refs resolve to live source (matching the
            // MSTest harness, which sets the go2csPath env var); Configuration=Release matches TargetConfig.
            string traversal = WriteTraversalProject(projects);

            string commonArgs = $"-nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}";

            // RESTORE FIRST -- and in its own process. The traversal drives each target through the
            // MSBuild *task* (Targets="Build"), which -- unlike the "dotnet build <csproj>" the
            // per-project path uses -- does NOT imply a restore. Any project in the graph that has no
            // obj\project.assets.json therefore fails instantly with NETSDK1004 ("Assets file not
            // found"), which fails the whole batch and drops the ENTIRE suite onto the per-project
            // attribution path below -- a ~20 minute tax to discover that nothing was actually broken
            // (the fallback restores as it goes, so it reports 0 failures). That is not a cold-clone
            // curiosity: it fires on any fresh worktree, after clean-bin, and for any dependency
            // subtree the pre-build does not cover. Restore runs as a SEPARATE invocation because
            // restore rewrites a project's imports, so it must not share an evaluation with Build.
            Console.Write($"[Compile]  C# (restoring {projects.Count})... ");

            ProcResult restore = Exec("dotnet",
                $"build \"{traversal}\" -t:RestoreAll {commonArgs}",
                s_behavioralDir, BuildAllTimeoutMs);

            Console.WriteLine(restore.ExitCode == 0 ? "ok" : $"restore reported errors (exit {restore.ExitCode})");

            if (restore.ExitCode != 0)
                Console.Error.WriteLine($"  restore output: {Truncate(restore.StdOut + restore.StdErr, 1000)}");

            Console.Write($"[Compile]  C# (one-shot parallel build of {projects.Count})... ");

            ProcResult all = Exec("dotnet",
                $"build \"{traversal}\" -t:BuildAll {commonArgs}",
                s_behavioralDir, BuildAllTimeoutMs);

            if (all.ExitCode == 0)
            {
                foreach (string p in projects)
                    results[p].Phases[Phase.Compile] = Status.Pass;

                Console.WriteLine("ok");
                return;
            }

            // Build-all failed. Narrow the per-project attribution to the projects that could actually
            // be responsible, so one broken project costs one rebuild instead of 438. A project is a
            // suspect when MSBuild named it in an error line, OR when it has no up-to-date output
            // assembly (which also covers targets the failed batch never got around to scheduling).
            // Anything with a fresh assembly demonstrably compiled in THIS batch and is passed without
            // a rebuild. When the suspect set cannot be determined -- an empty set, or a timeout, where
            // the batch was killed mid-flight and the assembly evidence is meaningless -- every project
            // is attributed, preserving the original conservative behavior.
            bool timedOut = all.ExitCode == -1 && all.StdErr.StartsWith("TIMEOUT", StringComparison.Ordinal);
            string buildOutput = all.StdOut + all.StdErr;

            List<string> suspects = timedOut
                ? projects.ToList()
                : SuspectProjects(projects, buildOutput);

            if (suspects.Count == 0)
                suspects = projects.ToList();

            Console.WriteLine(timedOut
                ? $"build-all TIMED OUT after {BuildAllTimeoutMs} ms; attributing all {suspects.Count} per project..."
                : $"build-all reported errors; attributing {suspects.Count} suspect project(s) per project...");

            // Always surface why the batch failed -- silently discarding it (as this path used to) makes
            // an infrastructure failure indistinguishable from a real compile break.
            Console.Error.WriteLine($"  build-all output: {Truncate(buildOutput, 1000)}");

            foreach (string p in projects.Except(suspects, StringComparer.OrdinalIgnoreCase))
                results[p].Phases[Phase.Compile] = Status.Pass;

            int failed = 0;

            foreach (string p in suspects)
            {
                string csproj = Path.Combine(s_behavioralDir, p, $"{p}.csproj");

                ProcResult r = Exec("dotnet",
                    $"build \"{csproj}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_behavioralDir, BuildOneTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Phases[Phase.Compile] = Status.Pass;
                }
                else
                {
                    results[p].Phases[Phase.Compile] = Status.Fail;
                    results[p].Messages.Add($"compile exit {r.ExitCode}: {Truncate(r.StdOut + r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine($"[Compile]  C# per-project: {failed} failed");
        }

        // Projects that could be responsible for a failed batch build: those MSBuild named in an error
        // line, plus those with no up-to-date output assembly. The assembly check is what makes it safe
        // to pass the remainder without rebuilding -- an assembly newer than every .cs in the project AND
        // newer than every shared dependency was necessarily produced from exactly the inputs in play now,
        // so a stale artifact from an earlier converter (or an earlier golib) can never be mistaken for a
        // fresh success. Note this degrades to attributing everything in the case that matters most: a
        // converter change rewrites every .cs, which makes every assembly stale and every project a
        // suspect. The narrowing only ever pays off when the inputs really did not move.
        private static List<string> SuspectProjects(IReadOnlyList<string> projects, string buildOutput)
        {
            HashSet<string> suspects = new(StringComparer.OrdinalIgnoreCase);

            // MSBuild appends the owning project to each diagnostic: "... error CS1002: ; expected
            // [C:\path\Foo.csproj]". Errors from a referenced package (e.g. a go-src-converted
            // dependency) name that project instead, which matches nothing here -- leaving the suspect
            // set empty and falling back to attributing everything.
            foreach (string line in buildOutput.Split('\n'))
            {
                int open = line.LastIndexOf('[');
                int close = line.LastIndexOf(".csproj]", StringComparison.OrdinalIgnoreCase);

                if (open < 0 || close < open)
                    continue;

                string name = Path.GetFileNameWithoutExtension(line[(open + 1)..(close + ".csproj".Length)]);

                foreach (string p in projects)
                {
                    if (string.Equals(p, name, StringComparison.OrdinalIgnoreCase))
                        suspects.Add(p);
                }
            }

            foreach (string p in projects)
            {
                if (suspects.Contains(p))
                    continue;

                string projPath = Path.Combine(s_behavioralDir, p);
                string assembly = Path.Combine(projPath, "bin", Config, NetVersion, $"{p}.dll");

                if (!File.Exists(assembly))
                {
                    suspects.Add(p);
                    continue;
                }

                DateTime built = File.GetLastWriteTimeUtc(assembly);

                // Stale against its own sources, or against the shared dependencies it links -- either
                // way the assembly proves nothing about whether this project still compiles today.
                if (built < s_sharedDepStamp || Directory.GetFiles(projPath, "*.cs").Any(cs => File.GetLastWriteTimeUtc(cs) > built))
                    suspects.Add(p);
            }

            return projects.Where(suspects.Contains).ToList();
        }

        private static void RunCompileGo(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Compile]  Go (per project)... ");
            int failed = 0;

            foreach (string p in projects)
            {
                // Only output-compared projects are Go-built (matching the MSTest harness, which emits a
                // Go-build step solely for [GoTestMatchingConsoleOutput] projects). Library-style projects
                // with no "package main" (e.g. Constraints) have nothing to "go build -o".
                if (!MatchConsoleOutput(p))
                    continue;

                string projPath = Path.Combine(s_behavioralDir, p);
                string goExeDir = Path.Combine(projPath, "bin", Config, "Go");
                Directory.CreateDirectory(goExeDir);

                if (!File.Exists(Path.Combine(projPath, "go.mod")))
                    Exec("go", $"mod init go2cs/{p}", projPath, BuildOneTimeoutMs);

                ProcResult r = Exec("go", $"build -o \"{goExeDir}\"", projPath, BuildOneTimeoutMs);

                if (r.ExitCode != 0)
                {
                    results[p].Messages.Add($"go build exit {r.ExitCode}: {Truncate(r.StdErr)}");
                    s_goBuildFailures.Add($"{p}: {Truncate(r.StdErr, 200)}");
                    failed++;
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static void RunOutputComparison(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Output]   running C# vs Go, comparing exit code + stdout... ");
            int failed = 0, compared = 0;

            foreach (string p in projects)
            {
                if (!MatchConsoleOutput(p))
                {
                    results[p].Phases[Phase.Output] = Status.Skip;
                    continue;
                }

                // A failed compile means there is no C# exe to run.
                if (results[p].Phases.TryGetValue(Phase.Compile, out Status c) && c == Status.Fail)
                {
                    results[p].Phases[Phase.Output] = Status.Skip;
                    continue;
                }

                string projPath = Path.Combine(s_behavioralDir, p);
                string csExe = Path.Combine(projPath, "bin", Config, NetVersion, $"{p}.exe");
                string goExe = Path.Combine(projPath, "bin", Config, "Go", $"{p}.exe");
                string workDir = Path.GetDirectoryName(projPath)!;

                if (!File.Exists(csExe) || !File.Exists(goExe))
                {
                    results[p].Phases[Phase.Output] = Status.Skip;
                    results[p].Messages.Add("missing C# or Go exe");
                    continue;
                }

                compared++;
                ProcResult cs = Exec(csExe, null, workDir, RunTimeoutMs);
                ProcResult go = Exec(goExe, null, workDir, RunTimeoutMs);

                // The Go binary is the oracle: exit codes must MATCH rather than both be zero, so a
                // program that legitimately crashes (e.g. an unrecovered panic exits 2, like Go) is
                // validated differentially instead of being rejected outright. stderr is compared by
                // FIRST LINE only: Go's panic report appends a machine-specific goroutine stack
                // trace, so a full comparison can never match; the first line carries the
                // deterministic report and is empty for clean runs.
                if (cs.ExitCode != go.ExitCode)
                {
                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add($"exit code mismatch: C# {cs.ExitCode} vs Go {go.ExitCode}");
                    failed++;
                }
                else if (!string.Equals(cs.StdOut, go.StdOut, StringComparison.Ordinal))
                {
                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add("stdout mismatch C# vs Go");
                    failed++;
                }
                else if (!string.Equals(FirstLine(cs.StdErr), FirstLine(go.StdErr), StringComparison.Ordinal))
                {
                    results[p].Phases[Phase.Output] = Status.Fail;
                    results[p].Messages.Add($"stderr first-line mismatch: C# \"{FirstLine(cs.StdErr)}\" vs Go \"{FirstLine(go.StdErr)}\"");
                    failed++;
                }
                else
                {
                    results[p].Phases[Phase.Output] = Status.Pass;
                }
            }

            Console.WriteLine($"{compared} compared, {failed} failed");
        }

        private static void UpdateTargets(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            foreach (string p in projects)
            {
                string projPath = Path.Combine(s_behavioralDir, p);

                foreach (string go in Directory.GetFiles(projPath, "*.go"))
                {
                    string cs = Path.ChangeExtension(go, ".cs");

                    if (File.Exists(cs))
                        File.Copy(cs, cs + ".target", overwrite: true);
                }
            }
        }

        // Builds the deduped union of ProjectReferences across the target csprojs (golib, the analyzer,
        // core/* packages) one at a time, so they are up to date before the parallel target fan-out.
        private static void PreBuildSharedDeps(IReadOnlyList<string> projects, string go2csPathArg)
        {
            HashSet<string> deps = new(StringComparer.OrdinalIgnoreCase);

            foreach (string p in projects)
            {
                string csprojDir = Path.Combine(s_behavioralDir, p);

                // Read the csproj AND the project-local Directory.Build.props/.targets, honouring both
                // Include and Remove. A test whose converter-generated csproj names a reference that does
                // not exist in the stub baseline redirects it from a Directory.Build.targets -- the csproj
                // itself is template output the converter rewrites on every re-transpile, so the redirect
                // cannot live there. DeepEqual does exactly this: it declares core\reflect (baseline has
                // no reflect) and swaps in go-src-converted\reflect. Reading the csproj alone therefore
                // both chased a phantom path (MSB1009 "Project file does not exist") and stayed blind to
                // the 34-project closure the test really pulls in -- leaving that closure unbuilt during
                // the parallel fan-out, which is precisely the race this pre-build exists to prevent.
                // Same order MSBuild imports them: props, then the project body, then targets -- so a
                // Remove in targets cancels an Include from the csproj, exactly as it does in a real build.
                foreach (string file in new[] { "Directory.Build.props", $"{p}.csproj", "Directory.Build.targets" })
                {
                    string path = Path.Combine(csprojDir, file);

                    if (!File.Exists(path))
                        continue;

                    foreach (string line in File.ReadLines(path))
                    {
                        string? include = AttributeValue(line, "ProjectReference Include=\"");
                        string? remove = AttributeValue(line, "ProjectReference Remove=\"");

                        // Resolve relative to the csproj's OWN directory (not the runner CWD) so a relative
                        // cross-project ProjectReference (e.g. a cross-package test's `..\lib\lib.csproj`)
                        // resolves correctly instead of producing a phantom path + MSB1009 warning.
                        if (include is not null)
                            deps.Add(Path.GetFullPath(Expand(include), csprojDir));

                        if (remove is not null)
                            deps.Remove(Path.GetFullPath(Expand(remove), csprojDir));
                    }
                }
            }

            // A reference that still does not exist is a genuine wiring defect, not something to spend a
            // build call discovering -- report it plainly instead of via an opaque MSB1009.
            foreach (string missing in deps.Where(d => !File.Exists(d)).ToList())
            {
                Console.Error.WriteLine($"\n  WARNING: unresolved ProjectReference (skipped): {missing}");
                deps.Remove(missing);
            }

            Console.Write($"[Compile]  pre-building {deps.Count} shared dependencies... ");

            foreach (string dep in deps)
            {
                ProcResult r = Exec("dotnet",
                    $"build \"{dep}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_behavioralDir, BuildOneTimeoutMs);

                if (r.ExitCode != 0)
                    Console.Error.WriteLine($"\n  WARNING: shared dep build failed ({Path.GetFileName(dep)}): {Truncate(r.StdOut + r.StdErr)}");

                // Record how fresh the dependency outputs are, for the staleness test in SuspectProjects.
                string depBin = Path.Combine(Path.GetDirectoryName(dep)!, "bin");

                if (!Directory.Exists(depBin))
                    continue;

                foreach (string dll in Directory.EnumerateFiles(depBin, "*.dll", SearchOption.AllDirectories))
                {
                    DateTime stamp = File.GetLastWriteTimeUtc(dll);

                    if (stamp > s_sharedDepStamp)
                        s_sharedDepStamp = stamp;
                }
            }

            Console.WriteLine("ok");
        }

        // Value of a double-quoted XML attribute on a line, or null when the attribute is absent.
        private static string? AttributeValue(string line, string attribute)
        {
            int idx = line.IndexOf(attribute, StringComparison.OrdinalIgnoreCase);

            if (idx < 0)
                return null;

            int start = idx + attribute.Length;
            int end = line.IndexOf('"', start);

            return end < 0 ? null : line[start..end];
        }

        // Expands the one MSBuild property the behavioral csprojs use in a ProjectReference path.
        private static string Expand(string path) =>
            path.Replace("$(go2csPath)", s_srcRoot + Path.DirectorySeparatorChar);

        // Writes a traversal MSBuild project that builds all target csprojs in parallel in one call.
        private static string WriteTraversalProject(IReadOnlyList<string> projects)
        {
            string objDir = Path.Combine(AppContext.BaseDirectory, "traversal");
            Directory.CreateDirectory(objDir);
            string projFile = Path.Combine(objDir, "_AllTargets.proj");

            StringBuilder sb = new();
            sb.AppendLine("<Project DefaultTargets=\"BuildAll\">");
            sb.AppendLine("  <ItemGroup>");

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_behavioralDir, p, $"{p}.csproj");
                sb.AppendLine($"    <ProjectToBuild Include=\"{csproj}\" />");
            }

            sb.AppendLine("  </ItemGroup>");
            // Global props (Configuration, go2csPath) passed on the command line propagate to each project.
            // ("Target" is a reserved MSBuild item name, hence ProjectToBuild.) RestoreAll and BuildAll are
            // separate targets because the runner drives them in separate processes -- see RunCompileCSharp.
            sb.AppendLine("  <Target Name=\"RestoreAll\">");
            sb.AppendLine("    <MSBuild Projects=\"@(ProjectToBuild)\" Targets=\"Restore\" BuildInParallel=\"true\" />");
            sb.AppendLine("  </Target>");
            sb.AppendLine("  <Target Name=\"BuildAll\">");
            sb.AppendLine("    <MSBuild Projects=\"@(ProjectToBuild)\" Targets=\"Build\" BuildInParallel=\"true\" />");
            sb.AppendLine("  </Target>");
            sb.AppendLine("</Project>");

            File.WriteAllText(projFile, sb.ToString());
            return projFile;
        }

        private static bool MatchConsoleOutput(string project)
        {
            string packageInfo = Path.Combine(s_behavioralDir, project, "package_info.cs");

            return File.Exists(packageInfo) &&
                   File.ReadLines(packageInfo).Any(l => l.Trim() == "[GoTestMatchingConsoleOutput]");
        }

        private static bool FilesEqual(string a, string b)
        {
            // Compare with line endings normalized (CRLF -> LF). The converter emits CRLF for C# line
            // endings but preserves the Go source's LF inside multi-line string literals, so a golden has
            // mixed CRLF/LF bytes; with core.autocrlf=true git rewrites those in-string LFs to CRLF on
            // checkout. Normalizing endings makes the comparison immune to that round-trip (a pure
            // line-ending diff can only come from autocrlf, never from the deterministic converter), so no
            // real regression signal is lost. NOTE: this relaxes only the golden TEXT comparison -- a
            // project whose COMPILED program embeds and observes a multi-line string literal at runtime
            // (e.g. Solitaire's board) still needs `-text` in .gitattributes so the on-disk .cs keeps LF
            // newlines, else autocrlf corrupts the runtime value.
            return NormalizeLineEndings(File.ReadAllBytes(a))
                .AsSpan()
                .SequenceEqual(NormalizeLineEndings(File.ReadAllBytes(b)));
        }

        // Returns the bytes with every CR (0x0D) removed, collapsing CRLF to LF. The transpiled C# never
        // contains a bare CR, so stripping all CRs is a safe, allocation-light line-ending normalization.
        private static byte[] NormalizeLineEndings(byte[] bytes)
        {
            int count = 0;

            foreach (byte b in bytes)
            {
                if (b != (byte)'\r')
                    bytes[count++] = b;
            }

            Array.Resize(ref bytes, count);
            return bytes;
        }

        private static int Report(IEnumerable<ProjectResult> results, TimeSpan elapsed)
        {
            List<ProjectResult> all = results.ToList();
            List<ProjectResult> failures = all.Where(r => r.Failed).ToList();

            int Count(Phase ph, Status st) => all.Count(r => r.Phases.TryGetValue(ph, out Status s) && s == st);

            Console.WriteLine();
            Console.WriteLine("================ summary ================");
            foreach (Phase ph in Enum.GetValues<Phase>())
            {
                int pass = Count(ph, Status.Pass), fail = Count(ph, Status.Fail), skip = Count(ph, Status.Skip);
                if (pass + fail + skip == 0) continue;
                Console.WriteLine($"  {ph,-9}  pass {pass,4}   fail {fail,4}   skip {skip,4}");
            }

            if (failures.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"---- {failures.Count} failing project(s) ----");
                foreach (ProjectResult r in failures.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase))
                {
                    string phs = string.Join(",", r.Phases.Where(kv => kv.Value == Status.Fail).Select(kv => kv.Key));
                    Console.WriteLine($"  {r.Name} [{phs}]");
                    foreach (string m in r.Messages)
                        Console.WriteLine($"      {m}");
                }
            }

            if (s_goBuildFailures.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"---- {s_goBuildFailures.Count} Go build failure(s) ----");
                foreach (string m in s_goBuildFailures)
                    Console.WriteLine($"  {m}");
            }

            bool ok = failures.Count == 0 && s_goBuildFailures.Count == 0;

            Console.WriteLine();
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")}  ({all.Count} projects, {elapsed.TotalSeconds:N1}s)");
            return ok ? 0 : 1;
        }

        private static HashSet<Phase> ParsePhases(string csv)
        {
            HashSet<Phase> set = new();

            foreach (string token in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                switch (token.ToLowerInvariant())
                {
                    case "transpile": set.Add(Phase.Transpile); break;
                    case "compile": set.Add(Phase.Compile); break;
                    case "target": set.Add(Phase.Target); break;
                    case "output": set.Add(Phase.Output); break;
                    case "all": set.UnionWith(Enum.GetValues<Phase>()); break;
                    default: Console.Error.WriteLine($"Unknown phase: {token}"); break;
                }
            }

            return set;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("""
                BehavioralRunner -- standalone go2cs behavioral test runner (no testhost).

                Usage:
                  BehavioralRunner [--filter <substr>] [--phase <list>] [--update-targets] [--list]

                Options:
                  --filter <substr>     Only projects whose name contains <substr> (case-insensitive).
                  --phase <list>        Comma list of: transpile,compile,target,output,all (default all).
                  --update-targets      Transpile, then copy each .cs to its .cs.target golden, and stop.
                  --list                List matched projects and exit.
                  -h, --help            Show this help.

                Exit code 0 = all matched projects pass; 1 = at least one failure; 2 = usage error.
                """);
        }

        // ---- process execution with timeout + whole-tree kill ----

        private readonly record struct ProcResult(int ExitCode, string StdOut, string StdErr);

        private static ProcResult Exec(string application, string? arguments, string workingDir, int timeoutMs)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = application,
                Arguments = arguments ?? "",
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Disable MSBuild node reuse so in-runner builds never leave worker nodes holding locks.
            startInfo.EnvironmentVariables["MSBUILDDISABLENODEREUSE"] = "1";
            startInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
            startInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "1";

            StringBuilder outBuf = new(), errBuf = new();

            using Process process = new();
            process.StartInfo = startInfo;
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) outBuf.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) errBuf.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(entireProcessTree: true); }
                catch { /* may have exited in the race */ }

                process.WaitForExit(5000);
                return new ProcResult(-1, outBuf.ToString(), $"TIMEOUT after {timeoutMs} ms; killed process tree.\n{errBuf}");
            }

            process.WaitForExit();
            return new ProcResult(process.ExitCode, outBuf.ToString(), errBuf.ToString());
        }

        private static string FirstLine(string text)
        {
            int index = text.IndexOf('\n');

            return (index < 0 ? text : text[..index]).TrimEnd('\r');
        }

        private static string Truncate(string s, int max = 300)
        {
            s = s.Replace("\r", "").Replace("\n", " ").Trim();
            return s.Length <= max ? s : s[..max] + " ...";
        }
    }
}
