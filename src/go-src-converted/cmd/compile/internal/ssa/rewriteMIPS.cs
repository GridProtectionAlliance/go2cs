// Code generated from gen/MIPS.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 August 29 09:12:54 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewriteMIPS.go
using math = go.math_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static var _ = math.MinInt8; // in case not otherwise used
        private static var _ = obj.ANOP; // in case not otherwise used
        private static var _ = objabi.GOROOT; // in case not otherwise used
        private static var _ = types.TypeMem; // in case not otherwise used

        private static bool rewriteValueMIPS(ref Value v)
        {

            if (v.Op == OpAdd16) 
                return rewriteValueMIPS_OpAdd16_0(v);
            else if (v.Op == OpAdd32) 
                return rewriteValueMIPS_OpAdd32_0(v);
            else if (v.Op == OpAdd32F) 
                return rewriteValueMIPS_OpAdd32F_0(v);
            else if (v.Op == OpAdd32withcarry) 
                return rewriteValueMIPS_OpAdd32withcarry_0(v);
            else if (v.Op == OpAdd64F) 
                return rewriteValueMIPS_OpAdd64F_0(v);
            else if (v.Op == OpAdd8) 
                return rewriteValueMIPS_OpAdd8_0(v);
            else if (v.Op == OpAddPtr) 
                return rewriteValueMIPS_OpAddPtr_0(v);
            else if (v.Op == OpAddr) 
                return rewriteValueMIPS_OpAddr_0(v);
            else if (v.Op == OpAnd16) 
                return rewriteValueMIPS_OpAnd16_0(v);
            else if (v.Op == OpAnd32) 
                return rewriteValueMIPS_OpAnd32_0(v);
            else if (v.Op == OpAnd8) 
                return rewriteValueMIPS_OpAnd8_0(v);
            else if (v.Op == OpAndB) 
                return rewriteValueMIPS_OpAndB_0(v);
            else if (v.Op == OpAtomicAdd32) 
                return rewriteValueMIPS_OpAtomicAdd32_0(v);
            else if (v.Op == OpAtomicAnd8) 
                return rewriteValueMIPS_OpAtomicAnd8_0(v);
            else if (v.Op == OpAtomicCompareAndSwap32) 
                return rewriteValueMIPS_OpAtomicCompareAndSwap32_0(v);
            else if (v.Op == OpAtomicExchange32) 
                return rewriteValueMIPS_OpAtomicExchange32_0(v);
            else if (v.Op == OpAtomicLoad32) 
                return rewriteValueMIPS_OpAtomicLoad32_0(v);
            else if (v.Op == OpAtomicLoadPtr) 
                return rewriteValueMIPS_OpAtomicLoadPtr_0(v);
            else if (v.Op == OpAtomicOr8) 
                return rewriteValueMIPS_OpAtomicOr8_0(v);
            else if (v.Op == OpAtomicStore32) 
                return rewriteValueMIPS_OpAtomicStore32_0(v);
            else if (v.Op == OpAtomicStorePtrNoWB) 
                return rewriteValueMIPS_OpAtomicStorePtrNoWB_0(v);
            else if (v.Op == OpAvg32u) 
                return rewriteValueMIPS_OpAvg32u_0(v);
            else if (v.Op == OpBitLen32) 
                return rewriteValueMIPS_OpBitLen32_0(v);
            else if (v.Op == OpClosureCall) 
                return rewriteValueMIPS_OpClosureCall_0(v);
            else if (v.Op == OpCom16) 
                return rewriteValueMIPS_OpCom16_0(v);
            else if (v.Op == OpCom32) 
                return rewriteValueMIPS_OpCom32_0(v);
            else if (v.Op == OpCom8) 
                return rewriteValueMIPS_OpCom8_0(v);
            else if (v.Op == OpConst16) 
                return rewriteValueMIPS_OpConst16_0(v);
            else if (v.Op == OpConst32) 
                return rewriteValueMIPS_OpConst32_0(v);
            else if (v.Op == OpConst32F) 
                return rewriteValueMIPS_OpConst32F_0(v);
            else if (v.Op == OpConst64F) 
                return rewriteValueMIPS_OpConst64F_0(v);
            else if (v.Op == OpConst8) 
                return rewriteValueMIPS_OpConst8_0(v);
            else if (v.Op == OpConstBool) 
                return rewriteValueMIPS_OpConstBool_0(v);
            else if (v.Op == OpConstNil) 
                return rewriteValueMIPS_OpConstNil_0(v);
            else if (v.Op == OpConvert) 
                return rewriteValueMIPS_OpConvert_0(v);
            else if (v.Op == OpCtz32) 
                return rewriteValueMIPS_OpCtz32_0(v);
            else if (v.Op == OpCvt32Fto32) 
                return rewriteValueMIPS_OpCvt32Fto32_0(v);
            else if (v.Op == OpCvt32Fto64F) 
                return rewriteValueMIPS_OpCvt32Fto64F_0(v);
            else if (v.Op == OpCvt32to32F) 
                return rewriteValueMIPS_OpCvt32to32F_0(v);
            else if (v.Op == OpCvt32to64F) 
                return rewriteValueMIPS_OpCvt32to64F_0(v);
            else if (v.Op == OpCvt64Fto32) 
                return rewriteValueMIPS_OpCvt64Fto32_0(v);
            else if (v.Op == OpCvt64Fto32F) 
                return rewriteValueMIPS_OpCvt64Fto32F_0(v);
            else if (v.Op == OpDiv16) 
                return rewriteValueMIPS_OpDiv16_0(v);
            else if (v.Op == OpDiv16u) 
                return rewriteValueMIPS_OpDiv16u_0(v);
            else if (v.Op == OpDiv32) 
                return rewriteValueMIPS_OpDiv32_0(v);
            else if (v.Op == OpDiv32F) 
                return rewriteValueMIPS_OpDiv32F_0(v);
            else if (v.Op == OpDiv32u) 
                return rewriteValueMIPS_OpDiv32u_0(v);
            else if (v.Op == OpDiv64F) 
                return rewriteValueMIPS_OpDiv64F_0(v);
            else if (v.Op == OpDiv8) 
                return rewriteValueMIPS_OpDiv8_0(v);
            else if (v.Op == OpDiv8u) 
                return rewriteValueMIPS_OpDiv8u_0(v);
            else if (v.Op == OpEq16) 
                return rewriteValueMIPS_OpEq16_0(v);
            else if (v.Op == OpEq32) 
                return rewriteValueMIPS_OpEq32_0(v);
            else if (v.Op == OpEq32F) 
                return rewriteValueMIPS_OpEq32F_0(v);
            else if (v.Op == OpEq64F) 
                return rewriteValueMIPS_OpEq64F_0(v);
            else if (v.Op == OpEq8) 
                return rewriteValueMIPS_OpEq8_0(v);
            else if (v.Op == OpEqB) 
                return rewriteValueMIPS_OpEqB_0(v);
            else if (v.Op == OpEqPtr) 
                return rewriteValueMIPS_OpEqPtr_0(v);
            else if (v.Op == OpGeq16) 
                return rewriteValueMIPS_OpGeq16_0(v);
            else if (v.Op == OpGeq16U) 
                return rewriteValueMIPS_OpGeq16U_0(v);
            else if (v.Op == OpGeq32) 
                return rewriteValueMIPS_OpGeq32_0(v);
            else if (v.Op == OpGeq32F) 
                return rewriteValueMIPS_OpGeq32F_0(v);
            else if (v.Op == OpGeq32U) 
                return rewriteValueMIPS_OpGeq32U_0(v);
            else if (v.Op == OpGeq64F) 
                return rewriteValueMIPS_OpGeq64F_0(v);
            else if (v.Op == OpGeq8) 
                return rewriteValueMIPS_OpGeq8_0(v);
            else if (v.Op == OpGeq8U) 
                return rewriteValueMIPS_OpGeq8U_0(v);
            else if (v.Op == OpGetCallerSP) 
                return rewriteValueMIPS_OpGetCallerSP_0(v);
            else if (v.Op == OpGetClosurePtr) 
                return rewriteValueMIPS_OpGetClosurePtr_0(v);
            else if (v.Op == OpGreater16) 
                return rewriteValueMIPS_OpGreater16_0(v);
            else if (v.Op == OpGreater16U) 
                return rewriteValueMIPS_OpGreater16U_0(v);
            else if (v.Op == OpGreater32) 
                return rewriteValueMIPS_OpGreater32_0(v);
            else if (v.Op == OpGreater32F) 
                return rewriteValueMIPS_OpGreater32F_0(v);
            else if (v.Op == OpGreater32U) 
                return rewriteValueMIPS_OpGreater32U_0(v);
            else if (v.Op == OpGreater64F) 
                return rewriteValueMIPS_OpGreater64F_0(v);
            else if (v.Op == OpGreater8) 
                return rewriteValueMIPS_OpGreater8_0(v);
            else if (v.Op == OpGreater8U) 
                return rewriteValueMIPS_OpGreater8U_0(v);
            else if (v.Op == OpHmul32) 
                return rewriteValueMIPS_OpHmul32_0(v);
            else if (v.Op == OpHmul32u) 
                return rewriteValueMIPS_OpHmul32u_0(v);
            else if (v.Op == OpInterCall) 
                return rewriteValueMIPS_OpInterCall_0(v);
            else if (v.Op == OpIsInBounds) 
                return rewriteValueMIPS_OpIsInBounds_0(v);
            else if (v.Op == OpIsNonNil) 
                return rewriteValueMIPS_OpIsNonNil_0(v);
            else if (v.Op == OpIsSliceInBounds) 
                return rewriteValueMIPS_OpIsSliceInBounds_0(v);
            else if (v.Op == OpLeq16) 
                return rewriteValueMIPS_OpLeq16_0(v);
            else if (v.Op == OpLeq16U) 
                return rewriteValueMIPS_OpLeq16U_0(v);
            else if (v.Op == OpLeq32) 
                return rewriteValueMIPS_OpLeq32_0(v);
            else if (v.Op == OpLeq32F) 
                return rewriteValueMIPS_OpLeq32F_0(v);
            else if (v.Op == OpLeq32U) 
                return rewriteValueMIPS_OpLeq32U_0(v);
            else if (v.Op == OpLeq64F) 
                return rewriteValueMIPS_OpLeq64F_0(v);
            else if (v.Op == OpLeq8) 
                return rewriteValueMIPS_OpLeq8_0(v);
            else if (v.Op == OpLeq8U) 
                return rewriteValueMIPS_OpLeq8U_0(v);
            else if (v.Op == OpLess16) 
                return rewriteValueMIPS_OpLess16_0(v);
            else if (v.Op == OpLess16U) 
                return rewriteValueMIPS_OpLess16U_0(v);
            else if (v.Op == OpLess32) 
                return rewriteValueMIPS_OpLess32_0(v);
            else if (v.Op == OpLess32F) 
                return rewriteValueMIPS_OpLess32F_0(v);
            else if (v.Op == OpLess32U) 
                return rewriteValueMIPS_OpLess32U_0(v);
            else if (v.Op == OpLess64F) 
                return rewriteValueMIPS_OpLess64F_0(v);
            else if (v.Op == OpLess8) 
                return rewriteValueMIPS_OpLess8_0(v);
            else if (v.Op == OpLess8U) 
                return rewriteValueMIPS_OpLess8U_0(v);
            else if (v.Op == OpLoad) 
                return rewriteValueMIPS_OpLoad_0(v);
            else if (v.Op == OpLsh16x16) 
                return rewriteValueMIPS_OpLsh16x16_0(v);
            else if (v.Op == OpLsh16x32) 
                return rewriteValueMIPS_OpLsh16x32_0(v);
            else if (v.Op == OpLsh16x64) 
                return rewriteValueMIPS_OpLsh16x64_0(v);
            else if (v.Op == OpLsh16x8) 
                return rewriteValueMIPS_OpLsh16x8_0(v);
            else if (v.Op == OpLsh32x16) 
                return rewriteValueMIPS_OpLsh32x16_0(v);
            else if (v.Op == OpLsh32x32) 
                return rewriteValueMIPS_OpLsh32x32_0(v);
            else if (v.Op == OpLsh32x64) 
                return rewriteValueMIPS_OpLsh32x64_0(v);
            else if (v.Op == OpLsh32x8) 
                return rewriteValueMIPS_OpLsh32x8_0(v);
            else if (v.Op == OpLsh8x16) 
                return rewriteValueMIPS_OpLsh8x16_0(v);
            else if (v.Op == OpLsh8x32) 
                return rewriteValueMIPS_OpLsh8x32_0(v);
            else if (v.Op == OpLsh8x64) 
                return rewriteValueMIPS_OpLsh8x64_0(v);
            else if (v.Op == OpLsh8x8) 
                return rewriteValueMIPS_OpLsh8x8_0(v);
            else if (v.Op == OpMIPSADD) 
                return rewriteValueMIPS_OpMIPSADD_0(v);
            else if (v.Op == OpMIPSADDconst) 
                return rewriteValueMIPS_OpMIPSADDconst_0(v);
            else if (v.Op == OpMIPSAND) 
                return rewriteValueMIPS_OpMIPSAND_0(v);
            else if (v.Op == OpMIPSANDconst) 
                return rewriteValueMIPS_OpMIPSANDconst_0(v);
            else if (v.Op == OpMIPSCMOVZ) 
                return rewriteValueMIPS_OpMIPSCMOVZ_0(v);
            else if (v.Op == OpMIPSCMOVZzero) 
                return rewriteValueMIPS_OpMIPSCMOVZzero_0(v);
            else if (v.Op == OpMIPSLoweredAtomicAdd) 
                return rewriteValueMIPS_OpMIPSLoweredAtomicAdd_0(v);
            else if (v.Op == OpMIPSLoweredAtomicStore) 
                return rewriteValueMIPS_OpMIPSLoweredAtomicStore_0(v);
            else if (v.Op == OpMIPSMOVBUload) 
                return rewriteValueMIPS_OpMIPSMOVBUload_0(v);
            else if (v.Op == OpMIPSMOVBUreg) 
                return rewriteValueMIPS_OpMIPSMOVBUreg_0(v);
            else if (v.Op == OpMIPSMOVBload) 
                return rewriteValueMIPS_OpMIPSMOVBload_0(v);
            else if (v.Op == OpMIPSMOVBreg) 
                return rewriteValueMIPS_OpMIPSMOVBreg_0(v);
            else if (v.Op == OpMIPSMOVBstore) 
                return rewriteValueMIPS_OpMIPSMOVBstore_0(v);
            else if (v.Op == OpMIPSMOVBstorezero) 
                return rewriteValueMIPS_OpMIPSMOVBstorezero_0(v);
            else if (v.Op == OpMIPSMOVDload) 
                return rewriteValueMIPS_OpMIPSMOVDload_0(v);
            else if (v.Op == OpMIPSMOVDstore) 
                return rewriteValueMIPS_OpMIPSMOVDstore_0(v);
            else if (v.Op == OpMIPSMOVFload) 
                return rewriteValueMIPS_OpMIPSMOVFload_0(v);
            else if (v.Op == OpMIPSMOVFstore) 
                return rewriteValueMIPS_OpMIPSMOVFstore_0(v);
            else if (v.Op == OpMIPSMOVHUload) 
                return rewriteValueMIPS_OpMIPSMOVHUload_0(v);
            else if (v.Op == OpMIPSMOVHUreg) 
                return rewriteValueMIPS_OpMIPSMOVHUreg_0(v);
            else if (v.Op == OpMIPSMOVHload) 
                return rewriteValueMIPS_OpMIPSMOVHload_0(v);
            else if (v.Op == OpMIPSMOVHreg) 
                return rewriteValueMIPS_OpMIPSMOVHreg_0(v);
            else if (v.Op == OpMIPSMOVHstore) 
                return rewriteValueMIPS_OpMIPSMOVHstore_0(v);
            else if (v.Op == OpMIPSMOVHstorezero) 
                return rewriteValueMIPS_OpMIPSMOVHstorezero_0(v);
            else if (v.Op == OpMIPSMOVWload) 
                return rewriteValueMIPS_OpMIPSMOVWload_0(v);
            else if (v.Op == OpMIPSMOVWreg) 
                return rewriteValueMIPS_OpMIPSMOVWreg_0(v);
            else if (v.Op == OpMIPSMOVWstore) 
                return rewriteValueMIPS_OpMIPSMOVWstore_0(v);
            else if (v.Op == OpMIPSMOVWstorezero) 
                return rewriteValueMIPS_OpMIPSMOVWstorezero_0(v);
            else if (v.Op == OpMIPSMUL) 
                return rewriteValueMIPS_OpMIPSMUL_0(v);
            else if (v.Op == OpMIPSNEG) 
                return rewriteValueMIPS_OpMIPSNEG_0(v);
            else if (v.Op == OpMIPSNOR) 
                return rewriteValueMIPS_OpMIPSNOR_0(v);
            else if (v.Op == OpMIPSNORconst) 
                return rewriteValueMIPS_OpMIPSNORconst_0(v);
            else if (v.Op == OpMIPSOR) 
                return rewriteValueMIPS_OpMIPSOR_0(v);
            else if (v.Op == OpMIPSORconst) 
                return rewriteValueMIPS_OpMIPSORconst_0(v);
            else if (v.Op == OpMIPSSGT) 
                return rewriteValueMIPS_OpMIPSSGT_0(v);
            else if (v.Op == OpMIPSSGTU) 
                return rewriteValueMIPS_OpMIPSSGTU_0(v);
            else if (v.Op == OpMIPSSGTUconst) 
                return rewriteValueMIPS_OpMIPSSGTUconst_0(v);
            else if (v.Op == OpMIPSSGTUzero) 
                return rewriteValueMIPS_OpMIPSSGTUzero_0(v);
            else if (v.Op == OpMIPSSGTconst) 
                return rewriteValueMIPS_OpMIPSSGTconst_0(v) || rewriteValueMIPS_OpMIPSSGTconst_10(v);
            else if (v.Op == OpMIPSSGTzero) 
                return rewriteValueMIPS_OpMIPSSGTzero_0(v);
            else if (v.Op == OpMIPSSLL) 
                return rewriteValueMIPS_OpMIPSSLL_0(v);
            else if (v.Op == OpMIPSSLLconst) 
                return rewriteValueMIPS_OpMIPSSLLconst_0(v);
            else if (v.Op == OpMIPSSRA) 
                return rewriteValueMIPS_OpMIPSSRA_0(v);
            else if (v.Op == OpMIPSSRAconst) 
                return rewriteValueMIPS_OpMIPSSRAconst_0(v);
            else if (v.Op == OpMIPSSRL) 
                return rewriteValueMIPS_OpMIPSSRL_0(v);
            else if (v.Op == OpMIPSSRLconst) 
                return rewriteValueMIPS_OpMIPSSRLconst_0(v);
            else if (v.Op == OpMIPSSUB) 
                return rewriteValueMIPS_OpMIPSSUB_0(v);
            else if (v.Op == OpMIPSSUBconst) 
                return rewriteValueMIPS_OpMIPSSUBconst_0(v);
            else if (v.Op == OpMIPSXOR) 
                return rewriteValueMIPS_OpMIPSXOR_0(v);
            else if (v.Op == OpMIPSXORconst) 
                return rewriteValueMIPS_OpMIPSXORconst_0(v);
            else if (v.Op == OpMod16) 
                return rewriteValueMIPS_OpMod16_0(v);
            else if (v.Op == OpMod16u) 
                return rewriteValueMIPS_OpMod16u_0(v);
            else if (v.Op == OpMod32) 
                return rewriteValueMIPS_OpMod32_0(v);
            else if (v.Op == OpMod32u) 
                return rewriteValueMIPS_OpMod32u_0(v);
            else if (v.Op == OpMod8) 
                return rewriteValueMIPS_OpMod8_0(v);
            else if (v.Op == OpMod8u) 
                return rewriteValueMIPS_OpMod8u_0(v);
            else if (v.Op == OpMove) 
                return rewriteValueMIPS_OpMove_0(v) || rewriteValueMIPS_OpMove_10(v);
            else if (v.Op == OpMul16) 
                return rewriteValueMIPS_OpMul16_0(v);
            else if (v.Op == OpMul32) 
                return rewriteValueMIPS_OpMul32_0(v);
            else if (v.Op == OpMul32F) 
                return rewriteValueMIPS_OpMul32F_0(v);
            else if (v.Op == OpMul32uhilo) 
                return rewriteValueMIPS_OpMul32uhilo_0(v);
            else if (v.Op == OpMul64F) 
                return rewriteValueMIPS_OpMul64F_0(v);
            else if (v.Op == OpMul8) 
                return rewriteValueMIPS_OpMul8_0(v);
            else if (v.Op == OpNeg16) 
                return rewriteValueMIPS_OpNeg16_0(v);
            else if (v.Op == OpNeg32) 
                return rewriteValueMIPS_OpNeg32_0(v);
            else if (v.Op == OpNeg32F) 
                return rewriteValueMIPS_OpNeg32F_0(v);
            else if (v.Op == OpNeg64F) 
                return rewriteValueMIPS_OpNeg64F_0(v);
            else if (v.Op == OpNeg8) 
                return rewriteValueMIPS_OpNeg8_0(v);
            else if (v.Op == OpNeq16) 
                return rewriteValueMIPS_OpNeq16_0(v);
            else if (v.Op == OpNeq32) 
                return rewriteValueMIPS_OpNeq32_0(v);
            else if (v.Op == OpNeq32F) 
                return rewriteValueMIPS_OpNeq32F_0(v);
            else if (v.Op == OpNeq64F) 
                return rewriteValueMIPS_OpNeq64F_0(v);
            else if (v.Op == OpNeq8) 
                return rewriteValueMIPS_OpNeq8_0(v);
            else if (v.Op == OpNeqB) 
                return rewriteValueMIPS_OpNeqB_0(v);
            else if (v.Op == OpNeqPtr) 
                return rewriteValueMIPS_OpNeqPtr_0(v);
            else if (v.Op == OpNilCheck) 
                return rewriteValueMIPS_OpNilCheck_0(v);
            else if (v.Op == OpNot) 
                return rewriteValueMIPS_OpNot_0(v);
            else if (v.Op == OpOffPtr) 
                return rewriteValueMIPS_OpOffPtr_0(v);
            else if (v.Op == OpOr16) 
                return rewriteValueMIPS_OpOr16_0(v);
            else if (v.Op == OpOr32) 
                return rewriteValueMIPS_OpOr32_0(v);
            else if (v.Op == OpOr8) 
                return rewriteValueMIPS_OpOr8_0(v);
            else if (v.Op == OpOrB) 
                return rewriteValueMIPS_OpOrB_0(v);
            else if (v.Op == OpRound32F) 
                return rewriteValueMIPS_OpRound32F_0(v);
            else if (v.Op == OpRound64F) 
                return rewriteValueMIPS_OpRound64F_0(v);
            else if (v.Op == OpRsh16Ux16) 
                return rewriteValueMIPS_OpRsh16Ux16_0(v);
            else if (v.Op == OpRsh16Ux32) 
                return rewriteValueMIPS_OpRsh16Ux32_0(v);
            else if (v.Op == OpRsh16Ux64) 
                return rewriteValueMIPS_OpRsh16Ux64_0(v);
            else if (v.Op == OpRsh16Ux8) 
                return rewriteValueMIPS_OpRsh16Ux8_0(v);
            else if (v.Op == OpRsh16x16) 
                return rewriteValueMIPS_OpRsh16x16_0(v);
            else if (v.Op == OpRsh16x32) 
                return rewriteValueMIPS_OpRsh16x32_0(v);
            else if (v.Op == OpRsh16x64) 
                return rewriteValueMIPS_OpRsh16x64_0(v);
            else if (v.Op == OpRsh16x8) 
                return rewriteValueMIPS_OpRsh16x8_0(v);
            else if (v.Op == OpRsh32Ux16) 
                return rewriteValueMIPS_OpRsh32Ux16_0(v);
            else if (v.Op == OpRsh32Ux32) 
                return rewriteValueMIPS_OpRsh32Ux32_0(v);
            else if (v.Op == OpRsh32Ux64) 
                return rewriteValueMIPS_OpRsh32Ux64_0(v);
            else if (v.Op == OpRsh32Ux8) 
                return rewriteValueMIPS_OpRsh32Ux8_0(v);
            else if (v.Op == OpRsh32x16) 
                return rewriteValueMIPS_OpRsh32x16_0(v);
            else if (v.Op == OpRsh32x32) 
                return rewriteValueMIPS_OpRsh32x32_0(v);
            else if (v.Op == OpRsh32x64) 
                return rewriteValueMIPS_OpRsh32x64_0(v);
            else if (v.Op == OpRsh32x8) 
                return rewriteValueMIPS_OpRsh32x8_0(v);
            else if (v.Op == OpRsh8Ux16) 
                return rewriteValueMIPS_OpRsh8Ux16_0(v);
            else if (v.Op == OpRsh8Ux32) 
                return rewriteValueMIPS_OpRsh8Ux32_0(v);
            else if (v.Op == OpRsh8Ux64) 
                return rewriteValueMIPS_OpRsh8Ux64_0(v);
            else if (v.Op == OpRsh8Ux8) 
                return rewriteValueMIPS_OpRsh8Ux8_0(v);
            else if (v.Op == OpRsh8x16) 
                return rewriteValueMIPS_OpRsh8x16_0(v);
            else if (v.Op == OpRsh8x32) 
                return rewriteValueMIPS_OpRsh8x32_0(v);
            else if (v.Op == OpRsh8x64) 
                return rewriteValueMIPS_OpRsh8x64_0(v);
            else if (v.Op == OpRsh8x8) 
                return rewriteValueMIPS_OpRsh8x8_0(v);
            else if (v.Op == OpSelect0) 
                return rewriteValueMIPS_OpSelect0_0(v) || rewriteValueMIPS_OpSelect0_10(v);
            else if (v.Op == OpSelect1) 
                return rewriteValueMIPS_OpSelect1_0(v) || rewriteValueMIPS_OpSelect1_10(v);
            else if (v.Op == OpSignExt16to32) 
                return rewriteValueMIPS_OpSignExt16to32_0(v);
            else if (v.Op == OpSignExt8to16) 
                return rewriteValueMIPS_OpSignExt8to16_0(v);
            else if (v.Op == OpSignExt8to32) 
                return rewriteValueMIPS_OpSignExt8to32_0(v);
            else if (v.Op == OpSignmask) 
                return rewriteValueMIPS_OpSignmask_0(v);
            else if (v.Op == OpSlicemask) 
                return rewriteValueMIPS_OpSlicemask_0(v);
            else if (v.Op == OpSqrt) 
                return rewriteValueMIPS_OpSqrt_0(v);
            else if (v.Op == OpStaticCall) 
                return rewriteValueMIPS_OpStaticCall_0(v);
            else if (v.Op == OpStore) 
                return rewriteValueMIPS_OpStore_0(v);
            else if (v.Op == OpSub16) 
                return rewriteValueMIPS_OpSub16_0(v);
            else if (v.Op == OpSub32) 
                return rewriteValueMIPS_OpSub32_0(v);
            else if (v.Op == OpSub32F) 
                return rewriteValueMIPS_OpSub32F_0(v);
            else if (v.Op == OpSub32withcarry) 
                return rewriteValueMIPS_OpSub32withcarry_0(v);
            else if (v.Op == OpSub64F) 
                return rewriteValueMIPS_OpSub64F_0(v);
            else if (v.Op == OpSub8) 
                return rewriteValueMIPS_OpSub8_0(v);
            else if (v.Op == OpSubPtr) 
                return rewriteValueMIPS_OpSubPtr_0(v);
            else if (v.Op == OpTrunc16to8) 
                return rewriteValueMIPS_OpTrunc16to8_0(v);
            else if (v.Op == OpTrunc32to16) 
                return rewriteValueMIPS_OpTrunc32to16_0(v);
            else if (v.Op == OpTrunc32to8) 
                return rewriteValueMIPS_OpTrunc32to8_0(v);
            else if (v.Op == OpXor16) 
                return rewriteValueMIPS_OpXor16_0(v);
            else if (v.Op == OpXor32) 
                return rewriteValueMIPS_OpXor32_0(v);
            else if (v.Op == OpXor8) 
                return rewriteValueMIPS_OpXor8_0(v);
            else if (v.Op == OpZero) 
                return rewriteValueMIPS_OpZero_0(v) || rewriteValueMIPS_OpZero_10(v);
            else if (v.Op == OpZeroExt16to32) 
                return rewriteValueMIPS_OpZeroExt16to32_0(v);
            else if (v.Op == OpZeroExt8to16) 
                return rewriteValueMIPS_OpZeroExt8to16_0(v);
            else if (v.Op == OpZeroExt8to32) 
                return rewriteValueMIPS_OpZeroExt8to32_0(v);
            else if (v.Op == OpZeromask) 
                return rewriteValueMIPS_OpZeromask_0(v);
                        return false;
        }
        private static bool rewriteValueMIPS_OpAdd16_0(ref Value v)
        { 
            // match: (Add16 x y)
            // cond:
            // result: (ADD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAdd32_0(ref Value v)
        { 
            // match: (Add32 x y)
            // cond:
            // result: (ADD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAdd32F_0(ref Value v)
        { 
            // match: (Add32F x y)
            // cond:
            // result: (ADDF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADDF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAdd32withcarry_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Add32withcarry <t> x y c)
            // cond:
            // result: (ADD c (ADD <t> x y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[2L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                var c = v.Args[2L];
                v.reset(OpMIPSADD);
                v.AddArg(c);
                var v0 = b.NewValue0(v.Pos, OpMIPSADD, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAdd64F_0(ref Value v)
        { 
            // match: (Add64F x y)
            // cond:
            // result: (ADDD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADDD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAdd8_0(ref Value v)
        { 
            // match: (Add8 x y)
            // cond:
            // result: (ADD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAddPtr_0(ref Value v)
        { 
            // match: (AddPtr x y)
            // cond:
            // result: (ADD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAddr_0(ref Value v)
        { 
            // match: (Addr {sym} base)
            // cond:
            // result: (MOVWaddr {sym} base)
            while (true)
            {
                var sym = v.Aux;
                var @base = v.Args[0L];
                v.reset(OpMIPSMOVWaddr);
                v.Aux = sym;
                v.AddArg(base);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAnd16_0(ref Value v)
        { 
            // match: (And16 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSAND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAnd32_0(ref Value v)
        { 
            // match: (And32 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSAND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAnd8_0(ref Value v)
        { 
            // match: (And8 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSAND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAndB_0(ref Value v)
        { 
            // match: (AndB x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSAND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicAdd32_0(ref Value v)
        { 
            // match: (AtomicAdd32 ptr val mem)
            // cond:
            // result: (LoweredAtomicAdd ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPSLoweredAtomicAdd);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicAnd8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (AtomicAnd8 ptr val mem)
            // cond: !config.BigEndian
            // result: (LoweredAtomicAnd (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr)         (OR <typ.UInt32> (SLL <typ.UInt32> (ZeroExt8to32 val)             (SLLconst <typ.UInt32> [3]                 (ANDconst  <typ.UInt32> [3] ptr)))         (NORconst [0] <typ.UInt32> (SLL <typ.UInt32>             (MOVWconst [0xff]) (SLLconst <typ.UInt32> [3]                 (ANDconst <typ.UInt32> [3] ptr))))) mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(!config.BigEndian))
                {
                    break;
                }
                v.reset(OpMIPSLoweredAtomicAnd);
                var v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = ~3L;
                v0.AddArg(v1);
                v0.AddArg(ptr);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSOR, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(val);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v5.AuxInt = 3L;
                var v6 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
                v6.AuxInt = 3L;
                v6.AddArg(ptr);
                v5.AddArg(v6);
                v3.AddArg(v5);
                v2.AddArg(v3);
                var v7 = b.NewValue0(v.Pos, OpMIPSNORconst, typ.UInt32);
                v7.AuxInt = 0L;
                var v8 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
                var v9 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v9.AuxInt = 0xffUL;
                v8.AddArg(v9);
                var v10 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v10.AuxInt = 3L;
                var v11 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
                v11.AuxInt = 3L;
                v11.AddArg(ptr);
                v10.AddArg(v11);
                v8.AddArg(v10);
                v7.AddArg(v8);
                v2.AddArg(v7);
                v.AddArg(v2);
                v.AddArg(mem);
                return true;
            } 
            // match: (AtomicAnd8 ptr val mem)
            // cond: config.BigEndian
            // result: (LoweredAtomicAnd (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr)         (OR <typ.UInt32> (SLL <typ.UInt32> (ZeroExt8to32 val)             (SLLconst <typ.UInt32> [3]                 (ANDconst  <typ.UInt32> [3]                     (XORconst <typ.UInt32> [3] ptr))))         (NORconst [0] <typ.UInt32> (SLL <typ.UInt32>             (MOVWconst [0xff]) (SLLconst <typ.UInt32> [3]                 (ANDconst <typ.UInt32> [3]                     (XORconst <typ.UInt32> [3] ptr)))))) mem)
 
            // match: (AtomicAnd8 ptr val mem)
            // cond: config.BigEndian
            // result: (LoweredAtomicAnd (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr)         (OR <typ.UInt32> (SLL <typ.UInt32> (ZeroExt8to32 val)             (SLLconst <typ.UInt32> [3]                 (ANDconst  <typ.UInt32> [3]                     (XORconst <typ.UInt32> [3] ptr))))         (NORconst [0] <typ.UInt32> (SLL <typ.UInt32>             (MOVWconst [0xff]) (SLLconst <typ.UInt32> [3]                 (ANDconst <typ.UInt32> [3]                     (XORconst <typ.UInt32> [3] ptr)))))) mem)
            while (true)
            {
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(config.BigEndian))
                {
                    break;
                }
                v.reset(OpMIPSLoweredAtomicAnd);
                v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = ~3L;
                v0.AddArg(v1);
                v0.AddArg(ptr);
                v.AddArg(v0);
                v2 = b.NewValue0(v.Pos, OpMIPSOR, typ.UInt32);
                v3 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
                v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(val);
                v3.AddArg(v4);
                v5 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v5.AuxInt = 3L;
                v6 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
                v6.AuxInt = 3L;
                v7 = b.NewValue0(v.Pos, OpMIPSXORconst, typ.UInt32);
                v7.AuxInt = 3L;
                v7.AddArg(ptr);
                v6.AddArg(v7);
                v5.AddArg(v6);
                v3.AddArg(v5);
                v2.AddArg(v3);
                v8 = b.NewValue0(v.Pos, OpMIPSNORconst, typ.UInt32);
                v8.AuxInt = 0L;
                v9 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
                v10 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v10.AuxInt = 0xffUL;
                v9.AddArg(v10);
                v11 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v11.AuxInt = 3L;
                var v12 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
                v12.AuxInt = 3L;
                var v13 = b.NewValue0(v.Pos, OpMIPSXORconst, typ.UInt32);
                v13.AuxInt = 3L;
                v13.AddArg(ptr);
                v12.AddArg(v13);
                v11.AddArg(v12);
                v9.AddArg(v11);
                v8.AddArg(v9);
                v2.AddArg(v8);
                v.AddArg(v2);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpAtomicCompareAndSwap32_0(ref Value v)
        { 
            // match: (AtomicCompareAndSwap32 ptr old new_ mem)
            // cond:
            // result: (LoweredAtomicCas ptr old new_ mem)
            while (true)
            {
                _ = v.Args[3L];
                var ptr = v.Args[0L];
                var old = v.Args[1L];
                var new_ = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(OpMIPSLoweredAtomicCas);
                v.AddArg(ptr);
                v.AddArg(old);
                v.AddArg(new_);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicExchange32_0(ref Value v)
        { 
            // match: (AtomicExchange32 ptr val mem)
            // cond:
            // result: (LoweredAtomicExchange ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPSLoweredAtomicExchange);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicLoad32_0(ref Value v)
        { 
            // match: (AtomicLoad32 ptr mem)
            // cond:
            // result: (LoweredAtomicLoad ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPSLoweredAtomicLoad);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicLoadPtr_0(ref Value v)
        { 
            // match: (AtomicLoadPtr ptr mem)
            // cond:
            // result: (LoweredAtomicLoad  ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPSLoweredAtomicLoad);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicOr8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (AtomicOr8 ptr val mem)
            // cond: !config.BigEndian
            // result: (LoweredAtomicOr (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr)         (SLL <typ.UInt32> (ZeroExt8to32 val)             (SLLconst <typ.UInt32> [3]                 (ANDconst <typ.UInt32> [3] ptr))) mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(!config.BigEndian))
                {
                    break;
                }
                v.reset(OpMIPSLoweredAtomicOr);
                var v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = ~3L;
                v0.AddArg(v1);
                v0.AddArg(ptr);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v3.AddArg(val);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v4.AuxInt = 3L;
                var v5 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
                v5.AuxInt = 3L;
                v5.AddArg(ptr);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v.AddArg(v2);
                v.AddArg(mem);
                return true;
            } 
            // match: (AtomicOr8 ptr val mem)
            // cond: config.BigEndian
            // result: (LoweredAtomicOr (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr)         (SLL <typ.UInt32> (ZeroExt8to32 val)             (SLLconst <typ.UInt32> [3]                 (ANDconst <typ.UInt32> [3]                     (XORconst <typ.UInt32> [3] ptr)))) mem)
 
            // match: (AtomicOr8 ptr val mem)
            // cond: config.BigEndian
            // result: (LoweredAtomicOr (AND <typ.UInt32Ptr> (MOVWconst [^3]) ptr)         (SLL <typ.UInt32> (ZeroExt8to32 val)             (SLLconst <typ.UInt32> [3]                 (ANDconst <typ.UInt32> [3]                     (XORconst <typ.UInt32> [3] ptr)))) mem)
            while (true)
            {
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(config.BigEndian))
                {
                    break;
                }
                v.reset(OpMIPSLoweredAtomicOr);
                v0 = b.NewValue0(v.Pos, OpMIPSAND, typ.UInt32Ptr);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = ~3L;
                v0.AddArg(v1);
                v0.AddArg(ptr);
                v.AddArg(v0);
                v2 = b.NewValue0(v.Pos, OpMIPSSLL, typ.UInt32);
                v3 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v3.AddArg(val);
                v2.AddArg(v3);
                v4 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v4.AuxInt = 3L;
                v5 = b.NewValue0(v.Pos, OpMIPSANDconst, typ.UInt32);
                v5.AuxInt = 3L;
                var v6 = b.NewValue0(v.Pos, OpMIPSXORconst, typ.UInt32);
                v6.AuxInt = 3L;
                v6.AddArg(ptr);
                v5.AddArg(v6);
                v4.AddArg(v5);
                v2.AddArg(v4);
                v.AddArg(v2);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpAtomicStore32_0(ref Value v)
        { 
            // match: (AtomicStore32 ptr val mem)
            // cond:
            // result: (LoweredAtomicStore ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPSLoweredAtomicStore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAtomicStorePtrNoWB_0(ref Value v)
        { 
            // match: (AtomicStorePtrNoWB ptr val mem)
            // cond:
            // result: (LoweredAtomicStore  ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPSLoweredAtomicStore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpAvg32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Avg32u <t> x y)
            // cond:
            // result: (ADD (SRLconst <t> (SUB <t> x y) [1]) y)
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSADD);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRLconst, t);
                v0.AuxInt = 1L;
                var v1 = b.NewValue0(v.Pos, OpMIPSSUB, t);
                v1.AddArg(x);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpBitLen32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (BitLen32 <t> x)
            // cond:
            // result: (SUB (MOVWconst [32]) (CLZ <t> x))
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(OpMIPSSUB);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 32L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCLZ, t);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpClosureCall_0(ref Value v)
        { 
            // match: (ClosureCall [argwid] entry closure mem)
            // cond:
            // result: (CALLclosure [argwid] entry closure mem)
            while (true)
            {
                var argwid = v.AuxInt;
                _ = v.Args[2L];
                var entry = v.Args[0L];
                var closure = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPSCALLclosure);
                v.AuxInt = argwid;
                v.AddArg(entry);
                v.AddArg(closure);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCom16_0(ref Value v)
        { 
            // match: (Com16 x)
            // cond:
            // result: (NORconst [0] x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNORconst);
                v.AuxInt = 0L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCom32_0(ref Value v)
        { 
            // match: (Com32 x)
            // cond:
            // result: (NORconst [0] x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNORconst);
                v.AuxInt = 0L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCom8_0(ref Value v)
        { 
            // match: (Com8 x)
            // cond:
            // result: (NORconst [0] x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNORconst);
                v.AuxInt = 0L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConst16_0(ref Value v)
        { 
            // match: (Const16 [val])
            // cond:
            // result: (MOVWconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConst32_0(ref Value v)
        { 
            // match: (Const32 [val])
            // cond:
            // result: (MOVWconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConst32F_0(ref Value v)
        { 
            // match: (Const32F [val])
            // cond:
            // result: (MOVFconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPSMOVFconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConst64F_0(ref Value v)
        { 
            // match: (Const64F [val])
            // cond:
            // result: (MOVDconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPSMOVDconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConst8_0(ref Value v)
        { 
            // match: (Const8 [val])
            // cond:
            // result: (MOVWconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConstBool_0(ref Value v)
        { 
            // match: (ConstBool [b])
            // cond:
            // result: (MOVWconst [b])
            while (true)
            {
                var b = v.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = b;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConstNil_0(ref Value v)
        { 
            // match: (ConstNil)
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpConvert_0(ref Value v)
        { 
            // match: (Convert x mem)
            // cond:
            // result: (MOVWconvert x mem)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPSMOVWconvert);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCtz32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Ctz32 <t> x)
            // cond:
            // result: (SUB (MOVWconst [32]) (CLZ <t> (SUBconst <t> [1] (AND <t> x (NEG <t> x)))))
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(OpMIPSSUB);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 32L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCLZ, t);
                var v2 = b.NewValue0(v.Pos, OpMIPSSUBconst, t);
                v2.AuxInt = 1L;
                var v3 = b.NewValue0(v.Pos, OpMIPSAND, t);
                v3.AddArg(x);
                var v4 = b.NewValue0(v.Pos, OpMIPSNEG, t);
                v4.AddArg(x);
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCvt32Fto32_0(ref Value v)
        { 
            // match: (Cvt32Fto32 x)
            // cond:
            // result: (TRUNCFW x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSTRUNCFW);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCvt32Fto64F_0(ref Value v)
        { 
            // match: (Cvt32Fto64F x)
            // cond:
            // result: (MOVFD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVFD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCvt32to32F_0(ref Value v)
        { 
            // match: (Cvt32to32F x)
            // cond:
            // result: (MOVWF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVWF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCvt32to64F_0(ref Value v)
        { 
            // match: (Cvt32to64F x)
            // cond:
            // result: (MOVWD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVWD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCvt64Fto32_0(ref Value v)
        { 
            // match: (Cvt64Fto32 x)
            // cond:
            // result: (TRUNCDW x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSTRUNCDW);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpCvt64Fto32F_0(ref Value v)
        { 
            // match: (Cvt64Fto32F x)
            // cond:
            // result: (MOVDF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVDF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div16 x y)
            // cond:
            // result: (Select1 (DIV (SignExt16to32 x) (SignExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv16u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div16u x y)
            // cond:
            // result: (Select1 (DIVU (ZeroExt16to32 x) (ZeroExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div32 x y)
            // cond:
            // result: (Select1 (DIV x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv32F_0(ref Value v)
        { 
            // match: (Div32F x y)
            // cond:
            // result: (DIVF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSDIVF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div32u x y)
            // cond:
            // result: (Select1 (DIVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv64F_0(ref Value v)
        { 
            // match: (Div64F x y)
            // cond:
            // result: (DIVD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSDIVD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div8 x y)
            // cond:
            // result: (Select1 (DIV (SignExt8to32 x) (SignExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpDiv8u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div8u x y)
            // cond:
            // result: (Select1 (DIVU (ZeroExt8to32 x) (ZeroExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq16 x y)
            // cond:
            // result: (SGTUconst [1] (XOR (ZeroExt16to32 x) (ZeroExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq32 x y)
            // cond:
            // result: (SGTUconst [1] (XOR x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq32F x y)
            // cond:
            // result: (FPFlagTrue (CMPEQF x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq64F x y)
            // cond:
            // result: (FPFlagTrue (CMPEQD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq8 x y)
            // cond:
            // result: (SGTUconst [1] (XOR (ZeroExt8to32 x) (ZeroExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEqB_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (EqB x y)
            // cond:
            // result: (XORconst [1] (XOR <typ.Bool> x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.Bool);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpEqPtr_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (EqPtr x y)
            // cond:
            // result: (SGTUconst [1] (XOR x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq16 x y)
            // cond:
            // result: (XORconst [1] (SGT (SignExt16to32 y) (SignExt16to32 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v2.AddArg(x);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq16U x y)
            // cond:
            // result: (XORconst [1] (SGTU (ZeroExt16to32 y) (ZeroExt16to32 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(x);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq32 x y)
            // cond:
            // result: (XORconst [1] (SGT y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq32F x y)
            // cond:
            // result: (FPFlagTrue (CMPGEF x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGEF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq32U x y)
            // cond:
            // result: (XORconst [1] (SGTU y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq64F x y)
            // cond:
            // result: (FPFlagTrue (CMPGED x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGED, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq8 x y)
            // cond:
            // result: (XORconst [1] (SGT (SignExt8to32 y) (SignExt8to32 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v2.AddArg(x);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGeq8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq8U x y)
            // cond:
            // result: (XORconst [1] (SGTU (ZeroExt8to32 y) (ZeroExt8to32 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(x);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGetCallerSP_0(ref Value v)
        { 
            // match: (GetCallerSP)
            // cond:
            // result: (LoweredGetCallerSP)
            while (true)
            {
                v.reset(OpMIPSLoweredGetCallerSP);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGetClosurePtr_0(ref Value v)
        { 
            // match: (GetClosurePtr)
            // cond:
            // result: (LoweredGetClosurePtr)
            while (true)
            {
                v.reset(OpMIPSLoweredGetClosurePtr);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater16 x y)
            // cond:
            // result: (SGT (SignExt16to32 x) (SignExt16to32 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater16U x y)
            // cond:
            // result: (SGTU (ZeroExt16to32 x) (ZeroExt16to32 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater32_0(ref Value v)
        { 
            // match: (Greater32 x y)
            // cond:
            // result: (SGT x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGT);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater32F x y)
            // cond:
            // result: (FPFlagTrue (CMPGTF x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGTF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater32U_0(ref Value v)
        { 
            // match: (Greater32U x y)
            // cond:
            // result: (SGTU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater64F x y)
            // cond:
            // result: (FPFlagTrue (CMPGTD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGTD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater8 x y)
            // cond:
            // result: (SGT (SignExt8to32 x) (SignExt8to32 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpGreater8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater8U x y)
            // cond:
            // result: (SGTU (ZeroExt8to32 x) (ZeroExt8to32 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpHmul32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Hmul32 x y)
            // cond:
            // result: (Select0 (MULT x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSMULT, types.NewTuple(typ.Int32, typ.Int32));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpHmul32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Hmul32u x y)
            // cond:
            // result: (Select0 (MULTU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSMULTU, types.NewTuple(typ.UInt32, typ.UInt32));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpInterCall_0(ref Value v)
        { 
            // match: (InterCall [argwid] entry mem)
            // cond:
            // result: (CALLinter [argwid] entry mem)
            while (true)
            {
                var argwid = v.AuxInt;
                _ = v.Args[1L];
                var entry = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPSCALLinter);
                v.AuxInt = argwid;
                v.AddArg(entry);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpIsInBounds_0(ref Value v)
        { 
            // match: (IsInBounds idx len)
            // cond:
            // result: (SGTU len idx)
            while (true)
            {
                _ = v.Args[1L];
                var idx = v.Args[0L];
                var len = v.Args[1L];
                v.reset(OpMIPSSGTU);
                v.AddArg(len);
                v.AddArg(idx);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpIsNonNil_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (IsNonNil ptr)
            // cond:
            // result: (SGTU ptr (MOVWconst [0]))
            while (true)
            {
                var ptr = v.Args[0L];
                v.reset(OpMIPSSGTU);
                v.AddArg(ptr);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpIsSliceInBounds_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (IsSliceInBounds idx len)
            // cond:
            // result: (XORconst [1] (SGTU idx len))
            while (true)
            {
                _ = v.Args[1L];
                var idx = v.Args[0L];
                var len = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                v0.AddArg(idx);
                v0.AddArg(len);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq16 x y)
            // cond:
            // result: (XORconst [1] (SGT (SignExt16to32 x) (SignExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq16U x y)
            // cond:
            // result: (XORconst [1] (SGTU (ZeroExt16to32 x) (ZeroExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq32 x y)
            // cond:
            // result: (XORconst [1] (SGT x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq32F x y)
            // cond:
            // result: (FPFlagTrue (CMPGEF y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGEF, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq32U x y)
            // cond:
            // result: (XORconst [1] (SGTU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq64F x y)
            // cond:
            // result: (FPFlagTrue (CMPGED y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGED, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq8 x y)
            // cond:
            // result: (XORconst [1] (SGT (SignExt8to32 x) (SignExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGT, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLeq8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq8U x y)
            // cond:
            // result: (XORconst [1] (SGTU (ZeroExt8to32 x) (ZeroExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less16 x y)
            // cond:
            // result: (SGT (SignExt16to32 y) (SignExt16to32 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less16U x y)
            // cond:
            // result: (SGTU (ZeroExt16to32 y) (ZeroExt16to32 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess32_0(ref Value v)
        { 
            // match: (Less32 x y)
            // cond:
            // result: (SGT y x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGT);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less32F x y)
            // cond:
            // result: (FPFlagTrue (CMPGTF y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGTF, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess32U_0(ref Value v)
        { 
            // match: (Less32U x y)
            // cond:
            // result: (SGTU y x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less64F x y)
            // cond:
            // result: (FPFlagTrue (CMPGTD y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPGTD, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less8 x y)
            // cond:
            // result: (SGT (SignExt8to32 y) (SignExt8to32 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLess8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less8U x y)
            // cond:
            // result: (SGTU (ZeroExt8to32 y) (ZeroExt8to32 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLoad_0(ref Value v)
        { 
            // match: (Load <t> ptr mem)
            // cond: t.IsBoolean()
            // result: (MOVBUload ptr mem)
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                if (!(t.IsBoolean()))
                {
                    break;
                }
                v.reset(OpMIPSMOVBUload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is8BitInt(t) && isSigned(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is8BitInt(t) && !isSigned(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBUload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is16BitInt(t) && isSigned(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is16BitInt(t) && !isSigned(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHUload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) || isPtr(t))
            // result: (MOVWload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) || isPtr(t))
            // result: (MOVWload ptr mem)
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is32BitInt(t) || isPtr(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is32BitFloat(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVFload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is64BitFloat(t)))
                {
                    break;
                }
                v.reset(OpMIPSMOVDload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpLsh16x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x16 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh16x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x32 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh16x64_0(ref Value v)
        { 
            // match: (Lsh16x64 x (Const64 [c]))
            // cond: uint32(c) < 16
            // result: (SLLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 16L))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Lsh16x64 _ (Const64 [c]))
            // cond: uint32(c) >= 16
            // result: (MOVWconst [0])
 
            // match: (Lsh16x64 _ (Const64 [c]))
            // cond: uint32(c) >= 16
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 16L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpLsh16x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x8 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh32x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x16 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh32x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x32 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh32x64_0(ref Value v)
        { 
            // match: (Lsh32x64 x (Const64 [c]))
            // cond: uint32(c) < 32
            // result: (SLLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 32L))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Lsh32x64 _ (Const64 [c]))
            // cond: uint32(c) >= 32
            // result: (MOVWconst [0])
 
            // match: (Lsh32x64 _ (Const64 [c]))
            // cond: uint32(c) >= 32
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 32L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpLsh32x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x8 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh8x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x16 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh8x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x32 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpLsh8x64_0(ref Value v)
        { 
            // match: (Lsh8x64 x (Const64 [c]))
            // cond: uint32(c) < 8
            // result: (SLLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 8L))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Lsh8x64 _ (Const64 [c]))
            // cond: uint32(c) >= 8
            // result: (MOVWconst [0])
 
            // match: (Lsh8x64 _ (Const64 [c]))
            // cond: uint32(c) >= 8
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 8L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpLsh8x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x8 <t> x y)
            // cond:
            // result: (CMOVZ (SLL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSLL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMIPSADD_0(ref Value v)
        { 
            // match: (ADD x (MOVWconst [c]))
            // cond:
            // result: (ADDconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(OpMIPSADDconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADD (MOVWconst [c]) x)
            // cond:
            // result: (ADDconst [c] x)
 
            // match: (ADD (MOVWconst [c]) x)
            // cond:
            // result: (ADDconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(OpMIPSADDconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADD x (NEG y))
            // cond:
            // result: (SUB x y)
 
            // match: (ADD x (NEG y))
            // cond:
            // result: (SUB x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSNEG)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(OpMIPSSUB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADD (NEG y) x)
            // cond:
            // result: (SUB x y)
 
            // match: (ADD (NEG y) x)
            // cond:
            // result: (SUB x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSNEG)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(OpMIPSSUB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSADDconst_0(ref Value v)
        { 
            // match: (ADDconst [off1] (MOVWaddr [off2] {sym} ptr))
            // cond:
            // result: (MOVWaddr [off1+off2] {sym} ptr)
            while (true)
            {
                var off1 = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var sym = v_0.Aux;
                var ptr = v_0.Args[0L];
                v.reset(OpMIPSMOVWaddr);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                return true;
            } 
            // match: (ADDconst [0] x)
            // cond:
            // result: x
 
            // match: (ADDconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(c+d))])
 
            // match: (ADDconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(c+d))])
            while (true)
            {
                var c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(c + d));
                return true;
            } 
            // match: (ADDconst [c] (ADDconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(c+d))] x)
 
            // match: (ADDconst [c] (ADDconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(c+d))] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSADDconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSADDconst);
                v.AuxInt = int64(int32(c + d));
                v.AddArg(x);
                return true;
            } 
            // match: (ADDconst [c] (SUBconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(c-d))] x)
 
            // match: (ADDconst [c] (SUBconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(c-d))] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSUBconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSADDconst);
                v.AuxInt = int64(int32(c - d));
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSAND_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (AND x (MOVWconst [c]))
            // cond:
            // result: (ANDconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(OpMIPSANDconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (AND (MOVWconst [c]) x)
            // cond:
            // result: (ANDconst [c] x)
 
            // match: (AND (MOVWconst [c]) x)
            // cond:
            // result: (ANDconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(OpMIPSANDconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (AND x x)
            // cond:
            // result: x
 
            // match: (AND x x)
            // cond:
            // result: x
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (AND (SGTUconst [1] x) (SGTUconst [1] y))
            // cond:
            // result: (SGTUconst [1] (OR <x.Type> x y))
 
            // match: (AND (SGTUconst [1] x) (SGTUconst [1] y))
            // cond:
            // result: (SGTUconst [1] (OR <x.Type> x y))
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSGTUconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSSGTUconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = 1L;
                var v0 = b.NewValue0(v.Pos, OpMIPSOR, x.Type);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            } 
            // match: (AND (SGTUconst [1] y) (SGTUconst [1] x))
            // cond:
            // result: (SGTUconst [1] (OR <x.Type> x y))
 
            // match: (AND (SGTUconst [1] y) (SGTUconst [1] x))
            // cond:
            // result: (SGTUconst [1] (OR <x.Type> x y))
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSGTUconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                y = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSSGTUconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                x = v_1.Args[0L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = 1L;
                v0 = b.NewValue0(v.Pos, OpMIPSOR, x.Type);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSANDconst_0(ref Value v)
        { 
            // match: (ANDconst [0] _)
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (ANDconst [-1] x)
            // cond:
            // result: x
 
            // match: (ANDconst [-1] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != -1L)
                {
                    break;
                }
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (ANDconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [c&d])
 
            // match: (ANDconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [c&d])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = c & d;
                return true;
            } 
            // match: (ANDconst [c] (ANDconst [d] x))
            // cond:
            // result: (ANDconst [c&d] x)
 
            // match: (ANDconst [c] (ANDconst [d] x))
            // cond:
            // result: (ANDconst [c&d] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSANDconst);
                v.AuxInt = c & d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSCMOVZ_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (CMOVZ _ b (MOVWconst [0]))
            // cond:
            // result: b
            while (true)
            {
                _ = v.Args[2L];
                b = v.Args[1L];
                var v_2 = v.Args[2L];
                if (v_2.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_2.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = b.Type;
                v.AddArg(b);
                return true;
            } 
            // match: (CMOVZ a _ (MOVWconst [c]))
            // cond: c!=0
            // result: a
 
            // match: (CMOVZ a _ (MOVWconst [c]))
            // cond: c!=0
            // result: a
            while (true)
            {
                _ = v.Args[2L];
                var a = v.Args[0L];
                v_2 = v.Args[2L];
                if (v_2.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_2.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = a.Type;
                v.AddArg(a);
                return true;
            } 
            // match: (CMOVZ a (MOVWconst [0]) c)
            // cond:
            // result: (CMOVZzero a c)
 
            // match: (CMOVZ a (MOVWconst [0]) c)
            // cond:
            // result: (CMOVZzero a c)
            while (true)
            {
                _ = v.Args[2L];
                a = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                c = v.Args[2L];
                v.reset(OpMIPSCMOVZzero);
                v.AddArg(a);
                v.AddArg(c);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSCMOVZzero_0(ref Value v)
        { 
            // match: (CMOVZzero _ (MOVWconst [0]))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (CMOVZzero a (MOVWconst [c]))
            // cond: c!=0
            // result: a
 
            // match: (CMOVZzero a (MOVWconst [c]))
            // cond: c!=0
            // result: a
            while (true)
            {
                _ = v.Args[1L];
                var a = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(c != 0L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = a.Type;
                v.AddArg(a);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSLoweredAtomicAdd_0(ref Value v)
        { 
            // match: (LoweredAtomicAdd ptr (MOVWconst [c]) mem)
            // cond: is16Bit(c)
            // result: (LoweredAtomicAddconst [c] ptr mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                var mem = v.Args[2L];
                if (!(is16Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPSLoweredAtomicAddconst);
                v.AuxInt = c;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSLoweredAtomicStore_0(ref Value v)
        { 
            // match: (LoweredAtomicStore ptr (MOVWconst [0]) mem)
            // cond:
            // result: (LoweredAtomicStorezero ptr mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                var mem = v.Args[2L];
                v.reset(OpMIPSLoweredAtomicStorezero);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVBUload_0(ref Value v)
        { 
            // match: (MOVBUload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVBUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVBUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVBUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBUload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVBUreg x)
 
            // match: (MOVBUload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVBUreg x)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVBstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBUreg);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVBUreg_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVBUreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVBUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBUreg <t> x:(MOVBload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBUload <t> [off] {sym} ptr mem)
 
            // match: (MOVBUreg <t> x:(MOVBload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBUload <t> [off] {sym} ptr mem)
            while (true)
            {
                var t = v.Type;
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBload)
                {
                    break;
                }
                var off = x.AuxInt;
                var sym = x.Aux;
                _ = x.Args[1L];
                var ptr = x.Args[0L];
                var mem = x.Args[1L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, t);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVBUreg (ANDconst [c] x))
            // cond:
            // result: (ANDconst [c&0xff] x)
 
            // match: (MOVBUreg (ANDconst [c] x))
            // cond:
            // result: (ANDconst [c&0xff] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSANDconst);
                v.AuxInt = c & 0xffUL;
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBUreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(uint8(c))])
 
            // match: (MOVBUreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(uint8(c))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(uint8(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVBload_0(ref Value v)
        { 
            // match: (MOVBload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVBload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVBload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVBreg x)
 
            // match: (MOVBload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVBreg x)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVBstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBreg);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVBreg_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVBreg x:(MOVBload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPSMOVBload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBreg x:(MOVBreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVBreg x:(MOVBreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBreg <t> x:(MOVBUload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBload <t> [off] {sym} ptr mem)
 
            // match: (MOVBreg <t> x:(MOVBUload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBload <t> [off] {sym} ptr mem)
            while (true)
            {
                var t = v.Type;
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUload)
                {
                    break;
                }
                var off = x.AuxInt;
                var sym = x.Aux;
                _ = x.Args[1L];
                var ptr = x.Args[0L];
                var mem = x.Args[1L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVBload, t);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVBreg (ANDconst [c] x))
            // cond: c & 0x80 == 0
            // result: (ANDconst [c&0x7f] x)
 
            // match: (MOVBreg (ANDconst [c] x))
            // cond: c & 0x80 == 0
            // result: (ANDconst [c&0x7f] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(c & 0x80UL == 0L))
                {
                    break;
                }
                v.reset(OpMIPSANDconst);
                v.AuxInt = c & 0x7fUL;
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int8(c))])
 
            // match: (MOVBreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int8(c))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int8(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVBstore_0(ref Value v)
        { 
            // match: (MOVBstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVBstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVBstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVWconst [0]) mem)
            // cond:
            // result: (MOVBstorezero [off] {sym} ptr mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVWconst [0]) mem)
            // cond:
            // result: (MOVBstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstorezero);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVBreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVBUreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVBUreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVHreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVHreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVHreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVHUreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVHUreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVHUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVWreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVWreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVBstorezero_0(ref Value v)
        { 
            // match: (MOVBstorezero [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVBstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVBstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVBstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVBstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVBstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVDload_0(ref Value v)
        { 
            // match: (MOVDload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVDload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVDload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVDload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVDload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVDload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVDload [off] {sym} ptr (MOVDstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
 
            // match: (MOVDload [off] {sym} ptr (MOVDstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVDstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVDstore_0(ref Value v)
        { 
            // match: (MOVDstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVDstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVDstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVDstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVDstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVDstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVFload_0(ref Value v)
        { 
            // match: (MOVFload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVFload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVFload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVFload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVFload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVFload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVFload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVFload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVFload [off] {sym} ptr (MOVFstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
 
            // match: (MOVFload [off] {sym} ptr (MOVFstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVFstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVFstore_0(ref Value v)
        { 
            // match: (MOVFstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVFstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVFstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVFstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVFstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVFstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVFstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVFstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVHUload_0(ref Value v)
        { 
            // match: (MOVHUload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVHUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVHUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHUload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVHUreg x)
 
            // match: (MOVHUload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVHUreg x)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVHstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHUreg);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVHUreg_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVHUreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHUreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVHUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg x:(MOVHUreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHUreg x:(MOVHUreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVHUreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg <t> x:(MOVHload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVHUload <t> [off] {sym} ptr mem)
 
            // match: (MOVHUreg <t> x:(MOVHload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVHUload <t> [off] {sym} ptr mem)
            while (true)
            {
                var t = v.Type;
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVHload)
                {
                    break;
                }
                var off = x.AuxInt;
                var sym = x.Aux;
                _ = x.Args[1L];
                var ptr = x.Args[0L];
                var mem = x.Args[1L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVHUload, t);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVHUreg (ANDconst [c] x))
            // cond:
            // result: (ANDconst [c&0xffff] x)
 
            // match: (MOVHUreg (ANDconst [c] x))
            // cond:
            // result: (ANDconst [c&0xffff] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSANDconst);
                v.AuxInt = c & 0xffffUL;
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(uint16(c))])
 
            // match: (MOVHUreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(uint16(c))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(uint16(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVHload_0(ref Value v)
        { 
            // match: (MOVHload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVHload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVHload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVHreg x)
 
            // match: (MOVHload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVHreg x)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVHstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHreg);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVHreg_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVHreg x:(MOVBload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPSMOVBload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVHload _ _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHreg x:(MOVHload _ _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVHload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVBreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHreg x:(MOVBreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVBUreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHreg x:(MOVBUreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVHreg _))
            // cond:
            // result: (MOVWreg x)
 
            // match: (MOVHreg x:(MOVHreg _))
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVHreg)
                {
                    break;
                }
                v.reset(OpMIPSMOVWreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg <t> x:(MOVHUload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVHload <t> [off] {sym} ptr mem)
 
            // match: (MOVHreg <t> x:(MOVHUload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVHload <t> [off] {sym} ptr mem)
            while (true)
            {
                var t = v.Type;
                x = v.Args[0L];
                if (x.Op != OpMIPSMOVHUload)
                {
                    break;
                }
                var off = x.AuxInt;
                var sym = x.Aux;
                _ = x.Args[1L];
                var ptr = x.Args[0L];
                var mem = x.Args[1L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVHload, t);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVHreg (ANDconst [c] x))
            // cond: c & 0x8000 == 0
            // result: (ANDconst [c&0x7fff] x)
 
            // match: (MOVHreg (ANDconst [c] x))
            // cond: c & 0x8000 == 0
            // result: (ANDconst [c&0x7fff] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(c & 0x8000UL == 0L))
                {
                    break;
                }
                v.reset(OpMIPSANDconst);
                v.AuxInt = c & 0x7fffUL;
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int16(c))])
 
            // match: (MOVHreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int16(c))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int16(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVHstore_0(ref Value v)
        { 
            // match: (MOVHstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVHstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVHstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off] {sym} ptr (MOVWconst [0]) mem)
            // cond:
            // result: (MOVHstorezero [off] {sym} ptr mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVWconst [0]) mem)
            // cond:
            // result: (MOVHstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPSMOVHstorezero);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVHreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off] {sym} ptr (MOVHUreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVHUreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVHUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off] {sym} ptr (MOVWreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVWreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVHstorezero_0(ref Value v)
        { 
            // match: (MOVHstorezero [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVHstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVHstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVHstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVWload_0(ref Value v)
        { 
            // match: (MOVWload [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVWload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVWload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
 
            // match: (MOVWload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWstore)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVWreg_0(ref Value v)
        { 
            // match: (MOVWreg x)
            // cond: x.Uses == 1
            // result: (MOVWnop x)
            while (true)
            {
                var x = v.Args[0L];
                if (!(x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWnop);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [c])
 
            // match: (MOVWreg (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [c])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = c;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVWstore_0(ref Value v)
        { 
            // match: (MOVWstore [off1] {sym} x:(ADDconst [off2] ptr) val mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVWstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
 
            // match: (MOVWstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWconst [0]) mem)
            // cond:
            // result: (MOVWstorezero [off] {sym} ptr mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWconst [0]) mem)
            // cond:
            // result: (MOVWstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPSMOVWstorezero);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWreg x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMOVWstorezero_0(ref Value v)
        { 
            // match: (MOVWstorezero [off1] {sym} x:(ADDconst [off2] ptr) mem)
            // cond: (is16Bit(off1+off2) || x.Uses == 1)
            // result: (MOVWstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var x = v.Args[0L];
                if (x.Op != OpMIPSADDconst)
                {
                    break;
                }
                var off2 = x.AuxInt;
                var ptr = x.Args[0L];
                var mem = v.Args[1L];
                if (!(is16Bit(off1 + off2) || x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVWstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
 
            // match: (MOVWstorezero [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
            // cond: canMergeSym(sym1,sym2)
            // result: (MOVWstorezero [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSMUL_0(ref Value v)
        { 
            // match: (MUL (MOVWconst [0]) _)
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (MUL _ (MOVWconst [0]))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (MUL _ (MOVWconst [0]))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (MUL (MOVWconst [1]) x)
            // cond:
            // result: x
 
            // match: (MUL (MOVWconst [1]) x)
            // cond:
            // result: x
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                var x = v.Args[1L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (MUL x (MOVWconst [1]))
            // cond:
            // result: x
 
            // match: (MUL x (MOVWconst [1]))
            // cond:
            // result: x
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (MUL (MOVWconst [-1]) x)
            // cond:
            // result: (NEG x)
 
            // match: (MUL (MOVWconst [-1]) x)
            // cond:
            // result: (NEG x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0.AuxInt != -1L)
                {
                    break;
                }
                x = v.Args[1L];
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;
            } 
            // match: (MUL x (MOVWconst [-1]))
            // cond:
            // result: (NEG x)
 
            // match: (MUL x (MOVWconst [-1]))
            // cond:
            // result: (NEG x)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;
            } 
            // match: (MUL (MOVWconst [c]) x)
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
 
            // match: (MUL (MOVWconst [c]) x)
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v.Args[1L];
                if (!(isPowerOfTwo(int64(uint32(c)))))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = log2(int64(uint32(c)));
                v.AddArg(x);
                return true;
            } 
            // match: (MUL x (MOVWconst [c]))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
 
            // match: (MUL x (MOVWconst [c]))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(isPowerOfTwo(int64(uint32(c)))))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = log2(int64(uint32(c)));
                v.AddArg(x);
                return true;
            } 
            // match: (MUL (MOVWconst [c]) (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(c)*int32(d))])
 
            // match: (MUL (MOVWconst [c]) (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(c)*int32(d))])
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(c) * int32(d));
                return true;
            } 
            // match: (MUL (MOVWconst [d]) (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int32(c)*int32(d))])
 
            // match: (MUL (MOVWconst [d]) (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int32(c)*int32(d))])
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(c) * int32(d));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSNEG_0(ref Value v)
        { 
            // match: (NEG (MOVWconst [c]))
            // cond:
            // result: (MOVWconst [int64(int32(-c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(-c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSNOR_0(ref Value v)
        { 
            // match: (NOR x (MOVWconst [c]))
            // cond:
            // result: (NORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(OpMIPSNORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (NOR (MOVWconst [c]) x)
            // cond:
            // result: (NORconst [c] x)
 
            // match: (NOR (MOVWconst [c]) x)
            // cond:
            // result: (NORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(OpMIPSNORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSNORconst_0(ref Value v)
        { 
            // match: (NORconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [^(c|d)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = ~(c | d);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSOR_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (OR x (MOVWconst [c]))
            // cond:
            // result: (ORconst  [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(OpMIPSORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (OR (MOVWconst [c]) x)
            // cond:
            // result: (ORconst  [c] x)
 
            // match: (OR (MOVWconst [c]) x)
            // cond:
            // result: (ORconst  [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(OpMIPSORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (OR x x)
            // cond:
            // result: x
 
            // match: (OR x x)
            // cond:
            // result: x
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (OR (SGTUzero x) (SGTUzero y))
            // cond:
            // result: (SGTUzero (OR <x.Type> x y))
 
            // match: (OR (SGTUzero x) (SGTUzero y))
            // cond:
            // result: (SGTUzero (OR <x.Type> x y))
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSGTUzero)
                {
                    break;
                }
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSSGTUzero)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(OpMIPSSGTUzero);
                var v0 = b.NewValue0(v.Pos, OpMIPSOR, x.Type);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            } 
            // match: (OR (SGTUzero y) (SGTUzero x))
            // cond:
            // result: (SGTUzero (OR <x.Type> x y))
 
            // match: (OR (SGTUzero y) (SGTUzero x))
            // cond:
            // result: (SGTUzero (OR <x.Type> x y))
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSGTUzero)
                {
                    break;
                }
                y = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSSGTUzero)
                {
                    break;
                }
                x = v_1.Args[0L];
                v.reset(OpMIPSSGTUzero);
                v0 = b.NewValue0(v.Pos, OpMIPSOR, x.Type);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSORconst_0(ref Value v)
        { 
            // match: (ORconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (ORconst [-1] _)
            // cond:
            // result: (MOVWconst [-1])
 
            // match: (ORconst [-1] _)
            // cond:
            // result: (MOVWconst [-1])
            while (true)
            {
                if (v.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = -1L;
                return true;
            } 
            // match: (ORconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [c|d])
 
            // match: (ORconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [c|d])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = c | d;
                return true;
            } 
            // match: (ORconst [c] (ORconst [d] x))
            // cond:
            // result: (ORconst [c|d] x)
 
            // match: (ORconst [c] (ORconst [d] x))
            // cond:
            // result: (ORconst [c|d] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSORconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSORconst);
                v.AuxInt = c | d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGT_0(ref Value v)
        { 
            // match: (SGT (MOVWconst [c]) x)
            // cond:
            // result: (SGTconst  [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var x = v.Args[1L];
                v.reset(OpMIPSSGTconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (SGT x (MOVWconst [0]))
            // cond:
            // result: (SGTzero x)
 
            // match: (SGT x (MOVWconst [0]))
            // cond:
            // result: (SGTzero x)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSSGTzero);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGTU_0(ref Value v)
        { 
            // match: (SGTU (MOVWconst [c]) x)
            // cond:
            // result: (SGTUconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var x = v.Args[1L];
                v.reset(OpMIPSSGTUconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (SGTU x (MOVWconst [0]))
            // cond:
            // result: (SGTUzero x)
 
            // match: (SGTU x (MOVWconst [0]))
            // cond:
            // result: (SGTUzero x)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSSGTUzero);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGTUconst_0(ref Value v)
        { 
            // match: (SGTUconst [c] (MOVWconst [d]))
            // cond: uint32(c)>uint32(d)
            // result: (MOVWconst [1])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(uint32(c) > uint32(d)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTUconst [c] (MOVWconst [d]))
            // cond: uint32(c)<=uint32(d)
            // result: (MOVWconst [0])
 
            // match: (SGTUconst [c] (MOVWconst [d]))
            // cond: uint32(c)<=uint32(d)
            // result: (MOVWconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(uint32(c) <= uint32(d)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTUconst [c] (MOVBUreg _))
            // cond: 0xff < uint32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTUconst [c] (MOVBUreg _))
            // cond: 0xff < uint32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                if (!(0xffUL < uint32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTUconst [c] (MOVHUreg _))
            // cond: 0xffff < uint32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTUconst [c] (MOVHUreg _))
            // cond: 0xffff < uint32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVHUreg)
                {
                    break;
                }
                if (!(0xffffUL < uint32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTUconst [c] (ANDconst [m] _))
            // cond: uint32(m) < uint32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTUconst [c] (ANDconst [m] _))
            // cond: uint32(m) < uint32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(uint32(m) < uint32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTUconst [c] (SRLconst _ [d]))
            // cond: uint32(d) <= 31 && 1<<(32-uint32(d)) <= uint32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTUconst [c] (SRLconst _ [d]))
            // cond: uint32(d) <= 31 && 1<<(32-uint32(d)) <= uint32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSRLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(uint32(d) <= 31L && 1L << (int)((32L - uint32(d))) <= uint32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGTUzero_0(ref Value v)
        { 
            // match: (SGTUzero (MOVWconst [d]))
            // cond: uint32(d) != 0
            // result: (MOVWconst [1])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(uint32(d) != 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTUzero (MOVWconst [d]))
            // cond: uint32(d) == 0
            // result: (MOVWconst [0])
 
            // match: (SGTUzero (MOVWconst [d]))
            // cond: uint32(d) == 0
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(uint32(d) == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGTconst_0(ref Value v)
        { 
            // match: (SGTconst [c] (MOVWconst [d]))
            // cond: int32(c) > int32(d)
            // result: (MOVWconst [1])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(int32(c) > int32(d)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVWconst [d]))
            // cond: int32(c) <= int32(d)
            // result: (MOVWconst [0])
 
            // match: (SGTconst [c] (MOVWconst [d]))
            // cond: int32(c) <= int32(d)
            // result: (MOVWconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(int32(c) <= int32(d)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: 0x7f < int32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: 0x7f < int32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVBreg)
                {
                    break;
                }
                if (!(0x7fUL < int32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: int32(c) <= -0x80
            // result: (MOVWconst [0])
 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: int32(c) <= -0x80
            // result: (MOVWconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVBreg)
                {
                    break;
                }
                if (!(int32(c) <= -0x80UL))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: 0xff < int32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: 0xff < int32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                if (!(0xffUL < int32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: int32(c) < 0
            // result: (MOVWconst [0])
 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: int32(c) < 0
            // result: (MOVWconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVBUreg)
                {
                    break;
                }
                if (!(int32(c) < 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: 0x7fff < int32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: 0x7fff < int32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVHreg)
                {
                    break;
                }
                if (!(0x7fffUL < int32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: int32(c) <= -0x8000
            // result: (MOVWconst [0])
 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: int32(c) <= -0x8000
            // result: (MOVWconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVHreg)
                {
                    break;
                }
                if (!(int32(c) <= -0x8000UL))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: 0xffff < int32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: 0xffff < int32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVHUreg)
                {
                    break;
                }
                if (!(0xffffUL < int32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: int32(c) < 0
            // result: (MOVWconst [0])
 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: int32(c) < 0
            // result: (MOVWconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVHUreg)
                {
                    break;
                }
                if (!(int32(c) < 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGTconst_10(ref Value v)
        { 
            // match: (SGTconst [c] (ANDconst [m] _))
            // cond: 0 <= int32(m) && int32(m) < int32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSANDconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(0L <= int32(m) && int32(m) < int32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (SRLconst _ [d]))
            // cond: 0 <= int32(c) && uint32(d) <= 31 && 1<<(32-uint32(d)) <= int32(c)
            // result: (MOVWconst [1])
 
            // match: (SGTconst [c] (SRLconst _ [d]))
            // cond: 0 <= int32(c) && uint32(d) <= 31 && 1<<(32-uint32(d)) <= int32(c)
            // result: (MOVWconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSRLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(0L <= int32(c) && uint32(d) <= 31L && 1L << (int)((32L - uint32(d))) <= int32(c)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSGTzero_0(ref Value v)
        { 
            // match: (SGTzero (MOVWconst [d]))
            // cond: int32(d) > 0
            // result: (MOVWconst [1])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(int32(d) > 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTzero (MOVWconst [d]))
            // cond: int32(d) <= 0
            // result: (MOVWconst [0])
 
            // match: (SGTzero (MOVWconst [d]))
            // cond: int32(d) <= 0
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(int32(d) <= 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSLL_0(ref Value v)
        { 
            // match: (SLL _ (MOVWconst [c]))
            // cond: uint32(c)>=32
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) >= 32L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SLL x (MOVWconst [c]))
            // cond:
            // result: (SLLconst x [c])
 
            // match: (SLL x (MOVWconst [c]))
            // cond:
            // result: (SLLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPSSLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSLLconst_0(ref Value v)
        { 
            // match: (SLLconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(d)<<uint32(c)))])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(uint32(d) << (int)(uint32(c))));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSRA_0(ref Value v)
        { 
            // match: (SRA x (MOVWconst [c]))
            // cond: uint32(c)>=32
            // result: (SRAconst x [31])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) >= 32L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (SRA x (MOVWconst [c]))
            // cond:
            // result: (SRAconst x [c])
 
            // match: (SRA x (MOVWconst [c]))
            // cond:
            // result: (SRAconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPSSRAconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSRAconst_0(ref Value v)
        { 
            // match: (SRAconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(d)>>uint32(c))])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(d) >> (int)(uint32(c)));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSRL_0(ref Value v)
        { 
            // match: (SRL _ (MOVWconst [c]))
            // cond: uint32(c)>=32
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) >= 32L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SRL x (MOVWconst [c]))
            // cond:
            // result: (SRLconst x [c])
 
            // match: (SRL x (MOVWconst [c]))
            // cond:
            // result: (SRLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPSSRLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSRLconst_0(ref Value v)
        { 
            // match: (SRLconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(uint32(d)>>uint32(c))])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(uint32(d) >> (int)(uint32(c)));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSUB_0(ref Value v)
        { 
            // match: (SUB x (MOVWconst [c]))
            // cond:
            // result: (SUBconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(OpMIPSSUBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (SUB x x)
            // cond:
            // result: (MOVWconst [0])
 
            // match: (SUB x x)
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SUB (MOVWconst [0]) x)
            // cond:
            // result: (NEG x)
 
            // match: (SUB (MOVWconst [0]) x)
            // cond:
            // result: (NEG x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[1L];
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSSUBconst_0(ref Value v)
        { 
            // match: (SUBconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (SUBconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(d-c))])
 
            // match: (SUBconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [int64(int32(d-c))])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(d - c));
                return true;
            } 
            // match: (SUBconst [c] (SUBconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(-c-d))] x)
 
            // match: (SUBconst [c] (SUBconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(-c-d))] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSSUBconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSADDconst);
                v.AuxInt = int64(int32(-c - d));
                v.AddArg(x);
                return true;
            } 
            // match: (SUBconst [c] (ADDconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(-c+d))] x)
 
            // match: (SUBconst [c] (ADDconst [d] x))
            // cond:
            // result: (ADDconst [int64(int32(-c+d))] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSADDconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSADDconst);
                v.AuxInt = int64(int32(-c + d));
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSXOR_0(ref Value v)
        { 
            // match: (XOR x (MOVWconst [c]))
            // cond:
            // result: (XORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(OpMIPSXORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XOR (MOVWconst [c]) x)
            // cond:
            // result: (XORconst [c] x)
 
            // match: (XOR (MOVWconst [c]) x)
            // cond:
            // result: (XORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XOR x x)
            // cond:
            // result: (MOVWconst [0])
 
            // match: (XOR x x)
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMIPSXORconst_0(ref Value v)
        { 
            // match: (XORconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (XORconst [-1] x)
            // cond:
            // result: (NORconst [0] x)
 
            // match: (XORconst [-1] x)
            // cond:
            // result: (NORconst [0] x)
            while (true)
            {
                if (v.AuxInt != -1L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(OpMIPSNORconst);
                v.AuxInt = 0L;
                v.AddArg(x);
                return true;
            } 
            // match: (XORconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [c^d])
 
            // match: (XORconst [c] (MOVWconst [d]))
            // cond:
            // result: (MOVWconst [c^d])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = c ^ d;
                return true;
            } 
            // match: (XORconst [c] (XORconst [d] x))
            // cond:
            // result: (XORconst [c^d] x)
 
            // match: (XORconst [c] (XORconst [d] x))
            // cond:
            // result: (XORconst [c^d] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSXORconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = c ^ d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMod16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod16 x y)
            // cond:
            // result: (Select0 (DIV (SignExt16to32 x) (SignExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
                var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMod16u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod16u x y)
            // cond:
            // result: (Select0 (DIVU (ZeroExt16to32 x) (ZeroExt16to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMod32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod32 x y)
            // cond:
            // result: (Select0 (DIV x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMod32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod32u x y)
            // cond:
            // result: (Select0 (DIVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMod8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod8 x y)
            // cond:
            // result: (Select0 (DIV (SignExt8to32 x) (SignExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIV, types.NewTuple(typ.Int32, typ.Int32));
                var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMod8u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod8u x y)
            // cond:
            // result: (Select0 (DIVU (ZeroExt8to32 x) (ZeroExt8to32 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPSDIVU, types.NewTuple(typ.UInt32, typ.UInt32));
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMove_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Move [0] _ _ mem)
            // cond:
            // result: mem
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                _ = v.Args[2L];
                var mem = v.Args[2L];
                v.reset(OpCopy);
                v.Type = mem.Type;
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [1] dst src mem)
            // cond:
            // result: (MOVBstore dst (MOVBUload src mem) mem)
 
            // match: (Move [1] dst src mem)
            // cond:
            // result: (MOVBstore dst (MOVBUload src mem) mem)
            while (true)
            {
                if (v.AuxInt != 1L)
                {
                    break;
                }
                _ = v.Args[2L];
                var dst = v.Args[0L];
                var src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AddArg(dst);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [2] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore dst (MOVHUload src mem) mem)
 
            // match: (Move [2] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore dst (MOVHUload src mem) mem)
            while (true)
            {
                if (v.AuxInt != 2L)
                {
                    break;
                }
                var t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVHUload, typ.UInt16);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [2] dst src mem)
            // cond:
            // result: (MOVBstore [1] dst (MOVBUload [1] src mem)         (MOVBstore dst (MOVBUload src mem) mem))
 
            // match: (Move [2] dst src mem)
            // cond:
            // result: (MOVBstore [1] dst (MOVBUload [1] src mem)         (MOVBstore dst (MOVBUload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 2L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = 1L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v0.AuxInt = 1L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v1.AddArg(dst);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [4] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore dst (MOVWload src mem) mem)
 
            // match: (Move [4] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore dst (MOVWload src mem) mem)
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [4] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] dst (MOVHUload [2] src mem)         (MOVHstore dst (MOVHUload src mem) mem))
 
            // match: (Move [4] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] dst (MOVHUload [2] src mem)         (MOVHstore dst (MOVHUload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = 2L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVHUload, typ.UInt16);
                v0.AuxInt = 2L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVHUload, typ.UInt16);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [4] dst src mem)
            // cond:
            // result: (MOVBstore [3] dst (MOVBUload [3] src mem)         (MOVBstore [2] dst (MOVBUload [2] src mem)             (MOVBstore [1] dst (MOVBUload [1] src mem)                 (MOVBstore dst (MOVBUload src mem) mem))))
 
            // match: (Move [4] dst src mem)
            // cond:
            // result: (MOVBstore [3] dst (MOVBUload [3] src mem)         (MOVBstore [2] dst (MOVBUload [2] src mem)             (MOVBstore [1] dst (MOVBUload [1] src mem)                 (MOVBstore dst (MOVBUload src mem) mem))))
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = 3L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v0.AuxInt = 3L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v2.AuxInt = 2L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v3.AuxInt = 1L;
                v3.AddArg(dst);
                var v4 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v4.AuxInt = 1L;
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v5.AddArg(dst);
                var v6 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v6.AddArg(src);
                v6.AddArg(mem);
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [3] dst src mem)
            // cond:
            // result: (MOVBstore [2] dst (MOVBUload [2] src mem)         (MOVBstore [1] dst (MOVBUload [1] src mem)             (MOVBstore dst (MOVBUload src mem) mem)))
 
            // match: (Move [3] dst src mem)
            // cond:
            // result: (MOVBstore [2] dst (MOVBUload [2] src mem)         (MOVBstore [1] dst (MOVBUload [1] src mem)             (MOVBstore dst (MOVBUload src mem) mem)))
            while (true)
            {
                if (v.AuxInt != 3L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = 2L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v0.AuxInt = 2L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v1.AuxInt = 1L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v2.AuxInt = 1L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVBUload, typ.UInt8);
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [8] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [4] dst (MOVWload [4] src mem)         (MOVWstore dst (MOVWload src mem) mem))
 
            // match: (Move [8] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [4] dst (MOVWload [4] src mem)         (MOVWstore dst (MOVWload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 8L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [8] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [6] dst (MOVHload [6] src mem)         (MOVHstore [4] dst (MOVHload [4] src mem)             (MOVHstore [2] dst (MOVHload [2] src mem)                 (MOVHstore dst (MOVHload src mem) mem))))
 
            // match: (Move [8] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [6] dst (MOVHload [6] src mem)         (MOVHstore [4] dst (MOVHload [4] src mem)             (MOVHstore [2] dst (MOVHload [2] src mem)                 (MOVHstore dst (MOVHload src mem) mem))))
            while (true)
            {
                if (v.AuxInt != 8L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = 6L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v0.AuxInt = 6L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v2.AuxInt = 4L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v3.AuxInt = 2L;
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v4.AuxInt = 2L;
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v5 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v5.AddArg(dst);
                v6 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v6.AddArg(src);
                v6.AddArg(mem);
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMove_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Move [6] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [4] dst (MOVHload [4] src mem)         (MOVHstore [2] dst (MOVHload [2] src mem)             (MOVHstore dst (MOVHload src mem) mem)))
            while (true)
            {
                if (v.AuxInt != 6L)
                {
                    break;
                }
                var t = v.Aux;
                _ = v.Args[2L];
                var dst = v.Args[0L];
                var src = v.Args[1L];
                var mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(dst);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v2.AuxInt = 2L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v3.AddArg(dst);
                var v4 = b.NewValue0(v.Pos, OpMIPSMOVHload, typ.Int16);
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [12] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [8] dst (MOVWload [8] src mem)         (MOVWstore [4] dst (MOVWload [4] src mem)             (MOVWstore dst (MOVWload src mem) mem)))
 
            // match: (Move [12] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [8] dst (MOVWload [8] src mem)         (MOVWstore [4] dst (MOVWload [4] src mem)             (MOVWstore dst (MOVWload src mem) mem)))
            while (true)
            {
                if (v.AuxInt != 12L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = 8L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v0.AuxInt = 8L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v2.AuxInt = 4L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [16] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [12] dst (MOVWload [12] src mem)         (MOVWstore [8] dst (MOVWload [8] src mem)             (MOVWstore [4] dst (MOVWload [4] src mem)                 (MOVWstore dst (MOVWload src mem) mem))))
 
            // match: (Move [16] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [12] dst (MOVWload [12] src mem)         (MOVWstore [8] dst (MOVWload [8] src mem)             (MOVWstore [4] dst (MOVWload [4] src mem)                 (MOVWstore dst (MOVWload src mem) mem))))
            while (true)
            {
                if (v.AuxInt != 16L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = 12L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v0.AuxInt = 12L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v1.AuxInt = 8L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v2.AuxInt = 8L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v3.AuxInt = 4L;
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v4.AuxInt = 4L;
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v5.AddArg(dst);
                var v6 = b.NewValue0(v.Pos, OpMIPSMOVWload, typ.UInt32);
                v6.AddArg(src);
                v6.AddArg(mem);
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [s] {t} dst src mem)
            // cond: (s > 16 || t.(*types.Type).Alignment()%4 != 0)
            // result: (LoweredMove [t.(*types.Type).Alignment()]         dst         src         (ADDconst <src.Type> src [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
 
            // match: (Move [s] {t} dst src mem)
            // cond: (s > 16 || t.(*types.Type).Alignment()%4 != 0)
            // result: (LoweredMove [t.(*types.Type).Alignment()]         dst         src         (ADDconst <src.Type> src [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
            while (true)
            {
                var s = v.AuxInt;
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(s > 16L || t._<ref types.Type>().Alignment() % 4L != 0L))
                {
                    break;
                }
                v.reset(OpMIPSLoweredMove);
                v.AuxInt = t._<ref types.Type>().Alignment();
                v.AddArg(dst);
                v.AddArg(src);
                v0 = b.NewValue0(v.Pos, OpMIPSADDconst, src.Type);
                v0.AuxInt = s - moveSize(t._<ref types.Type>().Alignment(), config);
                v0.AddArg(src);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpMul16_0(ref Value v)
        { 
            // match: (Mul16 x y)
            // cond:
            // result: (MUL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSMUL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMul32_0(ref Value v)
        { 
            // match: (Mul32 x y)
            // cond:
            // result: (MUL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSMUL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMul32F_0(ref Value v)
        { 
            // match: (Mul32F x y)
            // cond:
            // result: (MULF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSMULF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMul32uhilo_0(ref Value v)
        { 
            // match: (Mul32uhilo x y)
            // cond:
            // result: (MULTU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSMULTU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMul64F_0(ref Value v)
        { 
            // match: (Mul64F x y)
            // cond:
            // result: (MULD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSMULD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpMul8_0(ref Value v)
        { 
            // match: (Mul8 x y)
            // cond:
            // result: (MUL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSMUL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeg16_0(ref Value v)
        { 
            // match: (Neg16 x)
            // cond:
            // result: (NEG x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeg32_0(ref Value v)
        { 
            // match: (Neg32 x)
            // cond:
            // result: (NEG x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeg32F_0(ref Value v)
        { 
            // match: (Neg32F x)
            // cond:
            // result: (NEGF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNEGF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeg64F_0(ref Value v)
        { 
            // match: (Neg64F x)
            // cond:
            // result: (NEGD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNEGD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeg8_0(ref Value v)
        { 
            // match: (Neg8 x)
            // cond:
            // result: (NEG x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNEG);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq16 x y)
            // cond:
            // result: (SGTU (XOR (ZeroExt16to32 x) (ZeroExt16to32 y)) (MOVWconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq32 x y)
            // cond:
            // result: (SGTU (XOR x y) (MOVWconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq32F x y)
            // cond:
            // result: (FPFlagFalse (CMPEQF x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagFalse);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq64F x y)
            // cond:
            // result: (FPFlagFalse (CMPEQD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSFPFlagFalse);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMPEQD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq8 x y)
            // cond:
            // result: (SGTU (XOR (ZeroExt8to32 x) (ZeroExt8to32 y)) (MOVWconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeqB_0(ref Value v)
        { 
            // match: (NeqB x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNeqPtr_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (NeqPtr x y)
            // cond:
            // result: (SGTU (XOR x y) (MOVWconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPSXOR, typ.UInt32);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNilCheck_0(ref Value v)
        { 
            // match: (NilCheck ptr mem)
            // cond:
            // result: (LoweredNilCheck ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPSLoweredNilCheck);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpNot_0(ref Value v)
        { 
            // match: (Not x)
            // cond:
            // result: (XORconst [1] x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSXORconst);
                v.AuxInt = 1L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpOffPtr_0(ref Value v)
        { 
            // match: (OffPtr [off] ptr:(SP))
            // cond:
            // result: (MOVWaddr [off] ptr)
            while (true)
            {
                var off = v.AuxInt;
                var ptr = v.Args[0L];
                if (ptr.Op != OpSP)
                {
                    break;
                }
                v.reset(OpMIPSMOVWaddr);
                v.AuxInt = off;
                v.AddArg(ptr);
                return true;
            } 
            // match: (OffPtr [off] ptr)
            // cond:
            // result: (ADDconst [off] ptr)
 
            // match: (OffPtr [off] ptr)
            // cond:
            // result: (ADDconst [off] ptr)
            while (true)
            {
                off = v.AuxInt;
                ptr = v.Args[0L];
                v.reset(OpMIPSADDconst);
                v.AuxInt = off;
                v.AddArg(ptr);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpOr16_0(ref Value v)
        { 
            // match: (Or16 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpOr32_0(ref Value v)
        { 
            // match: (Or32 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpOr8_0(ref Value v)
        { 
            // match: (Or8 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpOrB_0(ref Value v)
        { 
            // match: (OrB x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRound32F_0(ref Value v)
        { 
            // match: (Round32F x)
            // cond:
            // result: x
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRound64F_0(ref Value v)
        { 
            // match: (Round64F x)
            // cond:
            // result: x
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh16Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux16 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> (ZeroExt16to32 x) (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh16Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux32 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> (ZeroExt16to32 x) y) (MOVWconst [0]) (SGTUconst [32] y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v0.AddArg(y);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh16Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux64 x (Const64 [c]))
            // cond: uint32(c) < 16
            // result: (SRLconst (SLLconst <typ.UInt32> x [16]) [c+16])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 16L))
                {
                    break;
                }
                v.reset(OpMIPSSRLconst);
                v.AuxInt = c + 16L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v0.AuxInt = 16L;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (Rsh16Ux64 _ (Const64 [c]))
            // cond: uint32(c) >= 16
            // result: (MOVWconst [0])
 
            // match: (Rsh16Ux64 _ (Const64 [c]))
            // cond: uint32(c) >= 16
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 16L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpRsh16Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux8 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> (ZeroExt16to32 x) (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh16x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x16 x y)
            // cond:
            // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt16to32 y) (MOVWconst [-1]) (SGTUconst [32] (ZeroExt16to32 y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = -1L;
                v1.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v1.AddArg(v4);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh16x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x32 x y)
            // cond:
            // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> y (MOVWconst [-1]) (SGTUconst [32] y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                v1.AddArg(y);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = -1L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh16x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x64 x (Const64 [c]))
            // cond: uint32(c) < 16
            // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [c+16])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 16L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = c + 16L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v0.AuxInt = 16L;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (Rsh16x64 x (Const64 [c]))
            // cond: uint32(c) >= 16
            // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [31])
 
            // match: (Rsh16x64 x (Const64 [c]))
            // cond: uint32(c) >= 16
            // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [31])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 16L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = 31L;
                v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v0.AuxInt = 16L;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpRsh16x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x8 x y)
            // cond:
            // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt8to32 y) (MOVWconst [-1]) (SGTUconst [32] (ZeroExt8to32 y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = -1L;
                v1.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v1.AddArg(v4);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh32Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux16 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> x (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh32Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux32 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> x y) (MOVWconst [0]) (SGTUconst [32] y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v.AddArg(v2);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh32Ux64_0(ref Value v)
        { 
            // match: (Rsh32Ux64 x (Const64 [c]))
            // cond: uint32(c) < 32
            // result: (SRLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 32L))
                {
                    break;
                }
                v.reset(OpMIPSSRLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh32Ux64 _ (Const64 [c]))
            // cond: uint32(c) >= 32
            // result: (MOVWconst [0])
 
            // match: (Rsh32Ux64 _ (Const64 [c]))
            // cond: uint32(c) >= 32
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 32L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpRsh32Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux8 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> x (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh32x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x16 x y)
            // cond:
            // result: (SRA x ( CMOVZ <typ.UInt32> (ZeroExt16to32 y) (MOVWconst [-1]) (SGTUconst [32] (ZeroExt16to32 y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = -1L;
                v0.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v0.AddArg(v3);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh32x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x32 x y)
            // cond:
            // result: (SRA x ( CMOVZ <typ.UInt32> y (MOVWconst [-1]) (SGTUconst [32] y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = -1L;
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh32x64_0(ref Value v)
        { 
            // match: (Rsh32x64 x (Const64 [c]))
            // cond: uint32(c) < 32
            // result: (SRAconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 32L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh32x64 x (Const64 [c]))
            // cond: uint32(c) >= 32
            // result: (SRAconst x [31])
 
            // match: (Rsh32x64 x (Const64 [c]))
            // cond: uint32(c) >= 32
            // result: (SRAconst x [31])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 32L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = 31L;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpRsh32x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x8 x y)
            // cond:
            // result: (SRA x ( CMOVZ <typ.UInt32> (ZeroExt8to32 y) (MOVWconst [-1]) (SGTUconst [32] (ZeroExt8to32 y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(y);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = -1L;
                v0.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v4.AddArg(y);
                v3.AddArg(v4);
                v0.AddArg(v3);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh8Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux16 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> (ZeroExt8to32 x) (ZeroExt16to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt16to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh8Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux32 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> (ZeroExt8to32 x) y) (MOVWconst [0]) (SGTUconst [32] y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                v0.AddArg(y);
                v.AddArg(v0);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh8Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux64 x (Const64 [c]))
            // cond: uint32(c) < 8
            // result: (SRLconst (SLLconst <typ.UInt32> x [24]) [c+24])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 8L))
                {
                    break;
                }
                v.reset(OpMIPSSRLconst);
                v.AuxInt = c + 24L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v0.AuxInt = 24L;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (Rsh8Ux64 _ (Const64 [c]))
            // cond: uint32(c) >= 8
            // result: (MOVWconst [0])
 
            // match: (Rsh8Ux64 _ (Const64 [c]))
            // cond: uint32(c) >= 8
            // result: (MOVWconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 8L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpRsh8Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux8 <t> x y)
            // cond:
            // result: (CMOVZ (SRL <t> (ZeroExt8to32 x) (ZeroExt8to32 y) ) (MOVWconst [0]) (SGTUconst [32] (ZeroExt8to32 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSSRL, t);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh8x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x16 x y)
            // cond:
            // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt16to32 y) (MOVWconst [-1]) (SGTUconst [32] (ZeroExt16to32 y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = -1L;
                v1.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v1.AddArg(v4);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh8x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x32 x y)
            // cond:
            // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> y (MOVWconst [-1]) (SGTUconst [32] y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                v1.AddArg(y);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = -1L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpRsh8x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x64 x (Const64 [c]))
            // cond: uint32(c) < 8
            // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [c+24])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint32(c) < 8L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = c + 24L;
                var v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v0.AuxInt = 24L;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (Rsh8x64 x (Const64 [c]))
            // cond: uint32(c) >= 8
            // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [31])
 
            // match: (Rsh8x64 x (Const64 [c]))
            // cond: uint32(c) >= 8
            // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [31])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint32(c) >= 8L))
                {
                    break;
                }
                v.reset(OpMIPSSRAconst);
                v.AuxInt = 31L;
                v0 = b.NewValue0(v.Pos, OpMIPSSLLconst, typ.UInt32);
                v0.AuxInt = 24L;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpRsh8x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x8 x y)
            // cond:
            // result: (SRA (SignExt16to32 x) ( CMOVZ <typ.UInt32> (ZeroExt8to32 y) (MOVWconst [-1]) (SGTUconst [32] (ZeroExt8to32 y))))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSRA);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSCMOVZ, typ.UInt32);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v3.AuxInt = -1L;
                v1.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPSSGTUconst, typ.Bool);
                v4.AuxInt = 32L;
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
                v5.AddArg(y);
                v4.AddArg(v5);
                v1.AddArg(v4);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSelect0_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Select0 (Add32carry <t> x y))
            // cond:
            // result: (ADD <t.FieldType(0)> x y)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpAdd32carry)
                {
                    break;
                }
                var t = v_0.Type;
                _ = v_0.Args[1L];
                var x = v_0.Args[0L];
                var y = v_0.Args[1L];
                v.reset(OpMIPSADD);
                v.Type = t.FieldType(0L);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (Select0 (Sub32carry <t> x y))
            // cond:
            // result: (SUB <t.FieldType(0)> x y)
 
            // match: (Select0 (Sub32carry <t> x y))
            // cond:
            // result: (SUB <t.FieldType(0)> x y)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpSub32carry)
                {
                    break;
                }
                t = v_0.Type;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                v.reset(OpMIPSSUB);
                v.Type = t.FieldType(0L);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (Select0 (MULTU (MOVWconst [0]) _))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (Select0 (MULTU (MOVWconst [0]) _))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select0 (MULTU _ (MOVWconst [0])))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (Select0 (MULTU _ (MOVWconst [0])))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select0 (MULTU (MOVWconst [1]) _))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (Select0 (MULTU (MOVWconst [1]) _))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select0 (MULTU _ (MOVWconst [1])))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (Select0 (MULTU _ (MOVWconst [1])))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select0 (MULTU (MOVWconst [-1]) x))
            // cond:
            // result: (CMOVZ (ADDconst <x.Type> [-1] x) (MOVWconst [0]) x)
 
            // match: (Select0 (MULTU (MOVWconst [-1]) x))
            // cond:
            // result: (CMOVZ (ADDconst <x.Type> [-1] x) (MOVWconst [0]) x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != -1L)
                {
                    break;
                }
                x = v_0.Args[1L];
                v.reset(OpMIPSCMOVZ);
                var v0 = b.NewValue0(v.Pos, OpMIPSADDconst, x.Type);
                v0.AuxInt = -1L;
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                v.AddArg(x);
                return true;
            } 
            // match: (Select0 (MULTU x (MOVWconst [-1])))
            // cond:
            // result: (CMOVZ (ADDconst <x.Type> [-1] x) (MOVWconst [0]) x)
 
            // match: (Select0 (MULTU x (MOVWconst [-1])))
            // cond:
            // result: (CMOVZ (ADDconst <x.Type> [-1] x) (MOVWconst [0]) x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPSCMOVZ);
                v0 = b.NewValue0(v.Pos, OpMIPSADDconst, x.Type);
                v0.AuxInt = -1L;
                v0.AddArg(x);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                v.AddArg(x);
                return true;
            } 
            // match: (Select0 (MULTU (MOVWconst [c]) x))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SRLconst [32-log2(int64(uint32(c)))] x)
 
            // match: (Select0 (MULTU (MOVWconst [c]) x))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SRLconst [32-log2(int64(uint32(c)))] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0_0.AuxInt;
                x = v_0.Args[1L];
                if (!(isPowerOfTwo(int64(uint32(c)))))
                {
                    break;
                }
                v.reset(OpMIPSSRLconst);
                v.AuxInt = 32L - log2(int64(uint32(c)));
                v.AddArg(x);
                return true;
            } 
            // match: (Select0 (MULTU x (MOVWconst [c])))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SRLconst [32-log2(int64(uint32(c)))] x)
 
            // match: (Select0 (MULTU x (MOVWconst [c])))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SRLconst [32-log2(int64(uint32(c)))] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_1.AuxInt;
                if (!(isPowerOfTwo(int64(uint32(c)))))
                {
                    break;
                }
                v.reset(OpMIPSSRLconst);
                v.AuxInt = 32L - log2(int64(uint32(c)));
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpSelect0_10(ref Value v)
        { 
            // match: (Select0 (MULTU (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [(c*d)>>32])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0_0.AuxInt;
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = (c * d) >> (int)(32L);
                return true;
            } 
            // match: (Select0 (MULTU (MOVWconst [d]) (MOVWconst [c])))
            // cond:
            // result: (MOVWconst [(c*d)>>32])
 
            // match: (Select0 (MULTU (MOVWconst [d]) (MOVWconst [c])))
            // cond:
            // result: (MOVWconst [(c*d)>>32])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = (c * d) >> (int)(32L);
                return true;
            } 
            // match: (Select0 (DIV (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(c)%int32(d))])
 
            // match: (Select0 (DIV (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(c)%int32(d))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSDIV)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(c) % int32(d));
                return true;
            } 
            // match: (Select0 (DIVU (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)%uint32(d)))])
 
            // match: (Select0 (DIVU (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)%uint32(d)))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSDIVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(uint32(c) % uint32(d)));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpSelect1_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Select1 (Add32carry <t> x y))
            // cond:
            // result: (SGTU <typ.Bool> x (ADD <t.FieldType(0)> x y))
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpAdd32carry)
                {
                    break;
                }
                var t = v_0.Type;
                _ = v_0.Args[1L];
                var x = v_0.Args[0L];
                var y = v_0.Args[1L];
                v.reset(OpMIPSSGTU);
                v.Type = typ.Bool;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPSADD, t.FieldType(0L));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            } 
            // match: (Select1 (Sub32carry <t> x y))
            // cond:
            // result: (SGTU <typ.Bool> (SUB <t.FieldType(0)> x y) x)
 
            // match: (Select1 (Sub32carry <t> x y))
            // cond:
            // result: (SGTU <typ.Bool> (SUB <t.FieldType(0)> x y) x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpSub32carry)
                {
                    break;
                }
                t = v_0.Type;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                v.reset(OpMIPSSGTU);
                v.Type = typ.Bool;
                v0 = b.NewValue0(v.Pos, OpMIPSSUB, t.FieldType(0L));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULTU (MOVWconst [0]) _))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (Select1 (MULTU (MOVWconst [0]) _))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select1 (MULTU _ (MOVWconst [0])))
            // cond:
            // result: (MOVWconst [0])
 
            // match: (Select1 (MULTU _ (MOVWconst [0])))
            // cond:
            // result: (MOVWconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select1 (MULTU (MOVWconst [1]) x))
            // cond:
            // result: x
 
            // match: (Select1 (MULTU (MOVWconst [1]) x))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 1L)
                {
                    break;
                }
                x = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULTU x (MOVWconst [1])))
            // cond:
            // result: x
 
            // match: (Select1 (MULTU x (MOVWconst [1])))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULTU (MOVWconst [-1]) x))
            // cond:
            // result: (NEG <x.Type> x)
 
            // match: (Select1 (MULTU (MOVWconst [-1]) x))
            // cond:
            // result: (NEG <x.Type> x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != -1L)
                {
                    break;
                }
                x = v_0.Args[1L];
                v.reset(OpMIPSNEG);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULTU x (MOVWconst [-1])))
            // cond:
            // result: (NEG <x.Type> x)
 
            // match: (Select1 (MULTU x (MOVWconst [-1])))
            // cond:
            // result: (NEG <x.Type> x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPSNEG);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULTU (MOVWconst [c]) x))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
 
            // match: (Select1 (MULTU (MOVWconst [c]) x))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0_0.AuxInt;
                x = v_0.Args[1L];
                if (!(isPowerOfTwo(int64(uint32(c)))))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = log2(int64(uint32(c)));
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULTU x (MOVWconst [c])))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
 
            // match: (Select1 (MULTU x (MOVWconst [c])))
            // cond: isPowerOfTwo(int64(uint32(c)))
            // result: (SLLconst [log2(int64(uint32(c)))] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_1.AuxInt;
                if (!(isPowerOfTwo(int64(uint32(c)))))
                {
                    break;
                }
                v.reset(OpMIPSSLLconst);
                v.AuxInt = log2(int64(uint32(c)));
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpSelect1_10(ref Value v)
        { 
            // match: (Select1 (MULTU (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)*uint32(d)))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var c = v_0_0.AuxInt;
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                var d = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(uint32(c) * uint32(d)));
                return true;
            } 
            // match: (Select1 (MULTU (MOVWconst [d]) (MOVWconst [c])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)*uint32(d)))])
 
            // match: (Select1 (MULTU (MOVWconst [d]) (MOVWconst [c])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)*uint32(d)))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSMULTU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(uint32(c) * uint32(d)));
                return true;
            } 
            // match: (Select1 (DIV (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(c)/int32(d))])
 
            // match: (Select1 (DIV (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(c)/int32(d))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSDIV)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(c) / int32(d));
                return true;
            } 
            // match: (Select1 (DIVU (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)/uint32(d)))])
 
            // match: (Select1 (DIVU (MOVWconst [c]) (MOVWconst [d])))
            // cond:
            // result: (MOVWconst [int64(int32(uint32(c)/uint32(d)))])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPSDIVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPSMOVWconst)
                {
                    break;
                }
                d = v_0_1.AuxInt;
                v.reset(OpMIPSMOVWconst);
                v.AuxInt = int64(int32(uint32(c) / uint32(d)));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpSignExt16to32_0(ref Value v)
        { 
            // match: (SignExt16to32 x)
            // cond:
            // result: (MOVHreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVHreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSignExt8to16_0(ref Value v)
        { 
            // match: (SignExt8to16 x)
            // cond:
            // result: (MOVBreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVBreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSignExt8to32_0(ref Value v)
        { 
            // match: (SignExt8to32 x)
            // cond:
            // result: (MOVBreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVBreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSignmask_0(ref Value v)
        { 
            // match: (Signmask x)
            // cond:
            // result: (SRAconst x [31])
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSSRAconst);
                v.AuxInt = 31L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSlicemask_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Slicemask <t> x)
            // cond:
            // result: (SRAconst (NEG <t> x) [31])
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(OpMIPSSRAconst);
                v.AuxInt = 31L;
                var v0 = b.NewValue0(v.Pos, OpMIPSNEG, t);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSqrt_0(ref Value v)
        { 
            // match: (Sqrt x)
            // cond:
            // result: (SQRTD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSSQRTD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpStaticCall_0(ref Value v)
        { 
            // match: (StaticCall [argwid] {target} mem)
            // cond:
            // result: (CALLstatic [argwid] {target} mem)
            while (true)
            {
                var argwid = v.AuxInt;
                var target = v.Aux;
                var mem = v.Args[0L];
                v.reset(OpMIPSCALLstatic);
                v.AuxInt = argwid;
                v.Aux = target;
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpStore_0(ref Value v)
        { 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 1
            // result: (MOVBstore ptr val mem)
            while (true)
            {
                var t = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 1L))
                {
                    break;
                }
                v.reset(OpMIPSMOVBstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 2
            // result: (MOVHstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 2
            // result: (MOVHstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 2L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4 && !is32BitFloat(val.Type)
            // result: (MOVWstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4 && !is32BitFloat(val.Type)
            // result: (MOVWstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 4L && !is32BitFloat(val.Type)))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4 && is32BitFloat(val.Type)
            // result: (MOVFstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4 && is32BitFloat(val.Type)
            // result: (MOVFstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 4L && is32BitFloat(val.Type)))
                {
                    break;
                }
                v.reset(OpMIPSMOVFstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 8 && is64BitFloat(val.Type)
            // result: (MOVDstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 8 && is64BitFloat(val.Type)
            // result: (MOVDstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 8L && is64BitFloat(val.Type)))
                {
                    break;
                }
                v.reset(OpMIPSMOVDstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpSub16_0(ref Value v)
        { 
            // match: (Sub16 x y)
            // cond:
            // result: (SUB x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSUB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSub32_0(ref Value v)
        { 
            // match: (Sub32 x y)
            // cond:
            // result: (SUB x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSUB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSub32F_0(ref Value v)
        { 
            // match: (Sub32F x y)
            // cond:
            // result: (SUBF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSUBF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSub32withcarry_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Sub32withcarry <t> x y c)
            // cond:
            // result: (SUB (SUB <t> x y) c)
            while (true)
            {
                var t = v.Type;
                _ = v.Args[2L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                var c = v.Args[2L];
                v.reset(OpMIPSSUB);
                var v0 = b.NewValue0(v.Pos, OpMIPSSUB, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                v.AddArg(c);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSub64F_0(ref Value v)
        { 
            // match: (Sub64F x y)
            // cond:
            // result: (SUBD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSUBD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSub8_0(ref Value v)
        { 
            // match: (Sub8 x y)
            // cond:
            // result: (SUB x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSUB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpSubPtr_0(ref Value v)
        { 
            // match: (SubPtr x y)
            // cond:
            // result: (SUB x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSSUB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpTrunc16to8_0(ref Value v)
        { 
            // match: (Trunc16to8 x)
            // cond:
            // result: x
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpTrunc32to16_0(ref Value v)
        { 
            // match: (Trunc32to16 x)
            // cond:
            // result: x
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpTrunc32to8_0(ref Value v)
        { 
            // match: (Trunc32to8 x)
            // cond:
            // result: x
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpXor16_0(ref Value v)
        { 
            // match: (Xor16 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpXor32_0(ref Value v)
        { 
            // match: (Xor32 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpXor8_0(ref Value v)
        { 
            // match: (Xor8 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPSXOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpZero_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Zero [0] _ mem)
            // cond:
            // result: mem
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                _ = v.Args[1L];
                var mem = v.Args[1L];
                v.reset(OpCopy);
                v.Type = mem.Type;
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [1] ptr mem)
            // cond:
            // result: (MOVBstore ptr (MOVWconst [0]) mem)
 
            // match: (Zero [1] ptr mem)
            // cond:
            // result: (MOVBstore ptr (MOVWconst [0]) mem)
            while (true)
            {
                if (v.AuxInt != 1L)
                {
                    break;
                }
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPSMOVBstore);
                v.AddArg(ptr);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [2] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore ptr (MOVWconst [0]) mem)
 
            // match: (Zero [2] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore ptr (MOVWconst [0]) mem)
            while (true)
            {
                if (v.AuxInt != 2L)
                {
                    break;
                }
                var t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [2] ptr mem)
            // cond:
            // result: (MOVBstore [1] ptr (MOVWconst [0])         (MOVBstore [0] ptr (MOVWconst [0]) mem))
 
            // match: (Zero [2] ptr mem)
            // cond:
            // result: (MOVBstore [1] ptr (MOVWconst [0])         (MOVBstore [0] ptr (MOVWconst [0]) mem))
            while (true)
            {
                if (v.AuxInt != 2L)
                {
                    break;
                }
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = 1L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore ptr (MOVWconst [0]) mem)
 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore ptr (MOVWconst [0]) mem)
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] ptr (MOVWconst [0])         (MOVHstore [0] ptr (MOVWconst [0]) mem))
 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] ptr (MOVWconst [0])         (MOVHstore [0] ptr (MOVWconst [0]) mem))
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = 2L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [4] ptr mem)
            // cond:
            // result: (MOVBstore [3] ptr (MOVWconst [0])         (MOVBstore [2] ptr (MOVWconst [0])             (MOVBstore [1] ptr (MOVWconst [0])                 (MOVBstore [0] ptr (MOVWconst [0]) mem))))
 
            // match: (Zero [4] ptr mem)
            // cond:
            // result: (MOVBstore [3] ptr (MOVWconst [0])         (MOVBstore [2] ptr (MOVWconst [0])             (MOVBstore [1] ptr (MOVWconst [0])                 (MOVBstore [0] ptr (MOVWconst [0]) mem))))
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = 3L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v3.AuxInt = 1L;
                v3.AddArg(ptr);
                var v4 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v5.AuxInt = 0L;
                v5.AddArg(ptr);
                var v6 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v6.AuxInt = 0L;
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [3] ptr mem)
            // cond:
            // result: (MOVBstore [2] ptr (MOVWconst [0])         (MOVBstore [1] ptr (MOVWconst [0])             (MOVBstore [0] ptr (MOVWconst [0]) mem)))
 
            // match: (Zero [3] ptr mem)
            // cond:
            // result: (MOVBstore [2] ptr (MOVWconst [0])         (MOVBstore [1] ptr (MOVWconst [0])             (MOVBstore [0] ptr (MOVWconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 3L)
                {
                    break;
                }
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPSMOVBstore);
                v.AuxInt = 2L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v1.AuxInt = 1L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVBstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [6] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [4] ptr (MOVWconst [0])         (MOVHstore [2] ptr (MOVWconst [0])             (MOVHstore [0] ptr (MOVWconst [0]) mem)))
 
            // match: (Zero [6] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [4] ptr (MOVWconst [0])         (MOVHstore [2] ptr (MOVWconst [0])             (MOVHstore [0] ptr (MOVWconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 6L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVHstore);
                v.AuxInt = 4L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVHstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [4] ptr (MOVWconst [0])             (MOVWstore [0] ptr (MOVWconst [0]) mem))
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [4] ptr (MOVWconst [0])             (MOVWstore [0] ptr (MOVWconst [0]) mem))
            while (true)
            {
                if (v.AuxInt != 8L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = 4L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpZero_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Zero [12] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [8] ptr (MOVWconst [0])         (MOVWstore [4] ptr (MOVWconst [0])             (MOVWstore [0] ptr (MOVWconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 12L)
                {
                    break;
                }
                var t = v.Aux;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = 8L;
                v.AddArg(ptr);
                var v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(ptr);
                var v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                var v4 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [16] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [12] ptr (MOVWconst [0])         (MOVWstore [8] ptr (MOVWconst [0])             (MOVWstore [4] ptr (MOVWconst [0])                 (MOVWstore [0] ptr (MOVWconst [0]) mem))))
 
            // match: (Zero [16] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [12] ptr (MOVWconst [0])         (MOVWstore [8] ptr (MOVWconst [0])             (MOVWstore [4] ptr (MOVWconst [0])                 (MOVWstore [0] ptr (MOVWconst [0]) mem))))
            while (true)
            {
                if (v.AuxInt != 16L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 4L == 0L))
                {
                    break;
                }
                v.reset(OpMIPSMOVWstore);
                v.AuxInt = 12L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v1.AuxInt = 8L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v3.AuxInt = 4L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPSMOVWstore, types.TypeMem);
                v5.AuxInt = 0L;
                v5.AddArg(ptr);
                var v6 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v6.AuxInt = 0L;
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [s] {t} ptr mem)
            // cond: (s > 16  || t.(*types.Type).Alignment()%4 != 0)
            // result: (LoweredZero [t.(*types.Type).Alignment()]         ptr         (ADDconst <ptr.Type> ptr [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
 
            // match: (Zero [s] {t} ptr mem)
            // cond: (s > 16  || t.(*types.Type).Alignment()%4 != 0)
            // result: (LoweredZero [t.(*types.Type).Alignment()]         ptr         (ADDconst <ptr.Type> ptr [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
            while (true)
            {
                var s = v.AuxInt;
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(s > 16L || t._<ref types.Type>().Alignment() % 4L != 0L))
                {
                    break;
                }
                v.reset(OpMIPSLoweredZero);
                v.AuxInt = t._<ref types.Type>().Alignment();
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPSADDconst, ptr.Type);
                v0.AuxInt = s - moveSize(t._<ref types.Type>().Alignment(), config);
                v0.AddArg(ptr);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS_OpZeroExt16to32_0(ref Value v)
        { 
            // match: (ZeroExt16to32 x)
            // cond:
            // result: (MOVHUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVHUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpZeroExt8to16_0(ref Value v)
        { 
            // match: (ZeroExt8to16 x)
            // cond:
            // result: (MOVBUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVBUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpZeroExt8to32_0(ref Value v)
        { 
            // match: (ZeroExt8to32 x)
            // cond:
            // result: (MOVBUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSMOVBUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS_OpZeromask_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Zeromask x)
            // cond:
            // result: (NEG (SGTU x (MOVWconst [0])))
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPSNEG);
                var v0 = b.NewValue0(v.Pos, OpMIPSSGTU, typ.Bool);
                v0.AddArg(x);
                var v1 = b.NewValue0(v.Pos, OpMIPSMOVWconst, typ.UInt32);
                v1.AuxInt = 0L;
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteBlockMIPS(ref Block b)
        {
            var config = b.Func.Config;
            _ = config;
            var fe = b.Func.fe;
            _ = fe;
            var typ = ref config.Types;
            _ = typ;

            if (b.Kind == BlockMIPSEQ) 
                // match: (EQ (FPFlagTrue cmp) yes no)
                // cond:
                // result: (FPF cmp yes no)
                while (true)
                {
                    var v = b.Control;
                    if (v.Op != OpMIPSFPFlagTrue)
                    {
                        break;
                    }
                    var cmp = v.Args[0L];
                    b.Kind = BlockMIPSFPF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (FPFlagFalse cmp) yes no)
                // cond:
                // result: (FPT cmp yes no)
 
                // match: (EQ (FPFlagFalse cmp) yes no)
                // cond:
                // result: (FPT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSFPFlagFalse)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = BlockMIPSFPT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGT _ _)) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGT _ _)) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGT)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGTU _ _)) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTU _ _)) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTU)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGTconst _)) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTconst _)) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGTUconst _)) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTUconst _)) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTUconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGTzero _)) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTzero _)) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTzero)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (XORconst [1] cmp:(SGTUzero _)) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (EQ (XORconst [1] cmp:(SGTUzero _)) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTUzero)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (SGTUconst [1] x) yes no)
                // cond:
                // result: (NE x yes no)
 
                // match: (EQ (SGTUconst [1] x) yes no)
                // cond:
                // result: (NE x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTUconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    var x = v.Args[0L];
                    b.Kind = BlockMIPSNE;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (SGTUzero x) yes no)
                // cond:
                // result: (EQ x yes no)
 
                // match: (EQ (SGTUzero x) yes no)
                // cond:
                // result: (EQ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTUzero)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (SGTconst [0] x) yes no)
                // cond:
                // result: (GEZ x yes no)
 
                // match: (EQ (SGTconst [0] x) yes no)
                // cond:
                // result: (GEZ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 0L)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSGEZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (SGTzero x) yes no)
                // cond:
                // result: (LEZ x yes no)
 
                // match: (EQ (SGTzero x) yes no)
                // cond:
                // result: (LEZ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTzero)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSLEZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (MOVWconst [0]) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (EQ (MOVWconst [0]) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 0L)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (MOVWconst [c]) yes no)
                // cond: c != 0
                // result: (First nil no yes)
 
                // match: (EQ (MOVWconst [c]) yes no)
                // cond: c != 0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    var c = v.AuxInt;
                    if (!(c != 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPSGEZ) 
                // match: (GEZ (MOVWconst [c]) yes no)
                // cond: int32(c) >= 0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) >= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GEZ (MOVWconst [c]) yes no)
                // cond: int32(c) <  0
                // result: (First nil no yes)
 
                // match: (GEZ (MOVWconst [c]) yes no)
                // cond: int32(c) <  0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) < 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPSGTZ) 
                // match: (GTZ (MOVWconst [c]) yes no)
                // cond: int32(c) >  0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) > 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GTZ (MOVWconst [c]) yes no)
                // cond: int32(c) <= 0
                // result: (First nil no yes)
 
                // match: (GTZ (MOVWconst [c]) yes no)
                // cond: int32(c) <= 0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) <= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockIf) 
                // match: (If cond yes no)
                // cond:
                // result: (NE cond yes no)
                while (true)
                {
                    v = b.Control;
                    _ = v;
                    var cond = b.Control;
                    b.Kind = BlockMIPSNE;
                    b.SetControl(cond);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == BlockMIPSLEZ) 
                // match: (LEZ (MOVWconst [c]) yes no)
                // cond: int32(c) <= 0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) <= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LEZ (MOVWconst [c]) yes no)
                // cond: int32(c) >  0
                // result: (First nil no yes)
 
                // match: (LEZ (MOVWconst [c]) yes no)
                // cond: int32(c) >  0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) > 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPSLTZ) 
                // match: (LTZ (MOVWconst [c]) yes no)
                // cond: int32(c) <  0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) < 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LTZ (MOVWconst [c]) yes no)
                // cond: int32(c) >= 0
                // result: (First nil no yes)
 
                // match: (LTZ (MOVWconst [c]) yes no)
                // cond: int32(c) >= 0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(int32(c) >= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPSNE) 
                // match: (NE (FPFlagTrue cmp) yes no)
                // cond:
                // result: (FPT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSFPFlagTrue)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = BlockMIPSFPT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (FPFlagFalse cmp) yes no)
                // cond:
                // result: (FPF cmp yes no)
 
                // match: (NE (FPFlagFalse cmp) yes no)
                // cond:
                // result: (FPF cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSFPFlagFalse)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = BlockMIPSFPF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGT _ _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGT _ _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGT)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGTU _ _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTU _ _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTU)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGTconst _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTconst _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGTUconst _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTUconst _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTUconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGTzero _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTzero _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTzero)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (XORconst [1] cmp:(SGTUzero _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
 
                // match: (NE (XORconst [1] cmp:(SGTUzero _)) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSXORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPSSGTUzero)
                    {
                        break;
                    }
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (SGTUconst [1] x) yes no)
                // cond:
                // result: (EQ x yes no)
 
                // match: (NE (SGTUconst [1] x) yes no)
                // cond:
                // result: (EQ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTUconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSEQ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (SGTUzero x) yes no)
                // cond:
                // result: (NE x yes no)
 
                // match: (NE (SGTUzero x) yes no)
                // cond:
                // result: (NE x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTUzero)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSNE;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (SGTconst [0] x) yes no)
                // cond:
                // result: (LTZ x yes no)
 
                // match: (NE (SGTconst [0] x) yes no)
                // cond:
                // result: (LTZ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 0L)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSLTZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (SGTzero x) yes no)
                // cond:
                // result: (GTZ x yes no)
 
                // match: (NE (SGTzero x) yes no)
                // cond:
                // result: (GTZ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSSGTzero)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPSGTZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (MOVWconst [0]) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (NE (MOVWconst [0]) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 0L)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (NE (MOVWconst [c]) yes no)
                // cond: c != 0
                // result: (First nil yes no)
 
                // match: (NE (MOVWconst [c]) yes no)
                // cond: c != 0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPSMOVWconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c != 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                }
                        return false;
        }
    }
}}}}
