//******************************************************************************************************
//  TargetComparisonTests.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  07/30/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests
{
    [TestClass]
    public class TargetComparisonTests
    {
        // Move up from here: "BehavioralTests\bin\Debug\netcoreapp3.1"
        private const string RootPath = @"..\..\..\..\";

        private static readonly string go2cs;

        static TargetComparisonTests()
        {
            string execPath = Directory.GetCurrentDirectory();
            int index = execPath.LastIndexOf(nameof(BehavioralTests), StringComparison.Ordinal);

            if (index > 0)
                execPath = execPath.Substring(index + nameof(BehavioralTests).Length);

            // For some reason, go2cs compiles into win-x64 folder even though target is AnyCPU - not sure why:
            execPath = Path.Combine(execPath, "win-x64");

            go2cs = Path.GetFullPath($@"..\..\{RootPath}go2cs{execPath}\go2cs.exe");

            if (!File.Exists(go2cs))
                throw new InvalidOperationException($"Failed to find \"go2cs.exe\" build for testing, check path: {go2cs}");
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CheckArrayPassByValueResult() => CheckProjectAgainstTarget("ArrayPassByValue");

        [TestMethod]
        public void CheckExprSwitchResult() => CheckProjectAgainstTarget("ExprSwitch");

        [TestMethod]
        public void CheckTypeSwitchResult() => CheckProjectAgainstTarget("TypeSwitch");

        [TestMethod]
        public void CheckForVariantsResult() => CheckProjectAgainstTarget("ForVariants");

        [TestMethod]
        public void CheckImportOptionsResult() => CheckProjectAgainstTarget("ImportOptions");

        [TestMethod]
        public void CheckInterfaceCastingResult() => CheckProjectAgainstTarget("InterfaceCasting");

        [TestMethod]
        public void CheckInterfaceImplementationResult() => CheckProjectAgainstTarget("InterfaceImplementation");

        [TestMethod]
        public void CheckInterfaceInheritanceResult() => CheckProjectAgainstTarget("InterfaceInheritance");

        [TestMethod]
        public void CheckLambdaFunctionsResult() => CheckProjectAgainstTarget("LambdaFunctions");

        [TestMethod]
        public void CheckPointerToPointerResult() => CheckProjectAgainstTarget("PointerToPointer");

        //[TestMethod]
        //public void DEBUGCheckSortArrayTypeResult() => CheckProjectAgainstTarget("SortArrayType");

        private void CheckProjectAgainstTarget(string targetProject, bool testLegacyTarget = true)
        {
            string projectPath = Path.GetFullPath($"{RootPath}{targetProject}");
            string convertedProjectFile = $@"{projectPath}\{targetProject}.cs";
            string conversionTargetFile;
            int exitCode;

            if (testLegacyTarget)
            {
                conversionTargetFile = $"{convertedProjectFile}.legacy.target";
                Assert.IsTrue((exitCode = Exec(go2cs, $"-o -i -h -c -a {projectPath}")) == 0, $"go2cs failed with exit code {exitCode:N0}");
                Assert.IsTrue(FileMatch(convertedProjectFile, conversionTargetFile), $"Go source file converted to C# \"{convertedProjectFile}\" does not match target \"{conversionTargetFile}\"");
            }

            conversionTargetFile = $"{convertedProjectFile}.target";
            Assert.IsTrue((exitCode = Exec(go2cs, $"-o -i -h {projectPath}")) == 0, $"go2cs failed with exit code {exitCode:N0}");
            Assert.IsTrue(FileMatch(convertedProjectFile, conversionTargetFile), $"Go source file converted to C# \"{convertedProjectFile}\" does not match target \"{conversionTargetFile}\"");
        }

        private int Exec(string application, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = application,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

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

        private static bool FileMatch(string file1, string file2)
        {
            FileStream fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (fileStream1.Length != fileStream2.Length)
            {
                fileStream1.Close();
                fileStream2.Close();
                return false;
            }

            int file1Byte, file2Byte;

            do
            {
                file1Byte = fileStream1.ReadByte();
                file2Byte = fileStream2.ReadByte();
            }
            while (file1Byte == file2Byte && file1Byte != -1);

            fileStream1.Close();
            fileStream2.Close();

            return file1Byte - file2Byte == 0;
        }
    }
}
