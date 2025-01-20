//******************************************************************************************************
//  BehavioralTestBase.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  01/19/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public abstract class BehavioralTestBase
{
    // Move up from here: "...\go2cs\src\Tests\Behavioral\BehavioralTests\bin\Debug\net9.0"
    private const string RootPath = @"..\..\..\..\..\..\..\"; // At "src" folder

    protected static string go2cs { get; private set; }

    protected static void Init(TestContext context)
    {
        string execPath = Directory.GetCurrentDirectory();
        string go2csSrc = Path.GetFullPath($@"{execPath}{RootPath}go2cs\");
        string go2csBin = Path.Combine(go2csSrc, @"bin\");

        if (!Directory.Exists(go2csBin))
            Directory.CreateDirectory(go2csBin);

        go2cs = Path.Combine(go2csBin, "go2cs.exe");

        if (File.Exists(go2cs))
        {
            // Compare exe timestamp to "main.go" see if we need to rebuild
            FileInfo go2csExe = new(go2cs);
            FileInfo mainGo = new(Path.Combine(go2csSrc, "main.go"));

            if (go2csExe.LastWriteTimeUtc >= mainGo.LastWriteTimeUtc)
                return;
        }

        Process build = Process.Start(new ProcessStartInfo("go", $"build -o \"{go2cs}\"")
        {
            WorkingDirectory = go2csSrc
        });

        if (build is null)
            throw new InvalidOperationException("Failed to start \"go build\" process");

        build.WaitForExit();

        if (build.ExitCode != 0)
            throw new InvalidOperationException($"\"go build\" failed with exit code {build.ExitCode:N0}");

        if (!File.Exists(go2cs))
            throw new InvalidOperationException($"Failed to find \"go2cs.exe\" build for testing, check path: {go2cs}");
    }

    public TestContext TestContext { get; set; }

    protected int Exec(string application, string arguments)
    {
        ProcessStartInfo startInfo = new()
        {
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = application,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using Process process = new();
        
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                TestContext?.WriteLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                TestContext?.WriteLine($"[ErrOut]: {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return process.ExitCode;
    }
}
