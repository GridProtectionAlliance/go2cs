//******************************************************************************************************
//  TargetBuildTests.cs - Gbtc
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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public class TargetBuildTests : BehavioralTestBase
{
    // Move up from here: "BehavioralTests\bin\Debug\net9.0"
    private const string RootPath = @"..\..\..\..\";

    [ClassInitialize]
    public static void Initialize(TestContext context) => Init(context);

    // <TestMethods>

    [TestMethod]
    public void CheckArrayPassByValue() => CheckTarget("ArrayPassByValue");

    [TestMethod]
    public void CheckChannelReceiveFromClosed() => CheckTarget("ChannelReceiveFromClosed");

    [TestMethod]
    public void CheckChannelReceiveFromNil() => CheckTarget("ChannelReceiveFromNil");

    [TestMethod]
    public void CheckChannelSendToClosed() => CheckTarget("ChannelSendToClosed");

    [TestMethod]
    public void CheckChannelSendToNil() => CheckTarget("ChannelSendToNil");

    [TestMethod]
    public void CheckExprSwitch() => CheckTarget("ExprSwitch");

    [TestMethod]
    public void CheckFirstClassFunctions() => CheckTarget("FirstClassFunctions");

    [TestMethod]
    public void CheckForVariants() => CheckTarget("ForVariants");

    [TestMethod]
    public void CheckIfStatements() => CheckTarget("IfStatements");

    //[TestMethod]
    //public void CheckImportOptions() => CheckTarget("ImportOptions");

    [TestMethod]
    public void CheckInterfaceCasting() => CheckTarget("InterfaceCasting");

    [TestMethod]
    public void CheckInterfaceImplementation() => CheckTarget("InterfaceImplementation");

    [TestMethod]
    public void CheckInterfaceInheritance() => CheckTarget("InterfaceInheritance");

    [TestMethod]
    public void CheckLambdaFunctions() => CheckTarget("LambdaFunctions");

    [TestMethod]
    public void CheckPointerToPointer() => CheckTarget("PointerToPointer");

    [TestMethod]
    public void CheckRangeStatements() => CheckTarget("RangeStatements");

    [TestMethod]
    public void CheckSelectStatement() => CheckTarget("SelectStatement");

    [TestMethod]
    public void CheckSolitaire() => CheckTarget("Solitaire");

    [TestMethod]
    public void CheckSortArrayType() => CheckTarget("SortArrayType");

    [TestMethod]
    public void CheckSpreadOperator() => CheckTarget("SpreadOperator");

    [TestMethod]
    public void CheckStringPassByValue() => CheckTarget("StringPassByValue");

    [TestMethod]
    public void CheckStructPointerPromotionWithInterface() => CheckTarget("StructPointerPromotionWithInterface");

    [TestMethod]
    public void CheckStructPromotion() => CheckTarget("StructPromotion");

    //[TestMethod]
    //public void CheckStructPromotionWithInterface() => CheckTarget("StructPromotionWithInterface");

    [TestMethod]
    public void CheckStructWithDelegate() => CheckTarget("StructWithDelegate");

    [TestMethod]
    public void CheckStructWithPointer() => CheckTarget("StructWithPointer");

    [TestMethod]
    public void CheckTypeConversion() => CheckTarget("TypeConversion");

    [TestMethod]
    public void CheckTypeSwitch() => CheckTarget("TypeSwitch");

    [TestMethod]
    public void CheckTypeSwitchAdvanced() => CheckTarget("TypeSwitchAdvanced");

    [TestMethod]
    public void CheckVariableCapture() => CheckTarget("VariableCapture");

    // </TestMethods>

    private void CheckTarget(string targetProject)
    {
        string projectPath = Path.GetFullPath($"{RootPath}{targetProject}");
        int exitCode;

        Assert.IsTrue((exitCode = Exec(go2cs, projectPath)) == 0, $"go2cs failed with exit code {exitCode:N0}");
    }
}