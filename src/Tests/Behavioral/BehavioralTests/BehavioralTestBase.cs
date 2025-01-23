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

    protected static string BinOutput { get; private set; } = null!;

    protected static string NetVersion { get; private set; } = null!;

    protected static string go2cs { get; private set; } = null!;

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
            // Compare exe timestamp to "main.go" see if we need to rebuild
            FileInfo go2csExe = new(go2cs);
            FileInfo mainGo = new(Path.Combine(go2csSrc, "main.go"));

            if (go2csExe.LastWriteTime >= mainGo.LastWriteTimeUtc)
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
            Assert.IsTrue((exitCode = Exec(go2cs, projPath)) == 0, $"go2cs transpile for \"{targetProject}\" failed with exit code {exitCode:N0}");
        }
    }

    protected void CompileCSProject(string targetProject, bool forceBuild = false)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projPath = Path.GetFullPath($"{targetPath}");
        string projExe = Path.Combine(projPath, $@"bin\Release\{NetVersion}\{targetProject}.exe");

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
            Assert.IsTrue((exitCode = Exec("dotnet", $"build --configuration Release \"{Path.Combine(projPath, $"{targetProject}.csproj")}\"")) == 0, $"dotnet build for \"{targetProject}\" failed with exit code {exitCode:N0}");
        }
    }

    protected void CompileGoProject(string targetProject)
    {
        string targetPath = $"{TestRootPath}{targetProject}";
        string projPath = Path.GetFullPath($"{targetPath}");
        string outputPath = Path.Combine(projPath, @"bin\Release\Go");

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        int exitCode;

        // Make sure Go module is initialized
        if (!File.Exists(Path.Combine(projPath, "go.mod")))
            Assert.IsTrue((exitCode = Exec("go", $"mod init go2cs/{targetProject}", projPath)) == 0, $"go build for \"{targetProject}\" failed with exit code {exitCode:N0}");

        // Compile Go project
        Assert.IsTrue((exitCode = Exec("go", $"build -o \"{outputPath}\"", projPath)) == 0, $"go build for \"{targetProject}\" failed with exit code {exitCode:N0}");
    }
}
