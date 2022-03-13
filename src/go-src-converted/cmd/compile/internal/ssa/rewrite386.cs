// Code generated from gen/386.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 13 06:03:53 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewrite386.go
namespace go.cmd.compile.@internal;

using math = math_package;
using types = cmd.compile.@internal.types_package;

public static partial class ssa_package {

private static bool rewriteValue386(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == Op386ADCL) 
        return rewriteValue386_Op386ADCL(_addr_v);
    else if (v.Op == Op386ADDL) 
        return rewriteValue386_Op386ADDL(_addr_v);
    else if (v.Op == Op386ADDLcarry) 
        return rewriteValue386_Op386ADDLcarry(_addr_v);
    else if (v.Op == Op386ADDLconst) 
        return rewriteValue386_Op386ADDLconst(_addr_v);
    else if (v.Op == Op386ADDLconstmodify) 
        return rewriteValue386_Op386ADDLconstmodify(_addr_v);
    else if (v.Op == Op386ADDLload) 
        return rewriteValue386_Op386ADDLload(_addr_v);
    else if (v.Op == Op386ADDLmodify) 
        return rewriteValue386_Op386ADDLmodify(_addr_v);
    else if (v.Op == Op386ADDSD) 
        return rewriteValue386_Op386ADDSD(_addr_v);
    else if (v.Op == Op386ADDSDload) 
        return rewriteValue386_Op386ADDSDload(_addr_v);
    else if (v.Op == Op386ADDSS) 
        return rewriteValue386_Op386ADDSS(_addr_v);
    else if (v.Op == Op386ADDSSload) 
        return rewriteValue386_Op386ADDSSload(_addr_v);
    else if (v.Op == Op386ANDL) 
        return rewriteValue386_Op386ANDL(_addr_v);
    else if (v.Op == Op386ANDLconst) 
        return rewriteValue386_Op386ANDLconst(_addr_v);
    else if (v.Op == Op386ANDLconstmodify) 
        return rewriteValue386_Op386ANDLconstmodify(_addr_v);
    else if (v.Op == Op386ANDLload) 
        return rewriteValue386_Op386ANDLload(_addr_v);
    else if (v.Op == Op386ANDLmodify) 
        return rewriteValue386_Op386ANDLmodify(_addr_v);
    else if (v.Op == Op386CMPB) 
        return rewriteValue386_Op386CMPB(_addr_v);
    else if (v.Op == Op386CMPBconst) 
        return rewriteValue386_Op386CMPBconst(_addr_v);
    else if (v.Op == Op386CMPBload) 
        return rewriteValue386_Op386CMPBload(_addr_v);
    else if (v.Op == Op386CMPL) 
        return rewriteValue386_Op386CMPL(_addr_v);
    else if (v.Op == Op386CMPLconst) 
        return rewriteValue386_Op386CMPLconst(_addr_v);
    else if (v.Op == Op386CMPLload) 
        return rewriteValue386_Op386CMPLload(_addr_v);
    else if (v.Op == Op386CMPW) 
        return rewriteValue386_Op386CMPW(_addr_v);
    else if (v.Op == Op386CMPWconst) 
        return rewriteValue386_Op386CMPWconst(_addr_v);
    else if (v.Op == Op386CMPWload) 
        return rewriteValue386_Op386CMPWload(_addr_v);
    else if (v.Op == Op386DIVSD) 
        return rewriteValue386_Op386DIVSD(_addr_v);
    else if (v.Op == Op386DIVSDload) 
        return rewriteValue386_Op386DIVSDload(_addr_v);
    else if (v.Op == Op386DIVSS) 
        return rewriteValue386_Op386DIVSS(_addr_v);
    else if (v.Op == Op386DIVSSload) 
        return rewriteValue386_Op386DIVSSload(_addr_v);
    else if (v.Op == Op386LEAL) 
        return rewriteValue386_Op386LEAL(_addr_v);
    else if (v.Op == Op386LEAL1) 
        return rewriteValue386_Op386LEAL1(_addr_v);
    else if (v.Op == Op386LEAL2) 
        return rewriteValue386_Op386LEAL2(_addr_v);
    else if (v.Op == Op386LEAL4) 
        return rewriteValue386_Op386LEAL4(_addr_v);
    else if (v.Op == Op386LEAL8) 
        return rewriteValue386_Op386LEAL8(_addr_v);
    else if (v.Op == Op386MOVBLSX) 
        return rewriteValue386_Op386MOVBLSX(_addr_v);
    else if (v.Op == Op386MOVBLSXload) 
        return rewriteValue386_Op386MOVBLSXload(_addr_v);
    else if (v.Op == Op386MOVBLZX) 
        return rewriteValue386_Op386MOVBLZX(_addr_v);
    else if (v.Op == Op386MOVBload) 
        return rewriteValue386_Op386MOVBload(_addr_v);
    else if (v.Op == Op386MOVBstore) 
        return rewriteValue386_Op386MOVBstore(_addr_v);
    else if (v.Op == Op386MOVBstoreconst) 
        return rewriteValue386_Op386MOVBstoreconst(_addr_v);
    else if (v.Op == Op386MOVLload) 
        return rewriteValue386_Op386MOVLload(_addr_v);
    else if (v.Op == Op386MOVLstore) 
        return rewriteValue386_Op386MOVLstore(_addr_v);
    else if (v.Op == Op386MOVLstoreconst) 
        return rewriteValue386_Op386MOVLstoreconst(_addr_v);
    else if (v.Op == Op386MOVSDconst) 
        return rewriteValue386_Op386MOVSDconst(_addr_v);
    else if (v.Op == Op386MOVSDload) 
        return rewriteValue386_Op386MOVSDload(_addr_v);
    else if (v.Op == Op386MOVSDstore) 
        return rewriteValue386_Op386MOVSDstore(_addr_v);
    else if (v.Op == Op386MOVSSconst) 
        return rewriteValue386_Op386MOVSSconst(_addr_v);
    else if (v.Op == Op386MOVSSload) 
        return rewriteValue386_Op386MOVSSload(_addr_v);
    else if (v.Op == Op386MOVSSstore) 
        return rewriteValue386_Op386MOVSSstore(_addr_v);
    else if (v.Op == Op386MOVWLSX) 
        return rewriteValue386_Op386MOVWLSX(_addr_v);
    else if (v.Op == Op386MOVWLSXload) 
        return rewriteValue386_Op386MOVWLSXload(_addr_v);
    else if (v.Op == Op386MOVWLZX) 
        return rewriteValue386_Op386MOVWLZX(_addr_v);
    else if (v.Op == Op386MOVWload) 
        return rewriteValue386_Op386MOVWload(_addr_v);
    else if (v.Op == Op386MOVWstore) 
        return rewriteValue386_Op386MOVWstore(_addr_v);
    else if (v.Op == Op386MOVWstoreconst) 
        return rewriteValue386_Op386MOVWstoreconst(_addr_v);
    else if (v.Op == Op386MULL) 
        return rewriteValue386_Op386MULL(_addr_v);
    else if (v.Op == Op386MULLconst) 
        return rewriteValue386_Op386MULLconst(_addr_v);
    else if (v.Op == Op386MULLload) 
        return rewriteValue386_Op386MULLload(_addr_v);
    else if (v.Op == Op386MULSD) 
        return rewriteValue386_Op386MULSD(_addr_v);
    else if (v.Op == Op386MULSDload) 
        return rewriteValue386_Op386MULSDload(_addr_v);
    else if (v.Op == Op386MULSS) 
        return rewriteValue386_Op386MULSS(_addr_v);
    else if (v.Op == Op386MULSSload) 
        return rewriteValue386_Op386MULSSload(_addr_v);
    else if (v.Op == Op386NEGL) 
        return rewriteValue386_Op386NEGL(_addr_v);
    else if (v.Op == Op386NOTL) 
        return rewriteValue386_Op386NOTL(_addr_v);
    else if (v.Op == Op386ORL) 
        return rewriteValue386_Op386ORL(_addr_v);
    else if (v.Op == Op386ORLconst) 
        return rewriteValue386_Op386ORLconst(_addr_v);
    else if (v.Op == Op386ORLconstmodify) 
        return rewriteValue386_Op386ORLconstmodify(_addr_v);
    else if (v.Op == Op386ORLload) 
        return rewriteValue386_Op386ORLload(_addr_v);
    else if (v.Op == Op386ORLmodify) 
        return rewriteValue386_Op386ORLmodify(_addr_v);
    else if (v.Op == Op386ROLBconst) 
        return rewriteValue386_Op386ROLBconst(_addr_v);
    else if (v.Op == Op386ROLLconst) 
        return rewriteValue386_Op386ROLLconst(_addr_v);
    else if (v.Op == Op386ROLWconst) 
        return rewriteValue386_Op386ROLWconst(_addr_v);
    else if (v.Op == Op386SARB) 
        return rewriteValue386_Op386SARB(_addr_v);
    else if (v.Op == Op386SARBconst) 
        return rewriteValue386_Op386SARBconst(_addr_v);
    else if (v.Op == Op386SARL) 
        return rewriteValue386_Op386SARL(_addr_v);
    else if (v.Op == Op386SARLconst) 
        return rewriteValue386_Op386SARLconst(_addr_v);
    else if (v.Op == Op386SARW) 
        return rewriteValue386_Op386SARW(_addr_v);
    else if (v.Op == Op386SARWconst) 
        return rewriteValue386_Op386SARWconst(_addr_v);
    else if (v.Op == Op386SBBL) 
        return rewriteValue386_Op386SBBL(_addr_v);
    else if (v.Op == Op386SBBLcarrymask) 
        return rewriteValue386_Op386SBBLcarrymask(_addr_v);
    else if (v.Op == Op386SETA) 
        return rewriteValue386_Op386SETA(_addr_v);
    else if (v.Op == Op386SETAE) 
        return rewriteValue386_Op386SETAE(_addr_v);
    else if (v.Op == Op386SETB) 
        return rewriteValue386_Op386SETB(_addr_v);
    else if (v.Op == Op386SETBE) 
        return rewriteValue386_Op386SETBE(_addr_v);
    else if (v.Op == Op386SETEQ) 
        return rewriteValue386_Op386SETEQ(_addr_v);
    else if (v.Op == Op386SETG) 
        return rewriteValue386_Op386SETG(_addr_v);
    else if (v.Op == Op386SETGE) 
        return rewriteValue386_Op386SETGE(_addr_v);
    else if (v.Op == Op386SETL) 
        return rewriteValue386_Op386SETL(_addr_v);
    else if (v.Op == Op386SETLE) 
        return rewriteValue386_Op386SETLE(_addr_v);
    else if (v.Op == Op386SETNE) 
        return rewriteValue386_Op386SETNE(_addr_v);
    else if (v.Op == Op386SHLL) 
        return rewriteValue386_Op386SHLL(_addr_v);
    else if (v.Op == Op386SHLLconst) 
        return rewriteValue386_Op386SHLLconst(_addr_v);
    else if (v.Op == Op386SHRB) 
        return rewriteValue386_Op386SHRB(_addr_v);
    else if (v.Op == Op386SHRBconst) 
        return rewriteValue386_Op386SHRBconst(_addr_v);
    else if (v.Op == Op386SHRL) 
        return rewriteValue386_Op386SHRL(_addr_v);
    else if (v.Op == Op386SHRLconst) 
        return rewriteValue386_Op386SHRLconst(_addr_v);
    else if (v.Op == Op386SHRW) 
        return rewriteValue386_Op386SHRW(_addr_v);
    else if (v.Op == Op386SHRWconst) 
        return rewriteValue386_Op386SHRWconst(_addr_v);
    else if (v.Op == Op386SUBL) 
        return rewriteValue386_Op386SUBL(_addr_v);
    else if (v.Op == Op386SUBLcarry) 
        return rewriteValue386_Op386SUBLcarry(_addr_v);
    else if (v.Op == Op386SUBLconst) 
        return rewriteValue386_Op386SUBLconst(_addr_v);
    else if (v.Op == Op386SUBLload) 
        return rewriteValue386_Op386SUBLload(_addr_v);
    else if (v.Op == Op386SUBLmodify) 
        return rewriteValue386_Op386SUBLmodify(_addr_v);
    else if (v.Op == Op386SUBSD) 
        return rewriteValue386_Op386SUBSD(_addr_v);
    else if (v.Op == Op386SUBSDload) 
        return rewriteValue386_Op386SUBSDload(_addr_v);
    else if (v.Op == Op386SUBSS) 
        return rewriteValue386_Op386SUBSS(_addr_v);
    else if (v.Op == Op386SUBSSload) 
        return rewriteValue386_Op386SUBSSload(_addr_v);
    else if (v.Op == Op386XORL) 
        return rewriteValue386_Op386XORL(_addr_v);
    else if (v.Op == Op386XORLconst) 
        return rewriteValue386_Op386XORLconst(_addr_v);
    else if (v.Op == Op386XORLconstmodify) 
        return rewriteValue386_Op386XORLconstmodify(_addr_v);
    else if (v.Op == Op386XORLload) 
        return rewriteValue386_Op386XORLload(_addr_v);
    else if (v.Op == Op386XORLmodify) 
        return rewriteValue386_Op386XORLmodify(_addr_v);
    else if (v.Op == OpAdd16) 
        v.Op = Op386ADDL;
        return true;
    else if (v.Op == OpAdd32) 
        v.Op = Op386ADDL;
        return true;
    else if (v.Op == OpAdd32F) 
        v.Op = Op386ADDSS;
        return true;
    else if (v.Op == OpAdd32carry) 
        v.Op = Op386ADDLcarry;
        return true;
    else if (v.Op == OpAdd32withcarry) 
        v.Op = Op386ADCL;
        return true;
    else if (v.Op == OpAdd64F) 
        v.Op = Op386ADDSD;
        return true;
    else if (v.Op == OpAdd8) 
        v.Op = Op386ADDL;
        return true;
    else if (v.Op == OpAddPtr) 
        v.Op = Op386ADDL;
        return true;
    else if (v.Op == OpAddr) 
        return rewriteValue386_OpAddr(_addr_v);
    else if (v.Op == OpAnd16) 
        v.Op = Op386ANDL;
        return true;
    else if (v.Op == OpAnd32) 
        v.Op = Op386ANDL;
        return true;
    else if (v.Op == OpAnd8) 
        v.Op = Op386ANDL;
        return true;
    else if (v.Op == OpAndB) 
        v.Op = Op386ANDL;
        return true;
    else if (v.Op == OpAvg32u) 
        v.Op = Op386AVGLU;
        return true;
    else if (v.Op == OpBswap32) 
        v.Op = Op386BSWAPL;
        return true;
    else if (v.Op == OpClosureCall) 
        v.Op = Op386CALLclosure;
        return true;
    else if (v.Op == OpCom16) 
        v.Op = Op386NOTL;
        return true;
    else if (v.Op == OpCom32) 
        v.Op = Op386NOTL;
        return true;
    else if (v.Op == OpCom8) 
        v.Op = Op386NOTL;
        return true;
    else if (v.Op == OpConst16) 
        return rewriteValue386_OpConst16(_addr_v);
    else if (v.Op == OpConst32) 
        v.Op = Op386MOVLconst;
        return true;
    else if (v.Op == OpConst32F) 
        v.Op = Op386MOVSSconst;
        return true;
    else if (v.Op == OpConst64F) 
        v.Op = Op386MOVSDconst;
        return true;
    else if (v.Op == OpConst8) 
        return rewriteValue386_OpConst8(_addr_v);
    else if (v.Op == OpConstBool) 
        return rewriteValue386_OpConstBool(_addr_v);
    else if (v.Op == OpConstNil) 
        return rewriteValue386_OpConstNil(_addr_v);
    else if (v.Op == OpCtz16) 
        return rewriteValue386_OpCtz16(_addr_v);
    else if (v.Op == OpCtz16NonZero) 
        v.Op = Op386BSFL;
        return true;
    else if (v.Op == OpCvt32Fto32) 
        v.Op = Op386CVTTSS2SL;
        return true;
    else if (v.Op == OpCvt32Fto64F) 
        v.Op = Op386CVTSS2SD;
        return true;
    else if (v.Op == OpCvt32to32F) 
        v.Op = Op386CVTSL2SS;
        return true;
    else if (v.Op == OpCvt32to64F) 
        v.Op = Op386CVTSL2SD;
        return true;
    else if (v.Op == OpCvt64Fto32) 
        v.Op = Op386CVTTSD2SL;
        return true;
    else if (v.Op == OpCvt64Fto32F) 
        v.Op = Op386CVTSD2SS;
        return true;
    else if (v.Op == OpCvtBoolToUint8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpDiv16) 
        v.Op = Op386DIVW;
        return true;
    else if (v.Op == OpDiv16u) 
        v.Op = Op386DIVWU;
        return true;
    else if (v.Op == OpDiv32) 
        v.Op = Op386DIVL;
        return true;
    else if (v.Op == OpDiv32F) 
        v.Op = Op386DIVSS;
        return true;
    else if (v.Op == OpDiv32u) 
        v.Op = Op386DIVLU;
        return true;
    else if (v.Op == OpDiv64F) 
        v.Op = Op386DIVSD;
        return true;
    else if (v.Op == OpDiv8) 
        return rewriteValue386_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValue386_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValue386_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValue386_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        return rewriteValue386_OpEq32F(_addr_v);
    else if (v.Op == OpEq64F) 
        return rewriteValue386_OpEq64F(_addr_v);
    else if (v.Op == OpEq8) 
        return rewriteValue386_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        return rewriteValue386_OpEqB(_addr_v);
    else if (v.Op == OpEqPtr) 
        return rewriteValue386_OpEqPtr(_addr_v);
    else if (v.Op == OpGetCallerPC) 
        v.Op = Op386LoweredGetCallerPC;
        return true;
    else if (v.Op == OpGetCallerSP) 
        v.Op = Op386LoweredGetCallerSP;
        return true;
    else if (v.Op == OpGetClosurePtr) 
        v.Op = Op386LoweredGetClosurePtr;
        return true;
    else if (v.Op == OpGetG) 
        v.Op = Op386LoweredGetG;
        return true;
    else if (v.Op == OpHmul32) 
        v.Op = Op386HMULL;
        return true;
    else if (v.Op == OpHmul32u) 
        v.Op = Op386HMULLU;
        return true;
    else if (v.Op == OpInterCall) 
        v.Op = Op386CALLinter;
        return true;
    else if (v.Op == OpIsInBounds) 
        return rewriteValue386_OpIsInBounds(_addr_v);
    else if (v.Op == OpIsNonNil) 
        return rewriteValue386_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        return rewriteValue386_OpIsSliceInBounds(_addr_v);
    else if (v.Op == OpLeq16) 
        return rewriteValue386_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValue386_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValue386_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        return rewriteValue386_OpLeq32F(_addr_v);
    else if (v.Op == OpLeq32U) 
        return rewriteValue386_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64F) 
        return rewriteValue386_OpLeq64F(_addr_v);
    else if (v.Op == OpLeq8) 
        return rewriteValue386_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValue386_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValue386_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValue386_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValue386_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        return rewriteValue386_OpLess32F(_addr_v);
    else if (v.Op == OpLess32U) 
        return rewriteValue386_OpLess32U(_addr_v);
    else if (v.Op == OpLess64F) 
        return rewriteValue386_OpLess64F(_addr_v);
    else if (v.Op == OpLess8) 
        return rewriteValue386_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValue386_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValue386_OpLoad(_addr_v);
    else if (v.Op == OpLocalAddr) 
        return rewriteValue386_OpLocalAddr(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValue386_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValue386_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValue386_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh16x8) 
        return rewriteValue386_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValue386_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValue386_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValue386_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh32x8) 
        return rewriteValue386_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValue386_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValue386_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValue386_OpLsh8x64(_addr_v);
    else if (v.Op == OpLsh8x8) 
        return rewriteValue386_OpLsh8x8(_addr_v);
    else if (v.Op == OpMod16) 
        v.Op = Op386MODW;
        return true;
    else if (v.Op == OpMod16u) 
        v.Op = Op386MODWU;
        return true;
    else if (v.Op == OpMod32) 
        v.Op = Op386MODL;
        return true;
    else if (v.Op == OpMod32u) 
        v.Op = Op386MODLU;
        return true;
    else if (v.Op == OpMod8) 
        return rewriteValue386_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValue386_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValue386_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        v.Op = Op386MULL;
        return true;
    else if (v.Op == OpMul32) 
        v.Op = Op386MULL;
        return true;
    else if (v.Op == OpMul32F) 
        v.Op = Op386MULSS;
        return true;
    else if (v.Op == OpMul32uhilo) 
        v.Op = Op386MULLQU;
        return true;
    else if (v.Op == OpMul64F) 
        v.Op = Op386MULSD;
        return true;
    else if (v.Op == OpMul8) 
        v.Op = Op386MULL;
        return true;
    else if (v.Op == OpNeg16) 
        v.Op = Op386NEGL;
        return true;
    else if (v.Op == OpNeg32) 
        v.Op = Op386NEGL;
        return true;
    else if (v.Op == OpNeg32F) 
        return rewriteValue386_OpNeg32F(_addr_v);
    else if (v.Op == OpNeg64F) 
        return rewriteValue386_OpNeg64F(_addr_v);
    else if (v.Op == OpNeg8) 
        v.Op = Op386NEGL;
        return true;
    else if (v.Op == OpNeq16) 
        return rewriteValue386_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValue386_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        return rewriteValue386_OpNeq32F(_addr_v);
    else if (v.Op == OpNeq64F) 
        return rewriteValue386_OpNeq64F(_addr_v);
    else if (v.Op == OpNeq8) 
        return rewriteValue386_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        return rewriteValue386_OpNeqB(_addr_v);
    else if (v.Op == OpNeqPtr) 
        return rewriteValue386_OpNeqPtr(_addr_v);
    else if (v.Op == OpNilCheck) 
        v.Op = Op386LoweredNilCheck;
        return true;
    else if (v.Op == OpNot) 
        return rewriteValue386_OpNot(_addr_v);
    else if (v.Op == OpOffPtr) 
        return rewriteValue386_OpOffPtr(_addr_v);
    else if (v.Op == OpOr16) 
        v.Op = Op386ORL;
        return true;
    else if (v.Op == OpOr32) 
        v.Op = Op386ORL;
        return true;
    else if (v.Op == OpOr8) 
        v.Op = Op386ORL;
        return true;
    else if (v.Op == OpOrB) 
        v.Op = Op386ORL;
        return true;
    else if (v.Op == OpPanicBounds) 
        return rewriteValue386_OpPanicBounds(_addr_v);
    else if (v.Op == OpPanicExtend) 
        return rewriteValue386_OpPanicExtend(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValue386_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        return rewriteValue386_OpRotateLeft32(_addr_v);
    else if (v.Op == OpRotateLeft8) 
        return rewriteValue386_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound32F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRound64F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValue386_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValue386_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValue386_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValue386_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValue386_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValue386_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValue386_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValue386_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValue386_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValue386_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValue386_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValue386_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValue386_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValue386_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValue386_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValue386_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValue386_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValue386_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValue386_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValue386_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValue386_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValue386_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValue386_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValue386_OpRsh8x8(_addr_v);
    else if (v.Op == OpSelect0) 
        return rewriteValue386_OpSelect0(_addr_v);
    else if (v.Op == OpSelect1) 
        return rewriteValue386_OpSelect1(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        v.Op = Op386MOVWLSX;
        return true;
    else if (v.Op == OpSignExt8to16) 
        v.Op = Op386MOVBLSX;
        return true;
    else if (v.Op == OpSignExt8to32) 
        v.Op = Op386MOVBLSX;
        return true;
    else if (v.Op == OpSignmask) 
        return rewriteValue386_OpSignmask(_addr_v);
    else if (v.Op == OpSlicemask) 
        return rewriteValue386_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        v.Op = Op386SQRTSD;
        return true;
    else if (v.Op == OpSqrt32) 
        v.Op = Op386SQRTSS;
        return true;
    else if (v.Op == OpStaticCall) 
        v.Op = Op386CALLstatic;
        return true;
    else if (v.Op == OpStore) 
        return rewriteValue386_OpStore(_addr_v);
    else if (v.Op == OpSub16) 
        v.Op = Op386SUBL;
        return true;
    else if (v.Op == OpSub32) 
        v.Op = Op386SUBL;
        return true;
    else if (v.Op == OpSub32F) 
        v.Op = Op386SUBSS;
        return true;
    else if (v.Op == OpSub32carry) 
        v.Op = Op386SUBLcarry;
        return true;
    else if (v.Op == OpSub32withcarry) 
        v.Op = Op386SBBL;
        return true;
    else if (v.Op == OpSub64F) 
        v.Op = Op386SUBSD;
        return true;
    else if (v.Op == OpSub8) 
        v.Op = Op386SUBL;
        return true;
    else if (v.Op == OpSubPtr) 
        v.Op = Op386SUBL;
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
    else if (v.Op == OpWB) 
        v.Op = Op386LoweredWB;
        return true;
    else if (v.Op == OpXor16) 
        v.Op = Op386XORL;
        return true;
    else if (v.Op == OpXor32) 
        v.Op = Op386XORL;
        return true;
    else if (v.Op == OpXor8) 
        v.Op = Op386XORL;
        return true;
    else if (v.Op == OpZero) 
        return rewriteValue386_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        v.Op = Op386MOVWLZX;
        return true;
    else if (v.Op == OpZeroExt8to16) 
        v.Op = Op386MOVBLZX;
        return true;
    else if (v.Op == OpZeroExt8to32) 
        v.Op = Op386MOVBLZX;
        return true;
    else if (v.Op == OpZeromask) 
        return rewriteValue386_OpZeromask(_addr_v);
        return false;
}
private static bool rewriteValue386_Op386ADCL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADCL x (MOVLconst [c]) f)
    // result: (ADCLconst [c] x f)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                var f = v_2;
                v.reset(Op386ADCLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, f);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDL x (MOVLconst [c]))
    // result: (ADDLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(Op386ADDLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL (SHLLconst [c] x) (SHRLconst [d] x))
    // cond: d == 32-c
    // result: (ROLLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRLconst) {
                    continue;
                }
                var d = auxIntToInt32(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 32 - c)) {
                    continue;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL <t> (SHLLconst x [c]) (SHRWconst x [d]))
    // cond: c < 16 && d == int16(16-c) && t.Size() == 2
    // result: (ROLWconst x [int16(c)])
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRWconst) {
                    continue;
                }
                d = auxIntToInt16(v_1.AuxInt);
                if (x != v_1.Args[0] || !(c < 16 && d == int16(16 - c) && t.Size() == 2)) {
                    continue;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = int16ToAuxInt(int16(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL <t> (SHLLconst x [c]) (SHRBconst x [d]))
    // cond: c < 8 && d == int8(8-c) && t.Size() == 1
    // result: (ROLBconst x [int8(c)])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRBconst) {
                    continue;
                }
                d = auxIntToInt8(v_1.AuxInt);
                if (x != v_1.Args[0] || !(c < 8 && d == int8(8 - c) && t.Size() == 1)) {
                    continue;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = int8ToAuxInt(int8(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (SHLLconst [3] y))
    // result: (LEAL8 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 3) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var y = v_1.Args[0];
                v.reset(Op386LEAL8);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (SHLLconst [2] y))
    // result: (LEAL4 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 2) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[0];
                v.reset(Op386LEAL4);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (SHLLconst [1] y))
    // result: (LEAL2 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[0];
                v.reset(Op386LEAL2);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (ADDL y y))
    // result: (LEAL2 x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386ADDL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[1];
                if (y != v_1.Args[0]) {
                    continue;
                }
                v.reset(Op386LEAL2);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (ADDL x y))
    // result: (LEAL2 y x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386ADDL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }
                        y = v_1_1;
                        v.reset(Op386LEAL2);
                        v.AddArg2(y, x);
                        return true;
                    }

                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL (ADDLconst [c] x) y)
    // result: (LEAL1 [c] x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386ADDLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                y = v_1;
                v.reset(Op386LEAL1);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (LEAL [c] {s} y))
    // cond: x.Op != OpSB && y.Op != OpSB
    // result: (LEAL1 [c] {s} x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386LEAL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_1.AuxInt);
                var s = auxToSym(v_1.Aux);
                y = v_1.Args[0];
                if (!(x.Op != OpSB && y.Op != OpSB)) {
                    continue;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = int32ToAuxInt(c);
                v.Aux = symToAux(s);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x l:(MOVLload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (ADDLload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVLload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386ADDLload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ADDL x (NEGL y))
    // result: (SUBL x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386NEGL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[0];
                v.reset(Op386SUBL);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDLcarry(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDLcarry x (MOVLconst [c]))
    // result: (ADDLconstcarry [c] x)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(Op386ADDLconstcarry);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ADDLconst [c] (ADDL x y))
    // result: (LEAL1 [c] x y)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386ADDL) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(Op386LEAL1);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDLconst [c] (LEAL [d] {s} x))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (LEAL [c+d] {s} x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var s = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(Op386LEAL);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg(x);
        return true;
    } 
    // match: (ADDLconst [c] x:(SP))
    // result: (LEAL [c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (x.Op != OpSP) {
            break;
        }
        v.reset(Op386LEAL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (ADDLconst [c] (LEAL1 [d] {s} x y))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (LEAL1 [c+d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386LEAL1) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        s = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(Op386LEAL1);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDLconst [c] (LEAL2 [d] {s} x y))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (LEAL2 [c+d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386LEAL2) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        s = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(Op386LEAL2);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDLconst [c] (LEAL4 [d] {s} x y))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (LEAL4 [c+d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386LEAL4) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        s = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDLconst [c] (LEAL8 [d] {s} x y))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (LEAL8 [c+d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386LEAL8) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        s = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (ADDLconst [c] x)
    // cond: c==0
    // result: x
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ADDLconst [c] (MOVLconst [d]))
    // result: (MOVLconst [c+d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(c + d);
        return true;
    } 
    // match: (ADDLconst [c] (ADDLconst [d] x))
    // result: (ADDLconst [c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(Op386ADDLconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDLconstmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ADDLconstmodify [valoff1] {sym} (ADDLconst [off2] base) mem)
    // cond: valoff1.canAdd32(off2)
    // result: (ADDLconstmodify [valoff1.addOffset32(off2)] {sym} base mem)
    while (true) {
        var valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var mem = v_1;
        if (!(valoff1.canAdd32(off2))) {
            break;
        }
        v.reset(Op386ADDLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (ADDLconstmodify [valoff1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ADDLconstmodify [valoff1.addOffset32(off2)] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        mem = v_1;
        if (!(valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ADDLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ADDLload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ADDLload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ADDLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (ADDLload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ADDLload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ADDLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDLmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ADDLmodify [off1] {sym} (ADDLconst [off2] base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ADDLmodify [off1+off2] {sym} base val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ADDLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (ADDLmodify [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ADDLmodify [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ADDLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDSD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDSD x l:(MOVSDload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (ADDSDload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVSDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386ADDSDload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDSDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ADDSDload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ADDSDload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ADDSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (ADDSDload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ADDSDload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ADDSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDSS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDSS x l:(MOVSSload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (ADDSSload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVSSload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386ADDSSload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386ADDSSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ADDSSload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ADDSSload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ADDSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (ADDSSload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ADDSSload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ADDSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ANDL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ANDL x (MOVLconst [c]))
    // result: (ANDLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(Op386ANDLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ANDL x l:(MOVLload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (ANDLload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVLload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386ANDLload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ANDL x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ANDLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ANDLconst [c] (ANDLconst [d] x))
    // result: (ANDLconst [c & d] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(Op386ANDLconst);
        v.AuxInt = int32ToAuxInt(c & d);
        v.AddArg(x);
        return true;
    } 
    // match: (ANDLconst [c] _)
    // cond: c==0
    // result: (MOVLconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (!(c == 0)) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (ANDLconst [c] x)
    // cond: c==-1
    // result: x
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c == -1)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ANDLconst [c] (MOVLconst [d]))
    // result: (MOVLconst [c&d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(c & d);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ANDLconstmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ANDLconstmodify [valoff1] {sym} (ADDLconst [off2] base) mem)
    // cond: valoff1.canAdd32(off2)
    // result: (ANDLconstmodify [valoff1.addOffset32(off2)] {sym} base mem)
    while (true) {
        var valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var mem = v_1;
        if (!(valoff1.canAdd32(off2))) {
            break;
        }
        v.reset(Op386ANDLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (ANDLconstmodify [valoff1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ANDLconstmodify [valoff1.addOffset32(off2)] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        mem = v_1;
        if (!(valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ANDLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ANDLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ANDLload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ANDLload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ANDLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (ANDLload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ANDLload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ANDLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ANDLmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ANDLmodify [off1] {sym} (ADDLconst [off2] base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ANDLmodify [off1+off2] {sym} base val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ANDLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (ANDLmodify [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ANDLmodify [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ANDLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPB x (MOVLconst [c]))
    // result: (CMPBconst x [int8(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386CMPBconst);
        v.AuxInt = int8ToAuxInt(int8(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPB (MOVLconst [c]) x)
    // result: (InvertFlags (CMPBconst x [int8(c)]))
    while (true) {
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        v.reset(Op386InvertFlags);
        var v0 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v0.AuxInt = int8ToAuxInt(int8(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPB x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPB y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(Op386InvertFlags);
        v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPB l:(MOVBload {sym} [off] ptr mem) x)
    // cond: canMergeLoad(v, l) && clobber(l)
    // result: (CMPBload {sym} [off] ptr x mem)
    while (true) {
        var l = v_0;
        if (l.Op != Op386MOVBload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        x = v_1;
        if (!(canMergeLoad(v, l) && clobber(l))) {
            break;
        }
        v.reset(Op386CMPBload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (CMPB x l:(MOVBload {sym} [off] ptr mem))
    // cond: canMergeLoad(v, l) && clobber(l)
    // result: (InvertFlags (CMPBload {sym} [off] ptr x mem))
    while (true) {
        x = v_0;
        l = v_1;
        if (l.Op != Op386MOVBload) {
            break;
        }
        off = auxIntToInt32(l.AuxInt);
        sym = auxToSym(l.Aux);
        mem = l.Args[1];
        ptr = l.Args[0];
        if (!(canMergeLoad(v, l) && clobber(l))) {
            break;
        }
        v.reset(Op386InvertFlags);
        v0 = b.NewValue0(l.Pos, Op386CMPBload, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, x, mem);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPBconst (MOVLconst [x]) [y])
    // cond: int8(x)==y
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        if (!(int8(x) == y)) {
            break;
        }
        v.reset(Op386FlagEQ);
        return true;
    } 
    // match: (CMPBconst (MOVLconst [x]) [y])
    // cond: int8(x)<y && uint8(x)<uint8(y)
    // result: (FlagLT_ULT)
    while (true) {
        y = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int8(x) < y && uint8(x) < uint8(y))) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPBconst (MOVLconst [x]) [y])
    // cond: int8(x)<y && uint8(x)>uint8(y)
    // result: (FlagLT_UGT)
    while (true) {
        y = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int8(x) < y && uint8(x) > uint8(y))) {
            break;
        }
        v.reset(Op386FlagLT_UGT);
        return true;
    } 
    // match: (CMPBconst (MOVLconst [x]) [y])
    // cond: int8(x)>y && uint8(x)<uint8(y)
    // result: (FlagGT_ULT)
    while (true) {
        y = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int8(x) > y && uint8(x) < uint8(y))) {
            break;
        }
        v.reset(Op386FlagGT_ULT);
        return true;
    } 
    // match: (CMPBconst (MOVLconst [x]) [y])
    // cond: int8(x)>y && uint8(x)>uint8(y)
    // result: (FlagGT_UGT)
    while (true) {
        y = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int8(x) > y && uint8(x) > uint8(y))) {
            break;
        }
        v.reset(Op386FlagGT_UGT);
        return true;
    } 
    // match: (CMPBconst (ANDLconst _ [m]) [n])
    // cond: 0 <= int8(m) && int8(m) < n
    // result: (FlagLT_ULT)
    while (true) {
        var n = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= int8(m) && int8(m) < n)) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPBconst l:(ANDL x y) [0])
    // cond: l.Uses==1
    // result: (TESTB x y)
    while (true) {
        if (auxIntToInt8(v.AuxInt) != 0) {
            break;
        }
        var l = v_0;
        if (l.Op != Op386ANDL) {
            break;
        }
        y = l.Args[1];
        x = l.Args[0];
        if (!(l.Uses == 1)) {
            break;
        }
        v.reset(Op386TESTB);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPBconst l:(ANDLconst [c] x) [0])
    // cond: l.Uses==1
    // result: (TESTBconst [int8(c)] x)
    while (true) {
        if (auxIntToInt8(v.AuxInt) != 0) {
            break;
        }
        l = v_0;
        if (l.Op != Op386ANDLconst) {
            break;
        }
        var c = auxIntToInt32(l.AuxInt);
        x = l.Args[0];
        if (!(l.Uses == 1)) {
            break;
        }
        v.reset(Op386TESTBconst);
        v.AuxInt = int8ToAuxInt(int8(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPBconst x [0])
    // result: (TESTB x x)
    while (true) {
        if (auxIntToInt8(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.reset(Op386TESTB);
        v.AddArg2(x, x);
        return true;
    } 
    // match: (CMPBconst l:(MOVBload {sym} [off] ptr mem) [c])
    // cond: l.Uses == 1 && clobber(l)
    // result: @l.Block (CMPBconstload {sym} [makeValAndOff(int32(c),off)] ptr mem)
    while (true) {
        c = auxIntToInt8(v.AuxInt);
        l = v_0;
        if (l.Op != Op386MOVBload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(l.Uses == 1 && clobber(l))) {
            break;
        }
        b = l.Block;
        var v0 = b.NewValue0(l.Pos, Op386CMPBconstload, types.TypeFlags);
        v.copyOf(v0);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(c), off));
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMPBload {sym} [off] ptr (MOVLconst [c]) mem)
    // result: (CMPBconstload {sym} [makeValAndOff(int32(int8(c)),off)] ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(Op386CMPBconstload);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(int8(c)), off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPL x (MOVLconst [c]))
    // result: (CMPLconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386CMPLconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPL (MOVLconst [c]) x)
    // result: (InvertFlags (CMPLconst x [c]))
    while (true) {
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        v.reset(Op386InvertFlags);
        var v0 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPL x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPL y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(Op386InvertFlags);
        v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPL l:(MOVLload {sym} [off] ptr mem) x)
    // cond: canMergeLoad(v, l) && clobber(l)
    // result: (CMPLload {sym} [off] ptr x mem)
    while (true) {
        var l = v_0;
        if (l.Op != Op386MOVLload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        x = v_1;
        if (!(canMergeLoad(v, l) && clobber(l))) {
            break;
        }
        v.reset(Op386CMPLload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (CMPL x l:(MOVLload {sym} [off] ptr mem))
    // cond: canMergeLoad(v, l) && clobber(l)
    // result: (InvertFlags (CMPLload {sym} [off] ptr x mem))
    while (true) {
        x = v_0;
        l = v_1;
        if (l.Op != Op386MOVLload) {
            break;
        }
        off = auxIntToInt32(l.AuxInt);
        sym = auxToSym(l.Aux);
        mem = l.Args[1];
        ptr = l.Args[0];
        if (!(canMergeLoad(v, l) && clobber(l))) {
            break;
        }
        v.reset(Op386InvertFlags);
        v0 = b.NewValue0(l.Pos, Op386CMPLload, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, x, mem);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPLconst (MOVLconst [x]) [y])
    // cond: x==y
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        if (!(x == y)) {
            break;
        }
        v.reset(Op386FlagEQ);
        return true;
    } 
    // match: (CMPLconst (MOVLconst [x]) [y])
    // cond: x<y && uint32(x)<uint32(y)
    // result: (FlagLT_ULT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(x < y && uint32(x) < uint32(y))) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPLconst (MOVLconst [x]) [y])
    // cond: x<y && uint32(x)>uint32(y)
    // result: (FlagLT_UGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(x < y && uint32(x) > uint32(y))) {
            break;
        }
        v.reset(Op386FlagLT_UGT);
        return true;
    } 
    // match: (CMPLconst (MOVLconst [x]) [y])
    // cond: x>y && uint32(x)<uint32(y)
    // result: (FlagGT_ULT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(x > y && uint32(x) < uint32(y))) {
            break;
        }
        v.reset(Op386FlagGT_ULT);
        return true;
    } 
    // match: (CMPLconst (MOVLconst [x]) [y])
    // cond: x>y && uint32(x)>uint32(y)
    // result: (FlagGT_UGT)
    while (true) {
        y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(x > y && uint32(x) > uint32(y))) {
            break;
        }
        v.reset(Op386FlagGT_UGT);
        return true;
    } 
    // match: (CMPLconst (SHRLconst _ [c]) [n])
    // cond: 0 <= n && 0 < c && c <= 32 && (1<<uint64(32-c)) <= uint64(n)
    // result: (FlagLT_ULT)
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386SHRLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= n && 0 < c && c <= 32 && (1 << (int)(uint64(32 - c))) <= uint64(n))) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPLconst (ANDLconst _ [m]) [n])
    // cond: 0 <= m && m < n
    // result: (FlagLT_ULT)
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= m && m < n)) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPLconst l:(ANDL x y) [0])
    // cond: l.Uses==1
    // result: (TESTL x y)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var l = v_0;
        if (l.Op != Op386ANDL) {
            break;
        }
        y = l.Args[1];
        x = l.Args[0];
        if (!(l.Uses == 1)) {
            break;
        }
        v.reset(Op386TESTL);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPLconst l:(ANDLconst [c] x) [0])
    // cond: l.Uses==1
    // result: (TESTLconst [c] x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        l = v_0;
        if (l.Op != Op386ANDLconst) {
            break;
        }
        c = auxIntToInt32(l.AuxInt);
        x = l.Args[0];
        if (!(l.Uses == 1)) {
            break;
        }
        v.reset(Op386TESTLconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (CMPLconst x [0])
    // result: (TESTL x x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.reset(Op386TESTL);
        v.AddArg2(x, x);
        return true;
    } 
    // match: (CMPLconst l:(MOVLload {sym} [off] ptr mem) [c])
    // cond: l.Uses == 1 && clobber(l)
    // result: @l.Block (CMPLconstload {sym} [makeValAndOff(int32(c),off)] ptr mem)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        l = v_0;
        if (l.Op != Op386MOVLload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(l.Uses == 1 && clobber(l))) {
            break;
        }
        b = l.Block;
        var v0 = b.NewValue0(l.Pos, Op386CMPLconstload, types.TypeFlags);
        v.copyOf(v0);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(c), off));
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMPLload {sym} [off] ptr (MOVLconst [c]) mem)
    // result: (CMPLconstload {sym} [makeValAndOff(c,off)] ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(Op386CMPLconstload);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPW x (MOVLconst [c]))
    // result: (CMPWconst x [int16(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386CMPWconst);
        v.AuxInt = int16ToAuxInt(int16(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPW (MOVLconst [c]) x)
    // result: (InvertFlags (CMPWconst x [int16(c)]))
    while (true) {
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        v.reset(Op386InvertFlags);
        var v0 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v0.AuxInt = int16ToAuxInt(int16(c));
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPW x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMPW y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(Op386InvertFlags);
        v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (CMPW l:(MOVWload {sym} [off] ptr mem) x)
    // cond: canMergeLoad(v, l) && clobber(l)
    // result: (CMPWload {sym} [off] ptr x mem)
    while (true) {
        var l = v_0;
        if (l.Op != Op386MOVWload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        x = v_1;
        if (!(canMergeLoad(v, l) && clobber(l))) {
            break;
        }
        v.reset(Op386CMPWload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (CMPW x l:(MOVWload {sym} [off] ptr mem))
    // cond: canMergeLoad(v, l) && clobber(l)
    // result: (InvertFlags (CMPWload {sym} [off] ptr x mem))
    while (true) {
        x = v_0;
        l = v_1;
        if (l.Op != Op386MOVWload) {
            break;
        }
        off = auxIntToInt32(l.AuxInt);
        sym = auxToSym(l.Aux);
        mem = l.Args[1];
        ptr = l.Args[0];
        if (!(canMergeLoad(v, l) && clobber(l))) {
            break;
        }
        v.reset(Op386InvertFlags);
        v0 = b.NewValue0(l.Pos, Op386CMPWload, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg3(ptr, x, mem);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPWconst (MOVLconst [x]) [y])
    // cond: int16(x)==y
    // result: (FlagEQ)
    while (true) {
        var y = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        if (!(int16(x) == y)) {
            break;
        }
        v.reset(Op386FlagEQ);
        return true;
    } 
    // match: (CMPWconst (MOVLconst [x]) [y])
    // cond: int16(x)<y && uint16(x)<uint16(y)
    // result: (FlagLT_ULT)
    while (true) {
        y = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int16(x) < y && uint16(x) < uint16(y))) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPWconst (MOVLconst [x]) [y])
    // cond: int16(x)<y && uint16(x)>uint16(y)
    // result: (FlagLT_UGT)
    while (true) {
        y = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int16(x) < y && uint16(x) > uint16(y))) {
            break;
        }
        v.reset(Op386FlagLT_UGT);
        return true;
    } 
    // match: (CMPWconst (MOVLconst [x]) [y])
    // cond: int16(x)>y && uint16(x)<uint16(y)
    // result: (FlagGT_ULT)
    while (true) {
        y = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int16(x) > y && uint16(x) < uint16(y))) {
            break;
        }
        v.reset(Op386FlagGT_ULT);
        return true;
    } 
    // match: (CMPWconst (MOVLconst [x]) [y])
    // cond: int16(x)>y && uint16(x)>uint16(y)
    // result: (FlagGT_UGT)
    while (true) {
        y = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        x = auxIntToInt32(v_0.AuxInt);
        if (!(int16(x) > y && uint16(x) > uint16(y))) {
            break;
        }
        v.reset(Op386FlagGT_UGT);
        return true;
    } 
    // match: (CMPWconst (ANDLconst _ [m]) [n])
    // cond: 0 <= int16(m) && int16(m) < n
    // result: (FlagLT_ULT)
    while (true) {
        var n = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= int16(m) && int16(m) < n)) {
            break;
        }
        v.reset(Op386FlagLT_ULT);
        return true;
    } 
    // match: (CMPWconst l:(ANDL x y) [0])
    // cond: l.Uses==1
    // result: (TESTW x y)
    while (true) {
        if (auxIntToInt16(v.AuxInt) != 0) {
            break;
        }
        var l = v_0;
        if (l.Op != Op386ANDL) {
            break;
        }
        y = l.Args[1];
        x = l.Args[0];
        if (!(l.Uses == 1)) {
            break;
        }
        v.reset(Op386TESTW);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (CMPWconst l:(ANDLconst [c] x) [0])
    // cond: l.Uses==1
    // result: (TESTWconst [int16(c)] x)
    while (true) {
        if (auxIntToInt16(v.AuxInt) != 0) {
            break;
        }
        l = v_0;
        if (l.Op != Op386ANDLconst) {
            break;
        }
        var c = auxIntToInt32(l.AuxInt);
        x = l.Args[0];
        if (!(l.Uses == 1)) {
            break;
        }
        v.reset(Op386TESTWconst);
        v.AuxInt = int16ToAuxInt(int16(c));
        v.AddArg(x);
        return true;
    } 
    // match: (CMPWconst x [0])
    // result: (TESTW x x)
    while (true) {
        if (auxIntToInt16(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.reset(Op386TESTW);
        v.AddArg2(x, x);
        return true;
    } 
    // match: (CMPWconst l:(MOVWload {sym} [off] ptr mem) [c])
    // cond: l.Uses == 1 && clobber(l)
    // result: @l.Block (CMPWconstload {sym} [makeValAndOff(int32(c),off)] ptr mem)
    while (true) {
        c = auxIntToInt16(v.AuxInt);
        l = v_0;
        if (l.Op != Op386MOVWload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(l.Uses == 1 && clobber(l))) {
            break;
        }
        b = l.Block;
        var v0 = b.NewValue0(l.Pos, Op386CMPWconstload, types.TypeFlags);
        v.copyOf(v0);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(c), off));
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386CMPWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMPWload {sym} [off] ptr (MOVLconst [c]) mem)
    // result: (CMPWconstload {sym} [makeValAndOff(int32(int16(c)),off)] ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(Op386CMPWconstload);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(int32(int16(c)), off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386DIVSD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (DIVSD x l:(MOVSDload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (DIVSDload x [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        var l = v_1;
        if (l.Op != Op386MOVSDload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
            break;
        }
        v.reset(Op386DIVSDload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386DIVSDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (DIVSDload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (DIVSDload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386DIVSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (DIVSDload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (DIVSDload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386DIVSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386DIVSS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (DIVSS x l:(MOVSSload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (DIVSSload x [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        var l = v_1;
        if (l.Op != Op386MOVSSload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
            break;
        }
        v.reset(Op386DIVSSload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386DIVSSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (DIVSSload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (DIVSSload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386DIVSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (DIVSSload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (DIVSSload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386DIVSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386LEAL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LEAL [c] {s} (ADDLconst [d] x))
    // cond: is32Bit(int64(c)+int64(d))
    // result: (LEAL [c+d] {s} x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(is32Bit(int64(c) + int64(d)))) {
            break;
        }
        v.reset(Op386LEAL);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg(x);
        return true;
    } 
    // match: (LEAL [c] {s} (ADDL x y))
    // cond: x.Op != OpSB && y.Op != OpSB
    // result: (LEAL1 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDL) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                x = v_0_0;
                var y = v_0_1;
                if (!(x.Op != OpSB && y.Op != OpSB)) {
                    continue;
                (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                }
                v.reset(Op386LEAL1);
                v.AuxInt = int32ToAuxInt(c);
                v.Aux = symToAux(s);
                v.AddArg2(x, y);
                return true;
            }

        }
        break;
    } 
    // match: (LEAL [off1] {sym1} (LEAL [off2] {sym2} x))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL [off1+off2] {mergeSym(sym1,sym2)} x)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(Op386LEAL);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg(x);
        return true;
    } 
    // match: (LEAL [off1] {sym1} (LEAL1 [off2] {sym2} x y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL1) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(Op386LEAL1);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL [off1] {sym1} (LEAL2 [off2] {sym2} x y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL2 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL2) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(Op386LEAL2);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL [off1] {sym1} (LEAL4 [off2] {sym2} x y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL4 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL4) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL [off1] {sym1} (LEAL8 [off2] {sym2} x y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL8 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL8) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        y = v_0.Args[1];
        x = v_0.Args[0];
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386LEAL1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LEAL1 [c] {s} (ADDLconst [d] x) y)
    // cond: is32Bit(int64(c)+int64(d)) && x.Op != OpSB
    // result: (LEAL1 [c+d] {s} x y)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386ADDLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var d = auxIntToInt32(v_0.AuxInt);
                var x = v_0.Args[0];
                var y = v_1;
                if (!(is32Bit(int64(c) + int64(d)) && x.Op != OpSB)) {
                    continue;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = int32ToAuxInt(c + d);
                v.Aux = symToAux(s);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [c] {s} x (SHLLconst [1] y))
    // result: (LEAL2 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[0];
                v.reset(Op386LEAL2);
                v.AuxInt = int32ToAuxInt(c);
                v.Aux = symToAux(s);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [c] {s} x (SHLLconst [2] y))
    // result: (LEAL4 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 2) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[0];
                v.reset(Op386LEAL4);
                v.AuxInt = int32ToAuxInt(c);
                v.Aux = symToAux(s);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [c] {s} x (SHLLconst [3] y))
    // result: (LEAL8 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 3) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                y = v_1.Args[0];
                v.reset(Op386LEAL8);
                v.AuxInt = int32ToAuxInt(c);
                v.Aux = symToAux(s);
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [off1] {sym1} (LEAL [off2] {sym2} x) y)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB
    // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386LEAL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off2 = auxIntToInt32(v_0.AuxInt);
                var sym2 = auxToSym(v_0.Aux);
                x = v_0.Args[0];
                y = v_1;
                if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB)) {
                    continue;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSym(sym1, sym2));
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [off1] {sym1} x (LEAL1 [off2] {sym2} y y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL2 [off1+off2] {mergeSym(sym1, sym2)} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386LEAL1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off2 = auxIntToInt32(v_1.AuxInt);
                sym2 = auxToSym(v_1.Aux);
                y = v_1.Args[1];
                if (y != v_1.Args[0] || !(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
                    continue;
                }
                v.reset(Op386LEAL2);
                v.AuxInt = int32ToAuxInt(off1 + off2);
                v.Aux = symToAux(mergeSym(sym1, sym2));
                v.AddArg2(x, y);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [off1] {sym1} x (LEAL1 [off2] {sym2} x y))
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2)
    // result: (LEAL2 [off1+off2] {mergeSym(sym1, sym2)} y x)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym1 = auxToSym(v.Aux);
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != Op386LEAL1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                off2 = auxIntToInt32(v_1.AuxInt);
                sym2 = auxToSym(v_1.Aux);
                _ = v_1.Args[1];
                var v_1_0 = v_1.Args[0];
                var v_1_1 = v_1.Args[1];
                {
                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        if (x != v_1_0) {
                            continue;
                        (_i1, v_1_0, v_1_1) = (_i1 + 1, v_1_1, v_1_0);
                        }
                        y = v_1_1;
                        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2))) {
                            continue;
                        }
                        v.reset(Op386LEAL2);
                        v.AuxInt = int32ToAuxInt(off1 + off2);
                        v.Aux = symToAux(mergeSym(sym1, sym2));
                        v.AddArg2(y, x);
                        return true;
                    }

                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (LEAL1 [0] {nil} x y)
    // result: (ADDL x y)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0 || auxToSym(v.Aux) != null) {
            break;
        }
        x = v_0;
        y = v_1;
        v.reset(Op386ADDL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386LEAL2(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LEAL2 [c] {s} (ADDLconst [d] x) y)
    // cond: is32Bit(int64(c)+int64(d)) && x.Op != OpSB
    // result: (LEAL2 [c+d] {s} x y)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var y = v_1;
        if (!(is32Bit(int64(c) + int64(d)) && x.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL2);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL2 [c] {s} x (ADDLconst [d] y))
    // cond: is32Bit(int64(c)+2*int64(d)) && y.Op != OpSB
    // result: (LEAL2 [c+2*d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(is32Bit(int64(c) + 2 * int64(d)) && y.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL2);
        v.AuxInt = int32ToAuxInt(c + 2 * d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL2 [c] {s} x (SHLLconst [1] y))
    // result: (LEAL4 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 1) {
            break;
        }
        y = v_1.Args[0];
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(c);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL2 [c] {s} x (SHLLconst [2] y))
    // result: (LEAL8 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 2) {
            break;
        }
        y = v_1.Args[0];
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(c);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL2 [off1] {sym1} (LEAL [off2] {sym2} x) y)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB
    // result: (LEAL2 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        y = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL2);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL2 [off1] {sym} x (LEAL1 [off2] {nil} y y))
    // cond: is32Bit(int64(off1)+2*int64(off2))
    // result: (LEAL4 [off1+2*off2] {sym} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386LEAL1) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        if (auxToSym(v_1.Aux) != null) {
            break;
        }
        y = v_1.Args[1];
        if (y != v_1.Args[0] || !(is32Bit(int64(off1) + 2 * int64(off2)))) {
            break;
        }
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(off1 + 2 * off2);
        v.Aux = symToAux(sym);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386LEAL4(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LEAL4 [c] {s} (ADDLconst [d] x) y)
    // cond: is32Bit(int64(c)+int64(d)) && x.Op != OpSB
    // result: (LEAL4 [c+d] {s} x y)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var y = v_1;
        if (!(is32Bit(int64(c) + int64(d)) && x.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL4 [c] {s} x (ADDLconst [d] y))
    // cond: is32Bit(int64(c)+4*int64(d)) && y.Op != OpSB
    // result: (LEAL4 [c+4*d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(is32Bit(int64(c) + 4 * int64(d)) && y.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(c + 4 * d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL4 [c] {s} x (SHLLconst [1] y))
    // result: (LEAL8 [c] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386SHLLconst || auxIntToInt32(v_1.AuxInt) != 1) {
            break;
        }
        y = v_1.Args[0];
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(c);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL4 [off1] {sym1} (LEAL [off2] {sym2} x) y)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB
    // result: (LEAL4 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        y = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL4);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL4 [off1] {sym} x (LEAL1 [off2] {nil} y y))
    // cond: is32Bit(int64(off1)+4*int64(off2))
    // result: (LEAL8 [off1+4*off2] {sym} x y)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386LEAL1) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        if (auxToSym(v_1.Aux) != null) {
            break;
        }
        y = v_1.Args[1];
        if (y != v_1.Args[0] || !(is32Bit(int64(off1) + 4 * int64(off2)))) {
            break;
        }
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(off1 + 4 * off2);
        v.Aux = symToAux(sym);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386LEAL8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (LEAL8 [c] {s} (ADDLconst [d] x) y)
    // cond: is32Bit(int64(c)+int64(d)) && x.Op != OpSB
    // result: (LEAL8 [c+d] {s} x y)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var y = v_1;
        if (!(is32Bit(int64(c) + int64(d)) && x.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(c + d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL8 [c] {s} x (ADDLconst [d] y))
    // cond: is32Bit(int64(c)+8*int64(d)) && y.Op != OpSB
    // result: (LEAL8 [c+8*d] {s} x y)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        x = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        d = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        if (!(is32Bit(int64(c) + 8 * int64(d)) && y.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(c + 8 * d);
        v.Aux = symToAux(s);
        v.AddArg2(x, y);
        return true;
    } 
    // match: (LEAL8 [off1] {sym1} (LEAL [off2] {sym2} x) y)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB
    // result: (LEAL8 [off1+off2] {mergeSym(sym1,sym2)} x y)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        x = v_0.Args[0];
        y = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && x.Op != OpSB)) {
            break;
        }
        v.reset(Op386LEAL8);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVBLSX(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVBLSX x:(MOVBload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVBLSXload <v.Type> [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        if (x.Op != Op386MOVBload) {
            break;
        }
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, Op386MOVBLSXload, v.Type);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBLSX (ANDLconst [c] x))
    // cond: c & 0x80 == 0
    // result: (ANDLconst [c & 0x7f] x)
    while (true) {
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c & 0x80 == 0)) {
            break;
        }
        v.reset(Op386ANDLconst);
        v.AuxInt = int32ToAuxInt(c & 0x7f);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVBLSXload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVBLSXload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVBLSX x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVBstore) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(Op386MOVBLSX);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBLSXload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVBLSXload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVBLSXload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVBLZX(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVBLZX x:(MOVBload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVBload <v.Type> [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        if (x.Op != Op386MOVBload) {
            break;
        }
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, Op386MOVBload, v.Type);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBLZX (ANDLconst [c] x))
    // result: (ANDLconst [c & 0xff] x)
    while (true) {
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(Op386ANDLconst);
        v.AuxInt = int32ToAuxInt(c & 0xff);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVBload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVBLZX x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVBstore) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(Op386MOVBLZX);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVBload [off1] {sym} (ADDLconst [off2] ptr) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVBload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (MOVBload [off] {sym} (SB) _)
    // cond: symIsRO(sym)
    // result: (MOVLconst [int32(read8(sym, int64(off)))])
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpSB || !(symIsRO(sym))) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(int32(read8(sym, int64(off))));
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVBstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVBstore [off] {sym} ptr (MOVBLSX x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVBLSX) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(Op386MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBLZX x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != Op386MOVBLZX) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(Op386MOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVBstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVBstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVBstore [off] {sym} ptr (MOVLconst [c]) mem)
    // result: (MOVBstoreconst [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        mem = v_2;
        v.reset(Op386MOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SHRWconst [8] w) x:(MOVBstore [i-1] {s} p w mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-1] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        if (v_1.Op != Op386SHRWconst || auxIntToInt16(v_1.AuxInt) != 8) {
            break;
        }
        var w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SHRLconst [8] w) x:(MOVBstore [i-1] {s} p w mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-1] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != Op386SHRLconst || auxIntToInt32(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p w x:(MOVBstore {s} [i+1] p (SHRWconst [8] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i + 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var x_1 = x.Args[1];
        if (x_1.Op != Op386SHRWconst || auxIntToInt16(x_1.AuxInt) != 8 || w != x_1.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p w x:(MOVBstore {s} [i+1] p (SHRLconst [8] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i] {s} p w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        w = v_1;
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i + 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        x_1 = x.Args[1];
        if (x_1.Op != Op386SHRLconst || auxIntToInt32(x_1.AuxInt) != 8 || w != x_1.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p (SHRLconst [j] w) x:(MOVBstore [i-1] {s} p w0:(SHRLconst [j-8] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVWstore [i-1] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != Op386SHRLconst) {
            break;
        }
        var j = auxIntToInt32(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i - 1 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        if (w0.Op != Op386SHRLconst || auxIntToInt32(w0.AuxInt) != j - 8 || w != w0.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i - 1);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p1 (SHRWconst [8] w) x:(MOVBstore [i] {s} p0 w mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstore [i] {s} p0 w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        var p1 = v_0;
        if (v_1.Op != Op386SHRWconst || auxIntToInt16(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        var p0 = x.Args[0];
        if (w != x.Args[1] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p1 (SHRLconst [8] w) x:(MOVBstore [i] {s} p0 w mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstore [i] {s} p0 w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p1 = v_0;
        if (v_1.Op != Op386SHRLconst || auxIntToInt32(v_1.AuxInt) != 8) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        p0 = x.Args[0];
        if (w != x.Args[1] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p0 w x:(MOVBstore {s} [i] p1 (SHRWconst [8] w) mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstore [i] {s} p0 w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p0 = v_0;
        w = v_1;
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        p1 = x.Args[0];
        x_1 = x.Args[1];
        if (x_1.Op != Op386SHRWconst || auxIntToInt16(x_1.AuxInt) != 8 || w != x_1.Args[0] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p0 w x:(MOVBstore {s} [i] p1 (SHRLconst [8] w) mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstore [i] {s} p0 w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p0 = v_0;
        w = v_1;
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        p1 = x.Args[0];
        x_1 = x.Args[1];
        if (x_1.Op != Op386SHRLconst || auxIntToInt32(x_1.AuxInt) != 8 || w != x_1.Args[0] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w, mem);
        return true;
    } 
    // match: (MOVBstore [i] {s} p1 (SHRLconst [j] w) x:(MOVBstore [i] {s} p0 w0:(SHRLconst [j-8] w) mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstore [i] {s} p0 w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p1 = v_0;
        if (v_1.Op != Op386SHRLconst) {
            break;
        }
        j = auxIntToInt32(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVBstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        p0 = x.Args[0];
        w0 = x.Args[1];
        if (w0.Op != Op386SHRLconst || auxIntToInt32(w0.AuxInt) != j - 8 || w != w0.Args[0] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVBstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVBstoreconst [sc] {s} (ADDLconst [off] ptr) mem)
    // cond: sc.canAdd32(off)
    // result: (MOVBstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(sc.canAdd32(off))) {
            break;
        }
        v.reset(Op386MOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
    // cond: canMergeSym(sym1, sym2) && sc.canAdd32(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVBstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && sc.canAdd32(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVBstoreconst [c] {s} p x:(MOVBstoreconst [a] {s} p mem))
    // cond: x.Uses == 1 && a.Off() + 1 == c.Off() && clobber(x)
    // result: (MOVWstoreconst [makeValAndOff(a.Val()&0xff | c.Val()<<8, a.Off())] {s} p mem)
    while (true) {
        var c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p = v_0;
        var x = v_1;
        if (x.Op != Op386MOVBstoreconst) {
            break;
        }
        var a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(x.Uses == 1 && a.Off() + 1 == c.Off() && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xff | c.Val() << 8, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVBstoreconst [a] {s} p x:(MOVBstoreconst [c] {s} p mem))
    // cond: x.Uses == 1 && a.Off() + 1 == c.Off() && clobber(x)
    // result: (MOVWstoreconst [makeValAndOff(a.Val()&0xff | c.Val()<<8, a.Off())] {s} p mem)
    while (true) {
        a = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        x = v_1;
        if (x.Op != Op386MOVBstoreconst) {
            break;
        }
        c = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(x.Uses == 1 && a.Off() + 1 == c.Off() && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xff | c.Val() << 8, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVBstoreconst [c] {s} p1 x:(MOVBstoreconst [a] {s} p0 mem))
    // cond: x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstoreconst [makeValAndOff(a.Val()&0xff | c.Val()<<8, a.Off())] {s} p0 mem)
    while (true) {
        c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p1 = v_0;
        x = v_1;
        if (x.Op != Op386MOVBstoreconst) {
            break;
        }
        a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        var p0 = x.Args[0];
        if (!(x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xff | c.Val() << 8, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p0, mem);
        return true;
    } 
    // match: (MOVBstoreconst [a] {s} p0 x:(MOVBstoreconst [c] {s} p1 mem))
    // cond: x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 1) && clobber(x)
    // result: (MOVWstoreconst [makeValAndOff(a.Val()&0xff | c.Val()<<8, a.Off())] {s} p0 mem)
    while (true) {
        a = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        p0 = v_0;
        x = v_1;
        if (x.Op != Op386MOVBstoreconst) {
            break;
        }
        c = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        p1 = x.Args[0];
        if (!(x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 1) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xff | c.Val() << 8, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVLload [off] {sym} ptr (MOVLstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVLstore) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (MOVLload [off1] {sym} (ADDLconst [off2] ptr) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVLload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVLload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVLload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (MOVLload [off] {sym} (SB) _)
    // cond: symIsRO(sym)
    // result: (MOVLconst [int32(read32(sym, int64(off), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpSB || !(symIsRO(sym))) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(int32(read32(sym, int64(off), config.ctxt.Arch.ByteOrder)));
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVLstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVLstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVLstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVLstore [off] {sym} ptr (MOVLconst [c]) mem)
    // result: (MOVLstoreconst [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        mem = v_2;
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVLstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVLstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ADDLload x [off] {sym} ptr mem) mem)
    // cond: y.Uses==1 && clobber(y)
    // result: (ADDLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        var y = v_1;
        if (y.Op != Op386ADDLload || auxIntToInt32(y.AuxInt) != off || auxToSym(y.Aux) != sym) {
            break;
        }
        mem = y.Args[2];
        var x = y.Args[0];
        if (ptr != y.Args[1] || mem != v_2 || !(y.Uses == 1 && clobber(y))) {
            break;
        }
        v.reset(Op386ADDLmodify);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ANDLload x [off] {sym} ptr mem) mem)
    // cond: y.Uses==1 && clobber(y)
    // result: (ANDLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ANDLload || auxIntToInt32(y.AuxInt) != off || auxToSym(y.Aux) != sym) {
            break;
        }
        mem = y.Args[2];
        x = y.Args[0];
        if (ptr != y.Args[1] || mem != v_2 || !(y.Uses == 1 && clobber(y))) {
            break;
        }
        v.reset(Op386ANDLmodify);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ORLload x [off] {sym} ptr mem) mem)
    // cond: y.Uses==1 && clobber(y)
    // result: (ORLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ORLload || auxIntToInt32(y.AuxInt) != off || auxToSym(y.Aux) != sym) {
            break;
        }
        mem = y.Args[2];
        x = y.Args[0];
        if (ptr != y.Args[1] || mem != v_2 || !(y.Uses == 1 && clobber(y))) {
            break;
        }
        v.reset(Op386ORLmodify);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(XORLload x [off] {sym} ptr mem) mem)
    // cond: y.Uses==1 && clobber(y)
    // result: (XORLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386XORLload || auxIntToInt32(y.AuxInt) != off || auxToSym(y.Aux) != sym) {
            break;
        }
        mem = y.Args[2];
        x = y.Args[0];
        if (ptr != y.Args[1] || mem != v_2 || !(y.Uses == 1 && clobber(y))) {
            break;
        }
        v.reset(Op386XORLmodify);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ADDL l:(MOVLload [off] {sym} ptr mem) x) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (ADDLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ADDL) {
            break;
        }
        _ = y.Args[1];
        var y_0 = y.Args[0];
        var y_1 = y.Args[1];
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var l = y_0;
                if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
                    continue;
                (_i0, y_0, y_1) = (_i0 + 1, y_1, y_0);
                }
                mem = l.Args[1];
                if (ptr != l.Args[0]) {
                    continue;
                }
                x = y_1;
                if (mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
                    continue;
                }
                v.reset(Op386ADDLmodify);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(SUBL l:(MOVLload [off] {sym} ptr mem) x) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (SUBLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386SUBL) {
            break;
        }
        x = y.Args[1];
        l = y.Args[0];
        if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
            break;
        }
        mem = l.Args[1];
        if (ptr != l.Args[0] || mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
            break;
        }
        v.reset(Op386SUBLmodify);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ANDL l:(MOVLload [off] {sym} ptr mem) x) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (ANDLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ANDL) {
            break;
        }
        _ = y.Args[1];
        y_0 = y.Args[0];
        y_1 = y.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                l = y_0;
                if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
                    continue;
                (_i0, y_0, y_1) = (_i0 + 1, y_1, y_0);
                }
                mem = l.Args[1];
                if (ptr != l.Args[0]) {
                    continue;
                }
                x = y_1;
                if (mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
                    continue;
                }
                v.reset(Op386ANDLmodify);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ORL l:(MOVLload [off] {sym} ptr mem) x) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (ORLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ORL) {
            break;
        }
        _ = y.Args[1];
        y_0 = y.Args[0];
        y_1 = y.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                l = y_0;
                if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
                    continue;
                (_i0, y_0, y_1) = (_i0 + 1, y_1, y_0);
                }
                mem = l.Args[1];
                if (ptr != l.Args[0]) {
                    continue;
                }
                x = y_1;
                if (mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
                    continue;
                }
                v.reset(Op386ORLmodify);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(XORL l:(MOVLload [off] {sym} ptr mem) x) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (XORLmodify [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386XORL) {
            break;
        }
        _ = y.Args[1];
        y_0 = y.Args[0];
        y_1 = y.Args[1];
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                l = y_0;
                if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
                    continue;
                (_i0, y_0, y_1) = (_i0 + 1, y_1, y_0);
                }
                mem = l.Args[1];
                if (ptr != l.Args[0]) {
                    continue;
                }
                x = y_1;
                if (mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
                    continue;
                }
                v.reset(Op386XORLmodify);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(ptr, x, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ADDLconst [c] l:(MOVLload [off] {sym} ptr mem)) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (ADDLconstmodify [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ADDLconst) {
            break;
        }
        c = auxIntToInt32(y.AuxInt);
        l = y.Args[0];
        if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
            break;
        }
        mem = l.Args[1];
        if (ptr != l.Args[0] || mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
            break;
        }
        v.reset(Op386ADDLconstmodify);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ANDLconst [c] l:(MOVLload [off] {sym} ptr mem)) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (ANDLconstmodify [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ANDLconst) {
            break;
        }
        c = auxIntToInt32(y.AuxInt);
        l = y.Args[0];
        if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
            break;
        }
        mem = l.Args[1];
        if (ptr != l.Args[0] || mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
            break;
        }
        v.reset(Op386ANDLconstmodify);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(ORLconst [c] l:(MOVLload [off] {sym} ptr mem)) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (ORLconstmodify [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386ORLconst) {
            break;
        }
        c = auxIntToInt32(y.AuxInt);
        l = y.Args[0];
        if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
            break;
        }
        mem = l.Args[1];
        if (ptr != l.Args[0] || mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
            break;
        }
        v.reset(Op386ORLconstmodify);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVLstore {sym} [off] ptr y:(XORLconst [c] l:(MOVLload [off] {sym} ptr mem)) mem)
    // cond: y.Uses==1 && l.Uses==1 && clobber(y, l)
    // result: (XORLconstmodify [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        y = v_1;
        if (y.Op != Op386XORLconst) {
            break;
        }
        c = auxIntToInt32(y.AuxInt);
        l = y.Args[0];
        if (l.Op != Op386MOVLload || auxIntToInt32(l.AuxInt) != off || auxToSym(l.Aux) != sym) {
            break;
        }
        mem = l.Args[1];
        if (ptr != l.Args[0] || mem != v_2 || !(y.Uses == 1 && l.Uses == 1 && clobber(y, l))) {
            break;
        }
        v.reset(Op386XORLconstmodify);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVLstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVLstoreconst [sc] {s} (ADDLconst [off] ptr) mem)
    // cond: sc.canAdd32(off)
    // result: (MOVLstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(sc.canAdd32(off))) {
            break;
        }
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVLstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
    // cond: canMergeSym(sym1, sym2) && sc.canAdd32(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVLstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && sc.canAdd32(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVSDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVSDconst [c])
    // cond: config.ctxt.Flag_shared
    // result: (MOVSDconst2 (MOVSDconst1 [c]))
    while (true) {
        var c = auxIntToFloat64(v.AuxInt);
        if (!(config.ctxt.Flag_shared)) {
            break;
        }
        v.reset(Op386MOVSDconst2);
        var v0 = b.NewValue0(v.Pos, Op386MOVSDconst1, typ.UInt32);
        v0.AuxInt = float64ToAuxInt(c);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVSDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVSDload [off1] {sym} (ADDLconst [off2] ptr) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVSDload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVSDload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVSDload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVSDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVSDstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVSDstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVSDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVSDstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVSDstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVSDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVSSconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (MOVSSconst [c])
    // cond: config.ctxt.Flag_shared
    // result: (MOVSSconst2 (MOVSSconst1 [c]))
    while (true) {
        var c = auxIntToFloat32(v.AuxInt);
        if (!(config.ctxt.Flag_shared)) {
            break;
        }
        v.reset(Op386MOVSSconst2);
        var v0 = b.NewValue0(v.Pos, Op386MOVSSconst1, typ.UInt32);
        v0.AuxInt = float32ToAuxInt(c);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVSSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVSSload [off1] {sym} (ADDLconst [off2] ptr) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVSSload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVSSload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVSSload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVSSstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVSSstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVSSstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVSSstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVSSstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVSSstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVSSstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVWLSX(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVWLSX x:(MOVWload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVWLSXload <v.Type> [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        if (x.Op != Op386MOVWload) {
            break;
        }
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, Op386MOVWLSXload, v.Type);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWLSX (ANDLconst [c] x))
    // cond: c & 0x8000 == 0
    // result: (ANDLconst [c & 0x7fff] x)
    while (true) {
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c & 0x8000 == 0)) {
            break;
        }
        v.reset(Op386ANDLconst);
        v.AuxInt = int32ToAuxInt(c & 0x7fff);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVWLSXload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVWLSXload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVWLSX x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVWstore) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(Op386MOVWLSX);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWLSXload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVWLSXload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVWLSXload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVWLZX(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MOVWLZX x:(MOVWload [off] {sym} ptr mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: @x.Block (MOVWload <v.Type> [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        if (x.Op != Op386MOVWload) {
            break;
        }
        var off = auxIntToInt32(x.AuxInt);
        var sym = auxToSym(x.Aux);
        var mem = x.Args[1];
        var ptr = x.Args[0];
        if (!(x.Uses == 1 && clobber(x))) {
            break;
        }
        b = x.Block;
        var v0 = b.NewValue0(x.Pos, Op386MOVWload, v.Type);
        v.copyOf(v0);
        v0.AuxInt = int32ToAuxInt(off);
        v0.Aux = symToAux(sym);
        v0.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWLZX (ANDLconst [c] x))
    // result: (ANDLconst [c & 0xffff] x)
    while (true) {
        if (v_0.Op != Op386ANDLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(Op386ANDLconst);
        v.AuxInt = int32ToAuxInt(c & 0xffff);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVWload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVWLZX x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVWstore) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(Op386MOVWLZX);
        v.AddArg(x);
        return true;
    } 
    // match: (MOVWload [off1] {sym} (ADDLconst [off2] ptr) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVWload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        mem = v_1;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (MOVWload [off] {sym} (SB) _)
    // cond: symIsRO(sym)
    // result: (MOVLconst [int32(read16(sym, int64(off), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpSB || !(symIsRO(sym))) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(int32(read16(sym, int64(off), config.ctxt.Arch.ByteOrder)));
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVWstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVWstore [off] {sym} ptr (MOVWLSX x) mem)
    // result: (MOVWstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var ptr = v_0;
        if (v_1.Op != Op386MOVWLSX) {
            break;
        }
        var x = v_1.Args[0];
        var mem = v_2;
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVWstore [off] {sym} ptr (MOVWLZX x) mem)
    // result: (MOVWstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != Op386MOVWLZX) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;
    } 
    // match: (MOVWstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MOVWstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        var val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (MOVWstore [off] {sym} ptr (MOVLconst [c]) mem)
    // result: (MOVWstoreconst [makeValAndOff(c,off)] {sym} ptr mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        mem = v_2;
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(c, off));
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        var @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p (SHRLconst [16] w) x:(MOVWstore [i-2] {s} p w mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVLstore [i-2] {s} p w mem)
    while (true) {
        var i = auxIntToInt32(v.AuxInt);
        var s = auxToSym(v.Aux);
        var p = v_0;
        if (v_1.Op != Op386SHRLconst || auxIntToInt32(v_1.AuxInt) != 16) {
            break;
        }
        var w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVWstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0] || w != x.Args[1] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p (SHRLconst [j] w) x:(MOVWstore [i-2] {s} p w0:(SHRLconst [j-16] w) mem))
    // cond: x.Uses == 1 && clobber(x)
    // result: (MOVLstore [i-2] {s} p w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        if (v_1.Op != Op386SHRLconst) {
            break;
        }
        var j = auxIntToInt32(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVWstore || auxIntToInt32(x.AuxInt) != i - 2 || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        if (p != x.Args[0]) {
            break;
        }
        var w0 = x.Args[1];
        if (w0.Op != Op386SHRLconst || auxIntToInt32(w0.AuxInt) != j - 16 || w != w0.Args[0] || !(x.Uses == 1 && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(i - 2);
        v.Aux = symToAux(s);
        v.AddArg3(p, w0, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p1 (SHRLconst [16] w) x:(MOVWstore [i] {s} p0 w mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 2) && clobber(x)
    // result: (MOVLstore [i] {s} p0 w mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        var p1 = v_0;
        if (v_1.Op != Op386SHRLconst || auxIntToInt32(v_1.AuxInt) != 16) {
            break;
        }
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVWstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        var p0 = x.Args[0];
        if (w != x.Args[1] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 2) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w, mem);
        return true;
    } 
    // match: (MOVWstore [i] {s} p1 (SHRLconst [j] w) x:(MOVWstore [i] {s} p0 w0:(SHRLconst [j-16] w) mem))
    // cond: x.Uses == 1 && sequentialAddresses(p0, p1, 2) && clobber(x)
    // result: (MOVLstore [i] {s} p0 w0 mem)
    while (true) {
        i = auxIntToInt32(v.AuxInt);
        s = auxToSym(v.Aux);
        p1 = v_0;
        if (v_1.Op != Op386SHRLconst) {
            break;
        }
        j = auxIntToInt32(v_1.AuxInt);
        w = v_1.Args[0];
        x = v_2;
        if (x.Op != Op386MOVWstore || auxIntToInt32(x.AuxInt) != i || auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[2];
        p0 = x.Args[0];
        w0 = x.Args[1];
        if (w0.Op != Op386SHRLconst || auxIntToInt32(w0.AuxInt) != j - 16 || w != w0.Args[0] || !(x.Uses == 1 && sequentialAddresses(p0, p1, 2) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(i);
        v.Aux = symToAux(s);
        v.AddArg3(p0, w0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MOVWstoreconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVWstoreconst [sc] {s} (ADDLconst [off] ptr) mem)
    // cond: sc.canAdd32(off)
    // result: (MOVWstoreconst [sc.addOffset32(off)] {s} ptr mem)
    while (true) {
        var sc = auxIntToValAndOff(v.AuxInt);
        var s = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        if (!(sc.canAdd32(off))) {
            break;
        }
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(s);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
    // cond: canMergeSym(sym1, sym2) && sc.canAdd32(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MOVWstoreconst [sc.addOffset32(off)] {mergeSym(sym1, sym2)} ptr mem)
    while (true) {
        sc = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2) && sc.canAdd32(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(sc.addOffset32(off));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (MOVWstoreconst [c] {s} p x:(MOVWstoreconst [a] {s} p mem))
    // cond: x.Uses == 1 && a.Off() + 2 == c.Off() && clobber(x)
    // result: (MOVLstoreconst [makeValAndOff(a.Val()&0xffff | c.Val()<<16, a.Off())] {s} p mem)
    while (true) {
        var c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p = v_0;
        var x = v_1;
        if (x.Op != Op386MOVWstoreconst) {
            break;
        }
        var a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(x.Uses == 1 && a.Off() + 2 == c.Off() && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xffff | c.Val() << 16, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVWstoreconst [a] {s} p x:(MOVWstoreconst [c] {s} p mem))
    // cond: x.Uses == 1 && ValAndOff(a).Off() + 2 == ValAndOff(c).Off() && clobber(x)
    // result: (MOVLstoreconst [makeValAndOff(a.Val()&0xffff | c.Val()<<16, a.Off())] {s} p mem)
    while (true) {
        a = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        p = v_0;
        x = v_1;
        if (x.Op != Op386MOVWstoreconst) {
            break;
        }
        c = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        if (p != x.Args[0] || !(x.Uses == 1 && ValAndOff(a).Off() + 2 == ValAndOff(c).Off() && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xffff | c.Val() << 16, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p, mem);
        return true;
    } 
    // match: (MOVWstoreconst [c] {s} p1 x:(MOVWstoreconst [a] {s} p0 mem))
    // cond: x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 2) && clobber(x)
    // result: (MOVLstoreconst [makeValAndOff(a.Val()&0xffff | c.Val()<<16, a.Off())] {s} p0 mem)
    while (true) {
        c = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        var p1 = v_0;
        x = v_1;
        if (x.Op != Op386MOVWstoreconst) {
            break;
        }
        a = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        var p0 = x.Args[0];
        if (!(x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 2) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xffff | c.Val() << 16, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p0, mem);
        return true;
    } 
    // match: (MOVWstoreconst [a] {s} p0 x:(MOVWstoreconst [c] {s} p1 mem))
    // cond: x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 2) && clobber(x)
    // result: (MOVLstoreconst [makeValAndOff(a.Val()&0xffff | c.Val()<<16, a.Off())] {s} p0 mem)
    while (true) {
        a = auxIntToValAndOff(v.AuxInt);
        s = auxToSym(v.Aux);
        p0 = v_0;
        x = v_1;
        if (x.Op != Op386MOVWstoreconst) {
            break;
        }
        c = auxIntToValAndOff(x.AuxInt);
        if (auxToSym(x.Aux) != s) {
            break;
        }
        mem = x.Args[1];
        p1 = x.Args[0];
        if (!(x.Uses == 1 && a.Off() == c.Off() && sequentialAddresses(p0, p1, 2) && clobber(x))) {
            break;
        }
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(a.Val() & 0xffff | c.Val() << 16, a.Off()));
        v.Aux = symToAux(s);
        v.AddArg2(p0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MULL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULL x (MOVLconst [c]))
    // result: (MULLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(Op386MULLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (MULL x l:(MOVLload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (MULLload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVLload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386MULLload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386MULLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MULLconst [c] (MULLconst [d] x))
    // result: (MULLconst [c * d] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MULLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(Op386MULLconst);
        v.AuxInt = int32ToAuxInt(c * d);
        v.AddArg(x);
        return true;
    } 
    // match: (MULLconst [-9] x)
    // result: (NEGL (LEAL8 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -9) {
            break;
        }
        x = v_0;
        v.reset(Op386NEGL);
        var v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MULLconst [-5] x)
    // result: (NEGL (LEAL4 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -5) {
            break;
        }
        x = v_0;
        v.reset(Op386NEGL);
        v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MULLconst [-3] x)
    // result: (NEGL (LEAL2 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -3) {
            break;
        }
        x = v_0;
        v.reset(Op386NEGL);
        v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MULLconst [-1] x)
    // result: (NEGL x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != -1) {
            break;
        }
        x = v_0;
        v.reset(Op386NEGL);
        v.AddArg(x);
        return true;
    } 
    // match: (MULLconst [0] _)
    // result: (MOVLconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (MULLconst [1] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 1) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (MULLconst [3] x)
    // result: (LEAL2 x x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 3) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL2);
        v.AddArg2(x, x);
        return true;
    } 
    // match: (MULLconst [5] x)
    // result: (LEAL4 x x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 5) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL4);
        v.AddArg2(x, x);
        return true;
    } 
    // match: (MULLconst [7] x)
    // result: (LEAL2 x (LEAL2 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 7) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL2);
        v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [9] x)
    // result: (LEAL8 x x)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 9) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v.AddArg2(x, x);
        return true;
    } 
    // match: (MULLconst [11] x)
    // result: (LEAL2 x (LEAL4 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 11) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL2);
        v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [13] x)
    // result: (LEAL4 x (LEAL2 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 13) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL4);
        v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [19] x)
    // result: (LEAL2 x (LEAL8 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 19) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL2);
        v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [21] x)
    // result: (LEAL4 x (LEAL4 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 21) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL4);
        v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [25] x)
    // result: (LEAL8 x (LEAL2 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 25) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [27] x)
    // result: (LEAL8 (LEAL2 <v.Type> x x) (LEAL2 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 27) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(v0, v0);
        return true;
    } 
    // match: (MULLconst [37] x)
    // result: (LEAL4 x (LEAL8 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 37) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL4);
        v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [41] x)
    // result: (LEAL8 x (LEAL4 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 41) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [45] x)
    // result: (LEAL8 (LEAL4 <v.Type> x x) (LEAL4 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 45) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(v0, v0);
        return true;
    } 
    // match: (MULLconst [73] x)
    // result: (LEAL8 x (LEAL8 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 73) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (MULLconst [81] x)
    // result: (LEAL8 (LEAL8 <v.Type> x x) (LEAL8 <v.Type> x x))
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 81) {
            break;
        }
        x = v_0;
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
        v0.AddArg2(x, x);
        v.AddArg2(v0, v0);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: isPowerOfTwo32(c+1) && c >= 15
    // result: (SUBL (SHLLconst <v.Type> [int32(log32(c+1))] x) x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c + 1) && c >= 15)) {
            break;
        }
        v.reset(Op386SUBL);
        v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c + 1)));
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: isPowerOfTwo32(c-1) && c >= 17
    // result: (LEAL1 (SHLLconst <v.Type> [int32(log32(c-1))] x) x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c - 1) && c >= 17)) {
            break;
        }
        v.reset(Op386LEAL1);
        v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 1)));
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: isPowerOfTwo32(c-2) && c >= 34
    // result: (LEAL2 (SHLLconst <v.Type> [int32(log32(c-2))] x) x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c - 2) && c >= 34)) {
            break;
        }
        v.reset(Op386LEAL2);
        v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 2)));
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: isPowerOfTwo32(c-4) && c >= 68
    // result: (LEAL4 (SHLLconst <v.Type> [int32(log32(c-4))] x) x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c - 4) && c >= 68)) {
            break;
        }
        v.reset(Op386LEAL4);
        v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 4)));
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: isPowerOfTwo32(c-8) && c >= 136
    // result: (LEAL8 (SHLLconst <v.Type> [int32(log32(c-8))] x) x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(isPowerOfTwo32(c - 8) && c >= 136)) {
            break;
        }
        v.reset(Op386LEAL8);
        v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 8)));
        v0.AddArg(x);
        v.AddArg2(v0, x);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: c%3 == 0 && isPowerOfTwo32(c/3)
    // result: (SHLLconst [int32(log32(c/3))] (LEAL2 <v.Type> x x))
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c % 3 == 0 && isPowerOfTwo32(c / 3))) {
            break;
        }
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(int32(log32(c / 3)));
        v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: c%5 == 0 && isPowerOfTwo32(c/5)
    // result: (SHLLconst [int32(log32(c/5))] (LEAL4 <v.Type> x x))
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c % 5 == 0 && isPowerOfTwo32(c / 5))) {
            break;
        }
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(int32(log32(c / 5)));
        v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MULLconst [c] x)
    // cond: c%9 == 0 && isPowerOfTwo32(c/9)
    // result: (SHLLconst [int32(log32(c/9))] (LEAL8 <v.Type> x x))
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c % 9 == 0 && isPowerOfTwo32(c / 9))) {
            break;
        }
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(int32(log32(c / 9)));
        v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    } 
    // match: (MULLconst [c] (MOVLconst [d]))
    // result: (MOVLconst [c*d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(c * d);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MULLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MULLload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MULLload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MULLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (MULLload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MULLload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MULLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MULSD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULSD x l:(MOVSDload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (MULSDload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVSDload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386MULSDload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386MULSDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MULSDload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MULSDload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MULSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (MULSDload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MULSDload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MULSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386MULSS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULSS x l:(MOVSSload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (MULSSload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVSSload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386MULSSload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }

        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386MULSSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MULSSload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (MULSSload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386MULSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (MULSSload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (MULSSload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386MULSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386NEGL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEGL (MOVLconst [c]))
    // result: (MOVLconst [-c])
    while (true) {
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(-c);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386NOTL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NOTL (MOVLconst [c]))
    // result: (MOVLconst [^c])
    while (true) {
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(~c);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ORL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ORL x (MOVLconst [c]))
    // result: (ORLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(Op386ORLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( ORL (SHLLconst [c] x) (SHRLconst [d] x))
    // cond: d == 32-c
    // result: (ROLLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRLconst) {
                    continue;
                }
                var d = auxIntToInt32(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 32 - c)) {
                    continue;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( ORL <t> (SHLLconst x [c]) (SHRWconst x [d]))
    // cond: c < 16 && d == int16(16-c) && t.Size() == 2
    // result: (ROLWconst x [int16(c)])
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRWconst) {
                    continue;
                }
                d = auxIntToInt16(v_1.AuxInt);
                if (x != v_1.Args[0] || !(c < 16 && d == int16(16 - c) && t.Size() == 2)) {
                    continue;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = int16ToAuxInt(int16(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: ( ORL <t> (SHLLconst x [c]) (SHRBconst x [d]))
    // cond: c < 8 && d == int8(8-c) && t.Size() == 1
    // result: (ROLBconst x [int8(c)])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRBconst) {
                    continue;
                }
                d = auxIntToInt8(v_1.AuxInt);
                if (x != v_1.Args[0] || !(c < 8 && d == int8(8 - c) && t.Size() == 1)) {
                    continue;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = int8ToAuxInt(int8(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORL x l:(MOVLload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (ORLload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVLload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386ORLload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORL x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ORL x0:(MOVBload [i0] {s} p mem) s0:(SHLLconst [8] x1:(MOVBload [i1] {s} p mem)))
    // cond: i1 == i0+1 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && mergePoint(b,x0,x1) != nil && clobber(x0, x1, s0)
    // result: @mergePoint(b,x0,x1) (MOVWload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var x0 = v_0;
                if (x0.Op != Op386MOVBload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var i0 = auxIntToInt32(x0.AuxInt);
                var s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                var p = x0.Args[0];
                var s0 = v_1;
                if (s0.Op != Op386SHLLconst || auxIntToInt32(s0.AuxInt) != 8) {
                    continue;
                }
                var x1 = s0.Args[0];
                if (x1.Op != Op386MOVBload) {
                    continue;
                }
                var i1 = auxIntToInt32(x1.AuxInt);
                if (auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                if (p != x1.Args[0] || mem != x1.Args[1] || !(i1 == i0 + 1 && x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && mergePoint(b, x0, x1) != null && clobber(x0, x1, s0))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                var v0 = b.NewValue0(x1.Pos, Op386MOVWload, typ.UInt16);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i0);
                v0.Aux = symToAux(s);
                v0.AddArg2(p, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORL x0:(MOVBload [i] {s} p0 mem) s0:(SHLLconst [8] x1:(MOVBload [i] {s} p1 mem)))
    // cond: x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && sequentialAddresses(p0, p1, 1) && mergePoint(b,x0,x1) != nil && clobber(x0, x1, s0)
    // result: @mergePoint(b,x0,x1) (MOVWload [i] {s} p0 mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x0 = v_0;
                if (x0.Op != Op386MOVBload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var i = auxIntToInt32(x0.AuxInt);
                s = auxToSym(x0.Aux);
                mem = x0.Args[1];
                var p0 = x0.Args[0];
                s0 = v_1;
                if (s0.Op != Op386SHLLconst || auxIntToInt32(s0.AuxInt) != 8) {
                    continue;
                }
                x1 = s0.Args[0];
                if (x1.Op != Op386MOVBload || auxIntToInt32(x1.AuxInt) != i || auxToSym(x1.Aux) != s) {
                    continue;
                }
                _ = x1.Args[1];
                var p1 = x1.Args[0];
                if (mem != x1.Args[1] || !(x0.Uses == 1 && x1.Uses == 1 && s0.Uses == 1 && sequentialAddresses(p0, p1, 1) && mergePoint(b, x0, x1) != null && clobber(x0, x1, s0))) {
                    continue;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(x1.Pos, Op386MOVWload, typ.UInt16);
                v.copyOf(v0);
                v0.AuxInt = int32ToAuxInt(i);
                v0.Aux = symToAux(s);
                v0.AddArg2(p0, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORL o0:(ORL x0:(MOVWload [i0] {s} p mem) s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem))) s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)))
    // cond: i2 == i0+2 && i3 == i0+3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && o0.Uses == 1 && mergePoint(b,x0,x1,x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                var o0 = v_0;
                if (o0.Op != Op386ORL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = o0.Args[1];
                var o0_0 = o0.Args[0];
                var o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    nint _i1 = 0;

                    while (_i1 <= 1) {
                        x0 = o0_0;
                        if (x0.Op != Op386MOVWload) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        i0 = auxIntToInt32(x0.AuxInt);
                        s = auxToSym(x0.Aux);
                        mem = x0.Args[1];
                        p = x0.Args[0];
                        s0 = o0_1;
                        if (s0.Op != Op386SHLLconst || auxIntToInt32(s0.AuxInt) != 16) {
                            continue;
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != Op386MOVBload) {
                            continue;
                        }
                        var i2 = auxIntToInt32(x1.AuxInt);
                        if (auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        if (p != x1.Args[0] || mem != x1.Args[1]) {
                            continue;
                        }
                        var s1 = v_1;
                        if (s1.Op != Op386SHLLconst || auxIntToInt32(s1.AuxInt) != 24) {
                            continue;
                        }
                        var x2 = s1.Args[0];
                        if (x2.Op != Op386MOVBload) {
                            continue;
                        }
                        var i3 = auxIntToInt32(x2.AuxInt);
                        if (auxToSym(x2.Aux) != s) {
                            continue;
                        }
                        _ = x2.Args[1];
                        if (p != x2.Args[0] || mem != x2.Args[1] || !(i2 == i0 + 2 && i3 == i0 + 3 && x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && o0.Uses == 1 && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x2.Pos, Op386MOVLload, typ.UInt32);
                        v.copyOf(v0);
                        v0.AuxInt = int32ToAuxInt(i0);
                        v0.Aux = symToAux(s);
                        v0.AddArg2(p, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (ORL o0:(ORL x0:(MOVWload [i] {s} p0 mem) s0:(SHLLconst [16] x1:(MOVBload [i] {s} p1 mem))) s1:(SHLLconst [24] x2:(MOVBload [i] {s} p2 mem)))
    // cond: x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && o0.Uses == 1 && sequentialAddresses(p0, p1, 2) && sequentialAddresses(p1, p2, 1) && mergePoint(b,x0,x1,x2) != nil && clobber(x0, x1, x2, s0, s1, o0)
    // result: @mergePoint(b,x0,x1,x2) (MOVLload [i] {s} p0 mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                o0 = v_0;
                if (o0.Op != Op386ORL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                _ = o0.Args[1];
                o0_0 = o0.Args[0];
                o0_1 = o0.Args[1];
                {
                    nint _i1__prev3 = _i1;

                    _i1 = 0;

                    while (_i1 <= 1) {
                        x0 = o0_0;
                        if (x0.Op != Op386MOVWload) {
                            continue;
                        (_i1, o0_0, o0_1) = (_i1 + 1, o0_1, o0_0);
                        }
                        i = auxIntToInt32(x0.AuxInt);
                        s = auxToSym(x0.Aux);
                        mem = x0.Args[1];
                        p0 = x0.Args[0];
                        s0 = o0_1;
                        if (s0.Op != Op386SHLLconst || auxIntToInt32(s0.AuxInt) != 16) {
                            continue;
                        }
                        x1 = s0.Args[0];
                        if (x1.Op != Op386MOVBload || auxIntToInt32(x1.AuxInt) != i || auxToSym(x1.Aux) != s) {
                            continue;
                        }
                        _ = x1.Args[1];
                        p1 = x1.Args[0];
                        if (mem != x1.Args[1]) {
                            continue;
                        }
                        s1 = v_1;
                        if (s1.Op != Op386SHLLconst || auxIntToInt32(s1.AuxInt) != 24) {
                            continue;
                        }
                        x2 = s1.Args[0];
                        if (x2.Op != Op386MOVBload || auxIntToInt32(x2.AuxInt) != i || auxToSym(x2.Aux) != s) {
                            continue;
                        }
                        _ = x2.Args[1];
                        var p2 = x2.Args[0];
                        if (mem != x2.Args[1] || !(x0.Uses == 1 && x1.Uses == 1 && x2.Uses == 1 && s0.Uses == 1 && s1.Uses == 1 && o0.Uses == 1 && sequentialAddresses(p0, p1, 2) && sequentialAddresses(p1, p2, 1) && mergePoint(b, x0, x1, x2) != null && clobber(x0, x1, x2, s0, s1, o0))) {
                            continue;
                        }
                        b = mergePoint(b, x0, x1, x2);
                        v0 = b.NewValue0(x2.Pos, Op386MOVLload, typ.UInt32);
                        v.copyOf(v0);
                        v0.AuxInt = int32ToAuxInt(i);
                        v0.Aux = symToAux(s);
                        v0.AddArg2(p0, mem);
                        return true;
                    }


                    _i1 = _i1__prev3;
                }
            }


            _i0 = _i0__prev2;
        }
        break;
    }
    return false;
}
private static bool rewriteValue386_Op386ORLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ORLconst [c] x)
    // cond: c==0
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(c == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (ORLconst [c] _)
    // cond: c==-1
    // result: (MOVLconst [-1])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (!(c == -1)) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(-1);
        return true;
    } 
    // match: (ORLconst [c] (MOVLconst [d]))
    // result: (MOVLconst [c|d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(c | d);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ORLconstmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ORLconstmodify [valoff1] {sym} (ADDLconst [off2] base) mem)
    // cond: valoff1.canAdd32(off2)
    // result: (ORLconstmodify [valoff1.addOffset32(off2)] {sym} base mem)
    while (true) {
        var valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var mem = v_1;
        if (!(valoff1.canAdd32(off2))) {
            break;
        }
        v.reset(Op386ORLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (ORLconstmodify [valoff1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ORLconstmodify [valoff1.addOffset32(off2)] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        mem = v_1;
        if (!(valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ORLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ORLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ORLload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ORLload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ORLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (ORLload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ORLload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ORLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ORLmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (ORLmodify [off1] {sym} (ADDLconst [off2] base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (ORLmodify [off1+off2] {sym} base val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386ORLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (ORLmodify [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (ORLmodify [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386ORLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ROLBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ROLBconst [c] (ROLBconst [d] x))
    // result: (ROLBconst [(c+d)& 7] x)
    while (true) {
        var c = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386ROLBconst) {
            break;
        }
        var d = auxIntToInt8(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(Op386ROLBconst);
        v.AuxInt = int8ToAuxInt((c + d) & 7);
        v.AddArg(x);
        return true;
    } 
    // match: (ROLBconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt8(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ROLLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ROLLconst [c] (ROLLconst [d] x))
    // result: (ROLLconst [(c+d)&31] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386ROLLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(Op386ROLLconst);
        v.AuxInt = int32ToAuxInt((c + d) & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (ROLLconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386ROLWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ROLWconst [c] (ROLWconst [d] x))
    // result: (ROLWconst [(c+d)&15] x)
    while (true) {
        var c = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386ROLWconst) {
            break;
        }
        var d = auxIntToInt16(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(Op386ROLWconst);
        v.AuxInt = int16ToAuxInt((c + d) & 15);
        v.AddArg(x);
        return true;
    } 
    // match: (ROLWconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt16(v.AuxInt) != 0) {
            break;
        }
        x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SARB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SARB x (MOVLconst [c]))
    // result: (SARBconst [int8(min(int64(c&31),7))] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SARBconst);
        v.AuxInt = int8ToAuxInt(int8(min(int64(c & 31), 7)));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SARBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SARBconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt8(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (SARBconst [c] (MOVLconst [d]))
    // result: (MOVLconst [d>>uint64(c)])
    while (true) {
        var c = auxIntToInt8(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(d >> (int)(uint64(c)));
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SARL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SARL x (MOVLconst [c]))
    // result: (SARLconst [c&31] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SARLconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (SARL x (ANDLconst [31] y))
    // result: (SARL x y)
    while (true) {
        x = v_0;
        if (v_1.Op != Op386ANDLconst || auxIntToInt32(v_1.AuxInt) != 31) {
            break;
        }
        var y = v_1.Args[0];
        v.reset(Op386SARL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SARLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SARLconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (SARLconst [c] (MOVLconst [d]))
    // result: (MOVLconst [d>>uint64(c)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(d >> (int)(uint64(c)));
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SARW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SARW x (MOVLconst [c]))
    // result: (SARWconst [int16(min(int64(c&31),15))] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SARWconst);
        v.AuxInt = int16ToAuxInt(int16(min(int64(c & 31), 15)));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SARWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SARWconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt16(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    } 
    // match: (SARWconst [c] (MOVLconst [d]))
    // result: (MOVLconst [d>>uint64(c)])
    while (true) {
        var c = auxIntToInt16(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(d >> (int)(uint64(c)));
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SBBL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SBBL x (MOVLconst [c]) f)
    // result: (SBBLconst [c] x f)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var f = v_2;
        v.reset(Op386SBBLconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, f);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SBBLcarrymask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SBBLcarrymask (FlagEQ))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SBBLcarrymask (FlagLT_ULT))
    // result: (MOVLconst [-1])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(-1);
        return true;
    } 
    // match: (SBBLcarrymask (FlagLT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SBBLcarrymask (FlagGT_ULT))
    // result: (MOVLconst [-1])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(-1);
        return true;
    } 
    // match: (SBBLcarrymask (FlagGT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETA (InvertFlags x))
    // result: (SETB x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETB);
        v.AddArg(x);
        return true;
    } 
    // match: (SETA (FlagEQ))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETA (FlagLT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETA (FlagLT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETA (FlagGT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETA (FlagGT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETAE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETAE (InvertFlags x))
    // result: (SETBE x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETBE);
        v.AddArg(x);
        return true;
    } 
    // match: (SETAE (FlagEQ))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETAE (FlagLT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETAE (FlagLT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETAE (FlagGT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETAE (FlagGT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETB (InvertFlags x))
    // result: (SETA x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETA);
        v.AddArg(x);
        return true;
    } 
    // match: (SETB (FlagEQ))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETB (FlagLT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETB (FlagLT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETB (FlagGT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETB (FlagGT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETBE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETBE (InvertFlags x))
    // result: (SETAE x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETAE);
        v.AddArg(x);
        return true;
    } 
    // match: (SETBE (FlagEQ))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETBE (FlagLT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETBE (FlagLT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETBE (FlagGT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETBE (FlagGT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETEQ(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETEQ (InvertFlags x))
    // result: (SETEQ x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETEQ);
        v.AddArg(x);
        return true;
    } 
    // match: (SETEQ (FlagEQ))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETEQ (FlagLT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETEQ (FlagLT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETEQ (FlagGT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETEQ (FlagGT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETG(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETG (InvertFlags x))
    // result: (SETL x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETL);
        v.AddArg(x);
        return true;
    } 
    // match: (SETG (FlagEQ))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETG (FlagLT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETG (FlagLT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETG (FlagGT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETG (FlagGT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETGE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETGE (InvertFlags x))
    // result: (SETLE x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETLE);
        v.AddArg(x);
        return true;
    } 
    // match: (SETGE (FlagEQ))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETGE (FlagLT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETGE (FlagLT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETGE (FlagGT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETGE (FlagGT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETL (InvertFlags x))
    // result: (SETG x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETG);
        v.AddArg(x);
        return true;
    } 
    // match: (SETL (FlagEQ))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETL (FlagLT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETL (FlagLT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETL (FlagGT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETL (FlagGT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETLE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETLE (InvertFlags x))
    // result: (SETGE x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETGE);
        v.AddArg(x);
        return true;
    } 
    // match: (SETLE (FlagEQ))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETLE (FlagLT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETLE (FlagLT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETLE (FlagGT_ULT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETLE (FlagGT_UGT))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SETNE(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SETNE (InvertFlags x))
    // result: (SETNE x)
    while (true) {
        if (v_0.Op != Op386InvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(Op386SETNE);
        v.AddArg(x);
        return true;
    } 
    // match: (SETNE (FlagEQ))
    // result: (MOVLconst [0])
    while (true) {
        if (v_0.Op != Op386FlagEQ) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    } 
    // match: (SETNE (FlagLT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETNE (FlagLT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagLT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETNE (FlagGT_ULT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_ULT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    } 
    // match: (SETNE (FlagGT_UGT))
    // result: (MOVLconst [1])
    while (true) {
        if (v_0.Op != Op386FlagGT_UGT) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(1);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SHLL x (MOVLconst [c]))
    // result: (SHLLconst [c&31] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (SHLL x (ANDLconst [31] y))
    // result: (SHLL x y)
    while (true) {
        x = v_0;
        if (v_1.Op != Op386ANDLconst || auxIntToInt32(v_1.AuxInt) != 31) {
            break;
        }
        var y = v_1.Args[0];
        v.reset(Op386SHLL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHLLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SHLLconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHRB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SHRB x (MOVLconst [c]))
    // cond: c&31 < 8
    // result: (SHRBconst [int8(c&31)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(c & 31 < 8)) {
            break;
        }
        v.reset(Op386SHRBconst);
        v.AuxInt = int8ToAuxInt(int8(c & 31));
        v.AddArg(x);
        return true;
    } 
    // match: (SHRB _ (MOVLconst [c]))
    // cond: c&31 >= 8
    // result: (MOVLconst [0])
    while (true) {
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(c & 31 >= 8)) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHRBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SHRBconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt8(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SHRL x (MOVLconst [c]))
    // result: (SHRLconst [c&31] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SHRLconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    } 
    // match: (SHRL x (ANDLconst [31] y))
    // result: (SHRL x y)
    while (true) {
        x = v_0;
        if (v_1.Op != Op386ANDLconst || auxIntToInt32(v_1.AuxInt) != 31) {
            break;
        }
        var y = v_1.Args[0];
        v.reset(Op386SHRL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHRLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SHRLconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHRW(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SHRW x (MOVLconst [c]))
    // cond: c&31 < 16
    // result: (SHRWconst [int16(c&31)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(c & 31 < 16)) {
            break;
        }
        v.reset(Op386SHRWconst);
        v.AuxInt = int16ToAuxInt(int16(c & 31));
        v.AddArg(x);
        return true;
    } 
    // match: (SHRW _ (MOVLconst [c]))
    // cond: c&31 >= 16
    // result: (MOVLconst [0])
    while (true) {
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        if (!(c & 31 >= 16)) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SHRWconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SHRWconst x [0])
    // result: x
    while (true) {
        if (auxIntToInt16(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBL x (MOVLconst [c]))
    // result: (SUBLconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SUBLconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    } 
    // match: (SUBL (MOVLconst [c]) x)
    // result: (NEGL (SUBLconst <v.Type> x [c]))
    while (true) {
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        v.reset(Op386NEGL);
        var v0 = b.NewValue0(v.Pos, Op386SUBLconst, v.Type);
        v0.AuxInt = int32ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    } 
    // match: (SUBL x l:(MOVLload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (SUBLload x [off] {sym} ptr mem)
    while (true) {
        x = v_0;
        var l = v_1;
        if (l.Op != Op386MOVLload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
            break;
        }
        v.reset(Op386SUBLload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    } 
    // match: (SUBL x x)
    // result: (MOVLconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBLcarry(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBLcarry x (MOVLconst [c]))
    // result: (SUBLconstcarry [c] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386SUBLconstcarry);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SUBLconst [c] x)
    // cond: c==0
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(c == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (SUBLconst [c] x)
    // result: (ADDLconst [-c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        v.reset(Op386ADDLconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValue386_Op386SUBLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (SUBLload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (SUBLload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386SUBLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (SUBLload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (SUBLload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386SUBLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBLmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (SUBLmodify [off1] {sym} (ADDLconst [off2] base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (SUBLmodify [off1+off2] {sym} base val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386SUBLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (SUBLmodify [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (SUBLmodify [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386SUBLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBSD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBSD x l:(MOVSDload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (SUBSDload x [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        var l = v_1;
        if (l.Op != Op386MOVSDload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
            break;
        }
        v.reset(Op386SUBSDload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBSDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (SUBSDload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (SUBSDload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386SUBSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (SUBSDload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (SUBSDload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386SUBSDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBSS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBSS x l:(MOVSSload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (SUBSSload x [off] {sym} ptr mem)
    while (true) {
        var x = v_0;
        var l = v_1;
        if (l.Op != Op386MOVSSload) {
            break;
        }
        var off = auxIntToInt32(l.AuxInt);
        var sym = auxToSym(l.Aux);
        var mem = l.Args[1];
        var ptr = l.Args[0];
        if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
            break;
        }
        v.reset(Op386SUBSSload);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(x, ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386SUBSSload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (SUBSSload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (SUBSSload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386SUBSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (SUBSSload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (SUBSSload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386SUBSSload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386XORL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (XORL x (MOVLconst [c]))
    // result: (XORLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != Op386MOVLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(Op386XORLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORL (SHLLconst [c] x) (SHRLconst [d] x))
    // cond: d == 32-c
    // result: (ROLLconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRLconst) {
                    continue;
                }
                var d = auxIntToInt32(v_1.AuxInt);
                if (x != v_1.Args[0] || !(d == 32 - c)) {
                    continue;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORL <t> (SHLLconst x [c]) (SHRWconst x [d]))
    // cond: c < 16 && d == int16(16-c) && t.Size() == 2
    // result: (ROLWconst x [int16(c)])
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRWconst) {
                    continue;
                }
                d = auxIntToInt16(v_1.AuxInt);
                if (x != v_1.Args[0] || !(c < 16 && d == int16(16 - c) && t.Size() == 2)) {
                    continue;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = int16ToAuxInt(int16(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORL <t> (SHLLconst x [c]) (SHRBconst x [d]))
    // cond: c < 8 && d == int8(8-c) && t.Size() == 1
    // result: (ROLBconst x [int8(c)])
    while (true) {
        t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != Op386SHLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != Op386SHRBconst) {
                    continue;
                }
                d = auxIntToInt8(v_1.AuxInt);
                if (x != v_1.Args[0] || !(c < 8 && d == int8(8 - c) && t.Size() == 1)) {
                    continue;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = int8ToAuxInt(int8(c));
                v.AddArg(x);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORL x l:(MOVLload [off] {sym} ptr mem))
    // cond: canMergeLoadClobber(v, l, x) && clobber(l)
    // result: (XORLload x [off] {sym} ptr mem)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                var l = v_1;
                if (l.Op != Op386MOVLload) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }
                var off = auxIntToInt32(l.AuxInt);
                var sym = auxToSym(l.Aux);
                var mem = l.Args[1];
                var ptr = l.Args[0];
                if (!(canMergeLoadClobber(v, l, x) && clobber(l))) {
                    continue;
                }
                v.reset(Op386XORLload);
                v.AuxInt = int32ToAuxInt(off);
                v.Aux = symToAux(sym);
                v.AddArg3(x, ptr, mem);
                return true;
            }


            _i0 = _i0__prev2;
        }
        break;
    } 
    // match: (XORL x x)
    // result: (MOVLconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386XORLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (XORLconst [c] (XORLconst [d] x))
    // result: (XORLconst [c ^ d] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386XORLconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(Op386XORLconst);
        v.AuxInt = int32ToAuxInt(c ^ d);
        v.AddArg(x);
        return true;
    } 
    // match: (XORLconst [c] x)
    // cond: c==0
    // result: x
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(c == 0)) {
            break;
        }
        v.copyOf(x);
        return true;
    } 
    // match: (XORLconst [c] (MOVLconst [d]))
    // result: (MOVLconst [c^d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != Op386MOVLconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(c ^ d);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386XORLconstmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (XORLconstmodify [valoff1] {sym} (ADDLconst [off2] base) mem)
    // cond: valoff1.canAdd32(off2)
    // result: (XORLconstmodify [valoff1.addOffset32(off2)] {sym} base mem)
    while (true) {
        var valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var mem = v_1;
        if (!(valoff1.canAdd32(off2))) {
            break;
        }
        v.reset(Op386XORLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(sym);
        v.AddArg2(base, mem);
        return true;
    } 
    // match: (XORLconstmodify [valoff1] {sym1} (LEAL [off2] {sym2} base) mem)
    // cond: valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (XORLconstmodify [valoff1.addOffset32(off2)] {mergeSym(sym1,sym2)} base mem)
    while (true) {
        valoff1 = auxIntToValAndOff(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        mem = v_1;
        if (!(valoff1.canAdd32(off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386XORLconstmodify);
        v.AuxInt = valAndOffToAuxInt(valoff1.addOffset32(off2));
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386XORLload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (XORLload [off1] {sym} val (ADDLconst [off2] base) mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (XORLload [off1+off2] {sym} val base mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        var val = v_0;
        if (v_1.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_1.AuxInt);
        var @base = v_1.Args[0];
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386XORLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(val, base, mem);
        return true;
    } 
    // match: (XORLload [off1] {sym1} val (LEAL [off2] {sym2} base) mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (XORLload [off1+off2] {mergeSym(sym1,sym2)} val base mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        val = v_0;
        if (v_1.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        var sym2 = auxToSym(v_1.Aux);
        @base = v_1.Args[0];
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386XORLload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(val, base, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_Op386XORLmodify(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (XORLmodify [off1] {sym} (ADDLconst [off2] base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2))
    // result: (XORLmodify [off1+off2] {sym} base val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != Op386ADDLconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var @base = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)))) {
            break;
        }
        v.reset(Op386XORLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(base, val, mem);
        return true;
    } 
    // match: (XORLmodify [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
    // cond: is32Bit(int64(off1)+int64(off2)) && canMergeSym(sym1, sym2) && (base.Op != OpSB || !config.ctxt.Flag_shared)
    // result: (XORLmodify [off1+off2] {mergeSym(sym1,sym2)} base val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != Op386LEAL) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        @base = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(is32Bit(int64(off1) + int64(off2)) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared))) {
            break;
        }
        v.reset(Op386XORLmodify);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(base, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Addr {sym} base)
    // result: (LEAL {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(Op386LEAL);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValue386_OpConst16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const16 [c])
    // result: (MOVLconst [int32(c)])
    while (true) {
        var c = auxIntToInt16(v.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;
    }
}
private static bool rewriteValue386_OpConst8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const8 [c])
    // result: (MOVLconst [int32(c)])
    while (true) {
        var c = auxIntToInt8(v.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        return true;
    }
}
private static bool rewriteValue386_OpConstBool(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstBool [c])
    // result: (MOVLconst [b2i32(c)])
    while (true) {
        var c = auxIntToBool(v.AuxInt);
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(b2i32(c));
        return true;
    }
}
private static bool rewriteValue386_OpConstNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstNil)
    // result: (MOVLconst [0])
    while (true) {
        v.reset(Op386MOVLconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
}
private static bool rewriteValue386_OpCtz16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz16 x)
    // result: (BSFL (ORLconst <typ.UInt32> [0x10000] x))
    while (true) {
        var x = v_0;
        v.reset(Op386BSFL);
        var v0 = b.NewValue0(v.Pos, Op386ORLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0x10000);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 x y)
    // result: (DIVW (SignExt8to16 x) (SignExt8to16 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386DIVW);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValue386_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u x y)
    // result: (DIVWU (ZeroExt8to16 x) (ZeroExt8to16 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386DIVWU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValue386_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq16 x y)
    // result: (SETEQ (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQ);
        var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32 x y)
    // result: (SETEQ (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQ);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpEq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32F x y)
    // result: (SETEQF (UCOMISS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpEq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq64F x y)
    // result: (SETEQF (UCOMISD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq8 x y)
    // result: (SETEQ (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQ);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpEqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (EqB x y)
    // result: (SETEQ (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQ);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpEqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (EqPtr x y)
    // result: (SETEQ (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETEQ);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpIsInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsInBounds idx len)
    // result: (SETB (CMPL idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(Op386SETB);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsNonNil p)
    // result: (SETNE (TESTL p p))
    while (true) {
        var p = v_0;
        v.reset(Op386SETNE);
        var v0 = b.NewValue0(v.Pos, Op386TESTL, types.TypeFlags);
        v0.AddArg2(p, p);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpIsSliceInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsSliceInBounds idx len)
    // result: (SETBE (CMPL idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(Op386SETBE);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq16 x y)
    // result: (SETLE (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETLE);
        var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq16U x y)
    // result: (SETBE (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETBE);
        var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32 x y)
    // result: (SETLE (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETLE);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32F x y)
    // result: (SETGEF (UCOMISS y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETGEF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32U x y)
    // result: (SETBE (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETBE);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq64F x y)
    // result: (SETGEF (UCOMISD y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETGEF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq8 x y)
    // result: (SETLE (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETLE);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq8U x y)
    // result: (SETBE (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETBE);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less16 x y)
    // result: (SETL (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETL);
        var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less16U x y)
    // result: (SETB (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETB);
        var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32 x y)
    // result: (SETL (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETL);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32F x y)
    // result: (SETGF (UCOMISS y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETGF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32U x y)
    // result: (SETB (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETB);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less64F x y)
    // result: (SETGF (UCOMISD y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETGF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less8 x y)
    // result: (SETL (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETL);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less8U x y)
    // result: (SETB (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETB);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Load <t> ptr mem)
    // cond: (is32BitInt(t) || isPtr(t))
    // result: (MOVLload ptr mem)
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(is32BitInt(t) || isPtr(t))) {
            break;
        }
        v.reset(Op386MOVLload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is16BitInt(t)
    // result: (MOVWload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t))) {
            break;
        }
        v.reset(Op386MOVWload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: (t.IsBoolean() || is8BitInt(t))
    // result: (MOVBload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(t.IsBoolean() || is8BitInt(t))) {
            break;
        }
        v.reset(Op386MOVBload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is32BitFloat(t)
    // result: (MOVSSload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitFloat(t))) {
            break;
        }
        v.reset(Op386MOVSSload);
        v.AddArg2(ptr, mem);
        return true;
    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitFloat(t)
    // result: (MOVSDload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitFloat(t))) {
            break;
        }
        v.reset(Op386MOVSDload);
        v.AddArg2(ptr, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLocalAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LocalAddr {sym} base _)
    // result: (LEAL {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(Op386LEAL);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }
}
private static bool rewriteValue386_OpLsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v2.AuxInt = int16ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh16x16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh16x32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh16x64 x (Const64 [c]))
    // cond: uint64(c) < 16
    // result: (SHLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh16x64 _ (Const64 [c]))
    // cond: uint64(c) >= 16
    // result: (Const16 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v2.AuxInt = int8ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh16x8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v2.AuxInt = int16ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh32x16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh32x32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh32x64 x (Const64 [c]))
    // cond: uint64(c) < 32
    // result: (SHLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh32x64 _ (Const64 [c]))
    // cond: uint64(c) >= 32
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v2.AuxInt = int8ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh32x8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v2.AuxInt = int16ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh8x16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh8x32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh8x64 x (Const64 [c]))
    // cond: uint64(c) < 8
    // result: (SHLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(Op386SHLLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Lsh8x64 _ (Const64 [c]))
    // cond: uint64(c) >= 8
    // result: (Const8 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpLsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v2.AuxInt = int8ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Lsh8x8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHLL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHLL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8 x y)
    // result: (MODW (SignExt8to16 x) (SignExt8to16 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386MODW);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValue386_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8u x y)
    // result: (MODWU (ZeroExt8to16 x) (ZeroExt8to16 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386MODWU);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }
}
private static bool rewriteValue386_OpMove(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Move [0] _ _ mem)
    // result: mem
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var mem = v_2;
        v.copyOf(mem);
        return true;
    } 
    // match: (Move [1] dst src mem)
    // result: (MOVBstore dst (MOVBload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var dst = v_0;
        var src = v_1;
        mem = v_2;
        v.reset(Op386MOVBstore);
        var v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [2] dst src mem)
    // result: (MOVWstore dst (MOVWload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVWstore);
        v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [4] dst src mem)
    // result: (MOVLstore dst (MOVLload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVLstore);
        v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;
    } 
    // match: (Move [3] dst src mem)
    // result: (MOVBstore [2] dst (MOVBload [2] src mem) (MOVWstore dst (MOVWload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        var v1 = b.NewValue0(v.Pos, Op386MOVWstore, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [5] dst src mem)
    // result: (MOVBstore [4] dst (MOVBload [4] src mem) (MOVLstore dst (MOVLload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVBstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [6] dst src mem)
    // result: (MOVWstore [4] dst (MOVWload [4] src mem) (MOVLstore dst (MOVLload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVWstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [7] dst src mem)
    // result: (MOVLstore [3] dst (MOVLload [3] src mem) (MOVLstore dst (MOVLload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(3);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [8] dst src mem)
    // result: (MOVLstore [4] dst (MOVLload [4] src mem) (MOVLstore dst (MOVLload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(Op386MOVLstore);
        v.AuxInt = int32ToAuxInt(4);
        v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(4);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 8 && s%4 != 0
    // result: (Move [s-s%4] (ADDLconst <dst.Type> dst [int32(s%4)]) (ADDLconst <src.Type> src [int32(s%4)]) (MOVLstore dst (MOVLload src mem) mem))
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 8 && s % 4 != 0)) {
            break;
        }
        v.reset(OpMove);
        v.AuxInt = int64ToAuxInt(s - s % 4);
        v0 = b.NewValue0(v.Pos, Op386ADDLconst, dst.Type);
        v0.AuxInt = int32ToAuxInt(int32(s % 4));
        v0.AddArg(dst);
        v1 = b.NewValue0(v.Pos, Op386ADDLconst, src.Type);
        v1.AuxInt = int32ToAuxInt(int32(s % 4));
        v1.AddArg(src);
        v2 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
        var v3 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
        v3.AddArg2(src, mem);
        v2.AddArg3(dst, v3, mem);
        v.AddArg3(v0, v1, v2);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: s > 8 && s <= 4*128 && s%4 == 0 && !config.noDuffDevice && logLargeCopy(v, s)
    // result: (DUFFCOPY [10*(128-s/4)] dst src mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s > 8 && s <= 4 * 128 && s % 4 == 0 && !config.noDuffDevice && logLargeCopy(v, s))) {
            break;
        }
        v.reset(Op386DUFFCOPY);
        v.AuxInt = int64ToAuxInt(10 * (128 - s / 4));
        v.AddArg3(dst, src, mem);
        return true;
    } 
    // match: (Move [s] dst src mem)
    // cond: (s > 4*128 || config.noDuffDevice) && s%4 == 0 && logLargeCopy(v, s)
    // result: (REPMOVSL dst src (MOVLconst [int32(s/4)]) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!((s > 4 * 128 || config.noDuffDevice) && s % 4 == 0 && logLargeCopy(v, s))) {
            break;
        }
        v.reset(Op386REPMOVSL);
        v0 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(s / 4));
        v.AddArg4(dst, src, v0, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpNeg32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neg32F x)
    // result: (PXOR x (MOVSSconst <typ.Float32> [float32(math.Copysign(0, -1))]))
    while (true) {
        var x = v_0;
        v.reset(Op386PXOR);
        var v0 = b.NewValue0(v.Pos, Op386MOVSSconst, typ.Float32);
        v0.AuxInt = float32ToAuxInt(float32(math.Copysign(0, -1)));
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeg64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neg64F x)
    // result: (PXOR x (MOVSDconst <typ.Float64> [math.Copysign(0, -1)]))
    while (true) {
        var x = v_0;
        v.reset(Op386PXOR);
        var v0 = b.NewValue0(v.Pos, Op386MOVSDconst, typ.Float64);
        v0.AuxInt = float64ToAuxInt(math.Copysign(0, -1));
        v.AddArg2(x, v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq16 x y)
    // result: (SETNE (CMPW x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNE);
        var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32 x y)
    // result: (SETNE (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNE);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32F x y)
    // result: (SETNEF (UCOMISS x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNEF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq64F x y)
    // result: (SETNEF (UCOMISD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNEF);
        var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq8 x y)
    // result: (SETNE (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNE);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (NeqB x y)
    // result: (SETNE (CMPB x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNE);
        var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNeqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (NeqPtr x y)
    // result: (SETNE (CMPL x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(Op386SETNE);
        var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpNot(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Not x)
    // result: (XORLconst [1] x)
    while (true) {
        var x = v_0;
        v.reset(Op386XORLconst);
        v.AuxInt = int32ToAuxInt(1);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValue386_OpOffPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (OffPtr [off] ptr)
    // result: (ADDLconst [int32(off)] ptr)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        var ptr = v_0;
        v.reset(Op386ADDLconst);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;
    }
}
private static bool rewriteValue386_OpPanicBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (PanicBounds [kind] x y mem)
    // cond: boundsABI(kind) == 0
    // result: (LoweredPanicBoundsA [kind] x y mem)
    while (true) {
        var kind = auxIntToInt64(v.AuxInt);
        var x = v_0;
        var y = v_1;
        var mem = v_2;
        if (!(boundsABI(kind) == 0)) {
            break;
        }
        v.reset(Op386LoweredPanicBoundsA);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    } 
    // match: (PanicBounds [kind] x y mem)
    // cond: boundsABI(kind) == 1
    // result: (LoweredPanicBoundsB [kind] x y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        x = v_0;
        y = v_1;
        mem = v_2;
        if (!(boundsABI(kind) == 1)) {
            break;
        }
        v.reset(Op386LoweredPanicBoundsB);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    } 
    // match: (PanicBounds [kind] x y mem)
    // cond: boundsABI(kind) == 2
    // result: (LoweredPanicBoundsC [kind] x y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        x = v_0;
        y = v_1;
        mem = v_2;
        if (!(boundsABI(kind) == 2)) {
            break;
        }
        v.reset(Op386LoweredPanicBoundsC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpPanicExtend(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (PanicExtend [kind] hi lo y mem)
    // cond: boundsABI(kind) == 0
    // result: (LoweredPanicExtendA [kind] hi lo y mem)
    while (true) {
        var kind = auxIntToInt64(v.AuxInt);
        var hi = v_0;
        var lo = v_1;
        var y = v_2;
        var mem = v_3;
        if (!(boundsABI(kind) == 0)) {
            break;
        }
        v.reset(Op386LoweredPanicExtendA);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;
    } 
    // match: (PanicExtend [kind] hi lo y mem)
    // cond: boundsABI(kind) == 1
    // result: (LoweredPanicExtendB [kind] hi lo y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        hi = v_0;
        lo = v_1;
        y = v_2;
        mem = v_3;
        if (!(boundsABI(kind) == 1)) {
            break;
        }
        v.reset(Op386LoweredPanicExtendB);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;
    } 
    // match: (PanicExtend [kind] hi lo y mem)
    // cond: boundsABI(kind) == 2
    // result: (LoweredPanicExtendC [kind] hi lo y mem)
    while (true) {
        kind = auxIntToInt64(v.AuxInt);
        hi = v_0;
        lo = v_1;
        y = v_2;
        mem = v_3;
        if (!(boundsABI(kind) == 2)) {
            break;
        }
        v.reset(Op386LoweredPanicExtendC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRotateLeft16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft16 x (MOVLconst [c]))
    // result: (ROLWconst [int16(c&15)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386ROLWconst);
        v.AuxInt = int16ToAuxInt(int16(c & 15));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRotateLeft32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft32 x (MOVLconst [c]))
    // result: (ROLLconst [c&31] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386ROLLconst);
        v.AuxInt = int32ToAuxInt(c & 31);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRotateLeft8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RotateLeft8 x (MOVLconst [c]))
    // result: (ROLBconst [int8(c&7)] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != Op386MOVLconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(Op386ROLBconst);
        v.AuxInt = int8ToAuxInt(int8(c & 7));
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16Ux16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRW <t> x y) (SBBLcarrymask <t> (CMPWconst y [16])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v2.AuxInt = int16ToAuxInt(16);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh16Ux16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRW <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRW);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16Ux32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRW <t> x y) (SBBLcarrymask <t> (CMPLconst y [16])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(16);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh16Ux32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRW <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRW);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh16Ux64 x (Const64 [c]))
    // cond: uint64(c) < 16
    // result: (SHRWconst x [int16(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(Op386SHRWconst);
        v.AuxInt = int16ToAuxInt(int16(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh16Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 16
    // result: (Const16 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpConst16);
        v.AuxInt = int16ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16Ux8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRW <t> x y) (SBBLcarrymask <t> (CMPBconst y [16])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRW, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v2.AuxInt = int8ToAuxInt(16);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh16Ux8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRW <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRW);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16x16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARW <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPWconst y [16])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARW);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v3.AuxInt = int16ToAuxInt(16);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh16x16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARW x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16x32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARW <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPLconst y [16])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARW);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(16);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh16x32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARW x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh16x64 x (Const64 [c]))
    // cond: uint64(c) < 16
    // result: (SARWconst x [int16(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(Op386SARWconst);
        v.AuxInt = int16ToAuxInt(int16(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh16x64 x (Const64 [c]))
    // cond: uint64(c) >= 16
    // result: (SARWconst x [15])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(Op386SARWconst);
        v.AuxInt = int16ToAuxInt(15);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh16x8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARW <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPBconst y [16])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARW);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v3.AuxInt = int8ToAuxInt(16);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh16x8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARW x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARW);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v2.AuxInt = int16ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh32Ux16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh32Ux32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh32Ux64 x (Const64 [c]))
    // cond: uint64(c) < 32
    // result: (SHRLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(Op386SHRLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 32
    // result: (Const32 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpConst32);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRL, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v2.AuxInt = int8ToAuxInt(32);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh32Ux8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRL <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRL);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARL <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPWconst y [32])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARL);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v3.AuxInt = int16ToAuxInt(32);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32x16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARL x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARL <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPLconst y [32])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARL);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(32);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32x32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARL x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh32x64 x (Const64 [c]))
    // cond: uint64(c) < 32
    // result: (SARLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(Op386SARLconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh32x64 x (Const64 [c]))
    // cond: uint64(c) >= 32
    // result: (SARLconst x [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(Op386SARLconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARL <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPBconst y [32])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARL);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v3.AuxInt = int8ToAuxInt(32);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh32x8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARL x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARL);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8Ux16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRB <t> x y) (SBBLcarrymask <t> (CMPWconst y [8])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRB, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v2.AuxInt = int16ToAuxInt(8);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh8Ux16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRB <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRB);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8Ux32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRB <t> x y) (SBBLcarrymask <t> (CMPLconst y [8])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRB, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(8);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh8Ux32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRB <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRB);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh8Ux64 x (Const64 [c]))
    // cond: uint64(c) < 8
    // result: (SHRBconst x [int8(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(Op386SHRBconst);
        v.AuxInt = int8ToAuxInt(int8(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh8Ux64 _ (Const64 [c]))
    // cond: uint64(c) >= 8
    // result: (Const8 [0])
    while (true) {
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpConst8);
        v.AuxInt = int8ToAuxInt(0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8Ux8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (ANDL (SHRB <t> x y) (SBBLcarrymask <t> (CMPBconst y [8])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386ANDL);
        var v0 = b.NewValue0(v.Pos, Op386SHRB, t);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v2.AuxInt = int8ToAuxInt(8);
        v2.AddArg(y);
        v1.AddArg(v2);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Rsh8Ux8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SHRB <t> x y)
    while (true) {
        t = v.Type;
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SHRB);
        v.Type = t;
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x16 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARB <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPWconst y [8])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARB);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
        v3.AuxInt = int16ToAuxInt(8);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh8x16 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARB x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARB);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x32 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARB <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPLconst y [8])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARB);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(8);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh8x32 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARB x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARB);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh8x64 x (Const64 [c]))
    // cond: uint64(c) < 8
    // result: (SARBconst x [int8(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(Op386SARBconst);
        v.AuxInt = int8ToAuxInt(int8(c));
        v.AddArg(x);
        return true;
    } 
    // match: (Rsh8x64 x (Const64 [c]))
    // cond: uint64(c) >= 8
    // result: (SARBconst x [7])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(Op386SARBconst);
        v.AuxInt = int8ToAuxInt(7);
        v.AddArg(x);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpRsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh8x8 <t> x y)
    // cond: !shiftIsBounded(v)
    // result: (SARB <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPBconst y [8])))))
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        if (!(!shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARB);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
        var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
        var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
        var v3 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
        v3.AuxInt = int8ToAuxInt(8);
        v3.AddArg(y);
        v2.AddArg(v3);
        v1.AddArg(v2);
        v0.AddArg2(y, v1);
        v.AddArg2(x, v0);
        return true;
    } 
    // match: (Rsh8x8 <t> x y)
    // cond: shiftIsBounded(v)
    // result: (SARB x y)
    while (true) {
        x = v_0;
        y = v_1;
        if (!(shiftIsBounded(v))) {
            break;
        }
        v.reset(Op386SARB);
        v.AddArg2(x, y);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpSelect0(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Select0 (Mul32uover x y))
    // result: (Select0 <typ.UInt32> (MULLU x y))
    while (true) {
        if (v_0.Op != OpMul32uover) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(OpSelect0);
        v.Type = typ.UInt32;
        var v0 = b.NewValue0(v.Pos, Op386MULLU, types.NewTuple(typ.UInt32, types.TypeFlags));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpSelect1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Select1 (Mul32uover x y))
    // result: (SETO (Select1 <types.TypeFlags> (MULLU x y)))
    while (true) {
        if (v_0.Op != OpMul32uover) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        v.reset(Op386SETO);
        var v0 = b.NewValue0(v.Pos, OpSelect1, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, Op386MULLU, types.NewTuple(typ.UInt32, types.TypeFlags));
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpSignmask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Signmask x)
    // result: (SARLconst x [31])
    while (true) {
        var x = v_0;
        v.reset(Op386SARLconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;
    }
}
private static bool rewriteValue386_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Slicemask <t> x)
    // result: (SARLconst (NEGL <t> x) [31])
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(Op386SARLconst);
        v.AuxInt = int32ToAuxInt(31);
        var v0 = b.NewValue0(v.Pos, Op386NEGL, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteValue386_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8 && is64BitFloat(val.Type)
    // result: (MOVSDstore ptr val mem)
    while (true) {
        var t = auxToType(v.Aux);
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(t.Size() == 8 && is64BitFloat(val.Type))) {
            break;
        }
        v.reset(Op386MOVSDstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && is32BitFloat(val.Type)
    // result: (MOVSSstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && is32BitFloat(val.Type))) {
            break;
        }
        v.reset(Op386MOVSSstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4
    // result: (MOVLstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4)) {
            break;
        }
        v.reset(Op386MOVLstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 2
    // result: (MOVWstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 2)) {
            break;
        }
        v.reset(Op386MOVWstore);
        v.AddArg3(ptr, val, mem);
        return true;
    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 1
    // result: (MOVBstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 1)) {
            break;
        }
        v.reset(Op386MOVBstore);
        v.AddArg3(ptr, val, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpZero(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Zero [0] _ mem)
    // result: mem
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 0) {
            break;
        }
        var mem = v_1;
        v.copyOf(mem);
        return true;
    } 
    // match: (Zero [1] destptr mem)
    // result: (MOVBstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [2] destptr mem)
    // result: (MOVWstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [4] destptr mem)
    // result: (MOVLstoreconst [0] destptr mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(0);
        v.AddArg2(destptr, mem);
        return true;
    } 
    // match: (Zero [3] destptr mem)
    // result: (MOVBstoreconst [makeValAndOff(0,2)] destptr (MOVWstoreconst [makeValAndOff(0,0)] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 2));
        var v0 = b.NewValue0(v.Pos, Op386MOVWstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [5] destptr mem)
    // result: (MOVBstoreconst [makeValAndOff(0,4)] destptr (MOVLstoreconst [makeValAndOff(0,0)] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 5) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVBstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [6] destptr mem)
    // result: (MOVWstoreconst [makeValAndOff(0,4)] destptr (MOVLstoreconst [makeValAndOff(0,0)] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 6) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVWstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [7] destptr mem)
    // result: (MOVLstoreconst [makeValAndOff(0,3)] destptr (MOVLstoreconst [makeValAndOff(0,0)] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 7) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 3));
        v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: s%4 != 0 && s > 4
    // result: (Zero [s-s%4] (ADDLconst destptr [int32(s%4)]) (MOVLstoreconst [0] destptr mem))
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!(s % 4 != 0 && s > 4)) {
            break;
        }
        v.reset(OpZero);
        v.AuxInt = int64ToAuxInt(s - s % 4);
        v0 = b.NewValue0(v.Pos, Op386ADDLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(s % 4));
        v0.AddArg(destptr);
        var v1 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v1.AuxInt = valAndOffToAuxInt(0);
        v1.AddArg2(destptr, mem);
        v.AddArg2(v0, v1);
        return true;
    } 
    // match: (Zero [8] destptr mem)
    // result: (MOVLstoreconst [makeValAndOff(0,4)] destptr (MOVLstoreconst [makeValAndOff(0,0)] destptr mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 8) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v0.AddArg2(destptr, mem);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [12] destptr mem)
    // result: (MOVLstoreconst [makeValAndOff(0,8)] destptr (MOVLstoreconst [makeValAndOff(0,4)] destptr (MOVLstoreconst [makeValAndOff(0,0)] destptr mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 12) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 8));
        v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        v1 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v1.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v1.AddArg2(destptr, mem);
        v0.AddArg2(destptr, v1);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [16] destptr mem)
    // result: (MOVLstoreconst [makeValAndOff(0,12)] destptr (MOVLstoreconst [makeValAndOff(0,8)] destptr (MOVLstoreconst [makeValAndOff(0,4)] destptr (MOVLstoreconst [makeValAndOff(0,0)] destptr mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 16) {
            break;
        }
        destptr = v_0;
        mem = v_1;
        v.reset(Op386MOVLstoreconst);
        v.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 12));
        v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v0.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 8));
        v1 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v1.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 4));
        var v2 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
        v2.AuxInt = valAndOffToAuxInt(makeValAndOff(0, 0));
        v2.AddArg2(destptr, mem);
        v1.AddArg2(destptr, v2);
        v0.AddArg2(destptr, v1);
        v.AddArg2(destptr, v0);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: s > 16 && s <= 4*128 && s%4 == 0 && !config.noDuffDevice
    // result: (DUFFZERO [1*(128-s/4)] destptr (MOVLconst [0]) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!(s > 16 && s <= 4 * 128 && s % 4 == 0 && !config.noDuffDevice)) {
            break;
        }
        v.reset(Op386DUFFZERO);
        v.AuxInt = int64ToAuxInt(1 * (128 - s / 4));
        v0 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(destptr, v0, mem);
        return true;
    } 
    // match: (Zero [s] destptr mem)
    // cond: (s > 4*128 || (config.noDuffDevice && s > 16)) && s%4 == 0
    // result: (REPSTOSL destptr (MOVLconst [int32(s/4)]) (MOVLconst [0]) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        destptr = v_0;
        mem = v_1;
        if (!((s > 4 * 128 || (config.noDuffDevice && s > 16)) && s % 4 == 0)) {
            break;
        }
        v.reset(Op386REPSTOSL);
        v0 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(int32(s / 4));
        v1 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        v.AddArg4(destptr, v0, v1, mem);
        return true;
    }
    return false;
}
private static bool rewriteValue386_OpZeromask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Zeromask <t> x)
    // result: (XORLconst [-1] (SBBLcarrymask <t> (CMPLconst x [1])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(Op386XORLconst);
        v.AuxInt = int32ToAuxInt(-1);
        var v0 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
        var v1 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(1);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;
    }
}
private static bool rewriteBlock386(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;


    if (b.Kind == Block386EQ) 
        // match: (EQ (InvertFlags cmp) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            var v_0 = b.Controls[0];
            var cmp = v_0.Args[0];
            b.resetWithControl(Block386EQ, cmp);
            return true;
        } 
        // match: (EQ (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (EQ (FlagLT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (EQ (FlagLT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (EQ (FlagGT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (EQ (FlagGT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == Block386GE) 
        // match: (GE (InvertFlags cmp) yes no)
        // result: (LE cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386LE, cmp);
            return true;
        } 
        // match: (GE (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GE (FlagLT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GE (FlagLT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GE (FlagGT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GE (FlagGT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            return true;
        }
    else if (b.Kind == Block386GT) 
        // match: (GT (InvertFlags cmp) yes no)
        // result: (LT cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386LT, cmp);
            return true;
        } 
        // match: (GT (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GT (FlagLT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GT (FlagLT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GT (FlagGT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GT (FlagGT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            return true;
        }
    else if (b.Kind == BlockIf) 
        // match: (If (SETL cmp) yes no)
        // result: (LT cmp yes no)
        while (b.Controls[0].Op == Op386SETL) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386LT, cmp);
            return true;
        } 
        // match: (If (SETLE cmp) yes no)
        // result: (LE cmp yes no)
        while (b.Controls[0].Op == Op386SETLE) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386LE, cmp);
            return true;
        } 
        // match: (If (SETG cmp) yes no)
        // result: (GT cmp yes no)
        while (b.Controls[0].Op == Op386SETG) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386GT, cmp);
            return true;
        } 
        // match: (If (SETGE cmp) yes no)
        // result: (GE cmp yes no)
        while (b.Controls[0].Op == Op386SETGE) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386GE, cmp);
            return true;
        } 
        // match: (If (SETEQ cmp) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == Op386SETEQ) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386EQ, cmp);
            return true;
        } 
        // match: (If (SETNE cmp) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == Op386SETNE) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386NE, cmp);
            return true;
        } 
        // match: (If (SETB cmp) yes no)
        // result: (ULT cmp yes no)
        while (b.Controls[0].Op == Op386SETB) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386ULT, cmp);
            return true;
        } 
        // match: (If (SETBE cmp) yes no)
        // result: (ULE cmp yes no)
        while (b.Controls[0].Op == Op386SETBE) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386ULE, cmp);
            return true;
        } 
        // match: (If (SETA cmp) yes no)
        // result: (UGT cmp yes no)
        while (b.Controls[0].Op == Op386SETA) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386UGT, cmp);
            return true;
        } 
        // match: (If (SETAE cmp) yes no)
        // result: (UGE cmp yes no)
        while (b.Controls[0].Op == Op386SETAE) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386UGE, cmp);
            return true;
        } 
        // match: (If (SETO cmp) yes no)
        // result: (OS cmp yes no)
        while (b.Controls[0].Op == Op386SETO) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386OS, cmp);
            return true;
        } 
        // match: (If (SETGF cmp) yes no)
        // result: (UGT cmp yes no)
        while (b.Controls[0].Op == Op386SETGF) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386UGT, cmp);
            return true;
        } 
        // match: (If (SETGEF cmp) yes no)
        // result: (UGE cmp yes no)
        while (b.Controls[0].Op == Op386SETGEF) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386UGE, cmp);
            return true;
        } 
        // match: (If (SETEQF cmp) yes no)
        // result: (EQF cmp yes no)
        while (b.Controls[0].Op == Op386SETEQF) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386EQF, cmp);
            return true;
        } 
        // match: (If (SETNEF cmp) yes no)
        // result: (NEF cmp yes no)
        while (b.Controls[0].Op == Op386SETNEF) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386NEF, cmp);
            return true;
        } 
        // match: (If cond yes no)
        // result: (NE (TESTB cond cond) yes no)
        while (true) {
            var cond = b.Controls[0];
            var v0 = b.NewValue0(cond.Pos, Op386TESTB, types.TypeFlags);
            v0.AddArg2(cond, cond);
            b.resetWithControl(Block386NE, v0);
            return true;
        }
    else if (b.Kind == Block386LE) 
        // match: (LE (InvertFlags cmp) yes no)
        // result: (GE cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386GE, cmp);
            return true;
        } 
        // match: (LE (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LE (FlagLT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LE (FlagLT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LE (FlagGT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LE (FlagGT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == Block386LT) 
        // match: (LT (InvertFlags cmp) yes no)
        // result: (GT cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386GT, cmp);
            return true;
        } 
        // match: (LT (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LT (FlagLT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LT (FlagLT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LT (FlagGT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LT (FlagGT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == Block386NE) 
        // match: (NE (TESTB (SETL cmp) (SETL cmp)) yes no)
        // result: (LT cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            var v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETL) {
                break;
            }
            cmp = v_0_0.Args[0];
            var v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETL || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386LT, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETLE cmp) (SETLE cmp)) yes no)
        // result: (LE cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETLE) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETLE || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386LE, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETG cmp) (SETG cmp)) yes no)
        // result: (GT cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETG) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETG || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386GT, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETGE cmp) (SETGE cmp)) yes no)
        // result: (GE cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETGE) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETGE || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386GE, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETEQ cmp) (SETEQ cmp)) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETEQ) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETEQ || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386EQ, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETNE cmp) (SETNE cmp)) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETNE) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETNE || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386NE, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETB cmp) (SETB cmp)) yes no)
        // result: (ULT cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETB) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETB || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386ULT, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETBE cmp) (SETBE cmp)) yes no)
        // result: (ULE cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETBE) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETBE || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386ULE, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETA cmp) (SETA cmp)) yes no)
        // result: (UGT cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETA) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETA || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386UGT, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETAE cmp) (SETAE cmp)) yes no)
        // result: (UGE cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETAE) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETAE || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386UGE, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETO cmp) (SETO cmp)) yes no)
        // result: (OS cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETO) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETO || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386OS, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETGF cmp) (SETGF cmp)) yes no)
        // result: (UGT cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETGF) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETGF || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386UGT, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETGEF cmp) (SETGEF cmp)) yes no)
        // result: (UGE cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETGEF) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETGEF || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386UGE, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETEQF cmp) (SETEQF cmp)) yes no)
        // result: (EQF cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETEQF) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETEQF || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386EQF, cmp);
            return true;
        } 
        // match: (NE (TESTB (SETNEF cmp) (SETNEF cmp)) yes no)
        // result: (NEF cmp yes no)
        while (b.Controls[0].Op == Op386TESTB) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != Op386SETNEF) {
                break;
            }
            cmp = v_0_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != Op386SETNEF || cmp != v_0_1.Args[0]) {
                break;
            }
            b.resetWithControl(Block386NEF, cmp);
            return true;
        } 
        // match: (NE (InvertFlags cmp) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386NE, cmp);
            return true;
        } 
        // match: (NE (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (NE (FlagLT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (NE (FlagLT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (NE (FlagGT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (NE (FlagGT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            return true;
        }
    else if (b.Kind == Block386UGE) 
        // match: (UGE (InvertFlags cmp) yes no)
        // result: (ULE cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386ULE, cmp);
            return true;
        } 
        // match: (UGE (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (UGE (FlagLT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGE (FlagLT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (UGE (FlagGT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGE (FlagGT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            return true;
        }
    else if (b.Kind == Block386UGT) 
        // match: (UGT (InvertFlags cmp) yes no)
        // result: (ULT cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386ULT, cmp);
            return true;
        } 
        // match: (UGT (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGT (FlagLT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGT (FlagLT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (UGT (FlagGT_ULT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGT (FlagGT_UGT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            return true;
        }
    else if (b.Kind == Block386ULE) 
        // match: (ULE (InvertFlags cmp) yes no)
        // result: (UGE cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386UGE, cmp);
            return true;
        } 
        // match: (ULE (FlagEQ) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULE (FlagLT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULE (FlagLT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (ULE (FlagGT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULE (FlagGT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
    else if (b.Kind == Block386ULT) 
        // match: (ULT (InvertFlags cmp) yes no)
        // result: (UGT cmp yes no)
        while (b.Controls[0].Op == Op386InvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(Block386UGT, cmp);
            return true;
        } 
        // match: (ULT (FlagEQ) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagEQ) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (ULT (FlagLT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagLT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULT (FlagLT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagLT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (ULT (FlagGT_ULT) yes no)
        // result: (First yes no)
        while (b.Controls[0].Op == Op386FlagGT_ULT) {
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULT (FlagGT_UGT) yes no)
        // result: (First no yes)
        while (b.Controls[0].Op == Op386FlagGT_UGT) {
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        }
        return false;
}

} // end ssa_package
