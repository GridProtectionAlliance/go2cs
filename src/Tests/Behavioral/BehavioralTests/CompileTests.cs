//******************************************************************************************************
//  CompileTests.cs - Gbtc
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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BehavioralTests;

[TestClass]
public class B2_CompileTests : BehavioralTestBase
{
    [ClassInitialize]
    public static void Initialize(TestContext context) => Init(context);

    // Run "UpdateTestTargets" utility to add new project test methods below this line

    // <TestMethods>

    [TestMethod]
    public void CheckAliasStructComposite() => CheckTarget("AliasStructComposite");

    [TestMethod]
    public void CheckAndNotAssignNarrow() => CheckTarget("AndNotAssignNarrow");

    [TestMethod]
    public void CheckAnonStructArrayElement() => CheckTarget("AnonStructArrayElement");

    [TestMethod]
    public void CheckAnonymousInterfaces() => CheckTarget("AnonymousInterfaces");

    [TestMethod]
    public void CheckAnonymousStructs() => CheckTarget("AnonymousStructs");

    [TestMethod]
    public void CheckAnyKeyMap() => CheckTarget("AnyKeyMap");

    [TestMethod]
    public void CheckAppendUntypedConst() => CheckTarget("AppendUntypedConst");

    [TestMethod]
    public void CheckArrayPassByValue() => CheckTarget("ArrayPassByValue");

    [TestMethod]
    public void CheckAtomicValues() => CheckTarget("AtomicValues");

    [TestMethod]
    public void CheckBigUntypedConstComparison() => CheckTarget("BigUntypedConstComparison");

    [TestMethod]
    public void CheckBitwiseUntypedConst() => CheckTarget("BitwiseUntypedConst");

    [TestMethod]
    public void CheckBlankIdentifierCollision() => CheckTarget("BlankIdentifierCollision");

    [TestMethod]
    public void CheckBuiltinShadowLocal() => CheckTarget("BuiltinShadowLocal");

    [TestMethod]
    public void CheckChannelReceiveFromClosed() => CheckTarget("ChannelReceiveFromClosed");

    [TestMethod]
    public void CheckChannelReceiveFromNil() => CheckTarget("ChannelReceiveFromNil");

    [TestMethod]
    public void CheckChannelSendToClosed() => CheckTarget("ChannelSendToClosed");

    [TestMethod]
    public void CheckChannelSendToNil() => CheckTarget("ChannelSendToNil");

    [TestMethod]
    public void CheckClosureDefer() => CheckTarget("ClosureDefer");

    [TestMethod]
    public void CheckCombinedStructFields() => CheckTarget("CombinedStructFields");

    [TestMethod]
    public void CheckConstraints() => CheckTarget("Constraints");

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
    public void CheckDeferSimple() => CheckTarget("DeferSimple");

    [TestMethod]
    public void CheckDefinedTypeOverPkgType() => CheckTarget("DefinedTypeOverPkgType");

    [TestMethod]
    public void CheckDivideByZeroPanic() => CheckTarget("DivideByZeroPanic");

    [TestMethod]
    public void CheckErrorfFormatting() => CheckTarget("ErrorfFormatting");

    [TestMethod]
    public void CheckExprSwitch() => CheckTarget("ExprSwitch");

    [TestMethod]
    public void CheckFieldNamedAsType() => CheckTarget("FieldNamedAsType");

    [TestMethod]
    public void CheckFileNameBuildConstraints() => CheckTarget("FileNameBuildConstraints");

    [TestMethod]
    public void CheckFirstClassFunctions() => CheckTarget("FirstClassFunctions");

    [TestMethod]
    public void CheckFloatConstIntContext() => CheckTarget("FloatConstIntContext");

    [TestMethod]
    public void CheckForInitMixedTypes() => CheckTarget("ForInitMixedTypes");

    [TestMethod]
    public void CheckForMethodInitPost() => CheckTarget("ForMethodInitPost");

    [TestMethod]
    public void CheckForVariants() => CheckTarget("ForVariants");

    [TestMethod]
    public void CheckFuncLitArgCapture() => CheckTarget("FuncLitArgCapture");

    [TestMethod]
    public void CheckFuncTypeParam() => CheckTarget("FuncTypeParam");

    [TestMethod]
    public void CheckGenericAtomicPointerField() => CheckTarget("GenericAtomicPointerField");

    [TestMethod]
    public void CheckGenericCompositeLiterals() => CheckTarget("GenericCompositeLiterals");

    [TestMethod]
    public void CheckGenericCompositeType() => CheckTarget("GenericCompositeType");

    [TestMethod]
    public void CheckGenericFuncCall() => CheckTarget("GenericFuncCall");

    [TestMethod]
    public void CheckGenericFuncDecl() => CheckTarget("GenericFuncDecl");

    [TestMethod]
    public void CheckGenericNamedArrayType() => CheckTarget("GenericNamedArrayType");

    [TestMethod]
    public void CheckGenericReceiverFieldAddress() => CheckTarget("GenericReceiverFieldAddress");

