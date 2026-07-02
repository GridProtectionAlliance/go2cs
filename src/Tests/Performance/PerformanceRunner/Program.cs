//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/01/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Standalone runner for the go2cs performance comparison suite. For each benchmark project under
// src\Tests\Performance it builds three variants of the same program -- the original Go binary, the
// transpiled C# on the normal JIT runtime, and the transpiled C# as a Native AOT self-contained
// executable -- verifies all three produce identical output (checksums), then measures workload time
// (in-program, excludes startup), process wall time, and peak working set, reducing the samples to a
// markdown report. Phases: Transpile -> Build -> Verify -> Measure.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using PerformanceRunner;

return Runner.Main(args);

namespace PerformanceRunner
{
    internal enum Phase { Transpile, Build, Verify, Measure }

    internal enum Variant { Go, Jit, Aot }

    internal readonly record struct RunSample(double WallMs, double InnerMs, long PeakBytes);

    internal sealed class VariantResult
    {
        public bool BuildOk { get; set; }
        public bool VerifyOk { get; set; }
        public List<RunSample> Samples { get; } = new();
        public string? Message { get; set; }
    }

    internal sealed class ProjectResult
    {
        public required string Name { get; init; }
        public Dictionary<Variant, VariantResult> Variants { get; } = new()
        {
            [Variant.Go] = new VariantResult(),
            [Variant.Jit] = new VariantResult(),
            [Variant.Aot] = new VariantResult()
        };

        public List<string> Messages { get; } = new();
        public bool Failed { get; set; }
    }

    internal static class Runner
    {
        private const int TranspileTimeoutMs = 60_000;
        private const int GoBuildTimeoutMs = 180_000;
        private const int BuildAllTimeoutMs = 300_000;
        private const int BuildOneTimeoutMs = 180_000;
        private const int AotPublishTimeoutMs = 600_000;
        private const int RunTimeoutMs = 120_000;

        private const string Config = "Release";
        private const string NetVersion = "net9.0";

        // Preferred report order; projects not listed are appended alphabetically.
        private static readonly string[] s_reportOrder =
        {
            "PerfStartup", "PerfFib", "PerfSieve", "PerfMatMul",
            "PerfString", "PerfMap", "PerfSort", "PerfChannel"
        };

        private static string s_srcRoot = null!;
        private static string s_perfDir = null!;
        private static string s_converterSrc = null!;
        private static string s_go2csExe = null!;

