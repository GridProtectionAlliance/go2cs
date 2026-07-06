//******************************************************************************************************
//  TranspileTests.cs - Gbtc
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
public class A1_TranspileTests : BehavioralTestBase
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
    public void CheckAppendNamedSliceElement() => CheckTarget("AppendNamedSliceElement");

    [TestMethod]
    public void CheckAppendUntypedConst() => CheckTarget("AppendUntypedConst");

    [TestMethod]
    public void CheckArrayOfCrossPackageType() => CheckTarget("ArrayOfCrossPackageType");

    [TestMethod]
    public void CheckArrayPassByValue() => CheckTarget("ArrayPassByValue");

    [TestMethod]
    public void CheckArrayWideIndexAddress() => CheckTarget("ArrayWideIndexAddress");

    [TestMethod]
    public void CheckAtomicFieldThroughPointer() => CheckTarget("AtomicFieldThroughPointer");

    [TestMethod]
    public void CheckAtomicValues() => CheckTarget("AtomicValues");

    [TestMethod]
    public void CheckBigUntypedConstComparison() => CheckTarget("BigUntypedConstComparison");

    [TestMethod]
    public void CheckBitwiseUntypedConst() => CheckTarget("BitwiseUntypedConst");

    [TestMethod]
    public void CheckBlankIdentifierCollision() => CheckTarget("BlankIdentifierCollision");

    [TestMethod]
    public void CheckBlankMultiResult() => CheckTarget("BlankMultiResult");

    [TestMethod]
    public void CheckBuiltinShadowLocal() => CheckTarget("BuiltinShadowLocal");

    [TestMethod]
    public void CheckCastNegativeNamedType() => CheckTarget("CastNegativeNamedType");

    [TestMethod]
    public void CheckChannelReceiveFromClosed() => CheckTarget("ChannelReceiveFromClosed");

    [TestMethod]
    public void CheckChannelReceiveFromNil() => CheckTarget("ChannelReceiveFromNil");

    [TestMethod]
    public void CheckChannelSendToClosed() => CheckTarget("ChannelSendToClosed");

    [TestMethod]
    public void CheckChannelSendToNil() => CheckTarget("ChannelSendToNil");

    [TestMethod]
    public void CheckClearBuiltinShadow() => CheckTarget("ClearBuiltinShadow");

    [TestMethod]
    public void CheckClosureBareReturnNamedResults() => CheckTarget("ClosureBareReturnNamedResults");

    [TestMethod]
    public void CheckClosureCapturedPointerAddress() => CheckTarget("ClosureCapturedPointerAddress");

    [TestMethod]
    public void CheckClosureDefer() => CheckTarget("ClosureDefer");

    [TestMethod]
    public void CheckClosureMixedReturnUnsigned() => CheckTarget("ClosureMixedReturnUnsigned");

    [TestMethod]
    public void CheckClosureParamShadow() => CheckTarget("ClosureParamShadow");

    [TestMethod]
    public void CheckClosureSelfShadowCapture() => CheckTarget("ClosureSelfShadowCapture");

    [TestMethod]
    public void CheckCollisionFieldBoxAccessor() => CheckTarget("CollisionFieldBoxAccessor");

    [TestMethod]
    public void CheckCollisionRenamedLocalBox() => CheckTarget("CollisionRenamedLocalBox");

    [TestMethod]
    public void CheckCombinedStructFields() => CheckTarget("CombinedStructFields");

    [TestMethod]
    public void CheckComplexImaginaryShadow() => CheckTarget("ComplexImaginaryShadow");

    [TestMethod]
    public void CheckConstraints() => CheckTarget("Constraints");

    [TestMethod]
    public void CheckConstShadowsParam() => CheckTarget("ConstShadowsParam");

    [TestMethod]
    public void CheckCrossPkgLib() => CheckTarget("CrossPkgLib");

    [TestMethod]
    public void CheckCrossPkgUser() => CheckTarget("CrossPkgUser");

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
    public void CheckDeferTypelessReturns() => CheckTarget("DeferTypelessReturns");

    [TestMethod]
    public void CheckDeferValueFieldPtrReceiver() => CheckTarget("DeferValueFieldPtrReceiver");

    [TestMethod]
    public void CheckDefinedTypeOverInterface() => CheckTarget("DefinedTypeOverInterface");

    [TestMethod]
    public void CheckDefinedTypeOverPkgType() => CheckTarget("DefinedTypeOverPkgType");

    [TestMethod]
    public void CheckDerefPointerToField() => CheckTarget("DerefPointerToField");

    [TestMethod]
    public void CheckDirectBoxReceiverPassedWhole() => CheckTarget("DirectBoxReceiverPassedWhole");

    [TestMethod]
    public void CheckDivideByZeroPanic() => CheckTarget("DivideByZeroPanic");

    [TestMethod]
    public void CheckElementAddressUnsignedIndex() => CheckTarget("ElementAddressUnsignedIndex");

    [TestMethod]
    public void CheckEmbeddedValuePointerMethod() => CheckTarget("EmbeddedValuePointerMethod");

    [TestMethod]
    public void CheckEmptyStructMapSet() => CheckTarget("EmptyStructMapSet");

    [TestMethod]
    public void CheckErrorfFormatting() => CheckTarget("ErrorfFormatting");

    [TestMethod]
    public void CheckEscapedLoopVarSiblingIndex() => CheckTarget("EscapedLoopVarSiblingIndex");

    [TestMethod]
    public void CheckExprSwitch() => CheckTarget("ExprSwitch");

    [TestMethod]
    public void CheckFieldChainBoxReceiver() => CheckTarget("FieldChainBoxReceiver");

    [TestMethod]
    public void CheckFieldNamedAsType() => CheckTarget("FieldNamedAsType");

    [TestMethod]
    public void CheckFieldNameShadowsLoopVar() => CheckTarget("FieldNameShadowsLoopVar");

    [TestMethod]
    public void CheckFileNameBuildConstraints() => CheckTarget("FileNameBuildConstraints");

    [TestMethod]
    public void CheckFirstClassFunctions() => CheckTarget("FirstClassFunctions");

    [TestMethod]
    public void CheckFloatConstIntContext() => CheckTarget("FloatConstIntContext");

    [TestMethod]
    public void CheckForInitMixedTypes() => CheckTarget("ForInitMixedTypes");

    [TestMethod]
    public void CheckForInitShadowedUse() => CheckTarget("ForInitShadowedUse");

    [TestMethod]
    public void CheckForMethodInitPost() => CheckTarget("ForMethodInitPost");

    [TestMethod]
    public void CheckForVariants() => CheckTarget("ForVariants");

    [TestMethod]
    public void CheckForVarMasksBlockLevel() => CheckTarget("ForVarMasksBlockLevel");

    [TestMethod]
    public void CheckForVarMasksFuncLevel() => CheckTarget("ForVarMasksFuncLevel");

    [TestMethod]
    public void CheckFuncLitArgCapture() => CheckTarget("FuncLitArgCapture");

    [TestMethod]
    public void CheckFuncTypeParam() => CheckTarget("FuncTypeParam");

    [TestMethod]
    public void CheckFuncVsMethodOverload() => CheckTarget("FuncVsMethodOverload");

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
    public void CheckGlobalArrayElementFieldAddress() => CheckTarget("GlobalArrayElementFieldAddress");

    [TestMethod]
    public void CheckGlobalArrayElementMethod() => CheckTarget("GlobalArrayElementMethod");

    [TestMethod]
    public void CheckGlobalAtomicDefer() => CheckTarget("GlobalAtomicDefer");

    [TestMethod]
    public void CheckGlobalAtomicFieldMethod() => CheckTarget("GlobalAtomicFieldMethod");

    [TestMethod]
    public void CheckGlobalCapturedInClosure() => CheckTarget("GlobalCapturedInClosure");

    [TestMethod]
    public void CheckGlobalNestedFieldAddress() => CheckTarget("GlobalNestedFieldAddress");

    [TestMethod]
    public void CheckGlobalPointerWalk() => CheckTarget("GlobalPointerWalk");

    [TestMethod]
    public void CheckGlobalShadowedByLocal() => CheckTarget("GlobalShadowedByLocal");

    [TestMethod]
    public void CheckGlobalStructFieldPointers() => CheckTarget("GlobalStructFieldPointers");

    [TestMethod]
    public void CheckGlobalTupleVarDecl() => CheckTarget("GlobalTupleVarDecl");

    [TestMethod]
    public void CheckGoCallVariations() => CheckTarget("GoCallVariations");

    [TestMethod]
    public void CheckHeapKeywordVar() => CheckTarget("HeapKeywordVar");

    [TestMethod]
    public void CheckHexByteStringLiteral() => CheckTarget("HexByteStringLiteral");

    [TestMethod]
    public void CheckIfStatements() => CheckTarget("IfStatements");

    [TestMethod]
    public void CheckImmediatelyInvokedFunc() => CheckTarget("ImmediatelyInvokedFunc");

    [TestMethod]
    public void CheckIncDecPointerField() => CheckTarget("IncDecPointerField");

    [TestMethod]
    public void CheckIndexedElementDirectBoxMethod() => CheckTarget("IndexedElementDirectBoxMethod");

    [TestMethod]
    public void CheckInferredForeignTypeNoImport() => CheckTarget("InferredForeignTypeNoImport");

    [TestMethod]
    public void CheckInterfaceCasting() => CheckTarget("InterfaceCasting");

    [TestMethod]
    public void CheckInterfaceFieldNamedScalar() => CheckTarget("InterfaceFieldNamedScalar");

    [TestMethod]
    public void CheckInterfaceImplementation() => CheckTarget("InterfaceImplementation");

    [TestMethod]
    public void CheckInterfaceInheritance() => CheckTarget("InterfaceInheritance");

    [TestMethod]
    public void CheckInterfaceIntraFunction() => CheckTarget("InterfaceIntraFunction");

    [TestMethod]
    public void CheckIotaEnum() => CheckTarget("IotaEnum");

    [TestMethod]
    public void CheckKeywordTrueFalseIdent() => CheckTarget("KeywordTrueFalseIdent");

    [TestMethod]
    public void CheckLambdaFunctions() => CheckTarget("LambdaFunctions");

    [TestMethod]
    public void CheckLargeUintptrConst() => CheckTarget("LargeUintptrConst");

    [TestMethod]
    public void CheckLocalTypeSliceElement() => CheckTarget("LocalTypeSliceElement");

    [TestMethod]
    public void CheckMakeLenNamedNumeric() => CheckTarget("MakeLenNamedNumeric");

    [TestMethod]
    public void CheckMakeSliceUintptrLen() => CheckTarget("MakeSliceUintptrLen");

    [TestMethod]
    public void CheckMapCommaOk() => CheckTarget("MapCommaOk");

    [TestMethod]
    public void CheckMapSamePackageTypes() => CheckTarget("MapSamePackageTypes");

    [TestMethod]
    public void CheckMethodExpression() => CheckTarget("MethodExpression");

    [TestMethod]
    public void CheckMethodGroupGenericArg() => CheckTarget("MethodGroupGenericArg");

    [TestMethod]
    public void CheckMethodOnBoxedGlobalIndex() => CheckTarget("MethodOnBoxedGlobalIndex");

    [TestMethod]
    public void CheckMethodSelector() => CheckTarget("MethodSelector");

    [TestMethod]
    public void CheckMethodValueReassignCapture() => CheckTarget("MethodValueReassignCapture");

    [TestMethod]
    public void CheckMinMaxBuiltin() => CheckTarget("MinMaxBuiltin");

    [TestMethod]
    public void CheckNamedArrayAnonElement() => CheckTarget("NamedArrayAnonElement");

    [TestMethod]
    public void CheckNamedArrayComposite() => CheckTarget("NamedArrayComposite");

    [TestMethod]
    public void CheckNamedArrayWrapper() => CheckTarget("NamedArrayWrapper");

    [TestMethod]
    public void CheckNamedBooleanLogic() => CheckTarget("NamedBooleanLogic");

    [TestMethod]
    public void CheckNamedFuncTypeStateMachine() => CheckTarget("NamedFuncTypeStateMachine");

    [TestMethod]
    public void CheckNamedMapValuesCollision() => CheckTarget("NamedMapValuesCollision");

    [TestMethod]
    public void CheckNamedNumericConstCast() => CheckTarget("NamedNumericConstCast");

    [TestMethod]
    public void CheckNamedNumericConversion() => CheckTarget("NamedNumericConversion");

    [TestMethod]
    public void CheckNamedNumericIncDec() => CheckTarget("NamedNumericIncDec");

    [TestMethod]
    public void CheckNamedNumericIntCast() => CheckTarget("NamedNumericIntCast");

    [TestMethod]
    public void CheckNamedNumericPointerReinterpret() => CheckTarget("NamedNumericPointerReinterpret");

    [TestMethod]
    public void CheckNamedNumericShiftConv() => CheckTarget("NamedNumericShiftConv");

    [TestMethod]
    public void CheckNamedPointerReinterpret() => CheckTarget("NamedPointerReinterpret");

    [TestMethod]
    public void CheckNamedReturnDefer() => CheckTarget("NamedReturnDefer");

    [TestMethod]
    public void CheckNamedSliceCaptureMethod() => CheckTarget("NamedSliceCaptureMethod");

    [TestMethod]
    public void CheckNamedSliceConversion() => CheckTarget("NamedSliceConversion");

    [TestMethod]
    public void CheckNamedSlicePointerReinterpret() => CheckTarget("NamedSlicePointerReinterpret");

    [TestMethod]
    public void CheckNamedStringConversion() => CheckTarget("NamedStringConversion");

    [TestMethod]
    public void CheckNamedTypeBitwiseConst() => CheckTarget("NamedTypeBitwiseConst");

    [TestMethod]
    public void CheckNamedTypeOverStruct() => CheckTarget("NamedTypeOverStruct");

    [TestMethod]
    public void CheckNarrowArithmeticArg() => CheckTarget("NarrowArithmeticArg");

    [TestMethod]
    public void CheckNarrowByteArithFirstOperandCast() => CheckTarget("NarrowByteArithFirstOperandCast");

    [TestMethod]
    public void CheckNarrowByteArithReturn() => CheckTarget("NarrowByteArithReturn");

    [TestMethod]
    public void CheckNativeIntConstMask() => CheckTarget("NativeIntConstMask");

    [TestMethod]
    public void CheckNativeIntWideConstAssign() => CheckTarget("NativeIntWideConstAssign");

    [TestMethod]
    public void CheckNestedEmbeddingPromotion() => CheckTarget("NestedEmbeddingPromotion");

    [TestMethod]
    public void CheckNestedFieldElementAddr() => CheckTarget("NestedFieldElementAddr");

    [TestMethod]
    public void CheckNestedFieldPointerAssign() => CheckTarget("NestedFieldPointerAssign");

    [TestMethod]
    public void CheckNestedGenericTypes() => CheckTarget("NestedGenericTypes");

    [TestMethod]
    public void CheckNestedMapAssign() => CheckTarget("NestedMapAssign");

    [TestMethod]
    public void CheckNestedVarShadow() => CheckTarget("NestedVarShadow");

    [TestMethod]
    public void CheckNilMapOperations() => CheckTarget("NilMapOperations");

    [TestMethod]
    public void CheckPackageShadowParam() => CheckTarget("PackageShadowParam");

    [TestMethod]
    public void CheckPackageShadowPointerParam() => CheckTarget("PackageShadowPointerParam");

    [TestMethod]
    public void CheckPanicRecover() => CheckTarget("PanicRecover");

    [TestMethod]
    public void CheckPartialRedeclaration() => CheckTarget("PartialRedeclaration");

    [TestMethod]
    public void CheckPointerArrayRange() => CheckTarget("PointerArrayRange");

    [TestMethod]
    public void CheckPointerArraySlice() => CheckTarget("PointerArraySlice");

    [TestMethod]
    public void CheckPointerCastSliceRange() => CheckTarget("PointerCastSliceRange");

    [TestMethod]
    public void CheckPointerCopyWalk() => CheckTarget("PointerCopyWalk");

    [TestMethod]
    public void CheckPointerEmbeddingPromotion() => CheckTarget("PointerEmbeddingPromotion");

    [TestMethod]
    public void CheckPointerFieldArrayElementAddress() => CheckTarget("PointerFieldArrayElementAddress");

    [TestMethod]
    public void CheckPointerFieldOfBoxedGlobal() => CheckTarget("PointerFieldOfBoxedGlobal");

    [TestMethod]
    public void CheckPointerParamCapturedInClosure() => CheckTarget("PointerParamCapturedInClosure");

    [TestMethod]
    public void CheckPointerParamInClosure() => CheckTarget("PointerParamInClosure");

    [TestMethod]
    public void CheckPointerParamNilWalk() => CheckTarget("PointerParamNilWalk");

    [TestMethod]
    public void CheckPointerParamWalk() => CheckTarget("PointerParamWalk");

    [TestMethod]
    public void CheckPointerReceiverPointerLocalField() => CheckTarget("PointerReceiverPointerLocalField");

    [TestMethod]
    public void CheckPointerRvalueFieldReceiver() => CheckTarget("PointerRvalueFieldReceiver");

    [TestMethod]
    public void CheckPointerSelectorDeref() => CheckTarget("PointerSelectorDeref");

    [TestMethod]
    public void CheckPointerToPointer() => CheckTarget("PointerToPointer");

    [TestMethod]
    public void CheckPrintfWidthFlags() => CheckTarget("PrintfWidthFlags");

    [TestMethod]
    public void CheckPromotedFieldPointerDeref() => CheckTarget("PromotedFieldPointerDeref");

    [TestMethod]
    public void CheckPublicizedFieldType() => CheckTarget("PublicizedFieldType");

    [TestMethod]
    public void CheckPublicizedFuncTypeParam() => CheckTarget("PublicizedFuncTypeParam");

    [TestMethod]
    public void CheckPublicizedInterfaceParam() => CheckTarget("PublicizedInterfaceParam");

    [TestMethod]
    public void CheckRangeStatements() => CheckTarget("RangeStatements");

    [TestMethod]
    public void CheckRangeVarHeapBox() => CheckTarget("RangeVarHeapBox");

    [TestMethod]
    public void CheckRangeVarReassign() => CheckTarget("RangeVarReassign");

    [TestMethod]
    public void CheckReceiverCapturedInClosure() => CheckTarget("ReceiverCapturedInClosure");

    [TestMethod]
    public void CheckReceiverFieldAddress() => CheckTarget("ReceiverFieldAddress");

    [TestMethod]
    public void CheckReceiverFieldMethodCall() => CheckTarget("ReceiverFieldMethodCall");

    [TestMethod]
    public void CheckReceiverPointerValue() => CheckTarget("ReceiverPointerValue");

    [TestMethod]
    public void CheckRelationalPatternGuard() => CheckTarget("RelationalPatternGuard");

    [TestMethod]
    public void CheckRenamedReceiverBox() => CheckTarget("RenamedReceiverBox");

    [TestMethod]
    public void CheckReservedTypeMethodCollision() => CheckTarget("ReservedTypeMethodCollision");

    [TestMethod]
    public void CheckReturnPointerFieldOfParam() => CheckTarget("ReturnPointerFieldOfParam");

    [TestMethod]
    public void CheckRingPointerMethods() => CheckTarget("RingPointerMethods");

    [TestMethod]
    public void CheckSameUnderlyingNamedConv() => CheckTarget("SameUnderlyingNamedConv");

    [TestMethod]
    public void CheckSelectStatement() => CheckTarget("SelectStatement");

    [TestMethod]
    public void CheckShadowedCompoundAssign() => CheckTarget("ShadowedCompoundAssign");

    [TestMethod]
    public void CheckShadowedPointerParam() => CheckTarget("ShadowedPointerParam");

    [TestMethod]
    public void CheckShadowedVarMethodCallLHS() => CheckTarget("ShadowedVarMethodCallLHS");

    [TestMethod]
    public void CheckSharedEmbeddedInterfaceMember() => CheckTarget("SharedEmbeddedInterfaceMember");

    [TestMethod]
    public void CheckShiftNegativeWideConst() => CheckTarget("ShiftNegativeWideConst");

    [TestMethod]
    public void CheckShiftPrecedenceUnsigned() => CheckTarget("ShiftPrecedenceUnsigned");

    [TestMethod]
    public void CheckSlice3IndexWideBound() => CheckTarget("Slice3IndexWideBound");

    [TestMethod]
    public void CheckSliceAliasing() => CheckTarget("SliceAliasing");

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
    public void CheckStringConvNamedInt() => CheckTarget("StringConvNamedInt");

    [TestMethod]
    public void CheckStringConvPostfix() => CheckTarget("StringConvPostfix");

    [TestMethod]
    public void CheckStringLiteralIndexLoop() => CheckTarget("StringLiteralIndexLoop");

    [TestMethod]
    public void CheckStringLiteralSliceConversion() => CheckTarget("StringLiteralSliceConversion");

    [TestMethod]
    public void CheckStringPassByValue() => CheckTarget("StringPassByValue");

    [TestMethod]
    public void CheckStringSliceAndUnsignedConst() => CheckTarget("StringSliceAndUnsignedConst");

    [TestMethod]
    public void CheckStringZeroValueConcat() => CheckTarget("StringZeroValueConcat");

    [TestMethod]
    public void CheckStructFieldNamedOther() => CheckTarget("StructFieldNamedOther");

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
    public void CheckSwitchBreakInCase() => CheckTarget("SwitchBreakInCase");

    [TestMethod]
    public void CheckSwitchDefaultOrder() => CheckTarget("SwitchDefaultOrder");

    [TestMethod]
    public void CheckSwitchFallthroughDefaultReturn() => CheckTarget("SwitchFallthroughDefaultReturn");

    [TestMethod]
    public void CheckSwitchNonConstCaseLabel() => CheckTarget("SwitchNonConstCaseLabel");

    [TestMethod]
    public void CheckSystemCollidingTypeName() => CheckTarget("SystemCollidingTypeName");

    [TestMethod]
    public void CheckTupleDestructureEscapingLocal() => CheckTarget("TupleDestructureEscapingLocal");

    [TestMethod]
    public void CheckTupleMixedDeclareReassign() => CheckTarget("TupleMixedDeclareReassign");

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
    public void CheckUnsafeBuiltinIntegerLen() => CheckTarget("UnsafeBuiltinIntegerLen");

    [TestMethod]
    public void CheckUnsafeOperations() => CheckTarget("UnsafeOperations");

    [TestMethod]
    public void CheckUnsafePointerArgPassing() => CheckTarget("UnsafePointerArgPassing");

    [TestMethod]
    public void CheckUnsafePointerInferredNoImport() => CheckTarget("UnsafePointerInferredNoImport");

    [TestMethod]
    public void CheckUnsafePointerKeywordParam() => CheckTarget("UnsafePointerKeywordParam");

    [TestMethod]
    public void CheckUnsafePointerParamPin() => CheckTarget("UnsafePointerParamPin");

    [TestMethod]
    public void CheckUnsafePointerReinterpret() => CheckTarget("UnsafePointerReinterpret");

    [TestMethod]
    public void CheckUnsignedNamedNumeric() => CheckTarget("UnsignedNamedNumeric");

    [TestMethod]
    public void CheckUntypedConstArithmetic() => CheckTarget("UntypedConstArithmetic");

    [TestMethod]
    public void CheckUntypedFloatDefault() => CheckTarget("UntypedFloatDefault");

    [TestMethod]
    public void CheckUntypedNestedSliceComposite() => CheckTarget("UntypedNestedSliceComposite");

    [TestMethod]
    public void CheckVariableCapture() => CheckTarget("VariableCapture");

    [TestMethod]
    public void CheckVariadicClosureSpread() => CheckTarget("VariadicClosureSpread");

    [TestMethod]
    public void CheckVariadicPointerParam() => CheckTarget("VariadicPointerParam");

    [TestMethod]
    public void CheckVarNamedAsType() => CheckTarget("VarNamedAsType");

    // </TestMethods>

    private void CheckTarget(string targetProject)
    {
        TranspileProject(targetProject, true);
    }
}