    [TestMethod]
    public void CheckGenericStringTypeArg() => CheckTarget("GenericStringTypeArg");

    [TestMethod]
    public void CheckGenericStructFields() => CheckTarget("GenericStructFields");

    [TestMethod]
    public void CheckGenericTypeAssertions() => CheckTarget("GenericTypeAssertions");

    [TestMethod]
    public void CheckGenericTypeDecl() => CheckTarget("GenericTypeDecl");

    [TestMethod]
    public void CheckGenericTypeInference() => CheckTarget("GenericTypeInference");

    [TestMethod]
    public void CheckGenericTypeInstantiation() => CheckTarget("GenericTypeInstantiation");

    [TestMethod]
    public void CheckGenericVariadicFunc() => CheckTarget("GenericVariadicFunc");

    [TestMethod]
    public void CheckGlobalAtomicDefer() => CheckTarget("GlobalAtomicDefer");

    [TestMethod]
    public void CheckGlobalAtomicFieldMethod() => CheckTarget("GlobalAtomicFieldMethod");

    [TestMethod]
    public void CheckGlobalNestedFieldAddress() => CheckTarget("GlobalNestedFieldAddress");

    [TestMethod]
    public void CheckGlobalStructFieldPointers() => CheckTarget("GlobalStructFieldPointers");

    [TestMethod]
    public void CheckGoCallVariations() => CheckTarget("GoCallVariations");

    [TestMethod]
    public void CheckHeapKeywordVar() => CheckTarget("HeapKeywordVar");

    [TestMethod]
    public void CheckIfStatements() => CheckTarget("IfStatements");

    [TestMethod]
    public void CheckImmediatelyInvokedFunc() => CheckTarget("ImmediatelyInvokedFunc");

    [TestMethod]
    public void CheckIncDecPointerField() => CheckTarget("IncDecPointerField");

    [TestMethod]
    public void CheckInterfaceCasting() => CheckTarget("InterfaceCasting");

    [TestMethod]
    public void CheckInterfaceImplementation() => CheckTarget("InterfaceImplementation");

    [TestMethod]
    public void CheckInterfaceInheritance() => CheckTarget("InterfaceInheritance");

    [TestMethod]
    public void CheckInterfaceIntraFunction() => CheckTarget("InterfaceIntraFunction");

    [TestMethod]
    public void CheckIotaEnum() => CheckTarget("IotaEnum");

    [TestMethod]
    public void CheckLambdaFunctions() => CheckTarget("LambdaFunctions");

    [TestMethod]
    public void CheckLargeUintptrConst() => CheckTarget("LargeUintptrConst");

    [TestMethod]
    public void CheckMapCommaOk() => CheckTarget("MapCommaOk");

    [TestMethod]
    public void CheckMapSamePackageTypes() => CheckTarget("MapSamePackageTypes");

    [TestMethod]
    public void CheckMethodSelector() => CheckTarget("MethodSelector");

    [TestMethod]
    public void CheckMinMaxBuiltin() => CheckTarget("MinMaxBuiltin");

    [TestMethod]
    public void CheckNamedArrayAnonElement() => CheckTarget("NamedArrayAnonElement");

    [TestMethod]
    public void CheckNamedArrayComposite() => CheckTarget("NamedArrayComposite");

    [TestMethod]
    public void CheckNamedReturnDefer() => CheckTarget("NamedReturnDefer");

    [TestMethod]
    public void CheckNamedSliceConversion() => CheckTarget("NamedSliceConversion");

    [TestMethod]
    public void CheckNamedStringConversion() => CheckTarget("NamedStringConversion");

    [TestMethod]
    public void CheckNamedTypeBitwiseConst() => CheckTarget("NamedTypeBitwiseConst");

    [TestMethod]
    public void CheckNestedFieldPointerAssign() => CheckTarget("NestedFieldPointerAssign");

    [TestMethod]
    public void CheckNestedGenericTypes() => CheckTarget("NestedGenericTypes");

    [TestMethod]
    public void CheckNestedVarShadow() => CheckTarget("NestedVarShadow");

    [TestMethod]
    public void CheckNilMapOperations() => CheckTarget("NilMapOperations");

    [TestMethod]
    public void CheckPanicRecover() => CheckTarget("PanicRecover");

    [TestMethod]
    public void CheckPartialRedeclaration() => CheckTarget("PartialRedeclaration");

    [TestMethod]
    public void CheckPointerArrayRange() => CheckTarget("PointerArrayRange");

    [TestMethod]
    public void CheckPointerCopyWalk() => CheckTarget("PointerCopyWalk");

    [TestMethod]
    public void CheckPointerParamInClosure() => CheckTarget("PointerParamInClosure");

    [TestMethod]
    public void CheckPointerReceiverPointerLocalField() => CheckTarget("PointerReceiverPointerLocalField");

    [TestMethod]
    public void CheckPointerToPointer() => CheckTarget("PointerToPointer");

    [TestMethod]
    public void CheckPromotedFieldPointerDeref() => CheckTarget("PromotedFieldPointerDeref");

