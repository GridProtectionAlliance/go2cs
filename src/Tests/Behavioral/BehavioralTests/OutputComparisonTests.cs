//******************************************************************************************************
//  OutputComparisonTests.cs - Gbtc
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
//  01/22/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public class D4_OutputComparisonTests : BehavioralTestBase
{
    [ClassInitialize]
    public static void Initialize(TestContext context) => Init(context);

    // Run "UpdateTestTargets" utility to add new project test methods below this line
    // Only projects marked with "GoTestMatchingConsoleOutput" attribute will be added

    // <TestMethods>

    [TestMethod]
    public void CheckAnonymousInterfaces() => CheckTarget("AnonymousInterfaces");

    [TestMethod]
    public void CheckAnyKeyMap() => CheckTarget("AnyKeyMap");

    [TestMethod]
    public void CheckArrayPassByValue() => CheckTarget("ArrayPassByValue");

    [TestMethod]
    public void CheckChannelReceiveFromClosed() => CheckTarget("ChannelReceiveFromClosed");

    [TestMethod]
    public void CheckDeferCallOrder() => CheckTarget("DeferCallOrder");

    [TestMethod]
    public void CheckDeferClosure() => CheckTarget("DeferClosure");

    [TestMethod]
    public void CheckDeferEvalParam() => CheckTarget("DeferEvalParam");

    [TestMethod]
    public void CheckDeferEvalParamFunc() => CheckTarget("DeferEvalParamFunc");

    [TestMethod]
    public void CheckDeferLambdaParam() => CheckTarget("DeferLambdaParam");

    [TestMethod]
    public void CheckExprSwitch() => CheckTarget("ExprSwitch");

    [TestMethod]
    public void CheckGenericCompositeLiterals() => CheckTarget("GenericCompositeLiterals");

    [TestMethod]
    public void CheckGenericCompositeType() => CheckTarget("GenericCompositeType");

    [TestMethod]
    public void CheckGenericFuncCall() => CheckTarget("GenericFuncCall");

    [TestMethod]
    public void CheckGenericFuncDecl() => CheckTarget("GenericFuncDecl");

    [TestMethod]
    public void CheckGenericTypeAssertions() => CheckTarget("GenericTypeAssertions");

    [TestMethod]
    public void CheckGenericTypeDecl() => CheckTarget("GenericTypeDecl");

    [TestMethod]
    public void CheckGenericTypeInference() => CheckTarget("GenericTypeInference");

    [TestMethod]
    public void CheckGenericTypeInstantiation() => CheckTarget("GenericTypeInstantiation");

    [TestMethod]
    public void CheckInterfaceImplementation() => CheckTarget("InterfaceImplementation");

    [TestMethod]
    public void CheckInterfaceIntraFunction() => CheckTarget("InterfaceIntraFunction");

    [TestMethod]
    public void CheckIotaEnum() => CheckTarget("IotaEnum");

    [TestMethod]
    public void CheckLambdaFunctions() => CheckTarget("LambdaFunctions");

    [TestMethod]
    public void CheckMethodSelector() => CheckTarget("MethodSelector");

    [TestMethod]
    public void CheckPanicRecover() => CheckTarget("PanicRecover");

    [TestMethod]
    public void CheckPointerToPointer() => CheckTarget("PointerToPointer");

    [TestMethod]
    public void CheckSolitaire() => CheckTarget("Solitaire");

    [TestMethod]
    public void CheckSortArrayType() => CheckTarget("SortArrayType");

    [TestMethod]
    public void CheckSpreadOperator() => CheckTarget("SpreadOperator");

    [TestMethod]
    public void CheckStdLibInternalAbi() => CheckTarget("StdLibInternalAbi");

    [TestMethod]
    public void CheckStringPassByValue() => CheckTarget("StringPassByValue");

    [TestMethod]
    public void CheckStructPointerPromotionWithInterface() => CheckTarget("StructPointerPromotionWithInterface");

    [TestMethod]
    public void CheckStructPromotion() => CheckTarget("StructPromotion");

    [TestMethod]
    public void CheckStructPromotionWithInterface() => CheckTarget("StructPromotionWithInterface");

    [TestMethod]
    public void CheckStructWithDelegate() => CheckTarget("StructWithDelegate");

    [TestMethod]
    public void CheckTypeAssert() => CheckTarget("TypeAssert");

    [TestMethod]
    public void CheckTypeConversion() => CheckTarget("TypeConversion");

    [TestMethod]
    public void CheckTypeConversionInterfaceParam() => CheckTarget("TypeConversionInterfaceParam");

    [TestMethod]
    public void CheckTypeInference() => CheckTarget("TypeInference");

    [TestMethod]
    public void CheckTypeSwitch() => CheckTarget("TypeSwitch");

    [TestMethod]
    public void CheckUnsafeOperations() => CheckTarget("UnsafeOperations");

    [TestMethod]
    public void CheckVariableCapture() => CheckTarget("VariableCapture");

    // </TestMethods>

    private void CheckTarget(string targetProject)
    {
        string projPath = Path.GetFullPath($"{TestRootPath}{targetProject}");

        // Transpile project, if needed
        TranspileProject(targetProject);

        // Compile C# project, if needed
        CompileCSProject(targetProject);

        // Compile Go project
        CompileGoProject(targetProject);

        // Set stop watch for performance measurement
        Stopwatch stopwatch = new();

        string csExe = Path.Combine(GetCSExecPath(projPath, targetProject), $"{targetProject}.exe");
        Assert.IsTrue(File.Exists(csExe), $"Expected C# executable does not exist: {csExe}");

        StringBuilder csOutput = new();

        stopwatch.Start();
        int csExitCode = Exec(csExe, null, Path.GetDirectoryName(projPath), csOutputHandler);
        stopwatch.Stop();

        Assert.AreEqual(0, csExitCode, $"C# executable failed with exit code {csExitCode:N0}");
        TestContext?.WriteLine($"C# execution Time: {stopwatch.ElapsedMilliseconds:N0} ms");

        string goExe = Path.Combine(GetGoExePath(projPath, targetProject), $"{targetProject}.exe");
        Assert.IsTrue(File.Exists(goExe), $"Expected Go executable does not exist: {goExe}");

        StringBuilder goOutput = new();
        
        stopwatch.Restart();
        int goExitCode = Exec(goExe, null, Path.GetDirectoryName(projPath), goOutputHandler);
        stopwatch.Stop();

        Assert.AreEqual(0, goExitCode, $"Go executable failed with exit code {goExitCode:N0}");
        TestContext?.WriteLine($"Go execution Time: {stopwatch.ElapsedMilliseconds:N0} ms");

        Assert.AreEqual(csOutput.ToString(), goOutput.ToString(), "Output mismatch between C# and Go executables");

        return;

        void csOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                csOutput.AppendLine(e.Data);
        }

        void goOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is not null)
                goOutput.AppendLine(e.Data);
        }
    }
}
