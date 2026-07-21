// AssemblySetup.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public sealed class AssemblySetup
{
    // NOTE: We deliberately do NOT kill stray "testhost" processes here. This code runs *inside* the
    // current testhost, so a blanket kill would terminate the run itself. Worse, the prior-run lock a
    // stale testhost holds on BehavioralTests.dll manifests at *build* time (MSB3027) — before this
    // assembly can even load — so the only effective place to clear stale hosts is a pre-build step
    // outside the test process. That lives in run-behavioral-tests.ps1 (the recommended entry point).
    [AssemblyCleanup]
    public static void Cleanup()
    {
        // Release any MSBuild build-server nodes spawned by the in-test "dotnet build" calls so they
        // don't linger holding file locks on this assembly / target bin+obj for the next run. Node
        // reuse is already disabled per child (MSBUILDDISABLENODEREUSE) in BehavioralTestBase.Exec;
        // this is belt-and-suspenders teardown. Best-effort only — never fail the run on cleanup.
        try
        {
            using Process process = new();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build-server shutdown",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.Start();

            if (!process.WaitForExit(30_000))
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Best-effort teardown; ignore any failure.
        }
    }
}
