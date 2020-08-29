// Code generated from gen/MIPS64.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 August 29 09:14:20 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewriteMIPS64.go
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

        private static bool rewriteValueMIPS64(ref Value v)
        {

            if (v.Op == OpAdd16) 
                return rewriteValueMIPS64_OpAdd16_0(v);
            else if (v.Op == OpAdd32) 
                return rewriteValueMIPS64_OpAdd32_0(v);
            else if (v.Op == OpAdd32F) 
                return rewriteValueMIPS64_OpAdd32F_0(v);
            else if (v.Op == OpAdd64) 
                return rewriteValueMIPS64_OpAdd64_0(v);
            else if (v.Op == OpAdd64F) 
                return rewriteValueMIPS64_OpAdd64F_0(v);
            else if (v.Op == OpAdd8) 
                return rewriteValueMIPS64_OpAdd8_0(v);
            else if (v.Op == OpAddPtr) 
                return rewriteValueMIPS64_OpAddPtr_0(v);
            else if (v.Op == OpAddr) 
                return rewriteValueMIPS64_OpAddr_0(v);
            else if (v.Op == OpAnd16) 
                return rewriteValueMIPS64_OpAnd16_0(v);
            else if (v.Op == OpAnd32) 
                return rewriteValueMIPS64_OpAnd32_0(v);
            else if (v.Op == OpAnd64) 
                return rewriteValueMIPS64_OpAnd64_0(v);
            else if (v.Op == OpAnd8) 
                return rewriteValueMIPS64_OpAnd8_0(v);
            else if (v.Op == OpAndB) 
                return rewriteValueMIPS64_OpAndB_0(v);
            else if (v.Op == OpAtomicAdd32) 
                return rewriteValueMIPS64_OpAtomicAdd32_0(v);
            else if (v.Op == OpAtomicAdd64) 
                return rewriteValueMIPS64_OpAtomicAdd64_0(v);
            else if (v.Op == OpAtomicCompareAndSwap32) 
                return rewriteValueMIPS64_OpAtomicCompareAndSwap32_0(v);
            else if (v.Op == OpAtomicCompareAndSwap64) 
                return rewriteValueMIPS64_OpAtomicCompareAndSwap64_0(v);
            else if (v.Op == OpAtomicExchange32) 
                return rewriteValueMIPS64_OpAtomicExchange32_0(v);
            else if (v.Op == OpAtomicExchange64) 
                return rewriteValueMIPS64_OpAtomicExchange64_0(v);
            else if (v.Op == OpAtomicLoad32) 
                return rewriteValueMIPS64_OpAtomicLoad32_0(v);
            else if (v.Op == OpAtomicLoad64) 
                return rewriteValueMIPS64_OpAtomicLoad64_0(v);
            else if (v.Op == OpAtomicLoadPtr) 
                return rewriteValueMIPS64_OpAtomicLoadPtr_0(v);
            else if (v.Op == OpAtomicStore32) 
                return rewriteValueMIPS64_OpAtomicStore32_0(v);
            else if (v.Op == OpAtomicStore64) 
                return rewriteValueMIPS64_OpAtomicStore64_0(v);
            else if (v.Op == OpAtomicStorePtrNoWB) 
                return rewriteValueMIPS64_OpAtomicStorePtrNoWB_0(v);
            else if (v.Op == OpAvg64u) 
                return rewriteValueMIPS64_OpAvg64u_0(v);
            else if (v.Op == OpClosureCall) 
                return rewriteValueMIPS64_OpClosureCall_0(v);
            else if (v.Op == OpCom16) 
                return rewriteValueMIPS64_OpCom16_0(v);
            else if (v.Op == OpCom32) 
                return rewriteValueMIPS64_OpCom32_0(v);
            else if (v.Op == OpCom64) 
                return rewriteValueMIPS64_OpCom64_0(v);
            else if (v.Op == OpCom8) 
                return rewriteValueMIPS64_OpCom8_0(v);
            else if (v.Op == OpConst16) 
                return rewriteValueMIPS64_OpConst16_0(v);
            else if (v.Op == OpConst32) 
                return rewriteValueMIPS64_OpConst32_0(v);
            else if (v.Op == OpConst32F) 
                return rewriteValueMIPS64_OpConst32F_0(v);
            else if (v.Op == OpConst64) 
                return rewriteValueMIPS64_OpConst64_0(v);
            else if (v.Op == OpConst64F) 
                return rewriteValueMIPS64_OpConst64F_0(v);
            else if (v.Op == OpConst8) 
                return rewriteValueMIPS64_OpConst8_0(v);
            else if (v.Op == OpConstBool) 
                return rewriteValueMIPS64_OpConstBool_0(v);
            else if (v.Op == OpConstNil) 
                return rewriteValueMIPS64_OpConstNil_0(v);
            else if (v.Op == OpConvert) 
                return rewriteValueMIPS64_OpConvert_0(v);
            else if (v.Op == OpCvt32Fto32) 
                return rewriteValueMIPS64_OpCvt32Fto32_0(v);
            else if (v.Op == OpCvt32Fto64) 
                return rewriteValueMIPS64_OpCvt32Fto64_0(v);
            else if (v.Op == OpCvt32Fto64F) 
                return rewriteValueMIPS64_OpCvt32Fto64F_0(v);
            else if (v.Op == OpCvt32to32F) 
                return rewriteValueMIPS64_OpCvt32to32F_0(v);
            else if (v.Op == OpCvt32to64F) 
                return rewriteValueMIPS64_OpCvt32to64F_0(v);
            else if (v.Op == OpCvt64Fto32) 
                return rewriteValueMIPS64_OpCvt64Fto32_0(v);
            else if (v.Op == OpCvt64Fto32F) 
                return rewriteValueMIPS64_OpCvt64Fto32F_0(v);
            else if (v.Op == OpCvt64Fto64) 
                return rewriteValueMIPS64_OpCvt64Fto64_0(v);
            else if (v.Op == OpCvt64to32F) 
                return rewriteValueMIPS64_OpCvt64to32F_0(v);
            else if (v.Op == OpCvt64to64F) 
                return rewriteValueMIPS64_OpCvt64to64F_0(v);
            else if (v.Op == OpDiv16) 
                return rewriteValueMIPS64_OpDiv16_0(v);
            else if (v.Op == OpDiv16u) 
                return rewriteValueMIPS64_OpDiv16u_0(v);
            else if (v.Op == OpDiv32) 
                return rewriteValueMIPS64_OpDiv32_0(v);
            else if (v.Op == OpDiv32F) 
                return rewriteValueMIPS64_OpDiv32F_0(v);
            else if (v.Op == OpDiv32u) 
                return rewriteValueMIPS64_OpDiv32u_0(v);
            else if (v.Op == OpDiv64) 
                return rewriteValueMIPS64_OpDiv64_0(v);
            else if (v.Op == OpDiv64F) 
                return rewriteValueMIPS64_OpDiv64F_0(v);
            else if (v.Op == OpDiv64u) 
                return rewriteValueMIPS64_OpDiv64u_0(v);
            else if (v.Op == OpDiv8) 
                return rewriteValueMIPS64_OpDiv8_0(v);
            else if (v.Op == OpDiv8u) 
                return rewriteValueMIPS64_OpDiv8u_0(v);
            else if (v.Op == OpEq16) 
                return rewriteValueMIPS64_OpEq16_0(v);
            else if (v.Op == OpEq32) 
                return rewriteValueMIPS64_OpEq32_0(v);
            else if (v.Op == OpEq32F) 
                return rewriteValueMIPS64_OpEq32F_0(v);
            else if (v.Op == OpEq64) 
                return rewriteValueMIPS64_OpEq64_0(v);
            else if (v.Op == OpEq64F) 
                return rewriteValueMIPS64_OpEq64F_0(v);
            else if (v.Op == OpEq8) 
                return rewriteValueMIPS64_OpEq8_0(v);
            else if (v.Op == OpEqB) 
                return rewriteValueMIPS64_OpEqB_0(v);
            else if (v.Op == OpEqPtr) 
                return rewriteValueMIPS64_OpEqPtr_0(v);
            else if (v.Op == OpGeq16) 
                return rewriteValueMIPS64_OpGeq16_0(v);
            else if (v.Op == OpGeq16U) 
                return rewriteValueMIPS64_OpGeq16U_0(v);
            else if (v.Op == OpGeq32) 
                return rewriteValueMIPS64_OpGeq32_0(v);
            else if (v.Op == OpGeq32F) 
                return rewriteValueMIPS64_OpGeq32F_0(v);
            else if (v.Op == OpGeq32U) 
                return rewriteValueMIPS64_OpGeq32U_0(v);
            else if (v.Op == OpGeq64) 
                return rewriteValueMIPS64_OpGeq64_0(v);
            else if (v.Op == OpGeq64F) 
                return rewriteValueMIPS64_OpGeq64F_0(v);
            else if (v.Op == OpGeq64U) 
                return rewriteValueMIPS64_OpGeq64U_0(v);
            else if (v.Op == OpGeq8) 
                return rewriteValueMIPS64_OpGeq8_0(v);
            else if (v.Op == OpGeq8U) 
                return rewriteValueMIPS64_OpGeq8U_0(v);
            else if (v.Op == OpGetCallerSP) 
                return rewriteValueMIPS64_OpGetCallerSP_0(v);
            else if (v.Op == OpGetClosurePtr) 
                return rewriteValueMIPS64_OpGetClosurePtr_0(v);
            else if (v.Op == OpGreater16) 
                return rewriteValueMIPS64_OpGreater16_0(v);
            else if (v.Op == OpGreater16U) 
                return rewriteValueMIPS64_OpGreater16U_0(v);
            else if (v.Op == OpGreater32) 
                return rewriteValueMIPS64_OpGreater32_0(v);
            else if (v.Op == OpGreater32F) 
                return rewriteValueMIPS64_OpGreater32F_0(v);
            else if (v.Op == OpGreater32U) 
                return rewriteValueMIPS64_OpGreater32U_0(v);
            else if (v.Op == OpGreater64) 
                return rewriteValueMIPS64_OpGreater64_0(v);
            else if (v.Op == OpGreater64F) 
                return rewriteValueMIPS64_OpGreater64F_0(v);
            else if (v.Op == OpGreater64U) 
                return rewriteValueMIPS64_OpGreater64U_0(v);
            else if (v.Op == OpGreater8) 
                return rewriteValueMIPS64_OpGreater8_0(v);
            else if (v.Op == OpGreater8U) 
                return rewriteValueMIPS64_OpGreater8U_0(v);
            else if (v.Op == OpHmul32) 
                return rewriteValueMIPS64_OpHmul32_0(v);
            else if (v.Op == OpHmul32u) 
                return rewriteValueMIPS64_OpHmul32u_0(v);
            else if (v.Op == OpHmul64) 
                return rewriteValueMIPS64_OpHmul64_0(v);
            else if (v.Op == OpHmul64u) 
                return rewriteValueMIPS64_OpHmul64u_0(v);
            else if (v.Op == OpInterCall) 
                return rewriteValueMIPS64_OpInterCall_0(v);
            else if (v.Op == OpIsInBounds) 
                return rewriteValueMIPS64_OpIsInBounds_0(v);
            else if (v.Op == OpIsNonNil) 
                return rewriteValueMIPS64_OpIsNonNil_0(v);
            else if (v.Op == OpIsSliceInBounds) 
                return rewriteValueMIPS64_OpIsSliceInBounds_0(v);
            else if (v.Op == OpLeq16) 
                return rewriteValueMIPS64_OpLeq16_0(v);
            else if (v.Op == OpLeq16U) 
                return rewriteValueMIPS64_OpLeq16U_0(v);
            else if (v.Op == OpLeq32) 
                return rewriteValueMIPS64_OpLeq32_0(v);
            else if (v.Op == OpLeq32F) 
                return rewriteValueMIPS64_OpLeq32F_0(v);
            else if (v.Op == OpLeq32U) 
                return rewriteValueMIPS64_OpLeq32U_0(v);
            else if (v.Op == OpLeq64) 
                return rewriteValueMIPS64_OpLeq64_0(v);
            else if (v.Op == OpLeq64F) 
                return rewriteValueMIPS64_OpLeq64F_0(v);
            else if (v.Op == OpLeq64U) 
                return rewriteValueMIPS64_OpLeq64U_0(v);
            else if (v.Op == OpLeq8) 
                return rewriteValueMIPS64_OpLeq8_0(v);
            else if (v.Op == OpLeq8U) 
                return rewriteValueMIPS64_OpLeq8U_0(v);
            else if (v.Op == OpLess16) 
                return rewriteValueMIPS64_OpLess16_0(v);
            else if (v.Op == OpLess16U) 
                return rewriteValueMIPS64_OpLess16U_0(v);
            else if (v.Op == OpLess32) 
                return rewriteValueMIPS64_OpLess32_0(v);
            else if (v.Op == OpLess32F) 
                return rewriteValueMIPS64_OpLess32F_0(v);
            else if (v.Op == OpLess32U) 
                return rewriteValueMIPS64_OpLess32U_0(v);
            else if (v.Op == OpLess64) 
                return rewriteValueMIPS64_OpLess64_0(v);
            else if (v.Op == OpLess64F) 
                return rewriteValueMIPS64_OpLess64F_0(v);
            else if (v.Op == OpLess64U) 
                return rewriteValueMIPS64_OpLess64U_0(v);
            else if (v.Op == OpLess8) 
                return rewriteValueMIPS64_OpLess8_0(v);
            else if (v.Op == OpLess8U) 
                return rewriteValueMIPS64_OpLess8U_0(v);
            else if (v.Op == OpLoad) 
                return rewriteValueMIPS64_OpLoad_0(v);
            else if (v.Op == OpLsh16x16) 
                return rewriteValueMIPS64_OpLsh16x16_0(v);
            else if (v.Op == OpLsh16x32) 
                return rewriteValueMIPS64_OpLsh16x32_0(v);
            else if (v.Op == OpLsh16x64) 
                return rewriteValueMIPS64_OpLsh16x64_0(v);
            else if (v.Op == OpLsh16x8) 
                return rewriteValueMIPS64_OpLsh16x8_0(v);
            else if (v.Op == OpLsh32x16) 
                return rewriteValueMIPS64_OpLsh32x16_0(v);
            else if (v.Op == OpLsh32x32) 
                return rewriteValueMIPS64_OpLsh32x32_0(v);
            else if (v.Op == OpLsh32x64) 
                return rewriteValueMIPS64_OpLsh32x64_0(v);
            else if (v.Op == OpLsh32x8) 
                return rewriteValueMIPS64_OpLsh32x8_0(v);
            else if (v.Op == OpLsh64x16) 
                return rewriteValueMIPS64_OpLsh64x16_0(v);
            else if (v.Op == OpLsh64x32) 
                return rewriteValueMIPS64_OpLsh64x32_0(v);
            else if (v.Op == OpLsh64x64) 
                return rewriteValueMIPS64_OpLsh64x64_0(v);
            else if (v.Op == OpLsh64x8) 
                return rewriteValueMIPS64_OpLsh64x8_0(v);
            else if (v.Op == OpLsh8x16) 
                return rewriteValueMIPS64_OpLsh8x16_0(v);
            else if (v.Op == OpLsh8x32) 
                return rewriteValueMIPS64_OpLsh8x32_0(v);
            else if (v.Op == OpLsh8x64) 
                return rewriteValueMIPS64_OpLsh8x64_0(v);
            else if (v.Op == OpLsh8x8) 
                return rewriteValueMIPS64_OpLsh8x8_0(v);
            else if (v.Op == OpMIPS64ADDV) 
                return rewriteValueMIPS64_OpMIPS64ADDV_0(v);
            else if (v.Op == OpMIPS64ADDVconst) 
                return rewriteValueMIPS64_OpMIPS64ADDVconst_0(v);
            else if (v.Op == OpMIPS64AND) 
                return rewriteValueMIPS64_OpMIPS64AND_0(v);
            else if (v.Op == OpMIPS64ANDconst) 
                return rewriteValueMIPS64_OpMIPS64ANDconst_0(v);
            else if (v.Op == OpMIPS64LoweredAtomicAdd32) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd32_0(v);
            else if (v.Op == OpMIPS64LoweredAtomicAdd64) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd64_0(v);
            else if (v.Op == OpMIPS64LoweredAtomicStore32) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicStore32_0(v);
            else if (v.Op == OpMIPS64LoweredAtomicStore64) 
                return rewriteValueMIPS64_OpMIPS64LoweredAtomicStore64_0(v);
            else if (v.Op == OpMIPS64MOVBUload) 
                return rewriteValueMIPS64_OpMIPS64MOVBUload_0(v);
            else if (v.Op == OpMIPS64MOVBUreg) 
                return rewriteValueMIPS64_OpMIPS64MOVBUreg_0(v);
            else if (v.Op == OpMIPS64MOVBload) 
                return rewriteValueMIPS64_OpMIPS64MOVBload_0(v);
            else if (v.Op == OpMIPS64MOVBreg) 
                return rewriteValueMIPS64_OpMIPS64MOVBreg_0(v);
            else if (v.Op == OpMIPS64MOVBstore) 
                return rewriteValueMIPS64_OpMIPS64MOVBstore_0(v);
            else if (v.Op == OpMIPS64MOVBstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVBstorezero_0(v);
            else if (v.Op == OpMIPS64MOVDload) 
                return rewriteValueMIPS64_OpMIPS64MOVDload_0(v);
            else if (v.Op == OpMIPS64MOVDstore) 
                return rewriteValueMIPS64_OpMIPS64MOVDstore_0(v);
            else if (v.Op == OpMIPS64MOVFload) 
                return rewriteValueMIPS64_OpMIPS64MOVFload_0(v);
            else if (v.Op == OpMIPS64MOVFstore) 
                return rewriteValueMIPS64_OpMIPS64MOVFstore_0(v);
            else if (v.Op == OpMIPS64MOVHUload) 
                return rewriteValueMIPS64_OpMIPS64MOVHUload_0(v);
            else if (v.Op == OpMIPS64MOVHUreg) 
                return rewriteValueMIPS64_OpMIPS64MOVHUreg_0(v);
            else if (v.Op == OpMIPS64MOVHload) 
                return rewriteValueMIPS64_OpMIPS64MOVHload_0(v);
            else if (v.Op == OpMIPS64MOVHreg) 
                return rewriteValueMIPS64_OpMIPS64MOVHreg_0(v);
            else if (v.Op == OpMIPS64MOVHstore) 
                return rewriteValueMIPS64_OpMIPS64MOVHstore_0(v);
            else if (v.Op == OpMIPS64MOVHstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVHstorezero_0(v);
            else if (v.Op == OpMIPS64MOVVload) 
                return rewriteValueMIPS64_OpMIPS64MOVVload_0(v);
            else if (v.Op == OpMIPS64MOVVreg) 
                return rewriteValueMIPS64_OpMIPS64MOVVreg_0(v);
            else if (v.Op == OpMIPS64MOVVstore) 
                return rewriteValueMIPS64_OpMIPS64MOVVstore_0(v);
            else if (v.Op == OpMIPS64MOVVstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVVstorezero_0(v);
            else if (v.Op == OpMIPS64MOVWUload) 
                return rewriteValueMIPS64_OpMIPS64MOVWUload_0(v);
            else if (v.Op == OpMIPS64MOVWUreg) 
                return rewriteValueMIPS64_OpMIPS64MOVWUreg_0(v);
            else if (v.Op == OpMIPS64MOVWload) 
                return rewriteValueMIPS64_OpMIPS64MOVWload_0(v);
            else if (v.Op == OpMIPS64MOVWreg) 
                return rewriteValueMIPS64_OpMIPS64MOVWreg_0(v) || rewriteValueMIPS64_OpMIPS64MOVWreg_10(v);
            else if (v.Op == OpMIPS64MOVWstore) 
                return rewriteValueMIPS64_OpMIPS64MOVWstore_0(v);
            else if (v.Op == OpMIPS64MOVWstorezero) 
                return rewriteValueMIPS64_OpMIPS64MOVWstorezero_0(v);
            else if (v.Op == OpMIPS64NEGV) 
                return rewriteValueMIPS64_OpMIPS64NEGV_0(v);
            else if (v.Op == OpMIPS64NOR) 
                return rewriteValueMIPS64_OpMIPS64NOR_0(v);
            else if (v.Op == OpMIPS64NORconst) 
                return rewriteValueMIPS64_OpMIPS64NORconst_0(v);
            else if (v.Op == OpMIPS64OR) 
                return rewriteValueMIPS64_OpMIPS64OR_0(v);
            else if (v.Op == OpMIPS64ORconst) 
                return rewriteValueMIPS64_OpMIPS64ORconst_0(v);
            else if (v.Op == OpMIPS64SGT) 
                return rewriteValueMIPS64_OpMIPS64SGT_0(v);
            else if (v.Op == OpMIPS64SGTU) 
                return rewriteValueMIPS64_OpMIPS64SGTU_0(v);
            else if (v.Op == OpMIPS64SGTUconst) 
                return rewriteValueMIPS64_OpMIPS64SGTUconst_0(v);
            else if (v.Op == OpMIPS64SGTconst) 
                return rewriteValueMIPS64_OpMIPS64SGTconst_0(v) || rewriteValueMIPS64_OpMIPS64SGTconst_10(v);
            else if (v.Op == OpMIPS64SLLV) 
                return rewriteValueMIPS64_OpMIPS64SLLV_0(v);
            else if (v.Op == OpMIPS64SLLVconst) 
                return rewriteValueMIPS64_OpMIPS64SLLVconst_0(v);
            else if (v.Op == OpMIPS64SRAV) 
                return rewriteValueMIPS64_OpMIPS64SRAV_0(v);
            else if (v.Op == OpMIPS64SRAVconst) 
                return rewriteValueMIPS64_OpMIPS64SRAVconst_0(v);
            else if (v.Op == OpMIPS64SRLV) 
                return rewriteValueMIPS64_OpMIPS64SRLV_0(v);
            else if (v.Op == OpMIPS64SRLVconst) 
                return rewriteValueMIPS64_OpMIPS64SRLVconst_0(v);
            else if (v.Op == OpMIPS64SUBV) 
                return rewriteValueMIPS64_OpMIPS64SUBV_0(v);
            else if (v.Op == OpMIPS64SUBVconst) 
                return rewriteValueMIPS64_OpMIPS64SUBVconst_0(v);
            else if (v.Op == OpMIPS64XOR) 
                return rewriteValueMIPS64_OpMIPS64XOR_0(v);
            else if (v.Op == OpMIPS64XORconst) 
                return rewriteValueMIPS64_OpMIPS64XORconst_0(v);
            else if (v.Op == OpMod16) 
                return rewriteValueMIPS64_OpMod16_0(v);
            else if (v.Op == OpMod16u) 
                return rewriteValueMIPS64_OpMod16u_0(v);
            else if (v.Op == OpMod32) 
                return rewriteValueMIPS64_OpMod32_0(v);
            else if (v.Op == OpMod32u) 
                return rewriteValueMIPS64_OpMod32u_0(v);
            else if (v.Op == OpMod64) 
                return rewriteValueMIPS64_OpMod64_0(v);
            else if (v.Op == OpMod64u) 
                return rewriteValueMIPS64_OpMod64u_0(v);
            else if (v.Op == OpMod8) 
                return rewriteValueMIPS64_OpMod8_0(v);
            else if (v.Op == OpMod8u) 
                return rewriteValueMIPS64_OpMod8u_0(v);
            else if (v.Op == OpMove) 
                return rewriteValueMIPS64_OpMove_0(v) || rewriteValueMIPS64_OpMove_10(v);
            else if (v.Op == OpMul16) 
                return rewriteValueMIPS64_OpMul16_0(v);
            else if (v.Op == OpMul32) 
                return rewriteValueMIPS64_OpMul32_0(v);
            else if (v.Op == OpMul32F) 
                return rewriteValueMIPS64_OpMul32F_0(v);
            else if (v.Op == OpMul64) 
                return rewriteValueMIPS64_OpMul64_0(v);
            else if (v.Op == OpMul64F) 
                return rewriteValueMIPS64_OpMul64F_0(v);
            else if (v.Op == OpMul8) 
                return rewriteValueMIPS64_OpMul8_0(v);
            else if (v.Op == OpNeg16) 
                return rewriteValueMIPS64_OpNeg16_0(v);
            else if (v.Op == OpNeg32) 
                return rewriteValueMIPS64_OpNeg32_0(v);
            else if (v.Op == OpNeg32F) 
                return rewriteValueMIPS64_OpNeg32F_0(v);
            else if (v.Op == OpNeg64) 
                return rewriteValueMIPS64_OpNeg64_0(v);
            else if (v.Op == OpNeg64F) 
                return rewriteValueMIPS64_OpNeg64F_0(v);
            else if (v.Op == OpNeg8) 
                return rewriteValueMIPS64_OpNeg8_0(v);
            else if (v.Op == OpNeq16) 
                return rewriteValueMIPS64_OpNeq16_0(v);
            else if (v.Op == OpNeq32) 
                return rewriteValueMIPS64_OpNeq32_0(v);
            else if (v.Op == OpNeq32F) 
                return rewriteValueMIPS64_OpNeq32F_0(v);
            else if (v.Op == OpNeq64) 
                return rewriteValueMIPS64_OpNeq64_0(v);
            else if (v.Op == OpNeq64F) 
                return rewriteValueMIPS64_OpNeq64F_0(v);
            else if (v.Op == OpNeq8) 
                return rewriteValueMIPS64_OpNeq8_0(v);
            else if (v.Op == OpNeqB) 
                return rewriteValueMIPS64_OpNeqB_0(v);
            else if (v.Op == OpNeqPtr) 
                return rewriteValueMIPS64_OpNeqPtr_0(v);
            else if (v.Op == OpNilCheck) 
                return rewriteValueMIPS64_OpNilCheck_0(v);
            else if (v.Op == OpNot) 
                return rewriteValueMIPS64_OpNot_0(v);
            else if (v.Op == OpOffPtr) 
                return rewriteValueMIPS64_OpOffPtr_0(v);
            else if (v.Op == OpOr16) 
                return rewriteValueMIPS64_OpOr16_0(v);
            else if (v.Op == OpOr32) 
                return rewriteValueMIPS64_OpOr32_0(v);
            else if (v.Op == OpOr64) 
                return rewriteValueMIPS64_OpOr64_0(v);
            else if (v.Op == OpOr8) 
                return rewriteValueMIPS64_OpOr8_0(v);
            else if (v.Op == OpOrB) 
                return rewriteValueMIPS64_OpOrB_0(v);
            else if (v.Op == OpRound32F) 
                return rewriteValueMIPS64_OpRound32F_0(v);
            else if (v.Op == OpRound64F) 
                return rewriteValueMIPS64_OpRound64F_0(v);
            else if (v.Op == OpRsh16Ux16) 
                return rewriteValueMIPS64_OpRsh16Ux16_0(v);
            else if (v.Op == OpRsh16Ux32) 
                return rewriteValueMIPS64_OpRsh16Ux32_0(v);
            else if (v.Op == OpRsh16Ux64) 
                return rewriteValueMIPS64_OpRsh16Ux64_0(v);
            else if (v.Op == OpRsh16Ux8) 
                return rewriteValueMIPS64_OpRsh16Ux8_0(v);
            else if (v.Op == OpRsh16x16) 
                return rewriteValueMIPS64_OpRsh16x16_0(v);
            else if (v.Op == OpRsh16x32) 
                return rewriteValueMIPS64_OpRsh16x32_0(v);
            else if (v.Op == OpRsh16x64) 
                return rewriteValueMIPS64_OpRsh16x64_0(v);
            else if (v.Op == OpRsh16x8) 
                return rewriteValueMIPS64_OpRsh16x8_0(v);
            else if (v.Op == OpRsh32Ux16) 
                return rewriteValueMIPS64_OpRsh32Ux16_0(v);
            else if (v.Op == OpRsh32Ux32) 
                return rewriteValueMIPS64_OpRsh32Ux32_0(v);
            else if (v.Op == OpRsh32Ux64) 
                return rewriteValueMIPS64_OpRsh32Ux64_0(v);
            else if (v.Op == OpRsh32Ux8) 
                return rewriteValueMIPS64_OpRsh32Ux8_0(v);
            else if (v.Op == OpRsh32x16) 
                return rewriteValueMIPS64_OpRsh32x16_0(v);
            else if (v.Op == OpRsh32x32) 
                return rewriteValueMIPS64_OpRsh32x32_0(v);
            else if (v.Op == OpRsh32x64) 
                return rewriteValueMIPS64_OpRsh32x64_0(v);
            else if (v.Op == OpRsh32x8) 
                return rewriteValueMIPS64_OpRsh32x8_0(v);
            else if (v.Op == OpRsh64Ux16) 
                return rewriteValueMIPS64_OpRsh64Ux16_0(v);
            else if (v.Op == OpRsh64Ux32) 
                return rewriteValueMIPS64_OpRsh64Ux32_0(v);
            else if (v.Op == OpRsh64Ux64) 
                return rewriteValueMIPS64_OpRsh64Ux64_0(v);
            else if (v.Op == OpRsh64Ux8) 
                return rewriteValueMIPS64_OpRsh64Ux8_0(v);
            else if (v.Op == OpRsh64x16) 
                return rewriteValueMIPS64_OpRsh64x16_0(v);
            else if (v.Op == OpRsh64x32) 
                return rewriteValueMIPS64_OpRsh64x32_0(v);
            else if (v.Op == OpRsh64x64) 
                return rewriteValueMIPS64_OpRsh64x64_0(v);
            else if (v.Op == OpRsh64x8) 
                return rewriteValueMIPS64_OpRsh64x8_0(v);
            else if (v.Op == OpRsh8Ux16) 
                return rewriteValueMIPS64_OpRsh8Ux16_0(v);
            else if (v.Op == OpRsh8Ux32) 
                return rewriteValueMIPS64_OpRsh8Ux32_0(v);
            else if (v.Op == OpRsh8Ux64) 
                return rewriteValueMIPS64_OpRsh8Ux64_0(v);
            else if (v.Op == OpRsh8Ux8) 
                return rewriteValueMIPS64_OpRsh8Ux8_0(v);
            else if (v.Op == OpRsh8x16) 
                return rewriteValueMIPS64_OpRsh8x16_0(v);
            else if (v.Op == OpRsh8x32) 
                return rewriteValueMIPS64_OpRsh8x32_0(v);
            else if (v.Op == OpRsh8x64) 
                return rewriteValueMIPS64_OpRsh8x64_0(v);
            else if (v.Op == OpRsh8x8) 
                return rewriteValueMIPS64_OpRsh8x8_0(v);
            else if (v.Op == OpSelect0) 
                return rewriteValueMIPS64_OpSelect0_0(v);
            else if (v.Op == OpSelect1) 
                return rewriteValueMIPS64_OpSelect1_0(v) || rewriteValueMIPS64_OpSelect1_10(v) || rewriteValueMIPS64_OpSelect1_20(v);
            else if (v.Op == OpSignExt16to32) 
                return rewriteValueMIPS64_OpSignExt16to32_0(v);
            else if (v.Op == OpSignExt16to64) 
                return rewriteValueMIPS64_OpSignExt16to64_0(v);
            else if (v.Op == OpSignExt32to64) 
                return rewriteValueMIPS64_OpSignExt32to64_0(v);
            else if (v.Op == OpSignExt8to16) 
                return rewriteValueMIPS64_OpSignExt8to16_0(v);
            else if (v.Op == OpSignExt8to32) 
                return rewriteValueMIPS64_OpSignExt8to32_0(v);
            else if (v.Op == OpSignExt8to64) 
                return rewriteValueMIPS64_OpSignExt8to64_0(v);
            else if (v.Op == OpSlicemask) 
                return rewriteValueMIPS64_OpSlicemask_0(v);
            else if (v.Op == OpStaticCall) 
                return rewriteValueMIPS64_OpStaticCall_0(v);
            else if (v.Op == OpStore) 
                return rewriteValueMIPS64_OpStore_0(v);
            else if (v.Op == OpSub16) 
                return rewriteValueMIPS64_OpSub16_0(v);
            else if (v.Op == OpSub32) 
                return rewriteValueMIPS64_OpSub32_0(v);
            else if (v.Op == OpSub32F) 
                return rewriteValueMIPS64_OpSub32F_0(v);
            else if (v.Op == OpSub64) 
                return rewriteValueMIPS64_OpSub64_0(v);
            else if (v.Op == OpSub64F) 
                return rewriteValueMIPS64_OpSub64F_0(v);
            else if (v.Op == OpSub8) 
                return rewriteValueMIPS64_OpSub8_0(v);
            else if (v.Op == OpSubPtr) 
                return rewriteValueMIPS64_OpSubPtr_0(v);
            else if (v.Op == OpTrunc16to8) 
                return rewriteValueMIPS64_OpTrunc16to8_0(v);
            else if (v.Op == OpTrunc32to16) 
                return rewriteValueMIPS64_OpTrunc32to16_0(v);
            else if (v.Op == OpTrunc32to8) 
                return rewriteValueMIPS64_OpTrunc32to8_0(v);
            else if (v.Op == OpTrunc64to16) 
                return rewriteValueMIPS64_OpTrunc64to16_0(v);
            else if (v.Op == OpTrunc64to32) 
                return rewriteValueMIPS64_OpTrunc64to32_0(v);
            else if (v.Op == OpTrunc64to8) 
                return rewriteValueMIPS64_OpTrunc64to8_0(v);
            else if (v.Op == OpXor16) 
                return rewriteValueMIPS64_OpXor16_0(v);
            else if (v.Op == OpXor32) 
                return rewriteValueMIPS64_OpXor32_0(v);
            else if (v.Op == OpXor64) 
                return rewriteValueMIPS64_OpXor64_0(v);
            else if (v.Op == OpXor8) 
                return rewriteValueMIPS64_OpXor8_0(v);
            else if (v.Op == OpZero) 
                return rewriteValueMIPS64_OpZero_0(v) || rewriteValueMIPS64_OpZero_10(v);
            else if (v.Op == OpZeroExt16to32) 
                return rewriteValueMIPS64_OpZeroExt16to32_0(v);
            else if (v.Op == OpZeroExt16to64) 
                return rewriteValueMIPS64_OpZeroExt16to64_0(v);
            else if (v.Op == OpZeroExt32to64) 
                return rewriteValueMIPS64_OpZeroExt32to64_0(v);
            else if (v.Op == OpZeroExt8to16) 
                return rewriteValueMIPS64_OpZeroExt8to16_0(v);
            else if (v.Op == OpZeroExt8to32) 
                return rewriteValueMIPS64_OpZeroExt8to32_0(v);
            else if (v.Op == OpZeroExt8to64) 
                return rewriteValueMIPS64_OpZeroExt8to64_0(v);
                        return false;
        }
        private static bool rewriteValueMIPS64_OpAdd16_0(ref Value v)
        { 
            // match: (Add16 x y)
            // cond:
            // result: (ADDV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAdd32_0(ref Value v)
        { 
            // match: (Add32 x y)
            // cond:
            // result: (ADDV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAdd32F_0(ref Value v)
        { 
            // match: (Add32F x y)
            // cond:
            // result: (ADDF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAdd64_0(ref Value v)
        { 
            // match: (Add64 x y)
            // cond:
            // result: (ADDV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAdd64F_0(ref Value v)
        { 
            // match: (Add64F x y)
            // cond:
            // result: (ADDD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAdd8_0(ref Value v)
        { 
            // match: (Add8 x y)
            // cond:
            // result: (ADDV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAddPtr_0(ref Value v)
        { 
            // match: (AddPtr x y)
            // cond:
            // result: (ADDV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAddr_0(ref Value v)
        { 
            // match: (Addr {sym} base)
            // cond:
            // result: (MOVVaddr {sym} base)
            while (true)
            {
                var sym = v.Aux;
                var @base = v.Args[0L];
                v.reset(OpMIPS64MOVVaddr);
                v.Aux = sym;
                v.AddArg(base);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAnd16_0(ref Value v)
        { 
            // match: (And16 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAnd32_0(ref Value v)
        { 
            // match: (And32 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAnd64_0(ref Value v)
        { 
            // match: (And64 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAnd8_0(ref Value v)
        { 
            // match: (And8 x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAndB_0(ref Value v)
        { 
            // match: (AndB x y)
            // cond:
            // result: (AND x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicAdd32_0(ref Value v)
        { 
            // match: (AtomicAdd32 ptr val mem)
            // cond:
            // result: (LoweredAtomicAdd32 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicAdd32);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicAdd64_0(ref Value v)
        { 
            // match: (AtomicAdd64 ptr val mem)
            // cond:
            // result: (LoweredAtomicAdd64 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicAdd64);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicCompareAndSwap32_0(ref Value v)
        { 
            // match: (AtomicCompareAndSwap32 ptr old new_ mem)
            // cond:
            // result: (LoweredAtomicCas32 ptr old new_ mem)
            while (true)
            {
                _ = v.Args[3L];
                var ptr = v.Args[0L];
                var old = v.Args[1L];
                var new_ = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(OpMIPS64LoweredAtomicCas32);
                v.AddArg(ptr);
                v.AddArg(old);
                v.AddArg(new_);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicCompareAndSwap64_0(ref Value v)
        { 
            // match: (AtomicCompareAndSwap64 ptr old new_ mem)
            // cond:
            // result: (LoweredAtomicCas64 ptr old new_ mem)
            while (true)
            {
                _ = v.Args[3L];
                var ptr = v.Args[0L];
                var old = v.Args[1L];
                var new_ = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(OpMIPS64LoweredAtomicCas64);
                v.AddArg(ptr);
                v.AddArg(old);
                v.AddArg(new_);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicExchange32_0(ref Value v)
        { 
            // match: (AtomicExchange32 ptr val mem)
            // cond:
            // result: (LoweredAtomicExchange32 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicExchange32);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicExchange64_0(ref Value v)
        { 
            // match: (AtomicExchange64 ptr val mem)
            // cond:
            // result: (LoweredAtomicExchange64 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicExchange64);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicLoad32_0(ref Value v)
        { 
            // match: (AtomicLoad32 ptr mem)
            // cond:
            // result: (LoweredAtomicLoad32 ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPS64LoweredAtomicLoad32);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicLoad64_0(ref Value v)
        { 
            // match: (AtomicLoad64 ptr mem)
            // cond:
            // result: (LoweredAtomicLoad64 ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPS64LoweredAtomicLoad64);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicLoadPtr_0(ref Value v)
        { 
            // match: (AtomicLoadPtr ptr mem)
            // cond:
            // result: (LoweredAtomicLoad64 ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPS64LoweredAtomicLoad64);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicStore32_0(ref Value v)
        { 
            // match: (AtomicStore32 ptr val mem)
            // cond:
            // result: (LoweredAtomicStore32 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicStore32);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicStore64_0(ref Value v)
        { 
            // match: (AtomicStore64 ptr val mem)
            // cond:
            // result: (LoweredAtomicStore64 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicStore64);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAtomicStorePtrNoWB_0(ref Value v)
        { 
            // match: (AtomicStorePtrNoWB ptr val mem)
            // cond:
            // result: (LoweredAtomicStore64 ptr val mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicStore64);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpAvg64u_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Avg64u <t> x y)
            // cond:
            // result: (ADDV (SRLVconst <t> (SUBV <t> x y) [1]) y)
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64ADDV);
                var v0 = b.NewValue0(v.Pos, OpMIPS64SRLVconst, t);
                v0.AuxInt = 1L;
                var v1 = b.NewValue0(v.Pos, OpMIPS64SUBV, t);
                v1.AddArg(x);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpClosureCall_0(ref Value v)
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
                v.reset(OpMIPS64CALLclosure);
                v.AuxInt = argwid;
                v.AddArg(entry);
                v.AddArg(closure);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCom16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Com16 x)
            // cond:
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCom32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Com32 x)
            // cond:
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCom64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Com64 x)
            // cond:
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCom8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Com8 x)
            // cond:
            // result: (NOR (MOVVconst [0]) x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConst16_0(ref Value v)
        { 
            // match: (Const16 [val])
            // cond:
            // result: (MOVVconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConst32_0(ref Value v)
        { 
            // match: (Const32 [val])
            // cond:
            // result: (MOVVconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConst32F_0(ref Value v)
        { 
            // match: (Const32F [val])
            // cond:
            // result: (MOVFconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPS64MOVFconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConst64_0(ref Value v)
        { 
            // match: (Const64 [val])
            // cond:
            // result: (MOVVconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConst64F_0(ref Value v)
        { 
            // match: (Const64F [val])
            // cond:
            // result: (MOVDconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPS64MOVDconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConst8_0(ref Value v)
        { 
            // match: (Const8 [val])
            // cond:
            // result: (MOVVconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConstBool_0(ref Value v)
        { 
            // match: (ConstBool [b])
            // cond:
            // result: (MOVVconst [b])
            while (true)
            {
                var b = v.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = b;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConstNil_0(ref Value v)
        { 
            // match: (ConstNil)
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpConvert_0(ref Value v)
        { 
            // match: (Convert x mem)
            // cond:
            // result: (MOVVconvert x mem)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPS64MOVVconvert);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt32Fto32_0(ref Value v)
        { 
            // match: (Cvt32Fto32 x)
            // cond:
            // result: (TRUNCFW x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64TRUNCFW);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt32Fto64_0(ref Value v)
        { 
            // match: (Cvt32Fto64 x)
            // cond:
            // result: (TRUNCFV x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64TRUNCFV);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt32Fto64F_0(ref Value v)
        { 
            // match: (Cvt32Fto64F x)
            // cond:
            // result: (MOVFD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVFD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt32to32F_0(ref Value v)
        { 
            // match: (Cvt32to32F x)
            // cond:
            // result: (MOVWF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVWF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt32to64F_0(ref Value v)
        { 
            // match: (Cvt32to64F x)
            // cond:
            // result: (MOVWD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVWD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt64Fto32_0(ref Value v)
        { 
            // match: (Cvt64Fto32 x)
            // cond:
            // result: (TRUNCDW x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64TRUNCDW);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt64Fto32F_0(ref Value v)
        { 
            // match: (Cvt64Fto32F x)
            // cond:
            // result: (MOVDF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVDF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt64Fto64_0(ref Value v)
        { 
            // match: (Cvt64Fto64 x)
            // cond:
            // result: (TRUNCDV x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64TRUNCDV);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt64to32F_0(ref Value v)
        { 
            // match: (Cvt64to32F x)
            // cond:
            // result: (MOVVF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVVF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpCvt64to64F_0(ref Value v)
        { 
            // match: (Cvt64to64F x)
            // cond:
            // result: (MOVVD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVVD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div16 x y)
            // cond:
            // result: (Select1 (DIVV (SignExt16to64 x) (SignExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv16u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div16u x y)
            // cond:
            // result: (Select1 (DIVVU (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div32 x y)
            // cond:
            // result: (Select1 (DIVV (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv32F_0(ref Value v)
        { 
            // match: (Div32F x y)
            // cond:
            // result: (DIVF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64DIVF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div32u x y)
            // cond:
            // result: (Select1 (DIVVU (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div64 x y)
            // cond:
            // result: (Select1 (DIVV x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv64F_0(ref Value v)
        { 
            // match: (Div64F x y)
            // cond:
            // result: (DIVD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64DIVD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv64u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div64u x y)
            // cond:
            // result: (Select1 (DIVVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div8 x y)
            // cond:
            // result: (Select1 (DIVV (SignExt8to64 x) (SignExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpDiv8u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div8u x y)
            // cond:
            // result: (Select1 (DIVVU (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq16 x y)
            // cond:
            // result: (SGTU (MOVVconst [1]) (XOR (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq32 x y)
            // cond:
            // result: (SGTU (MOVVconst [1]) (XOR (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEq32F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq64 x y)
            // cond:
            // result: (SGTU (MOVVconst [1]) (XOR x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v1.AddArg(x);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEq64F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Eq8 x y)
            // cond:
            // result: (SGTU (MOVVconst [1]) (XOR (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEqB_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (EqB x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (XOR <typ.Bool> x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.Bool);
                v1.AddArg(x);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpEqPtr_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (EqPtr x y)
            // cond:
            // result: (SGTU (MOVVconst [1]) (XOR x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v1.AddArg(x);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq16 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT (SignExt16to64 y) (SignExt16to64 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v3.AddArg(x);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq16U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt16to64 y) (ZeroExt16to64 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(x);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq32 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT (SignExt32to64 y) (SignExt32to64 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v3.AddArg(x);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq32F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGEF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq32U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt32to64 y) (ZeroExt32to64 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(x);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq64 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                v1.AddArg(y);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq64F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGED, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq64U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq64U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v1.AddArg(y);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq8 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT (SignExt8to64 y) (SignExt8to64 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v3.AddArg(x);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGeq8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Geq8U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt8to64 y) (ZeroExt8to64 x)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(x);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGetCallerSP_0(ref Value v)
        { 
            // match: (GetCallerSP)
            // cond:
            // result: (LoweredGetCallerSP)
            while (true)
            {
                v.reset(OpMIPS64LoweredGetCallerSP);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGetClosurePtr_0(ref Value v)
        { 
            // match: (GetClosurePtr)
            // cond:
            // result: (LoweredGetClosurePtr)
            while (true)
            {
                v.reset(OpMIPS64LoweredGetClosurePtr);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater16 x y)
            // cond:
            // result: (SGT (SignExt16to64 x) (SignExt16to64 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater16U x y)
            // cond:
            // result: (SGTU (ZeroExt16to64 x) (ZeroExt16to64 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater32 x y)
            // cond:
            // result: (SGT (SignExt32to64 x) (SignExt32to64 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater32F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGTF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater32U x y)
            // cond:
            // result: (SGTU (ZeroExt32to64 x) (ZeroExt32to64 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater64_0(ref Value v)
        { 
            // match: (Greater64 x y)
            // cond:
            // result: (SGT x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater64F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGTD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater64U_0(ref Value v)
        { 
            // match: (Greater64U x y)
            // cond:
            // result: (SGTU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater8 x y)
            // cond:
            // result: (SGT (SignExt8to64 x) (SignExt8to64 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpGreater8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Greater8U x y)
            // cond:
            // result: (SGTU (ZeroExt8to64 x) (ZeroExt8to64 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpHmul32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Hmul32 x y)
            // cond:
            // result: (SRAVconst (Select1 <typ.Int64> (MULV (SignExt32to64 x) (SignExt32to64 y))) [32])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = 32L;
                var v0 = b.NewValue0(v.Pos, OpSelect1, typ.Int64);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MULV, types.NewTuple(typ.Int64, typ.Int64));
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpHmul32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Hmul32u x y)
            // cond:
            // result: (SRLVconst (Select1 <typ.UInt64> (MULVU (ZeroExt32to64 x) (ZeroExt32to64 y))) [32])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRLVconst);
                v.AuxInt = 32L;
                var v0 = b.NewValue0(v.Pos, OpSelect1, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpHmul64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Hmul64 x y)
            // cond:
            // result: (Select0 (MULV x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULV, types.NewTuple(typ.Int64, typ.Int64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpHmul64u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Hmul64u x y)
            // cond:
            // result: (Select0 (MULVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpInterCall_0(ref Value v)
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
                v.reset(OpMIPS64CALLinter);
                v.AuxInt = argwid;
                v.AddArg(entry);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpIsInBounds_0(ref Value v)
        { 
            // match: (IsInBounds idx len)
            // cond:
            // result: (SGTU len idx)
            while (true)
            {
                _ = v.Args[1L];
                var idx = v.Args[0L];
                var len = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                v.AddArg(len);
                v.AddArg(idx);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpIsNonNil_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (IsNonNil ptr)
            // cond:
            // result: (SGTU ptr (MOVVconst [0]))
            while (true)
            {
                var ptr = v.Args[0L];
                v.reset(OpMIPS64SGTU);
                v.AddArg(ptr);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpIsSliceInBounds_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (IsSliceInBounds idx len)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU idx len))
            while (true)
            {
                _ = v.Args[1L];
                var idx = v.Args[0L];
                var len = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v1.AddArg(idx);
                v1.AddArg(len);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq16 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT (SignExt16to64 x) (SignExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq16U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq32 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq32F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGEF, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq32U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq64 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                v1.AddArg(x);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq64F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGED, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq64U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq64U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v1.AddArg(x);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq8 x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGT (SignExt8to64 x) (SignExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGT, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLeq8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Leq8U x y)
            // cond:
            // result: (XOR (MOVVconst [1]) (SGTU (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 1L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(x);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less16 x y)
            // cond:
            // result: (SGT (SignExt16to64 y) (SignExt16to64 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less16U x y)
            // cond:
            // result: (SGTU (ZeroExt16to64 y) (ZeroExt16to64 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less32 x y)
            // cond:
            // result: (SGT (SignExt32to64 y) (SignExt32to64 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess32F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGTF, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less32U x y)
            // cond:
            // result: (SGTU (ZeroExt32to64 y) (ZeroExt32to64 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess64_0(ref Value v)
        { 
            // match: (Less64 x y)
            // cond:
            // result: (SGT y x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess64F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagTrue);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPGTD, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess64U_0(ref Value v)
        { 
            // match: (Less64U x y)
            // cond:
            // result: (SGTU y x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less8 x y)
            // cond:
            // result: (SGT (SignExt8to64 y) (SignExt8to64 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGT);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLess8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Less8U x y)
            // cond:
            // result: (SGTU (ZeroExt8to64 y) (ZeroExt8to64 x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLoad_0(ref Value v)
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
                v.reset(OpMIPS64MOVBUload);
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
                v.reset(OpMIPS64MOVBload);
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
                v.reset(OpMIPS64MOVBUload);
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
                v.reset(OpMIPS64MOVHload);
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
                v.reset(OpMIPS64MOVHUload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is32BitInt(t) && isSigned(t)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is32BitInt(t) && !isSigned(t)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWUload);
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is64BitInt(t) || isPtr(t)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVload);
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
                v.reset(OpMIPS64MOVFload);
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
                v.reset(OpMIPS64MOVDload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpLsh16x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh16x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh16x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg(x);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh16x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh16x8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SLLV <t> x (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh32x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh32x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh32x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg(x);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh32x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh32x8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SLLV <t> x (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh64x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh64x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh64x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg(x);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh64x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh64x8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SLLV <t> x (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh8x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SLLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh8x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SLLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh8x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SLLV <t> x y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v3.AddArg(x);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpLsh8x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Lsh8x8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SLLV <t> x (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SLLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMIPS64ADDV_0(ref Value v)
        { 
            // match: (ADDV x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (ADDVconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDV (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (ADDVconst [c] x)
 
            // match: (ADDV (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (ADDVconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDV x (NEGV y))
            // cond:
            // result: (SUBV x y)
 
            // match: (ADDV x (NEGV y))
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64NEGV)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDV (NEGV y) x)
            // cond:
            // result: (SUBV x y)
 
            // match: (ADDV (NEGV y) x)
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64NEGV)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64ADDVconst_0(ref Value v)
        { 
            // match: (ADDVconst [off1] (MOVVaddr [off2] {sym} ptr))
            // cond:
            // result: (MOVVaddr [off1+off2] {sym} ptr)
            while (true)
            {
                var off1 = v.AuxInt;
                var v_0 = v.Args[0L];
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
            // cond:
            // result: x
 
            // match: (ADDVconst [0] x)
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
            // match: (ADDVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c+d])
 
            // match: (ADDVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c+d])
            while (true)
            {
                var c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = c + d;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = c + d;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64SUBVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(is32Bit(c - d)))
                {
                    break;
                }
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = c - d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64AND_0(ref Value v)
        { 
            // match: (AND x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (ANDconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ANDconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (AND (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (ANDconst [c] x)
 
            // match: (AND (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (ANDconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ANDconst);
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

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64ANDconst_0(ref Value v)
        { 
            // match: (ANDconst [0] _)
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
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
            // match: (ANDconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c&d])
 
            // match: (ANDconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c&d])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
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
                if (v_0.Op != OpMIPS64ANDconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(OpMIPS64ANDconst);
                v.AuxInt = c & d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd32_0(ref Value v)
        { 
            // match: (LoweredAtomicAdd32 ptr (MOVVconst [c]) mem)
            // cond: is32Bit(c)
            // result: (LoweredAtomicAddconst32 [c] ptr mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                var mem = v.Args[2L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64LoweredAtomicAddconst32);
                v.AuxInt = c;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicAdd64_0(ref Value v)
        { 
            // match: (LoweredAtomicAdd64 ptr (MOVVconst [c]) mem)
            // cond: is32Bit(c)
            // result: (LoweredAtomicAddconst64 [c] ptr mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                var mem = v.Args[2L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64LoweredAtomicAddconst64);
                v.AuxInt = c;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicStore32_0(ref Value v)
        { 
            // match: (LoweredAtomicStore32 ptr (MOVVconst [0]) mem)
            // cond:
            // result: (LoweredAtomicStorezero32 ptr mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicStorezero32);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64LoweredAtomicStore64_0(ref Value v)
        { 
            // match: (LoweredAtomicStore64 ptr (MOVVconst [0]) mem)
            // cond:
            // result: (LoweredAtomicStorezero64 ptr mem)
            while (true)
            {
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                var mem = v.Args[2L];
                v.reset(OpMIPS64LoweredAtomicStorezero64);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBUload_0(ref Value v)
        { 
            // match: (MOVBUload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBUreg_0(ref Value v)
        { 
            // match: (MOVBUreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVBUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBUreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(uint8(c))])
 
            // match: (MOVBUreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(uint8(c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(uint8(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBload_0(ref Value v)
        { 
            // match: (MOVBload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBreg_0(ref Value v)
        { 
            // match: (MOVBreg x:(MOVBload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBreg x:(MOVBreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVBreg x:(MOVBreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(int8(c))])
 
            // match: (MOVBreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(int8(c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(int8(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBstore_0(ref Value v)
        { 
            // match: (MOVBstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
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
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVBstorezero [off] {sym} ptr mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVBstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstorezero);
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
                if (v_1.Op != OpMIPS64MOVBreg)
                {
                    break;
                }
                var x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
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
                if (v_1.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
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
                if (v_1.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
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
                if (v_1.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
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
                if (v_1.Op != OpMIPS64MOVWreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVWUreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVWUreg x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVBstorezero_0(ref Value v)
        { 
            // match: (MOVBstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVBstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVDload_0(ref Value v)
        { 
            // match: (MOVDload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVDload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVDload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVDload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVDstore_0(ref Value v)
        { 
            // match: (MOVDstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVDstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVDstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
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
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVDstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVFload_0(ref Value v)
        { 
            // match: (MOVFload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVFload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVFload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVFload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVFstore_0(ref Value v)
        { 
            // match: (MOVFstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVFstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVFstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
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
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVFstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHUload_0(ref Value v)
        { 
            // match: (MOVHUload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHUreg_0(ref Value v)
        { 
            // match: (MOVHUreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHUreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg x:(MOVHUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHUreg x:(MOVHUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHUreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(uint16(c))])
 
            // match: (MOVHUreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(uint16(c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(uint16(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHload_0(ref Value v)
        { 
            // match: (MOVHload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHreg_0(ref Value v)
        { 
            // match: (MOVHreg x:(MOVBload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVHload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVHload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVBreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVBreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg x:(MOVHreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVHreg x:(MOVHreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVHreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(int16(c))])
 
            // match: (MOVHreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(int16(c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(int16(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHstore_0(ref Value v)
        { 
            // match: (MOVHstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
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
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVHstorezero [off] {sym} ptr mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVHstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVHstorezero);
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
                if (v_1.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                var x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVHstore);
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
                if (v_1.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVHstore);
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
                if (v_1.Op != OpMIPS64MOVWreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVHstore [off] {sym} ptr (MOVWUreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
 
            // match: (MOVHstore [off] {sym} ptr (MOVWUreg x) mem)
            // cond:
            // result: (MOVHstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVHstorezero_0(ref Value v)
        { 
            // match: (MOVHstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVHstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVload_0(ref Value v)
        { 
            // match: (MOVVload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVVload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVreg_0(ref Value v)
        { 
            // match: (MOVVreg x)
            // cond: x.Uses == 1
            // result: (MOVVnop x)
            while (true)
            {
                var x = v.Args[0L];
                if (!(x.Uses == 1L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVnop);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVVreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [c])
 
            // match: (MOVVreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [c])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = c;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVstore_0(ref Value v)
        { 
            // match: (MOVVstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVVstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
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
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVVstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVVstorezero [off] {sym} ptr mem)
 
            // match: (MOVVstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVVstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVVstorezero);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVVstorezero_0(ref Value v)
        { 
            // match: (MOVVstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVVstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWUload_0(ref Value v)
        { 
            // match: (MOVWUload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWUload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWUload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWUload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWUreg_0(ref Value v)
        { 
            // match: (MOVWUreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWUreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWUreg x:(MOVWUload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVWUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVWUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWUreg x:(MOVHUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVHUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWUreg x:(MOVWUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWUreg x:(MOVWUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWUreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(uint32(c))])
 
            // match: (MOVWUreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(uint32(c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(uint32(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWload_0(ref Value v)
        { 
            // match: (MOVWload [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWreg_0(ref Value v)
        { 
            // match: (MOVWreg x:(MOVBload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVBUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVHload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHUload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHUload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVWload _ _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVWload _ _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVWload)
                {
                    break;
                }
                _ = x.Args[1L];
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVBreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVBreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVBUreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVHreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVHreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVHreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWreg x:(MOVWreg _))
            // cond:
            // result: (MOVVreg x)
 
            // match: (MOVWreg x:(MOVWreg _))
            // cond:
            // result: (MOVVreg x)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != OpMIPS64MOVWreg)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVreg);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWreg_10(ref Value v)
        { 
            // match: (MOVWreg (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [int64(int32(c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(int32(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWstore_0(ref Value v)
        { 
            // match: (MOVWstore [off1] {sym} (ADDVconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
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
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVWstorezero [off] {sym} ptr mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVVconst [0]) mem)
            // cond:
            // result: (MOVWstorezero [off] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_1.AuxInt != 0L)
                {
                    break;
                }
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVWstorezero);
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
                if (v_1.Op != OpMIPS64MOVWreg)
                {
                    break;
                }
                var x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWUreg x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWUreg x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64MOVWstorezero_0(ref Value v)
        { 
            // match: (MOVWstorezero [off1] {sym} (ADDVconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWstorezero [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
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
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVaddr)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVWstorezero);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64NEGV_0(ref Value v)
        { 
            // match: (NEGV (MOVVconst [c]))
            // cond:
            // result: (MOVVconst [-c])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = -c;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64NOR_0(ref Value v)
        { 
            // match: (NOR x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (NORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64NORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (NOR (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (NORconst [c] x)
 
            // match: (NOR (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (NORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64NORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64NORconst_0(ref Value v)
        { 
            // match: (NORconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [^(c|d)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = ~(c | d);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64OR_0(ref Value v)
        { 
            // match: (OR x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (ORconst  [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (OR (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (ORconst  [c] x)
 
            // match: (OR (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (ORconst  [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ORconst);
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

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64ORconst_0(ref Value v)
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
            // result: (MOVVconst [-1])
 
            // match: (ORconst [-1] _)
            // cond:
            // result: (MOVVconst [-1])
            while (true)
            {
                if (v.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = -1L;
                return true;
            } 
            // match: (ORconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c|d])
 
            // match: (ORconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c|d])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = c | d;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ORconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(is32Bit(c | d)))
                {
                    break;
                }
                v.reset(OpMIPS64ORconst);
                v.AuxInt = c | d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SGT_0(ref Value v)
        { 
            // match: (SGT (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (SGTconst  [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SGTconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTU_0(ref Value v)
        { 
            // match: (SGTU (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (SGTUconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SGTUconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTUconst_0(ref Value v)
        { 
            // match: (SGTUconst [c] (MOVVconst [d]))
            // cond: uint64(c)>uint64(d)
            // result: (MOVVconst [1])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(uint64(c) > uint64(d)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(uint64(c) <= uint64(d)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                if (!(0xffUL < uint64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                if (!(0xffffUL < uint64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ANDconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(uint64(m) < uint64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTUconst [c] (SRLVconst _ [d]))
            // cond: 0 < d && d <= 63 && 1<<uint64(64-d) <= uint64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTUconst [c] (SRLVconst _ [d]))
            // cond: 0 < d && d <= 63 && 1<<uint64(64-d) <= uint64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64SRLVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(0L < d && d <= 63L && 1L << (int)(uint64(64L - d)) <= uint64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTconst_0(ref Value v)
        { 
            // match: (SGTconst [c] (MOVVconst [d]))
            // cond: int64(c)>int64(d)
            // result: (MOVVconst [1])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(int64(c) > int64(d)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVVconst [d]))
            // cond: int64(c)<=int64(d)
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVVconst [d]))
            // cond: int64(c)<=int64(d)
            // result: (MOVVconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                if (!(int64(c) <= int64(d)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: 0x7f < int64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: 0x7f < int64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVBreg)
                {
                    break;
                }
                if (!(0x7fUL < int64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: int64(c) <= -0x80
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVBreg _))
            // cond: int64(c) <= -0x80
            // result: (MOVVconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVBreg)
                {
                    break;
                }
                if (!(int64(c) <= -0x80UL))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: 0xff < int64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: 0xff < int64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                if (!(0xffUL < int64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: int64(c) < 0
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVBUreg _))
            // cond: int64(c) < 0
            // result: (MOVVconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVBUreg)
                {
                    break;
                }
                if (!(int64(c) < 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: 0x7fff < int64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: 0x7fff < int64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                if (!(0x7fffUL < int64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: int64(c) <= -0x8000
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVHreg _))
            // cond: int64(c) <= -0x8000
            // result: (MOVVconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVHreg)
                {
                    break;
                }
                if (!(int64(c) <= -0x8000UL))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: 0xffff < int64(c)
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: 0xffff < int64(c)
            // result: (MOVVconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                if (!(0xffffUL < int64(c)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: int64(c) < 0
            // result: (MOVVconst [0])
 
            // match: (SGTconst [c] (MOVHUreg _))
            // cond: int64(c) < 0
            // result: (MOVVconst [0])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVHUreg)
                {
                    break;
                }
                if (!(int64(c) < 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SGTconst_10(ref Value v)
        { 
            // match: (SGTconst [c] (MOVWUreg _))
            // cond: int64(c) < 0
            // result: (MOVVconst [0])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVWUreg)
                {
                    break;
                }
                if (!(int64(c) < 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ANDconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(0L <= m && m < c))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SGTconst [c] (SRLVconst _ [d]))
            // cond: 0 <= c && 0 < d && d <= 63 && 1<<uint64(64-d) <= c
            // result: (MOVVconst [1])
 
            // match: (SGTconst [c] (SRLVconst _ [d]))
            // cond: 0 <= c && 0 < d && d <= 63 && 1<<uint64(64-d) <= c
            // result: (MOVVconst [1])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64SRLVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                if (!(0L <= c && 0L < d && d <= 63L && 1L << (int)(uint64(64L - d)) <= c))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SLLV_0(ref Value v)
        { 
            // match: (SLLV _ (MOVVconst [c]))
            // cond: uint64(c)>=64
            // result: (MOVVconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint64(c) >= 64L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SLLV x (MOVVconst [c]))
            // cond:
            // result: (SLLVconst x [c])
 
            // match: (SLLV x (MOVVconst [c]))
            // cond:
            // result: (SLLVconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPS64SLLVconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SLLVconst_0(ref Value v)
        { 
            // match: (SLLVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [int64(d)<<uint64(c)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(d) << (int)(uint64(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SRAV_0(ref Value v)
        { 
            // match: (SRAV x (MOVVconst [c]))
            // cond: uint64(c)>=64
            // result: (SRAVconst x [63])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint64(c) >= 64L))
                {
                    break;
                }
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = 63L;
                v.AddArg(x);
                return true;
            } 
            // match: (SRAV x (MOVVconst [c]))
            // cond:
            // result: (SRAVconst x [c])
 
            // match: (SRAV x (MOVVconst [c]))
            // cond:
            // result: (SRAVconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SRAVconst_0(ref Value v)
        { 
            // match: (SRAVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [int64(d)>>uint64(c)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(d) >> (int)(uint64(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SRLV_0(ref Value v)
        { 
            // match: (SRLV _ (MOVVconst [c]))
            // cond: uint64(c)>=64
            // result: (MOVVconst [0])
            while (true)
            {
                _ = v.Args[1L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(uint64(c) >= 64L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SRLV x (MOVVconst [c]))
            // cond:
            // result: (SRLVconst x [c])
 
            // match: (SRLV x (MOVVconst [c]))
            // cond:
            // result: (SRLVconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                v.reset(OpMIPS64SRLVconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SRLVconst_0(ref Value v)
        { 
            // match: (SRLVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [int64(uint64(d)>>uint64(c))])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(uint64(d) >> (int)(uint64(c)));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SUBV_0(ref Value v)
        { 
            // match: (SUBV x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (SUBVconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SUBVconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (SUBV x x)
            // cond:
            // result: (MOVVconst [0])
 
            // match: (SUBV x x)
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SUBV (MOVVconst [0]) x)
            // cond:
            // result: (NEGV x)
 
            // match: (SUBV (MOVVconst [0]) x)
            // cond:
            // result: (NEGV x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[1L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64SUBVconst_0(ref Value v)
        { 
            // match: (SUBVconst [0] x)
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
            // match: (SUBVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [d-c])
 
            // match: (SUBVconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [d-c])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = d - c;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64SUBVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(is32Bit(-c - d)))
                {
                    break;
                }
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = -c - d;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64ADDVconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(is32Bit(-c + d)))
                {
                    break;
                }
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = -c + d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64XOR_0(ref Value v)
        { 
            // match: (XOR x (MOVVconst [c]))
            // cond: is32Bit(c)
            // result: (XORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64XORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XOR (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (XORconst [c] x)
 
            // match: (XOR (MOVVconst [c]) x)
            // cond: is32Bit(c)
            // result: (XORconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                if (!(is32Bit(c)))
                {
                    break;
                }
                v.reset(OpMIPS64XORconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XOR x x)
            // cond:
            // result: (MOVVconst [0])
 
            // match: (XOR x x)
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMIPS64XORconst_0(ref Value v)
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
                v.reset(OpMIPS64NORconst);
                v.AuxInt = 0L;
                v.AddArg(x);
                return true;
            } 
            // match: (XORconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c^d])
 
            // match: (XORconst [c] (MOVVconst [d]))
            // cond:
            // result: (MOVVconst [c^d])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = c ^ d;
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
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64XORconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(is32Bit(c ^ d)))
                {
                    break;
                }
                v.reset(OpMIPS64XORconst);
                v.AuxInt = c ^ d;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMod16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod16 x y)
            // cond:
            // result: (Select0 (DIVV (SignExt16to64 x) (SignExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod16u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod16u x y)
            // cond:
            // result: (Select0 (DIVVU (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod32 x y)
            // cond:
            // result: (Select0 (DIVV (SignExt32to64 x) (SignExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod32u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod32u x y)
            // cond:
            // result: (Select0 (DIVVU (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod64 x y)
            // cond:
            // result: (Select0 (DIVV x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod64u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod64u x y)
            // cond:
            // result: (Select0 (DIVVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod8 x y)
            // cond:
            // result: (Select0 (DIVV (SignExt8to64 x) (SignExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVV, types.NewTuple(typ.Int64, typ.Int64));
                var v1 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMod8u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod8u x y)
            // cond:
            // result: (Select0 (DIVVU (ZeroExt8to64 x) (ZeroExt8to64 y)))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect0);
                var v0 = b.NewValue0(v.Pos, OpMIPS64DIVVU, types.NewTuple(typ.UInt64, typ.UInt64));
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMove_0(ref Value v)
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
            // result: (MOVBstore dst (MOVBload src mem) mem)
 
            // match: (Move [1] dst src mem)
            // cond:
            // result: (MOVBstore dst (MOVBload src mem) mem)
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
                v.reset(OpMIPS64MOVBstore);
                v.AddArg(dst);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [2] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore dst (MOVHload src mem) mem)
 
            // match: (Move [2] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore dst (MOVHload src mem) mem)
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
                v.reset(OpMIPS64MOVHstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [2] dst src mem)
            // cond:
            // result: (MOVBstore [1] dst (MOVBload [1] src mem)         (MOVBstore dst (MOVBload src mem) mem))
 
            // match: (Move [2] dst src mem)
            // cond:
            // result: (MOVBstore [1] dst (MOVBload [1] src mem)         (MOVBstore dst (MOVBload src mem) mem))
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
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = 1L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AuxInt = 1L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AddArg(dst);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
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
                v.reset(OpMIPS64MOVWstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [4] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] dst (MOVHload [2] src mem)         (MOVHstore dst (MOVHload src mem) mem))
 
            // match: (Move [4] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] dst (MOVHload [2] src mem)         (MOVHstore dst (MOVHload src mem) mem))
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
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = 2L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AuxInt = 2L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [4] dst src mem)
            // cond:
            // result: (MOVBstore [3] dst (MOVBload [3] src mem)         (MOVBstore [2] dst (MOVBload [2] src mem)             (MOVBstore [1] dst (MOVBload [1] src mem)                 (MOVBstore dst (MOVBload src mem) mem))))
 
            // match: (Move [4] dst src mem)
            // cond:
            // result: (MOVBstore [3] dst (MOVBload [3] src mem)         (MOVBstore [2] dst (MOVBload [2] src mem)             (MOVBstore [1] dst (MOVBload [1] src mem)                 (MOVBstore dst (MOVBload src mem) mem))))
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
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = 3L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AuxInt = 3L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v2.AuxInt = 2L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v3.AuxInt = 1L;
                v3.AddArg(dst);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v4.AuxInt = 1L;
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v5.AddArg(dst);
                var v6 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v6.AddArg(src);
                v6.AddArg(mem);
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [8] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore dst (MOVVload src mem) mem)
 
            // match: (Move [8] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore dst (MOVVload src mem) mem)
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
                if (!(t._<ref types.Type>().Alignment() % 8L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
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
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
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
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = 6L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AuxInt = 6L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v2.AuxInt = 4L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v3.AuxInt = 2L;
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v4.AuxInt = 2L;
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v5 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v5.AddArg(dst);
                v6 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
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
        private static bool rewriteValueMIPS64_OpMove_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Move [3] dst src mem)
            // cond:
            // result: (MOVBstore [2] dst (MOVBload [2] src mem)         (MOVBstore [1] dst (MOVBload [1] src mem)             (MOVBstore dst (MOVBload src mem) mem)))
            while (true)
            {
                if (v.AuxInt != 3L)
                {
                    break;
                }
                _ = v.Args[2L];
                var dst = v.Args[0L];
                var src = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = 2L;
                v.AddArg(dst);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v0.AuxInt = 2L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = 1L;
                v1.AddArg(dst);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v2.AuxInt = 1L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v3.AddArg(dst);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVBload, typ.Int8);
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [6] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [4] dst (MOVHload [4] src mem)         (MOVHstore [2] dst (MOVHload [2] src mem)             (MOVHstore dst (MOVHload src mem) mem)))
 
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
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
                v2.AuxInt = 2L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVHload, typ.Int16);
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
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = 8L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v0.AuxInt = 8L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v2.AuxInt = 4L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVWload, typ.Int32);
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [16] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [8] dst (MOVVload [8] src mem)         (MOVVstore dst (MOVVload src mem) mem))
 
            // match: (Move [16] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [8] dst (MOVVload [8] src mem)         (MOVVstore dst (MOVVload src mem) mem))
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
                if (!(t._<ref types.Type>().Alignment() % 8L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = 8L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v0.AuxInt = 8L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [24] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [16] dst (MOVVload [16] src mem)         (MOVVstore [8] dst (MOVVload [8] src mem)             (MOVVstore dst (MOVVload src mem) mem)))
 
            // match: (Move [24] {t} dst src mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [16] dst (MOVVload [16] src mem)         (MOVVstore [8] dst (MOVVload [8] src mem)             (MOVVstore dst (MOVVload src mem) mem)))
            while (true)
            {
                if (v.AuxInt != 24L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Alignment() % 8L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = 16L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v0.AuxInt = 16L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AuxInt = 8L;
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v2.AuxInt = 8L;
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v3.AddArg(dst);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVVload, typ.UInt64);
                v4.AddArg(src);
                v4.AddArg(mem);
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [s] {t} dst src mem)
            // cond: s > 24 || t.(*types.Type).Alignment()%8 != 0
            // result: (LoweredMove [t.(*types.Type).Alignment()]         dst         src         (ADDVconst <src.Type> src [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
 
            // match: (Move [s] {t} dst src mem)
            // cond: s > 24 || t.(*types.Type).Alignment()%8 != 0
            // result: (LoweredMove [t.(*types.Type).Alignment()]         dst         src         (ADDVconst <src.Type> src [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
            while (true)
            {
                var s = v.AuxInt;
                t = v.Aux;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(s > 24L || t._<ref types.Type>().Alignment() % 8L != 0L))
                {
                    break;
                }
                v.reset(OpMIPS64LoweredMove);
                v.AuxInt = t._<ref types.Type>().Alignment();
                v.AddArg(dst);
                v.AddArg(src);
                v0 = b.NewValue0(v.Pos, OpMIPS64ADDVconst, src.Type);
                v0.AuxInt = s - moveSize(t._<ref types.Type>().Alignment(), config);
                v0.AddArg(src);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpMul16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mul16 x y)
            // cond:
            // result: (Select1 (MULVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMul32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mul32 x y)
            // cond:
            // result: (Select1 (MULVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMul32F_0(ref Value v)
        { 
            // match: (Mul32F x y)
            // cond:
            // result: (MULF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64MULF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMul64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mul64 x y)
            // cond:
            // result: (Select1 (MULVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMul64F_0(ref Value v)
        { 
            // match: (Mul64F x y)
            // cond:
            // result: (MULD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64MULD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpMul8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mul8 x y)
            // cond:
            // result: (Select1 (MULVU x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpSelect1);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MULVU, types.NewTuple(typ.UInt64, typ.UInt64));
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeg16_0(ref Value v)
        { 
            // match: (Neg16 x)
            // cond:
            // result: (NEGV x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeg32_0(ref Value v)
        { 
            // match: (Neg32 x)
            // cond:
            // result: (NEGV x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeg32F_0(ref Value v)
        { 
            // match: (Neg32F x)
            // cond:
            // result: (NEGF x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NEGF);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeg64_0(ref Value v)
        { 
            // match: (Neg64 x)
            // cond:
            // result: (NEGV x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeg64F_0(ref Value v)
        { 
            // match: (Neg64F x)
            // cond:
            // result: (NEGD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NEGD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeg8_0(ref Value v)
        { 
            // match: (Neg8 x)
            // cond:
            // result: (NEGV x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq16 x y)
            // cond:
            // result: (SGTU (XOR (ZeroExt16to32 x) (ZeroExt16to64 y)) (MOVVconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq32 x y)
            // cond:
            // result: (SGTU (XOR (ZeroExt32to64 x) (ZeroExt32to64 y)) (MOVVconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeq32F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagFalse);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQF, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeq64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq64 x y)
            // cond:
            // result: (SGTU (XOR x y) (MOVVconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeq64F_0(ref Value v)
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
                v.reset(OpMIPS64FPFlagFalse);
                var v0 = b.NewValue0(v.Pos, OpMIPS64CMPEQD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neq8 x y)
            // cond:
            // result: (SGTU (XOR (ZeroExt8to64 x) (ZeroExt8to64 y)) (MOVVconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v1.AddArg(x);
                v0.AddArg(v1);
                var v2 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v2.AddArg(y);
                v0.AddArg(v2);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = 0L;
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeqB_0(ref Value v)
        { 
            // match: (NeqB x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNeqPtr_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (NeqPtr x y)
            // cond:
            // result: (SGTU (XOR x y) (MOVVconst [0]))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SGTU);
                var v0 = b.NewValue0(v.Pos, OpMIPS64XOR, typ.UInt64);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNilCheck_0(ref Value v)
        { 
            // match: (NilCheck ptr mem)
            // cond:
            // result: (LoweredNilCheck ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPS64LoweredNilCheck);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpNot_0(ref Value v)
        { 
            // match: (Not x)
            // cond:
            // result: (XORconst [1] x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64XORconst);
                v.AuxInt = 1L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpOffPtr_0(ref Value v)
        { 
            // match: (OffPtr [off] ptr:(SP))
            // cond:
            // result: (MOVVaddr [off] ptr)
            while (true)
            {
                var off = v.AuxInt;
                var ptr = v.Args[0L];
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
            // cond:
            // result: (ADDVconst [off] ptr)
 
            // match: (OffPtr [off] ptr)
            // cond:
            // result: (ADDVconst [off] ptr)
            while (true)
            {
                off = v.AuxInt;
                ptr = v.Args[0L];
                v.reset(OpMIPS64ADDVconst);
                v.AuxInt = off;
                v.AddArg(ptr);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpOr16_0(ref Value v)
        { 
            // match: (Or16 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64OR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpOr32_0(ref Value v)
        { 
            // match: (Or32 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64OR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpOr64_0(ref Value v)
        { 
            // match: (Or64 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64OR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpOr8_0(ref Value v)
        { 
            // match: (Or8 x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64OR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpOrB_0(ref Value v)
        { 
            // match: (OrB x y)
            // cond:
            // result: (OR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64OR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRound32F_0(ref Value v)
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
        private static bool rewriteValueMIPS64_OpRound64F_0(ref Value v)
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
        private static bool rewriteValueMIPS64_OpRsh16Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> (ZeroExt16to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> (ZeroExt16to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> (ZeroExt16to64 x) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(x);
                v3.AddArg(v4);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16Ux8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SRLV <t> (ZeroExt16to64 x) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x16 <t> x y)
            // cond:
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x32 <t> x y)
            // cond:
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x64 <t> x y)
            // cond:
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v3.AddArg(y);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 63L;
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh16x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh16x8 <t> x y)
            // cond:
            // result: (SRAV (SignExt16to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt8to64  y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt16to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> (ZeroExt32to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> (ZeroExt32to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> (ZeroExt32to64 x) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(x);
                v3.AddArg(v4);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32Ux8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SRLV <t> (ZeroExt32to64 x) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x16 <t> x y)
            // cond:
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x32 <t> x y)
            // cond:
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x64 <t> x y)
            // cond:
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v3.AddArg(y);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 63L;
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh32x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh32x8 <t> x y)
            // cond:
            // result: (SRAV (SignExt32to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt8to64  y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt32to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> x (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> x (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> x y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v3.AddArg(x);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64Ux8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SRLV <t> x (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                v4.AddArg(x);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(y);
                v4.AddArg(v5);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x16 <t> x y)
            // cond:
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 63L;
                v2.AddArg(v4);
                v1.AddArg(v2);
                v0.AddArg(v1);
                var v5 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v5.AddArg(y);
                v0.AddArg(v5);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x32 <t> x y)
            // cond:
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 63L;
                v2.AddArg(v4);
                v1.AddArg(v2);
                v0.AddArg(v1);
                var v5 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v5.AddArg(y);
                v0.AddArg(v5);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x64 <t> x y)
            // cond:
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v2.AddArg(y);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v3.AuxInt = 63L;
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh64x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh64x8 <t> x y)
            // cond:
            // result: (SRAV x (OR <t> (NEGV <t> (SGTU (ZeroExt8to64  y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v2.AddArg(v3);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 63L;
                v2.AddArg(v4);
                v1.AddArg(v2);
                v0.AddArg(v1);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(y);
                v0.AddArg(v5);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux16 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt16to64 y))) (SRLV <t> (ZeroExt8to64 x) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux32 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt32to64 y))) (SRLV <t> (ZeroExt8to64 x) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8Ux64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux64 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) y)) (SRLV <t> (ZeroExt8to64 x) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                v1.AddArg(y);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(x);
                v3.AddArg(v4);
                v3.AddArg(y);
                v.AddArg(v3);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8Ux8 <t> x y)
            // cond:
            // result: (AND (NEGV <t> (SGTU (MOVVconst <typ.UInt64> [64]) (ZeroExt8to64  y))) (SRLV <t> (ZeroExt8to64 x) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64AND);
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v1 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 64L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v3.AddArg(y);
                v1.AddArg(v3);
                v0.AddArg(v1);
                v.AddArg(v0);
                var v4 = b.NewValue0(v.Pos, OpMIPS64SRLV, t);
                var v5 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v5.AddArg(x);
                v4.AddArg(v5);
                var v6 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v6.AddArg(y);
                v4.AddArg(v6);
                v.AddArg(v4);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x16 <t> x y)
            // cond:
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt16to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt16to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt16to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x32 <t> x y)
            // cond:
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt32to64 y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt32to64 y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt32to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8x64_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x64 <t> x y)
            // cond:
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU y (MOVVconst <typ.UInt64> [63]))) y))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                v3.AddArg(y);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 63L;
                v3.AddArg(v4);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpRsh8x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Rsh8x8 <t> x y)
            // cond:
            // result: (SRAV (SignExt8to64 x) (OR <t> (NEGV <t> (SGTU (ZeroExt8to64  y) (MOVVconst <typ.UInt64> [63]))) (ZeroExt8to64  y)))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SRAV);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to64, typ.Int64);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64OR, t);
                var v2 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                var v3 = b.NewValue0(v.Pos, OpMIPS64SGTU, typ.Bool);
                var v4 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v4.AddArg(y);
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v5.AuxInt = 63L;
                v3.AddArg(v5);
                v2.AddArg(v3);
                v1.AddArg(v2);
                var v6 = b.NewValue0(v.Pos, OpZeroExt8to64, typ.UInt64);
                v6.AddArg(y);
                v1.AddArg(v6);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSelect0_0(ref Value v)
        { 
            // match: (Select0 (DIVVU _ (MOVVconst [1])))
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
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
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64DIVVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var x = v_0.Args[0L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0_1.AuxInt;
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }
                v.reset(OpMIPS64ANDconst);
                v.AuxInt = c - 1L;
                v.AddArg(x);
                return true;
            } 
            // match: (Select0 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(c)%int64(d)])
 
            // match: (Select0 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(c)%int64(d)])
            while (true)
            {
                v_0 = v.Args[0L];
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
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0_1.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(c) % int64(d);
                return true;
            } 
            // match: (Select0 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(uint64(c)%uint64(d))])
 
            // match: (Select0 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(uint64(c)%uint64(d))])
            while (true)
            {
                v_0 = v.Args[0L];
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
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                d = v_0_1.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(uint64(c) % uint64(d));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpSelect1_0(ref Value v)
        { 
            // match: (Select1 (MULVU x (MOVVconst [-1])))
            // cond:
            // result: (NEGV x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var x = v_0.Args[0L];
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [-1]) x))
            // cond:
            // result: (NEGV x)
 
            // match: (Select1 (MULVU (MOVVconst [-1]) x))
            // cond:
            // result: (NEGV x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != -1L)
                {
                    break;
                }
                x = v_0.Args[1L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU _ (MOVVconst [0])))
            // cond:
            // result: (MOVVconst [0])
 
            // match: (Select1 (MULVU _ (MOVVconst [0])))
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [0]) _))
            // cond:
            // result: (MOVVconst [0])
 
            // match: (Select1 (MULVU (MOVVconst [0]) _))
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select1 (MULVU x (MOVVconst [1])))
            // cond:
            // result: x
 
            // match: (Select1 (MULVU x (MOVVconst [1])))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
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
                if (v_0_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [1]) x))
            // cond:
            // result: x
 
            // match: (Select1 (MULVU (MOVVconst [1]) x))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
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
            // match: (Select1 (MULVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
 
            // match: (Select1 (MULVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
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
                var c = v_0_1.AuxInt;
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SLLVconst);
                v.AuxInt = log2(c);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [c]) x))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
 
            // match: (Select1 (MULVU (MOVVconst [c]) x))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0_0.AuxInt;
                x = v_0.Args[1L];
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SLLVconst);
                v.AuxInt = log2(c);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [-1]) x))
            // cond:
            // result: (NEGV x)
 
            // match: (Select1 (MULVU (MOVVconst [-1]) x))
            // cond:
            // result: (NEGV x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != -1L)
                {
                    break;
                }
                x = v_0.Args[1L];
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU x (MOVVconst [-1])))
            // cond:
            // result: (NEGV x)
 
            // match: (Select1 (MULVU x (MOVVconst [-1])))
            // cond:
            // result: (NEGV x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
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
                if (v_0_1.AuxInt != -1L)
                {
                    break;
                }
                v.reset(OpMIPS64NEGV);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpSelect1_10(ref Value v)
        { 
            // match: (Select1 (MULVU (MOVVconst [0]) _))
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select1 (MULVU _ (MOVVconst [0])))
            // cond:
            // result: (MOVVconst [0])
 
            // match: (Select1 (MULVU _ (MOVVconst [0])))
            // cond:
            // result: (MOVVconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_1.AuxInt != 0L)
                {
                    break;
                }
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [1]) x))
            // cond:
            // result: x
 
            // match: (Select1 (MULVU (MOVVconst [1]) x))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                if (v_0_0.AuxInt != 1L)
                {
                    break;
                }
                var x = v_0.Args[1L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU x (MOVVconst [1])))
            // cond:
            // result: x
 
            // match: (Select1 (MULVU x (MOVVconst [1])))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
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
                if (v_0_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [c]) x))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
 
            // match: (Select1 (MULVU (MOVVconst [c]) x))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var c = v_0_0.AuxInt;
                x = v_0.Args[1L];
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SLLVconst);
                v.AuxInt = log2(c);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
 
            // match: (Select1 (MULVU x (MOVVconst [c])))
            // cond: isPowerOfTwo(c)
            // result: (SLLVconst [log2(c)] x)
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
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
                c = v_0_1.AuxInt;
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SLLVconst);
                v.AuxInt = log2(c);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (DIVVU x (MOVVconst [1])))
            // cond:
            // result: x
 
            // match: (Select1 (DIVVU x (MOVVconst [1])))
            // cond:
            // result: x
            while (true)
            {
                v_0 = v.Args[0L];
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
                if (v_0_1.AuxInt != 1L)
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
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
                v_0 = v.Args[0L];
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
                c = v_0_1.AuxInt;
                if (!(isPowerOfTwo(c)))
                {
                    break;
                }
                v.reset(OpMIPS64SRLVconst);
                v.AuxInt = log2(c);
                v.AddArg(x);
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [c*d])
 
            // match: (Select1 (MULVU (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [c*d])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0_1.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = c * d;
                return true;
            } 
            // match: (Select1 (MULVU (MOVVconst [d]) (MOVVconst [c])))
            // cond:
            // result: (MOVVconst [c*d])
 
            // match: (Select1 (MULVU (MOVVconst [d]) (MOVVconst [c])))
            // cond:
            // result: (MOVVconst [c*d])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != OpMIPS64MULVU)
                {
                    break;
                }
                _ = v_0.Args[1L];
                v_0_0 = v_0.Args[0L];
                if (v_0_0.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                d = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                c = v_0_1.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = c * d;
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpSelect1_20(ref Value v)
        { 
            // match: (Select1 (DIVV (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(c)/int64(d)])
            while (true)
            {
                var v_0 = v.Args[0L];
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
                var c = v_0_0.AuxInt;
                var v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                var d = v_0_1.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(c) / int64(d);
                return true;
            } 
            // match: (Select1 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(uint64(c)/uint64(d))])
 
            // match: (Select1 (DIVVU (MOVVconst [c]) (MOVVconst [d])))
            // cond:
            // result: (MOVVconst [int64(uint64(c)/uint64(d))])
            while (true)
            {
                v_0 = v.Args[0L];
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
                c = v_0_0.AuxInt;
                v_0_1 = v_0.Args[1L];
                if (v_0_1.Op != OpMIPS64MOVVconst)
                {
                    break;
                }
                d = v_0_1.AuxInt;
                v.reset(OpMIPS64MOVVconst);
                v.AuxInt = int64(uint64(c) / uint64(d));
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpSignExt16to32_0(ref Value v)
        { 
            // match: (SignExt16to32 x)
            // cond:
            // result: (MOVHreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVHreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSignExt16to64_0(ref Value v)
        { 
            // match: (SignExt16to64 x)
            // cond:
            // result: (MOVHreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVHreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSignExt32to64_0(ref Value v)
        { 
            // match: (SignExt32to64 x)
            // cond:
            // result: (MOVWreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVWreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSignExt8to16_0(ref Value v)
        { 
            // match: (SignExt8to16 x)
            // cond:
            // result: (MOVBreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVBreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSignExt8to32_0(ref Value v)
        { 
            // match: (SignExt8to32 x)
            // cond:
            // result: (MOVBreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVBreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSignExt8to64_0(ref Value v)
        { 
            // match: (SignExt8to64 x)
            // cond:
            // result: (MOVBreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVBreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSlicemask_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Slicemask <t> x)
            // cond:
            // result: (SRAVconst (NEGV <t> x) [63])
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(OpMIPS64SRAVconst);
                v.AuxInt = 63L;
                var v0 = b.NewValue0(v.Pos, OpMIPS64NEGV, t);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpStaticCall_0(ref Value v)
        { 
            // match: (StaticCall [argwid] {target} mem)
            // cond:
            // result: (CALLstatic [argwid] {target} mem)
            while (true)
            {
                var argwid = v.AuxInt;
                var target = v.Aux;
                var mem = v.Args[0L];
                v.reset(OpMIPS64CALLstatic);
                v.AuxInt = argwid;
                v.Aux = target;
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpStore_0(ref Value v)
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
                v.reset(OpMIPS64MOVBstore);
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
                v.reset(OpMIPS64MOVHstore);
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
                v.reset(OpMIPS64MOVWstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 8 && !is64BitFloat(val.Type)
            // result: (MOVVstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 8 && !is64BitFloat(val.Type)
            // result: (MOVVstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 8L && !is64BitFloat(val.Type)))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
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
                v.reset(OpMIPS64MOVFstore);
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
                v.reset(OpMIPS64MOVDstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpSub16_0(ref Value v)
        { 
            // match: (Sub16 x y)
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSub32_0(ref Value v)
        { 
            // match: (Sub32 x y)
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSub32F_0(ref Value v)
        { 
            // match: (Sub32F x y)
            // cond:
            // result: (SUBF x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBF);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSub64_0(ref Value v)
        { 
            // match: (Sub64 x y)
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSub64F_0(ref Value v)
        { 
            // match: (Sub64F x y)
            // cond:
            // result: (SUBD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSub8_0(ref Value v)
        { 
            // match: (Sub8 x y)
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpSubPtr_0(ref Value v)
        { 
            // match: (SubPtr x y)
            // cond:
            // result: (SUBV x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64SUBV);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpTrunc16to8_0(ref Value v)
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
        private static bool rewriteValueMIPS64_OpTrunc32to16_0(ref Value v)
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
        private static bool rewriteValueMIPS64_OpTrunc32to8_0(ref Value v)
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
        private static bool rewriteValueMIPS64_OpTrunc64to16_0(ref Value v)
        { 
            // match: (Trunc64to16 x)
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
        private static bool rewriteValueMIPS64_OpTrunc64to32_0(ref Value v)
        { 
            // match: (Trunc64to32 x)
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
        private static bool rewriteValueMIPS64_OpTrunc64to8_0(ref Value v)
        { 
            // match: (Trunc64to8 x)
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
        private static bool rewriteValueMIPS64_OpXor16_0(ref Value v)
        { 
            // match: (Xor16 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpXor32_0(ref Value v)
        { 
            // match: (Xor32 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpXor64_0(ref Value v)
        { 
            // match: (Xor64 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpXor8_0(ref Value v)
        { 
            // match: (Xor8 x y)
            // cond:
            // result: (XOR x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(OpMIPS64XOR);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpZero_0(ref Value v)
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
            // result: (MOVBstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [1] ptr mem)
            // cond:
            // result: (MOVBstore ptr (MOVVconst [0]) mem)
            while (true)
            {
                if (v.AuxInt != 1L)
                {
                    break;
                }
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPS64MOVBstore);
                v.AddArg(ptr);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [2] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [2] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore ptr (MOVVconst [0]) mem)
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
                v.reset(OpMIPS64MOVHstore);
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [2] ptr mem)
            // cond:
            // result: (MOVBstore [1] ptr (MOVVconst [0])         (MOVBstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [2] ptr mem)
            // cond:
            // result: (MOVBstore [1] ptr (MOVVconst [0])         (MOVBstore [0] ptr (MOVVconst [0]) mem))
            while (true)
            {
                if (v.AuxInt != 2L)
                {
                    break;
                }
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = 1L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore ptr (MOVVconst [0]) mem)
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
                v.reset(OpMIPS64MOVWstore);
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] ptr (MOVVconst [0])         (MOVHstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [4] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [2] ptr (MOVVconst [0])         (MOVHstore [0] ptr (MOVVconst [0]) mem))
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
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = 2L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [4] ptr mem)
            // cond:
            // result: (MOVBstore [3] ptr (MOVVconst [0])         (MOVBstore [2] ptr (MOVVconst [0])             (MOVBstore [1] ptr (MOVVconst [0])                 (MOVBstore [0] ptr (MOVVconst [0]) mem))))
 
            // match: (Zero [4] ptr mem)
            // cond:
            // result: (MOVBstore [3] ptr (MOVVconst [0])         (MOVBstore [2] ptr (MOVVconst [0])             (MOVBstore [1] ptr (MOVVconst [0])                 (MOVBstore [0] ptr (MOVVconst [0]) mem))))
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = 3L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v3.AuxInt = 1L;
                v3.AddArg(ptr);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                var v5 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v5.AuxInt = 0L;
                v5.AddArg(ptr);
                var v6 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v6.AuxInt = 0L;
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore ptr (MOVVconst [0]) mem)
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore ptr (MOVVconst [0]) mem)
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
                if (!(t._<ref types.Type>().Alignment() % 8L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [4] ptr (MOVVconst [0])         (MOVWstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [4] ptr (MOVVconst [0])         (MOVWstore [0] ptr (MOVVconst [0]) mem))
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
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = 4L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [6] ptr (MOVVconst [0])         (MOVHstore [4] ptr (MOVVconst [0])             (MOVHstore [2] ptr (MOVVconst [0])                 (MOVHstore [0] ptr (MOVVconst [0]) mem))))
 
            // match: (Zero [8] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [6] ptr (MOVVconst [0])         (MOVHstore [4] ptr (MOVVconst [0])             (MOVHstore [2] ptr (MOVVconst [0])                 (MOVHstore [0] ptr (MOVVconst [0]) mem))))
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
                if (!(t._<ref types.Type>().Alignment() % 2L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = 6L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v3.AuxInt = 2L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v5 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v5.AuxInt = 0L;
                v5.AddArg(ptr);
                v6 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v6.AuxInt = 0L;
                v5.AddArg(v6);
                v5.AddArg(mem);
                v3.AddArg(v5);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpZero_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Zero [3] ptr mem)
            // cond:
            // result: (MOVBstore [2] ptr (MOVVconst [0])         (MOVBstore [1] ptr (MOVVconst [0])             (MOVBstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 3L)
                {
                    break;
                }
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(OpMIPS64MOVBstore);
                v.AuxInt = 2L;
                v.AddArg(ptr);
                var v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v1.AuxInt = 1L;
                v1.AddArg(ptr);
                var v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                var v3 = b.NewValue0(v.Pos, OpMIPS64MOVBstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                var v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [6] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [4] ptr (MOVVconst [0])         (MOVHstore [2] ptr (MOVVconst [0])             (MOVHstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [6] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%2 == 0
            // result: (MOVHstore [4] ptr (MOVVconst [0])         (MOVHstore [2] ptr (MOVVconst [0])             (MOVHstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 6L)
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
                v.reset(OpMIPS64MOVHstore);
                v.AuxInt = 4L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v1.AuxInt = 2L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVHstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [12] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [8] ptr (MOVVconst [0])         (MOVWstore [4] ptr (MOVVconst [0])             (MOVWstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [12] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%4 == 0
            // result: (MOVWstore [8] ptr (MOVVconst [0])         (MOVWstore [4] ptr (MOVVconst [0])             (MOVWstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 12L)
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
                v.reset(OpMIPS64MOVWstore);
                v.AuxInt = 8L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v1.AuxInt = 4L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVWstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [16] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [8] ptr (MOVVconst [0])         (MOVVstore [0] ptr (MOVVconst [0]) mem))
 
            // match: (Zero [16] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [8] ptr (MOVVconst [0])         (MOVVstore [0] ptr (MOVVconst [0]) mem))
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
                if (!(t._<ref types.Type>().Alignment() % 8L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = 8L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [24] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [16] ptr (MOVVconst [0])         (MOVVstore [8] ptr (MOVVconst [0])             (MOVVstore [0] ptr (MOVVconst [0]) mem)))
 
            // match: (Zero [24] {t} ptr mem)
            // cond: t.(*types.Type).Alignment()%8 == 0
            // result: (MOVVstore [16] ptr (MOVVconst [0])         (MOVVstore [8] ptr (MOVVconst [0])             (MOVVstore [0] ptr (MOVVconst [0]) mem)))
            while (true)
            {
                if (v.AuxInt != 24L)
                {
                    break;
                }
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t._<ref types.Type>().Alignment() % 8L == 0L))
                {
                    break;
                }
                v.reset(OpMIPS64MOVVstore);
                v.AuxInt = 16L;
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v1.AuxInt = 8L;
                v1.AddArg(ptr);
                v2 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v2.AuxInt = 0L;
                v1.AddArg(v2);
                v3 = b.NewValue0(v.Pos, OpMIPS64MOVVstore, types.TypeMem);
                v3.AuxInt = 0L;
                v3.AddArg(ptr);
                v4 = b.NewValue0(v.Pos, OpMIPS64MOVVconst, typ.UInt64);
                v4.AuxInt = 0L;
                v3.AddArg(v4);
                v3.AddArg(mem);
                v1.AddArg(v3);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [s] {t} ptr mem)
            // cond: s%8 == 0 && s > 24 && s <= 8*128     && t.(*types.Type).Alignment()%8 == 0 && !config.noDuffDevice
            // result: (DUFFZERO [8 * (128 - int64(s/8))] ptr mem)
 
            // match: (Zero [s] {t} ptr mem)
            // cond: s%8 == 0 && s > 24 && s <= 8*128     && t.(*types.Type).Alignment()%8 == 0 && !config.noDuffDevice
            // result: (DUFFZERO [8 * (128 - int64(s/8))] ptr mem)
            while (true)
            {
                var s = v.AuxInt;
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(s % 8L == 0L && s > 24L && s <= 8L * 128L && t._<ref types.Type>().Alignment() % 8L == 0L && !config.noDuffDevice))
                {
                    break;
                }
                v.reset(OpMIPS64DUFFZERO);
                v.AuxInt = 8L * (128L - int64(s / 8L));
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [s] {t} ptr mem)
            // cond: (s > 8*128 || config.noDuffDevice) || t.(*types.Type).Alignment()%8 != 0
            // result: (LoweredZero [t.(*types.Type).Alignment()]         ptr         (ADDVconst <ptr.Type> ptr [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
 
            // match: (Zero [s] {t} ptr mem)
            // cond: (s > 8*128 || config.noDuffDevice) || t.(*types.Type).Alignment()%8 != 0
            // result: (LoweredZero [t.(*types.Type).Alignment()]         ptr         (ADDVconst <ptr.Type> ptr [s-moveSize(t.(*types.Type).Alignment(), config)])         mem)
            while (true)
            {
                s = v.AuxInt;
                t = v.Aux;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!((s > 8L * 128L || config.noDuffDevice) || t._<ref types.Type>().Alignment() % 8L != 0L))
                {
                    break;
                }
                v.reset(OpMIPS64LoweredZero);
                v.AuxInt = t._<ref types.Type>().Alignment();
                v.AddArg(ptr);
                v0 = b.NewValue0(v.Pos, OpMIPS64ADDVconst, ptr.Type);
                v0.AuxInt = s - moveSize(t._<ref types.Type>().Alignment(), config);
                v0.AddArg(ptr);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValueMIPS64_OpZeroExt16to32_0(ref Value v)
        { 
            // match: (ZeroExt16to32 x)
            // cond:
            // result: (MOVHUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVHUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpZeroExt16to64_0(ref Value v)
        { 
            // match: (ZeroExt16to64 x)
            // cond:
            // result: (MOVHUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVHUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpZeroExt32to64_0(ref Value v)
        { 
            // match: (ZeroExt32to64 x)
            // cond:
            // result: (MOVWUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVWUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpZeroExt8to16_0(ref Value v)
        { 
            // match: (ZeroExt8to16 x)
            // cond:
            // result: (MOVBUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVBUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpZeroExt8to32_0(ref Value v)
        { 
            // match: (ZeroExt8to32 x)
            // cond:
            // result: (MOVBUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVBUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValueMIPS64_OpZeroExt8to64_0(ref Value v)
        { 
            // match: (ZeroExt8to64 x)
            // cond:
            // result: (MOVBUreg x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(OpMIPS64MOVBUreg);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteBlockMIPS64(ref Block b)
        {
            var config = b.Func.Config;
            _ = config;
            var fe = b.Func.fe;
            _ = fe;
            var typ = ref config.Types;
            _ = typ;

            if (b.Kind == BlockMIPS64EQ) 
                // match: (EQ (FPFlagTrue cmp) yes no)
                // cond:
                // result: (FPF cmp yes no)
                while (true)
                {
                    var v = b.Control;
                    if (v.Op != OpMIPS64FPFlagTrue)
                    {
                        break;
                    }
                    var cmp = v.Args[0L];
                    b.Kind = BlockMIPS64FPF;
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
                    if (v.Op != OpMIPS64FPFlagFalse)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = BlockMIPS64FPT;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGT)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPS64NE;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGTU)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPS64NE;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGTconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64NE;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGTUconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64NE;
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
                    if (v.Op != OpMIPS64SGTUconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    var x = v.Args[0L];
                    b.Kind = BlockMIPS64NE;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (SGTU x (MOVVconst [0])) yes no)
                // cond:
                // result: (EQ x yes no)
 
                // match: (EQ (SGTU x (MOVVconst [0])) yes no)
                // cond:
                // result: (EQ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64SGTU)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    x = v.Args[0L];
                    var v_1 = v.Args[1L];
                    if (v_1.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    if (v_1.AuxInt != 0L)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64EQ;
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
                    if (v.Op != OpMIPS64SGTconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 0L)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPS64GEZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (SGT x (MOVVconst [0])) yes no)
                // cond:
                // result: (LEZ x yes no)
 
                // match: (EQ (SGT x (MOVVconst [0])) yes no)
                // cond:
                // result: (LEZ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64SGT)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    x = v.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    if (v_1.AuxInt != 0L)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64LEZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (MOVVconst [0]) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (EQ (MOVVconst [0]) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
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
                // match: (EQ (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First nil no yes)
 
                // match: (EQ (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
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
            else if (b.Kind == BlockMIPS64GEZ) 
                // match: (GEZ (MOVVconst [c]) yes no)
                // cond: c >= 0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c >= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GEZ (MOVVconst [c]) yes no)
                // cond: c <  0
                // result: (First nil no yes)
 
                // match: (GEZ (MOVVconst [c]) yes no)
                // cond: c <  0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c < 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPS64GTZ) 
                // match: (GTZ (MOVVconst [c]) yes no)
                // cond: c >  0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c > 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GTZ (MOVVconst [c]) yes no)
                // cond: c <= 0
                // result: (First nil no yes)
 
                // match: (GTZ (MOVVconst [c]) yes no)
                // cond: c <= 0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c <= 0L))
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
                    b.Kind = BlockMIPS64NE;
                    b.SetControl(cond);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == BlockMIPS64LEZ) 
                // match: (LEZ (MOVVconst [c]) yes no)
                // cond: c <= 0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c <= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LEZ (MOVVconst [c]) yes no)
                // cond: c >  0
                // result: (First nil no yes)
 
                // match: (LEZ (MOVVconst [c]) yes no)
                // cond: c >  0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c > 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPS64LTZ) 
                // match: (LTZ (MOVVconst [c]) yes no)
                // cond: c <  0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c < 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LTZ (MOVVconst [c]) yes no)
                // cond: c >= 0
                // result: (First nil no yes)
 
                // match: (LTZ (MOVVconst [c]) yes no)
                // cond: c >= 0
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    c = v.AuxInt;
                    if (!(c >= 0L))
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == BlockMIPS64NE) 
                // match: (NE (FPFlagTrue cmp) yes no)
                // cond:
                // result: (FPT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64FPFlagTrue)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = BlockMIPS64FPT;
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
                    if (v.Op != OpMIPS64FPFlagFalse)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = BlockMIPS64FPF;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGT)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPS64EQ;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGTU)
                    {
                        break;
                    }
                    _ = cmp.Args[1L];
                    b.Kind = BlockMIPS64EQ;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGTconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64EQ;
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
                    if (v.Op != OpMIPS64XORconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    if (cmp.Op != OpMIPS64SGTUconst)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64EQ;
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
                    if (v.Op != OpMIPS64SGTUconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 1L)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPS64EQ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (SGTU x (MOVVconst [0])) yes no)
                // cond:
                // result: (NE x yes no)
 
                // match: (NE (SGTU x (MOVVconst [0])) yes no)
                // cond:
                // result: (NE x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64SGTU)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    x = v.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    if (v_1.AuxInt != 0L)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64NE;
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
                    if (v.Op != OpMIPS64SGTconst)
                    {
                        break;
                    }
                    if (v.AuxInt != 0L)
                    {
                        break;
                    }
                    x = v.Args[0L];
                    b.Kind = BlockMIPS64LTZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (SGT x (MOVVconst [0])) yes no)
                // cond:
                // result: (GTZ x yes no)
 
                // match: (NE (SGT x (MOVVconst [0])) yes no)
                // cond:
                // result: (GTZ x yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64SGT)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    x = v.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != OpMIPS64MOVVconst)
                    {
                        break;
                    }
                    if (v_1.AuxInt != 0L)
                    {
                        break;
                    }
                    b.Kind = BlockMIPS64GTZ;
                    b.SetControl(x);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (MOVVconst [0]) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (NE (MOVVconst [0]) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
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
                // match: (NE (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First nil yes no)
 
                // match: (NE (MOVVconst [c]) yes no)
                // cond: c != 0
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != OpMIPS64MOVVconst)
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