        public static int Main(string[] args)
        {
            // The report uses non-ASCII glyphs (× ratios, · separators); default console codepage mangles them.
            Console.OutputEncoding = Encoding.UTF8;

            // ----- argument parsing -----
            string? filter = null;
            bool listOnly = false;
            bool noAot = false;
            bool updateReadme = false;
            int runs = 5;
            HashSet<Phase> phases = new() { Phase.Transpile, Phase.Build, Phase.Verify, Phase.Measure };

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
                    case "--runs" when i + 1 < args.Length:
                        if (!int.TryParse(args[++i], out runs) || runs < 1)
                        {
                            Console.Error.WriteLine("--runs requires a positive integer");
                            return 2;
                        }
                        break;
                    case "--no-aot":
                        noAot = true;
                        break;
                    case "--update-readme":
                        updateReadme = true;
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
            // Runner lives at src\Tests\Performance\PerformanceRunner; performance dir is its parent.
            s_perfDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\.."));
            s_srcRoot = Path.GetFullPath(Path.Combine(s_perfDir, @"..\.."));
            s_converterSrc = Path.Combine(s_srcRoot, "go2cs");
            s_go2csExe = Path.Combine(s_converterSrc, "bin", "go2cs.exe");

            // ----- discover projects -----
            // A benchmark project is a folder with Go source; the generated .csproj appears after the
            // first transpile. This naturally excludes this runner utility (no .go).
            List<string> projects = Directory.GetDirectories(s_perfDir)
                .Where(d => Directory.GetFiles(d, "*.go").Length > 0)
                .Select(Path.GetFileName)
                .Where(n => filter is null || n!.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(ReportIndex)
                .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
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
                Console.Error.WriteLine("No benchmark projects matched.");
                return 2;
            }

            Console.WriteLine($"go2cs performance runner: {projects.Count} project(s), phases [{string.Join(", ", phases)}], {runs} measured run(s){(noAot ? ", AOT disabled" : "")}");
            Stopwatch total = Stopwatch.StartNew();

            Dictionary<string, ProjectResult> results = projects.ToDictionary(p => p, p => new ProjectResult { Name = p });

            // ----- Phase: Transpile -----
            if (phases.Contains(Phase.Transpile))
            {
                if (!EnsureConverterBuilt())
                    return 1;

                RunTranspile(projects, results);
            }

            // ----- Phase: Build (Go binary + C# JIT + C# Native AOT) -----
            if (phases.Contains(Phase.Build))
            {
                RunBuildGo(projects, results);
                RunBuildJit(projects, results);

                if (!noAot)
                    RunBuildAot(projects, results);
            }
            else
            {
                // Assume prior build outputs exist; verified per exe below.
                foreach (ProjectResult r in results.Values)
                foreach (VariantResult v in r.Variants.Values)
                    v.BuildOk = true;
            }

            // ----- Phase: Verify (identical output across variants) -----
            if (phases.Contains(Phase.Verify))
                RunVerify(projects, results, noAot);

            // ----- Phase: Measure -----
            if (phases.Contains(Phase.Measure))
            {
                RunMeasure(projects, results, runs, noAot);

                string markdown = BuildMarkdown(projects, results, runs, noAot);
                Console.WriteLine();
                Console.WriteLine(markdown);

                if (updateReadme && !UpdateReadme(markdown))
                    return 1;
            }

            total.Stop();

            // ----- summary -----
            List<ProjectResult> failures = results.Values.Where(r => r.Failed).ToList();

            if (failures.Count > 0)
            {
                Console.WriteLine($"---- {failures.Count} failing project(s) ----");

                foreach (ProjectResult r in failures)
                {
                    Console.WriteLine($"  {r.Name}");
                    foreach (string m in r.Messages)
                        Console.WriteLine($"      {m}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{(failures.Count == 0 ? "PASS" : "FAIL")}  ({projects.Count} projects, {total.Elapsed.TotalSeconds:N1}s)");
            return failures.Count == 0 ? 0 : 1;
        }

        private static int ReportIndex(string? name)
        {
            int idx = Array.IndexOf(s_reportOrder, name);
            return idx < 0 ? int.MaxValue : idx;
        }

        // ---- Phase: Transpile ----

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
            ProcResult r = Exec("go", $"build -o \"{s_go2csExe}\"", s_converterSrc, GoBuildTimeoutMs);

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
                string projPath = Path.Combine(s_perfDir, p);

                if (UpToDate(projPath))
                    continue;

                ProcResult r = Exec(s_go2csExe, $"\"{projPath}\"", projPath, TranspileTimeoutMs);

                if (r.ExitCode != 0)
                {
                    results[p].Failed = true;
                    results[p].Messages.Add($"transpile exit {r.ExitCode}: {Truncate(r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        // A project is up to date when every .cs is newer than its matching .go source.
        private static bool UpToDate(string projPath)
        {
            foreach (string go in Directory.GetFiles(projPath, "*.go"))
            {
                string cs = Path.ChangeExtension(go, ".cs");

                if (!File.Exists(cs) || File.GetLastWriteTimeUtc(cs) <= File.GetLastWriteTimeUtc(go))
                    return false;
            }

            return true;
        }

        // ---- Phase: Build ----

        private static void RunBuildGo(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            Console.Write($"[Build]    Go binaries... ");
            int failed = 0;

            foreach (string p in projects)
            {
                string projPath = Path.Combine(s_perfDir, p);
                string goExe = GetExePath(p, Variant.Go);
                Directory.CreateDirectory(Path.GetDirectoryName(goExe)!);

                ProcResult r = Exec("go", $"build -o \"{goExe}\" .", projPath, GoBuildTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Variants[Variant.Go].BuildOk = true;
                }
                else
                {
                    results[p].Failed = true;
                    results[p].Messages.Add($"go build exit {r.ExitCode}: {Truncate(r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static void RunBuildJit(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            string go2csPathArg = Go2csPathArg();

            // Pre-build the shared dependencies (golib, the go2cs-gen analyzer, and the core/* packages
            // the targets reference) sequentially first, so the parallel target fan-out never races on
            // their obj/bin outputs (same MSB3026/27 mitigation as the BehavioralRunner).
            PreBuildSharedDeps(projects, go2csPathArg);

            Console.Write($"[Build]    C# JIT (one-shot parallel build of {projects.Count})... ");

            string traversal = WriteTraversalProject(projects);

            ProcResult all = Exec("dotnet",
                $"build \"{traversal}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                s_perfDir, BuildAllTimeoutMs);

            if (all.ExitCode == 0)
            {
                foreach (string p in projects)
                    results[p].Variants[Variant.Jit].BuildOk = true;

                Console.WriteLine("ok");
                return;
            }

            // Build-all failed: fall back to per-project builds to attribute the failure(s).
            Console.WriteLine("build-all reported errors; attributing per project...");

            int failed = 0;

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");

                ProcResult r = Exec("dotnet",
                    $"build \"{csproj}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_perfDir, BuildOneTimeoutMs);

                if (r.ExitCode == 0)
                {
                    results[p].Variants[Variant.Jit].BuildOk = true;
                }
                else
                {
                    results[p].Failed = true;
                    results[p].Messages.Add($"C# build exit {r.ExitCode}: {Truncate(r.StdOut + r.StdErr)}");
                    failed++;
                }
            }

            Console.WriteLine($"[Build]    C# JIT per-project: {failed} failed");
        }

        private static void RunBuildAot(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results)
        {
            string go2csPathArg = Go2csPathArg();

            // The ILC native link step probes for MSVC link.exe via vswhere.exe and assumes it is on
            // PATH; prepend the VS Installer directory so the probe resolves cleanly.
            string vsInstaller = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft Visual Studio", "Installer");

            Dictionary<string, string>? env = null;

            if (Directory.Exists(vsInstaller))
                env = new Dictionary<string, string> { ["PATH"] = vsInstaller + ";" + Environment.GetEnvironmentVariable("PATH") };

            Console.WriteLine($"[Build]    C# Native AOT ({projects.Count} publish(es), sequential -- slow)...");

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");
                string outDir = Path.GetDirectoryName(GetExePath(p, Variant.Aot))!;

                Console.Write($"           {p}... ");
                Stopwatch sw = Stopwatch.StartNew();

                ProcResult r = Exec("dotnet",
                    $"publish \"{csproj}\" -nologo -clp:ErrorsOnly -c {Config} -p:PerfAot=true -p:go2csPath={go2csPathArg} -o \"{outDir}\"",
                    s_perfDir, AotPublishTimeoutMs, env);

                if (r.ExitCode == 0)
                {
                    results[p].Variants[Variant.Aot].BuildOk = true;
                    Console.WriteLine($"ok ({sw.Elapsed.TotalSeconds:N0}s)");
                }
                else
                {
                    // An AOT failure degrades that column to n/a rather than failing the whole run;
                    // it is still reported.
                    results[p].Variants[Variant.Aot].Message = "publish failed";
                    results[p].Messages.Add($"AOT publish exit {r.ExitCode}: {Truncate(r.StdOut + r.StdErr)}");
                    Console.WriteLine("FAILED (column reported as n/a)");
                }
            }
        }

        // ---- Phase: Verify ----

        private static void RunVerify(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, bool noAot)
        {
            Console.Write($"[Verify]   comparing Go vs C# output... ");
            int failed = 0;

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                string projPath = Path.Combine(s_perfDir, p);
                string? goOutput = null;

                foreach (Variant v in Enum.GetValues<Variant>())
                {
                    if (v == Variant.Aot && noAot)
                        continue;

                    VariantResult vr = result.Variants[v];

                    if (!vr.BuildOk)
                        continue;

                    string exe = GetExePath(p, v);

                    if (!File.Exists(exe))
                    {
                        vr.BuildOk = false;
                        vr.Message = "exe missing";
                        continue;
                    }

                    ProcResult r = Exec(exe, null, projPath, RunTimeoutMs);

                    if (r.ExitCode != 0)
                    {
                        vr.Message = $"exit {r.ExitCode}";
                        result.Failed = true;
                        result.Messages.Add($"{v} run exit {r.ExitCode}: {Truncate(r.StdErr)}");
                        failed++;
                        continue;
                    }

                    // Timing lines differ between runs by construction; compare everything else.
                    string filtered = FilterTimingLines(r.StdOut);

                    if (v == Variant.Go)
                    {
                        goOutput = filtered;
                        vr.VerifyOk = true;
                    }
                    else if (goOutput is null)
                    {
                        vr.Message = "no Go output to compare";
                    }
                    else if (string.Equals(filtered, goOutput, StringComparison.Ordinal))
                    {
                        vr.VerifyOk = true;
                    }
                    else
                    {
                        vr.Message = "output mismatch vs Go";
                        result.Failed = true;
                        result.Messages.Add($"{v} output mismatch vs Go: [{Truncate(filtered, 120)}] vs [{Truncate(goOutput, 120)}]");
                        failed++;
                    }
                }
            }

            Console.WriteLine(failed == 0 ? "ok" : $"{failed} failed");
        }

        private static string FilterTimingLines(string output)
        {
            return string.Join('\n', output
                .Replace("\r", "")
                .Split('\n')
                .Where(l => !l.StartsWith("elapsed_ns:", StringComparison.Ordinal)));
        }

        // ---- Phase: Measure ----

        private static void RunMeasure(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, int runs, bool noAot)
        {
            Console.WriteLine($"[Measure]  1 warmup + {runs} run(s) per variant...");

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                string projPath = Path.Combine(s_perfDir, p);
                Console.Write($"           {p}... ");

                foreach (Variant v in Enum.GetValues<Variant>())
                {
                    if (v == Variant.Aot && noAot)
                        continue;

                    VariantResult vr = result.Variants[v];

                    if (!vr.BuildOk || !vr.VerifyOk)
                        continue;

                    string exe = GetExePath(p, v);

                    // Warmup run (OS file cache, AV scan of the exe) -- discarded.
                    RunMeasured(exe, projPath);

                    for (int i = 0; i < runs; i++)
                    {
                        (int exitCode, string stdOut, double wallMs, long peakBytes) = RunMeasured(exe, projPath);

                        if (exitCode != 0)
                        {
                            vr.Message = $"measure run exit {exitCode}";
                            result.Failed = true;
                            result.Messages.Add($"{v} measure run exit {exitCode}");
                            break;
                        }

                        vr.Samples.Add(new RunSample(wallMs, ParseInnerMs(stdOut), peakBytes));
                    }
                }

                Console.WriteLine("done");
            }
        }

        private static double ParseInnerMs(string stdOut)
        {
            foreach (string line in stdOut.Split('\n'))
            {
                if (!line.StartsWith("elapsed_ns:", StringComparison.Ordinal))
                    continue;

                if (long.TryParse(line["elapsed_ns:".Length..].Trim(), out long ns))
                    return ns / 1_000_000.0;
            }

            return 0.0;
        }

        private static (int exitCode, string stdOut, double wallMs, long peakBytes) RunMeasured(string exe, string workDir)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exe,
                Arguments = "",
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            StringBuilder outBuf = new();

            using Process process = new();
            process.StartInfo = startInfo;
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) outBuf.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, _) => { };

            Stopwatch sw = Stopwatch.StartNew();
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Peak working set is only queryable while the process is alive; poll on a tight loop.
            // The counter is monotonic, so the last successful sample is the peak (to within ~1 ms).
            // Sample immediately so even a near-instant process (Startup) yields at least one reading.
            long peak = 0;

            try
            {
                peak = process.PeakWorkingSet64;
            }
            catch
            {
                // Process exited before the first sample.
            }

            while (!process.WaitForExit(1))
            {
                try
                {
                    process.Refresh();
                    long ws = process.PeakWorkingSet64;

                    if (ws > peak)
                        peak = ws;
                }
                catch
                {
                    // Process exited between the wait and the query.
                }

                if (sw.ElapsedMilliseconds > RunTimeoutMs)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch { /* may have exited in the race */ }

                    process.WaitForExit(5000);
                    return (-1, outBuf.ToString(), sw.Elapsed.TotalMilliseconds, peak);
                }
            }

            sw.Stop();
            process.WaitForExit();  // flush async output handlers

            return (process.ExitCode, outBuf.ToString(), sw.Elapsed.TotalMilliseconds, peak);
        }

        // ---- Report ----

        private static double Median(IEnumerable<double> values)
        {
            List<double> sorted = values.OrderBy(v => v).ToList();

            if (sorted.Count == 0)
                return 0.0;

            int mid = sorted.Count / 2;
            return sorted.Count % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2.0;
        }

        private static string DisplayName(string project)
        {
            return project.StartsWith("Perf", StringComparison.Ordinal) ? project[4..] : project;
        }

        // The Startup benchmark has an empty workload; its meaningful number is process wall time.
        // Every other benchmark reports the in-program workload time (excludes startup + fmt setup).
        private static double TimeMetric(string project, VariantResult vr)
        {
            return project == "PerfStartup" ? Median(vr.Samples.Select(s => s.WallMs)) : Median(vr.Samples.Select(s => s.InnerMs));
        }

        private static string BuildMarkdown(IReadOnlyList<string> projects, Dictionary<string, ProjectResult> results, int runs, bool noAot)
        {
            StringBuilder sb = new();
            CultureInfo ci = CultureInfo.InvariantCulture;

            string goVersion = "go";
            ProcResult goVer = Exec("go", "version", s_perfDir, 15_000);

            if (goVer.ExitCode == 0)
            {
                string[] parts = goVer.StdOut.Trim().Split(' ');
                if (parts.Length >= 3)
                    goVersion = parts[2];
            }

            string sdkVersion = "?";
            ProcResult sdkVer = Exec("dotnet", "--version", s_perfDir, 30_000);

            if (sdkVer.ExitCode == 0)
                sdkVersion = sdkVer.StdOut.Trim();

            string cpu = GetCpuName();
            string os = RuntimeInformation.OSDescription.Trim();

            sb.AppendLine($"**Environment:** {cpu} · {os} · {goVersion} · .NET SDK {sdkVersion} · {DateTime.Now:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine($"C# builds: JIT = framework-dependent `Release`; Native AOT = `-p:PublishAot=true` self-contained, partial trim. Median of {runs} runs (1 discarded warmup). Workload time is measured in-program and excludes process startup; the Startup row is pure process wall time. Ratios are relative to Go.");
            sb.AppendLine();
            sb.AppendLine("**Execution time** (milliseconds -- lower is better):");
            sb.AppendLine();
            AppendTable(sb, projects, results, noAot, ci,
                (p, vr) => TimeMetric(p, vr),
                (value, goValue) => FormatTimeCell(value, goValue, ci));

            sb.AppendLine();
            sb.AppendLine("**Peak memory** (working set, MB -- lower is better):");
            sb.AppendLine();
            AppendTable(sb, projects, results, noAot, ci,
                (_, vr) => Median(vr.Samples.Select(s => (double)s.PeakBytes)) / (1024.0 * 1024.0),
                (value, _) => value.ToString("N1", ci));

            return sb.ToString();
        }

        private static string FormatTimeCell(double value, double goValue, CultureInfo ci)
        {
            string cell = value.ToString("N1", ci);

            if (goValue > 0.0)
                cell += $" ({(value / goValue).ToString("N2", ci)}×)";

            return cell;
        }

        private static void AppendTable(StringBuilder sb, IReadOnlyList<string> projects,
            Dictionary<string, ProjectResult> results, bool noAot, CultureInfo ci,
            Func<string, VariantResult, double> metric, Func<double, double, string> format)
        {
            sb.AppendLine("| Benchmark | Go | C# (JIT) | C# (Native AOT) |");
            sb.AppendLine("|---|---:|---:|---:|");

            foreach (string p in projects)
            {
                ProjectResult result = results[p];
                VariantResult go = result.Variants[Variant.Go];
                double goValue = go.Samples.Count > 0 ? metric(p, go) : 0.0;

                sb.Append($"| {DisplayName(p)} ");

                foreach (Variant v in Enum.GetValues<Variant>())
                {
                    VariantResult vr = result.Variants[v];

                    if ((v == Variant.Aot && noAot) || vr.Samples.Count == 0)
                    {
                        sb.Append("| n/a ");
                        continue;
                    }

                    double value = metric(p, vr);
                    sb.Append($"| {(v == Variant.Go ? value.ToString("N1", ci) : format(value, goValue))} ");
                }

                sb.AppendLine("|");
            }
        }

        private static string GetCpuName()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using Microsoft.Win32.RegistryKey? key =
                        Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");

                    if (key?.GetValue("ProcessorNameString") is string name)
                        return name.Trim();
                }
            }
            catch
            {
                // fall through to the environment variable
            }

            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown CPU";
        }

