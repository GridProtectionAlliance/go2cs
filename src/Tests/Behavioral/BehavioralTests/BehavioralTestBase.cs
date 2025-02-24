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

// Comment out the following line to standard dotnet builds instead of publish profiles for C# projects.
// Using publish profiles can increase run-time startup performance, after first run initialization, but
// increases build times due to extra compile time processing, e.g. trimming analysis, etc.
//#define USE_PUBLISH_PROFILES

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

    protected static string BinOutput { get; private set; } = null!;

    protected static string NetVersion { get; private set; } = null!;

    protected static string go2cs { get; private set; } = null!;

    protected static string PublishProfile { get; private set; } = "win-x64";

    protected static string GetCSExecPath(string projPath, string targetProject)
    {
    #if USE_PUBLISH_PROFILES
        return Path.Combine(projPath, $@"bin\Release\{NetVersion}\publish\{PublishProfile}");
    #else
        return Path.Combine(projPath, $@"bin\Release\{NetVersion}\");
    #endif
    }

    protected static string GetGoExePath(string projPath, string targetProject)
    {
        return Path.Combine(projPath, @"bin\Release\Go");
    }

    private static readonly ConcurrentDictionary<string, object> s_projectLocks = new(StringComparer.OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected static void Init(TestContext context)
    {
        string execPath = Directory.GetCurrentDirectory();
        string go2csSrc = Path.GetFullPath($@"{execPath}{RootPath}go2cs\");
        string go2csBin = Path.Combine(go2csSrc, @"bin\");

        int projectNameIndex = execPath.IndexOf(nameof(BehavioralTests), StringComparison.OrdinalIgnoreCase);

        BinOutput = execPath[(projectNameIndex + nameof(BehavioralTests).Length)..];
        NetVersion = BinOutput.Split(@"\", StringSplitOptions.RemoveEmptyEntries)[^1];

        if (!Directory.Exists(go2csBin))
            Directory.CreateDirectory(go2csBin);

        go2cs = Path.Combine(go2csBin, "go2cs.exe");

        if (File.Exists(go2cs))
        {
            FileInfo go2csExeInfo = new(go2cs);

            // If exe is newer than all .go source files, can skip build
            if (Directory.GetFiles(go2csSrc, "*.go").Select(fileName => new FileInfo(fileName)).All(info => go2csExeInfo.LastWriteTimeUtc > info.LastWriteTimeUtc))
                return;
        }

        int exitCode = Exec(context, "go", $"build -o \"{go2cs}\"", go2csSrc);

        if (exitCode != 0)
            throw new InvalidOperationException($"\"go build\" failed with exit code {exitCode:N0}");

        if (!File.Exists(go2cs))
            throw new InvalidOperationException($"Failed to find \"go2cs.exe\" build for testing, check path: {go2cs}");
    }

    public TestContext? TestContext { get; set; }

    protected int Exec(string application, string? arguments, string? workingDir = null, DataReceivedEventHandler? outputHandler = null)
    {
        return Exec(TestContext, application, arguments, workingDir, outputHandler);
    }

    protected static int Exec(TestContext? context, string application, string? arguments, string? workingDir = null, DataReceivedEventHandler? outputHandler = null)
    {
        ProcessStartInfo startInfo = new()
        {
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = application,
            Arguments = arguments ?? "",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        if (!string.IsNullOrWhiteSpace(workingDir))
            startInfo.WorkingDirectory = workingDir;

        using Process process = new();
        
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        if (outputHandler is null)
        {
            process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    context?.WriteLine(e.Data);
            };
        }
        else
        {
            process.OutputDataReceived += outputHandler;
        }

        if (outputHandler is null)
        {
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    context?.WriteLine($"[ErrOut]: {e.Data}");
            };
        }
        else
        {
            process.ErrorDataReceived += outputHandler;
        }

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

            Assert.AreEqual(0, exitCode = Exec(go2cs, projPath), $"go2cs transpile for \"{targetProject}\" failed with exit code {exitCode:N0}");
        }
    }

    protected void CompileCSProject(string targetProject, bool forceBuild = false)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projPath = Path.GetFullPath($"{targetPath}");
        string csExePath = GetCSExecPath(projPath, targetProject);
        string projExe = Path.Combine(csExePath, $"{targetProject}.exe");
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

            // Compile C# project
        #if USE_PUBLISH_PROFILES
            Assert.AreEqual(0, exitCode = Exec("dotnet", $"publish \"{Path.Combine(projPath, $"{targetProject}.csproj")}\" -f {NetVersion} -r {PublishProfile} -c Release --sc true -o \"{csExePath}\""), $"dotnet publish for \"{targetProject}\" failed with exit code {exitCode:N0}");
        #else
            Assert.AreEqual(0, exitCode = Exec("dotnet", $"build --configuration Release \"{Path.Combine(projPath, $"{targetProject}.csproj")}\""), $"dotnet build for \"{targetProject}\" failed with exit code {exitCode:N0}");
        #endif

            // If matching console output, run once after compile to ensure any first run initialization steps are completed
            if (MatchConsoleOutput(targetProject))
                Assert.AreEqual(0, exitCode = Exec(projExe, null, csExePath), $"Initial run of \"{targetProject}\" failed with exit code {exitCode:N0}");
        }
    }

    protected void CompileGoProject(string targetProject)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projPath = Path.GetFullPath($"{targetPath}");
        string goExePath = GetGoExePath(projPath, targetProject);

        if (!Directory.Exists(goExePath))
            Directory.CreateDirectory(goExePath);
        
        object projectLock = s_projectLocks.GetOrAdd(targetProject, _ => new object());

        lock (projectLock)
        {
            int exitCode;

            // Make sure Go module is initialized
            if (!File.Exists(Path.Combine(projPath, "go.mod")))
                Assert.AreEqual(0, exitCode = Exec("go", $"mod init go2cs/{targetProject}", projPath), $"go build for \"{targetProject}\" failed with exit code {exitCode:N0}");

            // Compile Go project
            Assert.AreEqual(0, exitCode = Exec("go", $"build -o \"{goExePath}\"", projPath), $"go build for \"{targetProject}\" failed with exit code {exitCode:N0}");
        }
    }

    private static bool MatchConsoleOutput(string targetProject)
    {
        // Access "package_info.cs" file for the target test project
        string packageInfoFile = Path.GetFullPath($@"{TestRootPath}{targetProject}\package_info.cs");

        if (!File.Exists(packageInfoFile))
            return false;

        string[] packageInfoLines = File.ReadAllLines(packageInfoFile);

        // Check for "GoTestMatchingConsoleOutput" attribute -- for now, just check for its presence
        // by looking for the attribute name in the file on its own line. Future implementations could
        // load assembly and verify attribute presence via reflection -- this is a simpler approach:
        return packageInfoLines.Any(line => line.Trim().Equals("[GoTestMatchingConsoleOutput]"));
    }
}
