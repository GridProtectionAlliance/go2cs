// Code generated from gen/MIPS64.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 October 09 05:35:53 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewriteMIPS64.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static bool rewriteValueMIPS64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Op == OpAdd16) 
                v.Op = OpMIPS64ADDV;
                return true;
            else if (v.Op == OpAdd32) 
                v.Op = OpMIPS64ADDV;
                return true;
            else if (v.Op == OpAdd32F) 
                v.Op = OpMIPS64ADDF;
                return true;
            else if (v.Op == OpAdd64) 
                v.Op = OpMIPS64ADDV;
                return true;
            else if (v.Op == OpAdd64F) 
                v.Op = OpMIPS64ADDD;
                return true;
            else if (v.Op == OpAdd8) 
                v.Op = OpMIPS64ADDV;
                return true;
            else if (v.Op == OpAddPtr) 
                v.Op = OpMIPS64ADDV;
                return true;
            else if (v.Op == OpAddr) 
                v.Op = OpMIPS64MOVVaddr;
                return true;
            else if (v.Op == OpAnd16) 
                v.Op = OpMIPS64AND;
                return true;
            else if (v.Op == OpAnd32) 
                v.Op = OpMIPS64AND;
                return true;
            else if (v.Op == OpAnd64) 
                v.Op = OpMIPS64AND;
                return true;
            else if (v.Op == OpAnd8) 
                v.Op = OpMIPS64AND;
                return true;
            else if (v.Op == OpAndB) 
                v.Op = OpMIPS64AND;
                return true;
            else if (v.Op == OpAtomicAdd32) 
                v.Op = OpMIPS64LoweredAtomicAdd32;
                return true;
            else if (v.Op == OpAtomicAdd64) 
                v.Op = OpMIPS64LoweredAtomicAdd64;
                return true;
            else if (v.Op == OpAtomicCompareAndSwap32) 
                v.Op = OpMIPS64LoweredAtomicCas32;
                return true;
            else if (v.Op == OpAtomicCompareAndSwap64) 
                v.Op = OpMIPS64LoweredAtomicCas64;
                return true;
            else if (v.Op == OpAtomicExchange32) 
                v.Op = OpMIPS64LoweredAtomicExchange32;
                return true;
            else if (v.Op == OpAtomicExchange64) 
                v.Op = OpMIPS64LoweredAtomicExchange64;
                return true;
            else if (v.Op == OpAtomicLoad32) 
                v.Op = OpMIPS64LoweredAtomicLoad32;
                return true;
            else if (v.Op == OpAtomicLoad64) 
                v.Op = OpMIPS64LoweredAtomicLoad64;
                return true;
            else if (v.Op == OpAtomicLoad8) 
                v.Op = OpMIPS64LoweredAtomicLoad8;
                return true;
            else if (v.Op == OpAtomicLoadPtr) 
                v.Op = OpMIPS64LoweredAtomicLoad64;
                return true;
            else if (v.Op == OpAtomicStore32) 
                v.Op = OpMIPS64LoweredAtomicStore32;
                return true;
            else if (v.Op == OpAtomicStore64) 
                v.Op = OpMIPS64LoweredAtomicStore64;
                return true;
            else if (v.Op == OpAtomicStore8) 
                v.Op = OpMIPS64LoweredAtomicStore8;
                return true;
            else if (v.Op == OpAtomicStorePtrNoWB) 
                v.Op = OpMIPS64LoweredAtomicStore64;
                return true;
            else if (v.Op == OpAvg64u) 
                return rewriteValueMIPS64_OpAvg64u(_addr_v);
            else if (v.Op == OpClosureCall) 
                v.Op = OpMIPS64CALLclosure;
                return true;
            else if (v.Op == OpCom16) 
                return rewriteValueMIPS64_OpCom16(_addr_v);
            else if (v.Op == OpCom32) 
                return rewriteValueMIPS64_OpCom32(_addr_v);
            else if (v.Op == OpCom64) 
                return rewriteValueMIPS64_OpCom64(_addr_v);
            else if (v.Op == OpCom8) 
                return rewriteValueMIPS64_OpCom8(_addr_v);
            else if (v.Op == OpConst16) 
                v.Op = OpMIPS64MOVVconst;
                return true;
            else if (v.Op == OpConst32) 
                v.Op = OpMIPS64MOVVconst;
                return true;
            else if (v.Op == OpConst32F) 
                v.Op = OpMIPS64MOVFconst;
                return true;
            else if (v.Op == OpConst64) 
                v.Op = OpMIPS64MOVVconst;
                return true;
            else if (v.Op == OpConst64F) 
                v.Op = OpMIPS64MOVDconst;
                return true;
            else if (v.Op == OpConst8) 
                v.Op = OpMIPS64MOVVconst;
                return true;
            else if (v.Op == OpConstBool) 
                v.Op = OpMIPS64MOVVconst;
                return true;
            else if (v.Op == OpConstNil) 
                return rewriteValueMIPS64_OpConstNil(_addr_v);
            else if (v.Op == OpCvt32Fto32) 
                v.Op = OpMIPS64TRUNCFW;
                return true;
            else if (v.Op == OpCvt32Fto64) 
                v.Op = OpMIPS64TRUNCFV;
                return true;
            else if (v.Op == OpCvt32Fto64F) 
                v.Op = OpMIPS64MOVFD;
                return true;
            else if (v.Op == OpCvt32to32F) 
                v.Op = OpMIPS64MOVWF;
                return true;
            else if (v.Op == OpCvt32to64F) 
                v.Op = OpMIPS64MOVWD;
                return true;
            else if (v.Op == OpCvt64Fto32) 
                v.Op = OpMIPS64TRUNCDW;
                return true;
            else if (v.Op == OpCvt64Fto32F) 
                v.Op = OpMIPS64MOVDF;
                return true;
            else if (v.Op == OpCvt64Fto64) 
                v.Op = OpMIPS64TRUNCDV;
                return true;
            else if (v.Op == OpCvt64to32F) 
                v.Op = OpMIPS64MOVVF;
                return true;
            else if (v.Op == OpCvt64to64F) 
                v.Op = OpMIPS64MOVVD;
                return true;
            else if (v.Op == OpCvtBoolToUint8) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpDiv16) 
                return rewriteValueMIPS64_OpDiv16(_addr_v);
            else if (v.Op == OpDiv16u) 
                return rewriteValueMIPS64_OpDiv16u(_addr_v);
            else if (v.Op == OpDiv32) 
                return rewriteValueMIPS64_OpDiv32(_addr_v);
            else if (v.Op == OpDiv32F) 
                v.Op = OpMIPS64DIVF;
                return true;
            else if (v.Op == OpDiv32u) 
                return rewriteValueMIPS64_OpDiv32u(_addr_v);
            else if (v.Op == OpDiv64) 
                return rewriteValueMIPS64_OpDiv64(_addr_v);
            else if (v.Op == OpDiv64F) 
                v.Op = OpMIPS64DIVD;
                return true;
            else if (v.Op == OpDiv64u) 
                return rewriteValueMIPS64_OpDiv64u(_addr_v);
            else if (v.Op == OpDiv8) 
                return rewriteValueMIPS64_OpDiv8(_addr_v);
            else if (v.Op == OpDiv8u) 
                return rewriteValueMIPS64_OpDiv8u(_addr_v);
            else if (v.Op == OpEq16) 
                return rewriteValueMIPS64_OpEq16(_addr_v);
            else if (v.Op == OpEq32) 
                return rewriteValueMIPS64_OpEq32(_addr_v);
            else if (v.Op == OpEq32F) 
                return rewriteValueMIPS64_OpEq32F(_addr_v);
            else if (v.Op == OpEq64) 
                return rewriteValueMIPS64_OpEq64(_addr_v);
            else if (v.Op == OpEq64F) 
                return rewriteValueMIPS64_OpEq64F(_addr_v);
            else if (v.Op == OpEq8) 
                return rewriteValueMIPS64_OpEq8(_addr_v);
            else if (v.Op == OpEqB) 
                return rewriteValueMIPS64_OpEqB(_addr_v);
            else if (v.Op == OpEqPtr) 
                return rewriteValueMIPS64_OpEqPtr(_addr_v);
            else if (v.Op == OpGetCallerPC) 
                v.Op = OpMIPS64LoweredGetCallerPC;
                return true;
            else if (v.Op == OpGetCallerSP) 
                v.Op = OpMIPS64LoweredGetCallerSP;
                return true;
            else if (v.Op == OpGetClosurePtr) 
                v.Op = OpMIPS64LoweredGetClosurePtr;
                return true;
            else if (v.Op == OpHmul32) 
                return rewriteValueMIPS64_OpHmul32(_addr_v);
            else if (v.Op == OpHmul32u) 
                return rewriteValueMIPS64_OpHmul32u(_addr_v);
            else if (v.Op == OpHmul64) 
                return rewriteValueMIPS64_OpHmul64(_addr_v);
            else if (v.Op == OpHmul64u) 
                return rewriteValueMIPS64_OpHmul64u(_addr_v);
            else if (v.Op == OpInterCall) 
                v.Op = OpMIPS64CALLinter;
                return true;
            else if (v.Op == OpIsInBounds) 
                return rewriteValueMIPS64_OpIsInBounds(_addr_v);
            else if (v.Op == OpIsNonNil) 
                return rewriteValueMIPS64_OpIsNonNil(_addr_v);
            else if (v.Op == OpIsSliceInBounds) 
                return rewriteValueMIPS64_OpIsSliceInBounds(_addr_v);
            else if (v.Op == OpLeq16) 
                return rewriteValueMIPS64_OpLeq16(_addr_v);
            else if (v.Op == OpLeq16U) 
                return rewriteValueMIPS64_OpLeq16U(_addr_v);
            else if (v.Op == OpLeq32) 
                return rewriteValueMIPS64_OpLeq32(_addr_v);
            else if (v.Op == OpLeq32F) 
                return rewriteValueMIPS64_OpLeq32F(_addr_v);
            else if (v.Op == OpLeq32U) 
                return rewriteValueMIPS64_OpLeq32U(_addr_v);
            else if (v.Op == OpLeq64) 
                return rewriteValueMIPS64_OpLeq64(_addr_v);
            else if (v.Op == OpLeq64F) 
                return rewriteValueMIPS64_OpLeq64F(_addr_v);
            else if (v.Op == OpLeq64U) 
                return rewriteValueMIPS64_OpLeq64U(_addr_v);
            else if (v.Op == OpLeq8) 
                return rewriteValueMIPS64_OpLeq8(_addr_v);
            else if (v.Op == OpLeq8U) 
                return rewriteValueMIPS64_OpLeq8U(_addr_v);
            else if (v.Op == OpLess16) 
                return rewriteValueMIPS64_OpLess16(_addr_v);
            else if (v.Op == OpLess16U) 
                return rewriteValueMIPS64_OpLess16U(_addr_v);
            else if (v.Op == OpLess32) 
                return rewriteValueMIPS64_OpLess32(_addr_v);
            else if (v.Op == OpLess32F) 
                return rewriteValueMIPS64_OpLess32F(_addr_v);
            else if (v.Op == OpLess32U) 
                return rewriteValueMIPS64_OpLess32U(_addr_v);
            else if (v.Op == OpLess64) 
                return rewriteValueMIPS64_OpLess64(_addr_v);
            else if (v.Op == OpLess64F) 
                return rewriteValueMIPS64_OpLess64F(_addr_v);
            else if (v.Op == OpLess64U) 
                return rewriteValueMIPS64_OpLess64U(_addr_v);
            else if (v.Op == OpLess8) 
                return rewriteValueMIPS64_OpLess8(_addr_v);
            else if (v.Op == OpLess8U) 
                return rewriteValueMIPS64_OpLess8U(_addr_v);
            else if (v.Op == OpLoad) 
                return rewriteValueMIPS64_OpLoad(_addr_v);
            else if (v.Op == OpLocalAddr) 
                return rewriteValueMIPS64_OpLocalAddr(_addr_v);
            else if (v.Op == OpLsh16x16) 
                return rewriteValueMIPS64_OpLsh16x16(_addr_v);
            else if (v.Op == OpLsh16x32) 
                return rewriteValueMIPS64_OpLsh16x32(_addr_v);
            else if (v.Op == OpLsh16x64) 
                return rewriteValueMIPS64_OpLsh16x64(_addr_v);
            else if (v.Op == OpLsh16x8) 
                return rewriteValueMIPS64_OpLsh16x8(_addr_v);
            else if (v.Op == OpLsh32x16) 
                return rewriteValueMIPS64_OpLsh32x16(_addr_v);
            else if (v.Op == OpLsh32x32) 
                return rewriteValueMIPS64_OpLsh32x32(_addr_v);
            else if (v.Op == OpLsh32x64) 
                return rewriteValueMIPS64_OpLsh32x64(_addr_v);
            else if (v.Op == OpLsh32x8) 
                return rewriteValueMIPS64_OpLsh32x8(_addr_v);
            else if (v.Op == OpLsh64x16) 
                return rewriteValueMIPS64_OpLsh64x16(_addr_v);
            else if (v.Op == OpLsh64x32) 
                return rewriteValueMIPS64_OpLsh64x32(_addr_v);
            else if (v.Op == OpLsh64x64) 
                return rewriteValueMIPS64_OpLsh64x64(_addr_v);
            else if (v.Op == OpLsh64x8) 
                return rewriteValueMIPS64_OpLsh64x8(_addr_v);
            else if (v.Op == OpLsh8x16) 
                return rewriteValueMIPS64_OpLsh8x16(_addr_v);
            else if (v.Op == OpLsh8x32) 
                return rewriteValueMIPS64_OpLsh8x32(_addr_v);
            else if (v.Op == OpLsh8x64) 
                return rewriteValueMIPS64_OpLsh8x64(_addr_v);
            else if (v.Op == OpLsh8x8) 
                return rewriteValueMIPS64_OpLsh8x8(_addr_v);
            else if (v.Op == OpMIPS64ADDV) 
                return rewriteValueMIPS64_OpMIPS64ADDV(_addr_v);
            else if (v.Op == OpMIPS64ADDVconst) 
                return rewriteValueMIPS64_OpMIPS64ADDVconst(_addr_v);
            else if (v.Op == OpMIPS64AND) 
                return rewriteValueMIPS64_OpMIPS64AND(_addr_v);
            else if (v.Op == OpMIPS64ANDconst) 
                return rewriteValueMIPS64_OpMIPS64ANDconst(_addr_v);
            else if (v.Op == OpMIPS64LoweredAtomicAdd32) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd32(_addr_v);
            else if (v.Op == OpMIPS64LoweredAtomicAdd64) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd64(_addr_v);
            else if (v.Op == OpMIPS64LoweredAtomicStore32) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicStore32(_addr_v);
            else if (v.Op == OpMIPS64LoweredAtomicStore64) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicStore64(_addr_v);
            else if (v.Op == OpMIPS64MOVBUload) 
                return rewriteValueMIPS64_OpMIPS64MOVBUload(_addr_v);
            else if (v.Op == OpMIPS64MOVBUreg) 
                return rewriteValueMIPS64_OpMIPS64MOVBUreg(_addr_v);
            else if (v.Op == OpMIPS64MOVBload) 
                return rewriteValueMIPS64_OpMIPS64MOVBload(_addr_v);
            else if (v.Op == OpMIPS64MOVBreg) 
                return rewriteValueMIPS64_OpMIPS64MOVBreg(_addr_v);
            else if (v.Op == OpMIPS64MOVBstore) 
                return rewriteValueMIPS64_OpMIPS64MOVBstore(_addr_v);
            else if (v.Op == OpMIPS64MOVBstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVBstorezero(_addr_v);
            else if (v.Op == OpMIPS64MOVDload) 
                return rewriteValueMIPS64_OpMIPS64MOVDload(_addr_v);
            else if (v.Op == OpMIPS64MOVDstore) 
                return rewriteValueMIPS64_OpMIPS64MOVDstore(_addr_v);
            else if (v.Op == OpMIPS64MOVFload) 
                return rewriteValueMIPS64_OpMIPS64MOVFload(_addr_v);
            else if (v.Op == OpMIPS64MOVFstore) 
                return rewriteValueMIPS64_OpMIPS64MOVFstore(_addr_v);
            else if (v.Op == OpMIPS64MOVHUload) 
                return rewriteValueMIPS64_OpMIPS64MOVHUload(_addr_v);
            else if (v.Op == OpMIPS64MOVHUreg) 
                return rewriteValueMIPS64_OpMIPS64MOVHUreg(_addr_v);
            else if (v.Op == OpMIPS64MOVHload) 
                return rewriteValueMIPS64_OpMIPS64MOVHload(_addr_v);
            else if (v.Op == OpMIPS64MOVHreg) 
                return rewriteValueMIPS64_OpMIPS64MOVHreg(_addr_v);
            else if (v.Op == OpMIPS64MOVHstore) 
                return rewriteValueMIPS64_OpMIPS64MOVHstore(_addr_v);
            else if (v.Op == OpMIPS64MOVHstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVHstorezero(_addr_v);
            else if (v.Op == OpMIPS64MOVVload) 
                return rewriteValueMIPS64_OpMIPS64MOVVload(_addr_v);
            else if (v.Op == OpMIPS64MOVVreg) 
                return rewriteValueMIPS64_OpMIPS64MOVVreg(_addr_v);
            else if (v.Op == OpMIPS64MOVVstore) 
                return rewriteValueMIPS64_OpMIPS64MOVVstore(_addr_v);
            else if (v.Op == OpMIPS64MOVVstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVVstorezero(_addr_v);
            else if (v.Op == OpMIPS64MOVWUload) 
                return rewriteValueMIPS64_OpMIPS64MOVWUload(_addr_v);
            else if (v.Op == OpMIPS64MOVWUreg) 
                return rewriteValueMIPS64_OpMIPS64MOVWUreg(_addr_v);
            else if (v.Op == OpMIPS64MOVWload) 
                return rewriteValueMIPS64_OpMIPS64MOVWload(_addr_v);
            else if (v.Op == OpMIPS64MOVWreg) 
                return rewriteValueMIPS64_OpMIPS64MOVWreg(_addr_v);
            else if (v.Op == OpMIPS64MOVWstore) 
                return rewriteValueMIPS64_OpMIPS64MOVWstore(_addr_v);
            else if (v.Op == OpMIPS64MOVWstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVWstorezero(_addr_v);
            else if (v.Op == OpMIPS64NEGV) 
                return rewriteValueMIPS64_OpMIPS64NEGV(_addr_v);
            else if (v.Op == OpMIPS64NOR) 
                return rewriteValueMIPS64_OpMIPS64NOR(_addr_v);
            else if (v.Op == OpMIPS64NORconst) 
                return rewriteValueMIPS64_OpMIPS64NORconst(_addr_v);
            else if (v.Op == OpMIPS64OR) 
                return rewriteValueMIPS64_OpMIPS64OR(_addr_v);
            else if (v.Op == OpMIPS64ORconst) 
                return rewriteValueMIPS64_OpMIPS64ORconst(_addr_v);
            else if (v.Op == OpMIPS64SGT) 
                return rewriteValueMIPS64_OpMIPS64SGT(_addr_v);
            else if (v.Op == OpMIPS64SGTU) 
                return rewriteValueMIPS64_OpMIPS64SGTU(_addr_v);
            else if (v.Op == OpMIPS64SGTUconst) 
                return rewriteValueMIPS64_OpMIPS64SGTUconst(_addr_v);
            else if (v.Op == OpMIPS64SGTconst) 
                return rewriteValueMIPS64_OpMIPS64SGTconst(_addr_v);
            else if (v.Op == OpMIPS64SLLV) 
                return rewriteValueMIPS64_OpMIPS64SLLV(_addr_v);
            else if (v.Op == OpMIPS64SLLVconst) 
                return rewriteValueMIPS64_OpMIPS64SLLVconst(_addr_v);
            else if (v.Op == OpMIPS64SRAV) 
                return rewriteValueMIPS64_OpMIPS64SRAV(_addr_v);
            else if (v.Op == OpMIPS64SRAVconst) 
                return rewriteValueMIPS64_OpMIPS64SRAVconst(_addr_v);
            else if (v.Op == OpMIPS64SRLV) 
                return rewriteValueMIPS64_OpMIPS64SRLV(_addr_v);
            else if (v.Op == OpMIPS64SRLVconst) 
                return rewriteValueMIPS64_OpMIPS64SRLVconst(_addr_v);
            else if (v.Op == OpMIPS64SUBV) 
                return rewriteValueMIPS64_OpMIPS64SUBV(_addr_v);
            else if (v.Op == OpMIPS64SUBVconst) 
                return rewriteValueMIPS64_OpMIPS64SUBVconst(_addr_v);
            else if (v.Op == OpMIPS64XOR) 
                return rewriteValueMIPS64_OpMIPS64XOR(_addr_v);
            else if (v.Op == OpMIPS64XORconst) 
                return rewriteValueMIPS64_OpMIPS64XORconst(_addr_v);
            else if (v.Op == OpMod16) 
                return rewriteValueMIPS64_OpMod16(_addr_v);
            else if (v.Op == OpMod16u) 
                return rewriteValueMIPS64_OpMod16u(_addr_v);
            else if (v.Op == OpMod32) 
                return rewriteValueMIPS64_OpMod32(_addr_v);
            else if (v.Op == OpMod32u) 
                return rewriteValueMIPS64_OpMod32u(_addr_v);
            else if (v.Op == OpMod64) 
                return rewriteValueMIPS64_OpMod64(_addr_v);
            else if (v.Op == OpMod64u) 
                return rewriteValueMIPS64_OpMod64u(_addr_v);
            else if (v.Op == OpMod8) 
                return rewriteValueMIPS64_OpMod8(_addr_v);
            else if (v.Op == OpMod8u) 
                return rewriteValueMIPS64_OpMod8u(_addr_v);
            else if (v.Op == OpMove) 
                return rewriteValueMIPS64_OpMove(_addr_v);
            else if (v.Op == OpMul16) 
                return rewriteValueMIPS64_OpMul16(_addr_v);
            else if (v.Op == OpMul32) 
                return rewriteValueMIPS64_OpMul32(_addr_v);
            else if (v.Op == OpMul32F) 
                v.Op = OpMIPS64MULF;
                return true;
            else if (v.Op == OpMul64) 
                return rewriteValueMIPS64_OpMul64(_addr_v);
            else if (v.Op == OpMul64F) 
                v.Op = OpMIPS64MULD;
                return true;
            else if (v.Op == OpMul64uhilo) 
                v.Op = OpMIPS64MULVU;
                return true;
            else if (v.Op == OpMul8) 
                return rewriteValueMIPS64_OpMul8(_addr_v);
            else if (v.Op == OpNeg16) 
                v.Op = OpMIPS64NEGV;
                return true;
            else if (v.Op == OpNeg32) 
                v.Op = OpMIPS64NEGV;
                return true;
            else if (v.Op == OpNeg32F) 
                v.Op = OpMIPS64NEGF;
                return true;
            else if (v.Op == OpNeg64) 
                v.Op = OpMIPS64NEGV;
                return true;
            else if (v.Op == OpNeg64F) 
                v.Op = OpMIPS64NEGD;
                return true;
            else if (v.Op == OpNeg8) 
                v.Op = OpMIPS64NEGV;
                return true;
            else if (v.Op == OpNeq16) 
                return rewriteValueMIPS64_OpNeq16(_addr_v);
            else if (v.Op == OpNeq32) 
                return rewriteValueMIPS64_OpNeq32(_addr_v);
            else if (v.Op == OpNeq32F) 
                return rewriteValueMIPS64_OpNeq32F(_addr_v);
            else if (v.Op == OpNeq64) 
                return rewriteValueMIPS64_OpNeq64(_addr_v);
            else if (v.Op == OpNeq64F) 
                return rewriteValueMIPS64_OpNeq64F(_addr_v);
            else if (v.Op == OpNeq8) 
                return rewriteValueMIPS64_OpNeq8(_addr_v);
            else if (v.Op == OpNeqB) 
                v.Op = OpMIPS64XOR;
                return true;
            else if (v.Op == OpNeqPtr) 
                return rewriteValueMIPS64_OpNeqPtr(_addr_v);
            else if (v.Op == OpNilCheck) 
                v.Op = OpMIPS64LoweredNilCheck;
                return true;
            else if (v.Op == OpNot) 
                return rewriteValueMIPS64_OpNot(_addr_v);
            else if (v.Op == OpOffPtr) 
                return rewriteValueMIPS64_OpOffPtr(_addr_v);
            else if (v.Op == OpOr16) 
                v.Op = OpMIPS64OR;
                return true;
            else if (v.Op == OpOr32) 
                v.Op = OpMIPS64OR;
                return true;
            else if (v.Op == OpOr64) 
                v.Op = OpMIPS64OR;
                return true;
            else if (v.Op == OpOr8) 
                v.Op = OpMIPS64OR;
                return true;
            else if (v.Op == OpOrB) 
                v.Op = OpMIPS64OR;
                return true;
            else if (v.Op == OpPanicBounds) 
                return rewriteValueMIPS64_OpPanicBounds(_addr_v);
            else if (v.Op == OpRotateLeft16) 
                return rewriteValueMIPS64_OpRotateLeft16(_addr_v);
            else if (v.Op == OpRotateLeft32) 
                return rewriteValueMIPS64_OpRotateLeft32(_addr_v);
            else if (v.Op == OpRotateLeft64) 
                return rewriteValueMIPS64_OpRotateLeft64(_addr_v);
            else if (v.Op == OpRotateLeft8) 
                return rewriteValueMIPS64_OpRotateLeft8(_addr_v);
            else if (v.Op == OpRound32F) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpRound64F) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpRsh16Ux16) 
                return rewriteValueMIPS64_OpRsh16Ux16(_addr_v);
            else if (v.Op == OpRsh16Ux32) 
                return rewriteValueMIPS64_OpRsh16Ux32(_addr_v);
            else if (v.Op == OpRsh16Ux64) 
                return rewriteValueMIPS64_OpRsh16Ux64(_addr_v);
            else if (v.Op == OpRsh16Ux8) 
                return rewriteValueMIPS64_OpRsh16Ux8(_addr_v);
            else if (v.Op == OpRsh16x16) 
                return rewriteValueMIPS64_OpRsh16x16(_addr_v);
            else if (v.Op == OpRsh16x32) 
                return rewriteValueMIPS64_OpRsh16x32(_addr_v);
            else if (v.Op == OpRsh16x64) 
                return rewriteValueMIPS64_OpRsh16x64(_addr_v);
            else if (v.Op == OpRsh16x8) 
                return rewriteValueMIPS64_OpRsh16x8(_addr_v);
            else if (v.Op == OpRsh32Ux16) 
                return rewriteValueMIPS64_OpRsh32Ux16(_addr_v);
            else if (v.Op == OpRsh32Ux32) 
                return rewriteValueMIPS64_OpRsh32Ux32(_addr_v);
            else if (v.Op == OpRsh32Ux64) 
                return rewriteValueMIPS64_OpRsh32Ux64(_addr_v);
            else if (v.Op == OpRsh32Ux8) 
                return rewriteValueMIPS64_OpRsh32Ux8(_addr_v);
            else if (v.Op == OpRsh32x16) 
                return rewriteValueMIPS64_OpRsh32x16(_addr_v);
            else if (v.Op == OpRsh32x32) 
                return rewriteValueMIPS64_OpRsh32x32(_addr_v);
            else if (v.Op == OpRsh32x64) 
                return rewriteValueMIPS64_OpRsh32x64(_addr_v);
            else if (v.Op == OpRsh32x8) 
                return rewriteValueMIPS64_OpRsh32x8(_addr_v);
            else if (v.Op == OpRsh64Ux16) 
                return rewriteValueMIPS64_OpRsh64Ux16(_addr_v);
            else if (v.Op == OpRsh64Ux32) 
                return rewriteValueMIPS64_OpRsh64Ux32(_addr_v);
            else if (v.Op == OpRsh64Ux64) 
                return rewriteValueMIPS64_OpRsh64Ux64(_addr_v);
            else if (v.Op == OpRsh64Ux8) 
                return rewriteValueMIPS64_OpRsh64Ux8(_addr_v);
            else if (v.Op == OpRsh64x16) 
                return rewriteValueMIPS64_OpRsh64x16(_addr_v);
            else if (v.Op == OpRsh64x32) 
                return rewriteValueMIPS64_OpRsh64x32(_addr_v);
            else if (v.Op == OpRsh64x64) 
                return rewriteValueMIPS64_OpRsh64x64(_addr_v);
            else if (v.Op == OpRsh64x8) 
                return rewriteValueMIPS64_OpRsh64x8(_addr_v);
            else if (v.Op == OpRsh8Ux16) 
                return rewriteValueMIPS64_OpRsh8Ux16(_addr_v);
            else if (v.Op == OpRsh8Ux32) 
                return rewriteValueMIPS64_OpRsh8Ux32(_addr_v);
            else if (v.Op == OpRsh8Ux64) 
                return rewriteValueMIPS64_OpRsh8Ux64(_addr_v);
            else if (v.Op == OpRsh8Ux8) 
                return rewriteValueMIPS64_OpRsh8Ux8(_addr_v);
            else if (v.Op == OpRsh8x16) 
                return rewriteValueMIPS64_OpRsh8x16(_addr_v);
            else if (v.Op == OpRsh8x32) 
                return rewriteValueMIPS64_OpRsh8x32(_addr_v);
            else if (v.Op == OpRsh8x64) 
                return rewriteValueMIPS64_OpRsh8x64(_addr_v);
            else if (v.Op == OpRsh8x8) 
                return rewriteValueMIPS64_OpRsh8x8(_addr_v);
            else if (v.Op == OpSelect0) 
                return rewriteValueMIPS64_OpSelect0(_addr_v);
            else if (v.Op == OpSelect1) 
                return rewriteValueMIPS64_OpSelect1(_addr_v);
            else if (v.Op == OpSignExt16to32) 
                v.Op = OpMIPS64MOVHreg;
                return true;
            else if (v.Op == OpSignExt16to64) 
                v.Op = OpMIPS64MOVHreg;
                return true;
            else if (v.Op == OpSignExt32to64) 
                v.Op = OpMIPS64MOVWreg;
                return true;
            else if (v.Op == OpSignExt8to16) 
                v.Op = OpMIPS64MOVBreg;
                return true;
            else if (v.Op == OpSignExt8to32) 
                v.Op = OpMIPS64MOVBreg;
                return true;
            else if (v.Op == OpSignExt8to64) 
                v.Op = OpMIPS64MOVBreg;
                return true;
            else if (v.Op == OpSlicemask) 
                return rewriteValueMIPS64_OpSlicemask(_addr_v);
            else if (v.Op == OpSqrt) 
                v.Op = OpMIPS64SQRTD;
                return true;
            else if (v.Op == OpStaticCall) 
                v.Op = OpMIPS64CALLstatic;
                return true;
            else if (v.Op == OpStore) 
                return rewriteValueMIPS64_OpStore(_addr_v);
            else if (v.Op == OpSub16) 
                v.Op = OpMIPS64SUBV;
                return true;
            else if (v.Op == OpSub32) 
                v.Op = OpMIPS64SUBV;
                return true;
            else if (v.Op == OpSub32F) 
                v.Op = OpMIPS64SUBF;
                return true;
            else if (v.Op == OpSub64) 
                v.Op = OpMIPS64SUBV;
                return true;
            else if (v.Op == OpSub64F) 
                v.Op = OpMIPS64SUBD;
                return true;
            else if (v.Op == OpSub8) 
                v.Op = OpMIPS64SUBV;
                return true;
            else if (v.Op == OpSubPtr) 
                v.Op = OpMIPS64SUBV;
                return true;
            else if (v.Op == OpTrunc16to8) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpTrunc32to16) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpTrunc32to8) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpTrunc64to16) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpTrunc64to32) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpTrunc64to8) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpWB) 
                v.Op = OpMIPS64LoweredWB;
                return true;
            else if (v.Op == OpXor16) 
                v.Op = OpMIPS64XOR;
                return true;
            else if (v.Op == OpXor32) 
                v.Op = OpMIPS64XOR;
                return true;
            else if (v.Op == OpXor64) 
                v.Op = OpMIPS64XOR;
                return true;
            else if (v.Op == OpXor8) 
                v.Op = OpMIPS64XOR;
                return true;
            else if (v.Op == OpZero) 
                return rewriteValueMIPS64_OpZero(_addr_v);
            else if (v.Op == OpZeroExt16to32) 
                v.Op = OpMIPS64MOVHUreg;
                return true;
            else if (v.Op == OpZeroExt16to64) 
                v.Op = OpMIPS64MOVHUreg;
                return true;
            else if (v.Op == OpZeroExt32to64) 
                v.Op = OpMIPS64MOVWUreg;
                return true;
            else if (v.Op == OpZeroExt8to16) 
                v.Op = OpMIPS64MOVBUreg;
                return true;
            else if (v.Op == OpZeroExt8to32) 
                v.Op = OpMIPS64MOVBUreg;
                return true;
            else if (v.Op == OpZeroExt8to64) 
                v.Op = OpMIPS64MOVBUreg;
                return true;
                        return false;

        }
        private static bool rewriteValueMIPS64_OpAvg64u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Avg64u <t> x y)
            // result: (ADDV (SRLVconst <t> (SUBV <t> x y) [1]) y)
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64ADDV);
                var v0 = b.NewValue0(v.Pos, OpMIPS64SRLVconst, t);
                v0.AuxInt = 1L;
                var v1 = b.NewValue0(v.Pos, OpMIPS64SUBV, t);
                v1.AddArg2(x, y);
                v0.AddArg(v1);
                v.AddArg2(v0, y);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpCom16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Com16 x)
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v_0;
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpCom32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Com32 x)
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v_0;
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpCom64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Com64 x)
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v_0;
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpCom8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Com8 x)
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v_0;
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpConstNil(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;
 
            // match: (ConstNil)
            // result: (MOVVconst [0])
            while (true)
            {
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div16 x y)
            // result: (Select1 (DIVV (SignExt16to64 x) (SignExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv16u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div16u x y)
            // result: (Select1 (DIVVU (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div32 x y)
            // result: (Select1 (DIVV (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv32u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div32u x y)
            // result: (Select1 (DIVVU (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div64 x y)
            // result: (Select1 (DIVV x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv64u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div64u x y)
            // result: (Select1 (DIVVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div8 x y)
            // result: (Select1 (DIVV (SignExt8to64 x) (SignExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpDiv8u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div8u x y)
            // result: (Select1 (DIVVU (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEq16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Eq16 x y)
            // result: (SGTU (MOVVconst [1]) (XOR (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEq32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Eq32 x y)
            // result: (SGTU (MOVVconst [1]) (XOR (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEq32F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Eq32F x y)
            // result: (FPFlagTrue (CMPEQF x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQF, types.TypeFlags);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEq64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Eq64 x y)
            // result: (SGTU (MOVVconst [1]) (XOR x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v1.AddArg2(x, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEq64F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Eq64F x y)
            // result: (FPFlagTrue (CMPEQD x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQD, types.TypeFlags);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEq8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Eq8 x y)
            // result: (SGTU (MOVVconst [1]) (XOR (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEqB(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (EqB x y)
            // result: (XOR (MOVVconst [1]) (XOR <typ.Bool> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.Bool);
                v1.AddArg2(x, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpEqPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (EqPtr x y)
            // result: (SGTU (MOVVconst [1]) (XOR x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v1.AddArg2(x, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpHmul32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Hmul32 x y)
            // result: (SRAVconst (Select1 <typ.Int64> (MULV (SignExt32to64 x) (SignExt32to64 y))) [32])
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = int64ToAuxInt(32L);
                var v0 = b.NewValue0(v.Pos, OpSelect1, typ.Int64);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MULV, types.NewTuple(typ.Int64, typ.Int64));
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpHmul32u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Hmul32u x y)
            // result: (SRLVconst (Select1 <typ.UInt64> (MULVU (ZeroExt32to64 x) (ZeroExt32to64 y))) [32])
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRLVconst);
                v.AuxInt = int64ToAuxInt(32L);
                var v0 = b.NewValue0(v.Pos, OpSelect1, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpHmul64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Hmul64 x y)
            // result: (Select0 (MULV x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULV, types.NewTuple(typ.Int64, typ.Int64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpHmul64u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Hmul64u x y)
            // result: (Select0 (MULVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpIsInBounds(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (IsInBounds idx len)
            // result: (SGTU len idx)
            while (true)
            {
                var idx = v_0;
                var len = v_1;
                v.reset(OpMIPS64SGTU);
                v.AddArg2(len, idx);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpIsNonNil(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (IsNonNil ptr)
            // result: (SGTU ptr (MOVVconst [0]))
            while (true)
            {
                var ptr = v_0;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(ptr, v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpIsSliceInBounds(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (IsSliceInBounds idx len)
            // result: (XOR (MOVVconst [1]) (SGTU idx len))
            while (true)
            {
                var idx = v_0;
                var len = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v1.AddArg2(idx, len);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq16 x y)
            // result: (XOR (MOVVconst [1]) (SGT (SignExt16to64 x) (SignExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq16U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq16U x y)
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq32 x y)
            // result: (XOR (MOVVconst [1]) (SGT (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq32F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Leq32F x y)
            // result: (FPFlagTrue (CMPGEF y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGEF, types.TypeFlags);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq32U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq32U x y)
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq64 x y)
            // result: (XOR (MOVVconst [1]) (SGT x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                v1.AddArg2(x, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq64F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Leq64F x y)
            // result: (FPFlagTrue (CMPGED y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGED, types.TypeFlags);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq64U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq64U x y)
            // result: (XOR (MOVVconst [1]) (SGTU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v1.AddArg2(x, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq8 x y)
            // result: (XOR (MOVVconst [1]) (SGT (SignExt8to64 x) (SignExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLeq8U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq8U x y)
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(1L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(x);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less16 x y)
            // result: (SGT (SignExt16to64 y) (SignExt16to64 x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(x);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess16U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less16U x y)
            // result: (SGTU (ZeroExt16to64 y) (ZeroExt16to64 x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less32 x y)
            // result: (SGT (SignExt32to64 y) (SignExt32to64 x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess32F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Less32F x y)
            // result: (FPFlagTrue (CMPGTF y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGTF, types.TypeFlags);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess32U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less32U x y)
            // result: (SGTU (ZeroExt32to64 y) (ZeroExt32to64 x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Less64 x y)
            // result: (SGT y x)
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGT);
                v.AddArg2(y, x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess64F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Less64F x y)
            // result: (FPFlagTrue (CMPGTD y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGTD, types.TypeFlags);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess64U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Less64U x y)
            // result: (SGTU y x)
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                v.AddArg2(y, x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less8 x y)
            // result: (SGT (SignExt8to64 y) (SignExt8to64 x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(x);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLess8U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less8U x y)
            // result: (SGTU (ZeroExt8to64 y) (ZeroExt8to64 x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLoad(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Load <t> ptr mem)
            // cond: t.IsBoolean()
            // result: (MOVBUload ptr mem)
            while (true)
            {
                var t = v.Type;
                var ptr = v_0;
                var mem = v_1;
                if (!(t.IsBoolean()))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBUload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is8BitInt(t) && isSigned(t))
            // result: (MOVBload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is8BitInt(t) && isSigned(t))
            // result: (MOVBload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is8BitInt(t) && isSigned(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is8BitInt(t) && !isSigned(t))
            // result: (MOVBUload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is8BitInt(t) && !isSigned(t))
            // result: (MOVBUload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is8BitInt(t) && !isSigned(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBUload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is16BitInt(t) && isSigned(t))
            // result: (MOVHload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is16BitInt(t) && isSigned(t))
            // result: (MOVHload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is16BitInt(t) && isSigned(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is16BitInt(t) && !isSigned(t))
            // result: (MOVHUload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is16BitInt(t) && !isSigned(t))
            // result: (MOVHUload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is16BitInt(t) && !isSigned(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHUload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) && isSigned(t))
            // result: (MOVWload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) && isSigned(t))
            // result: (MOVWload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is32BitInt(t) && isSigned(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) && !isSigned(t))
            // result: (MOVWUload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) && !isSigned(t))
            // result: (MOVWUload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is32BitInt(t) && !isSigned(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWUload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is64BitInt(t) || isPtr(t))
            // result: (MOVVload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is64BitInt(t) || isPtr(t))
            // result: (MOVVload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is64BitInt(t) || isPtr(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: is32BitFloat(t)
            // result: (MOVFload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is32BitFloat(t)
            // result: (MOVFload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is32BitFloat(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVFload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: is64BitFloat(t)
            // result: (MOVDload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is64BitFloat(t)
            // result: (MOVDload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is64BitFloat(t)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVDload);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpLocalAddr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (LocalAddr {sym} base _)
            // result: (MOVVaddr {sym} base)
            while (true)
            {
                var sym = v.Aux;
                var @base = v_0;
                v.reset(OpMIPS64MOVVaddr);
                v.Aux = sym;
                v.AddArg(base);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh16x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh16x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh16x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg2(x, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh16x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SLLV <t> x (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh32x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh32x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh32x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg2(x, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh32x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SLLV <t> x (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh64x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh64x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh64x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg2(x, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh64x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SLLV <t> x (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh8x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh8x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh8x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg2(x, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpLsh8x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SLLV <t> x (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMIPS64ADDV(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (ADDV x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (ADDVconst [c] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        var x = v_0;
                        if (v_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var c = auxIntToInt64(v_1.AuxInt);
                        if (!(is32Bit(c)))
                        {
                            continue;
                        }

                        v.reset(OpMIPS64ADDVconst);
                        v.AuxInt = int64ToAuxInt(c);
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (ADDV x (NEGV y))
            // result: (SUBV x y)
 
            // match: (ADDV x (NEGV y))
            // result: (SUBV x y)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        x = v_0;
                        if (v_1.Op != OpMIPS64NEGV)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var y = v_1.Args[0L];
                        v.reset(OpMIPS64SUBV);
                        v.AddArg2(x, y);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64ADDVconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (ADDVconst [off1] (MOVVaddr [off2] {sym} ptr))
            // result: (MOVVaddr [off1+off2] {sym} ptr)
            while (true)
            {
                var off1 = v.AuxInt;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var sym = v_0.Aux;
                var ptr = v_0.Args[0L];
                v.reset(OpMIPS64MOVVaddr);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                return true;

            } 
            // match: (ADDVconst [0] x)
            // result: x
 
            // match: (ADDVconst [0] x)
            // result: x
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                var x = v_0;
                v.copyOf(x);
                return true;

            } 
            // match: (ADDVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c+d])
 
            // match: (ADDVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c+d])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c + d);
                return true;

            } 
            // match: (ADDVconst [c] (ADDVconst [d] x))
            // cond: is32Bit(c+d)
            // result: (ADDVconst [c+d] x)
 
            // match: (ADDVconst [c] (ADDVconst [d] x))
            // cond: is32Bit(c+d)
            // result: (ADDVconst [c+d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }

                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = int64ToAuxInt(c + d);
                v.AddArg(x);
                return true;

            } 
            // match: (ADDVconst [c] (SUBVconst [d] x))
            // cond: is32Bit(c-d)
            // result: (ADDVconst [c-d] x)
 
            // match: (ADDVconst [c] (SUBVconst [d] x))
            // cond: is32Bit(c-d)
            // result: (ADDVconst [c-d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64SUBVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                if (!(is32Bit(c - d)))
                {
                    break;
                }

                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = int64ToAuxInt(c - d);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64AND(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (AND x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (ANDconst [c] x)
            while (true)
            {
                {
                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        var x = v_0;
                        if (v_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var c = auxIntToInt64(v_1.AuxInt);
                        if (!(is32Bit(c)))
                        {
                            continue;
                        }

                        v.reset(OpMIPS64ANDconst);
                        v.AuxInt = int64ToAuxInt(c);
                        v.AddArg(x);
                        return true;

                    }

                }
                break;

            } 
            // match: (AND x x)
            // result: x
 
            // match: (AND x x)
            // result: x
            while (true)
            {
                x = v_0;
                if (x != v_1)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64ANDconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (ANDconst [0] _)
            // result: (MOVVconst [0])
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (ANDconst [-1] x)
            // result: x
 
            // match: (ANDconst [-1] x)
            // result: x
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != -1L)
                {
                    break;
                }

                var x = v_0;
                v.copyOf(x);
                return true;

            } 
            // match: (ANDconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c&d])
 
            // match: (ANDconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c&d])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c & d);
                return true;

            } 
            // match: (ANDconst [c] (ANDconst [d] x))
            // result: (ANDconst [c&d] x)
 
            // match: (ANDconst [c] (ANDconst [d] x))
            // result: (ANDconst [c&d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64ANDconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                v.reset(OpMIPS64ANDconst);
                v.AuxInt = int64ToAuxInt(c & d);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (LoweredAtomicAdd32 ptr (MOVVconst [c]) mem)
            // cond: is32Bit(c)
            // result: (LoweredAtomicAddconst32 [c] ptr mem)
            while (true)
            {
                var ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = v_1.AuxInt;
                var mem = v_2;
                if (!(is32Bit(c)))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredAtomicAddconst32);
                v.AuxInt = c;
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (LoweredAtomicAdd64 ptr (MOVVconst [c]) mem)
            // cond: is32Bit(c)
            // result: (LoweredAtomicAddconst64 [c] ptr mem)
            while (true)
            {
                var ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = v_1.AuxInt;
                var mem = v_2;
                if (!(is32Bit(c)))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredAtomicAddconst64);
                v.AuxInt = c;
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicStore32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (LoweredAtomicStore32 ptr (MOVVconst [0]) mem)
            // result: (LoweredAtomicStorezero32 ptr mem)
            while (true)
            {
                var ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst || v_1.AuxInt != 0L)
                {
                    break;
                }

                var mem = v_2;
                v.reset(OpMIPS64LoweredAtomicStorezero32);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicStore64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (LoweredAtomicStore64 ptr (MOVVconst [0]) mem)
            // result: (LoweredAtomicStorezero64 ptr mem)
            while (true)
            {
                var ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst || v_1.AuxInt != 0L)
                {
                    break;
                }

                var mem = v_2;
                v.reset(OpMIPS64LoweredAtomicStorezero64);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBUload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBUload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVBUload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVBUload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBUreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVBUreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                var x = v_0;
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVBUreg x:(MOVBUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVBUreg x:(MOVBUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVBUreg (MOVVconst [c]))
            // result: (MOVVconst [int64(uint8(c))])
 
            // match: (MOVBUreg (MOVVconst [c]))
            // result: (MOVVconst [int64(uint8(c))])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(uint8(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVBload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVBload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVBreg x:(MOVBload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                var x = v_0;
                if (x.Op != OpMIPS64MOVBload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVBreg x:(MOVBreg _))
            // result: (MOVVreg x)
 
            // match: (MOVBreg x:(MOVBreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVBreg (MOVVconst [c]))
            // result: (MOVVconst [int64(int8(c))])
 
            // match: (MOVBreg (MOVVconst [c]))
            // result: (MOVVconst [int64(int8(c))])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(int8(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVBstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVBstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVBstorezero [off] {sym} ptr mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVBstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpMIPS64MOVBstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVBreg)
                {
                    break;
                }

                var x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVBUreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVBUreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVHreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVHreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVHreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVHUreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVHUreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVWreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVWreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVWreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVWUreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVWUreg x) mem)
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVBstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVBstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVBstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVDload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVDload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVDload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVDload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVDload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVDload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVDload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVDstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVDstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVDstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVDstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVDstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVDstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVDstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg3(ptr, val, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVFload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVFload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVFload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVFload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVFload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVFload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVFload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVFload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVFload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVFstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVFstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVFstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVFstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVFstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVFstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVFstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVFstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVFstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg3(ptr, val, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHUload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHUload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVHUload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVHUload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHUreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVHUreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                var x = v_0;
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHUreg x:(MOVHUload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVHUreg x:(MOVHUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHUreg x:(MOVBUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVHUreg x:(MOVBUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHUreg x:(MOVHUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVHUreg x:(MOVHUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHUreg (MOVVconst [c]))
            // result: (MOVVconst [int64(uint16(c))])
 
            // match: (MOVHUreg (MOVVconst [c]))
            // result: (MOVVconst [int64(uint16(c))])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(uint16(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVHload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVHload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVHreg x:(MOVBload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                var x = v_0;
                if (x.Op != OpMIPS64MOVBload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHreg x:(MOVHload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVHload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHreg x:(MOVBreg _))
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVBreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHreg x:(MOVBUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVBUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHreg x:(MOVHreg _))
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVHreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVHreg (MOVVconst [c]))
            // result: (MOVVconst [int64(int16(c))])
 
            // match: (MOVHreg (MOVVconst [c]))
            // result: (MOVVconst [int64(int16(c))])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(int16(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVHstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVHstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVHstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVHstorezero [off] {sym} ptr mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVHstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpMIPS64MOVHstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVHreg)
                {
                    break;
                }

                var x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVHstore [off] {sym} ptr (MOVHUreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVHUreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVHstore [off] {sym} ptr (MOVWreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVWreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVWreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVHstore [off] {sym} ptr (MOVWUreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVWUreg x) mem)
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVHstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVHstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVHstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVVload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVVload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVVload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVVload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVVload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVVload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVVreg x)
            // cond: x.Uses == 1
            // result: (MOVVnop x)
            while (true)
            {
                var x = v_0;
                if (!(x.Uses == 1L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVnop);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVVreg (MOVVconst [c]))
            // result: (MOVVconst [c])
 
            // match: (MOVVreg (MOVVconst [c]))
            // result: (MOVVconst [c])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVVstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVVstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVVstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVVstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVVstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVVstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVVstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVVstorezero [off] {sym} ptr mem)
 
            // match: (MOVVstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVVstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpMIPS64MOVVstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVVstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVVstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVVstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVVstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVVstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVVstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWUload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWUload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVWUload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVWUload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWUreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVWUreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                var x = v_0;
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWUreg x:(MOVHUload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVHUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWUreg x:(MOVWUload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVWUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVWUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWUreg x:(MOVBUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVBUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWUreg x:(MOVHUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVHUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWUreg x:(MOVWUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVWUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWUreg (MOVVconst [c]))
            // result: (MOVVconst [int64(uint32(c))])
 
            // match: (MOVWUreg (MOVVconst [c]))
            // result: (MOVVconst [int64(uint32(c))])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(uint32(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVWload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVWload [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWreg(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (MOVWreg x:(MOVBload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                var x = v_0;
                if (x.Op != OpMIPS64MOVBload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVBUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVHload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVHUload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHUload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHUload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVWload _ _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVWload _ _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVWload)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVBreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVBreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVBUreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVBUreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVHreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVHreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg x:(MOVWreg _))
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVWreg _))
            // result: (MOVVreg x)
            while (true)
            {
                x = v_0;
                if (x.Op != OpMIPS64MOVWreg)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;

            } 
            // match: (MOVWreg (MOVVconst [c]))
            // result: (MOVVconst [int64(int32(c))])
 
            // match: (MOVWreg (MOVVconst [c]))
            // result: (MOVVconst [int64(int32(c))])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(int32(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVWstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVWstore [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (MOVWstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVWstorezero [off] {sym} ptr mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVVconst [0]) mem)
            // result: (MOVWstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpMIPS64MOVWstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
            // result: (MOVWstore [off] {sym} ptr x mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
            // result: (MOVWstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVWreg)
                {
                    break;
                }

                var x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWUreg x) mem)
            // result: (MOVWstore [off] {sym} ptr x mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWUreg x) mem)
            // result: (MOVWstore [off] {sym} ptr x mem)
            while (true)
            {
                off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                ptr = v_0;
                if (v_1.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }

                x = v_1.Args[0L];
                mem = v_2;
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVWstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVWstorezero [off1] {sym1} (MOVVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(off1+off2)
            // result: (MOVWstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }

                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64NEGV(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (NEGV (MOVVconst [c]))
            // result: (MOVVconst [-c])
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(-c);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64NOR(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (NOR x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (NORconst [c] x)
            while (true)
            {
                {
                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        var x = v_0;
                        if (v_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var c = auxIntToInt64(v_1.AuxInt);
                        if (!(is32Bit(c)))
                        {
                            continue;
                        }

                        v.reset(OpMIPS64NORconst);
                        v.AuxInt = int64ToAuxInt(c);
                        v.AddArg(x);
                        return true;

                    }

                }
                break;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64NORconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (NORconst [c] (MOVVconst [d]))
            // result: (MOVVconst [^(c|d)])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(~(c | d));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64OR(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (OR x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (ORconst [c] x)
            while (true)
            {
                {
                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        var x = v_0;
                        if (v_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var c = auxIntToInt64(v_1.AuxInt);
                        if (!(is32Bit(c)))
                        {
                            continue;
                        }

                        v.reset(OpMIPS64ORconst);
                        v.AuxInt = int64ToAuxInt(c);
                        v.AddArg(x);
                        return true;

                    }

                }
                break;

            } 
            // match: (OR x x)
            // result: x
 
            // match: (OR x x)
            // result: x
            while (true)
            {
                x = v_0;
                if (x != v_1)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64ORconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (ORconst [0] x)
            // result: x
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                var x = v_0;
                v.copyOf(x);
                return true;

            } 
            // match: (ORconst [-1] _)
            // result: (MOVVconst [-1])
 
            // match: (ORconst [-1] _)
            // result: (MOVVconst [-1])
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != -1L)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(-1L);
                return true;

            } 
            // match: (ORconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c|d])
 
            // match: (ORconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c|d])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c | d);
                return true;

            } 
            // match: (ORconst [c] (ORconst [d] x))
            // cond: is32Bit(c|d)
            // result: (ORconst [c|d] x)
 
            // match: (ORconst [c] (ORconst [d] x))
            // cond: is32Bit(c|d)
            // result: (ORconst [c|d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64ORconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                if (!(is32Bit(c | d)))
                {
                    break;
                }

                v.reset(OpMIPS64ORconst);
                v.AuxInt = int64ToAuxInt(c | d);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SGT(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SGT (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (SGTconst [c] x)
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                var x = v_1;
                if (!(is32Bit(c)))
                {
                    break;
                }

                v.reset(OpMIPS64SGTconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTU(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SGTU (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (SGTUconst [c] x)
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0.AuxInt);
                var x = v_1;
                if (!(is32Bit(c)))
                {
                    break;
                }

                v.reset(OpMIPS64SGTUconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTUconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SGTUconst [c] (MOVVconst [d]))
            // cond: uint64(c)>uint64(d)
            // result: (MOVVconst [1])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                if (!(uint64(c) > uint64(d)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTUconst [c] (MOVVconst [d]))
            // cond: uint64(c)<=uint64(d)
            // result: (MOVVconst [0])
 
            // match: (SGTUconst [c] (MOVVconst [d]))
            // cond: uint64(c)<=uint64(d)
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                if (!(uint64(c) <= uint64(d)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTUconst [c] (MOVBUreg _))
            // cond: 0xff < uint64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTUconst [c] (MOVBUreg _))
            // cond: 0xff < uint64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVBUreg || !(0xffUL < uint64(c)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTUconst [c] (MOVHUreg _))
            // cond: 0xffff < uint64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTUconst [c] (MOVHUreg _))
            // cond: 0xffff < uint64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVHUreg || !(0xffffUL < uint64(c)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTUconst [c] (ANDconst [m] _))
            // cond: uint64(m) < uint64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTUconst [c] (ANDconst [m] _))
            // cond: uint64(m) < uint64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64ANDconst)
                {
                    break;
                }

                var m = auxIntToInt64(v_0.AuxInt);
                if (!(uint64(m) < uint64(c)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTUconst [c] (SRLVconst _ [d]))
            // cond: 0 < d && d <= 63 && 0xffffffffffffffff>>uint64(d) < uint64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTUconst [c] (SRLVconst _ [d]))
            // cond: 0 < d && d <= 63 && 0xffffffffffffffff>>uint64(d) < uint64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64SRLVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                if (!(0L < d && d <= 63L && 0xffffffffffffffffUL >> (int)(uint64(d)) < uint64(c)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SGTconst [c] (MOVVconst [d]))
            // cond: c>d
            // result: (MOVVconst [1])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                if (!(c > d))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTconst [c] (MOVVconst [d]))
            // cond: c<=d
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVVconst [d]))
            // cond: c<=d
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                if (!(c <= d))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: 0x7f < c
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: 0x7f < c
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVBreg || !(0x7fUL < c))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: c <= -0x80
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: c <= -0x80
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVBreg || !(c <= -0x80UL))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: 0xff < c
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: 0xff < c
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVBUreg || !(0xffUL < c))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: c < 0
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: c < 0
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVBUreg || !(c < 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: 0x7fff < c
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: 0x7fff < c
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVHreg || !(0x7fffUL < c))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: c <= -0x8000
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: c <= -0x8000
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVHreg || !(c <= -0x8000UL))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: 0xffff < c
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: 0xffff < c
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVHUreg || !(0xffffUL < c))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: c < 0
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: c < 0
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVHUreg || !(c < 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTconst [c] (MOVWUreg _))
            // cond: c < 0
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVWUreg _))
            // cond: c < 0
            // result: (MOVVconst [0])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVWUreg || !(c < 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SGTconst [c] (ANDconst [m] _))
            // cond: 0 <= m && m < c
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (ANDconst [m] _))
            // cond: 0 <= m && m < c
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64ANDconst)
                {
                    break;
                }

                var m = auxIntToInt64(v_0.AuxInt);
                if (!(0L <= m && m < c))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            } 
            // match: (SGTconst [c] (SRLVconst _ [d]))
            // cond: 0 <= c && 0 < d && d <= 63 && 0xffffffffffffffff>>uint64(d) < uint64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (SRLVconst _ [d]))
            // cond: 0 <= c && 0 < d && d <= 63 && 0xffffffffffffffff>>uint64(d) < uint64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64SRLVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                if (!(0L <= c && 0L < d && d <= 63L && 0xffffffffffffffffUL >> (int)(uint64(d)) < uint64(c)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(1L);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SLLV(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SLLV _ (MOVVconst [c]))
            // cond: uint64(c)>=64
            // result: (MOVVconst [0])
            while (true)
            {
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                if (!(uint64(c) >= 64L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SLLV x (MOVVconst [c]))
            // result: (SLLVconst x [c])
 
            // match: (SLLV x (MOVVconst [c]))
            // result: (SLLVconst x [c])
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpMIPS64SLLVconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SLLVconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SLLVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [d<<uint64(c)])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(d << (int)(uint64(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SRAV(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SRAV x (MOVVconst [c]))
            // cond: uint64(c)>=64
            // result: (SRAVconst x [63])
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                if (!(uint64(c) >= 64L))
                {
                    break;
                }

                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = int64ToAuxInt(63L);
                v.AddArg(x);
                return true;

            } 
            // match: (SRAV x (MOVVconst [c]))
            // result: (SRAVconst x [c])
 
            // match: (SRAV x (MOVVconst [c]))
            // result: (SRAVconst x [c])
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SRAVconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SRAVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [d>>uint64(c)])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(d >> (int)(uint64(c)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SRLV(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SRLV _ (MOVVconst [c]))
            // cond: uint64(c)>=64
            // result: (MOVVconst [0])
            while (true)
            {
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                if (!(uint64(c) >= 64L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SRLV x (MOVVconst [c]))
            // result: (SRLVconst x [c])
 
            // match: (SRLV x (MOVVconst [c]))
            // result: (SRLVconst x [c])
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpMIPS64SRLVconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SRLVconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SRLVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [int64(uint64(d)>>uint64(c))])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(uint64(d) >> (int)(uint64(c))));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SUBV(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SUBV x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (SUBVconst [c] x)
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(c)))
                {
                    break;
                }

                v.reset(OpMIPS64SUBVconst);
                v.AuxInt = int64ToAuxInt(c);
                v.AddArg(x);
                return true;

            } 
            // match: (SUBV x x)
            // result: (MOVVconst [0])
 
            // match: (SUBV x x)
            // result: (MOVVconst [0])
            while (true)
            {
                x = v_0;
                if (x != v_1)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (SUBV (MOVVconst [0]) x)
            // result: (NEGV x)
 
            // match: (SUBV (MOVVconst [0]) x)
            // result: (NEGV x)
            while (true)
            {
                if (v_0.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0.AuxInt) != 0L)
                {
                    break;
                }

                x = v_1;
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64SUBVconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SUBVconst [0] x)
            // result: x
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                var x = v_0;
                v.copyOf(x);
                return true;

            } 
            // match: (SUBVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [d-c])
 
            // match: (SUBVconst [c] (MOVVconst [d]))
            // result: (MOVVconst [d-c])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(d - c);
                return true;

            } 
            // match: (SUBVconst [c] (SUBVconst [d] x))
            // cond: is32Bit(-c-d)
            // result: (ADDVconst [-c-d] x)
 
            // match: (SUBVconst [c] (SUBVconst [d] x))
            // cond: is32Bit(-c-d)
            // result: (ADDVconst [-c-d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64SUBVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                if (!(is32Bit(-c - d)))
                {
                    break;
                }

                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = int64ToAuxInt(-c - d);
                v.AddArg(x);
                return true;

            } 
            // match: (SUBVconst [c] (ADDVconst [d] x))
            // cond: is32Bit(-c+d)
            // result: (ADDVconst [-c+d] x)
 
            // match: (SUBVconst [c] (ADDVconst [d] x))
            // cond: is32Bit(-c+d)
            // result: (ADDVconst [-c+d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                if (!(is32Bit(-c + d)))
                {
                    break;
                }

                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = int64ToAuxInt(-c + d);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64XOR(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (XOR x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (XORconst [c] x)
            while (true)
            {
                {
                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        var x = v_0;
                        if (v_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var c = auxIntToInt64(v_1.AuxInt);
                        if (!(is32Bit(c)))
                        {
                            continue;
                        }

                        v.reset(OpMIPS64XORconst);
                        v.AuxInt = int64ToAuxInt(c);
                        v.AddArg(x);
                        return true;

                    }

                }
                break;

            } 
            // match: (XOR x x)
            // result: (MOVVconst [0])
 
            // match: (XOR x x)
            // result: (MOVVconst [0])
            while (true)
            {
                x = v_0;
                if (x != v_1)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMIPS64XORconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (XORconst [0] x)
            // result: x
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                var x = v_0;
                v.copyOf(x);
                return true;

            } 
            // match: (XORconst [-1] x)
            // result: (NORconst [0] x)
 
            // match: (XORconst [-1] x)
            // result: (NORconst [0] x)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != -1L)
                {
                    break;
                }

                x = v_0;
                v.reset(OpMIPS64NORconst);
                v.AuxInt = int64ToAuxInt(0L);
                v.AddArg(x);
                return true;

            } 
            // match: (XORconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c^d])
 
            // match: (XORconst [c] (MOVVconst [d]))
            // result: (MOVVconst [c^d])
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c ^ d);
                return true;

            } 
            // match: (XORconst [c] (XORconst [d] x))
            // cond: is32Bit(c^d)
            // result: (XORconst [c^d] x)
 
            // match: (XORconst [c] (XORconst [d] x))
            // cond: is32Bit(c^d)
            // result: (XORconst [c^d] x)
            while (true)
            {
                c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpMIPS64XORconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0.AuxInt);
                x = v_0.Args[0L];
                if (!(is32Bit(c ^ d)))
                {
                    break;
                }

                v.reset(OpMIPS64XORconst);
                v.AuxInt = int64ToAuxInt(c ^ d);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMod16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod16 x y)
            // result: (Select0 (DIVV (SignExt16to64 x) (SignExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod16u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod16u x y)
            // result: (Select0 (DIVVU (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod32 x y)
            // result: (Select0 (DIVV (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod32u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod32u x y)
            // result: (Select0 (DIVVU (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod64 x y)
            // result: (Select0 (DIVV x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod64u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod64u x y)
            // result: (Select0 (DIVVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod8 x y)
            // result: (Select0 (DIVV (SignExt8to64 x) (SignExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMod8u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod8u x y)
            // result: (Select0 (DIVVU (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMove(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var config = b.Func.Config;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Move [0] _ _ mem)
            // result: mem
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                var mem = v_2;
                v.copyOf(mem);
                return true;

            } 
            // match: (Move [1] dst src mem)
            // result: (MOVBstore dst (MOVBload src mem) mem)
 
            // match: (Move [1] dst src mem)
            // result: (MOVBstore dst (MOVBload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 1L)
                {
                    break;
                }

                var dst = v_0;
                var src = v_1;
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [2] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore dst (MOVHload src mem) mem)
 
            // match: (Move [2] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore dst (MOVHload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 2L)
                {
                    break;
                }

                var t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [2] dst src mem)
            // result: (MOVBstore [1] dst (MOVBload [1] src mem) (MOVBstore dst (MOVBload src mem) mem))
 
            // match: (Move [2] dst src mem)
            // result: (MOVBstore [1] dst (MOVBload [1] src mem) (MOVBstore dst (MOVBload src mem) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 2L)
                {
                    break;
                }

                dst = v_0;
                src = v_1;
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(1L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AuxInt = int32ToAuxInt(1L);
                v0.AddArg2(src, mem);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v2.AddArg2(src, mem);
                v1.AddArg3(dst, v2, mem);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [4] {t} dst src mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore dst (MOVWload src mem) mem)
 
            // match: (Move [4] {t} dst src mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore dst (MOVWload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 4L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [4] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem))
 
            // match: (Move [4] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(2L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AuxInt = int32ToAuxInt(2L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v2.AddArg2(src, mem);
                v1.AddArg3(dst, v2, mem);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [4] dst src mem)
            // result: (MOVBstore [3] dst (MOVBload [3] src mem) (MOVBstore [2] dst (MOVBload [2] src mem) (MOVBstore [1] dst (MOVBload [1] src mem) (MOVBstore dst (MOVBload src mem) mem))))
 
            // match: (Move [4] dst src mem)
            // result: (MOVBstore [3] dst (MOVBload [3] src mem) (MOVBstore [2] dst (MOVBload [2] src mem) (MOVBstore [1] dst (MOVBload [1] src mem) (MOVBstore dst (MOVBload src mem) mem))))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                dst = v_0;
                src = v_1;
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(3L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AuxInt = int32ToAuxInt(3L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(2L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v2.AuxInt = int32ToAuxInt(2L);
                v2.AddArg2(src, mem);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v3.AuxInt = int32ToAuxInt(1L);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v4.AuxInt = int32ToAuxInt(1L);
                v4.AddArg2(src, mem);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                var v6 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v6.AddArg2(src, mem);
                v5.AddArg3(dst, v6, mem);
                v3.AddArg3(dst, v4, v5);
                v1.AddArg3(dst, v2, v3);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [8] {t} dst src mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore dst (MOVVload src mem) mem)
 
            // match: (Move [8] {t} dst src mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore dst (MOVVload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 8L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [8] {t} dst src mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem))
 
            // match: (Move [8] {t} dst src mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 4L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = int32ToAuxInt(4L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v0.AuxInt = int32ToAuxInt(4L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v2.AddArg2(src, mem);
                v1.AddArg3(dst, v2, mem);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [8] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [6] dst (MOVHload [6] src mem) (MOVHstore [4] dst (MOVHload [4] src mem) (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem))))
 
            // match: (Move [8] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [6] dst (MOVHload [6] src mem) (MOVHstore [4] dst (MOVHload [4] src mem) (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem))))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(6L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AuxInt = int32ToAuxInt(6L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(4L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v2.AuxInt = int32ToAuxInt(4L);
                v2.AddArg2(src, mem);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v3.AuxInt = int32ToAuxInt(2L);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v4.AuxInt = int32ToAuxInt(2L);
                v4.AddArg2(src, mem);
                v5 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v6 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v6.AddArg2(src, mem);
                v5.AddArg3(dst, v6, mem);
                v3.AddArg3(dst, v4, v5);
                v1.AddArg3(dst, v2, v3);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [3] dst src mem)
            // result: (MOVBstore [2] dst (MOVBload [2] src mem) (MOVBstore [1] dst (MOVBload [1] src mem) (MOVBstore dst (MOVBload src mem) mem)))
 
            // match: (Move [3] dst src mem)
            // result: (MOVBstore [2] dst (MOVBload [2] src mem) (MOVBstore [1] dst (MOVBload [1] src mem) (MOVBstore dst (MOVBload src mem) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 3L)
                {
                    break;
                }

                dst = v_0;
                src = v_1;
                mem = v_2;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(2L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AuxInt = int32ToAuxInt(2L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(1L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v2.AuxInt = int32ToAuxInt(1L);
                v2.AddArg2(src, mem);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v4.AddArg2(src, mem);
                v3.AddArg3(dst, v4, mem);
                v1.AddArg3(dst, v2, v3);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [6] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [4] dst (MOVHload [4] src mem) (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem)))
 
            // match: (Move [6] {t} dst src mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [4] dst (MOVHload [4] src mem) (MOVHstore [2] dst (MOVHload [2] src mem) (MOVHstore dst (MOVHload src mem) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 6L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(4L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AuxInt = int32ToAuxInt(4L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(2L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v2.AuxInt = int32ToAuxInt(2L);
                v2.AddArg2(src, mem);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v4.AddArg2(src, mem);
                v3.AddArg3(dst, v4, mem);
                v1.AddArg3(dst, v2, v3);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [12] {t} dst src mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [8] dst (MOVWload [8] src mem) (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem)))
 
            // match: (Move [12] {t} dst src mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [8] dst (MOVWload [8] src mem) (MOVWstore [4] dst (MOVWload [4] src mem) (MOVWstore dst (MOVWload src mem) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 12L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 4L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = int32ToAuxInt(8L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v0.AuxInt = int32ToAuxInt(8L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(4L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v2.AuxInt = int32ToAuxInt(4L);
                v2.AddArg2(src, mem);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v4.AddArg2(src, mem);
                v3.AddArg3(dst, v4, mem);
                v1.AddArg3(dst, v2, v3);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [16] {t} dst src mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [8] dst (MOVVload [8] src mem) (MOVVstore dst (MOVVload src mem) mem))
 
            // match: (Move [16] {t} dst src mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [8] dst (MOVVload [8] src mem) (MOVVstore dst (MOVVload src mem) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 16L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 8L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = int32ToAuxInt(8L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v0.AuxInt = int32ToAuxInt(8L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v2.AddArg2(src, mem);
                v1.AddArg3(dst, v2, mem);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [24] {t} dst src mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [16] dst (MOVVload [16] src mem) (MOVVstore [8] dst (MOVVload [8] src mem) (MOVVstore dst (MOVVload src mem) mem)))
 
            // match: (Move [24] {t} dst src mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [16] dst (MOVVload [16] src mem) (MOVVstore [8] dst (MOVVload [8] src mem) (MOVVstore dst (MOVVload src mem) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 24L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(t.Alignment() % 8L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = int32ToAuxInt(16L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v0.AuxInt = int32ToAuxInt(16L);
                v0.AddArg2(src, mem);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(8L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v2.AuxInt = int32ToAuxInt(8L);
                v2.AddArg2(src, mem);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v4.AddArg2(src, mem);
                v3.AddArg3(dst, v4, mem);
                v1.AddArg3(dst, v2, v3);
                v.AddArg3(dst, v0, v1);
                return true;

            } 
            // match: (Move [s] {t} dst src mem)
            // cond: s%8 == 0 && s >= 24 && s <= 8*128 && t.Alignment()%8 == 0 && !config.noDuffDevice && logLargeCopy(v, s)
            // result: (DUFFCOPY [16 * (128 - s/8)] dst src mem)
 
            // match: (Move [s] {t} dst src mem)
            // cond: s%8 == 0 && s >= 24 && s <= 8*128 && t.Alignment()%8 == 0 && !config.noDuffDevice && logLargeCopy(v, s)
            // result: (DUFFCOPY [16 * (128 - s/8)] dst src mem)
            while (true)
            {
                var s = auxIntToInt64(v.AuxInt);
                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(s % 8L == 0L && s >= 24L && s <= 8L * 128L && t.Alignment() % 8L == 0L && !config.noDuffDevice && logLargeCopy(v, s)))
                {
                    break;
                }

                v.reset(OpMIPS64DUFFCOPY);
                v.AuxInt = int64ToAuxInt(16L * (128L - s / 8L));
                v.AddArg3(dst, src, mem);
                return true;

            } 
            // match: (Move [s] {t} dst src mem)
            // cond: s > 24 && logLargeCopy(v, s) || t.Alignment()%8 != 0
            // result: (LoweredMove [t.Alignment()] dst src (ADDVconst <src.Type> src [s-moveSize(t.Alignment(), config)]) mem)
 
            // match: (Move [s] {t} dst src mem)
            // cond: s > 24 && logLargeCopy(v, s) || t.Alignment()%8 != 0
            // result: (LoweredMove [t.Alignment()] dst src (ADDVconst <src.Type> src [s-moveSize(t.Alignment(), config)]) mem)
            while (true)
            {
                s = auxIntToInt64(v.AuxInt);
                t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(s > 24L && logLargeCopy(v, s) || t.Alignment() % 8L != 0L))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredMove);
                v.AuxInt = int64ToAuxInt(t.Alignment());
                v0 = b.NewValue0(v.Pos, OpMIPS64ADDVconst, src.Type);
                v0.AuxInt = int64ToAuxInt(s - moveSize(t.Alignment(), config));
                v0.AddArg(src);
                v.AddArg4(dst, src, v0, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpMul16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mul16 x y)
            // result: (Select1 (MULVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMul32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mul32 x y)
            // result: (Select1 (MULVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMul64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mul64 x y)
            // result: (Select1 (MULVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpMul8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mul8 x y)
            // result: (Select1 (MULVU x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeq16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Neq16 x y)
            // result: (SGTU (XOR (ZeroExt16to32 x) (ZeroExt16to64 y)) (MOVVconst [0]))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeq32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Neq32 x y)
            // result: (SGTU (XOR (ZeroExt32to64 x) (ZeroExt32to64 y)) (MOVVconst [0]))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeq32F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Neq32F x y)
            // result: (FPFlagFalse (CMPEQF x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagFalse);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQF, types.TypeFlags);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeq64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Neq64 x y)
            // result: (SGTU (XOR x y) (MOVVconst [0]))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeq64F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Neq64F x y)
            // result: (FPFlagFalse (CMPEQD x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64FPFlagFalse);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQD, types.TypeFlags);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeq8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Neq8 x y)
            // result: (SGTU (XOR (ZeroExt8to64 x) (ZeroExt8to64 y)) (MOVVconst [0]))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNeqPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (NeqPtr x y)
            // result: (SGTU (XOR x y) (MOVVconst [0]))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(0L);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpNot(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (Not x)
            // result: (XORconst [1] x)
            while (true)
            {
                var x = v_0;
                v.reset(OpMIPS64XORconst);
                v.AuxInt = int64ToAuxInt(1L);
                v.AddArg(x);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpOffPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (OffPtr [off] ptr:(SP))
            // result: (MOVVaddr [off] ptr)
            while (true)
            {
                var off = v.AuxInt;
                var ptr = v_0;
                if (ptr.Op != OpSP)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVaddr);
                v.AuxInt = off;
                v.AddArg(ptr);
                return true;

            } 
            // match: (OffPtr [off] ptr)
            // result: (ADDVconst [off] ptr)
 
            // match: (OffPtr [off] ptr)
            // result: (ADDVconst [off] ptr)
            while (true)
            {
                off = v.AuxInt;
                ptr = v_0;
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = off;
                v.AddArg(ptr);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpPanicBounds(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (PanicBounds [kind] x y mem)
            // cond: boundsABI(kind) == 0
            // result: (LoweredPanicBoundsA [kind] x y mem)
            while (true)
            {
                var kind = auxIntToInt64(v.AuxInt);
                var x = v_0;
                var y = v_1;
                var mem = v_2;
                if (!(boundsABI(kind) == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredPanicBoundsA);
                v.AuxInt = int64ToAuxInt(kind);
                v.AddArg3(x, y, mem);
                return true;

            } 
            // match: (PanicBounds [kind] x y mem)
            // cond: boundsABI(kind) == 1
            // result: (LoweredPanicBoundsB [kind] x y mem)
 
            // match: (PanicBounds [kind] x y mem)
            // cond: boundsABI(kind) == 1
            // result: (LoweredPanicBoundsB [kind] x y mem)
            while (true)
            {
                kind = auxIntToInt64(v.AuxInt);
                x = v_0;
                y = v_1;
                mem = v_2;
                if (!(boundsABI(kind) == 1L))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredPanicBoundsB);
                v.AuxInt = int64ToAuxInt(kind);
                v.AddArg3(x, y, mem);
                return true;

            } 
            // match: (PanicBounds [kind] x y mem)
            // cond: boundsABI(kind) == 2
            // result: (LoweredPanicBoundsC [kind] x y mem)
 
            // match: (PanicBounds [kind] x y mem)
            // cond: boundsABI(kind) == 2
            // result: (LoweredPanicBoundsC [kind] x y mem)
            while (true)
            {
                kind = auxIntToInt64(v.AuxInt);
                x = v_0;
                y = v_1;
                mem = v_2;
                if (!(boundsABI(kind) == 2L))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredPanicBoundsC);
                v.AuxInt = int64ToAuxInt(kind);
                v.AddArg3(x, y, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpRotateLeft16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft16 <t> x (MOVVconst [c]))
            // result: (Or16 (Lsh16x64 <t> x (MOVVconst [c&15])) (Rsh16Ux64 <t> x (MOVVconst [-c&15])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpOr16);
                var v0 = b.NewValue0(v.Pos, OpLsh16x64, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c & 15L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh16Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(-c & 15L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpRotateLeft32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft32 <t> x (MOVVconst [c]))
            // result: (Or32 (Lsh32x64 <t> x (MOVVconst [c&31])) (Rsh32Ux64 <t> x (MOVVconst [-c&31])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpOr32);
                var v0 = b.NewValue0(v.Pos, OpLsh32x64, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c & 31L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh32Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(-c & 31L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpRotateLeft64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft64 <t> x (MOVVconst [c]))
            // result: (Or64 (Lsh64x64 <t> x (MOVVconst [c&63])) (Rsh64Ux64 <t> x (MOVVconst [-c&63])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpOr64);
                var v0 = b.NewValue0(v.Pos, OpLsh64x64, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c & 63L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh64Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(-c & 63L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpRotateLeft8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft8 <t> x (MOVVconst [c]))
            // result: (Or8 (Lsh8x64 <t> x (MOVVconst [c&7])) (Rsh8Ux64 <t> x (MOVVconst [-c&7])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpOr8);
                var v0 = b.NewValue0(v.Pos, OpLsh8x64, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c & 7L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh8Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(-c & 7L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpRsh16Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> (ZeroExt16to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> (ZeroExt16to64 x) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(x);
                v3.AddArg2(v4, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SRLV <t> (ZeroExt16to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x16 <t> x y)
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x32 <t> x y)
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x64 <t> x y)
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(y, v4);
                v2.AddArg(v3);
                v1.AddArg2(v2, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh16x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x8 <t> x y)
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt8to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> (ZeroExt32to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> (ZeroExt32to64 x) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(x);
                v3.AddArg2(v4, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SRLV <t> (ZeroExt32to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x16 <t> x y)
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x32 <t> x y)
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x64 <t> x y)
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(y, v4);
                v2.AddArg(v3);
                v1.AddArg2(v2, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh32x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x8 <t> x y)
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt8to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> x y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v3.AddArg2(x, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SRLV <t> x (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v4.AddArg2(x, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x16 <t> x y)
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = int64ToAuxInt(63L);
                v2.AddArg2(v3, v4);
                v1.AddArg(v2);
                v0.AddArg2(v1, v3);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x32 <t> x y)
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = int64ToAuxInt(63L);
                v2.AddArg2(v3, v4);
                v1.AddArg(v2);
                v0.AddArg2(v1, v3);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x64 <t> x y)
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(63L);
                v2.AddArg2(y, v3);
                v1.AddArg(v2);
                v0.AddArg2(v1, y);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh64x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x8 <t> x y)
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU (ZeroExt8to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = int64ToAuxInt(63L);
                v2.AddArg2(v3, v4);
                v1.AddArg(v2);
                v0.AddArg2(v1, v3);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux16 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> (ZeroExt8to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux32 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> (ZeroExt8to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux64 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> (ZeroExt8to64 x) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                v1.AddArg2(v2, y);
                v0.AddArg(v1);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(x);
                v3.AddArg2(v4, y);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux8 <t> x y)
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64 y))) (SRLV <t> (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg2(v2, v3);
                v0.AddArg(v1);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg2(v5, v3);
                v.AddArg2(v0, v4);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x16 <t> x y)
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x32 <t> x y)
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x64 <t> x y)
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(y, v4);
                v2.AddArg(v3);
                v1.AddArg2(v2, y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpRsh8x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x8 <t> x y)
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt8to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64 y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = int64ToAuxInt(63L);
                v3.AddArg2(v4, v5);
                v2.AddArg(v3);
                v1.AddArg2(v2, v4);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpSelect0(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Select0 (Mul64uover x y))
            // result: (Select1 <typ.UInt64> (MULVU x y))
            while (true)
            {
                if (v_0.Op != OpMul64uover)
                {
                    break;
                }

                var y = v_0.Args[1L];
                var x = v_0.Args[0L];
                v.reset(OpSelect1);
                v.Type = typ.UInt64;
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;

            } 
            // match: (Select0 (DIVVU _ (MOVVconst [1])))
            // result: (MOVVconst [0])
 
            // match: (Select0 (DIVVU _ (MOVVconst [1])))
            // result: (MOVVconst [0])
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 1L)
                {
                    break;
                }

                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;

            } 
            // match: (Select0 (DIVVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (ANDconst [c-1] x)
 
            // match: (Select0 (DIVVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (ANDconst [c-1] x)
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_0_1.AuxInt);
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }

                v.reset(OpMIPS64ANDconst);
                v.AuxInt = int64ToAuxInt(c - 1L);
                v.AddArg(x);
                return true;

            } 
            // match: (Select0 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [c%d])
 
            // match: (Select0 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [c%d])
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVV)
                {
                    break;
                }

                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                var d = auxIntToInt64(v_0_1.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c % d);
                return true;

            } 
            // match: (Select0 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [int64(uint64(c)%uint64(d))])
 
            // match: (Select0 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [int64(uint64(c)%uint64(d))])
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0_1.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(uint64(c) % uint64(d)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpSelect1(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Select1 (Mul64uover x y))
            // result: (SGTU <typ.Bool> (Select0 <typ.UInt64> (MULVU x y)) (MOVVconst <typ.UInt64> [0]))
            while (true)
            {
                if (v_0.Op != OpMul64uover)
                {
                    break;
                }

                var y = v_0.Args[1L];
                var x = v_0.Args[0L];
                v.reset(OpMIPS64SGTU);
                v.Type = typ.Bool;
                var v0 = b.NewValue0(v.Pos, OpSelect0, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v1.AddArg2(x, y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v.AddArg2(v0, v2);
                return true;

            } 
            // match: (Select1 (MULVU x (MOVVconst [-1])))
            // result: (NEGV x)
 
            // match: (Select1 (MULVU x (MOVVconst [-1])))
            // result: (NEGV x)
            while (true)
            {
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                var v_0_1 = v_0.Args[1L];
                {
                    long _i0__prev2 = _i0;

                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        x = v_0_0;
                        if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != -1L)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0_0 = v_0_1;
                    v_0_1 = v_0_0;
                        }

                        v.reset(OpMIPS64NEGV);
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (Select1 (MULVU _ (MOVVconst [0])))
            // result: (MOVVconst [0])
 
            // match: (Select1 (MULVU _ (MOVVconst [0])))
            // result: (MOVVconst [0])
            while (true)
            {
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 0L)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0_0 = v_0_1;
                    v_0_1 = v_0_0;
                        }

                        v.reset(OpMIPS64MOVVconst);
                        v.AuxInt = int64ToAuxInt(0L);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (Select1 (MULVU x (MOVVconst [1])))
            // result: x
 
            // match: (Select1 (MULVU x (MOVVconst [1])))
            // result: x
            while (true)
            {
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        x = v_0_0;
                        if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 1L)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0_0 = v_0_1;
                    v_0_1 = v_0_0;
                        }

                        v.copyOf(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (Select1 (MULVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
 
            // match: (Select1 (MULVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
            while (true)
            {
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        x = v_0_0;
                        if (v_0_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0_0 = v_0_1;
                    v_0_1 = v_0_0;
                        }

                        var c = auxIntToInt64(v_0_1.AuxInt);
                        if (!(isPowerOfTwo(c)))
                        {
                            continue;
                        }

                        v.reset(OpMIPS64SLLVconst);
                        v.AuxInt = int64ToAuxInt(log2(c));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (Select1 (DIVVU x (MOVVconst [1])))
            // result: x
 
            // match: (Select1 (DIVVU x (MOVVconst [1])))
            // result: x
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 1L)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            } 
            // match: (Select1 (DIVVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SRLVconst [log2(c)] x)
 
            // match: (Select1 (DIVVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SRLVconst [log2(c)] x)
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_0_1.AuxInt);
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }

                v.reset(OpMIPS64SRLVconst);
                v.AuxInt = int64ToAuxInt(log2(c));
                v.AddArg(x);
                return true;

            } 
            // match: (Select1 (MULVU (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [c*d])
 
            // match: (Select1 (MULVU (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [c*d])
            while (true)
            {
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0_0.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0_0 = v_0_1;
                    v_0_1 = v_0_0;
                        }

                        c = auxIntToInt64(v_0_0.AuxInt);
                        if (v_0_1.Op != OpMIPS64MOVVconst)
                        {
                            continue;
                        }

                        var d = auxIntToInt64(v_0_1.AuxInt);
                        v.reset(OpMIPS64MOVVconst);
                        v.AuxInt = int64ToAuxInt(c * d);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (Select1 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [c/d])
 
            // match: (Select1 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [c/d])
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVV)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0_1.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(c / d);
                return true;

            } 
            // match: (Select1 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [int64(uint64(c)/uint64(d))])
 
            // match: (Select1 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // result: (MOVVconst [int64(uint64(c)/uint64(d))])
            while (true)
            {
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }

                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                c = auxIntToInt64(v_0_0.AuxInt);
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }

                d = auxIntToInt64(v_0_1.AuxInt);
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64ToAuxInt(int64(uint64(c) / uint64(d)));
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpSlicemask(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Slicemask <t> x)
            // result: (SRAVconst (NEGV <t> x) [63])
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = int64ToAuxInt(63L);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueMIPS64_OpStore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 1
            // result: (MOVBstore ptr val mem)
            while (true)
            {
                var t = auxToType(v.Aux);
                var ptr = v_0;
                var val = v_1;
                var mem = v_2;
                if (!(t.Size() == 1L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVBstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 2
            // result: (MOVHstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 2
            // result: (MOVHstore ptr val mem)
            while (true)
            {
                t = auxToType(v.Aux);
                ptr = v_0;
                val = v_1;
                mem = v_2;
                if (!(t.Size() == 2L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 4 && !is32BitFloat(val.Type)
            // result: (MOVWstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 4 && !is32BitFloat(val.Type)
            // result: (MOVWstore ptr val mem)
            while (true)
            {
                t = auxToType(v.Aux);
                ptr = v_0;
                val = v_1;
                mem = v_2;
                if (!(t.Size() == 4L && !is32BitFloat(val.Type)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && !is64BitFloat(val.Type)
            // result: (MOVVstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && !is64BitFloat(val.Type)
            // result: (MOVVstore ptr val mem)
            while (true)
            {
                t = auxToType(v.Aux);
                ptr = v_0;
                val = v_1;
                mem = v_2;
                if (!(t.Size() == 8L && !is64BitFloat(val.Type)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 4 && is32BitFloat(val.Type)
            // result: (MOVFstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 4 && is32BitFloat(val.Type)
            // result: (MOVFstore ptr val mem)
            while (true)
            {
                t = auxToType(v.Aux);
                ptr = v_0;
                val = v_1;
                mem = v_2;
                if (!(t.Size() == 4L && is32BitFloat(val.Type)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVFstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && is64BitFloat(val.Type)
            // result: (MOVDstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && is64BitFloat(val.Type)
            // result: (MOVDstore ptr val mem)
            while (true)
            {
                t = auxToType(v.Aux);
                ptr = v_0;
                val = v_1;
                mem = v_2;
                if (!(t.Size() == 8L && is64BitFloat(val.Type)))
                {
                    break;
                }

                v.reset(OpMIPS64MOVDstore);
                v.AddArg3(ptr, val, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueMIPS64_OpZero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var config = b.Func.Config;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Zero [0] _ mem)
            // result: mem
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                var mem = v_1;
                v.copyOf(mem);
                return true;

            } 
            // match: (Zero [1] ptr mem)
            // result: (MOVBstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [1] ptr mem)
            // result: (MOVBstore ptr (MOVVconst [0]) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 1L)
                {
                    break;
                }

                var ptr = v_0;
                mem = v_1;
                v.reset(OpMIPS64MOVBstore);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [2] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [2] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore ptr (MOVVconst [0]) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 2L)
                {
                    break;
                }

                var t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [2] ptr mem)
            // result: (MOVBstore [1] ptr (MOVVconst [0]) (MOVBstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [2] ptr mem)
            // result: (MOVBstore [1] ptr (MOVVconst [0]) (MOVBstore [0] ptr (MOVVconst [0]) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 2L)
                {
                    break;
                }

                ptr = v_0;
                mem = v_1;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(1L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(0L);
                v1.AddArg3(ptr, v0, mem);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore ptr (MOVVconst [0]) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 4L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [2] ptr (MOVVconst [0]) (MOVHstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [2] ptr (MOVVconst [0]) (MOVHstore [0] ptr (MOVVconst [0]) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(2L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(0L);
                v1.AddArg3(ptr, v0, mem);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [4] ptr mem)
            // result: (MOVBstore [3] ptr (MOVVconst [0]) (MOVBstore [2] ptr (MOVVconst [0]) (MOVBstore [1] ptr (MOVVconst [0]) (MOVBstore [0] ptr (MOVVconst [0]) mem))))
 
            // match: (Zero [4] ptr mem)
            // result: (MOVBstore [3] ptr (MOVVconst [0]) (MOVBstore [2] ptr (MOVVconst [0]) (MOVBstore [1] ptr (MOVVconst [0]) (MOVBstore [0] ptr (MOVVconst [0]) mem))))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                ptr = v_0;
                mem = v_1;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(3L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(2L);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v2.AuxInt = int32ToAuxInt(1L);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v3.AuxInt = int32ToAuxInt(0L);
                v3.AddArg3(ptr, v0, mem);
                v2.AddArg3(ptr, v0, v3);
                v1.AddArg3(ptr, v0, v2);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore ptr (MOVVconst [0]) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 8L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [4] ptr (MOVVconst [0]) (MOVWstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [4] ptr (MOVVconst [0]) (MOVWstore [0] ptr (MOVVconst [0]) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 4L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = int32ToAuxInt(4L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(0L);
                v1.AddArg3(ptr, v0, mem);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [6] ptr (MOVVconst [0]) (MOVHstore [4] ptr (MOVVconst [0]) (MOVHstore [2] ptr (MOVVconst [0]) (MOVHstore [0] ptr (MOVVconst [0]) mem))))
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [6] ptr (MOVVconst [0]) (MOVHstore [4] ptr (MOVVconst [0]) (MOVHstore [2] ptr (MOVVconst [0]) (MOVHstore [0] ptr (MOVVconst [0]) mem))))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(6L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(4L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v2.AuxInt = int32ToAuxInt(2L);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v3.AuxInt = int32ToAuxInt(0L);
                v3.AddArg3(ptr, v0, mem);
                v2.AddArg3(ptr, v0, v3);
                v1.AddArg3(ptr, v0, v2);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [3] ptr mem)
            // result: (MOVBstore [2] ptr (MOVVconst [0]) (MOVBstore [1] ptr (MOVVconst [0]) (MOVBstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [3] ptr mem)
            // result: (MOVBstore [2] ptr (MOVVconst [0]) (MOVBstore [1] ptr (MOVVconst [0]) (MOVBstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 3L)
                {
                    break;
                }

                ptr = v_0;
                mem = v_1;
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = int32ToAuxInt(2L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(1L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v2.AuxInt = int32ToAuxInt(0L);
                v2.AddArg3(ptr, v0, mem);
                v1.AddArg3(ptr, v0, v2);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [6] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [4] ptr (MOVVconst [0]) (MOVHstore [2] ptr (MOVVconst [0]) (MOVHstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [6] {t} ptr mem)
            // cond: t.Alignment()%2 == 0
            // result: (MOVHstore [4] ptr (MOVVconst [0]) (MOVHstore [2] ptr (MOVVconst [0]) (MOVHstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 6L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 2L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = int32ToAuxInt(4L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(2L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v2.AuxInt = int32ToAuxInt(0L);
                v2.AddArg3(ptr, v0, mem);
                v1.AddArg3(ptr, v0, v2);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [12] {t} ptr mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [8] ptr (MOVVconst [0]) (MOVWstore [4] ptr (MOVVconst [0]) (MOVWstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [12] {t} ptr mem)
            // cond: t.Alignment()%4 == 0
            // result: (MOVWstore [8] ptr (MOVVconst [0]) (MOVWstore [4] ptr (MOVVconst [0]) (MOVWstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 12L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 4L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = int32ToAuxInt(8L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(4L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v2.AuxInt = int32ToAuxInt(0L);
                v2.AddArg3(ptr, v0, mem);
                v1.AddArg3(ptr, v0, v2);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [16] {t} ptr mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [8] ptr (MOVVconst [0]) (MOVVstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [16] {t} ptr mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [8] ptr (MOVVconst [0]) (MOVVstore [0] ptr (MOVVconst [0]) mem))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 16L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 8L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = int32ToAuxInt(8L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(0L);
                v1.AddArg3(ptr, v0, mem);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [24] {t} ptr mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [16] ptr (MOVVconst [0]) (MOVVstore [8] ptr (MOVVconst [0]) (MOVVstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [24] {t} ptr mem)
            // cond: t.Alignment()%8 == 0
            // result: (MOVVstore [16] ptr (MOVVconst [0]) (MOVVstore [8] ptr (MOVVconst [0]) (MOVVstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 24L)
                {
                    break;
                }

                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(t.Alignment() % 8L == 0L))
                {
                    break;
                }

                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = int32ToAuxInt(16L);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(0L);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AuxInt = int32ToAuxInt(8L);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v2.AuxInt = int32ToAuxInt(0L);
                v2.AddArg3(ptr, v0, mem);
                v1.AddArg3(ptr, v0, v2);
                v.AddArg3(ptr, v0, v1);
                return true;

            } 
            // match: (Zero [s] {t} ptr mem)
            // cond: s%8 == 0 && s > 24 && s <= 8*128 && t.Alignment()%8 == 0 && !config.noDuffDevice
            // result: (DUFFZERO [8 * (128 - s/8)] ptr mem)
 
            // match: (Zero [s] {t} ptr mem)
            // cond: s%8 == 0 && s > 24 && s <= 8*128 && t.Alignment()%8 == 0 && !config.noDuffDevice
            // result: (DUFFZERO [8 * (128 - s/8)] ptr mem)
            while (true)
            {
                var s = auxIntToInt64(v.AuxInt);
                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!(s % 8L == 0L && s > 24L && s <= 8L * 128L && t.Alignment() % 8L == 0L && !config.noDuffDevice))
                {
                    break;
                }

                v.reset(OpMIPS64DUFFZERO);
                v.AuxInt = int64ToAuxInt(8L * (128L - s / 8L));
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Zero [s] {t} ptr mem)
            // cond: (s > 8*128 || config.noDuffDevice) || t.Alignment()%8 != 0
            // result: (LoweredZero [t.Alignment()] ptr (ADDVconst <ptr.Type> ptr [s-moveSize(t.Alignment(), config)]) mem)
 
            // match: (Zero [s] {t} ptr mem)
            // cond: (s > 8*128 || config.noDuffDevice) || t.Alignment()%8 != 0
            // result: (LoweredZero [t.Alignment()] ptr (ADDVconst <ptr.Type> ptr [s-moveSize(t.Alignment(), config)]) mem)
            while (true)
            {
                s = auxIntToInt64(v.AuxInt);
                t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                if (!((s > 8L * 128L || config.noDuffDevice) || t.Alignment() % 8L != 0L))
                {
                    break;
                }

                v.reset(OpMIPS64LoweredZero);
                v.AuxInt = int64ToAuxInt(t.Alignment());
                v0 = b.NewValue0(v.Pos, OpMIPS64ADDVconst, ptr.Type);
                v0.AuxInt = int64ToAuxInt(s - moveSize(t.Alignment(), config));
                v0.AddArg(ptr);
                v.AddArg3(ptr, v0, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteBlockMIPS64(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;


            if (b.Kind == BlockMIPS64EQ) 
                // match: (EQ (FPFlagTrue cmp) yes no)
                // result: (FPF cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64FPFlagTrue)
                {
                    var v_0 = b.Controls[0L];
                    var cmp = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64FPF, cmp);
                    return true;
                } 
                // match: (EQ (FPFlagFalse cmp) yes no)
                // result: (FPT cmp yes no)
 
                // match: (EQ (FPFlagFalse cmp) yes no)
                // result: (FPT cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64FPFlagFalse)
                {
                    v_0 = b.Controls[0L];
                    cmp = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64FPT, cmp);
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGT _ _)) yes no)
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGT _ _)) yes no)
                // result: (NE cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGT)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64NE, cmp);
                    return true;

                } 
                // match: (EQ (XORconst [1] cmp:(SGTU _ _)) yes no)
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTU _ _)) yes no)
                // result: (NE cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGTU)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64NE, cmp);
                    return true;

                } 
                // match: (EQ (XORconst [1] cmp:(SGTconst _)) yes no)
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTconst _)) yes no)
                // result: (NE cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGTconst)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64NE, cmp);
                    return true;

                } 
                // match: (EQ (XORconst [1] cmp:(SGTUconst _)) yes no)
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTUconst _)) yes no)
                // result: (NE cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGTUconst)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64NE, cmp);
                    return true;

                } 
                // match: (EQ (SGTUconst [1] x) yes no)
                // result: (NE x yes no)
 
                // match: (EQ (SGTUconst [1] x) yes no)
                // result: (NE x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGTUconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    var x = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64NE, x);
                    return true;

                } 
                // match: (EQ (SGTU x (MOVVconst [0])) yes no)
                // result: (EQ x yes no)
 
                // match: (EQ (SGTU x (MOVVconst [0])) yes no)
                // result: (EQ x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGTU)
                {
                    v_0 = b.Controls[0L];
                    _ = v_0.Args[1L];
                    x = v_0.Args[0L];
                    var v_0_1 = v_0.Args[1L];
                    if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64EQ, x);
                    return true;

                } 
                // match: (EQ (SGTconst [0] x) yes no)
                // result: (GEZ x yes no)
 
                // match: (EQ (SGTconst [0] x) yes no)
                // result: (GEZ x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGTconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 0L)
                    {
                        break;
                    }

                    x = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64GEZ, x);
                    return true;

                } 
                // match: (EQ (SGT x (MOVVconst [0])) yes no)
                // result: (LEZ x yes no)
 
                // match: (EQ (SGT x (MOVVconst [0])) yes no)
                // result: (LEZ x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGT)
                {
                    v_0 = b.Controls[0L];
                    _ = v_0.Args[1L];
                    x = v_0.Args[0L];
                    v_0_1 = v_0.Args[1L];
                    if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64LEZ, x);
                    return true;

                } 
                // match: (EQ (MOVVconst [0]) yes no)
                // result: (First yes no)
 
                // match: (EQ (MOVVconst [0]) yes no)
                // result: (First yes no)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    return true;

                } 
                // match: (EQ (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First no yes)
 
                // match: (EQ (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First no yes)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    var c = auxIntToInt64(v_0.AuxInt);
                    if (!(c != 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    b.swapSuccessors();
                    return true;

                }
            else if (b.Kind == BlockMIPS64GEZ) 
                // match: (GEZ (MOVVconst [c]) yes no)
                // cond: c >= 0
                // result: (First yes no)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c >= 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    return true;

                } 
                // match: (GEZ (MOVVconst [c]) yes no)
                // cond: c < 0
                // result: (First no yes)
 
                // match: (GEZ (MOVVconst [c]) yes no)
                // cond: c < 0
                // result: (First no yes)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c < 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    b.swapSuccessors();
                    return true;

                }
            else if (b.Kind == BlockMIPS64GTZ) 
                // match: (GTZ (MOVVconst [c]) yes no)
                // cond: c > 0
                // result: (First yes no)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c > 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    return true;

                } 
                // match: (GTZ (MOVVconst [c]) yes no)
                // cond: c <= 0
                // result: (First no yes)
 
                // match: (GTZ (MOVVconst [c]) yes no)
                // cond: c <= 0
                // result: (First no yes)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c <= 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    b.swapSuccessors();
                    return true;

                }
            else if (b.Kind == BlockIf) 
                // match: (If cond yes no)
                // result: (NE cond yes no)
                while (true)
                {
                    var cond = b.Controls[0L];
                    b.resetWithControl(BlockMIPS64NE, cond);
                    return true;
                }
            else if (b.Kind == BlockMIPS64LEZ) 
                // match: (LEZ (MOVVconst [c]) yes no)
                // cond: c <= 0
                // result: (First yes no)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c <= 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    return true;

                } 
                // match: (LEZ (MOVVconst [c]) yes no)
                // cond: c > 0
                // result: (First no yes)
 
                // match: (LEZ (MOVVconst [c]) yes no)
                // cond: c > 0
                // result: (First no yes)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c > 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    b.swapSuccessors();
                    return true;

                }
            else if (b.Kind == BlockMIPS64LTZ) 
                // match: (LTZ (MOVVconst [c]) yes no)
                // cond: c < 0
                // result: (First yes no)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c < 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    return true;

                } 
                // match: (LTZ (MOVVconst [c]) yes no)
                // cond: c >= 0
                // result: (First no yes)
 
                // match: (LTZ (MOVVconst [c]) yes no)
                // cond: c >= 0
                // result: (First no yes)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c >= 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    b.swapSuccessors();
                    return true;

                }
            else if (b.Kind == BlockMIPS64NE) 
                // match: (NE (FPFlagTrue cmp) yes no)
                // result: (FPT cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64FPFlagTrue)
                {
                    v_0 = b.Controls[0L];
                    cmp = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64FPT, cmp);
                    return true;
                } 
                // match: (NE (FPFlagFalse cmp) yes no)
                // result: (FPF cmp yes no)
 
                // match: (NE (FPFlagFalse cmp) yes no)
                // result: (FPF cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64FPFlagFalse)
                {
                    v_0 = b.Controls[0L];
                    cmp = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64FPF, cmp);
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGT _ _)) yes no)
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGT _ _)) yes no)
                // result: (EQ cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGT)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64EQ, cmp);
                    return true;

                } 
                // match: (NE (XORconst [1] cmp:(SGTU _ _)) yes no)
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTU _ _)) yes no)
                // result: (EQ cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGTU)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64EQ, cmp);
                    return true;

                } 
                // match: (NE (XORconst [1] cmp:(SGTconst _)) yes no)
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTconst _)) yes no)
                // result: (EQ cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGTconst)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64EQ, cmp);
                    return true;

                } 
                // match: (NE (XORconst [1] cmp:(SGTUconst _)) yes no)
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTUconst _)) yes no)
                // result: (EQ cmp yes no)
                while (b.Controls[0L].Op == OpMIPS64XORconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    cmp = v_0.Args[0L];
                    if (cmp.Op != OpMIPS64SGTUconst)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64EQ, cmp);
                    return true;

                } 
                // match: (NE (SGTUconst [1] x) yes no)
                // result: (EQ x yes no)
 
                // match: (NE (SGTUconst [1] x) yes no)
                // result: (EQ x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGTUconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 1L)
                    {
                        break;
                    }

                    x = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64EQ, x);
                    return true;

                } 
                // match: (NE (SGTU x (MOVVconst [0])) yes no)
                // result: (NE x yes no)
 
                // match: (NE (SGTU x (MOVVconst [0])) yes no)
                // result: (NE x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGTU)
                {
                    v_0 = b.Controls[0L];
                    _ = v_0.Args[1L];
                    x = v_0.Args[0L];
                    v_0_1 = v_0.Args[1L];
                    if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64NE, x);
                    return true;

                } 
                // match: (NE (SGTconst [0] x) yes no)
                // result: (LTZ x yes no)
 
                // match: (NE (SGTconst [0] x) yes no)
                // result: (LTZ x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGTconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 0L)
                    {
                        break;
                    }

                    x = v_0.Args[0L];
                    b.resetWithControl(BlockMIPS64LTZ, x);
                    return true;

                } 
                // match: (NE (SGT x (MOVVconst [0])) yes no)
                // result: (GTZ x yes no)
 
                // match: (NE (SGT x (MOVVconst [0])) yes no)
                // result: (GTZ x yes no)
                while (b.Controls[0L].Op == OpMIPS64SGT)
                {
                    v_0 = b.Controls[0L];
                    _ = v_0.Args[1L];
                    x = v_0.Args[0L];
                    v_0_1 = v_0.Args[1L];
                    if (v_0_1.Op != OpMIPS64MOVVconst || auxIntToInt64(v_0_1.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.resetWithControl(BlockMIPS64GTZ, x);
                    return true;

                } 
                // match: (NE (MOVVconst [0]) yes no)
                // result: (First no yes)
 
                // match: (NE (MOVVconst [0]) yes no)
                // result: (First no yes)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    b.swapSuccessors();
                    return true;

                } 
                // match: (NE (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First yes no)
 
                // match: (NE (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First yes no)
                while (b.Controls[0L].Op == OpMIPS64MOVVconst)
                {
                    v_0 = b.Controls[0L];
                    c = auxIntToInt64(v_0.AuxInt);
                    if (!(c != 0L))
                    {
                        break;
                    }

                    b.Reset(BlockFirst);
                    return true;

                }
                        return false;

        }
    }
}}}}