        private static bool UpdateReadme(string markdown)
        {
            const string BeginMarker = "<!-- PERF-RESULTS:BEGIN -->";
            const string EndMarker = "<!-- PERF-RESULTS:END -->";

            string readme = Path.Combine(s_perfDir, "README.md");

            if (!File.Exists(readme))
            {
                Console.Error.WriteLine($"README not found: {readme}");
                return false;
            }

            string text = File.ReadAllText(readme);
            int begin = text.IndexOf(BeginMarker, StringComparison.Ordinal);
            int end = text.IndexOf(EndMarker, StringComparison.Ordinal);

            if (begin < 0 || end < 0 || end < begin)
            {
                Console.Error.WriteLine($"README markers not found ({BeginMarker} / {EndMarker}); not updated.");
                return false;
            }

            string updated = text[..(begin + BeginMarker.Length)] + "\n\n" + markdown.TrimEnd() + "\n\n" + text[end..];
            File.WriteAllText(readme, updated);
            Console.WriteLine($"Updated results block in {readme}");
            return true;
        }

        // ---- Build helpers ----

        private static string Go2csPathArg() => s_srcRoot.Replace('\\', '/').TrimEnd('/') + "/";

        private static string GetExePath(string project, Variant variant)
        {
            string projPath = Path.Combine(s_perfDir, project);

            return variant switch
            {
                Variant.Go => Path.Combine(projPath, "bin", Config, "Go", $"{project}.exe"),
                Variant.Jit => Path.Combine(projPath, "bin", Config, NetVersion, $"{project}.exe"),
                Variant.Aot => Path.Combine(projPath, "bin", Config, "aot", $"{project}.exe"),
                _ => throw new ArgumentOutOfRangeException(nameof(variant))
            };
        }

