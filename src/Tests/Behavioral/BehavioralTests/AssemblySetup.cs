//******************************************************************************************************
//  AssemblySetup.cs - Gbtc
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
//  06/30/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

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
