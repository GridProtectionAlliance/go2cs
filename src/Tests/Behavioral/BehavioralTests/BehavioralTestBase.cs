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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public abstract class BehavioralTestBase
{
    // Move up from here: "...\go2cs\src\Tests\Behavioral\BehavioralTests\bin\Debug\net9.0"
    private const string RootPath = @"..\..\..\..\..\..\..\"; // At "src" folder

    protected const string TestRootPath = @"..\..\..\..\";

    protected static string BinOutput { get; private set; }

    protected static string go2cs { get; private set; }

    private static readonly ConcurrentDictionary<string, object> s_projectLocks = new(StringComparer.OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected static void Init(TestContext context)
    {
        string execPath = Directory.GetCurrentDirectory();
        string go2csSrc = Path.GetFullPath($@"{execPath}{RootPath}go2cs\");
        string go2csBin = Path.Combine(go2csSrc, @"bin\");

        int projectNameIndex = execPath.IndexOf(nameof(BehavioralTests), StringComparison.OrdinalIgnoreCase);

        BinOutput = execPath[(projectNameIndex+nameof(BehavioralTests).Length)..];

        if (!Directory.Exists(go2csBin))
            Directory.CreateDirectory(go2csBin);

        go2cs = Path.Combine(go2csBin, "go2cs.exe");

        if (File.Exists(go2cs))
        {
            // Compare exe timestamp to all "*.go" files to see if we need to rebuild
            FileInfo go2csExe = new(go2cs);
            FileInfo[] goFiles = Directory.GetFiles(go2csSrc, "*.go").Select(fileName => new FileInfo(fileName)).ToArray();

            if (goFiles.All(goFile => go2csExe.LastWriteTime >= goFile.LastWriteTimeUtc))
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

    protected void TranspileProject(string targetProject, bool forceBuild = false)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string csproj = Path.GetFullPath($@"{targetPath}\{targetProject}.csproj");
        string projPath = Path.GetFullPath($"{targetPath}");

        object projectLock = s_projectLocks.GetOrAdd(targetProject, _ => new object());

        lock (projectLock)
        {
            if (!forceBuild && File.Exists(csproj))
            {
                // If all .cs files are newer than associated .go files, skip build
                FileInfo[] goFiles = Directory.GetFiles(projPath, "*.go").Select(fileName => new FileInfo(fileName)).ToArray();
                bool allUpToDate = true;

                foreach (FileInfo goFile in goFiles)
                {
                    FileInfo csFile = new(Path.ChangeExtension(goFile.FullName, ".cs"));

                    if (csFile.Exists && csFile.LastWriteTimeUtc > goFile.LastWriteTimeUtc)
                        continue;

                    allUpToDate = false;
                    break;
                }

                if (allUpToDate)
                    return;
            }

            int exitCode;
            Assert.IsTrue((exitCode = Exec(go2cs, projPath)) == 0, $"go2cs failed with exit code {exitCode:N0}");
        }
    }

    protected void CompileProject(string targetProject, bool forceBuild = false)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projExe = Path.GetFullPath($@"{targetPath}{BinOutput}\{targetProject}.exe");
        string projPath = Path.GetFullPath($"{targetPath}");

        object projectLock = s_projectLocks.GetOrAdd(targetProject, _ => new object());

        lock (projectLock)
        {
            if (!forceBuild && File.Exists(projExe))
            {
                FileInfo projExeInfo = new(projExe);

                // If exe is newer than all .cs source files, can skip build
                if (Directory.GetFiles(projPath, "*.cs").Select(fileName => new FileInfo(fileName)).All(info => projExeInfo.LastWriteTimeUtc > info.LastWriteTimeUtc))
                    return;
            }

            int exitCode;
            Assert.IsTrue((exitCode = Exec("dotnet", $"build \"{Path.Combine(projPath, $"{targetProject}.csproj")}\"")) == 0, $"dotnet build failed with exit code {exitCode:N0}");
        }
    }
}
