// Code generated from gen/RISCV64.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 October 08 04:24:29 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewriteRISCV64.go
using math = go.math_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static bool rewriteValueRISCV64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;


            if (v.Op == OpAdd16) 
                v.Op = OpRISCV64ADD;
                return true;
            else if (v.Op == OpAdd32) 
                v.Op = OpRISCV64ADD;
                return true;
            else if (v.Op == OpAdd32F) 
                v.Op = OpRISCV64FADDS;
                return true;
            else if (v.Op == OpAdd64) 
                v.Op = OpRISCV64ADD;
                return true;
            else if (v.Op == OpAdd64F) 
                v.Op = OpRISCV64FADDD;
                return true;
            else if (v.Op == OpAdd8) 
                v.Op = OpRISCV64ADD;
                return true;
            else if (v.Op == OpAddPtr) 
                v.Op = OpRISCV64ADD;
                return true;
            else if (v.Op == OpAddr) 
                return rewriteValueRISCV64_OpAddr(_addr_v);
            else if (v.Op == OpAnd16) 
                v.Op = OpRISCV64AND;
                return true;
            else if (v.Op == OpAnd32) 
                v.Op = OpRISCV64AND;
                return true;
            else if (v.Op == OpAnd64) 
                v.Op = OpRISCV64AND;
                return true;
            else if (v.Op == OpAnd8) 
                v.Op = OpRISCV64AND;
                return true;
            else if (v.Op == OpAndB) 
                v.Op = OpRISCV64AND;
                return true;
            else if (v.Op == OpAtomicAdd32) 
                v.Op = OpRISCV64LoweredAtomicAdd32;
                return true;
            else if (v.Op == OpAtomicAdd64) 
                v.Op = OpRISCV64LoweredAtomicAdd64;
                return true;
            else if (v.Op == OpAtomicCompareAndSwap32) 
                v.Op = OpRISCV64LoweredAtomicCas32;
                return true;
            else if (v.Op == OpAtomicCompareAndSwap64) 
                v.Op = OpRISCV64LoweredAtomicCas64;
                return true;
            else if (v.Op == OpAtomicExchange32) 
                v.Op = OpRISCV64LoweredAtomicExchange32;
                return true;
            else if (v.Op == OpAtomicExchange64) 
                v.Op = OpRISCV64LoweredAtomicExchange64;
                return true;
            else if (v.Op == OpAtomicLoad32) 
                v.Op = OpRISCV64LoweredAtomicLoad32;
                return true;
            else if (v.Op == OpAtomicLoad64) 
                v.Op = OpRISCV64LoweredAtomicLoad64;
                return true;
            else if (v.Op == OpAtomicLoad8) 
                v.Op = OpRISCV64LoweredAtomicLoad8;
                return true;
            else if (v.Op == OpAtomicLoadPtr) 
                v.Op = OpRISCV64LoweredAtomicLoad64;
                return true;
            else if (v.Op == OpAtomicStore32) 
                v.Op = OpRISCV64LoweredAtomicStore32;
                return true;
            else if (v.Op == OpAtomicStore64) 
                v.Op = OpRISCV64LoweredAtomicStore64;
                return true;
            else if (v.Op == OpAtomicStore8) 
                v.Op = OpRISCV64LoweredAtomicStore8;
                return true;
            else if (v.Op == OpAtomicStorePtrNoWB) 
                v.Op = OpRISCV64LoweredAtomicStore64;
                return true;
            else if (v.Op == OpAvg64u) 
                return rewriteValueRISCV64_OpAvg64u(_addr_v);
            else if (v.Op == OpClosureCall) 
                v.Op = OpRISCV64CALLclosure;
                return true;
            else if (v.Op == OpCom16) 
                v.Op = OpRISCV64NOT;
                return true;
            else if (v.Op == OpCom32) 
                v.Op = OpRISCV64NOT;
                return true;
            else if (v.Op == OpCom64) 
                v.Op = OpRISCV64NOT;
                return true;
            else if (v.Op == OpCom8) 
                v.Op = OpRISCV64NOT;
                return true;
            else if (v.Op == OpConst16) 
                v.Op = OpRISCV64MOVHconst;
                return true;
            else if (v.Op == OpConst32) 
                v.Op = OpRISCV64MOVWconst;
                return true;
            else if (v.Op == OpConst32F) 
                return rewriteValueRISCV64_OpConst32F(_addr_v);
            else if (v.Op == OpConst64) 
                v.Op = OpRISCV64MOVDconst;
                return true;
            else if (v.Op == OpConst64F) 
                return rewriteValueRISCV64_OpConst64F(_addr_v);
            else if (v.Op == OpConst8) 
                v.Op = OpRISCV64MOVBconst;
                return true;
            else if (v.Op == OpConstBool) 
                return rewriteValueRISCV64_OpConstBool(_addr_v);
            else if (v.Op == OpConstNil) 
                return rewriteValueRISCV64_OpConstNil(_addr_v);
            else if (v.Op == OpConvert) 
                v.Op = OpRISCV64MOVconvert;
                return true;
            else if (v.Op == OpCvt32Fto32) 
                v.Op = OpRISCV64FCVTWS;
                return true;
            else if (v.Op == OpCvt32Fto64) 
                v.Op = OpRISCV64FCVTLS;
                return true;
            else if (v.Op == OpCvt32Fto64F) 
                v.Op = OpRISCV64FCVTDS;
                return true;
            else if (v.Op == OpCvt32to32F) 
                v.Op = OpRISCV64FCVTSW;
                return true;
            else if (v.Op == OpCvt32to64F) 
                v.Op = OpRISCV64FCVTDW;
                return true;
            else if (v.Op == OpCvt64Fto32) 
                v.Op = OpRISCV64FCVTWD;
                return true;
            else if (v.Op == OpCvt64Fto32F) 
                v.Op = OpRISCV64FCVTSD;
                return true;
            else if (v.Op == OpCvt64Fto64) 
                v.Op = OpRISCV64FCVTLD;
                return true;
            else if (v.Op == OpCvt64to32F) 
                v.Op = OpRISCV64FCVTSL;
                return true;
            else if (v.Op == OpCvt64to64F) 
                v.Op = OpRISCV64FCVTDL;
                return true;
            else if (v.Op == OpCvtBoolToUint8) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpDiv16) 
                return rewriteValueRISCV64_OpDiv16(_addr_v);
            else if (v.Op == OpDiv16u) 
                return rewriteValueRISCV64_OpDiv16u(_addr_v);
            else if (v.Op == OpDiv32) 
                return rewriteValueRISCV64_OpDiv32(_addr_v);
            else if (v.Op == OpDiv32F) 
                v.Op = OpRISCV64FDIVS;
                return true;
            else if (v.Op == OpDiv32u) 
                v.Op = OpRISCV64DIVUW;
                return true;
            else if (v.Op == OpDiv64) 
                return rewriteValueRISCV64_OpDiv64(_addr_v);
            else if (v.Op == OpDiv64F) 
                v.Op = OpRISCV64FDIVD;
                return true;
            else if (v.Op == OpDiv64u) 
                v.Op = OpRISCV64DIVU;
                return true;
            else if (v.Op == OpDiv8) 
                return rewriteValueRISCV64_OpDiv8(_addr_v);
            else if (v.Op == OpDiv8u) 
                return rewriteValueRISCV64_OpDiv8u(_addr_v);
            else if (v.Op == OpEq16) 
                return rewriteValueRISCV64_OpEq16(_addr_v);
            else if (v.Op == OpEq32) 
                return rewriteValueRISCV64_OpEq32(_addr_v);
            else if (v.Op == OpEq32F) 
                v.Op = OpRISCV64FEQS;
                return true;
            else if (v.Op == OpEq64) 
                return rewriteValueRISCV64_OpEq64(_addr_v);
            else if (v.Op == OpEq64F) 
                v.Op = OpRISCV64FEQD;
                return true;
            else if (v.Op == OpEq8) 
                return rewriteValueRISCV64_OpEq8(_addr_v);
            else if (v.Op == OpEqB) 
                return rewriteValueRISCV64_OpEqB(_addr_v);
            else if (v.Op == OpEqPtr) 
                return rewriteValueRISCV64_OpEqPtr(_addr_v);
            else if (v.Op == OpGetCallerPC) 
                v.Op = OpRISCV64LoweredGetCallerPC;
                return true;
            else if (v.Op == OpGetCallerSP) 
                v.Op = OpRISCV64LoweredGetCallerSP;
                return true;
            else if (v.Op == OpGetClosurePtr) 
                v.Op = OpRISCV64LoweredGetClosurePtr;
                return true;
            else if (v.Op == OpHmul32) 
                return rewriteValueRISCV64_OpHmul32(_addr_v);
            else if (v.Op == OpHmul32u) 
                return rewriteValueRISCV64_OpHmul32u(_addr_v);
            else if (v.Op == OpHmul64) 
                v.Op = OpRISCV64MULH;
                return true;
            else if (v.Op == OpHmul64u) 
                v.Op = OpRISCV64MULHU;
                return true;
            else if (v.Op == OpInterCall) 
                v.Op = OpRISCV64CALLinter;
                return true;
            else if (v.Op == OpIsInBounds) 
                v.Op = OpLess64U;
                return true;
            else if (v.Op == OpIsNonNil) 
                return rewriteValueRISCV64_OpIsNonNil(_addr_v);
            else if (v.Op == OpIsSliceInBounds) 
                v.Op = OpLeq64U;
                return true;
            else if (v.Op == OpLeq16) 
                return rewriteValueRISCV64_OpLeq16(_addr_v);
            else if (v.Op == OpLeq16U) 
                return rewriteValueRISCV64_OpLeq16U(_addr_v);
            else if (v.Op == OpLeq32) 
                return rewriteValueRISCV64_OpLeq32(_addr_v);
            else if (v.Op == OpLeq32F) 
                v.Op = OpRISCV64FLES;
                return true;
            else if (v.Op == OpLeq32U) 
                return rewriteValueRISCV64_OpLeq32U(_addr_v);
            else if (v.Op == OpLeq64) 
                return rewriteValueRISCV64_OpLeq64(_addr_v);
            else if (v.Op == OpLeq64F) 
                v.Op = OpRISCV64FLED;
                return true;
            else if (v.Op == OpLeq64U) 
                return rewriteValueRISCV64_OpLeq64U(_addr_v);
            else if (v.Op == OpLeq8) 
                return rewriteValueRISCV64_OpLeq8(_addr_v);
            else if (v.Op == OpLeq8U) 
                return rewriteValueRISCV64_OpLeq8U(_addr_v);
            else if (v.Op == OpLess16) 
                return rewriteValueRISCV64_OpLess16(_addr_v);
            else if (v.Op == OpLess16U) 
                return rewriteValueRISCV64_OpLess16U(_addr_v);
            else if (v.Op == OpLess32) 
                return rewriteValueRISCV64_OpLess32(_addr_v);
            else if (v.Op == OpLess32F) 
                v.Op = OpRISCV64FLTS;
                return true;
            else if (v.Op == OpLess32U) 
                return rewriteValueRISCV64_OpLess32U(_addr_v);
            else if (v.Op == OpLess64) 
                v.Op = OpRISCV64SLT;
                return true;
            else if (v.Op == OpLess64F) 
                v.Op = OpRISCV64FLTD;
                return true;
            else if (v.Op == OpLess64U) 
                v.Op = OpRISCV64SLTU;
                return true;
            else if (v.Op == OpLess8) 
                return rewriteValueRISCV64_OpLess8(_addr_v);
            else if (v.Op == OpLess8U) 
                return rewriteValueRISCV64_OpLess8U(_addr_v);
            else if (v.Op == OpLoad) 
                return rewriteValueRISCV64_OpLoad(_addr_v);
            else if (v.Op == OpLocalAddr) 
                return rewriteValueRISCV64_OpLocalAddr(_addr_v);
            else if (v.Op == OpLsh16x16) 
                return rewriteValueRISCV64_OpLsh16x16(_addr_v);
            else if (v.Op == OpLsh16x32) 
                return rewriteValueRISCV64_OpLsh16x32(_addr_v);
            else if (v.Op == OpLsh16x64) 
                return rewriteValueRISCV64_OpLsh16x64(_addr_v);
            else if (v.Op == OpLsh16x8) 
                return rewriteValueRISCV64_OpLsh16x8(_addr_v);
            else if (v.Op == OpLsh32x16) 
                return rewriteValueRISCV64_OpLsh32x16(_addr_v);
            else if (v.Op == OpLsh32x32) 
                return rewriteValueRISCV64_OpLsh32x32(_addr_v);
            else if (v.Op == OpLsh32x64) 
                return rewriteValueRISCV64_OpLsh32x64(_addr_v);
            else if (v.Op == OpLsh32x8) 
                return rewriteValueRISCV64_OpLsh32x8(_addr_v);
            else if (v.Op == OpLsh64x16) 
                return rewriteValueRISCV64_OpLsh64x16(_addr_v);
            else if (v.Op == OpLsh64x32) 
                return rewriteValueRISCV64_OpLsh64x32(_addr_v);
            else if (v.Op == OpLsh64x64) 
                return rewriteValueRISCV64_OpLsh64x64(_addr_v);
            else if (v.Op == OpLsh64x8) 
                return rewriteValueRISCV64_OpLsh64x8(_addr_v);
            else if (v.Op == OpLsh8x16) 
                return rewriteValueRISCV64_OpLsh8x16(_addr_v);
            else if (v.Op == OpLsh8x32) 
                return rewriteValueRISCV64_OpLsh8x32(_addr_v);
            else if (v.Op == OpLsh8x64) 
                return rewriteValueRISCV64_OpLsh8x64(_addr_v);
            else if (v.Op == OpLsh8x8) 
                return rewriteValueRISCV64_OpLsh8x8(_addr_v);
            else if (v.Op == OpMod16) 
                return rewriteValueRISCV64_OpMod16(_addr_v);
            else if (v.Op == OpMod16u) 
                return rewriteValueRISCV64_OpMod16u(_addr_v);
            else if (v.Op == OpMod32) 
                return rewriteValueRISCV64_OpMod32(_addr_v);
            else if (v.Op == OpMod32u) 
                v.Op = OpRISCV64REMUW;
                return true;
            else if (v.Op == OpMod64) 
                return rewriteValueRISCV64_OpMod64(_addr_v);
            else if (v.Op == OpMod64u) 
                v.Op = OpRISCV64REMU;
                return true;
            else if (v.Op == OpMod8) 
                return rewriteValueRISCV64_OpMod8(_addr_v);
            else if (v.Op == OpMod8u) 
                return rewriteValueRISCV64_OpMod8u(_addr_v);
            else if (v.Op == OpMove) 
                return rewriteValueRISCV64_OpMove(_addr_v);
            else if (v.Op == OpMul16) 
                return rewriteValueRISCV64_OpMul16(_addr_v);
            else if (v.Op == OpMul32) 
                v.Op = OpRISCV64MULW;
                return true;
            else if (v.Op == OpMul32F) 
                v.Op = OpRISCV64FMULS;
                return true;
            else if (v.Op == OpMul64) 
                v.Op = OpRISCV64MUL;
                return true;
            else if (v.Op == OpMul64F) 
                v.Op = OpRISCV64FMULD;
                return true;
            else if (v.Op == OpMul8) 
                return rewriteValueRISCV64_OpMul8(_addr_v);
            else if (v.Op == OpNeg16) 
                v.Op = OpRISCV64NEG;
                return true;
            else if (v.Op == OpNeg32) 
                v.Op = OpRISCV64NEG;
                return true;
            else if (v.Op == OpNeg32F) 
                v.Op = OpRISCV64FNEGS;
                return true;
            else if (v.Op == OpNeg64) 
                v.Op = OpRISCV64NEG;
                return true;
            else if (v.Op == OpNeg64F) 
                v.Op = OpRISCV64FNEGD;
                return true;
            else if (v.Op == OpNeg8) 
                v.Op = OpRISCV64NEG;
                return true;
            else if (v.Op == OpNeq16) 
                return rewriteValueRISCV64_OpNeq16(_addr_v);
            else if (v.Op == OpNeq32) 
                return rewriteValueRISCV64_OpNeq32(_addr_v);
            else if (v.Op == OpNeq32F) 
                v.Op = OpRISCV64FNES;
                return true;
            else if (v.Op == OpNeq64) 
                return rewriteValueRISCV64_OpNeq64(_addr_v);
            else if (v.Op == OpNeq64F) 
                v.Op = OpRISCV64FNED;
                return true;
            else if (v.Op == OpNeq8) 
                return rewriteValueRISCV64_OpNeq8(_addr_v);
            else if (v.Op == OpNeqB) 
                v.Op = OpRISCV64XOR;
                return true;
            else if (v.Op == OpNeqPtr) 
                return rewriteValueRISCV64_OpNeqPtr(_addr_v);
            else if (v.Op == OpNilCheck) 
                v.Op = OpRISCV64LoweredNilCheck;
                return true;
            else if (v.Op == OpNot) 
                v.Op = OpRISCV64SEQZ;
                return true;
            else if (v.Op == OpOffPtr) 
                return rewriteValueRISCV64_OpOffPtr(_addr_v);
            else if (v.Op == OpOr16) 
                v.Op = OpRISCV64OR;
                return true;
            else if (v.Op == OpOr32) 
                v.Op = OpRISCV64OR;
                return true;
            else if (v.Op == OpOr64) 
                v.Op = OpRISCV64OR;
                return true;
            else if (v.Op == OpOr8) 
                v.Op = OpRISCV64OR;
                return true;
            else if (v.Op == OpOrB) 
                v.Op = OpRISCV64OR;
                return true;
            else if (v.Op == OpPanicBounds) 
                return rewriteValueRISCV64_OpPanicBounds(_addr_v);
            else if (v.Op == OpRISCV64ADD) 
                return rewriteValueRISCV64_OpRISCV64ADD(_addr_v);
            else if (v.Op == OpRISCV64ADDI) 
                return rewriteValueRISCV64_OpRISCV64ADDI(_addr_v);
            else if (v.Op == OpRISCV64AND) 
                return rewriteValueRISCV64_OpRISCV64AND(_addr_v);
            else if (v.Op == OpRISCV64MOVBUload) 
                return rewriteValueRISCV64_OpRISCV64MOVBUload(_addr_v);
            else if (v.Op == OpRISCV64MOVBload) 
                return rewriteValueRISCV64_OpRISCV64MOVBload(_addr_v);
            else if (v.Op == OpRISCV64MOVBstore) 
                return rewriteValueRISCV64_OpRISCV64MOVBstore(_addr_v);
            else if (v.Op == OpRISCV64MOVBstorezero) 
                return rewriteValueRISCV64_OpRISCV64MOVBstorezero(_addr_v);
            else if (v.Op == OpRISCV64MOVDconst) 
                return rewriteValueRISCV64_OpRISCV64MOVDconst(_addr_v);
            else if (v.Op == OpRISCV64MOVDload) 
                return rewriteValueRISCV64_OpRISCV64MOVDload(_addr_v);
            else if (v.Op == OpRISCV64MOVDstore) 
                return rewriteValueRISCV64_OpRISCV64MOVDstore(_addr_v);
            else if (v.Op == OpRISCV64MOVDstorezero) 
                return rewriteValueRISCV64_OpRISCV64MOVDstorezero(_addr_v);
            else if (v.Op == OpRISCV64MOVHUload) 
                return rewriteValueRISCV64_OpRISCV64MOVHUload(_addr_v);
            else if (v.Op == OpRISCV64MOVHload) 
                return rewriteValueRISCV64_OpRISCV64MOVHload(_addr_v);
            else if (v.Op == OpRISCV64MOVHstore) 
                return rewriteValueRISCV64_OpRISCV64MOVHstore(_addr_v);
            else if (v.Op == OpRISCV64MOVHstorezero) 
                return rewriteValueRISCV64_OpRISCV64MOVHstorezero(_addr_v);
            else if (v.Op == OpRISCV64MOVWUload) 
                return rewriteValueRISCV64_OpRISCV64MOVWUload(_addr_v);
            else if (v.Op == OpRISCV64MOVWload) 
                return rewriteValueRISCV64_OpRISCV64MOVWload(_addr_v);
            else if (v.Op == OpRISCV64MOVWstore) 
                return rewriteValueRISCV64_OpRISCV64MOVWstore(_addr_v);
            else if (v.Op == OpRISCV64MOVWstorezero) 
                return rewriteValueRISCV64_OpRISCV64MOVWstorezero(_addr_v);
            else if (v.Op == OpRISCV64OR) 
                return rewriteValueRISCV64_OpRISCV64OR(_addr_v);
            else if (v.Op == OpRISCV64SLL) 
                return rewriteValueRISCV64_OpRISCV64SLL(_addr_v);
            else if (v.Op == OpRISCV64SRA) 
                return rewriteValueRISCV64_OpRISCV64SRA(_addr_v);
            else if (v.Op == OpRISCV64SRL) 
                return rewriteValueRISCV64_OpRISCV64SRL(_addr_v);
            else if (v.Op == OpRISCV64SUB) 
                return rewriteValueRISCV64_OpRISCV64SUB(_addr_v);
            else if (v.Op == OpRISCV64SUBW) 
                return rewriteValueRISCV64_OpRISCV64SUBW(_addr_v);
            else if (v.Op == OpRISCV64XOR) 
                return rewriteValueRISCV64_OpRISCV64XOR(_addr_v);
            else if (v.Op == OpRotateLeft16) 
                return rewriteValueRISCV64_OpRotateLeft16(_addr_v);
            else if (v.Op == OpRotateLeft32) 
                return rewriteValueRISCV64_OpRotateLeft32(_addr_v);
            else if (v.Op == OpRotateLeft64) 
                return rewriteValueRISCV64_OpRotateLeft64(_addr_v);
            else if (v.Op == OpRotateLeft8) 
                return rewriteValueRISCV64_OpRotateLeft8(_addr_v);
            else if (v.Op == OpRound32F) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpRound64F) 
                v.Op = OpCopy;
                return true;
            else if (v.Op == OpRsh16Ux16) 
                return rewriteValueRISCV64_OpRsh16Ux16(_addr_v);
            else if (v.Op == OpRsh16Ux32) 
                return rewriteValueRISCV64_OpRsh16Ux32(_addr_v);
            else if (v.Op == OpRsh16Ux64) 
                return rewriteValueRISCV64_OpRsh16Ux64(_addr_v);
            else if (v.Op == OpRsh16Ux8) 
                return rewriteValueRISCV64_OpRsh16Ux8(_addr_v);
            else if (v.Op == OpRsh16x16) 
                return rewriteValueRISCV64_OpRsh16x16(_addr_v);
            else if (v.Op == OpRsh16x32) 
                return rewriteValueRISCV64_OpRsh16x32(_addr_v);
            else if (v.Op == OpRsh16x64) 
                return rewriteValueRISCV64_OpRsh16x64(_addr_v);
            else if (v.Op == OpRsh16x8) 
                return rewriteValueRISCV64_OpRsh16x8(_addr_v);
            else if (v.Op == OpRsh32Ux16) 
                return rewriteValueRISCV64_OpRsh32Ux16(_addr_v);
            else if (v.Op == OpRsh32Ux32) 
                return rewriteValueRISCV64_OpRsh32Ux32(_addr_v);
            else if (v.Op == OpRsh32Ux64) 
                return rewriteValueRISCV64_OpRsh32Ux64(_addr_v);
            else if (v.Op == OpRsh32Ux8) 
                return rewriteValueRISCV64_OpRsh32Ux8(_addr_v);
            else if (v.Op == OpRsh32x16) 
                return rewriteValueRISCV64_OpRsh32x16(_addr_v);
            else if (v.Op == OpRsh32x32) 
                return rewriteValueRISCV64_OpRsh32x32(_addr_v);
            else if (v.Op == OpRsh32x64) 
                return rewriteValueRISCV64_OpRsh32x64(_addr_v);
            else if (v.Op == OpRsh32x8) 
                return rewriteValueRISCV64_OpRsh32x8(_addr_v);
            else if (v.Op == OpRsh64Ux16) 
                return rewriteValueRISCV64_OpRsh64Ux16(_addr_v);
            else if (v.Op == OpRsh64Ux32) 
                return rewriteValueRISCV64_OpRsh64Ux32(_addr_v);
            else if (v.Op == OpRsh64Ux64) 
                return rewriteValueRISCV64_OpRsh64Ux64(_addr_v);
            else if (v.Op == OpRsh64Ux8) 
                return rewriteValueRISCV64_OpRsh64Ux8(_addr_v);
            else if (v.Op == OpRsh64x16) 
                return rewriteValueRISCV64_OpRsh64x16(_addr_v);
            else if (v.Op == OpRsh64x32) 
                return rewriteValueRISCV64_OpRsh64x32(_addr_v);
            else if (v.Op == OpRsh64x64) 
                return rewriteValueRISCV64_OpRsh64x64(_addr_v);
            else if (v.Op == OpRsh64x8) 
                return rewriteValueRISCV64_OpRsh64x8(_addr_v);
            else if (v.Op == OpRsh8Ux16) 
                return rewriteValueRISCV64_OpRsh8Ux16(_addr_v);
            else if (v.Op == OpRsh8Ux32) 
                return rewriteValueRISCV64_OpRsh8Ux32(_addr_v);
            else if (v.Op == OpRsh8Ux64) 
                return rewriteValueRISCV64_OpRsh8Ux64(_addr_v);
            else if (v.Op == OpRsh8Ux8) 
                return rewriteValueRISCV64_OpRsh8Ux8(_addr_v);
            else if (v.Op == OpRsh8x16) 
                return rewriteValueRISCV64_OpRsh8x16(_addr_v);
            else if (v.Op == OpRsh8x32) 
                return rewriteValueRISCV64_OpRsh8x32(_addr_v);
            else if (v.Op == OpRsh8x64) 
                return rewriteValueRISCV64_OpRsh8x64(_addr_v);
            else if (v.Op == OpRsh8x8) 
                return rewriteValueRISCV64_OpRsh8x8(_addr_v);
            else if (v.Op == OpSignExt16to32) 
                return rewriteValueRISCV64_OpSignExt16to32(_addr_v);
            else if (v.Op == OpSignExt16to64) 
                return rewriteValueRISCV64_OpSignExt16to64(_addr_v);
            else if (v.Op == OpSignExt32to64) 
                return rewriteValueRISCV64_OpSignExt32to64(_addr_v);
            else if (v.Op == OpSignExt8to16) 
                return rewriteValueRISCV64_OpSignExt8to16(_addr_v);
            else if (v.Op == OpSignExt8to32) 
                return rewriteValueRISCV64_OpSignExt8to32(_addr_v);
            else if (v.Op == OpSignExt8to64) 
                return rewriteValueRISCV64_OpSignExt8to64(_addr_v);
            else if (v.Op == OpSlicemask) 
                return rewriteValueRISCV64_OpSlicemask(_addr_v);
            else if (v.Op == OpSqrt) 
                v.Op = OpRISCV64FSQRTD;
                return true;
            else if (v.Op == OpStaticCall) 
                v.Op = OpRISCV64CALLstatic;
                return true;
            else if (v.Op == OpStore) 
                return rewriteValueRISCV64_OpStore(_addr_v);
            else if (v.Op == OpSub16) 
                v.Op = OpRISCV64SUB;
                return true;
            else if (v.Op == OpSub32) 
                v.Op = OpRISCV64SUB;
                return true;
            else if (v.Op == OpSub32F) 
                v.Op = OpRISCV64FSUBS;
                return true;
            else if (v.Op == OpSub64) 
                v.Op = OpRISCV64SUB;
                return true;
            else if (v.Op == OpSub64F) 
                v.Op = OpRISCV64FSUBD;
                return true;
            else if (v.Op == OpSub8) 
                v.Op = OpRISCV64SUB;
                return true;
            else if (v.Op == OpSubPtr) 
                v.Op = OpRISCV64SUB;
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
                v.Op = OpRISCV64LoweredWB;
                return true;
            else if (v.Op == OpXor16) 
                v.Op = OpRISCV64XOR;
                return true;
            else if (v.Op == OpXor32) 
                v.Op = OpRISCV64XOR;
                return true;
            else if (v.Op == OpXor64) 
                v.Op = OpRISCV64XOR;
                return true;
            else if (v.Op == OpXor8) 
                v.Op = OpRISCV64XOR;
                return true;
            else if (v.Op == OpZero) 
                return rewriteValueRISCV64_OpZero(_addr_v);
            else if (v.Op == OpZeroExt16to32) 
                return rewriteValueRISCV64_OpZeroExt16to32(_addr_v);
            else if (v.Op == OpZeroExt16to64) 
                return rewriteValueRISCV64_OpZeroExt16to64(_addr_v);
            else if (v.Op == OpZeroExt32to64) 
                return rewriteValueRISCV64_OpZeroExt32to64(_addr_v);
            else if (v.Op == OpZeroExt8to16) 
                return rewriteValueRISCV64_OpZeroExt8to16(_addr_v);
            else if (v.Op == OpZeroExt8to32) 
                return rewriteValueRISCV64_OpZeroExt8to32(_addr_v);
            else if (v.Op == OpZeroExt8to64) 
                return rewriteValueRISCV64_OpZeroExt8to64(_addr_v);
                        return false;

        }
        private static bool rewriteValueRISCV64_OpAddr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (Addr {sym} base)
            // result: (MOVaddr {sym} [0] base)
            while (true)
            {
                var sym = auxToSym(v.Aux);
                var @base = v_0;
                v.reset(OpRISCV64MOVaddr);
                v.AuxInt = int32ToAuxInt(0L);
                v.Aux = symToAux(sym);
                v.AddArg(base);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpAvg64u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Avg64u <t> x y)
            // result: (ADD (ADD <t> (SRLI <t> [1] x) (SRLI <t> [1] y)) (ANDI <t> [1] (AND <t> x y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64ADD);
                var v0 = b.NewValue0(v.Pos, OpRISCV64ADD, t);
                var v1 = b.NewValue0(v.Pos, OpRISCV64SRLI, t);
                v1.AuxInt = int64ToAuxInt(1L);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SRLI, t);
                v2.AuxInt = int64ToAuxInt(1L);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                var v3 = b.NewValue0(v.Pos, OpRISCV64ANDI, t);
                v3.AuxInt = int64ToAuxInt(1L);
                var v4 = b.NewValue0(v.Pos, OpRISCV64AND, t);
                v4.AddArg2(x, y);
                v3.AddArg(v4);
                v.AddArg2(v0, v3);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpConst32F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Const32F [val])
            // result: (FMVSX (MOVWconst [int32(math.Float32bits(val))]))
            while (true)
            {
                var val = auxIntToFloat32(v.AuxInt);
                v.reset(OpRISCV64FMVSX);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MOVWconst, typ.UInt32);
                v0.AuxInt = int32ToAuxInt(int32(math.Float32bits(val)));
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpConst64F(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Const64F [val])
            // result: (FMVDX (MOVDconst [int64(math.Float64bits(val))]))
            while (true)
            {
                var val = auxIntToFloat64(v.AuxInt);
                v.reset(OpRISCV64FMVDX);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(int64(math.Float64bits(val)));
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpConstBool(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;
 
            // match: (ConstBool [val])
            // result: (MOVBconst [int8(b2i(val))])
            while (true)
            {
                var val = auxIntToBool(v.AuxInt);
                v.reset(OpRISCV64MOVBconst);
                v.AuxInt = int8ToAuxInt(int8(b2i(val)));
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpConstNil(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;
 
            // match: (ConstNil)
            // result: (MOVDconst [0])
            while (true)
            {
                v.reset(OpRISCV64MOVDconst);
                v.AuxInt = int64ToAuxInt(0L);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpDiv16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div16 x y [false])
            // result: (DIVW (SignExt16to32 x) (SignExt16to32 y))
            while (true)
            {
                if (auxIntToBool(v.AuxInt) != false)
                {
                    break;
                }

                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64DIVW);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpDiv16u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div16u x y)
            // result: (DIVUW (ZeroExt16to32 x) (ZeroExt16to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64DIVUW);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpDiv32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Div32 x y [false])
            // result: (DIVW x y)
            while (true)
            {
                if (auxIntToBool(v.AuxInt) != false)
                {
                    break;
                }

                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64DIVW);
                v.AddArg2(x, y);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpDiv64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Div64 x y [false])
            // result: (DIV x y)
            while (true)
            {
                if (auxIntToBool(v.AuxInt) != false)
                {
                    break;
                }

                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64DIV);
                v.AddArg2(x, y);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpDiv8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div8 x y)
            // result: (DIVW (SignExt8to32 x) (SignExt8to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64DIVW);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpDiv8u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Div8u x y)
            // result: (DIVUW (ZeroExt8to32 x) (ZeroExt8to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64DIVUW);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpEq16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Eq16 x y)
            // result: (SEQZ (ZeroExt16to64 (SUB <x.Type> x y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SEQZ);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v1.AddArg2(x, y);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpEq32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Eq32 x y)
            // result: (SEQZ (SUBW <x.Type> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SEQZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SUBW, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpEq64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Eq64 x y)
            // result: (SEQZ (SUB <x.Type> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SEQZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpEq8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Eq8 x y)
            // result: (SEQZ (ZeroExt8to64 (SUB <x.Type> x y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SEQZ);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v1.AddArg2(x, y);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpEqB(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (EqB x y)
            // result: (SEQZ (XOR <typ.Bool> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SEQZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64XOR, typ.Bool);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpEqPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (EqPtr x y)
            // result: (SEQZ (SUB <x.Type> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SEQZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpHmul32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Hmul32 x y)
            // result: (SRAI [32] (MUL (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(32L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MUL, typ.Int64);
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpHmul32u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Hmul32u x y)
            // result: (SRLI [32] (MUL (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(32L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MUL, typ.Int64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg2(v1, v2);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpIsNonNil(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (IsNonNil p)
            // result: (NeqPtr (MOVDconst) p)
            while (true)
            {
                var p = v_0;
                v.reset(OpNeqPtr);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v.AddArg2(v0, p);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq16 x y)
            // result: (Not (Less16 y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess16, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq16U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq16U x y)
            // result: (Not (Less16U y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess16U, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq32 x y)
            // result: (Not (Less32 y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess32, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq32U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq32U x y)
            // result: (Not (Less32U y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess32U, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq64 x y)
            // result: (Not (Less64 y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess64, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq64U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq64U x y)
            // result: (Not (Less64U y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess64U, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq8 x y)
            // result: (Not (Less8 y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess8, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLeq8U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Leq8U x y)
            // result: (Not (Less8U y x))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpNot);
                var v0 = b.NewValue0(v.Pos, OpLess8U, typ.Bool);
                v0.AddArg2(y, x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLess16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less16 x y)
            // result: (SLT (SignExt16to64 x) (SignExt16to64 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SLT);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLess16U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less16U x y)
            // result: (SLTU (ZeroExt16to64 x) (ZeroExt16to64 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SLTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLess32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less32 x y)
            // result: (SLT (SignExt32to64 x) (SignExt32to64 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SLT);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLess32U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less32U x y)
            // result: (SLTU (ZeroExt32to64 x) (ZeroExt32to64 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SLTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLess8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less8 x y)
            // result: (SLT (SignExt8to64 x) (SignExt8to64 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SLT);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLess8U(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Less8U x y)
            // result: (SLTU (ZeroExt8to64 x) (ZeroExt8to64 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SLTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLoad(ptr<Value> _addr_v)
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

                v.reset(OpRISCV64MOVBUload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: ( is8BitInt(t) && isSigned(t))
            // result: (MOVBload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: ( is8BitInt(t) && isSigned(t))
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

                v.reset(OpRISCV64MOVBload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: ( is8BitInt(t) && !isSigned(t))
            // result: (MOVBUload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: ( is8BitInt(t) && !isSigned(t))
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

                v.reset(OpRISCV64MOVBUload);
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

                v.reset(OpRISCV64MOVHload);
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

                v.reset(OpRISCV64MOVHUload);
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

                v.reset(OpRISCV64MOVWload);
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

                v.reset(OpRISCV64MOVWUload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: (is64BitInt(t) || isPtr(t))
            // result: (MOVDload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is64BitInt(t) || isPtr(t))
            // result: (MOVDload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is64BitInt(t) || isPtr(t)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: is32BitFloat(t)
            // result: (FMOVWload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is32BitFloat(t)
            // result: (FMOVWload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is32BitFloat(t)))
                {
                    break;
                }

                v.reset(OpRISCV64FMOVWload);
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (Load <t> ptr mem)
            // cond: is64BitFloat(t)
            // result: (FMOVDload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is64BitFloat(t)
            // result: (FMOVDload ptr mem)
            while (true)
            {
                t = v.Type;
                ptr = v_0;
                mem = v_1;
                if (!(is64BitFloat(t)))
                {
                    break;
                }

                v.reset(OpRISCV64FMOVDload);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpLocalAddr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (LocalAddr {sym} base _)
            // result: (MOVaddr {sym} base)
            while (true)
            {
                var sym = auxToSym(v.Aux);
                var @base = v_0;
                v.reset(OpRISCV64MOVaddr);
                v.Aux = symToAux(sym);
                v.AddArg(base);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh16x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x16 <t> x y)
            // result: (AND (SLL <t> x y) (Neg16 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg16, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh16x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x32 <t> x y)
            // result: (AND (SLL <t> x y) (Neg16 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg16, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh16x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Lsh16x64 <t> x y)
            // result: (AND (SLL <t> x y) (Neg16 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg16, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh16x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh16x8 <t> x y)
            // result: (AND (SLL <t> x y) (Neg16 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg16, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh32x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x16 <t> x y)
            // result: (AND (SLL <t> x y) (Neg32 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg32, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh32x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x32 <t> x y)
            // result: (AND (SLL <t> x y) (Neg32 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg32, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh32x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Lsh32x64 <t> x y)
            // result: (AND (SLL <t> x y) (Neg32 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg32, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh32x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh32x8 <t> x y)
            // result: (AND (SLL <t> x y) (Neg32 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg32, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh64x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x16 <t> x y)
            // result: (AND (SLL <t> x y) (Neg64 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh64x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x32 <t> x y)
            // result: (AND (SLL <t> x y) (Neg64 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh64x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Lsh64x64 <t> x y)
            // result: (AND (SLL <t> x y) (Neg64 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh64x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh64x8 <t> x y)
            // result: (AND (SLL <t> x y) (Neg64 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh8x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x16 <t> x y)
            // result: (AND (SLL <t> x y) (Neg8 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg8, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh8x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x32 <t> x y)
            // result: (AND (SLL <t> x y) (Neg8 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg8, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh8x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Lsh8x64 <t> x y)
            // result: (AND (SLL <t> x y) (Neg8 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg8, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpLsh8x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Lsh8x8 <t> x y)
            // result: (AND (SLL <t> x y) (Neg8 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg8, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpMod16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod16 x y [false])
            // result: (REMW (SignExt16to32 x) (SignExt16to32 y))
            while (true)
            {
                if (auxIntToBool(v.AuxInt) != false)
                {
                    break;
                }

                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64REMW);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpMod16u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod16u x y)
            // result: (REMUW (ZeroExt16to32 x) (ZeroExt16to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64REMUW);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpMod32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Mod32 x y [false])
            // result: (REMW x y)
            while (true)
            {
                if (auxIntToBool(v.AuxInt) != false)
                {
                    break;
                }

                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64REMW);
                v.AddArg2(x, y);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpMod64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (Mod64 x y [false])
            // result: (REM x y)
            while (true)
            {
                if (auxIntToBool(v.AuxInt) != false)
                {
                    break;
                }

                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64REM);
                v.AddArg2(x, y);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpMod8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod8 x y)
            // result: (REMW (SignExt8to32 x) (SignExt8to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64REMW);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpMod8u(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mod8u x y)
            // result: (REMUW (ZeroExt8to32 x) (ZeroExt8to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64REMUW);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpMove(ptr<Value> _addr_v)
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
                v.reset(OpRISCV64MOVBstore);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MOVBload, typ.Int8);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [2] dst src mem)
            // result: (MOVHstore dst (MOVHload src mem) mem)
 
            // match: (Move [2] dst src mem)
            // result: (MOVHstore dst (MOVHload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 2L)
                {
                    break;
                }

                dst = v_0;
                src = v_1;
                mem = v_2;
                v.reset(OpRISCV64MOVHstore);
                v0 = b.NewValue0(v.Pos, OpRISCV64MOVHload, typ.Int16);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [4] dst src mem)
            // result: (MOVWstore dst (MOVWload src mem) mem)
 
            // match: (Move [4] dst src mem)
            // result: (MOVWstore dst (MOVWload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                dst = v_0;
                src = v_1;
                mem = v_2;
                v.reset(OpRISCV64MOVWstore);
                v0 = b.NewValue0(v.Pos, OpRISCV64MOVWload, typ.Int32);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [8] dst src mem)
            // result: (MOVDstore dst (MOVDload src mem) mem)
 
            // match: (Move [8] dst src mem)
            // result: (MOVDstore dst (MOVDload src mem) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                dst = v_0;
                src = v_1;
                mem = v_2;
                v.reset(OpRISCV64MOVDstore);
                v0 = b.NewValue0(v.Pos, OpRISCV64MOVDload, typ.Int64);
                v0.AddArg2(src, mem);
                v.AddArg3(dst, v0, mem);
                return true;

            } 
            // match: (Move [s] {t} dst src mem)
            // cond: (s <= 16 || logLargeCopy(v, s))
            // result: (LoweredMove [t.Alignment()] dst src (ADDI <src.Type> [s-moveSize(t.Alignment(), config)] src) mem)
 
            // match: (Move [s] {t} dst src mem)
            // cond: (s <= 16 || logLargeCopy(v, s))
            // result: (LoweredMove [t.Alignment()] dst src (ADDI <src.Type> [s-moveSize(t.Alignment(), config)] src) mem)
            while (true)
            {
                var s = auxIntToInt64(v.AuxInt);
                var t = auxToType(v.Aux);
                dst = v_0;
                src = v_1;
                mem = v_2;
                if (!(s <= 16L || logLargeCopy(v, s)))
                {
                    break;
                }

                v.reset(OpRISCV64LoweredMove);
                v.AuxInt = int64ToAuxInt(t.Alignment());
                v0 = b.NewValue0(v.Pos, OpRISCV64ADDI, src.Type);
                v0.AuxInt = int64ToAuxInt(s - moveSize(t.Alignment(), config));
                v0.AddArg(src);
                v.AddArg4(dst, src, v0, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpMul16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mul16 x y)
            // result: (MULW (SignExt16to32 x) (SignExt16to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64MULW);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpMul8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Mul8 x y)
            // result: (MULW (SignExt8to32 x) (SignExt8to32 y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64MULW);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpNeq16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Neq16 x y)
            // result: (SNEZ (ZeroExt16to64 (SUB <x.Type> x y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SNEZ);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v1.AddArg2(x, y);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpNeq32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Neq32 x y)
            // result: (SNEZ (SUBW <x.Type> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SNEZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SUBW, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpNeq64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Neq64 x y)
            // result: (SNEZ (SUB <x.Type> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SNEZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpNeq8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Neq8 x y)
            // result: (SNEZ (ZeroExt8to64 (SUB <x.Type> x y)))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SNEZ);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v1.AddArg2(x, y);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpNeqPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (NeqPtr x y)
            // result: (SNEZ (SUB <x.Type> x y))
            while (true)
            {
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SNEZ);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SUB, x.Type);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpOffPtr(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (OffPtr [off] ptr:(SP))
            // cond: is32Bit(off)
            // result: (MOVaddr [int32(off)] ptr)
            while (true)
            {
                var off = auxIntToInt64(v.AuxInt);
                var ptr = v_0;
                if (ptr.Op != OpSP || !(is32Bit(off)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVaddr);
                v.AuxInt = int32ToAuxInt(int32(off));
                v.AddArg(ptr);
                return true;

            } 
            // match: (OffPtr [off] ptr)
            // cond: is32Bit(off)
            // result: (ADDI [off] ptr)
 
            // match: (OffPtr [off] ptr)
            // cond: is32Bit(off)
            // result: (ADDI [off] ptr)
            while (true)
            {
                off = auxIntToInt64(v.AuxInt);
                ptr = v_0;
                if (!(is32Bit(off)))
                {
                    break;
                }

                v.reset(OpRISCV64ADDI);
                v.AuxInt = int64ToAuxInt(off);
                v.AddArg(ptr);
                return true;

            } 
            // match: (OffPtr [off] ptr)
            // result: (ADD (MOVDconst [off]) ptr)
 
            // match: (OffPtr [off] ptr)
            // result: (ADD (MOVDconst [off]) ptr)
            while (true)
            {
                off = auxIntToInt64(v.AuxInt);
                ptr = v_0;
                v.reset(OpRISCV64ADD);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v0.AuxInt = int64ToAuxInt(off);
                v.AddArg2(v0, ptr);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpPanicBounds(ptr<Value> _addr_v)
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

                v.reset(OpRISCV64LoweredPanicBoundsA);
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

                v.reset(OpRISCV64LoweredPanicBoundsB);
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

                v.reset(OpRISCV64LoweredPanicBoundsC);
                v.AuxInt = int64ToAuxInt(kind);
                v.AddArg3(x, y, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64ADD(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (ADD (MOVBconst [val]) x)
            // result: (ADDI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVBconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var val = auxIntToInt8(v_0.AuxInt);
                        var x = v_1;
                        v.reset(OpRISCV64ADDI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (ADD (MOVHconst [val]) x)
            // result: (ADDI [int64(val)] x)
 
            // match: (ADD (MOVHconst [val]) x)
            // result: (ADDI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVHconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt16(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64ADDI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (ADD (MOVWconst [val]) x)
            // result: (ADDI [int64(val)] x)
 
            // match: (ADD (MOVWconst [val]) x)
            // result: (ADDI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVWconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt32(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64ADDI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (ADD (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (ADDI [val] x)
 
            // match: (ADD (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (ADDI [val] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVDconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt64(v_0.AuxInt);
                        x = v_1;
                        if (!(is32Bit(val)))
                        {
                            continue;
                        }

                        v.reset(OpRISCV64ADDI);
                        v.AuxInt = int64ToAuxInt(val);
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64ADDI(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (ADDI [c] (MOVaddr [d] {s} x))
            // cond: is32Bit(c+int64(d))
            // result: (MOVaddr [int32(c)+d] {s} x)
            while (true)
            {
                var c = auxIntToInt64(v.AuxInt);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var d = auxIntToInt32(v_0.AuxInt);
                var s = auxToSym(v_0.Aux);
                var x = v_0.Args[0L];
                if (!(is32Bit(c + int64(d))))
                {
                    break;
                }

                v.reset(OpRISCV64MOVaddr);
                v.AuxInt = int32ToAuxInt(int32(c) + d);
                v.Aux = symToAux(s);
                v.AddArg(x);
                return true;

            } 
            // match: (ADDI [0] x)
            // result: x
 
            // match: (ADDI [0] x)
            // result: x
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 0L)
                {
                    break;
                }

                x = v_0;
                v.copyOf(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64AND(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (AND (MOVBconst [val]) x)
            // result: (ANDI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVBconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var val = auxIntToInt8(v_0.AuxInt);
                        var x = v_1;
                        v.reset(OpRISCV64ANDI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (AND (MOVHconst [val]) x)
            // result: (ANDI [int64(val)] x)
 
            // match: (AND (MOVHconst [val]) x)
            // result: (ANDI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVHconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt16(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64ANDI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (AND (MOVWconst [val]) x)
            // result: (ANDI [int64(val)] x)
 
            // match: (AND (MOVWconst [val]) x)
            // result: (ANDI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVWconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt32(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64ANDI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (AND (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (ANDI [val] x)
 
            // match: (AND (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (ANDI [val] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVDconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt64(v_0.AuxInt);
                        x = v_1;
                        if (!(is32Bit(val)))
                        {
                            continue;
                        }

                        v.reset(OpRISCV64ANDI);
                        v.AuxInt = int64ToAuxInt(val);
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVBUload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBUload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVBUload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBUload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVBUload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBUload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVBUload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBUload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBUload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVBload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVBload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVBload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVBload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVBstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBstore [off1] {sym1} (MOVaddr [off2] {sym2} base) val mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVBstore [off1+off2] {mergeSymTyped(sym1,sym2)} base val mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBstore);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVBstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBstore [off1+int32(off2)] {sym} base val mem)
 
            // match: (MOVBstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBstore [off1+int32(off2)] {sym} base val mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBstore);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVBstore [off] {sym} ptr (MOVBconst [0]) mem)
            // result: (MOVBstorezero [off] {sym} ptr mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVBconst [0]) mem)
            // result: (MOVBstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                var ptr = v_0;
                if (v_1.Op != OpRISCV64MOVBconst || auxIntToInt8(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpRISCV64MOVBstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVBstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVBstorezero [off1] {sym1} (MOVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(int64(off1)+int64(off2))
            // result: (MOVBstorezero [off1+off2] {mergeSymTyped(sym1,sym2)} ptr mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(int64(off1) + int64(off2))))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBstorezero);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVBstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBstorezero [off1+int32(off2)] {sym} ptr mem)
 
            // match: (MOVBstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVBstorezero [off1+int32(off2)] {sym} ptr mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVBstorezero);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVDconst(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (MOVDconst <t> [c])
            // cond: !is32Bit(c) && int32(c) < 0
            // result: (ADD (SLLI <t> [32] (MOVDconst [c>>32+1])) (MOVDconst [int64(int32(c))]))
            while (true)
            {
                var t = v.Type;
                var c = auxIntToInt64(v.AuxInt);
                if (!(!is32Bit(c) && int32(c) < 0L))
                {
                    break;
                }

                v.reset(OpRISCV64ADD);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(32L);
                var v1 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c >> (int)(32L) + 1L);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(int64(int32(c)));
                v.AddArg2(v0, v2);
                return true;

            } 
            // match: (MOVDconst <t> [c])
            // cond: !is32Bit(c) && int32(c) >= 0
            // result: (ADD (SLLI <t> [32] (MOVDconst [c>>32+0])) (MOVDconst [int64(int32(c))]))
 
            // match: (MOVDconst <t> [c])
            // cond: !is32Bit(c) && int32(c) >= 0
            // result: (ADD (SLLI <t> [32] (MOVDconst [c>>32+0])) (MOVDconst [int64(int32(c))]))
            while (true)
            {
                t = v.Type;
                c = auxIntToInt64(v.AuxInt);
                if (!(!is32Bit(c) && int32(c) >= 0L))
                {
                    break;
                }

                v.reset(OpRISCV64ADD);
                v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(32L);
                v1 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c >> (int)(32L) + 0L);
                v0.AddArg(v1);
                v2 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v2.AuxInt = int64ToAuxInt(int64(int32(c)));
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVDload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVDload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVDload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVDload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVDload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVDload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVDload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVDstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVDstore [off1] {sym1} (MOVaddr [off2] {sym2} base) val mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVDstore [off1+off2] {mergeSymTyped(sym1,sym2)} base val mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDstore);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVDstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVDstore [off1+int32(off2)] {sym} base val mem)
 
            // match: (MOVDstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVDstore [off1+int32(off2)] {sym} base val mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDstore);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVDstore [off] {sym} ptr (MOVDconst [0]) mem)
            // result: (MOVDstorezero [off] {sym} ptr mem)
 
            // match: (MOVDstore [off] {sym} ptr (MOVDconst [0]) mem)
            // result: (MOVDstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                var ptr = v_0;
                if (v_1.Op != OpRISCV64MOVDconst || auxIntToInt64(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpRISCV64MOVDstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVDstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVDstorezero [off1] {sym1} (MOVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(int64(off1)+int64(off2))
            // result: (MOVDstorezero [off1+off2] {mergeSymTyped(sym1,sym2)} ptr mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(int64(off1) + int64(off2))))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDstorezero);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVDstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVDstorezero [off1+int32(off2)] {sym} ptr mem)
 
            // match: (MOVDstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVDstorezero [off1+int32(off2)] {sym} ptr mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVDstorezero);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVHUload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHUload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVHUload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHUload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVHUload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHUload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVHUload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHUload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHUload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVHload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVHload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVHload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVHload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVHstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHstore [off1] {sym1} (MOVaddr [off2] {sym2} base) val mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVHstore [off1+off2] {mergeSymTyped(sym1,sym2)} base val mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHstore);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVHstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHstore [off1+int32(off2)] {sym} base val mem)
 
            // match: (MOVHstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHstore [off1+int32(off2)] {sym} base val mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHstore);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVHstore [off] {sym} ptr (MOVHconst [0]) mem)
            // result: (MOVHstorezero [off] {sym} ptr mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVHconst [0]) mem)
            // result: (MOVHstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                var ptr = v_0;
                if (v_1.Op != OpRISCV64MOVHconst || auxIntToInt16(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpRISCV64MOVHstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVHstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVHstorezero [off1] {sym1} (MOVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(int64(off1)+int64(off2))
            // result: (MOVHstorezero [off1+off2] {mergeSymTyped(sym1,sym2)} ptr mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(int64(off1) + int64(off2))))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHstorezero);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVHstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHstorezero [off1+int32(off2)] {sym} ptr mem)
 
            // match: (MOVHstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVHstorezero [off1+int32(off2)] {sym} ptr mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVHstorezero);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVWUload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWUload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVWUload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWUload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVWUload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWUload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVWUload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWUload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWUload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVWload(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWload [off1] {sym1} (MOVaddr [off2] {sym2} base) mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVWload [off1+off2] {mergeSymTyped(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var mem = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWload);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(base, mem);
                return true;

            } 
            // match: (MOVWload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWload [off1+int32(off2)] {sym} base mem)
 
            // match: (MOVWload [off1] {sym} (ADDI [off2] base) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWload [off1+int32(off2)] {sym} base mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWload);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(base, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVWstore(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_2 = v.Args[2L];
            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWstore [off1] {sym1} (MOVaddr [off2] {sym2} base) val mem)
            // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
            // result: (MOVWstore [off1+off2] {mergeSymTyped(sym1,sym2)} base val mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var @base = v_0.Args[0L];
                var val = v_1;
                var mem = v_2;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWstore);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVWstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWstore [off1+int32(off2)] {sym} base val mem)
 
            // match: (MOVWstore [off1] {sym} (ADDI [off2] base) val mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWstore [off1+int32(off2)] {sym} base val mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                @base = v_0.Args[0L];
                val = v_1;
                mem = v_2;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWstore);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg3(base, val, mem);
                return true;

            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWconst [0]) mem)
            // result: (MOVWstorezero [off] {sym} ptr mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWconst [0]) mem)
            // result: (MOVWstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = auxIntToInt32(v.AuxInt);
                sym = auxToSym(v.Aux);
                var ptr = v_0;
                if (v_1.Op != OpRISCV64MOVWconst || auxIntToInt32(v_1.AuxInt) != 0L)
                {
                    break;
                }

                mem = v_2;
                v.reset(OpRISCV64MOVWstorezero);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64MOVWstorezero(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (MOVWstorezero [off1] {sym1} (MOVaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2) && is32Bit(int64(off1)+int64(off2))
            // result: (MOVWstorezero [off1+off2] {mergeSymTyped(sym1,sym2)} ptr mem)
            while (true)
            {
                var off1 = auxIntToInt32(v.AuxInt);
                var sym1 = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64MOVaddr)
                {
                    break;
                }

                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                var ptr = v_0.Args[0L];
                var mem = v_1;
                if (!(canMergeSym(sym1, sym2) && is32Bit(int64(off1) + int64(off2))))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWstorezero);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSymTyped(sym1, sym2));
                v.AddArg2(ptr, mem);
                return true;

            } 
            // match: (MOVWstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWstorezero [off1+int32(off2)] {sym} ptr mem)
 
            // match: (MOVWstorezero [off1] {sym} (ADDI [off2] ptr) mem)
            // cond: is32Bit(int64(off1)+off2)
            // result: (MOVWstorezero [off1+int32(off2)] {sym} ptr mem)
            while (true)
            {
                off1 = auxIntToInt32(v.AuxInt);
                var sym = auxToSym(v.Aux);
                if (v_0.Op != OpRISCV64ADDI)
                {
                    break;
                }

                off2 = auxIntToInt64(v_0.AuxInt);
                ptr = v_0.Args[0L];
                mem = v_1;
                if (!(is32Bit(int64(off1) + off2)))
                {
                    break;
                }

                v.reset(OpRISCV64MOVWstorezero);
                v.AuxInt = int32ToAuxInt(off1 + int32(off2));
                v.Aux = symToAux(sym);
                v.AddArg2(ptr, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64OR(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (OR (MOVBconst [val]) x)
            // result: (ORI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVBconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var val = auxIntToInt8(v_0.AuxInt);
                        var x = v_1;
                        v.reset(OpRISCV64ORI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (OR (MOVHconst [val]) x)
            // result: (ORI [int64(val)] x)
 
            // match: (OR (MOVHconst [val]) x)
            // result: (ORI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVHconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt16(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64ORI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (OR (MOVWconst [val]) x)
            // result: (ORI [int64(val)] x)
 
            // match: (OR (MOVWconst [val]) x)
            // result: (ORI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVWconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt32(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64ORI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (OR (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (ORI [val] x)
 
            // match: (OR (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (ORI [val] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVDconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt64(v_0.AuxInt);
                        x = v_1;
                        if (!(is32Bit(val)))
                        {
                            continue;
                        }

                        v.reset(OpRISCV64ORI);
                        v.AuxInt = int64ToAuxInt(val);
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64SLL(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SLL x (MOVBconst [val]))
            // result: (SLLI [int64(val&63)] x)
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVBconst)
                {
                    break;
                }

                var val = auxIntToInt8(v_1.AuxInt);
                v.reset(OpRISCV64SLLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SLL x (MOVHconst [val]))
            // result: (SLLI [int64(val&63)] x)
 
            // match: (SLL x (MOVHconst [val]))
            // result: (SLLI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVHconst)
                {
                    break;
                }

                val = auxIntToInt16(v_1.AuxInt);
                v.reset(OpRISCV64SLLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SLL x (MOVWconst [val]))
            // result: (SLLI [int64(val&63)] x)
 
            // match: (SLL x (MOVWconst [val]))
            // result: (SLLI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst)
                {
                    break;
                }

                val = auxIntToInt32(v_1.AuxInt);
                v.reset(OpRISCV64SLLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SLL x (MOVDconst [val]))
            // result: (SLLI [int64(val&63)] x)
 
            // match: (SLL x (MOVDconst [val]))
            // result: (SLLI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVDconst)
                {
                    break;
                }

                val = auxIntToInt64(v_1.AuxInt);
                v.reset(OpRISCV64SLLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64SRA(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SRA x (MOVBconst [val]))
            // result: (SRAI [int64(val&63)] x)
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVBconst)
                {
                    break;
                }

                var val = auxIntToInt8(v_1.AuxInt);
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SRA x (MOVHconst [val]))
            // result: (SRAI [int64(val&63)] x)
 
            // match: (SRA x (MOVHconst [val]))
            // result: (SRAI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVHconst)
                {
                    break;
                }

                val = auxIntToInt16(v_1.AuxInt);
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SRA x (MOVWconst [val]))
            // result: (SRAI [int64(val&63)] x)
 
            // match: (SRA x (MOVWconst [val]))
            // result: (SRAI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst)
                {
                    break;
                }

                val = auxIntToInt32(v_1.AuxInt);
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SRA x (MOVDconst [val]))
            // result: (SRAI [int64(val&63)] x)
 
            // match: (SRA x (MOVDconst [val]))
            // result: (SRAI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVDconst)
                {
                    break;
                }

                val = auxIntToInt64(v_1.AuxInt);
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64SRL(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SRL x (MOVBconst [val]))
            // result: (SRLI [int64(val&63)] x)
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVBconst)
                {
                    break;
                }

                var val = auxIntToInt8(v_1.AuxInt);
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SRL x (MOVHconst [val]))
            // result: (SRLI [int64(val&63)] x)
 
            // match: (SRL x (MOVHconst [val]))
            // result: (SRLI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVHconst)
                {
                    break;
                }

                val = auxIntToInt16(v_1.AuxInt);
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SRL x (MOVWconst [val]))
            // result: (SRLI [int64(val&63)] x)
 
            // match: (SRL x (MOVWconst [val]))
            // result: (SRLI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst)
                {
                    break;
                }

                val = auxIntToInt32(v_1.AuxInt);
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            } 
            // match: (SRL x (MOVDconst [val]))
            // result: (SRLI [int64(val&63)] x)
 
            // match: (SRL x (MOVDconst [val]))
            // result: (SRLI [int64(val&63)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVDconst)
                {
                    break;
                }

                val = auxIntToInt64(v_1.AuxInt);
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(int64(val & 63L));
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64SUB(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SUB x (MOVBconst [val]))
            // result: (ADDI [-int64(val)] x)
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVBconst)
                {
                    break;
                }

                var val = auxIntToInt8(v_1.AuxInt);
                v.reset(OpRISCV64ADDI);
                v.AuxInt = int64ToAuxInt(-int64(val));
                v.AddArg(x);
                return true;

            } 
            // match: (SUB x (MOVHconst [val]))
            // result: (ADDI [-int64(val)] x)
 
            // match: (SUB x (MOVHconst [val]))
            // result: (ADDI [-int64(val)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVHconst)
                {
                    break;
                }

                val = auxIntToInt16(v_1.AuxInt);
                v.reset(OpRISCV64ADDI);
                v.AuxInt = int64ToAuxInt(-int64(val));
                v.AddArg(x);
                return true;

            } 
            // match: (SUB x (MOVWconst [val]))
            // cond: is32Bit(-int64(val))
            // result: (ADDI [-int64(val)] x)
 
            // match: (SUB x (MOVWconst [val]))
            // cond: is32Bit(-int64(val))
            // result: (ADDI [-int64(val)] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst)
                {
                    break;
                }

                val = auxIntToInt32(v_1.AuxInt);
                if (!(is32Bit(-int64(val))))
                {
                    break;
                }

                v.reset(OpRISCV64ADDI);
                v.AuxInt = int64ToAuxInt(-int64(val));
                v.AddArg(x);
                return true;

            } 
            // match: (SUB x (MOVDconst [val]))
            // cond: is32Bit(-val)
            // result: (ADDI [-val] x)
 
            // match: (SUB x (MOVDconst [val]))
            // cond: is32Bit(-val)
            // result: (ADDI [-val] x)
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVDconst)
                {
                    break;
                }

                val = auxIntToInt64(v_1.AuxInt);
                if (!(is32Bit(-val)))
                {
                    break;
                }

                v.reset(OpRISCV64ADDI);
                v.AuxInt = int64ToAuxInt(-val);
                v.AddArg(x);
                return true;

            } 
            // match: (SUB x (MOVBconst [0]))
            // result: x
 
            // match: (SUB x (MOVBconst [0]))
            // result: x
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVBconst || auxIntToInt8(v_1.AuxInt) != 0L)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            } 
            // match: (SUB x (MOVHconst [0]))
            // result: x
 
            // match: (SUB x (MOVHconst [0]))
            // result: x
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVHconst || auxIntToInt16(v_1.AuxInt) != 0L)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            } 
            // match: (SUB x (MOVWconst [0]))
            // result: x
 
            // match: (SUB x (MOVWconst [0]))
            // result: x
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst || auxIntToInt32(v_1.AuxInt) != 0L)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            } 
            // match: (SUB x (MOVDconst [0]))
            // result: x
 
            // match: (SUB x (MOVDconst [0]))
            // result: x
            while (true)
            {
                x = v_0;
                if (v_1.Op != OpRISCV64MOVDconst || auxIntToInt64(v_1.AuxInt) != 0L)
                {
                    break;
                }

                v.copyOf(x);
                return true;

            } 
            // match: (SUB (MOVBconst [0]) x)
            // result: (NEG x)
 
            // match: (SUB (MOVBconst [0]) x)
            // result: (NEG x)
            while (true)
            {
                if (v_0.Op != OpRISCV64MOVBconst || auxIntToInt8(v_0.AuxInt) != 0L)
                {
                    break;
                }

                x = v_1;
                v.reset(OpRISCV64NEG);
                v.AddArg(x);
                return true;

            } 
            // match: (SUB (MOVHconst [0]) x)
            // result: (NEG x)
 
            // match: (SUB (MOVHconst [0]) x)
            // result: (NEG x)
            while (true)
            {
                if (v_0.Op != OpRISCV64MOVHconst || auxIntToInt16(v_0.AuxInt) != 0L)
                {
                    break;
                }

                x = v_1;
                v.reset(OpRISCV64NEG);
                v.AddArg(x);
                return true;

            } 
            // match: (SUB (MOVWconst [0]) x)
            // result: (NEG x)
 
            // match: (SUB (MOVWconst [0]) x)
            // result: (NEG x)
            while (true)
            {
                if (v_0.Op != OpRISCV64MOVWconst || auxIntToInt32(v_0.AuxInt) != 0L)
                {
                    break;
                }

                x = v_1;
                v.reset(OpRISCV64NEG);
                v.AddArg(x);
                return true;

            } 
            // match: (SUB (MOVDconst [0]) x)
            // result: (NEG x)
 
            // match: (SUB (MOVDconst [0]) x)
            // result: (NEG x)
            while (true)
            {
                if (v_0.Op != OpRISCV64MOVDconst || auxIntToInt64(v_0.AuxInt) != 0L)
                {
                    break;
                }

                x = v_1;
                v.reset(OpRISCV64NEG);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64SUBW(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (SUBW x (MOVWconst [0]))
            // result: (ADDIW [0] x)
            while (true)
            {
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst || auxIntToInt32(v_1.AuxInt) != 0L)
                {
                    break;
                }

                v.reset(OpRISCV64ADDIW);
                v.AuxInt = int64ToAuxInt(0L);
                v.AddArg(x);
                return true;

            } 
            // match: (SUBW (MOVDconst [0]) x)
            // result: (NEGW x)
 
            // match: (SUBW (MOVDconst [0]) x)
            // result: (NEGW x)
            while (true)
            {
                if (v_0.Op != OpRISCV64MOVDconst || auxIntToInt64(v_0.AuxInt) != 0L)
                {
                    break;
                }

                x = v_1;
                v.reset(OpRISCV64NEGW);
                v.AddArg(x);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRISCV64XOR(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L]; 
            // match: (XOR (MOVBconst [val]) x)
            // result: (XORI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    long _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVBconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        var val = auxIntToInt8(v_0.AuxInt);
                        var x = v_1;
                        v.reset(OpRISCV64XORI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (XOR (MOVHconst [val]) x)
            // result: (XORI [int64(val)] x)
 
            // match: (XOR (MOVHconst [val]) x)
            // result: (XORI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVHconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt16(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64XORI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (XOR (MOVWconst [val]) x)
            // result: (XORI [int64(val)] x)
 
            // match: (XOR (MOVWconst [val]) x)
            // result: (XORI [int64(val)] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVWconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt32(v_0.AuxInt);
                        x = v_1;
                        v.reset(OpRISCV64XORI);
                        v.AuxInt = int64ToAuxInt(int64(val));
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            } 
            // match: (XOR (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (XORI [val] x)
 
            // match: (XOR (MOVDconst [val]) x)
            // cond: is32Bit(val)
            // result: (XORI [val] x)
            while (true)
            {
                {
                    long _i0__prev2 = _i0;

                    _i0 = 0L;

                    while (_i0 <= 1L)
                    {
                        if (v_0.Op != OpRISCV64MOVDconst)
                        {
                            continue;
                        _i0 = _i0 + 1L;
                    v_0 = v_1;
                    v_1 = v_0;
                        }

                        val = auxIntToInt64(v_0.AuxInt);
                        x = v_1;
                        if (!(is32Bit(val)))
                        {
                            continue;
                        }

                        v.reset(OpRISCV64XORI);
                        v.AuxInt = int64ToAuxInt(val);
                        v.AddArg(x);
                        return true;

                    }


                    _i0 = _i0__prev2;
                }
                break;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRotateLeft16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft16 <t> x (MOVHconst [c]))
            // result: (Or16 (Lsh16x64 <t> x (MOVHconst [c&15])) (Rsh16Ux64 <t> x (MOVHconst [-c&15])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVHconst)
                {
                    break;
                }

                var c = auxIntToInt16(v_1.AuxInt);
                v.reset(OpOr16);
                var v0 = b.NewValue0(v.Pos, OpLsh16x64, t);
                var v1 = b.NewValue0(v.Pos, OpRISCV64MOVHconst, typ.UInt16);
                v1.AuxInt = int16ToAuxInt(c & 15L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh16Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64MOVHconst, typ.UInt16);
                v3.AuxInt = int16ToAuxInt(-c & 15L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRotateLeft32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft32 <t> x (MOVWconst [c]))
            // result: (Or32 (Lsh32x64 <t> x (MOVWconst [c&31])) (Rsh32Ux64 <t> x (MOVWconst [-c&31])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVWconst)
                {
                    break;
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpOr32);
                var v0 = b.NewValue0(v.Pos, OpLsh32x64, t);
                var v1 = b.NewValue0(v.Pos, OpRISCV64MOVWconst, typ.UInt32);
                v1.AuxInt = int32ToAuxInt(c & 31L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh32Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64MOVWconst, typ.UInt32);
                v3.AuxInt = int32ToAuxInt(-c & 31L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRotateLeft64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft64 <t> x (MOVDconst [c]))
            // result: (Or64 (Lsh64x64 <t> x (MOVDconst [c&63])) (Rsh64Ux64 <t> x (MOVDconst [-c&63])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVDconst)
                {
                    break;
                }

                var c = auxIntToInt64(v_1.AuxInt);
                v.reset(OpOr64);
                var v0 = b.NewValue0(v.Pos, OpLsh64x64, t);
                var v1 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(c & 63L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh64Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v3.AuxInt = int64ToAuxInt(-c & 63L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRotateLeft8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (RotateLeft8 <t> x (MOVBconst [c]))
            // result: (Or8 (Lsh8x64 <t> x (MOVBconst [c&7])) (Rsh8Ux64 <t> x (MOVBconst [-c&7])))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                if (v_1.Op != OpRISCV64MOVBconst)
                {
                    break;
                }

                var c = auxIntToInt8(v_1.AuxInt);
                v.reset(OpOr8);
                var v0 = b.NewValue0(v.Pos, OpLsh8x64, t);
                var v1 = b.NewValue0(v.Pos, OpRISCV64MOVBconst, typ.UInt8);
                v1.AuxInt = int8ToAuxInt(c & 7L);
                v0.AddArg2(x, v1);
                var v2 = b.NewValue0(v.Pos, OpRsh8Ux64, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64MOVBconst, typ.UInt8);
                v3.AuxInt = int8ToAuxInt(-c & 7L);
                v2.AddArg2(x, v3);
                v.AddArg2(v0, v2);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpRsh16Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux16 <t> x y)
            // result: (AND (SRL <t> (ZeroExt16to64 x) y) (Neg16 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg16, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux32 <t> x y)
            // result: (AND (SRL <t> (ZeroExt16to64 x) y) (Neg16 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg16, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux64 <t> x y)
            // result: (AND (SRL <t> (ZeroExt16to64 x) y) (Neg16 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg16, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                v3.AddArg(y);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16Ux8 <t> x y)
            // result: (AND (SRL <t> (ZeroExt16to64 x) y) (Neg16 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg16, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x16 <t> x y)
            // result: (SRA <t> (SignExt16to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt16to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x32 <t> x y)
            // result: (SRA <t> (SignExt16to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt32to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x64 <t> x y)
            // result: (SRA <t> (SignExt16to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh16x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh16x8 <t> x y)
            // result: (SRA <t> (SignExt16to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt8to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux16 <t> x y)
            // result: (AND (SRL <t> (ZeroExt32to64 x) y) (Neg32 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg32, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux32 <t> x y)
            // result: (AND (SRL <t> (ZeroExt32to64 x) y) (Neg32 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg32, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux64 <t> x y)
            // result: (AND (SRL <t> (ZeroExt32to64 x) y) (Neg32 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg32, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                v3.AddArg(y);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32Ux8 <t> x y)
            // result: (AND (SRL <t> (ZeroExt32to64 x) y) (Neg32 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg32, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x16 <t> x y)
            // result: (SRA <t> (SignExt32to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt16to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x32 <t> x y)
            // result: (SRA <t> (SignExt32to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt32to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x64 <t> x y)
            // result: (SRA <t> (SignExt32to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh32x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh32x8 <t> x y)
            // result: (SRA <t> (SignExt32to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt8to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux16 <t> x y)
            // result: (AND (SRL <t> x y) (Neg64 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux32 <t> x y)
            // result: (AND (SRL <t> x y) (Neg64 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Rsh64Ux64 <t> x y)
            // result: (AND (SRL <t> x y) (Neg64 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64Ux8 <t> x y)
            // result: (AND (SRL <t> x y) (Neg64 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                v0.AddArg2(x, y);
                var v1 = b.NewValue0(v.Pos, OpNeg64, t);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x16 <t> x y)
            // result: (SRA <t> x (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt16to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v1 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v1.AuxInt = int64ToAuxInt(-1L);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg2(y, v1);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x32 <t> x y)
            // result: (SRA <t> x (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt32to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v1 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v1.AuxInt = int64ToAuxInt(-1L);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg2(y, v1);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Rsh64x64 <t> x y)
            // result: (SRA <t> x (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v1 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v1.AuxInt = int64ToAuxInt(-1L);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v2.AuxInt = int64ToAuxInt(64L);
                v2.AddArg(y);
                v1.AddArg(v2);
                v0.AddArg2(y, v1);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh64x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh64x8 <t> x y)
            // result: (SRA <t> x (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt8to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v1 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v1.AuxInt = int64ToAuxInt(-1L);
                var v2 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v2.AuxInt = int64ToAuxInt(64L);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg2(y, v1);
                v.AddArg2(x, v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8Ux16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux16 <t> x y)
            // result: (AND (SRL <t> (ZeroExt8to64 x) y) (Neg8 <t> (SLTIU <t> [64] (ZeroExt16to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg8, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8Ux32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux32 <t> x y)
            // result: (AND (SRL <t> (ZeroExt8to64 x) y) (Neg8 <t> (SLTIU <t> [64] (ZeroExt32to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg8, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8Ux64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux64 <t> x y)
            // result: (AND (SRL <t> (ZeroExt8to64 x) y) (Neg8 <t> (SLTIU <t> [64] y)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg8, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                v3.AddArg(y);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8Ux8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8Ux8 <t> x y)
            // result: (AND (SRL <t> (ZeroExt8to64 x) y) (Neg8 <t> (SLTIU <t> [64] (ZeroExt8to64 y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64AND);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg2(v1, y);
                var v2 = b.NewValue0(v.Pos, OpNeg8, t);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, t);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v.AddArg2(v0, v2);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8x16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x16 <t> x y)
            // result: (SRA <t> (SignExt8to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt16to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8x32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x32 <t> x y)
            // result: (SRA <t> (SignExt8to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt32to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8x64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x64 <t> x y)
            // result: (SRA <t> (SignExt8to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] y))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpRsh8x8(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_1 = v.Args[1L];
            var v_0 = v.Args[0L];
            var b = v.Block;
            var typ = _addr_b.Func.Config.Types; 
            // match: (Rsh8x8 <t> x y)
            // result: (SRA <t> (SignExt8to64 x) (OR <y.Type> y (ADDI <y.Type> [-1] (SLTIU <y.Type> [64] (ZeroExt8to64 y)))))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                var y = v_1;
                v.reset(OpRISCV64SRA);
                v.Type = t;
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpRISCV64OR, y.Type);
                var v2 = b.NewValue0(v.Pos, OpRISCV64ADDI, y.Type);
                v2.AuxInt = int64ToAuxInt(-1L);
                var v3 = b.NewValue0(v.Pos, OpRISCV64SLTIU, y.Type);
                v3.AuxInt = int64ToAuxInt(64L);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg2(y, v2);
                v.AddArg2(v0, v1);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSignExt16to32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (SignExt16to32 <t> x)
            // result: (SRAI [48] (SLLI <t> [48] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(48L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(48L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSignExt16to64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (SignExt16to64 <t> x)
            // result: (SRAI [48] (SLLI <t> [48] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(48L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(48L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSignExt32to64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L]; 
            // match: (SignExt32to64 <t> x)
            // result: (ADDIW [0] x)
            while (true)
            {
                var x = v_0;
                v.reset(OpRISCV64ADDIW);
                v.AuxInt = int64ToAuxInt(0L);
                v.AddArg(x);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSignExt8to16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (SignExt8to16 <t> x)
            // result: (SRAI [56] (SLLI <t> [56] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(56L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(56L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSignExt8to32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (SignExt8to32 <t> x)
            // result: (SRAI [56] (SLLI <t> [56] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(56L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(56L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSignExt8to64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (SignExt8to64 <t> x)
            // result: (SRAI [56] (SLLI <t> [56] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRAI);
                v.AuxInt = int64ToAuxInt(56L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(56L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpSlicemask(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (Slicemask <t> x)
            // result: (NOT (SRAI <t> [63] (ADDI <t> [-1] x)))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64NOT);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SRAI, t);
                v0.AuxInt = int64ToAuxInt(63L);
                var v1 = b.NewValue0(v.Pos, OpRISCV64ADDI, t);
                v1.AuxInt = int64ToAuxInt(-1L);
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpStore(ptr<Value> _addr_v)
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

                v.reset(OpRISCV64MOVBstore);
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

                v.reset(OpRISCV64MOVHstore);
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

                v.reset(OpRISCV64MOVWstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && !is64BitFloat(val.Type)
            // result: (MOVDstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && !is64BitFloat(val.Type)
            // result: (MOVDstore ptr val mem)
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

                v.reset(OpRISCV64MOVDstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 4 && is32BitFloat(val.Type)
            // result: (FMOVWstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 4 && is32BitFloat(val.Type)
            // result: (FMOVWstore ptr val mem)
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

                v.reset(OpRISCV64FMOVWstore);
                v.AddArg3(ptr, val, mem);
                return true;

            } 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && is64BitFloat(val.Type)
            // result: (FMOVDstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.Size() == 8 && is64BitFloat(val.Type)
            // result: (FMOVDstore ptr val mem)
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

                v.reset(OpRISCV64FMOVDstore);
                v.AddArg3(ptr, val, mem);
                return true;

            }

            return false;

        }
        private static bool rewriteValueRISCV64_OpZero(ptr<Value> _addr_v)
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
            // result: (MOVBstore ptr (MOVBconst) mem)
 
            // match: (Zero [1] ptr mem)
            // result: (MOVBstore ptr (MOVBconst) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 1L)
                {
                    break;
                }

                var ptr = v_0;
                mem = v_1;
                v.reset(OpRISCV64MOVBstore);
                var v0 = b.NewValue0(v.Pos, OpRISCV64MOVBconst, typ.UInt8);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [2] ptr mem)
            // result: (MOVHstore ptr (MOVHconst) mem)
 
            // match: (Zero [2] ptr mem)
            // result: (MOVHstore ptr (MOVHconst) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 2L)
                {
                    break;
                }

                ptr = v_0;
                mem = v_1;
                v.reset(OpRISCV64MOVHstore);
                v0 = b.NewValue0(v.Pos, OpRISCV64MOVHconst, typ.UInt16);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [4] ptr mem)
            // result: (MOVWstore ptr (MOVWconst) mem)
 
            // match: (Zero [4] ptr mem)
            // result: (MOVWstore ptr (MOVWconst) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 4L)
                {
                    break;
                }

                ptr = v_0;
                mem = v_1;
                v.reset(OpRISCV64MOVWstore);
                v0 = b.NewValue0(v.Pos, OpRISCV64MOVWconst, typ.UInt32);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [8] ptr mem)
            // result: (MOVDstore ptr (MOVDconst) mem)
 
            // match: (Zero [8] ptr mem)
            // result: (MOVDstore ptr (MOVDconst) mem)
            while (true)
            {
                if (auxIntToInt64(v.AuxInt) != 8L)
                {
                    break;
                }

                ptr = v_0;
                mem = v_1;
                v.reset(OpRISCV64MOVDstore);
                v0 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v.AddArg3(ptr, v0, mem);
                return true;

            } 
            // match: (Zero [s] {t} ptr mem)
            // result: (LoweredZero [t.Alignment()] ptr (ADD <ptr.Type> ptr (MOVDconst [s-moveSize(t.Alignment(), config)])) mem)
 
            // match: (Zero [s] {t} ptr mem)
            // result: (LoweredZero [t.Alignment()] ptr (ADD <ptr.Type> ptr (MOVDconst [s-moveSize(t.Alignment(), config)])) mem)
            while (true)
            {
                var s = auxIntToInt64(v.AuxInt);
                var t = auxToType(v.Aux);
                ptr = v_0;
                mem = v_1;
                v.reset(OpRISCV64LoweredZero);
                v.AuxInt = int64ToAuxInt(t.Alignment());
                v0 = b.NewValue0(v.Pos, OpRISCV64ADD, ptr.Type);
                var v1 = b.NewValue0(v.Pos, OpRISCV64MOVDconst, typ.UInt64);
                v1.AuxInt = int64ToAuxInt(s - moveSize(t.Alignment(), config));
                v0.AddArg2(ptr, v1);
                v.AddArg3(ptr, v0, mem);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpZeroExt16to32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (ZeroExt16to32 <t> x)
            // result: (SRLI [48] (SLLI <t> [48] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(48L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(48L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpZeroExt16to64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (ZeroExt16to64 <t> x)
            // result: (SRLI [48] (SLLI <t> [48] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(48L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(48L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpZeroExt32to64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (ZeroExt32to64 <t> x)
            // result: (SRLI [32] (SLLI <t> [32] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(32L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(32L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpZeroExt8to16(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (ZeroExt8to16 <t> x)
            // result: (SRLI [56] (SLLI <t> [56] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(56L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(56L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpZeroExt8to32(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (ZeroExt8to32 <t> x)
            // result: (SRLI [56] (SLLI <t> [56] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(56L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(56L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteValueRISCV64_OpZeroExt8to64(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            var v_0 = v.Args[0L];
            var b = v.Block; 
            // match: (ZeroExt8to64 <t> x)
            // result: (SRLI [56] (SLLI <t> [56] x))
            while (true)
            {
                var t = v.Type;
                var x = v_0;
                v.reset(OpRISCV64SRLI);
                v.AuxInt = int64ToAuxInt(56L);
                var v0 = b.NewValue0(v.Pos, OpRISCV64SLLI, t);
                v0.AuxInt = int64ToAuxInt(56L);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }


        }
        private static bool rewriteBlockRISCV64(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;


            if (b.Kind == BlockRISCV64BEQ) 
                // match: (BEQ (MOVDconst [0]) cond yes no)
                // result: (BEQZ cond yes no)
                while (b.Controls[0L].Op == OpRISCV64MOVDconst)
                {
                    var v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 0L)
                    {
                        break;
                    }

                    var cond = b.Controls[1L];
                    b.resetWithControl(BlockRISCV64BEQZ, cond);
                    return true;

                } 
                // match: (BEQ cond (MOVDconst [0]) yes no)
                // result: (BEQZ cond yes no)
 
                // match: (BEQ cond (MOVDconst [0]) yes no)
                // result: (BEQZ cond yes no)
                while (b.Controls[1L].Op == OpRISCV64MOVDconst)
                {
                    cond = b.Controls[0L];
                    var v_1 = b.Controls[1L];
                    if (auxIntToInt64(v_1.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.resetWithControl(BlockRISCV64BEQZ, cond);
                    return true;

                }
            else if (b.Kind == BlockRISCV64BEQZ) 
                // match: (BEQZ (SEQZ x) yes no)
                // result: (BNEZ x yes no)
                while (b.Controls[0L].Op == OpRISCV64SEQZ)
                {
                    v_0 = b.Controls[0L];
                    var x = v_0.Args[0L];
                    b.resetWithControl(BlockRISCV64BNEZ, x);
                    return true;
                } 
                // match: (BEQZ (SNEZ x) yes no)
                // result: (BEQZ x yes no)
 
                // match: (BEQZ (SNEZ x) yes no)
                // result: (BEQZ x yes no)
                while (b.Controls[0L].Op == OpRISCV64SNEZ)
                {
                    v_0 = b.Controls[0L];
                    x = v_0.Args[0L];
                    b.resetWithControl(BlockRISCV64BEQZ, x);
                    return true;
                } 
                // match: (BEQZ (SUB x y) yes no)
                // result: (BEQ x y yes no)
 
                // match: (BEQZ (SUB x y) yes no)
                // result: (BEQ x y yes no)
                while (b.Controls[0L].Op == OpRISCV64SUB)
                {
                    v_0 = b.Controls[0L];
                    var y = v_0.Args[1L];
                    x = v_0.Args[0L];
                    b.resetWithControl2(BlockRISCV64BEQ, x, y);
                    return true;
                } 
                // match: (BEQZ (SLT x y) yes no)
                // result: (BGE x y yes no)
 
                // match: (BEQZ (SLT x y) yes no)
                // result: (BGE x y yes no)
                while (b.Controls[0L].Op == OpRISCV64SLT)
                {
                    v_0 = b.Controls[0L];
                    y = v_0.Args[1L];
                    x = v_0.Args[0L];
                    b.resetWithControl2(BlockRISCV64BGE, x, y);
                    return true;
                } 
                // match: (BEQZ (SLTU x y) yes no)
                // result: (BGEU x y yes no)
 
                // match: (BEQZ (SLTU x y) yes no)
                // result: (BGEU x y yes no)
                while (b.Controls[0L].Op == OpRISCV64SLTU)
                {
                    v_0 = b.Controls[0L];
                    y = v_0.Args[1L];
                    x = v_0.Args[0L];
                    b.resetWithControl2(BlockRISCV64BGEU, x, y);
                    return true;
                }
            else if (b.Kind == BlockRISCV64BNE) 
                // match: (BNE (MOVDconst [0]) cond yes no)
                // result: (BNEZ cond yes no)
                while (b.Controls[0L].Op == OpRISCV64MOVDconst)
                {
                    v_0 = b.Controls[0L];
                    if (auxIntToInt64(v_0.AuxInt) != 0L)
                    {
                        break;
                    }

                    cond = b.Controls[1L];
                    b.resetWithControl(BlockRISCV64BNEZ, cond);
                    return true;

                } 
                // match: (BNE cond (MOVDconst [0]) yes no)
                // result: (BNEZ cond yes no)
 
                // match: (BNE cond (MOVDconst [0]) yes no)
                // result: (BNEZ cond yes no)
                while (b.Controls[1L].Op == OpRISCV64MOVDconst)
                {
                    cond = b.Controls[0L];
                    v_1 = b.Controls[1L];
                    if (auxIntToInt64(v_1.AuxInt) != 0L)
                    {
                        break;
                    }

                    b.resetWithControl(BlockRISCV64BNEZ, cond);
                    return true;

                }
            else if (b.Kind == BlockRISCV64BNEZ) 
                // match: (BNEZ (SEQZ x) yes no)
                // result: (BEQZ x yes no)
                while (b.Controls[0L].Op == OpRISCV64SEQZ)
                {
                    v_0 = b.Controls[0L];
                    x = v_0.Args[0L];
                    b.resetWithControl(BlockRISCV64BEQZ, x);
                    return true;
                } 
                // match: (BNEZ (SNEZ x) yes no)
                // result: (BNEZ x yes no)
 
                // match: (BNEZ (SNEZ x) yes no)
                // result: (BNEZ x yes no)
                while (b.Controls[0L].Op == OpRISCV64SNEZ)
                {
                    v_0 = b.Controls[0L];
                    x = v_0.Args[0L];
                    b.resetWithControl(BlockRISCV64BNEZ, x);
                    return true;
                } 
                // match: (BNEZ (SUB x y) yes no)
                // result: (BNE x y yes no)
 
                // match: (BNEZ (SUB x y) yes no)
                // result: (BNE x y yes no)
                while (b.Controls[0L].Op == OpRISCV64SUB)
                {
                    v_0 = b.Controls[0L];
                    y = v_0.Args[1L];
                    x = v_0.Args[0L];
                    b.resetWithControl2(BlockRISCV64BNE, x, y);
                    return true;
                } 
                // match: (BNEZ (SLT x y) yes no)
                // result: (BLT x y yes no)
 
                // match: (BNEZ (SLT x y) yes no)
                // result: (BLT x y yes no)
                while (b.Controls[0L].Op == OpRISCV64SLT)
                {
                    v_0 = b.Controls[0L];
                    y = v_0.Args[1L];
                    x = v_0.Args[0L];
                    b.resetWithControl2(BlockRISCV64BLT, x, y);
                    return true;
                } 
                // match: (BNEZ (SLTU x y) yes no)
                // result: (BLTU x y yes no)
 
                // match: (BNEZ (SLTU x y) yes no)
                // result: (BLTU x y yes no)
                while (b.Controls[0L].Op == OpRISCV64SLTU)
                {
                    v_0 = b.Controls[0L];
                    y = v_0.Args[1L];
                    x = v_0.Args[0L];
                    b.resetWithControl2(BlockRISCV64BLTU, x, y);
                    return true;
                }
            else if (b.Kind == BlockIf) 
                // match: (If cond yes no)
                // result: (BNEZ cond yes no)
                while (true)
                {
                    cond = b.Controls[0L];
                    b.resetWithControl(BlockRISCV64BNEZ, cond);
                    return true;
                }
                        return false;

        }
    }
}}}}
