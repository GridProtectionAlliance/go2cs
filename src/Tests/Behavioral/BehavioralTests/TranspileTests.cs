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
    public void CheckAnonInterfaceCrossFile() => CheckTarget("AnonInterfaceCrossFile");

    [TestMethod]
    public void CheckAnonStructArrayElement() => CheckTarget("AnonStructArrayElement");

    [TestMethod]
    public void CheckAnonStructCrossFile() => CheckTarget("AnonStructCrossFile");

    [TestMethod]
    public void CheckAnonymousInterfaces() => CheckTarget("AnonymousInterfaces");

    [TestMethod]
    public void CheckAnonymousStructs() => CheckTarget("AnonymousStructs");

    [TestMethod]
    public void CheckAnyKeyMap() => CheckTarget("AnyKeyMap");

    [TestMethod]
    public void CheckAnyStringLitAssign() => CheckTarget("AnyStringLitAssign");

    [TestMethod]
    public void CheckAnyStringLitChanSend() => CheckTarget("AnyStringLitChanSend");

    [TestMethod]
    public void CheckAnyStringLitComposite() => CheckTarget("AnyStringLitComposite");

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
    public void CheckAtomicValue() => CheckTarget("AtomicValue");

    [TestMethod]
    public void CheckAtomicValues() => CheckTarget("AtomicValues");

    [TestMethod]
    public void CheckBclTypeNameShadow() => CheckTarget("BclTypeNameShadow");

    [TestMethod]
    public void CheckBigUntypedConstComparison() => CheckTarget("BigUntypedConstComparison");

    [TestMethod]
    public void CheckBitwiseUntypedConst() => CheckTarget("BitwiseUntypedConst");

    [TestMethod]
    public void CheckBlankIdentifierCollision() => CheckTarget("BlankIdentifierCollision");

    [TestMethod]
    public void CheckBlankMultiResult() => CheckTarget("BlankMultiResult");

    [TestMethod]
    public void CheckBlankNamedReturn() => CheckTarget("BlankNamedReturn");

    [TestMethod]
    public void CheckBoxedMapFieldWrite() => CheckTarget("BoxedMapFieldWrite");

    [TestMethod]
    public void CheckBuiltinShadowLocal() => CheckTarget("BuiltinShadowLocal");

    [TestMethod]
    public void CheckCaptureModeFuncLitParam() => CheckTarget("CaptureModeFuncLitParam");

    [TestMethod]
    public void CheckCaptureModeParamClosure() => CheckTarget("CaptureModeParamClosure");

    [TestMethod]
    public void CheckCaptureModeValueParam() => CheckTarget("CaptureModeValueParam");

    [TestMethod]
    public void CheckCaptureModeValueParamLib() => CheckTarget("CaptureModeValueParamLib");

    [TestMethod]
    public void CheckCaptureModeValueParamUser() => CheckTarget("CaptureModeValueParamUser");

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
    public void CheckClosureEmbeddedPromotedPtrMethod() => CheckTarget("ClosureEmbeddedPromotedPtrMethod");

    [TestMethod]
    public void CheckClosureMixedReturnUnsigned() => CheckTarget("ClosureMixedReturnUnsigned");

    [TestMethod]
    public void CheckClosureParamShadow() => CheckTarget("ClosureParamShadow");

    [TestMethod]
    public void CheckClosurePtrLocalFieldMethod() => CheckTarget("ClosurePtrLocalFieldMethod");

    [TestMethod]
    public void CheckClosureReassignsPtrParam() => CheckTarget("ClosureReassignsPtrParam");

    [TestMethod]
    public void CheckClosureReturnAnonStruct() => CheckTarget("ClosureReturnAnonStruct");

    [TestMethod]
    public void CheckClosureSelfShadowCapture() => CheckTarget("ClosureSelfShadowCapture");

    [TestMethod]
    public void CheckClosureWriteVisibility() => CheckTarget("ClosureWriteVisibility");

    [TestMethod]
    public void CheckCollisionFieldBoxAccessor() => CheckTarget("CollisionFieldBoxAccessor");

    [TestMethod]
    public void CheckCollisionRenamedLocalBox() => CheckTarget("CollisionRenamedLocalBox");

    [TestMethod]
    public void CheckCombinedStructFields() => CheckTarget("CombinedStructFields");

    [TestMethod]
    public void CheckComplexFormat() => CheckTarget("ComplexFormat");

    [TestMethod]
    public void CheckComplexImaginaryShadow() => CheckTarget("ComplexImaginaryShadow");

    [TestMethod]
    public void CheckConstrainedSliceParamInPlace() => CheckTarget("ConstrainedSliceParamInPlace");

    [TestMethod]
    public void CheckConstraints() => CheckTarget("Constraints");

    [TestMethod]
    public void CheckConstShadowsParam() => CheckTarget("ConstShadowsParam");

    [TestMethod]
    public void CheckCrossPkgBox() => CheckTarget("CrossPkgBox");

    [TestMethod]
    public void CheckCrossPkgFuncLib() => CheckTarget("CrossPkgFuncLib");

    [TestMethod]
    public void CheckCrossPkgLib() => CheckTarget("CrossPkgLib");

    [TestMethod]
    public void CheckCrossPkgSameNameAlias() => CheckTarget("CrossPkgSameNameAlias");

    [TestMethod]
    public void CheckCrossPkgUser() => CheckTarget("CrossPkgUser");

    [TestMethod]
    public void CheckDeadPointerParamAlias() => CheckTarget("DeadPointerParamAlias");

    [TestMethod]
    public void CheckDeferArgEnclosingCapture() => CheckTarget("DeferArgEnclosingCapture");

    [TestMethod]
    public void CheckDeferCallOrder() => CheckTarget("DeferCallOrder");

    [TestMethod]
    public void CheckDeferClosure() => CheckTarget("DeferClosure");

    [TestMethod]
    public void CheckDeferEvalParam() => CheckTarget("DeferEvalParam");

    [TestMethod]
    public void CheckDeferEvalParamFunc() => CheckTarget("DeferEvalParamFunc");

    [TestMethod]
    public void CheckDeferHeapFieldPtrMethod() => CheckTarget("DeferHeapFieldPtrMethod");

    [TestMethod]
    public void CheckDeferHeapLocalPtrMethod() => CheckTarget("DeferHeapLocalPtrMethod");

    [TestMethod]
    public void CheckDeferInterfaceReturn() => CheckTarget("DeferInterfaceReturn");

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
    public void CheckDynamicInterfaceKeywordMethod() => CheckTarget("DynamicInterfaceKeywordMethod");

    [TestMethod]
    public void CheckElementAddressUnsignedIndex() => CheckTarget("ElementAddressUnsignedIndex");

    [TestMethod]
    public void CheckElidedNestedPtrComposite() => CheckTarget("ElidedNestedPtrComposite");

    [TestMethod]
    public void CheckEmbeddedPointerNilAssign() => CheckTarget("EmbeddedPointerNilAssign");

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
    public void CheckFieldNameTypeMethodCollision() => CheckTarget("FieldNameTypeMethodCollision");

    [TestMethod]
    public void CheckFileNameBuildConstraints() => CheckTarget("FileNameBuildConstraints");

    [TestMethod]
    public void CheckFirstClassFunctions() => CheckTarget("FirstClassFunctions");

    [TestMethod]
    public void CheckFixedArrayBufferPointer() => CheckTarget("FixedArrayBufferPointer");

    [TestMethod]
    public void CheckFloatConstIntContext() => CheckTarget("FloatConstIntContext");

    [TestMethod]
    public void CheckFloatFormatExponent() => CheckTarget("FloatFormatExponent");

    [TestMethod]
    public void CheckFloatFormatting() => CheckTarget("FloatFormatting");

    [TestMethod]
    public void CheckForeignPtrEmbedIfaceLib() => CheckTarget("ForeignPtrEmbedIfaceLib");

    [TestMethod]
    public void CheckForeignPtrEmbedIfaceUser() => CheckTarget("ForeignPtrEmbedIfaceUser");

    [TestMethod]
    public void CheckForInitMixedTypes() => CheckTarget("ForInitMixedTypes");

    [TestMethod]
    public void CheckForInitShadowedUse() => CheckTarget("ForInitShadowedUse");

    [TestMethod]
    public void CheckForLoopPerIterationVars() => CheckTarget("ForLoopPerIterationVars");

    [TestMethod]
    public void CheckForMethodInitPost() => CheckTarget("ForMethodInitPost");

    [TestMethod]
    public void CheckForVariants() => CheckTarget("ForVariants");

    [TestMethod]
    public void CheckForVarMasksBlockLevel() => CheckTarget("ForVarMasksBlockLevel");

    [TestMethod]
    public void CheckForVarMasksFuncLevel() => CheckTarget("ForVarMasksFuncLevel");

    [TestMethod]
    public void CheckFuncFieldNestedTupleParam() => CheckTarget("FuncFieldNestedTupleParam");

    [TestMethod]
    public void CheckFuncFieldUnexportedType() => CheckTarget("FuncFieldUnexportedType");

    [TestMethod]
    public void CheckFuncLitArgCapture() => CheckTarget("FuncLitArgCapture");

    [TestMethod]
    public void CheckFuncLitCaptureInCondition() => CheckTarget("FuncLitCaptureInCondition");

    [TestMethod]
    public void CheckFuncLitNumericTupleReturn() => CheckTarget("FuncLitNumericTupleReturn");

    [TestMethod]
    public void CheckFuncLitStringConcatReturn() => CheckTarget("FuncLitStringConcatReturn");

    [TestMethod]
    public void CheckFuncLitUntypedConstReturn() => CheckTarget("FuncLitUntypedConstReturn");

    [TestMethod]
    public void CheckFuncTypeParam() => CheckTarget("FuncTypeParam");

    [TestMethod]
    public void CheckFuncVsMethodOverload() => CheckTarget("FuncVsMethodOverload");

    [TestMethod]
    public void CheckGenericArrayConstraint() => CheckTarget("GenericArrayConstraint");

    [TestMethod]
    public void CheckGenericAtomicPointerField() => CheckTarget("GenericAtomicPointerField");

    [TestMethod]
    public void CheckGenericCompositeLiterals() => CheckTarget("GenericCompositeLiterals");

    [TestMethod]
    public void CheckGenericCompositeType() => CheckTarget("GenericCompositeType");

    [TestMethod]
    public void CheckGenericEmbedPromotion() => CheckTarget("GenericEmbedPromotion");

    [TestMethod]
    public void CheckGenericFuncCall() => CheckTarget("GenericFuncCall");

    [TestMethod]
    public void CheckGenericFuncDecl() => CheckTarget("GenericFuncDecl");

    [TestMethod]
    public void CheckGenericInterfaceConstraint() => CheckTarget("GenericInterfaceConstraint");

    [TestMethod]
    public void CheckGenericNamedArrayType() => CheckTarget("GenericNamedArrayType");

    [TestMethod]
    public void CheckGenericPointerInterfaceImpl() => CheckTarget("GenericPointerInterfaceImpl");

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
    public void CheckGoNamespaceShadow() => CheckTarget("GoNamespaceShadow");

    [TestMethod]
    public void CheckGoOnlyFloatLiteralForms() => CheckTarget("GoOnlyFloatLiteralForms");

    [TestMethod]
    public void CheckGoroutinePanicExitCode() => CheckTarget("GoroutinePanicExitCode");

    [TestMethod]
    public void CheckGoStmtReceiverLambda() => CheckTarget("GoStmtReceiverLambda");

    [TestMethod]
    public void CheckGoStmtValueReturn() => CheckTarget("GoStmtValueReturn");

    [TestMethod]
    public void CheckHeapKeywordVar() => CheckTarget("HeapKeywordVar");

    [TestMethod]
    public void CheckHexByteStringLiteral() => CheckTarget("HexByteStringLiteral");

    [TestMethod]
    public void CheckIfaceFieldEmbedAdapter() => CheckTarget("IfaceFieldEmbedAdapter");

    [TestMethod]
    public void CheckIfaceFieldMethodValueBind() => CheckTarget("IfaceFieldMethodValueBind");

    [TestMethod]
    public void CheckIfaceToIfaceNarrow() => CheckTarget("IfaceToIfaceNarrow");

    [TestMethod]
    public void CheckIfStatements() => CheckTarget("IfStatements");

    [TestMethod]
    public void CheckImmediatelyInvokedFunc() => CheckTarget("ImmediatelyInvokedFunc");

    [TestMethod]
    public void CheckIncDecPointerField() => CheckTarget("IncDecPointerField");

    [TestMethod]
    public void CheckIndexedElementDirectBoxMethod() => CheckTarget("IndexedElementDirectBoxMethod");

    [TestMethod]
    public void CheckIndexExprCaseLabel() => CheckTarget("IndexExprCaseLabel");

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
    public void CheckInterfaceMapKeyPointer() => CheckTarget("InterfaceMapKeyPointer");

    [TestMethod]
    public void CheckInterfaceToInterfaceAdapter() => CheckTarget("InterfaceToInterfaceAdapter");

    [TestMethod]
    public void CheckIntMinLiterals() => CheckTarget("IntMinLiterals");

    [TestMethod]
    public void CheckIoLike() => CheckTarget("IoLike");

    [TestMethod]
    public void CheckIotaEnum() => CheckTarget("IotaEnum");

    [TestMethod]
    public void CheckKeyedLiteralIfaceAssign() => CheckTarget("KeyedLiteralIfaceAssign");

    [TestMethod]
    public void CheckKeywordNamedTypes() => CheckTarget("KeywordNamedTypes");

    [TestMethod]
    public void CheckKeywordTrueFalseIdent() => CheckTarget("KeywordTrueFalseIdent");

    [TestMethod]
    public void CheckLabeledEmptyStmt() => CheckTarget("LabeledEmptyStmt");

    [TestMethod]
    public void CheckLambdaFunctions() => CheckTarget("LambdaFunctions");

    [TestMethod]
    public void CheckLambdaReturnsPointerParam() => CheckTarget("LambdaReturnsPointerParam");

    [TestMethod]
    public void CheckLargeUintptrConst() => CheckTarget("LargeUintptrConst");

    [TestMethod]
    public void CheckLocalStructFieldAddr() => CheckTarget("LocalStructFieldAddr");

    [TestMethod]
    public void CheckLocalTypeSliceElement() => CheckTarget("LocalTypeSliceElement");

    [TestMethod]
    public void CheckMakeLenNamedNumeric() => CheckTarget("MakeLenNamedNumeric");

    [TestMethod]
    public void CheckMakeSliceUintptrLen() => CheckTarget("MakeSliceUintptrLen");

    [TestMethod]
    public void CheckManagedAtomicPointer() => CheckTarget("ManagedAtomicPointer");

    [TestMethod]
    public void CheckManualConversionSiblingState() => CheckTarget("ManualConversionSiblingState");

    [TestMethod]
    public void CheckMapAnonStructValue() => CheckTarget("MapAnonStructValue");

    [TestMethod]
    public void CheckMapCommaOk() => CheckTarget("MapCommaOk");

    [TestMethod]
    public void CheckMapPointerElementLiteral() => CheckTarget("MapPointerElementLiteral");

    [TestMethod]
    public void CheckMapSamePackageTypes() => CheckTarget("MapSamePackageTypes");

    [TestMethod]
    public void CheckMathFloatBits() => CheckTarget("MathFloatBits");

    [TestMethod]
    public void CheckMethodExpression() => CheckTarget("MethodExpression");

    [TestMethod]
    public void CheckMethodGroupGenericArg() => CheckTarget("MethodGroupGenericArg");

    [TestMethod]
    public void CheckMethodlessFuncType() => CheckTarget("MethodlessFuncType");

    [TestMethod]
    public void CheckMethodlessFuncTypeAssert() => CheckTarget("MethodlessFuncTypeAssert");

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
    public void CheckNamedArrayKeyedLiteral() => CheckTarget("NamedArrayKeyedLiteral");

    [TestMethod]
    public void CheckNamedArrayWrapper() => CheckTarget("NamedArrayWrapper");

    [TestMethod]
    public void CheckNamedBooleanLogic() => CheckTarget("NamedBooleanLogic");

    [TestMethod]
    public void CheckNamedByteSliceFromStringLit() => CheckTarget("NamedByteSliceFromStringLit");

    [TestMethod]
    public void CheckNamedChannelType() => CheckTarget("NamedChannelType");

    [TestMethod]
    public void CheckNamedDelegateStructuralParam() => CheckTarget("NamedDelegateStructuralParam");

    [TestMethod]
    public void CheckNamedFuncResultPointerArg() => CheckTarget("NamedFuncResultPointerArg");

    [TestMethod]
    public void CheckNamedFuncTypeMapParam() => CheckTarget("NamedFuncTypeMapParam");

    [TestMethod]
    public void CheckNamedFuncTypeStateMachine() => CheckTarget("NamedFuncTypeStateMachine");

    [TestMethod]
    public void CheckNamedFuncTypeStructuralField() => CheckTarget("NamedFuncTypeStructuralField");

    [TestMethod]
    public void CheckNamedIntSignednessConv() => CheckTarget("NamedIntSignednessConv");

    [TestMethod]
    public void CheckNamedMapCrossPkgKey() => CheckTarget("NamedMapCrossPkgKey");

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
    public void CheckNamedNumericOperatorConstraint() => CheckTarget("NamedNumericOperatorConstraint");

    [TestMethod]
    public void CheckNamedNumericPointerReinterpret() => CheckTarget("NamedNumericPointerReinterpret");

    [TestMethod]
    public void CheckNamedNumericShiftConv() => CheckTarget("NamedNumericShiftConv");

    [TestMethod]
    public void CheckNamedNumericSliceIndex() => CheckTarget("NamedNumericSliceIndex");

    [TestMethod]
    public void CheckNamedNumericSwitchLiteral() => CheckTarget("NamedNumericSwitchLiteral");

    [TestMethod]
    public void CheckNamedPointerReinterpret() => CheckTarget("NamedPointerReinterpret");

    [TestMethod]
    public void CheckNamedResultDeferCapture() => CheckTarget("NamedResultDeferCapture");

    [TestMethod]
    public void CheckNamedResultLambdaInfer() => CheckTarget("NamedResultLambdaInfer");

    [TestMethod]
    public void CheckNamedReturnDefer() => CheckTarget("NamedReturnDefer");

    [TestMethod]
    public void CheckNamedSliceCaptureMethod() => CheckTarget("NamedSliceCaptureMethod");

    [TestMethod]
    public void CheckNamedSliceChildPkg() => CheckTarget("NamedSliceChildPkg");

    [TestMethod]
    public void CheckNamedSliceConversion() => CheckTarget("NamedSliceConversion");

    [TestMethod]
    public void CheckNamedSlicePointerElements() => CheckTarget("NamedSlicePointerElements");

    [TestMethod]
    public void CheckNamedSlicePointerReinterpret() => CheckTarget("NamedSlicePointerReinterpret");

    [TestMethod]
    public void CheckNamedStringConsts() => CheckTarget("NamedStringConsts");

    [TestMethod]
    public void CheckNamedStringConversion() => CheckTarget("NamedStringConversion");

    [TestMethod]
    public void CheckNamedStringDefine() => CheckTarget("NamedStringDefine");

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
    public void CheckNarrowShiftVarCount() => CheckTarget("NarrowShiftVarCount");

    [TestMethod]
    public void CheckNativeIntConstMask() => CheckTarget("NativeIntConstMask");

    [TestMethod]
    public void CheckNativeIntWideConstAssign() => CheckTarget("NativeIntWideConstAssign");

    [TestMethod]
    public void CheckNativeIntWideConstElement() => CheckTarget("NativeIntWideConstElement");

    [TestMethod]
    public void CheckNestedAliasUser() => CheckTarget("NestedAliasUser");

    [TestMethod]
    public void CheckNestedEmbeddingPromotion() => CheckTarget("NestedEmbeddingPromotion");

    [TestMethod]
    public void CheckNestedFieldElementAddr() => CheckTarget("NestedFieldElementAddr");

    [TestMethod]
    public void CheckNestedFieldPointerAssign() => CheckTarget("NestedFieldPointerAssign");

    [TestMethod]
    public void CheckNestedGenericTypes() => CheckTarget("NestedGenericTypes");

    [TestMethod]
    public void CheckNestedLambdaReceiverField() => CheckTarget("NestedLambdaReceiverField");

    [TestMethod]
    public void CheckNestedMapAssign() => CheckTarget("NestedMapAssign");

    [TestMethod]
    public void CheckNestedPromotedEmbedInit() => CheckTarget("NestedPromotedEmbedInit");

    [TestMethod]
    public void CheckNestedVarShadow() => CheckTarget("NestedVarShadow");

    [TestMethod]
    public void CheckNewAnonStructIfaceEmbed() => CheckTarget("NewAnonStructIfaceEmbed");

    [TestMethod]
    public void CheckNilMapOperations() => CheckTarget("NilMapOperations");

    [TestMethod]
    public void CheckNilPointerUintptr() => CheckTarget("NilPointerUintptr");

    [TestMethod]
    public void CheckNilSliceConversion() => CheckTarget("NilSliceConversion");

    [TestMethod]
    public void CheckPackageShadowParam() => CheckTarget("PackageShadowParam");

    [TestMethod]
    public void CheckPackageShadowPointerParam() => CheckTarget("PackageShadowPointerParam");

    [TestMethod]
    public void CheckPackageVarInitOrder() => CheckTarget("PackageVarInitOrder");

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
    public void CheckPointerCoreConstraints() => CheckTarget("PointerCoreConstraints");

    [TestMethod]
    public void CheckPointerEmbedBoxReceiver() => CheckTarget("PointerEmbedBoxReceiver");

    [TestMethod]
    public void CheckPointerEmbeddingPromotion() => CheckTarget("PointerEmbeddingPromotion");

    [TestMethod]
    public void CheckPointerFieldArrayElementAddress() => CheckTarget("PointerFieldArrayElementAddress");

    [TestMethod]
    public void CheckPointerFieldOfBoxedGlobal() => CheckTarget("PointerFieldOfBoxedGlobal");

    [TestMethod]
    public void CheckPointerInterfaceStructField() => CheckTarget("PointerInterfaceStructField");

    [TestMethod]
    public void CheckPointerParamCapturedInClosure() => CheckTarget("PointerParamCapturedInClosure");

    [TestMethod]
    public void CheckPointerParamInClosure() => CheckTarget("PointerParamInClosure");

    [TestMethod]
    public void CheckPointerParamNilWalk() => CheckTarget("PointerParamNilWalk");

    [TestMethod]
    public void CheckPointerParamWalk() => CheckTarget("PointerParamWalk");

    [TestMethod]
    public void CheckPointerReceiverNilCompare() => CheckTarget("PointerReceiverNilCompare");

    [TestMethod]
    public void CheckPointerReceiverPointerLocalField() => CheckTarget("PointerReceiverPointerLocalField");

    [TestMethod]
    public void CheckPointerReinterpretIdentity() => CheckTarget("PointerReinterpretIdentity");

    [TestMethod]
    public void CheckPointerRvalueFieldReceiver() => CheckTarget("PointerRvalueFieldReceiver");

    [TestMethod]
    public void CheckPointerSelectorDeref() => CheckTarget("PointerSelectorDeref");

    [TestMethod]
    public void CheckPointerToPointer() => CheckTarget("PointerToPointer");

    [TestMethod]
    public void CheckPointerValueToInterfaceArg() => CheckTarget("PointerValueToInterfaceArg");

    [TestMethod]
    public void CheckPrintfWidthFlags() => CheckTarget("PrintfWidthFlags");

    [TestMethod]
    public void CheckPromotedEmbedLib() => CheckTarget("PromotedEmbedLib");

    [TestMethod]
    public void CheckPromotedEmbedUser() => CheckTarget("PromotedEmbedUser");

    [TestMethod]
    public void CheckPromotedFieldNameIsType() => CheckTarget("PromotedFieldNameIsType");

    [TestMethod]
    public void CheckPromotedFieldPointerDeref() => CheckTarget("PromotedFieldPointerDeref");

    [TestMethod]
    public void CheckPromotedValueEmbedExprRecv() => CheckTarget("PromotedValueEmbedExprRecv");

    [TestMethod]
    public void CheckPromotedValueEmbedLib() => CheckTarget("PromotedValueEmbedLib");

    [TestMethod]
    public void CheckPromotedValueEmbedUser() => CheckTarget("PromotedValueEmbedUser");

    [TestMethod]
    public void CheckPtrKeyMapReceiverLookup() => CheckTarget("PtrKeyMapReceiverLookup");

    [TestMethod]
    public void CheckPublicizedFieldType() => CheckTarget("PublicizedFieldType");

    [TestMethod]
    public void CheckPublicizedFuncTypeParam() => CheckTarget("PublicizedFuncTypeParam");

    [TestMethod]
    public void CheckPublicizedInterfaceAnonAlias() => CheckTarget("PublicizedInterfaceAnonAlias");

    [TestMethod]
    public void CheckPublicizedInterfaceParam() => CheckTarget("PublicizedInterfaceParam");

    [TestMethod]
    public void CheckRangePointerArrayConversion() => CheckTarget("RangePointerArrayConversion");

    [TestMethod]
    public void CheckRangeShadowSelectorMethod() => CheckTarget("RangeShadowSelectorMethod");

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
    public void CheckRecvMapElementDeref() => CheckTarget("RecvMapElementDeref");

    [TestMethod]
    public void CheckRelationalPatternGuard() => CheckTarget("RelationalPatternGuard");

    [TestMethod]
    public void CheckRenamedReceiverBox() => CheckTarget("RenamedReceiverBox");

    [TestMethod]
    public void CheckReservedNameShadows() => CheckTarget("ReservedNameShadows");

    [TestMethod]
    public void CheckReservedTypeMethodCollision() => CheckTarget("ReservedTypeMethodCollision");

    [TestMethod]
    public void CheckReturnPointerFieldOfParam() => CheckTarget("ReturnPointerFieldOfParam");

    [TestMethod]
    public void CheckReturnTupleFuncLitArg() => CheckTarget("ReturnTupleFuncLitArg");

    [TestMethod]
    public void CheckReverseSortNaNOrder() => CheckTarget("ReverseSortNaNOrder");

    [TestMethod]
    public void CheckRingPointerMethods() => CheckTarget("RingPointerMethods");

    [TestMethod]
    public void CheckSameUnderlyingNamedConv() => CheckTarget("SameUnderlyingNamedConv");

    [TestMethod]
    public void CheckSelectEscapeBinding() => CheckTarget("SelectEscapeBinding");

    [TestMethod]
    public void CheckSelectStatement() => CheckTarget("SelectStatement");

    [TestMethod]
    public void CheckShadowedCompoundAssign() => CheckTarget("ShadowedCompoundAssign");

    [TestMethod]
    public void CheckShadowedHeapBoxReceiver() => CheckTarget("ShadowedHeapBoxReceiver");

    [TestMethod]
    public void CheckShadowedImportConstLib() => CheckTarget("ShadowedImportConstLib");

    [TestMethod]
    public void CheckShadowedImportConstUser() => CheckTarget("ShadowedImportConstUser");

    [TestMethod]
    public void CheckShadowedInterfaceEmbed() => CheckTarget("ShadowedInterfaceEmbed");

    [TestMethod]
    public void CheckShadowedPointerParam() => CheckTarget("ShadowedPointerParam");

    [TestMethod]
    public void CheckShadowedVarMethodCallLHS() => CheckTarget("ShadowedVarMethodCallLHS");

    [TestMethod]
    public void CheckShadowLocalOverRecvName() => CheckTarget("ShadowLocalOverRecvName");

    [TestMethod]
    public void CheckShadowRangeVarOverRecvName() => CheckTarget("ShadowRangeVarOverRecvName");

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
    public void CheckSliceNilVsEmpty() => CheckTarget("SliceNilVsEmpty");

    [TestMethod]
    public void CheckSolitaire() => CheckTarget("Solitaire");

    [TestMethod]
    public void CheckSortArrayType() => CheckTarget("SortArrayType");

    [TestMethod]
    public void CheckSparseArrayIfaceElem() => CheckTarget("SparseArrayIfaceElem");

    [TestMethod]
    public void CheckSparseArrayNamedIntKey() => CheckTarget("SparseArrayNamedIntKey");

    [TestMethod]
    public void CheckSpreadOperator() => CheckTarget("SpreadOperator");

    [TestMethod]
    public void CheckSStringElision() => CheckTarget("SStringElision");

    [TestMethod]
    public void CheckStdLibInternalAbi() => CheckTarget("StdLibInternalAbi");

    [TestMethod]
    public void CheckStringByteSemantics() => CheckTarget("StringByteSemantics");

    [TestMethod]
    public void CheckStringByteUnionConstraint() => CheckTarget("StringByteUnionConstraint");

    [TestMethod]
    public void CheckStringConvNamedInt() => CheckTarget("StringConvNamedInt");

    [TestMethod]
    public void CheckStringConvPostfix() => CheckTarget("StringConvPostfix");

    [TestMethod]
    public void CheckStringLenUtf8Bytes() => CheckTarget("StringLenUtf8Bytes");

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
    public void CheckSubpackageFuncTypeParam() => CheckTarget("SubpackageFuncTypeParam");

    [TestMethod]
    public void CheckSwitchBreakInCase() => CheckTarget("SwitchBreakInCase");

    [TestMethod]
    public void CheckSwitchDefaultOrder() => CheckTarget("SwitchDefaultOrder");

    [TestMethod]
    public void CheckSwitchFallthroughDefault() => CheckTarget("SwitchFallthroughDefault");

    [TestMethod]
    public void CheckSwitchFallthroughDefaultReturn() => CheckTarget("SwitchFallthroughDefaultReturn");

    [TestMethod]
    public void CheckSwitchNonConstCaseLabel() => CheckTarget("SwitchNonConstCaseLabel");

    [TestMethod]
    public void CheckSynthesizedDelegateChildPkg() => CheckTarget("SynthesizedDelegateChildPkg");

    [TestMethod]
    public void CheckSynthesizedDelegateCrossPkg() => CheckTarget("SynthesizedDelegateCrossPkg");

    [TestMethod]
    public void CheckSystemCollidingTypeName() => CheckTarget("SystemCollidingTypeName");

    [TestMethod]
    public void CheckTransitiveAliasPreload() => CheckTarget("TransitiveAliasPreload");

    [TestMethod]
    public void CheckTupleDestructureEscapingLocal() => CheckTarget("TupleDestructureEscapingLocal");

    [TestMethod]
    public void CheckTupleMixedDeclareReassign() => CheckTarget("TupleMixedDeclareReassign");

    [TestMethod]
    public void CheckTupleSpreadIntoCall() => CheckTarget("TupleSpreadIntoCall");

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
    public void CheckTypeSwitchImpureTag() => CheckTarget("TypeSwitchImpureTag");

    [TestMethod]
    public void CheckTypeSwitchMultiCase() => CheckTarget("TypeSwitchMultiCase");

    [TestMethod]
    public void CheckTypeSwitchNamedInterfaceCase() => CheckTarget("TypeSwitchNamedInterfaceCase");

    [TestMethod]
    public void CheckTypeSwitchPointerAdapter() => CheckTarget("TypeSwitchPointerAdapter");

    [TestMethod]
    public void CheckTypeSwitchTagShadowRename() => CheckTarget("TypeSwitchTagShadowRename");

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
    public void CheckUntypedConstDefine() => CheckTarget("UntypedConstDefine");

    [TestMethod]
    public void CheckUntypedConstFloatContext() => CheckTarget("UntypedConstFloatContext");

    [TestMethod]
    public void CheckUntypedFloatConstExpr() => CheckTarget("UntypedFloatConstExpr");

    [TestMethod]
    public void CheckUntypedFloatDefault() => CheckTarget("UntypedFloatDefault");

    [TestMethod]
    public void CheckUntypedIntFloatContexts() => CheckTarget("UntypedIntFloatContexts");

    [TestMethod]
    public void CheckUntypedIntInterfaceBox() => CheckTarget("UntypedIntInterfaceBox");

    [TestMethod]
    public void CheckUntypedNestedSliceComposite() => CheckTarget("UntypedNestedSliceComposite");

    [TestMethod]
    public void CheckVariableCapture() => CheckTarget("VariableCapture");

    [TestMethod]
    public void CheckVariadicBoxReceiver() => CheckTarget("VariadicBoxReceiver");

    [TestMethod]
    public void CheckVariadicClosureSpread() => CheckTarget("VariadicClosureSpread");

    [TestMethod]
    public void CheckVariadicFuncFields() => CheckTarget("VariadicFuncFields");

    [TestMethod]
    public void CheckVariadicFuncTypeAssert() => CheckTarget("VariadicFuncTypeAssert");

    [TestMethod]
    public void CheckVariadicFuncValues() => CheckTarget("VariadicFuncValues");

    [TestMethod]
    public void CheckVariadicPointerParam() => CheckTarget("VariadicPointerParam");

    [TestMethod]
    public void CheckVariadicSlotInterfaces() => CheckTarget("VariadicSlotInterfaces");

    [TestMethod]
    public void CheckVarNamedAsType() => CheckTarget("VarNamedAsType");

    [TestMethod]
    public void CheckVersionedImport() => CheckTarget("VersionedImport");

    [TestMethod]
    public void CheckWrittenCaptureParam() => CheckTarget("WrittenCaptureParam");

    // </TestMethods>

    private void CheckTarget(string targetProject)
    {
        TranspileProject(targetProject, true);
    }
}