    [TestMethod]
    public void CheckPublicizedFieldType() => CheckTarget("PublicizedFieldType");

    [TestMethod]
    public void CheckRangeStatements() => CheckTarget("RangeStatements");

    [TestMethod]
    public void CheckReceiverFieldAddress() => CheckTarget("ReceiverFieldAddress");

    [TestMethod]
    public void CheckReceiverFieldMethodCall() => CheckTarget("ReceiverFieldMethodCall");

    [TestMethod]
    public void CheckReceiverPointerValue() => CheckTarget("ReceiverPointerValue");

    [TestMethod]
    public void CheckRelationalPatternGuard() => CheckTarget("RelationalPatternGuard");

    [TestMethod]
    public void CheckReservedTypeMethodCollision() => CheckTarget("ReservedTypeMethodCollision");

    [TestMethod]
    public void CheckRingPointerMethods() => CheckTarget("RingPointerMethods");

    [TestMethod]
    public void CheckSelectStatement() => CheckTarget("SelectStatement");

    [TestMethod]
    public void CheckShadowedCompoundAssign() => CheckTarget("ShadowedCompoundAssign");

    [TestMethod]
    public void CheckShiftPrecedenceUnsigned() => CheckTarget("ShiftPrecedenceUnsigned");

    [TestMethod]
    public void CheckSolitaire() => CheckTarget("Solitaire");

    [TestMethod]
    public void CheckSortArrayType() => CheckTarget("SortArrayType");

    [TestMethod]
    public void CheckSparseArrayNamedIntKey() => CheckTarget("SparseArrayNamedIntKey");

    [TestMethod]
    public void CheckSpreadOperator() => CheckTarget("SpreadOperator");

    [TestMethod]
    public void CheckStdLibInternalAbi() => CheckTarget("StdLibInternalAbi");

    [TestMethod]
    public void CheckStringByteUnionConstraint() => CheckTarget("StringByteUnionConstraint");

    [TestMethod]
    public void CheckStringLiteralSliceConversion() => CheckTarget("StringLiteralSliceConversion");

    [TestMethod]
    public void CheckStringPassByValue() => CheckTarget("StringPassByValue");

    [TestMethod]
    public void CheckStringSliceAndUnsignedConst() => CheckTarget("StringSliceAndUnsignedConst");

    [TestMethod]
    public void CheckStringZeroValueConcat() => CheckTarget("StringZeroValueConcat");

    [TestMethod]
    public void CheckStructPointerPromotionWithInterface() => CheckTarget("StructPointerPromotionWithInterface");

    [TestMethod]
    public void CheckStructPromotion() => CheckTarget("StructPromotion");

    [TestMethod]
    public void CheckStructPromotionWithInterface() => CheckTarget("StructPromotionWithInterface");

    [TestMethod]
    public void CheckStructWithDelegate() => CheckTarget("StructWithDelegate");

    [TestMethod]
    public void CheckStructWithPointer() => CheckTarget("StructWithPointer");

    [TestMethod]
    public void CheckSwitchDefaultOrder() => CheckTarget("SwitchDefaultOrder");

    [TestMethod]
    public void CheckTypeAssert() => CheckTarget("TypeAssert");

    [TestMethod]
    public void CheckTypeConversion() => CheckTarget("TypeConversion");

    [TestMethod]
    public void CheckTypeConversionInterfaceParam() => CheckTarget("TypeConversionInterfaceParam");

    [TestMethod]
    public void CheckTypeConversionReturnType() => CheckTarget("TypeConversionReturnType");

    [TestMethod]
    public void CheckTypeInference() => CheckTarget("TypeInference");

    [TestMethod]
    public void CheckTypeSwitch() => CheckTarget("TypeSwitch");

    [TestMethod]
    public void CheckTypeSwitchGuardShadow() => CheckTarget("TypeSwitchGuardShadow");

    [TestMethod]
    public void CheckUnexportedEmbeddedMarker() => CheckTarget("UnexportedEmbeddedMarker");

    [TestMethod]
    public void CheckUnnamedParams() => CheckTarget("UnnamedParams");

    [TestMethod]
    public void CheckUnsafeOperations() => CheckTarget("UnsafeOperations");

    [TestMethod]
    public void CheckUnsafePointerKeywordParam() => CheckTarget("UnsafePointerKeywordParam");

    [TestMethod]
    public void CheckUnsignedNamedNumeric() => CheckTarget("UnsignedNamedNumeric");

    [TestMethod]
    public void CheckUntypedConstArithmetic() => CheckTarget("UntypedConstArithmetic");

    [TestMethod]
    public void CheckUntypedFloatDefault() => CheckTarget("UntypedFloatDefault");

    [TestMethod]
    public void CheckVariableCapture() => CheckTarget("VariableCapture");

    [TestMethod]
    public void CheckVariadicPointerParam() => CheckTarget("VariadicPointerParam");

    // </TestMethods>

    private void CheckTarget(string targetProject)
    {
        // Transpile project, if needed
        TranspileProject(targetProject);

        // Compile the transpiled project
        CompileCSProject(targetProject, true);
    }
}
