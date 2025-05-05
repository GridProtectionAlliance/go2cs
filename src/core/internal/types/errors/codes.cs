// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.types;

partial class errors_package {

[GoType("num:nint")] partial struct Code;

//go:generate go run golang.org/x/tools/cmd/stringer@latest -type Code codes.go
// This file defines the error codes that can be produced during type-checking.
// Collectively, these codes provide an identifier that may be used to
// implement special handling for certain types of errors.
//
// Error code values should not be changed: add new codes at the end.
//
// Error codes should be fine-grained enough that the exact nature of the error
// can be easily determined, but coarse enough that they are not an
// implementation detail of the type checking algorithm. As a rule-of-thumb,
// errors should be considered equivalent if there is a theoretical refactoring
// of the type checker in which they are emitted in exactly one place. For
// example, the type checker emits different error messages for "too many
// arguments" and "too few arguments", but one can imagine an alternative type
// checker where this check instead just emits a single "wrong number of
// arguments", so these errors should have the same code.
//
// Error code names should be as brief as possible while retaining accuracy and
// distinctiveness. In most cases names should start with an adjective
// describing the nature of the error (e.g. "invalid", "unused", "misplaced"),
// and end with a noun identifying the relevant language object. For example,
// "_DuplicateDecl" or "_InvalidSliceExpr". For brevity, naming follows the
// convention that "bad" implies a problem with syntax, and "invalid" implies a
// problem with types.
public static readonly Code InvalidSyntaxTree = -1;

internal static readonly Code Δ_ = /* iota */ 0;
public static readonly Code Test = 1;
public static readonly Code BlankPkgName = 2;
public static readonly Code MismatchedPkgName = 3;
public static readonly Code InvalidPkgUse = 4;
public static readonly Code BadImportPath = 5;
public static readonly Code BrokenImport = 6;
public static readonly Code ImportCRenamed = 7;
public static readonly Code UnusedImport = 8;
public static readonly Code InvalidInitCycle = 9;
public static readonly Code DuplicateDecl = 10;
public static readonly Code InvalidDeclCycle = 11;
public static readonly Code InvalidTypeCycle = 12;
public static readonly Code InvalidConstInit = 13;
public static readonly Code InvalidConstVal = 14;
public static readonly Code InvalidConstType = 15;
public static readonly Code UntypedNilUse = 16;
public static readonly Code WrongAssignCount = 17;
public static readonly Code UnassignableOperand = 18;
public static readonly Code NoNewVar = 19;
public static readonly Code MultiValAssignOp = 20;
public static readonly Code InvalidIfaceAssign = 21;
public static readonly Code InvalidChanAssign = 22;
public static readonly Code IncompatibleAssign = 23;
public static readonly Code UnaddressableFieldAssign = 24;
public static readonly Code NotAType = 25;
public static readonly Code InvalidArrayLen = 26;
public static readonly Code BlankIfaceMethod = 27;
public static readonly Code IncomparableMapKey = 28;
internal static readonly Code Δ_ = 29; // not used anymore
public static readonly Code InvalidPtrEmbed = 30;
public static readonly Code BadRecv = 31;
public static readonly Code InvalidRecv = 32;
public static readonly Code DuplicateFieldAndMethod = 33;
public static readonly Code DuplicateMethod = 34;
public static readonly Code InvalidBlank = 35;
public static readonly Code InvalidIota = 36;
public static readonly Code MissingInitBody = 37;
public static readonly Code InvalidInitSig = 38;
public static readonly Code InvalidInitDecl = 39;
public static readonly Code InvalidMainDecl = 40;
public static readonly Code TooManyValues = 41;
public static readonly Code NotAnExpr = 42;
public static readonly Code TruncatedFloat = 43;
public static readonly Code NumericOverflow = 44;
public static readonly Code UndefinedOp = 45;
public static readonly Code MismatchedTypes = 46;
public static readonly Code DivByZero = 47;
public static readonly Code NonNumericIncDec = 48;
public static readonly Code UnaddressableOperand = 49;
public static readonly Code InvalidIndirection = 50;
public static readonly Code NonIndexableOperand = 51;
public static readonly Code InvalidIndex = 52;
public static readonly Code SwappedSliceIndices = 53;
public static readonly Code NonSliceableOperand = 54;
public static readonly Code InvalidSliceExpr = 55;
public static readonly Code InvalidShiftCount = 56;
public static readonly Code InvalidShiftOperand = 57;
public static readonly Code InvalidReceive = 58;
public static readonly Code InvalidSend = 59;
public static readonly Code DuplicateLitKey = 60;
public static readonly Code MissingLitKey = 61;
public static readonly Code InvalidLitIndex = 62;
public static readonly Code OversizeArrayLit = 63;
public static readonly Code MixedStructLit = 64;
public static readonly Code InvalidStructLit = 65;
public static readonly Code MissingLitField = 66;
public static readonly Code DuplicateLitField = 67;
public static readonly Code UnexportedLitField = 68;
public static readonly Code InvalidLitField = 69;
public static readonly Code UntypedLit = 70;
public static readonly Code InvalidLit = 71;
public static readonly Code AmbiguousSelector = 72;
public static readonly Code UndeclaredImportedName = 73;
public static readonly Code UnexportedName = 74;
public static readonly Code UndeclaredName = 75;
public static readonly Code MissingFieldOrMethod = 76;
public static readonly Code BadDotDotDotSyntax = 77;
public static readonly Code NonVariadicDotDotDot = 78;
public static readonly Code MisplacedDotDotDot = 79;
internal static readonly Code Δ_ = 80; // InvalidDotDotDotOperand was removed.
public static readonly Code InvalidDotDotDot = 81;
public static readonly Code UncalledBuiltin = 82;
public static readonly Code InvalidAppend = 83;
public static readonly Code InvalidCap = 84;
public static readonly Code InvalidClose = 85;
public static readonly Code InvalidCopy = 86;
public static readonly Code InvalidComplex = 87;
public static readonly Code InvalidDelete = 88;
public static readonly Code InvalidImag = 89;
public static readonly Code InvalidLen = 90;
public static readonly Code SwappedMakeArgs = 91;
public static readonly Code InvalidMake = 92;
public static readonly Code InvalidReal = 93;
public static readonly Code InvalidAssert = 94;
public static readonly Code ImpossibleAssert = 95;
public static readonly Code InvalidConversion = 96;
public static readonly Code InvalidUntypedConversion = 97;
public static readonly Code BadOffsetofSyntax = 98;
public static readonly Code InvalidOffsetof = 99;
public static readonly Code UnusedExpr = 100;
public static readonly Code UnusedVar = 101;
public static readonly Code MissingReturn = 102;
public static readonly Code WrongResultCount = 103;
public static readonly Code OutOfScopeResult = 104;
public static readonly Code InvalidCond = 105;
public static readonly Code InvalidPostDecl = 106;
internal static readonly Code Δ_ = 107; // InvalidChanRange was removed.
public static readonly Code InvalidIterVar = 108;
public static readonly Code InvalidRangeExpr = 109;
public static readonly Code MisplacedBreak = 110;
public static readonly Code MisplacedContinue = 111;
public static readonly Code MisplacedFallthrough = 112;
public static readonly Code DuplicateCase = 113;
public static readonly Code DuplicateDefault = 114;
public static readonly Code BadTypeKeyword = 115;
public static readonly Code InvalidTypeSwitch = 116;
public static readonly Code InvalidExprSwitch = 117;
public static readonly Code InvalidSelectCase = 118;
public static readonly Code UndeclaredLabel = 119;
public static readonly Code DuplicateLabel = 120;
public static readonly Code MisplacedLabel = 121;
public static readonly Code UnusedLabel = 122;
public static readonly Code JumpOverDecl = 123;
public static readonly Code JumpIntoBlock = 124;
public static readonly Code InvalidMethodExpr = 125;
public static readonly Code WrongArgCount = 126;
public static readonly Code InvalidCall = 127;
public static readonly Code UnusedResults = 128;
public static readonly Code InvalidDefer = 129;
public static readonly Code InvalidGo = 130;
// All codes below were added in Go 1.17.
public static readonly Code BadDecl = 131;
public static readonly Code RepeatedDecl = 132;
public static readonly Code InvalidUnsafeAdd = 133;
public static readonly Code InvalidUnsafeSlice = 134;
// All codes below were added in Go 1.18.
public static readonly Code UnsupportedFeature = 135;
public static readonly Code NotAGenericType = 136;
public static readonly Code WrongTypeArgCount = 137;
public static readonly Code CannotInferTypeArgs = 138;
public static readonly Code InvalidTypeArg = 139; // arguments? InferenceFailed
public static readonly Code InvalidInstanceCycle = 140;
public static readonly Code InvalidUnion = 141;
public static readonly Code MisplacedConstraintIface = 142;
public static readonly Code InvalidMethodTypeParams = 143;
public static readonly Code MisplacedTypeParam = 144;
public static readonly Code InvalidUnsafeSliceData = 145;
public static readonly Code InvalidUnsafeString = 146;
internal static readonly Code Δ_ = 147; // not used anymore
public static readonly Code InvalidClear = 148;
public static readonly Code TypeTooLarge = 149;
public static readonly Code InvalidMinMaxOperand = 150;
public static readonly Code TooNew = 151;

} // end errors_package