        // Builds the deduped union of ProjectReferences across the target csprojs (golib, the analyzer,
        // core/* packages) one at a time, so they are up to date before the parallel target fan-out.
        private static void PreBuildSharedDeps(IReadOnlyList<string> projects, string go2csPathArg)
        {
            HashSet<string> deps = new(StringComparer.OrdinalIgnoreCase);

            foreach (string p in projects)
            {
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");

                if (!File.Exists(csproj))
                    continue;

                string csprojDir = Path.GetDirectoryName(csproj)!;

                foreach (string line in File.ReadLines(csproj))
                {
                    int idx = line.IndexOf("ProjectReference Include=\"", StringComparison.OrdinalIgnoreCase);
                    if (idx < 0) continue;

                    int start = idx + "ProjectReference Include=\"".Length;
                    int end = line.IndexOf('"', start);
                    if (end < 0) continue;

                    string raw = line[start..end].Replace("$(go2csPath)", s_srcRoot + Path.DirectorySeparatorChar);
                    deps.Add(Path.GetFullPath(raw, csprojDir));
                }
            }

            Console.Write($"[Build]    pre-building {deps.Count} shared dependencies... ");

            foreach (string dep in deps)
            {
                ProcResult r = Exec("dotnet",
                    $"build \"{dep}\" -nologo -clp:ErrorsOnly -p:Configuration={Config} -p:go2csPath={go2csPathArg}",
                    s_perfDir, BuildOneTimeoutMs);

                if (r.ExitCode != 0)
                    Console.Error.WriteLine($"\n  WARNING: shared dep build failed ({Path.GetFileName(dep)}): {Truncate(r.StdOut + r.StdErr)}");
            }

            Console.WriteLine("ok");
        }

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
                string csproj = Path.Combine(s_perfDir, p, $"{p}.csproj");
                sb.AppendLine($"    <ProjectToBuild Include=\"{csproj}\" />");
            }

            sb.AppendLine("  </ItemGroup>");
            sb.AppendLine("  <Target Name=\"BuildAll\">");
            sb.AppendLine("    <MSBuild Projects=\"@(ProjectToBuild)\" Targets=\"Build\" BuildInParallel=\"true\" />");
            sb.AppendLine("  </Target>");
            sb.AppendLine("</Project>");

            File.WriteAllText(projFile, sb.ToString());
            return projFile;
        }

        private static HashSet<Phase> ParsePhases(string csv)
        {
            HashSet<Phase> set = new();

            foreach (string token in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                switch (token.ToLowerInvariant())
                {
                    case "transpile": set.Add(Phase.Transpile); break;
                    case "build": set.Add(Phase.Build); break;
                    case "verify": set.Add(Phase.Verify); break;
                    case "measure": set.Add(Phase.Measure); break;
                    case "all": set.UnionWith(Enum.GetValues<Phase>()); break;
                    default: Console.Error.WriteLine($"Unknown phase: {token}"); break;
                }
            }

            return set;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("""
                PerformanceRunner -- go2cs Go vs transpiled C# performance comparison.

                Builds each benchmark three ways (Go binary, C# JIT, C# Native AOT self-contained),
                verifies identical program output, then measures workload time, process wall time,
                and peak working set, reporting a markdown summary.

                Usage:
                  PerformanceRunner [--filter <substr>] [--phase <list>] [--runs <n>] [--no-aot]
                                    [--update-readme] [--list]

                Options:
                  --filter <substr>     Only projects whose name contains <substr> (case-insensitive).
                  --phase <list>        Comma list of: transpile,build,verify,measure,all (default all).
                  --runs <n>            Measured runs per variant (default 5; +1 discarded warmup).
                  --no-aot              Skip the Native AOT column (much faster builds).
                  --update-readme       Rewrite the results block in ..\README.md (between the
                                        PERF-RESULTS markers) with this run's tables.
                  --list                List matched projects and exit.
                  -h, --help            Show this help.

                Exit code 0 = pass, 1 = failure, 2 = usage error.
                """);
        }

        // ---- process execution with timeout + whole-tree kill ----

        private readonly record struct ProcResult(int ExitCode, string StdOut, string StdErr);

        private static ProcResult Exec(string application, string? arguments, string workingDir, int timeoutMs,
            Dictionary<string, string>? environment = null)
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

            if (environment is not null)
            {
                foreach ((string key, string value) in environment)
                    startInfo.EnvironmentVariables[key] = value;
            }

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

        private static string Truncate(string s, int max = 300)
        {
            s = s.Replace("\r", "").Replace("\n", " ").Trim();
            return s.Length <= max ? s : s[..max] + " ...";
        }
    }
}
