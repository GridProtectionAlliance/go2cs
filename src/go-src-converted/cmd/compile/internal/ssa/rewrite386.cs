// Code generated from gen/386.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2020 August 29 08:57:15 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\rewrite386.go
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

        private static bool rewriteValue386(ref Value v)
        {

            if (v.Op == Op386ADCL) 
                return rewriteValue386_Op386ADCL_0(v);
            else if (v.Op == Op386ADDL) 
                return rewriteValue386_Op386ADDL_0(v) || rewriteValue386_Op386ADDL_10(v) || rewriteValue386_Op386ADDL_20(v);
            else if (v.Op == Op386ADDLcarry) 
                return rewriteValue386_Op386ADDLcarry_0(v);
            else if (v.Op == Op386ADDLconst) 
                return rewriteValue386_Op386ADDLconst_0(v);
            else if (v.Op == Op386ANDL) 
                return rewriteValue386_Op386ANDL_0(v);
            else if (v.Op == Op386ANDLconst) 
                return rewriteValue386_Op386ANDLconst_0(v);
            else if (v.Op == Op386CMPB) 
                return rewriteValue386_Op386CMPB_0(v);
            else if (v.Op == Op386CMPBconst) 
                return rewriteValue386_Op386CMPBconst_0(v);
            else if (v.Op == Op386CMPL) 
                return rewriteValue386_Op386CMPL_0(v);
            else if (v.Op == Op386CMPLconst) 
                return rewriteValue386_Op386CMPLconst_0(v);
            else if (v.Op == Op386CMPW) 
                return rewriteValue386_Op386CMPW_0(v);
            else if (v.Op == Op386CMPWconst) 
                return rewriteValue386_Op386CMPWconst_0(v);
            else if (v.Op == Op386LEAL) 
                return rewriteValue386_Op386LEAL_0(v);
            else if (v.Op == Op386LEAL1) 
                return rewriteValue386_Op386LEAL1_0(v);
            else if (v.Op == Op386LEAL2) 
                return rewriteValue386_Op386LEAL2_0(v);
            else if (v.Op == Op386LEAL4) 
                return rewriteValue386_Op386LEAL4_0(v);
            else if (v.Op == Op386LEAL8) 
                return rewriteValue386_Op386LEAL8_0(v);
            else if (v.Op == Op386MOVBLSX) 
                return rewriteValue386_Op386MOVBLSX_0(v);
            else if (v.Op == Op386MOVBLSXload) 
                return rewriteValue386_Op386MOVBLSXload_0(v);
            else if (v.Op == Op386MOVBLZX) 
                return rewriteValue386_Op386MOVBLZX_0(v);
            else if (v.Op == Op386MOVBload) 
                return rewriteValue386_Op386MOVBload_0(v);
            else if (v.Op == Op386MOVBloadidx1) 
                return rewriteValue386_Op386MOVBloadidx1_0(v);
            else if (v.Op == Op386MOVBstore) 
                return rewriteValue386_Op386MOVBstore_0(v);
            else if (v.Op == Op386MOVBstoreconst) 
                return rewriteValue386_Op386MOVBstoreconst_0(v);
            else if (v.Op == Op386MOVBstoreconstidx1) 
                return rewriteValue386_Op386MOVBstoreconstidx1_0(v);
            else if (v.Op == Op386MOVBstoreidx1) 
                return rewriteValue386_Op386MOVBstoreidx1_0(v) || rewriteValue386_Op386MOVBstoreidx1_10(v);
            else if (v.Op == Op386MOVLload) 
                return rewriteValue386_Op386MOVLload_0(v);
            else if (v.Op == Op386MOVLloadidx1) 
                return rewriteValue386_Op386MOVLloadidx1_0(v);
            else if (v.Op == Op386MOVLloadidx4) 
                return rewriteValue386_Op386MOVLloadidx4_0(v);
            else if (v.Op == Op386MOVLstore) 
                return rewriteValue386_Op386MOVLstore_0(v);
            else if (v.Op == Op386MOVLstoreconst) 
                return rewriteValue386_Op386MOVLstoreconst_0(v);
            else if (v.Op == Op386MOVLstoreconstidx1) 
                return rewriteValue386_Op386MOVLstoreconstidx1_0(v);
            else if (v.Op == Op386MOVLstoreconstidx4) 
                return rewriteValue386_Op386MOVLstoreconstidx4_0(v);
            else if (v.Op == Op386MOVLstoreidx1) 
                return rewriteValue386_Op386MOVLstoreidx1_0(v);
            else if (v.Op == Op386MOVLstoreidx4) 
                return rewriteValue386_Op386MOVLstoreidx4_0(v);
            else if (v.Op == Op386MOVSDconst) 
                return rewriteValue386_Op386MOVSDconst_0(v);
            else if (v.Op == Op386MOVSDload) 
                return rewriteValue386_Op386MOVSDload_0(v);
            else if (v.Op == Op386MOVSDloadidx1) 
                return rewriteValue386_Op386MOVSDloadidx1_0(v);
            else if (v.Op == Op386MOVSDloadidx8) 
                return rewriteValue386_Op386MOVSDloadidx8_0(v);
            else if (v.Op == Op386MOVSDstore) 
                return rewriteValue386_Op386MOVSDstore_0(v);
            else if (v.Op == Op386MOVSDstoreidx1) 
                return rewriteValue386_Op386MOVSDstoreidx1_0(v);
            else if (v.Op == Op386MOVSDstoreidx8) 
                return rewriteValue386_Op386MOVSDstoreidx8_0(v);
            else if (v.Op == Op386MOVSSconst) 
                return rewriteValue386_Op386MOVSSconst_0(v);
            else if (v.Op == Op386MOVSSload) 
                return rewriteValue386_Op386MOVSSload_0(v);
            else if (v.Op == Op386MOVSSloadidx1) 
                return rewriteValue386_Op386MOVSSloadidx1_0(v);
            else if (v.Op == Op386MOVSSloadidx4) 
                return rewriteValue386_Op386MOVSSloadidx4_0(v);
            else if (v.Op == Op386MOVSSstore) 
                return rewriteValue386_Op386MOVSSstore_0(v);
            else if (v.Op == Op386MOVSSstoreidx1) 
                return rewriteValue386_Op386MOVSSstoreidx1_0(v);
            else if (v.Op == Op386MOVSSstoreidx4) 
                return rewriteValue386_Op386MOVSSstoreidx4_0(v);
            else if (v.Op == Op386MOVWLSX) 
                return rewriteValue386_Op386MOVWLSX_0(v);
            else if (v.Op == Op386MOVWLSXload) 
                return rewriteValue386_Op386MOVWLSXload_0(v);
            else if (v.Op == Op386MOVWLZX) 
                return rewriteValue386_Op386MOVWLZX_0(v);
            else if (v.Op == Op386MOVWload) 
                return rewriteValue386_Op386MOVWload_0(v);
            else if (v.Op == Op386MOVWloadidx1) 
                return rewriteValue386_Op386MOVWloadidx1_0(v);
            else if (v.Op == Op386MOVWloadidx2) 
                return rewriteValue386_Op386MOVWloadidx2_0(v);
            else if (v.Op == Op386MOVWstore) 
                return rewriteValue386_Op386MOVWstore_0(v);
            else if (v.Op == Op386MOVWstoreconst) 
                return rewriteValue386_Op386MOVWstoreconst_0(v);
            else if (v.Op == Op386MOVWstoreconstidx1) 
                return rewriteValue386_Op386MOVWstoreconstidx1_0(v);
            else if (v.Op == Op386MOVWstoreconstidx2) 
                return rewriteValue386_Op386MOVWstoreconstidx2_0(v);
            else if (v.Op == Op386MOVWstoreidx1) 
                return rewriteValue386_Op386MOVWstoreidx1_0(v) || rewriteValue386_Op386MOVWstoreidx1_10(v);
            else if (v.Op == Op386MOVWstoreidx2) 
                return rewriteValue386_Op386MOVWstoreidx2_0(v);
            else if (v.Op == Op386MULL) 
                return rewriteValue386_Op386MULL_0(v);
            else if (v.Op == Op386MULLconst) 
                return rewriteValue386_Op386MULLconst_0(v) || rewriteValue386_Op386MULLconst_10(v) || rewriteValue386_Op386MULLconst_20(v);
            else if (v.Op == Op386NEGL) 
                return rewriteValue386_Op386NEGL_0(v);
            else if (v.Op == Op386NOTL) 
                return rewriteValue386_Op386NOTL_0(v);
            else if (v.Op == Op386ORL) 
                return rewriteValue386_Op386ORL_0(v) || rewriteValue386_Op386ORL_10(v) || rewriteValue386_Op386ORL_20(v) || rewriteValue386_Op386ORL_30(v) || rewriteValue386_Op386ORL_40(v) || rewriteValue386_Op386ORL_50(v);
            else if (v.Op == Op386ORLconst) 
                return rewriteValue386_Op386ORLconst_0(v);
            else if (v.Op == Op386ROLBconst) 
                return rewriteValue386_Op386ROLBconst_0(v);
            else if (v.Op == Op386ROLLconst) 
                return rewriteValue386_Op386ROLLconst_0(v);
            else if (v.Op == Op386ROLWconst) 
                return rewriteValue386_Op386ROLWconst_0(v);
            else if (v.Op == Op386SARB) 
                return rewriteValue386_Op386SARB_0(v);
            else if (v.Op == Op386SARBconst) 
                return rewriteValue386_Op386SARBconst_0(v);
            else if (v.Op == Op386SARL) 
                return rewriteValue386_Op386SARL_0(v);
            else if (v.Op == Op386SARLconst) 
                return rewriteValue386_Op386SARLconst_0(v);
            else if (v.Op == Op386SARW) 
                return rewriteValue386_Op386SARW_0(v);
            else if (v.Op == Op386SARWconst) 
                return rewriteValue386_Op386SARWconst_0(v);
            else if (v.Op == Op386SBBL) 
                return rewriteValue386_Op386SBBL_0(v);
            else if (v.Op == Op386SBBLcarrymask) 
                return rewriteValue386_Op386SBBLcarrymask_0(v);
            else if (v.Op == Op386SETA) 
                return rewriteValue386_Op386SETA_0(v);
            else if (v.Op == Op386SETAE) 
                return rewriteValue386_Op386SETAE_0(v);
            else if (v.Op == Op386SETB) 
                return rewriteValue386_Op386SETB_0(v);
            else if (v.Op == Op386SETBE) 
                return rewriteValue386_Op386SETBE_0(v);
            else if (v.Op == Op386SETEQ) 
                return rewriteValue386_Op386SETEQ_0(v);
            else if (v.Op == Op386SETG) 
                return rewriteValue386_Op386SETG_0(v);
            else if (v.Op == Op386SETGE) 
                return rewriteValue386_Op386SETGE_0(v);
            else if (v.Op == Op386SETL) 
                return rewriteValue386_Op386SETL_0(v);
            else if (v.Op == Op386SETLE) 
                return rewriteValue386_Op386SETLE_0(v);
            else if (v.Op == Op386SETNE) 
                return rewriteValue386_Op386SETNE_0(v);
            else if (v.Op == Op386SHLL) 
                return rewriteValue386_Op386SHLL_0(v);
            else if (v.Op == Op386SHLLconst) 
                return rewriteValue386_Op386SHLLconst_0(v);
            else if (v.Op == Op386SHRB) 
                return rewriteValue386_Op386SHRB_0(v);
            else if (v.Op == Op386SHRBconst) 
                return rewriteValue386_Op386SHRBconst_0(v);
            else if (v.Op == Op386SHRL) 
                return rewriteValue386_Op386SHRL_0(v);
            else if (v.Op == Op386SHRLconst) 
                return rewriteValue386_Op386SHRLconst_0(v);
            else if (v.Op == Op386SHRW) 
                return rewriteValue386_Op386SHRW_0(v);
            else if (v.Op == Op386SHRWconst) 
                return rewriteValue386_Op386SHRWconst_0(v);
            else if (v.Op == Op386SUBL) 
                return rewriteValue386_Op386SUBL_0(v);
            else if (v.Op == Op386SUBLcarry) 
                return rewriteValue386_Op386SUBLcarry_0(v);
            else if (v.Op == Op386SUBLconst) 
                return rewriteValue386_Op386SUBLconst_0(v);
            else if (v.Op == Op386XORL) 
                return rewriteValue386_Op386XORL_0(v);
            else if (v.Op == Op386XORLconst) 
                return rewriteValue386_Op386XORLconst_0(v);
            else if (v.Op == OpAdd16) 
                return rewriteValue386_OpAdd16_0(v);
            else if (v.Op == OpAdd32) 
                return rewriteValue386_OpAdd32_0(v);
            else if (v.Op == OpAdd32F) 
                return rewriteValue386_OpAdd32F_0(v);
            else if (v.Op == OpAdd32carry) 
                return rewriteValue386_OpAdd32carry_0(v);
            else if (v.Op == OpAdd32withcarry) 
                return rewriteValue386_OpAdd32withcarry_0(v);
            else if (v.Op == OpAdd64F) 
                return rewriteValue386_OpAdd64F_0(v);
            else if (v.Op == OpAdd8) 
                return rewriteValue386_OpAdd8_0(v);
            else if (v.Op == OpAddPtr) 
                return rewriteValue386_OpAddPtr_0(v);
            else if (v.Op == OpAddr) 
                return rewriteValue386_OpAddr_0(v);
            else if (v.Op == OpAnd16) 
                return rewriteValue386_OpAnd16_0(v);
            else if (v.Op == OpAnd32) 
                return rewriteValue386_OpAnd32_0(v);
            else if (v.Op == OpAnd8) 
                return rewriteValue386_OpAnd8_0(v);
            else if (v.Op == OpAndB) 
                return rewriteValue386_OpAndB_0(v);
            else if (v.Op == OpAvg32u) 
                return rewriteValue386_OpAvg32u_0(v);
            else if (v.Op == OpBswap32) 
                return rewriteValue386_OpBswap32_0(v);
            else if (v.Op == OpClosureCall) 
                return rewriteValue386_OpClosureCall_0(v);
            else if (v.Op == OpCom16) 
                return rewriteValue386_OpCom16_0(v);
            else if (v.Op == OpCom32) 
                return rewriteValue386_OpCom32_0(v);
            else if (v.Op == OpCom8) 
                return rewriteValue386_OpCom8_0(v);
            else if (v.Op == OpConst16) 
                return rewriteValue386_OpConst16_0(v);
            else if (v.Op == OpConst32) 
                return rewriteValue386_OpConst32_0(v);
            else if (v.Op == OpConst32F) 
                return rewriteValue386_OpConst32F_0(v);
            else if (v.Op == OpConst64F) 
                return rewriteValue386_OpConst64F_0(v);
            else if (v.Op == OpConst8) 
                return rewriteValue386_OpConst8_0(v);
            else if (v.Op == OpConstBool) 
                return rewriteValue386_OpConstBool_0(v);
            else if (v.Op == OpConstNil) 
                return rewriteValue386_OpConstNil_0(v);
            else if (v.Op == OpConvert) 
                return rewriteValue386_OpConvert_0(v);
            else if (v.Op == OpCvt32Fto32) 
                return rewriteValue386_OpCvt32Fto32_0(v);
            else if (v.Op == OpCvt32Fto64F) 
                return rewriteValue386_OpCvt32Fto64F_0(v);
            else if (v.Op == OpCvt32to32F) 
                return rewriteValue386_OpCvt32to32F_0(v);
            else if (v.Op == OpCvt32to64F) 
                return rewriteValue386_OpCvt32to64F_0(v);
            else if (v.Op == OpCvt64Fto32) 
                return rewriteValue386_OpCvt64Fto32_0(v);
            else if (v.Op == OpCvt64Fto32F) 
                return rewriteValue386_OpCvt64Fto32F_0(v);
            else if (v.Op == OpDiv16) 
                return rewriteValue386_OpDiv16_0(v);
            else if (v.Op == OpDiv16u) 
                return rewriteValue386_OpDiv16u_0(v);
            else if (v.Op == OpDiv32) 
                return rewriteValue386_OpDiv32_0(v);
            else if (v.Op == OpDiv32F) 
                return rewriteValue386_OpDiv32F_0(v);
            else if (v.Op == OpDiv32u) 
                return rewriteValue386_OpDiv32u_0(v);
            else if (v.Op == OpDiv64F) 
                return rewriteValue386_OpDiv64F_0(v);
            else if (v.Op == OpDiv8) 
                return rewriteValue386_OpDiv8_0(v);
            else if (v.Op == OpDiv8u) 
                return rewriteValue386_OpDiv8u_0(v);
            else if (v.Op == OpEq16) 
                return rewriteValue386_OpEq16_0(v);
            else if (v.Op == OpEq32) 
                return rewriteValue386_OpEq32_0(v);
            else if (v.Op == OpEq32F) 
                return rewriteValue386_OpEq32F_0(v);
            else if (v.Op == OpEq64F) 
                return rewriteValue386_OpEq64F_0(v);
            else if (v.Op == OpEq8) 
                return rewriteValue386_OpEq8_0(v);
            else if (v.Op == OpEqB) 
                return rewriteValue386_OpEqB_0(v);
            else if (v.Op == OpEqPtr) 
                return rewriteValue386_OpEqPtr_0(v);
            else if (v.Op == OpGeq16) 
                return rewriteValue386_OpGeq16_0(v);
            else if (v.Op == OpGeq16U) 
                return rewriteValue386_OpGeq16U_0(v);
            else if (v.Op == OpGeq32) 
                return rewriteValue386_OpGeq32_0(v);
            else if (v.Op == OpGeq32F) 
                return rewriteValue386_OpGeq32F_0(v);
            else if (v.Op == OpGeq32U) 
                return rewriteValue386_OpGeq32U_0(v);
            else if (v.Op == OpGeq64F) 
                return rewriteValue386_OpGeq64F_0(v);
            else if (v.Op == OpGeq8) 
                return rewriteValue386_OpGeq8_0(v);
            else if (v.Op == OpGeq8U) 
                return rewriteValue386_OpGeq8U_0(v);
            else if (v.Op == OpGetCallerPC) 
                return rewriteValue386_OpGetCallerPC_0(v);
            else if (v.Op == OpGetCallerSP) 
                return rewriteValue386_OpGetCallerSP_0(v);
            else if (v.Op == OpGetClosurePtr) 
                return rewriteValue386_OpGetClosurePtr_0(v);
            else if (v.Op == OpGetG) 
                return rewriteValue386_OpGetG_0(v);
            else if (v.Op == OpGreater16) 
                return rewriteValue386_OpGreater16_0(v);
            else if (v.Op == OpGreater16U) 
                return rewriteValue386_OpGreater16U_0(v);
            else if (v.Op == OpGreater32) 
                return rewriteValue386_OpGreater32_0(v);
            else if (v.Op == OpGreater32F) 
                return rewriteValue386_OpGreater32F_0(v);
            else if (v.Op == OpGreater32U) 
                return rewriteValue386_OpGreater32U_0(v);
            else if (v.Op == OpGreater64F) 
                return rewriteValue386_OpGreater64F_0(v);
            else if (v.Op == OpGreater8) 
                return rewriteValue386_OpGreater8_0(v);
            else if (v.Op == OpGreater8U) 
                return rewriteValue386_OpGreater8U_0(v);
            else if (v.Op == OpHmul32) 
                return rewriteValue386_OpHmul32_0(v);
            else if (v.Op == OpHmul32u) 
                return rewriteValue386_OpHmul32u_0(v);
            else if (v.Op == OpInterCall) 
                return rewriteValue386_OpInterCall_0(v);
            else if (v.Op == OpIsInBounds) 
                return rewriteValue386_OpIsInBounds_0(v);
            else if (v.Op == OpIsNonNil) 
                return rewriteValue386_OpIsNonNil_0(v);
            else if (v.Op == OpIsSliceInBounds) 
                return rewriteValue386_OpIsSliceInBounds_0(v);
            else if (v.Op == OpLeq16) 
                return rewriteValue386_OpLeq16_0(v);
            else if (v.Op == OpLeq16U) 
                return rewriteValue386_OpLeq16U_0(v);
            else if (v.Op == OpLeq32) 
                return rewriteValue386_OpLeq32_0(v);
            else if (v.Op == OpLeq32F) 
                return rewriteValue386_OpLeq32F_0(v);
            else if (v.Op == OpLeq32U) 
                return rewriteValue386_OpLeq32U_0(v);
            else if (v.Op == OpLeq64F) 
                return rewriteValue386_OpLeq64F_0(v);
            else if (v.Op == OpLeq8) 
                return rewriteValue386_OpLeq8_0(v);
            else if (v.Op == OpLeq8U) 
                return rewriteValue386_OpLeq8U_0(v);
            else if (v.Op == OpLess16) 
                return rewriteValue386_OpLess16_0(v);
            else if (v.Op == OpLess16U) 
                return rewriteValue386_OpLess16U_0(v);
            else if (v.Op == OpLess32) 
                return rewriteValue386_OpLess32_0(v);
            else if (v.Op == OpLess32F) 
                return rewriteValue386_OpLess32F_0(v);
            else if (v.Op == OpLess32U) 
                return rewriteValue386_OpLess32U_0(v);
            else if (v.Op == OpLess64F) 
                return rewriteValue386_OpLess64F_0(v);
            else if (v.Op == OpLess8) 
                return rewriteValue386_OpLess8_0(v);
            else if (v.Op == OpLess8U) 
                return rewriteValue386_OpLess8U_0(v);
            else if (v.Op == OpLoad) 
                return rewriteValue386_OpLoad_0(v);
            else if (v.Op == OpLsh16x16) 
                return rewriteValue386_OpLsh16x16_0(v);
            else if (v.Op == OpLsh16x32) 
                return rewriteValue386_OpLsh16x32_0(v);
            else if (v.Op == OpLsh16x64) 
                return rewriteValue386_OpLsh16x64_0(v);
            else if (v.Op == OpLsh16x8) 
                return rewriteValue386_OpLsh16x8_0(v);
            else if (v.Op == OpLsh32x16) 
                return rewriteValue386_OpLsh32x16_0(v);
            else if (v.Op == OpLsh32x32) 
                return rewriteValue386_OpLsh32x32_0(v);
            else if (v.Op == OpLsh32x64) 
                return rewriteValue386_OpLsh32x64_0(v);
            else if (v.Op == OpLsh32x8) 
                return rewriteValue386_OpLsh32x8_0(v);
            else if (v.Op == OpLsh8x16) 
                return rewriteValue386_OpLsh8x16_0(v);
            else if (v.Op == OpLsh8x32) 
                return rewriteValue386_OpLsh8x32_0(v);
            else if (v.Op == OpLsh8x64) 
                return rewriteValue386_OpLsh8x64_0(v);
            else if (v.Op == OpLsh8x8) 
                return rewriteValue386_OpLsh8x8_0(v);
            else if (v.Op == OpMod16) 
                return rewriteValue386_OpMod16_0(v);
            else if (v.Op == OpMod16u) 
                return rewriteValue386_OpMod16u_0(v);
            else if (v.Op == OpMod32) 
                return rewriteValue386_OpMod32_0(v);
            else if (v.Op == OpMod32u) 
                return rewriteValue386_OpMod32u_0(v);
            else if (v.Op == OpMod8) 
                return rewriteValue386_OpMod8_0(v);
            else if (v.Op == OpMod8u) 
                return rewriteValue386_OpMod8u_0(v);
            else if (v.Op == OpMove) 
                return rewriteValue386_OpMove_0(v) || rewriteValue386_OpMove_10(v);
            else if (v.Op == OpMul16) 
                return rewriteValue386_OpMul16_0(v);
            else if (v.Op == OpMul32) 
                return rewriteValue386_OpMul32_0(v);
            else if (v.Op == OpMul32F) 
                return rewriteValue386_OpMul32F_0(v);
            else if (v.Op == OpMul32uhilo) 
                return rewriteValue386_OpMul32uhilo_0(v);
            else if (v.Op == OpMul64F) 
                return rewriteValue386_OpMul64F_0(v);
            else if (v.Op == OpMul8) 
                return rewriteValue386_OpMul8_0(v);
            else if (v.Op == OpNeg16) 
                return rewriteValue386_OpNeg16_0(v);
            else if (v.Op == OpNeg32) 
                return rewriteValue386_OpNeg32_0(v);
            else if (v.Op == OpNeg32F) 
                return rewriteValue386_OpNeg32F_0(v);
            else if (v.Op == OpNeg64F) 
                return rewriteValue386_OpNeg64F_0(v);
            else if (v.Op == OpNeg8) 
                return rewriteValue386_OpNeg8_0(v);
            else if (v.Op == OpNeq16) 
                return rewriteValue386_OpNeq16_0(v);
            else if (v.Op == OpNeq32) 
                return rewriteValue386_OpNeq32_0(v);
            else if (v.Op == OpNeq32F) 
                return rewriteValue386_OpNeq32F_0(v);
            else if (v.Op == OpNeq64F) 
                return rewriteValue386_OpNeq64F_0(v);
            else if (v.Op == OpNeq8) 
                return rewriteValue386_OpNeq8_0(v);
            else if (v.Op == OpNeqB) 
                return rewriteValue386_OpNeqB_0(v);
            else if (v.Op == OpNeqPtr) 
                return rewriteValue386_OpNeqPtr_0(v);
            else if (v.Op == OpNilCheck) 
                return rewriteValue386_OpNilCheck_0(v);
            else if (v.Op == OpNot) 
                return rewriteValue386_OpNot_0(v);
            else if (v.Op == OpOffPtr) 
                return rewriteValue386_OpOffPtr_0(v);
            else if (v.Op == OpOr16) 
                return rewriteValue386_OpOr16_0(v);
            else if (v.Op == OpOr32) 
                return rewriteValue386_OpOr32_0(v);
            else if (v.Op == OpOr8) 
                return rewriteValue386_OpOr8_0(v);
            else if (v.Op == OpOrB) 
                return rewriteValue386_OpOrB_0(v);
            else if (v.Op == OpRound32F) 
                return rewriteValue386_OpRound32F_0(v);
            else if (v.Op == OpRound64F) 
                return rewriteValue386_OpRound64F_0(v);
            else if (v.Op == OpRsh16Ux16) 
                return rewriteValue386_OpRsh16Ux16_0(v);
            else if (v.Op == OpRsh16Ux32) 
                return rewriteValue386_OpRsh16Ux32_0(v);
            else if (v.Op == OpRsh16Ux64) 
                return rewriteValue386_OpRsh16Ux64_0(v);
            else if (v.Op == OpRsh16Ux8) 
                return rewriteValue386_OpRsh16Ux8_0(v);
            else if (v.Op == OpRsh16x16) 
                return rewriteValue386_OpRsh16x16_0(v);
            else if (v.Op == OpRsh16x32) 
                return rewriteValue386_OpRsh16x32_0(v);
            else if (v.Op == OpRsh16x64) 
                return rewriteValue386_OpRsh16x64_0(v);
            else if (v.Op == OpRsh16x8) 
                return rewriteValue386_OpRsh16x8_0(v);
            else if (v.Op == OpRsh32Ux16) 
                return rewriteValue386_OpRsh32Ux16_0(v);
            else if (v.Op == OpRsh32Ux32) 
                return rewriteValue386_OpRsh32Ux32_0(v);
            else if (v.Op == OpRsh32Ux64) 
                return rewriteValue386_OpRsh32Ux64_0(v);
            else if (v.Op == OpRsh32Ux8) 
                return rewriteValue386_OpRsh32Ux8_0(v);
            else if (v.Op == OpRsh32x16) 
                return rewriteValue386_OpRsh32x16_0(v);
            else if (v.Op == OpRsh32x32) 
                return rewriteValue386_OpRsh32x32_0(v);
            else if (v.Op == OpRsh32x64) 
                return rewriteValue386_OpRsh32x64_0(v);
            else if (v.Op == OpRsh32x8) 
                return rewriteValue386_OpRsh32x8_0(v);
            else if (v.Op == OpRsh8Ux16) 
                return rewriteValue386_OpRsh8Ux16_0(v);
            else if (v.Op == OpRsh8Ux32) 
                return rewriteValue386_OpRsh8Ux32_0(v);
            else if (v.Op == OpRsh8Ux64) 
                return rewriteValue386_OpRsh8Ux64_0(v);
            else if (v.Op == OpRsh8Ux8) 
                return rewriteValue386_OpRsh8Ux8_0(v);
            else if (v.Op == OpRsh8x16) 
                return rewriteValue386_OpRsh8x16_0(v);
            else if (v.Op == OpRsh8x32) 
                return rewriteValue386_OpRsh8x32_0(v);
            else if (v.Op == OpRsh8x64) 
                return rewriteValue386_OpRsh8x64_0(v);
            else if (v.Op == OpRsh8x8) 
                return rewriteValue386_OpRsh8x8_0(v);
            else if (v.Op == OpSignExt16to32) 
                return rewriteValue386_OpSignExt16to32_0(v);
            else if (v.Op == OpSignExt8to16) 
                return rewriteValue386_OpSignExt8to16_0(v);
            else if (v.Op == OpSignExt8to32) 
                return rewriteValue386_OpSignExt8to32_0(v);
            else if (v.Op == OpSignmask) 
                return rewriteValue386_OpSignmask_0(v);
            else if (v.Op == OpSlicemask) 
                return rewriteValue386_OpSlicemask_0(v);
            else if (v.Op == OpSqrt) 
                return rewriteValue386_OpSqrt_0(v);
            else if (v.Op == OpStaticCall) 
                return rewriteValue386_OpStaticCall_0(v);
            else if (v.Op == OpStore) 
                return rewriteValue386_OpStore_0(v);
            else if (v.Op == OpSub16) 
                return rewriteValue386_OpSub16_0(v);
            else if (v.Op == OpSub32) 
                return rewriteValue386_OpSub32_0(v);
            else if (v.Op == OpSub32F) 
                return rewriteValue386_OpSub32F_0(v);
            else if (v.Op == OpSub32carry) 
                return rewriteValue386_OpSub32carry_0(v);
            else if (v.Op == OpSub32withcarry) 
                return rewriteValue386_OpSub32withcarry_0(v);
            else if (v.Op == OpSub64F) 
                return rewriteValue386_OpSub64F_0(v);
            else if (v.Op == OpSub8) 
                return rewriteValue386_OpSub8_0(v);
            else if (v.Op == OpSubPtr) 
                return rewriteValue386_OpSubPtr_0(v);
            else if (v.Op == OpTrunc16to8) 
                return rewriteValue386_OpTrunc16to8_0(v);
            else if (v.Op == OpTrunc32to16) 
                return rewriteValue386_OpTrunc32to16_0(v);
            else if (v.Op == OpTrunc32to8) 
                return rewriteValue386_OpTrunc32to8_0(v);
            else if (v.Op == OpXor16) 
                return rewriteValue386_OpXor16_0(v);
            else if (v.Op == OpXor32) 
                return rewriteValue386_OpXor32_0(v);
            else if (v.Op == OpXor8) 
                return rewriteValue386_OpXor8_0(v);
            else if (v.Op == OpZero) 
                return rewriteValue386_OpZero_0(v) || rewriteValue386_OpZero_10(v);
            else if (v.Op == OpZeroExt16to32) 
                return rewriteValue386_OpZeroExt16to32_0(v);
            else if (v.Op == OpZeroExt8to16) 
                return rewriteValue386_OpZeroExt8to16_0(v);
            else if (v.Op == OpZeroExt8to32) 
                return rewriteValue386_OpZeroExt8to32_0(v);
            else if (v.Op == OpZeromask) 
                return rewriteValue386_OpZeromask_0(v);
                        return false;
        }
        private static bool rewriteValue386_Op386ADCL_0(ref Value v)
        { 
            // match: (ADCL x (MOVLconst [c]) f)
            // cond:
            // result: (ADCLconst [c] x f)
            while (true)
            {
                _ = v.Args[2L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                var f = v.Args[2L];
                v.reset(Op386ADCLconst);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(f);
                return true;
            } 
            // match: (ADCL (MOVLconst [c]) x f)
            // cond:
            // result: (ADCLconst [c] x f)
 
            // match: (ADCL (MOVLconst [c]) x f)
            // cond:
            // result: (ADCLconst [c] x f)
            while (true)
            {
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                f = v.Args[2L];
                v.reset(Op386ADCLconst);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(f);
                return true;
            } 
            // match: (ADCL (MOVLconst [c]) x f)
            // cond:
            // result: (ADCLconst [c] x f)
 
            // match: (ADCL (MOVLconst [c]) x f)
            // cond:
            // result: (ADCLconst [c] x f)
            while (true)
            {
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                f = v.Args[2L];
                v.reset(Op386ADCLconst);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(f);
                return true;
            } 
            // match: (ADCL x (MOVLconst [c]) f)
            // cond:
            // result: (ADCLconst [c] x f)
 
            // match: (ADCL x (MOVLconst [c]) f)
            // cond:
            // result: (ADCLconst [c] x f)
            while (true)
            {
                _ = v.Args[2L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                f = v.Args[2L];
                v.reset(Op386ADCLconst);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(f);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ADDL_0(ref Value v)
        { 
            // match: (ADDL x (MOVLconst [c]))
            // cond:
            // result: (ADDLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386ADDLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL (MOVLconst [c]) x)
            // cond:
            // result: (ADDLconst [c] x)
 
            // match: (ADDL (MOVLconst [c]) x)
            // cond:
            // result: (ADDLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386ADDLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL (SHLLconst [c] x) (SHRLconst [d] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
 
            // match: (ADDL (SHLLconst [c] x) (SHRLconst [d] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                var d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(d == 32L - c))
                {
                    break;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL (SHRLconst [d] x) (SHLLconst [c] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
 
            // match: (ADDL (SHRLconst [d] x) (SHLLconst [c] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(d == 32L - c))
                {
                    break;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL <t> (SHLLconst x [c]) (SHRWconst x [d]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
 
            // match: (ADDL <t> (SHLLconst x [c]) (SHRWconst x [d]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRWconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 16L && d == 16L - c && t.Size() == 2L))
                {
                    break;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL <t> (SHRWconst x [d]) (SHLLconst x [c]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
 
            // match: (ADDL <t> (SHRWconst x [d]) (SHLLconst x [c]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 16L && d == 16L - c && t.Size() == 2L))
                {
                    break;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL <t> (SHLLconst x [c]) (SHRBconst x [d]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
 
            // match: (ADDL <t> (SHLLconst x [c]) (SHRBconst x [d]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRBconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 8L && d == 8L - c && t.Size() == 1L))
                {
                    break;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL <t> (SHRBconst x [d]) (SHLLconst x [c]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
 
            // match: (ADDL <t> (SHRBconst x [d]) (SHLLconst x [c]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRBconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 8L && d == 8L - c && t.Size() == 1L))
                {
                    break;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL x (SHLLconst [3] y))
            // cond:
            // result: (LEAL8 x y)
 
            // match: (ADDL x (SHLLconst [3] y))
            // cond:
            // result: (LEAL8 x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 3L)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(Op386LEAL8);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL (SHLLconst [3] y) x)
            // cond:
            // result: (LEAL8 x y)
 
            // match: (ADDL (SHLLconst [3] y) x)
            // cond:
            // result: (LEAL8 x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 3L)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386LEAL8);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ADDL_10(ref Value v)
        { 
            // match: (ADDL x (SHLLconst [2] y))
            // cond:
            // result: (LEAL4 x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 2L)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(Op386LEAL4);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL (SHLLconst [2] y) x)
            // cond:
            // result: (LEAL4 x y)
 
            // match: (ADDL (SHLLconst [2] y) x)
            // cond:
            // result: (LEAL4 x y)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 2L)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386LEAL4);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL x (SHLLconst [1] y))
            // cond:
            // result: (LEAL2 x y)
 
            // match: (ADDL x (SHLLconst [1] y))
            // cond:
            // result: (LEAL2 x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL (SHLLconst [1] y) x)
            // cond:
            // result: (LEAL2 x y)
 
            // match: (ADDL (SHLLconst [1] y) x)
            // cond:
            // result: (LEAL2 x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386LEAL2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL x (ADDL y y))
            // cond:
            // result: (LEAL2 x y)
 
            // match: (ADDL x (ADDL y y))
            // cond:
            // result: (LEAL2 x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_1.Args[1L];
                y = v_1.Args[0L];
                if (y != v_1.Args[1L])
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL (ADDL y y) x)
            // cond:
            // result: (LEAL2 x y)
 
            // match: (ADDL (ADDL y y) x)
            // cond:
            // result: (LEAL2 x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                y = v_0.Args[0L];
                if (y != v_0.Args[1L])
                {
                    break;
                }
                x = v.Args[1L];
                v.reset(Op386LEAL2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL x (ADDL x y))
            // cond:
            // result: (LEAL2 y x)
 
            // match: (ADDL x (ADDL x y))
            // cond:
            // result: (LEAL2 y x)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_1.Args[1L];
                if (x != v_1.Args[0L])
                {
                    break;
                }
                y = v_1.Args[1L];
                v.reset(Op386LEAL2);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL x (ADDL y x))
            // cond:
            // result: (LEAL2 y x)
 
            // match: (ADDL x (ADDL y x))
            // cond:
            // result: (LEAL2 y x)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_1.Args[1L];
                y = v_1.Args[0L];
                if (x != v_1.Args[1L])
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL (ADDL x y) x)
            // cond:
            // result: (LEAL2 y x)
 
            // match: (ADDL (ADDL x y) x)
            // cond:
            // result: (LEAL2 y x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            } 
            // match: (ADDL (ADDL y x) x)
            // cond:
            // result: (LEAL2 y x)
 
            // match: (ADDL (ADDL y x) x)
            // cond:
            // result: (LEAL2 y x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                y = v_0.Args[0L];
                x = v_0.Args[1L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AddArg(y);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ADDL_20(ref Value v)
        { 
            // match: (ADDL (ADDLconst [c] x) y)
            // cond:
            // result: (LEAL1 [c] x y)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var x = v_0.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386LEAL1);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL y (ADDLconst [c] x))
            // cond:
            // result: (LEAL1 [c] x y)
 
            // match: (ADDL y (ADDLconst [c] x))
            // cond:
            // result: (LEAL1 [c] x y)
            while (true)
            {
                _ = v.Args[1L];
                y = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                x = v_1.Args[0L];
                v.reset(Op386LEAL1);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL x (LEAL [c] {s} y))
            // cond: x.Op != OpSB && y.Op != OpSB
            // result: (LEAL1 [c] {s} x y)
 
            // match: (ADDL x (LEAL [c] {s} y))
            // cond: x.Op != OpSB && y.Op != OpSB
            // result: (LEAL1 [c] {s} x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386LEAL)
                {
                    break;
                }
                c = v_1.AuxInt;
                var s = v_1.Aux;
                y = v_1.Args[0L];
                if (!(x.Op != OpSB && y.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL (LEAL [c] {s} y) x)
            // cond: x.Op != OpSB && y.Op != OpSB
            // result: (LEAL1 [c] {s} x y)
 
            // match: (ADDL (LEAL [c] {s} y) x)
            // cond: x.Op != OpSB && y.Op != OpSB
            // result: (LEAL1 [c] {s} x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                c = v_0.AuxInt;
                s = v_0.Aux;
                y = v_0.Args[0L];
                x = v.Args[1L];
                if (!(x.Op != OpSB && y.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL x (NEGL y))
            // cond:
            // result: (SUBL x y)
 
            // match: (ADDL x (NEGL y))
            // cond:
            // result: (SUBL x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386NEGL)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386SUBL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDL (NEGL y) x)
            // cond:
            // result: (SUBL x y)
 
            // match: (ADDL (NEGL y) x)
            // cond:
            // result: (SUBL x y)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386NEGL)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386SUBL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ADDLcarry_0(ref Value v)
        { 
            // match: (ADDLcarry x (MOVLconst [c]))
            // cond:
            // result: (ADDLconstcarry [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386ADDLconstcarry);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDLcarry (MOVLconst [c]) x)
            // cond:
            // result: (ADDLconstcarry [c] x)
 
            // match: (ADDLcarry (MOVLconst [c]) x)
            // cond:
            // result: (ADDLconstcarry [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386ADDLconstcarry);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ADDLconst_0(ref Value v)
        { 
            // match: (ADDLconst [c] (ADDL x y))
            // cond:
            // result: (LEAL1 [c] x y)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                var x = v_0.Args[0L];
                var y = v_0.Args[1L];
                v.reset(Op386LEAL1);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDLconst [c] (LEAL [d] {s} x))
            // cond: is32Bit(c+d)
            // result: (LEAL [c+d] {s} x)
 
            // match: (ADDLconst [c] (LEAL [d] {s} x))
            // cond: is32Bit(c+d)
            // result: (LEAL [c+d] {s} x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var s = v_0.Aux;
                x = v_0.Args[0L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(Op386LEAL);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDLconst [c] (LEAL1 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL1 [c+d] {s} x y)
 
            // match: (ADDLconst [c] (LEAL1 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL1 [c+d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                d = v_0.AuxInt;
                s = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDLconst [c] (LEAL2 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL2 [c+d] {s} x y)
 
            // match: (ADDLconst [c] (LEAL2 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL2 [c+d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL2)
                {
                    break;
                }
                d = v_0.AuxInt;
                s = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDLconst [c] (LEAL4 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL4 [c+d] {s} x y)
 
            // match: (ADDLconst [c] (LEAL4 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL4 [c+d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                d = v_0.AuxInt;
                s = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(Op386LEAL4);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDLconst [c] (LEAL8 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL8 [c+d] {s} x y)
 
            // match: (ADDLconst [c] (LEAL8 [d] {s} x y))
            // cond: is32Bit(c+d)
            // result: (LEAL8 [c+d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL8)
                {
                    break;
                }
                d = v_0.AuxInt;
                s = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(Op386LEAL8);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (ADDLconst [c] x)
            // cond: int32(c)==0
            // result: x
 
            // match: (ADDLconst [c] x)
            // cond: int32(c)==0
            // result: x
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(int32(c) == 0L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (ADDLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [int64(int32(c+d))])
 
            // match: (ADDLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [int64(int32(c+d))])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = int64(int32(c + d));
                return true;
            } 
            // match: (ADDLconst [c] (ADDLconst [d] x))
            // cond:
            // result: (ADDLconst [int64(int32(c+d))] x)
 
            // match: (ADDLconst [c] (ADDLconst [d] x))
            // cond:
            // result: (ADDLconst [int64(int32(c+d))] x)
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(Op386ADDLconst);
                v.AuxInt = int64(int32(c + d));
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ANDL_0(ref Value v)
        { 
            // match: (ANDL x (MOVLconst [c]))
            // cond:
            // result: (ANDLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386ANDLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ANDL (MOVLconst [c]) x)
            // cond:
            // result: (ANDLconst [c] x)
 
            // match: (ANDL (MOVLconst [c]) x)
            // cond:
            // result: (ANDLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386ANDLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ANDL x x)
            // cond:
            // result: x
 
            // match: (ANDL x x)
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
        private static bool rewriteValue386_Op386ANDLconst_0(ref Value v)
        { 
            // match: (ANDLconst [c] (ANDLconst [d] x))
            // cond:
            // result: (ANDLconst [c & d] x)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                v.reset(Op386ANDLconst);
                v.AuxInt = c & d;
                v.AddArg(x);
                return true;
            } 
            // match: (ANDLconst [c] _)
            // cond: int32(c)==0
            // result: (MOVLconst [0])
 
            // match: (ANDLconst [c] _)
            // cond: int32(c)==0
            // result: (MOVLconst [0])
            while (true)
            {
                c = v.AuxInt;
                if (!(int32(c) == 0L))
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (ANDLconst [c] x)
            // cond: int32(c)==-1
            // result: x
 
            // match: (ANDLconst [c] x)
            // cond: int32(c)==-1
            // result: x
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(int32(c) == -1L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (ANDLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [c&d])
 
            // match: (ANDLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [c&d])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = c & d;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386CMPB_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (CMPB x (MOVLconst [c]))
            // cond:
            // result: (CMPBconst x [int64(int8(c))])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386CMPBconst);
                v.AuxInt = int64(int8(c));
                v.AddArg(x);
                return true;
            } 
            // match: (CMPB (MOVLconst [c]) x)
            // cond:
            // result: (InvertFlags (CMPBconst x [int64(int8(c))]))
 
            // match: (CMPB (MOVLconst [c]) x)
            // cond:
            // result: (InvertFlags (CMPBconst x [int64(int8(c))]))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386InvertFlags);
                var v0 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v0.AuxInt = int64(int8(c));
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386CMPBconst_0(ref Value v)
        { 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)==int8(y)
            // result: (FlagEQ)
            while (true)
            {
                var y = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var x = v_0.AuxInt;
                if (!(int8(x) == int8(y)))
                {
                    break;
                }
                v.reset(Op386FlagEQ);
                return true;
            } 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)<int8(y) && uint8(x)<uint8(y)
            // result: (FlagLT_ULT)
 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)<int8(y) && uint8(x)<uint8(y)
            // result: (FlagLT_ULT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int8(x) < int8(y) && uint8(x) < uint8(y)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)<int8(y) && uint8(x)>uint8(y)
            // result: (FlagLT_UGT)
 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)<int8(y) && uint8(x)>uint8(y)
            // result: (FlagLT_UGT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int8(x) < int8(y) && uint8(x) > uint8(y)))
                {
                    break;
                }
                v.reset(Op386FlagLT_UGT);
                return true;
            } 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)>int8(y) && uint8(x)<uint8(y)
            // result: (FlagGT_ULT)
 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)>int8(y) && uint8(x)<uint8(y)
            // result: (FlagGT_ULT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int8(x) > int8(y) && uint8(x) < uint8(y)))
                {
                    break;
                }
                v.reset(Op386FlagGT_ULT);
                return true;
            } 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)>int8(y) && uint8(x)>uint8(y)
            // result: (FlagGT_UGT)
 
            // match: (CMPBconst (MOVLconst [x]) [y])
            // cond: int8(x)>int8(y) && uint8(x)>uint8(y)
            // result: (FlagGT_UGT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int8(x) > int8(y) && uint8(x) > uint8(y)))
                {
                    break;
                }
                v.reset(Op386FlagGT_UGT);
                return true;
            } 
            // match: (CMPBconst (ANDLconst _ [m]) [n])
            // cond: 0 <= int8(m) && int8(m) < int8(n)
            // result: (FlagLT_ULT)
 
            // match: (CMPBconst (ANDLconst _ [m]) [n])
            // cond: 0 <= int8(m) && int8(m) < int8(n)
            // result: (FlagLT_ULT)
            while (true)
            {
                var n = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(0L <= int8(m) && int8(m) < int8(n)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPBconst (ANDL x y) [0])
            // cond:
            // result: (TESTB x y)
 
            // match: (CMPBconst (ANDL x y) [0])
            // cond:
            // result: (TESTB x y)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                v.reset(Op386TESTB);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (CMPBconst (ANDLconst [c] x) [0])
            // cond:
            // result: (TESTBconst [int64(int8(c))] x)
 
            // match: (CMPBconst (ANDLconst [c] x) [0])
            // cond:
            // result: (TESTBconst [int64(int8(c))] x)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(Op386TESTBconst);
                v.AuxInt = int64(int8(c));
                v.AddArg(x);
                return true;
            } 
            // match: (CMPBconst x [0])
            // cond:
            // result: (TESTB x x)
 
            // match: (CMPBconst x [0])
            // cond:
            // result: (TESTB x x)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386TESTB);
                v.AddArg(x);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386CMPL_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (CMPL x (MOVLconst [c]))
            // cond:
            // result: (CMPLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386CMPLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (CMPL (MOVLconst [c]) x)
            // cond:
            // result: (InvertFlags (CMPLconst x [c]))
 
            // match: (CMPL (MOVLconst [c]) x)
            // cond:
            // result: (InvertFlags (CMPLconst x [c]))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386InvertFlags);
                var v0 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v0.AuxInt = c;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386CMPLconst_0(ref Value v)
        { 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)==int32(y)
            // result: (FlagEQ)
            while (true)
            {
                var y = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var x = v_0.AuxInt;
                if (!(int32(x) == int32(y)))
                {
                    break;
                }
                v.reset(Op386FlagEQ);
                return true;
            } 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)<int32(y) && uint32(x)<uint32(y)
            // result: (FlagLT_ULT)
 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)<int32(y) && uint32(x)<uint32(y)
            // result: (FlagLT_ULT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int32(x) < int32(y) && uint32(x) < uint32(y)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)<int32(y) && uint32(x)>uint32(y)
            // result: (FlagLT_UGT)
 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)<int32(y) && uint32(x)>uint32(y)
            // result: (FlagLT_UGT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int32(x) < int32(y) && uint32(x) > uint32(y)))
                {
                    break;
                }
                v.reset(Op386FlagLT_UGT);
                return true;
            } 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)>int32(y) && uint32(x)<uint32(y)
            // result: (FlagGT_ULT)
 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)>int32(y) && uint32(x)<uint32(y)
            // result: (FlagGT_ULT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int32(x) > int32(y) && uint32(x) < uint32(y)))
                {
                    break;
                }
                v.reset(Op386FlagGT_ULT);
                return true;
            } 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)>int32(y) && uint32(x)>uint32(y)
            // result: (FlagGT_UGT)
 
            // match: (CMPLconst (MOVLconst [x]) [y])
            // cond: int32(x)>int32(y) && uint32(x)>uint32(y)
            // result: (FlagGT_UGT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int32(x) > int32(y) && uint32(x) > uint32(y)))
                {
                    break;
                }
                v.reset(Op386FlagGT_UGT);
                return true;
            } 
            // match: (CMPLconst (SHRLconst _ [c]) [n])
            // cond: 0 <= n && 0 < c && c <= 32 && (1<<uint64(32-c)) <= uint64(n)
            // result: (FlagLT_ULT)
 
            // match: (CMPLconst (SHRLconst _ [c]) [n])
            // cond: 0 <= n && 0 < c && c <= 32 && (1<<uint64(32-c)) <= uint64(n)
            // result: (FlagLT_ULT)
            while (true)
            {
                var n = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                if (!(0L <= n && 0L < c && c <= 32L && (1L << (int)(uint64(32L - c))) <= uint64(n)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPLconst (ANDLconst _ [m]) [n])
            // cond: 0 <= int32(m) && int32(m) < int32(n)
            // result: (FlagLT_ULT)
 
            // match: (CMPLconst (ANDLconst _ [m]) [n])
            // cond: 0 <= int32(m) && int32(m) < int32(n)
            // result: (FlagLT_ULT)
            while (true)
            {
                n = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(0L <= int32(m) && int32(m) < int32(n)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPLconst (ANDL x y) [0])
            // cond:
            // result: (TESTL x y)
 
            // match: (CMPLconst (ANDL x y) [0])
            // cond:
            // result: (TESTL x y)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                v.reset(Op386TESTL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (CMPLconst (ANDLconst [c] x) [0])
            // cond:
            // result: (TESTLconst [c] x)
 
            // match: (CMPLconst (ANDLconst [c] x) [0])
            // cond:
            // result: (TESTLconst [c] x)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(Op386TESTLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (CMPLconst x [0])
            // cond:
            // result: (TESTL x x)
 
            // match: (CMPLconst x [0])
            // cond:
            // result: (TESTL x x)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386TESTL);
                v.AddArg(x);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386CMPW_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (CMPW x (MOVLconst [c]))
            // cond:
            // result: (CMPWconst x [int64(int16(c))])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386CMPWconst);
                v.AuxInt = int64(int16(c));
                v.AddArg(x);
                return true;
            } 
            // match: (CMPW (MOVLconst [c]) x)
            // cond:
            // result: (InvertFlags (CMPWconst x [int64(int16(c))]))
 
            // match: (CMPW (MOVLconst [c]) x)
            // cond:
            // result: (InvertFlags (CMPWconst x [int64(int16(c))]))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386InvertFlags);
                var v0 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v0.AuxInt = int64(int16(c));
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386CMPWconst_0(ref Value v)
        { 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)==int16(y)
            // result: (FlagEQ)
            while (true)
            {
                var y = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var x = v_0.AuxInt;
                if (!(int16(x) == int16(y)))
                {
                    break;
                }
                v.reset(Op386FlagEQ);
                return true;
            } 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)<int16(y) && uint16(x)<uint16(y)
            // result: (FlagLT_ULT)
 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)<int16(y) && uint16(x)<uint16(y)
            // result: (FlagLT_ULT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int16(x) < int16(y) && uint16(x) < uint16(y)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)<int16(y) && uint16(x)>uint16(y)
            // result: (FlagLT_UGT)
 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)<int16(y) && uint16(x)>uint16(y)
            // result: (FlagLT_UGT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int16(x) < int16(y) && uint16(x) > uint16(y)))
                {
                    break;
                }
                v.reset(Op386FlagLT_UGT);
                return true;
            } 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)>int16(y) && uint16(x)<uint16(y)
            // result: (FlagGT_ULT)
 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)>int16(y) && uint16(x)<uint16(y)
            // result: (FlagGT_ULT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int16(x) > int16(y) && uint16(x) < uint16(y)))
                {
                    break;
                }
                v.reset(Op386FlagGT_ULT);
                return true;
            } 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)>int16(y) && uint16(x)>uint16(y)
            // result: (FlagGT_UGT)
 
            // match: (CMPWconst (MOVLconst [x]) [y])
            // cond: int16(x)>int16(y) && uint16(x)>uint16(y)
            // result: (FlagGT_UGT)
            while (true)
            {
                y = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                x = v_0.AuxInt;
                if (!(int16(x) > int16(y) && uint16(x) > uint16(y)))
                {
                    break;
                }
                v.reset(Op386FlagGT_UGT);
                return true;
            } 
            // match: (CMPWconst (ANDLconst _ [m]) [n])
            // cond: 0 <= int16(m) && int16(m) < int16(n)
            // result: (FlagLT_ULT)
 
            // match: (CMPWconst (ANDLconst _ [m]) [n])
            // cond: 0 <= int16(m) && int16(m) < int16(n)
            // result: (FlagLT_ULT)
            while (true)
            {
                var n = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var m = v_0.AuxInt;
                if (!(0L <= int16(m) && int16(m) < int16(n)))
                {
                    break;
                }
                v.reset(Op386FlagLT_ULT);
                return true;
            } 
            // match: (CMPWconst (ANDL x y) [0])
            // cond:
            // result: (TESTW x y)
 
            // match: (CMPWconst (ANDL x y) [0])
            // cond:
            // result: (TESTW x y)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                v.reset(Op386TESTW);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (CMPWconst (ANDLconst [c] x) [0])
            // cond:
            // result: (TESTWconst [int64(int16(c))] x)
 
            // match: (CMPWconst (ANDLconst [c] x) [0])
            // cond:
            // result: (TESTWconst [int64(int16(c))] x)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(Op386TESTWconst);
                v.AuxInt = int64(int16(c));
                v.AddArg(x);
                return true;
            } 
            // match: (CMPWconst x [0])
            // cond:
            // result: (TESTW x x)
 
            // match: (CMPWconst x [0])
            // cond:
            // result: (TESTW x x)
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386TESTW);
                v.AddArg(x);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386LEAL_0(ref Value v)
        { 
            // match: (LEAL [c] {s} (ADDLconst [d] x))
            // cond: is32Bit(c+d)
            // result: (LEAL [c+d] {s} x)
            while (true)
            {
                var c = v.AuxInt;
                var s = v.Aux;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                if (!(is32Bit(c + d)))
                {
                    break;
                }
                v.reset(Op386LEAL);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                return true;
            } 
            // match: (LEAL [c] {s} (ADDL x y))
            // cond: x.Op != OpSB && y.Op != OpSB
            // result: (LEAL1 [c] {s} x y)
 
            // match: (LEAL [c] {s} (ADDL x y))
            // cond: x.Op != OpSB && y.Op != OpSB
            // result: (LEAL1 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                var y = v_0.Args[1L];
                if (!(x.Op != OpSB && y.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL [off1] {sym1} (LEAL [off2] {sym2} x))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL [off1+off2] {mergeSym(sym1,sym2)} x)
 
            // match: (LEAL [off1] {sym1} (LEAL [off2] {sym2} x))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL [off1+off2] {mergeSym(sym1,sym2)} x)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                x = v_0.Args[0L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386LEAL);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                return true;
            } 
            // match: (LEAL [off1] {sym1} (LEAL1 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL [off1] {sym1} (LEAL1 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL [off1] {sym1} (LEAL2 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL2 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL [off1] {sym1} (LEAL2 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL2 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL2)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL [off1] {sym1} (LEAL4 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL4 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL [off1] {sym1} (LEAL4 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL4 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386LEAL4);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL [off1] {sym1} (LEAL8 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL8 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL [off1] {sym1} (LEAL8 [off2] {sym2} x y))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (LEAL8 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL8)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                x = v_0.Args[0L];
                y = v_0.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386LEAL8);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386LEAL1_0(ref Value v)
        { 
            // match: (LEAL1 [c] {s} (ADDLconst [d] x) y)
            // cond: is32Bit(c+d)   && x.Op != OpSB
            // result: (LEAL1 [c+d] {s} x y)
            while (true)
            {
                var c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                var y = v.Args[1L];
                if (!(is32Bit(c + d) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} y (ADDLconst [d] x))
            // cond: is32Bit(c+d)   && x.Op != OpSB
            // result: (LEAL1 [c+d] {s} x y)
 
            // match: (LEAL1 [c] {s} y (ADDLconst [d] x))
            // cond: is32Bit(c+d)   && x.Op != OpSB
            // result: (LEAL1 [c+d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                y = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                x = v_1.Args[0L];
                if (!(is32Bit(c + d) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} x (SHLLconst [1] y))
            // cond:
            // result: (LEAL2 [c] {s} x y)
 
            // match: (LEAL1 [c] {s} x (SHLLconst [1] y))
            // cond:
            // result: (LEAL2 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL2);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} (SHLLconst [1] y) x)
            // cond:
            // result: (LEAL2 [c] {s} x y)
 
            // match: (LEAL1 [c] {s} (SHLLconst [1] y) x)
            // cond:
            // result: (LEAL2 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386LEAL2);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} x (SHLLconst [2] y))
            // cond:
            // result: (LEAL4 [c] {s} x y)
 
            // match: (LEAL1 [c] {s} x (SHLLconst [2] y))
            // cond:
            // result: (LEAL4 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 2L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL4);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} (SHLLconst [2] y) x)
            // cond:
            // result: (LEAL4 [c] {s} x y)
 
            // match: (LEAL1 [c] {s} (SHLLconst [2] y) x)
            // cond:
            // result: (LEAL4 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 2L)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386LEAL4);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} x (SHLLconst [3] y))
            // cond:
            // result: (LEAL8 [c] {s} x y)
 
            // match: (LEAL1 [c] {s} x (SHLLconst [3] y))
            // cond:
            // result: (LEAL8 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 3L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL8);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [c] {s} (SHLLconst [3] y) x)
            // cond:
            // result: (LEAL8 [c] {s} x y)
 
            // match: (LEAL1 [c] {s} (SHLLconst [3] y) x)
            // cond:
            // result: (LEAL8 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 3L)
                {
                    break;
                }
                y = v_0.Args[0L];
                x = v.Args[1L];
                v.reset(Op386LEAL8);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL1 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                x = v_0.Args[0L];
                y = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL1 [off1] {sym1} y (LEAL [off2] {sym2} x))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL1 [off1] {sym1} y (LEAL [off2] {sym2} x))
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL1 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                y = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_1.AuxInt;
                sym2 = v_1.Aux;
                x = v_1.Args[0L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386LEAL2_0(ref Value v)
        { 
            // match: (LEAL2 [c] {s} (ADDLconst [d] x) y)
            // cond: is32Bit(c+d)   && x.Op != OpSB
            // result: (LEAL2 [c+d] {s} x y)
            while (true)
            {
                var c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                var y = v.Args[1L];
                if (!(is32Bit(c + d) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL2 [c] {s} x (ADDLconst [d] y))
            // cond: is32Bit(c+2*d) && y.Op != OpSB
            // result: (LEAL2 [c+2*d] {s} x y)
 
            // match: (LEAL2 [c] {s} x (ADDLconst [d] y))
            // cond: is32Bit(c+2*d) && y.Op != OpSB
            // result: (LEAL2 [c+2*d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                y = v_1.Args[0L];
                if (!(is32Bit(c + 2L * d) && y.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AuxInt = c + 2L * d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL2 [c] {s} x (SHLLconst [1] y))
            // cond:
            // result: (LEAL4 [c] {s} x y)
 
            // match: (LEAL2 [c] {s} x (SHLLconst [1] y))
            // cond:
            // result: (LEAL4 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL4);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL2 [c] {s} x (SHLLconst [2] y))
            // cond:
            // result: (LEAL8 [c] {s} x y)
 
            // match: (LEAL2 [c] {s} x (SHLLconst [2] y))
            // cond:
            // result: (LEAL8 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 2L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL8);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL2 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL2 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL2 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL2 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                x = v_0.Args[0L];
                y = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386LEAL4_0(ref Value v)
        { 
            // match: (LEAL4 [c] {s} (ADDLconst [d] x) y)
            // cond: is32Bit(c+d)   && x.Op != OpSB
            // result: (LEAL4 [c+d] {s} x y)
            while (true)
            {
                var c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                var y = v.Args[1L];
                if (!(is32Bit(c + d) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL4);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL4 [c] {s} x (ADDLconst [d] y))
            // cond: is32Bit(c+4*d) && y.Op != OpSB
            // result: (LEAL4 [c+4*d] {s} x y)
 
            // match: (LEAL4 [c] {s} x (ADDLconst [d] y))
            // cond: is32Bit(c+4*d) && y.Op != OpSB
            // result: (LEAL4 [c+4*d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                y = v_1.Args[0L];
                if (!(is32Bit(c + 4L * d) && y.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL4);
                v.AuxInt = c + 4L * d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL4 [c] {s} x (SHLLconst [1] y))
            // cond:
            // result: (LEAL8 [c] {s} x y)
 
            // match: (LEAL4 [c] {s} x (SHLLconst [1] y))
            // cond:
            // result: (LEAL8 [c] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                y = v_1.Args[0L];
                v.reset(Op386LEAL8);
                v.AuxInt = c;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL4 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL4 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL4 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL4 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                x = v_0.Args[0L];
                y = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL4);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386LEAL8_0(ref Value v)
        { 
            // match: (LEAL8 [c] {s} (ADDLconst [d] x) y)
            // cond: is32Bit(c+d)   && x.Op != OpSB
            // result: (LEAL8 [c+d] {s} x y)
            while (true)
            {
                var c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                var y = v.Args[1L];
                if (!(is32Bit(c + d) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL8);
                v.AuxInt = c + d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL8 [c] {s} x (ADDLconst [d] y))
            // cond: is32Bit(c+8*d) && y.Op != OpSB
            // result: (LEAL8 [c+8*d] {s} x y)
 
            // match: (LEAL8 [c] {s} x (ADDLconst [d] y))
            // cond: is32Bit(c+8*d) && y.Op != OpSB
            // result: (LEAL8 [c+8*d] {s} x y)
            while (true)
            {
                c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                y = v_1.Args[0L];
                if (!(is32Bit(c + 8L * d) && y.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL8);
                v.AuxInt = c + 8L * d;
                v.Aux = s;
                v.AddArg(x);
                v.AddArg(y);
                return true;
            } 
            // match: (LEAL8 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL8 [off1+off2] {mergeSym(sym1,sym2)} x y)
 
            // match: (LEAL8 [off1] {sym1} (LEAL [off2] {sym2} x) y)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2) && x.Op != OpSB
            // result: (LEAL8 [off1+off2] {mergeSym(sym1,sym2)} x y)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                x = v_0.Args[0L];
                y = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && x.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386LEAL8);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBLSX_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVBLSX x:(MOVBload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBLSXload <v.Type> [off] {sym} ptr mem)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != Op386MOVBload)
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
                var v0 = b.NewValue0(v.Pos, Op386MOVBLSXload, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVBLSX (ANDLconst [c] x))
            // cond: c & 0x80 == 0
            // result: (ANDLconst [c & 0x7f] x)
 
            // match: (MOVBLSX (ANDLconst [c] x))
            // cond: c & 0x80 == 0
            // result: (ANDLconst [c & 0x7f] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(c & 0x80UL == 0L))
                {
                    break;
                }
                v.reset(Op386ANDLconst);
                v.AuxInt = c & 0x7fUL;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBLSXload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVBLSXload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVBLSX x)
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVBstore)
                {
                    break;
                }
                var off2 = v_1.AuxInt;
                var sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                var x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(Op386MOVBLSX);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBLSXload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBLSXload [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVBLSXload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBLSXload [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVBLSXload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBLZX_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVBLZX x:(MOVBload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBload <v.Type> [off] {sym} ptr mem)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != Op386MOVBload)
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
                var v0 = b.NewValue0(v.Pos, Op386MOVBload, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVBLZX x:(MOVBloadidx1 [off] {sym} ptr idx mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBloadidx1 <v.Type> [off] {sym} ptr idx mem)
 
            // match: (MOVBLZX x:(MOVBloadidx1 [off] {sym} ptr idx mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVBloadidx1 <v.Type> [off] {sym} ptr idx mem)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                off = x.AuxInt;
                sym = x.Aux;
                _ = x.Args[2L];
                ptr = x.Args[0L];
                var idx = x.Args[1L];
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                v0 = b.NewValue0(v.Pos, Op386MOVBloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVBLZX (ANDLconst [c] x))
            // cond:
            // result: (ANDLconst [c & 0xff] x)
 
            // match: (MOVBLZX (ANDLconst [c] x))
            // cond:
            // result: (ANDLconst [c & 0xff] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(Op386ANDLconst);
                v.AuxInt = c & 0xffUL;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVBload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVBLZX x)
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVBstore)
                {
                    break;
                }
                var off2 = v_1.AuxInt;
                var sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                var x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(Op386MOVBLZX);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVBload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBload  [off1+off2] {sym} ptr mem)
 
            // match: (MOVBload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(Op386MOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBload  [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVBload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBload  [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVBload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVBloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVBload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVBloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVBloadidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVBloadidx1 [off] {sym} ptr idx mem)
 
            // match: (MOVBload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVBloadidx1 [off] {sym} ptr idx mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVBloadidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBloadidx1_0(ref Value v)
        { 
            // match: (MOVBloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVBloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVBloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBloadidx1 [c] {sym} idx (ADDLconst [d] ptr) mem)
            // cond:
            // result: (MOVBloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
 
            // match: (MOVBloadidx1 [c] {sym} idx (ADDLconst [d] ptr) mem)
            // cond:
            // result: (MOVBloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                idx = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                ptr = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVBloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVBloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVBloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVBloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVBloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBloadidx1 [c] {sym} (ADDLconst [d] idx) ptr mem)
            // cond:
            // result: (MOVBloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVBloadidx1 [c] {sym} (ADDLconst [d] idx) ptr mem)
            // cond:
            // result: (MOVBloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVBloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBstore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVBstore [off] {sym} ptr (MOVBLSX x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVBLSX)
                {
                    break;
                }
                var x = v_1.Args[0L];
                var mem = v.Args[2L];
                v.reset(Op386MOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVBLZX x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVBLZX x) mem)
            // cond:
            // result: (MOVBstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVBLZX)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVBstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBstore  [off1+off2] {sym} ptr val mem)
 
            // match: (MOVBstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVBstore  [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                ptr = v_0.Args[0L];
                var val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(Op386MOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} ptr (MOVLconst [c]) mem)
            // cond: validOff(off)
            // result: (MOVBstoreconst [makeValAndOff(int64(int8(c)),off)] {sym} ptr mem)
 
            // match: (MOVBstore [off] {sym} ptr (MOVLconst [c]) mem)
            // cond: validOff(off)
            // result: (MOVBstoreconst [makeValAndOff(int64(int8(c)),off)] {sym} ptr mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                mem = v.Args[2L];
                if (!(validOff(off)))
                {
                    break;
                }
                v.reset(Op386MOVBstoreconst);
                v.AuxInt = makeValAndOff(int64(int8(c)), off);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBstore  [off1+off2] {mergeSym(sym1,sym2)} base val mem)
 
            // match: (MOVBstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBstore  [off1+off2] {mergeSym(sym1,sym2)} base val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVBstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVBstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVBstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVBstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVBstoreidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVBstoreidx1 [off] {sym} ptr idx val mem)
 
            // match: (MOVBstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVBstoreidx1 [off] {sym} ptr idx val mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVBstoreidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [i] {s} p (SHRLconst [8] w) x:(MOVBstore [i-1] {s} p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstore [i-1] {s} p w mem)
 
            // match: (MOVBstore [i] {s} p (SHRLconst [8] w) x:(MOVBstore [i-1] {s} p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstore [i-1] {s} p w mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[2L];
                var p = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 8L)
                {
                    break;
                }
                var w = v_1.Args[0L];
                x = v.Args[2L];
                if (x.Op != Op386MOVBstore)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (w != x.Args[1L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstore);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstore [i] {s} p (SHRLconst [j] w) x:(MOVBstore [i-1] {s} p w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstore [i-1] {s} p w0 mem)
 
            // match: (MOVBstore [i] {s} p (SHRLconst [j] w) x:(MOVBstore [i-1] {s} p w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstore [i-1] {s} p w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[2L];
                p = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                var j = v_1.AuxInt;
                w = v_1.Args[0L];
                x = v.Args[2L];
                if (x.Op != Op386MOVBstore)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                var w0 = x.Args[1L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 8L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstore);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBstoreconst_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVBstoreconst [sc] {s} (ADDLconst [off] ptr) mem)
            // cond: ValAndOff(sc).canAdd(off)
            // result: (MOVBstoreconst [ValAndOff(sc).add(off)] {s} ptr mem)
            while (true)
            {
                var sc = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var off = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(ValAndOff(sc).canAdd(off)))
                {
                    break;
                }
                v.reset(Op386MOVBstoreconst);
                v.AuxInt = ValAndOff(sc).add(off);
                v.Aux = s;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
            // cond: canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off)   && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBstoreconst [ValAndOff(sc).add(off)] {mergeSym(sym1, sym2)} ptr mem)
 
            // match: (MOVBstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
            // cond: canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off)   && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVBstoreconst [ValAndOff(sc).add(off)] {mergeSym(sym1, sym2)} ptr mem)
            while (true)
            {
                sc = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVBstoreconst);
                v.AuxInt = ValAndOff(sc).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreconst [x] {sym1} (LEAL1 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVBstoreconstidx1 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVBstoreconst [x] {sym1} (LEAL1 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVBstoreconstidx1 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVBstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreconst [x] {sym} (ADDL ptr idx) mem)
            // cond:
            // result: (MOVBstoreconstidx1 [x] {sym} ptr idx mem)
 
            // match: (MOVBstoreconst [x] {sym} (ADDL ptr idx) mem)
            // cond:
            // result: (MOVBstoreconstidx1 [x] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                v.reset(Op386MOVBstoreconstidx1);
                v.AuxInt = x;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreconst [c] {s} p x:(MOVBstoreconst [a] {s} p mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 1 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVWstoreconst [makeValAndOff(ValAndOff(a).Val()&0xff | ValAndOff(c).Val()<<8, ValAndOff(a).Off())] {s} p mem)
 
            // match: (MOVBstoreconst [c] {s} p x:(MOVBstoreconst [a] {s} p mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 1 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVWstoreconst [makeValAndOff(ValAndOff(a).Val()&0xff | ValAndOff(c).Val()<<8, ValAndOff(a).Off())] {s} p mem)
            while (true)
            {
                var c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                var p = v.Args[0L];
                x = v.Args[1L];
                if (x.Op != Op386MOVBstoreconst)
                {
                    break;
                }
                var a = x.AuxInt;
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[1L];
                if (p != x.Args[0L])
                {
                    break;
                }
                mem = x.Args[1L];
                if (!(x.Uses == 1L && ValAndOff(a).Off() + 1L == ValAndOff(c).Off() && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconst);
                v.AuxInt = makeValAndOff(ValAndOff(a).Val() & 0xffUL | ValAndOff(c).Val() << (int)(8L), ValAndOff(a).Off());
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBstoreconstidx1_0(ref Value v)
        { 
            // match: (MOVBstoreconstidx1 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVBstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVBstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreconstidx1 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVBstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
 
            // match: (MOVBstoreconstidx1 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVBstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVBstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreconstidx1 [c] {s} p i x:(MOVBstoreconstidx1 [a] {s} p i mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 1 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVWstoreconstidx1 [makeValAndOff(ValAndOff(a).Val()&0xff | ValAndOff(c).Val()<<8, ValAndOff(a).Off())] {s} p i mem)
 
            // match: (MOVBstoreconstidx1 [c] {s} p i x:(MOVBstoreconstidx1 [a] {s} p i mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 1 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVWstoreconstidx1 [makeValAndOff(ValAndOff(a).Val()&0xff | ValAndOff(c).Val()<<8, ValAndOff(a).Off())] {s} p i mem)
            while (true)
            {
                c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[2L];
                var p = v.Args[0L];
                var i = v.Args[1L];
                x = v.Args[2L];
                if (x.Op != Op386MOVBstoreconstidx1)
                {
                    break;
                }
                var a = x.AuxInt;
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (i != x.Args[1L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && ValAndOff(a).Off() + 1L == ValAndOff(c).Off() && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconstidx1);
                v.AuxInt = makeValAndOff(ValAndOff(a).Val() & 0xffUL | ValAndOff(c).Val() << (int)(8L), ValAndOff(a).Off());
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(i);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBstoreidx1_0(ref Value v)
        { 
            // match: (MOVBstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVBstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVBstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [c] {sym} idx (ADDLconst [d] ptr) val mem)
            // cond:
            // result: (MOVBstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
 
            // match: (MOVBstoreidx1 [c] {sym} idx (ADDLconst [d] ptr) val mem)
            // cond:
            // result: (MOVBstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                ptr = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVBstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVBstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVBstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVBstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVBstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [c] {sym} (ADDLconst [d] idx) ptr val mem)
            // cond:
            // result: (MOVBstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVBstoreidx1 [c] {sym} (ADDLconst [d] idx) ptr val mem)
            // cond:
            // result: (MOVBstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVBstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[3L];
                var p = v.Args[0L];
                idx = v.Args[1L];
                var v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 8L)
                {
                    break;
                }
                var w = v_2.Args[0L];
                var x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                p = v.Args[0L];
                idx = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 8L)
                {
                    break;
                }
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 8L)
                {
                    break;
                }
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [8] w) x:(MOVBstoreidx1 [i-1] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 8L)
                {
                    break;
                }
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} p idx w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} p idx w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                p = v.Args[0L];
                idx = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                var j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                var w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 8L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} idx p w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
 
            // match: (MOVBstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} idx p w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                p = v.Args[0L];
                idx = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 8L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVBstoreidx1_10(ref Value v)
        { 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} p idx w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[3L];
                var idx = v.Args[0L];
                var p = v.Args[1L];
                var v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                var j = v_2.AuxInt;
                var w = v_2.Args[0L];
                var x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                var w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 8L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                var mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} idx p w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
 
            // match: (MOVBstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVBstoreidx1 [i-1] {s} idx p w0:(SHRLconst [j-8] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVWstoreidx1 [i-1] {s} p idx w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVBstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 1L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 8L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = i - 1L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVLload [off] {sym} ptr (MOVLstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: x
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLstore)
                {
                    break;
                }
                var off2 = v_1.AuxInt;
                var sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                var x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (MOVLload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVLload  [off1+off2] {sym} ptr mem)
 
            // match: (MOVLload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVLload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(Op386MOVLload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVLload  [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVLload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVLload  [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVLload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVLload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVLloadidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLload [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLloadidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVLload [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLloadidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVLloadidx4);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVLloadidx1 [off] {sym} ptr idx mem)
 
            // match: (MOVLload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVLloadidx1 [off] {sym} ptr idx mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVLloadidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLloadidx1_0(ref Value v)
        { 
            // match: (MOVLloadidx1 [c] {sym} ptr (SHLLconst [2] idx) mem)
            // cond:
            // result: (MOVLloadidx4 [c] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 2L)
                {
                    break;
                }
                var idx = v_1.Args[0L];
                var mem = v.Args[2L];
                v.reset(Op386MOVLloadidx4);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLloadidx1 [c] {sym} (SHLLconst [2] idx) ptr mem)
            // cond:
            // result: (MOVLloadidx4 [c] {sym} ptr idx mem)
 
            // match: (MOVLloadidx1 [c] {sym} (SHLLconst [2] idx) ptr mem)
            // cond:
            // result: (MOVLloadidx4 [c] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 2L)
                {
                    break;
                }
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVLloadidx4);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVLloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
 
            // match: (MOVLloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVLloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                ptr = v_0.Args[0L];
                idx = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVLloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLloadidx1 [c] {sym} idx (ADDLconst [d] ptr) mem)
            // cond:
            // result: (MOVLloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
 
            // match: (MOVLloadidx1 [c] {sym} idx (ADDLconst [d] ptr) mem)
            // cond:
            // result: (MOVLloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                idx = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                ptr = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVLloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVLloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVLloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVLloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVLloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLloadidx1 [c] {sym} (ADDLconst [d] idx) ptr mem)
            // cond:
            // result: (MOVLloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVLloadidx1 [c] {sym} (ADDLconst [d] idx) ptr mem)
            // cond:
            // result: (MOVLloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVLloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLloadidx4_0(ref Value v)
        { 
            // match: (MOVLloadidx4 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVLloadidx4 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVLloadidx4);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLloadidx4 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVLloadidx4  [int64(int32(c+4*d))] {sym} ptr idx mem)
 
            // match: (MOVLloadidx4 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVLloadidx4  [int64(int32(c+4*d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVLloadidx4);
                v.AuxInt = int64(int32(c + 4L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLstore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVLstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVLstore  [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
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
                v.reset(Op386MOVLstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstore [off] {sym} ptr (MOVLconst [c]) mem)
            // cond: validOff(off)
            // result: (MOVLstoreconst [makeValAndOff(int64(int32(c)),off)] {sym} ptr mem)
 
            // match: (MOVLstore [off] {sym} ptr (MOVLconst [c]) mem)
            // cond: validOff(off)
            // result: (MOVLstoreconst [makeValAndOff(int64(int32(c)),off)] {sym} ptr mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                mem = v.Args[2L];
                if (!(validOff(off)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = makeValAndOff(int64(int32(c)), off);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVLstore  [off1+off2] {mergeSym(sym1,sym2)} base val mem)
 
            // match: (MOVLstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVLstore  [off1+off2] {mergeSym(sym1,sym2)} base val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVLstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVLstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstore [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLstoreidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVLstore [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVLstoreidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx4);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVLstoreidx1 [off] {sym} ptr idx val mem)
 
            // match: (MOVLstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVLstoreidx1 [off] {sym} ptr idx val mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLstoreconst_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVLstoreconst [sc] {s} (ADDLconst [off] ptr) mem)
            // cond: ValAndOff(sc).canAdd(off)
            // result: (MOVLstoreconst [ValAndOff(sc).add(off)] {s} ptr mem)
            while (true)
            {
                var sc = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var off = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(ValAndOff(sc).canAdd(off)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = ValAndOff(sc).add(off);
                v.Aux = s;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
            // cond: canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off)   && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVLstoreconst [ValAndOff(sc).add(off)] {mergeSym(sym1, sym2)} ptr mem)
 
            // match: (MOVLstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
            // cond: canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off)   && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVLstoreconst [ValAndOff(sc).add(off)] {mergeSym(sym1, sym2)} ptr mem)
            while (true)
            {
                sc = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = ValAndOff(sc).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconst [x] {sym1} (LEAL1 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVLstoreconstidx1 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVLstoreconst [x] {sym1} (LEAL1 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVLstoreconstidx1 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconst [x] {sym1} (LEAL4 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVLstoreconstidx4 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVLstoreconst [x] {sym1} (LEAL4 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVLstoreconstidx4 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                off = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconstidx4);
                v.AuxInt = ValAndOff(x).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconst [x] {sym} (ADDL ptr idx) mem)
            // cond:
            // result: (MOVLstoreconstidx1 [x] {sym} ptr idx mem)
 
            // match: (MOVLstoreconst [x] {sym} (ADDL ptr idx) mem)
            // cond:
            // result: (MOVLstoreconstidx1 [x] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                v.reset(Op386MOVLstoreconstidx1);
                v.AuxInt = x;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLstoreconstidx1_0(ref Value v)
        { 
            // match: (MOVLstoreconstidx1 [c] {sym} ptr (SHLLconst [2] idx) mem)
            // cond:
            // result: (MOVLstoreconstidx4 [c] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 2L)
                {
                    break;
                }
                var idx = v_1.Args[0L];
                var mem = v.Args[2L];
                v.reset(Op386MOVLstoreconstidx4);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconstidx1 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVLstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
 
            // match: (MOVLstoreconstidx1 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVLstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                ptr = v_0.Args[0L];
                idx = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVLstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconstidx1 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVLstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
 
            // match: (MOVLstoreconstidx1 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVLstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVLstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLstoreconstidx4_0(ref Value v)
        { 
            // match: (MOVLstoreconstidx4 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVLstoreconstidx4 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVLstoreconstidx4);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreconstidx4 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVLstoreconstidx4 [ValAndOff(x).add(4*c)] {sym} ptr idx mem)
 
            // match: (MOVLstoreconstidx4 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVLstoreconstidx4 [ValAndOff(x).add(4*c)] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVLstoreconstidx4);
                v.AuxInt = ValAndOff(x).add(4L * c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLstoreidx1_0(ref Value v)
        { 
            // match: (MOVLstoreidx1 [c] {sym} ptr (SHLLconst [2] idx) val mem)
            // cond:
            // result: (MOVLstoreidx4 [c] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 2L)
                {
                    break;
                }
                var idx = v_1.Args[0L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx4);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreidx1 [c] {sym} (SHLLconst [2] idx) ptr val mem)
            // cond:
            // result: (MOVLstoreidx4 [c] {sym} ptr idx val mem)
 
            // match: (MOVLstoreidx1 [c] {sym} (SHLLconst [2] idx) ptr val mem)
            // cond:
            // result: (MOVLstoreidx4 [c] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 2L)
                {
                    break;
                }
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx4);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVLstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
 
            // match: (MOVLstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVLstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                ptr = v_0.Args[0L];
                idx = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreidx1 [c] {sym} idx (ADDLconst [d] ptr) val mem)
            // cond:
            // result: (MOVLstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
 
            // match: (MOVLstoreidx1 [c] {sym} idx (ADDLconst [d] ptr) val mem)
            // cond:
            // result: (MOVLstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                ptr = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVLstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVLstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVLstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreidx1 [c] {sym} (ADDLconst [d] idx) ptr val mem)
            // cond:
            // result: (MOVLstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVLstoreidx1 [c] {sym} (ADDLconst [d] idx) ptr val mem)
            // cond:
            // result: (MOVLstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVLstoreidx4_0(ref Value v)
        { 
            // match: (MOVLstoreidx4 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVLstoreidx4 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx4);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVLstoreidx4 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVLstoreidx4  [int64(int32(c+4*d))] {sym} ptr idx val mem)
 
            // match: (MOVLstoreidx4 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVLstoreidx4  [int64(int32(c+4*d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVLstoreidx4);
                v.AuxInt = int64(int32(c + 4L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDconst_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (MOVSDconst [c])
            // cond: config.ctxt.Flag_shared
            // result: (MOVSDconst2 (MOVSDconst1 [c]))
            while (true)
            {
                var c = v.AuxInt;
                if (!(config.ctxt.Flag_shared))
                {
                    break;
                }
                v.reset(Op386MOVSDconst2);
                var v0 = b.NewValue0(v.Pos, Op386MOVSDconst1, typ.UInt32);
                v0.AuxInt = c;
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVSDload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVSDload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
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
                v.reset(Op386MOVSDload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSDload [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVSDload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSDload [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVSDload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVSDload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSDloadidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDload [off1] {sym1} (LEAL8 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDloadidx8 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVSDload [off1] {sym1} (LEAL8 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDloadidx8 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL8)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSDloadidx8);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSDloadidx1 [off] {sym} ptr idx mem)
 
            // match: (MOVSDload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSDloadidx1 [off] {sym} ptr idx mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVSDloadidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDloadidx1_0(ref Value v)
        { 
            // match: (MOVSDloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVSDloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVSDloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSDloadidx1 [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVSDloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSDloadidx1 [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVSDloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDloadidx8_0(ref Value v)
        { 
            // match: (MOVSDloadidx8 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVSDloadidx8 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVSDloadidx8);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDloadidx8 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSDloadidx8 [int64(int32(c+8*d))] {sym} ptr idx mem)
 
            // match: (MOVSDloadidx8 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSDloadidx8 [int64(int32(c+8*d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVSDloadidx8);
                v.AuxInt = int64(int32(c + 8L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDstore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVSDstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVSDstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
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
                v.reset(Op386MOVSDstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSDstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
 
            // match: (MOVSDstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSDstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVSDstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVSDstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSDstoreidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDstore [off1] {sym1} (LEAL8 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDstoreidx8 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVSDstore [off1] {sym1} (LEAL8 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSDstoreidx8 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL8)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSDstoreidx8);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSDstoreidx1 [off] {sym} ptr idx val mem)
 
            // match: (MOVSDstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSDstoreidx1 [off] {sym} ptr idx val mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVSDstoreidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDstoreidx1_0(ref Value v)
        { 
            // match: (MOVSDstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVSDstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVSDstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSDstoreidx1 [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVSDstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSDstoreidx1 [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVSDstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSDstoreidx8_0(ref Value v)
        { 
            // match: (MOVSDstoreidx8 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVSDstoreidx8 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVSDstoreidx8);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSDstoreidx8 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSDstoreidx8 [int64(int32(c+8*d))] {sym} ptr idx val mem)
 
            // match: (MOVSDstoreidx8 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSDstoreidx8 [int64(int32(c+8*d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVSDstoreidx8);
                v.AuxInt = int64(int32(c + 8L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSconst_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (MOVSSconst [c])
            // cond: config.ctxt.Flag_shared
            // result: (MOVSSconst2 (MOVSSconst1 [c]))
            while (true)
            {
                var c = v.AuxInt;
                if (!(config.ctxt.Flag_shared))
                {
                    break;
                }
                v.reset(Op386MOVSSconst2);
                var v0 = b.NewValue0(v.Pos, Op386MOVSSconst1, typ.UInt32);
                v0.AuxInt = c;
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVSSload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVSSload [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
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
                v.reset(Op386MOVSSload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSSload [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVSSload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSSload [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVSSload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVSSload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSSloadidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSload [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSloadidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVSSload [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSloadidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSSloadidx4);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSSloadidx1 [off] {sym} ptr idx mem)
 
            // match: (MOVSSload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSSloadidx1 [off] {sym} ptr idx mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVSSloadidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSloadidx1_0(ref Value v)
        { 
            // match: (MOVSSloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVSSloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVSSloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSSloadidx1 [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVSSloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSSloadidx1 [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVSSloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSloadidx4_0(ref Value v)
        { 
            // match: (MOVSSloadidx4 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVSSloadidx4 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVSSloadidx4);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSloadidx4 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSSloadidx4 [int64(int32(c+4*d))] {sym} ptr idx mem)
 
            // match: (MOVSSloadidx4 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVSSloadidx4 [int64(int32(c+4*d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVSSloadidx4);
                v.AuxInt = int64(int32(c + 4L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSstore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVSSstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVSSstore [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
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
                v.reset(Op386MOVSSstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSSstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
 
            // match: (MOVSSstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVSSstore [off1+off2] {mergeSym(sym1,sym2)} base val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVSSstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVSSstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSSstoreidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSstore [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSstoreidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVSSstore [off1] {sym1} (LEAL4 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVSSstoreidx4 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL4)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVSSstoreidx4);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSSstoreidx1 [off] {sym} ptr idx val mem)
 
            // match: (MOVSSstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVSSstoreidx1 [off] {sym} ptr idx val mem)
            while (true)
            {
                var off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVSSstoreidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSstoreidx1_0(ref Value v)
        { 
            // match: (MOVSSstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVSSstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVSSstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSSstoreidx1 [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVSSstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSSstoreidx1 [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVSSstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVSSstoreidx4_0(ref Value v)
        { 
            // match: (MOVSSstoreidx4 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVSSstoreidx4 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVSSstoreidx4);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVSSstoreidx4 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSSstoreidx4 [int64(int32(c+4*d))] {sym} ptr idx val mem)
 
            // match: (MOVSSstoreidx4 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVSSstoreidx4 [int64(int32(c+4*d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVSSstoreidx4);
                v.AuxInt = int64(int32(c + 4L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWLSX_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVWLSX x:(MOVWload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVWLSXload <v.Type> [off] {sym} ptr mem)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != Op386MOVWload)
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
                var v0 = b.NewValue0(v.Pos, Op386MOVWLSXload, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVWLSX (ANDLconst [c] x))
            // cond: c & 0x8000 == 0
            // result: (ANDLconst [c & 0x7fff] x)
 
            // match: (MOVWLSX (ANDLconst [c] x))
            // cond: c & 0x8000 == 0
            // result: (ANDLconst [c & 0x7fff] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                if (!(c & 0x8000UL == 0L))
                {
                    break;
                }
                v.reset(Op386ANDLconst);
                v.AuxInt = c & 0x7fffUL;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWLSXload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVWLSXload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVWLSX x)
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVWstore)
                {
                    break;
                }
                var off2 = v_1.AuxInt;
                var sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                var x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(Op386MOVWLSX);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWLSXload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWLSXload [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVWLSXload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWLSXload [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                var off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVWLSXload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWLZX_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVWLZX x:(MOVWload [off] {sym} ptr mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVWload <v.Type> [off] {sym} ptr mem)
            while (true)
            {
                var x = v.Args[0L];
                if (x.Op != Op386MOVWload)
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
                var v0 = b.NewValue0(v.Pos, Op386MOVWload, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVWLZX x:(MOVWloadidx1 [off] {sym} ptr idx mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVWloadidx1 <v.Type> [off] {sym} ptr idx mem)
 
            // match: (MOVWLZX x:(MOVWloadidx1 [off] {sym} ptr idx mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVWloadidx1 <v.Type> [off] {sym} ptr idx mem)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                off = x.AuxInt;
                sym = x.Aux;
                _ = x.Args[2L];
                ptr = x.Args[0L];
                var idx = x.Args[1L];
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVWLZX x:(MOVWloadidx2 [off] {sym} ptr idx mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVWloadidx2 <v.Type> [off] {sym} ptr idx mem)
 
            // match: (MOVWLZX x:(MOVWloadidx2 [off] {sym} ptr idx mem))
            // cond: x.Uses == 1 && clobber(x)
            // result: @x.Block (MOVWloadidx2 <v.Type> [off] {sym} ptr idx mem)
            while (true)
            {
                x = v.Args[0L];
                if (x.Op != Op386MOVWloadidx2)
                {
                    break;
                }
                off = x.AuxInt;
                sym = x.Aux;
                _ = x.Args[2L];
                ptr = x.Args[0L];
                idx = x.Args[1L];
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                b = x.Block;
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx2, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = off;
                v0.Aux = sym;
                v0.AddArg(ptr);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (MOVWLZX (ANDLconst [c] x))
            // cond:
            // result: (ANDLconst [c & 0xffff] x)
 
            // match: (MOVWLZX (ANDLconst [c] x))
            // cond:
            // result: (ANDLconst [c & 0xffff] x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ANDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                x = v_0.Args[0L];
                v.reset(Op386ANDLconst);
                v.AuxInt = c & 0xffffUL;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWload_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVWload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
            // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
            // result: (MOVWLZX x)
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVWstore)
                {
                    break;
                }
                var off2 = v_1.AuxInt;
                var sym2 = v_1.Aux;
                _ = v_1.Args[2L];
                var ptr2 = v_1.Args[0L];
                var x = v_1.Args[1L];
                if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)))
                {
                    break;
                }
                v.reset(Op386MOVWLZX);
                v.AddArg(x);
                return true;
            } 
            // match: (MOVWload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWload  [off1+off2] {sym} ptr mem)
 
            // match: (MOVWload [off1] {sym} (ADDLconst [off2] ptr) mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWload  [off1+off2] {sym} ptr mem)
            while (true)
            {
                var off1 = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(Op386MOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWload  [off1+off2] {mergeSym(sym1,sym2)} base mem)
 
            // match: (MOVWload [off1] {sym1} (LEAL [off2] {sym2} base) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWload  [off1+off2] {mergeSym(sym1,sym2)} base mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVWload);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVWload [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWloadidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVWloadidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWload [off1] {sym1} (LEAL2 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWloadidx2 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVWload [off1] {sym1} (LEAL2 [off2] {sym2} ptr idx) mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWloadidx2 [off1+off2] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL2)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVWloadidx2);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVWloadidx1 [off] {sym} ptr idx mem)
 
            // match: (MOVWload [off] {sym} (ADDL ptr idx) mem)
            // cond: ptr.Op != OpSB
            // result: (MOVWloadidx1 [off] {sym} ptr idx mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVWloadidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWloadidx1_0(ref Value v)
        { 
            // match: (MOVWloadidx1 [c] {sym} ptr (SHLLconst [1] idx) mem)
            // cond:
            // result: (MOVWloadidx2 [c] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                var idx = v_1.Args[0L];
                var mem = v.Args[2L];
                v.reset(Op386MOVWloadidx2);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWloadidx1 [c] {sym} (SHLLconst [1] idx) ptr mem)
            // cond:
            // result: (MOVWloadidx2 [c] {sym} ptr idx mem)
 
            // match: (MOVWloadidx1 [c] {sym} (SHLLconst [1] idx) ptr mem)
            // cond:
            // result: (MOVWloadidx2 [c] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVWloadidx2);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVWloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
 
            // match: (MOVWloadidx1 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVWloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                ptr = v_0.Args[0L];
                idx = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVWloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWloadidx1 [c] {sym} idx (ADDLconst [d] ptr) mem)
            // cond:
            // result: (MOVWloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
 
            // match: (MOVWloadidx1 [c] {sym} idx (ADDLconst [d] ptr) mem)
            // cond:
            // result: (MOVWloadidx1 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                idx = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                ptr = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVWloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVWloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVWloadidx1 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVWloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVWloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWloadidx1 [c] {sym} (ADDLconst [d] idx) ptr mem)
            // cond:
            // result: (MOVWloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
 
            // match: (MOVWloadidx1 [c] {sym} (ADDLconst [d] idx) ptr mem)
            // cond:
            // result: (MOVWloadidx1  [int64(int32(c+d))]   {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVWloadidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWloadidx2_0(ref Value v)
        { 
            // match: (MOVWloadidx2 [c] {sym} (ADDLconst [d] ptr) idx mem)
            // cond:
            // result: (MOVWloadidx2 [int64(int32(c+d))] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVWloadidx2);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWloadidx2 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVWloadidx2  [int64(int32(c+2*d))] {sym} ptr idx mem)
 
            // match: (MOVWloadidx2 [c] {sym} ptr (ADDLconst [d] idx) mem)
            // cond:
            // result: (MOVWloadidx2  [int64(int32(c+2*d))] {sym} ptr idx mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVWloadidx2);
                v.AuxInt = int64(int32(c + 2L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstore_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVWstore [off] {sym} ptr (MOVWLSX x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
            while (true)
            {
                var off = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVWLSX)
                {
                    break;
                }
                var x = v_1.Args[0L];
                var mem = v.Args[2L];
                v.reset(Op386MOVWstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} ptr (MOVWLZX x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVWLZX x) mem)
            // cond:
            // result: (MOVWstore [off] {sym} ptr x mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVWLZX)
                {
                    break;
                }
                x = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVWstore);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWstore  [off1+off2] {sym} ptr val mem)
 
            // match: (MOVWstore [off1] {sym} (ADDLconst [off2] ptr) val mem)
            // cond: is32Bit(off1+off2)
            // result: (MOVWstore  [off1+off2] {sym} ptr val mem)
            while (true)
            {
                var off1 = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var off2 = v_0.AuxInt;
                ptr = v_0.Args[0L];
                var val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2)))
                {
                    break;
                }
                v.reset(Op386MOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} ptr (MOVLconst [c]) mem)
            // cond: validOff(off)
            // result: (MOVWstoreconst [makeValAndOff(int64(int16(c)),off)] {sym} ptr mem)
 
            // match: (MOVWstore [off] {sym} ptr (MOVLconst [c]) mem)
            // cond: validOff(off)
            // result: (MOVWstoreconst [makeValAndOff(int64(int16(c)),off)] {sym} ptr mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                mem = v.Args[2L];
                if (!(validOff(off)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconst);
                v.AuxInt = makeValAndOff(int64(int16(c)), off);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWstore  [off1+off2] {mergeSym(sym1,sym2)} base val mem)
 
            // match: (MOVWstore [off1] {sym1} (LEAL [off2] {sym2} base) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)   && (base.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWstore  [off1+off2] {mergeSym(sym1,sym2)} base val mem)
            while (true)
            {
                off1 = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                var sym2 = v_0.Aux;
                var @base = v_0.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2) && (@base.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVWstore);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(base);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVWstore [off1] {sym1} (LEAL1 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWstoreidx1 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off1] {sym1} (LEAL2 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWstoreidx2 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
 
            // match: (MOVWstore [off1] {sym1} (LEAL2 [off2] {sym2} ptr idx) val mem)
            // cond: is32Bit(off1+off2) && canMergeSym(sym1, sym2)
            // result: (MOVWstoreidx2 [off1+off2] {mergeSym(sym1,sym2)} ptr idx val mem)
            while (true)
            {
                off1 = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL2)
                {
                    break;
                }
                off2 = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(is32Bit(off1 + off2) && canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx2);
                v.AuxInt = off1 + off2;
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVWstoreidx1 [off] {sym} ptr idx val mem)
 
            // match: (MOVWstore [off] {sym} (ADDL ptr idx) val mem)
            // cond: ptr.Op != OpSB
            // result: (MOVWstoreidx1 [off] {sym} ptr idx val mem)
            while (true)
            {
                off = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(ptr.Op != OpSB))
                {
                    break;
                }
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = off;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [i] {s} p (SHRLconst [16] w) x:(MOVWstore [i-2] {s} p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstore [i-2] {s} p w mem)
 
            // match: (MOVWstore [i] {s} p (SHRLconst [16] w) x:(MOVWstore [i-2] {s} p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstore [i-2] {s} p w mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[2L];
                var p = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 16L)
                {
                    break;
                }
                var w = v_1.Args[0L];
                x = v.Args[2L];
                if (x.Op != Op386MOVWstore)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (w != x.Args[1L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstore);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstore [i] {s} p (SHRLconst [j] w) x:(MOVWstore [i-2] {s} p w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstore [i-2] {s} p w0 mem)
 
            // match: (MOVWstore [i] {s} p (SHRLconst [j] w) x:(MOVWstore [i-2] {s} p w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstore [i-2] {s} p w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[2L];
                p = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                var j = v_1.AuxInt;
                w = v_1.Args[0L];
                x = v.Args[2L];
                if (x.Op != Op386MOVWstore)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                var w0 = x.Args[1L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 16L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstore);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstoreconst_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config; 
            // match: (MOVWstoreconst [sc] {s} (ADDLconst [off] ptr) mem)
            // cond: ValAndOff(sc).canAdd(off)
            // result: (MOVWstoreconst [ValAndOff(sc).add(off)] {s} ptr mem)
            while (true)
            {
                var sc = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var off = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var mem = v.Args[1L];
                if (!(ValAndOff(sc).canAdd(off)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconst);
                v.AuxInt = ValAndOff(sc).add(off);
                v.Aux = s;
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
            // cond: canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off)   && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWstoreconst [ValAndOff(sc).add(off)] {mergeSym(sym1, sym2)} ptr mem)
 
            // match: (MOVWstoreconst [sc] {sym1} (LEAL [off] {sym2} ptr) mem)
            // cond: canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off)   && (ptr.Op != OpSB || !config.ctxt.Flag_shared)
            // result: (MOVWstoreconst [ValAndOff(sc).add(off)] {mergeSym(sym1, sym2)} ptr mem)
            while (true)
            {
                sc = v.AuxInt;
                var sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL)
                {
                    break;
                }
                off = v_0.AuxInt;
                var sym2 = v_0.Aux;
                ptr = v_0.Args[0L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2) && ValAndOff(sc).canAdd(off) && (ptr.Op != OpSB || !config.ctxt.Flag_shared)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconst);
                v.AuxInt = ValAndOff(sc).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconst [x] {sym1} (LEAL1 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVWstoreconstidx1 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVWstoreconst [x] {sym1} (LEAL1 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVWstoreconstidx1 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL1)
                {
                    break;
                }
                off = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                var idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconst [x] {sym1} (LEAL2 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVWstoreconstidx2 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
 
            // match: (MOVWstoreconst [x] {sym1} (LEAL2 [off] {sym2} ptr idx) mem)
            // cond: canMergeSym(sym1, sym2)
            // result: (MOVWstoreconstidx2 [ValAndOff(x).add(off)] {mergeSym(sym1,sym2)} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym1 = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386LEAL2)
                {
                    break;
                }
                off = v_0.AuxInt;
                sym2 = v_0.Aux;
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                if (!(canMergeSym(sym1, sym2)))
                {
                    break;
                }
                v.reset(Op386MOVWstoreconstidx2);
                v.AuxInt = ValAndOff(x).add(off);
                v.Aux = mergeSym(sym1, sym2);
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconst [x] {sym} (ADDL ptr idx) mem)
            // cond:
            // result: (MOVWstoreconstidx1 [x] {sym} ptr idx mem)
 
            // match: (MOVWstoreconst [x] {sym} (ADDL ptr idx) mem)
            // cond:
            // result: (MOVWstoreconstidx1 [x] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDL)
                {
                    break;
                }
                _ = v_0.Args[1L];
                ptr = v_0.Args[0L];
                idx = v_0.Args[1L];
                mem = v.Args[1L];
                v.reset(Op386MOVWstoreconstidx1);
                v.AuxInt = x;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconst [c] {s} p x:(MOVWstoreconst [a] {s} p mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 2 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVLstoreconst [makeValAndOff(ValAndOff(a).Val()&0xffff | ValAndOff(c).Val()<<16, ValAndOff(a).Off())] {s} p mem)
 
            // match: (MOVWstoreconst [c] {s} p x:(MOVWstoreconst [a] {s} p mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 2 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVLstoreconst [makeValAndOff(ValAndOff(a).Val()&0xffff | ValAndOff(c).Val()<<16, ValAndOff(a).Off())] {s} p mem)
            while (true)
            {
                var c = v.AuxInt;
                s = v.Aux;
                _ = v.Args[1L];
                var p = v.Args[0L];
                x = v.Args[1L];
                if (x.Op != Op386MOVWstoreconst)
                {
                    break;
                }
                var a = x.AuxInt;
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[1L];
                if (p != x.Args[0L])
                {
                    break;
                }
                mem = x.Args[1L];
                if (!(x.Uses == 1L && ValAndOff(a).Off() + 2L == ValAndOff(c).Off() && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = makeValAndOff(ValAndOff(a).Val() & 0xffffUL | ValAndOff(c).Val() << (int)(16L), ValAndOff(a).Off());
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstoreconstidx1_0(ref Value v)
        { 
            // match: (MOVWstoreconstidx1 [c] {sym} ptr (SHLLconst [1] idx) mem)
            // cond:
            // result: (MOVWstoreconstidx2 [c] {sym} ptr idx mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                var idx = v_1.Args[0L];
                var mem = v.Args[2L];
                v.reset(Op386MOVWstoreconstidx2);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconstidx1 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVWstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
 
            // match: (MOVWstoreconstidx1 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVWstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                ptr = v_0.Args[0L];
                idx = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVWstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconstidx1 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVWstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
 
            // match: (MOVWstoreconstidx1 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVWstoreconstidx1 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVWstoreconstidx1);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconstidx1 [c] {s} p i x:(MOVWstoreconstidx1 [a] {s} p i mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 2 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVLstoreconstidx1 [makeValAndOff(ValAndOff(a).Val()&0xffff | ValAndOff(c).Val()<<16, ValAndOff(a).Off())] {s} p i mem)
 
            // match: (MOVWstoreconstidx1 [c] {s} p i x:(MOVWstoreconstidx1 [a] {s} p i mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 2 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVLstoreconstidx1 [makeValAndOff(ValAndOff(a).Val()&0xffff | ValAndOff(c).Val()<<16, ValAndOff(a).Off())] {s} p i mem)
            while (true)
            {
                c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[2L];
                var p = v.Args[0L];
                var i = v.Args[1L];
                x = v.Args[2L];
                if (x.Op != Op386MOVWstoreconstidx1)
                {
                    break;
                }
                var a = x.AuxInt;
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (i != x.Args[1L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && ValAndOff(a).Off() + 2L == ValAndOff(c).Off() && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconstidx1);
                v.AuxInt = makeValAndOff(ValAndOff(a).Val() & 0xffffUL | ValAndOff(c).Val() << (int)(16L), ValAndOff(a).Off());
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(i);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstoreconstidx2_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVWstoreconstidx2 [x] {sym} (ADDLconst [c] ptr) idx mem)
            // cond:
            // result: (MOVWstoreconstidx2 [ValAndOff(x).add(c)] {sym} ptr idx mem)
            while (true)
            {
                var x = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[2L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var mem = v.Args[2L];
                v.reset(Op386MOVWstoreconstidx2);
                v.AuxInt = ValAndOff(x).add(c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconstidx2 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVWstoreconstidx2 [ValAndOff(x).add(2*c)] {sym} ptr idx mem)
 
            // match: (MOVWstoreconstidx2 [x] {sym} ptr (ADDLconst [c] idx) mem)
            // cond:
            // result: (MOVWstoreconstidx2 [ValAndOff(x).add(2*c)] {sym} ptr idx mem)
            while (true)
            {
                x = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                idx = v_1.Args[0L];
                mem = v.Args[2L];
                v.reset(Op386MOVWstoreconstidx2);
                v.AuxInt = ValAndOff(x).add(2L * c);
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreconstidx2 [c] {s} p i x:(MOVWstoreconstidx2 [a] {s} p i mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 2 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVLstoreconstidx1 [makeValAndOff(ValAndOff(a).Val()&0xffff | ValAndOff(c).Val()<<16, ValAndOff(a).Off())] {s} p (SHLLconst <i.Type> [1] i) mem)
 
            // match: (MOVWstoreconstidx2 [c] {s} p i x:(MOVWstoreconstidx2 [a] {s} p i mem))
            // cond: x.Uses == 1   && ValAndOff(a).Off() + 2 == ValAndOff(c).Off()   && clobber(x)
            // result: (MOVLstoreconstidx1 [makeValAndOff(ValAndOff(a).Val()&0xffff | ValAndOff(c).Val()<<16, ValAndOff(a).Off())] {s} p (SHLLconst <i.Type> [1] i) mem)
            while (true)
            {
                c = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[2L];
                var p = v.Args[0L];
                var i = v.Args[1L];
                x = v.Args[2L];
                if (x.Op != Op386MOVWstoreconstidx2)
                {
                    break;
                }
                var a = x.AuxInt;
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[2L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (i != x.Args[1L])
                {
                    break;
                }
                mem = x.Args[2L];
                if (!(x.Uses == 1L && ValAndOff(a).Off() + 2L == ValAndOff(c).Off() && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreconstidx1);
                v.AuxInt = makeValAndOff(ValAndOff(a).Val() & 0xffffUL | ValAndOff(c).Val() << (int)(16L), ValAndOff(a).Off());
                v.Aux = s;
                v.AddArg(p);
                var v0 = b.NewValue0(v.Pos, Op386SHLLconst, i.Type);
                v0.AuxInt = 1L;
                v0.AddArg(i);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstoreidx1_0(ref Value v)
        { 
            // match: (MOVWstoreidx1 [c] {sym} ptr (SHLLconst [1] idx) val mem)
            // cond:
            // result: (MOVWstoreidx2 [c] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 1L)
                {
                    break;
                }
                var idx = v_1.Args[0L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx2);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [c] {sym} (SHLLconst [1] idx) ptr val mem)
            // cond:
            // result: (MOVWstoreidx2 [c] {sym} ptr idx val mem)
 
            // match: (MOVWstoreidx1 [c] {sym} (SHLLconst [1] idx) ptr val mem)
            // cond:
            // result: (MOVWstoreidx2 [c] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (v_0.AuxInt != 1L)
                {
                    break;
                }
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx2);
                v.AuxInt = c;
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVWstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
 
            // match: (MOVWstoreidx1 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVWstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                ptr = v_0.Args[0L];
                idx = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [c] {sym} idx (ADDLconst [d] ptr) val mem)
            // cond:
            // result: (MOVWstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
 
            // match: (MOVWstoreidx1 [c] {sym} idx (ADDLconst [d] ptr) val mem)
            // cond:
            // result: (MOVWstoreidx1 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                ptr = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVWstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVWstoreidx1 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVWstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [c] {sym} (ADDLconst [d] idx) ptr val mem)
            // cond:
            // result: (MOVWstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
 
            // match: (MOVWstoreidx1 [c] {sym} (ADDLconst [d] idx) ptr val mem)
            // cond:
            // result: (MOVWstoreidx1  [int64(int32(c+d))]   {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                idx = v_0.Args[0L];
                ptr = v.Args[1L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx1);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[3L];
                var p = v.Args[0L];
                idx = v.Args[1L];
                var v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 16L)
                {
                    break;
                }
                var w = v_2.Args[0L];
                var x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                p = v.Args[0L];
                idx = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 16L)
                {
                    break;
                }
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 16L)
                {
                    break;
                }
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [16] w) x:(MOVWstoreidx1 [i-2] {s} idx p w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 16L)
                {
                    break;
                }
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstoreidx1_10(ref Value v)
        { 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} p idx w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[3L];
                var p = v.Args[0L];
                var idx = v.Args[1L];
                var v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                var j = v_2.AuxInt;
                var w = v_2.Args[0L];
                var x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                var w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 16L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                var mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} idx p w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
 
            // match: (MOVWstoreidx1 [i] {s} p idx (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} idx p w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                p = v.Args[0L];
                idx = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 16L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} p idx w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} p idx w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 16L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} idx p w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
 
            // match: (MOVWstoreidx1 [i] {s} idx p (SHRLconst [j] w) x:(MOVWstoreidx1 [i-2] {s} idx p w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p idx w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                idx = v.Args[0L];
                p = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx1)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (idx != x.Args[0L])
                {
                    break;
                }
                if (p != x.Args[1L])
                {
                    break;
                }
                w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 16L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v.AddArg(idx);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MOVWstoreidx2_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MOVWstoreidx2 [c] {sym} (ADDLconst [d] ptr) idx val mem)
            // cond:
            // result: (MOVWstoreidx2 [int64(int32(c+d))] {sym} ptr idx val mem)
            while (true)
            {
                var c = v.AuxInt;
                var sym = v.Aux;
                _ = v.Args[3L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ADDLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var ptr = v_0.Args[0L];
                var idx = v.Args[1L];
                var val = v.Args[2L];
                var mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx2);
                v.AuxInt = int64(int32(c + d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx2 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVWstoreidx2  [int64(int32(c+2*d))] {sym} ptr idx val mem)
 
            // match: (MOVWstoreidx2 [c] {sym} ptr (ADDLconst [d] idx) val mem)
            // cond:
            // result: (MOVWstoreidx2  [int64(int32(c+2*d))] {sym} ptr idx val mem)
            while (true)
            {
                c = v.AuxInt;
                sym = v.Aux;
                _ = v.Args[3L];
                ptr = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386ADDLconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                idx = v_1.Args[0L];
                val = v.Args[2L];
                mem = v.Args[3L];
                v.reset(Op386MOVWstoreidx2);
                v.AuxInt = int64(int32(c + 2L * d));
                v.Aux = sym;
                v.AddArg(ptr);
                v.AddArg(idx);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx2 [i] {s} p idx (SHRLconst [16] w) x:(MOVWstoreidx2 [i-2] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p (SHLLconst <idx.Type> [1] idx) w mem)
 
            // match: (MOVWstoreidx2 [i] {s} p idx (SHRLconst [16] w) x:(MOVWstoreidx2 [i-2] {s} p idx w mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p (SHLLconst <idx.Type> [1] idx) w mem)
            while (true)
            {
                var i = v.AuxInt;
                var s = v.Aux;
                _ = v.Args[3L];
                var p = v.Args[0L];
                idx = v.Args[1L];
                var v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                if (v_2.AuxInt != 16L)
                {
                    break;
                }
                var w = v_2.Args[0L];
                var x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx2)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                if (w != x.Args[2L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                var v0 = b.NewValue0(v.Pos, Op386SHLLconst, idx.Type);
                v0.AuxInt = 1L;
                v0.AddArg(idx);
                v.AddArg(v0);
                v.AddArg(w);
                v.AddArg(mem);
                return true;
            } 
            // match: (MOVWstoreidx2 [i] {s} p idx (SHRLconst [j] w) x:(MOVWstoreidx2 [i-2] {s} p idx w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p (SHLLconst <idx.Type> [1] idx) w0 mem)
 
            // match: (MOVWstoreidx2 [i] {s} p idx (SHRLconst [j] w) x:(MOVWstoreidx2 [i-2] {s} p idx w0:(SHRLconst [j-16] w) mem))
            // cond: x.Uses == 1   && clobber(x)
            // result: (MOVLstoreidx1 [i-2] {s} p (SHLLconst <idx.Type> [1] idx) w0 mem)
            while (true)
            {
                i = v.AuxInt;
                s = v.Aux;
                _ = v.Args[3L];
                p = v.Args[0L];
                idx = v.Args[1L];
                v_2 = v.Args[2L];
                if (v_2.Op != Op386SHRLconst)
                {
                    break;
                }
                var j = v_2.AuxInt;
                w = v_2.Args[0L];
                x = v.Args[3L];
                if (x.Op != Op386MOVWstoreidx2)
                {
                    break;
                }
                if (x.AuxInt != i - 2L)
                {
                    break;
                }
                if (x.Aux != s)
                {
                    break;
                }
                _ = x.Args[3L];
                if (p != x.Args[0L])
                {
                    break;
                }
                if (idx != x.Args[1L])
                {
                    break;
                }
                var w0 = x.Args[2L];
                if (w0.Op != Op386SHRLconst)
                {
                    break;
                }
                if (w0.AuxInt != j - 16L)
                {
                    break;
                }
                if (w != w0.Args[0L])
                {
                    break;
                }
                mem = x.Args[3L];
                if (!(x.Uses == 1L && clobber(x)))
                {
                    break;
                }
                v.reset(Op386MOVLstoreidx1);
                v.AuxInt = i - 2L;
                v.Aux = s;
                v.AddArg(p);
                v0 = b.NewValue0(v.Pos, Op386SHLLconst, idx.Type);
                v0.AuxInt = 1L;
                v0.AddArg(idx);
                v.AddArg(v0);
                v.AddArg(w0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MULL_0(ref Value v)
        { 
            // match: (MULL x (MOVLconst [c]))
            // cond:
            // result: (MULLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386MULLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (MULL (MOVLconst [c]) x)
            // cond:
            // result: (MULLconst [c] x)
 
            // match: (MULL (MOVLconst [c]) x)
            // cond:
            // result: (MULLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386MULLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MULLconst_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MULLconst [c] (MULLconst [d] x))
            // cond:
            // result: (MULLconst [int64(int32(c * d))] x)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MULLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                v.reset(Op386MULLconst);
                v.AuxInt = int64(int32(c * d));
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [-1] x)
            // cond:
            // result: (NEGL x)
 
            // match: (MULLconst [-1] x)
            // cond:
            // result: (NEGL x)
            while (true)
            {
                if (v.AuxInt != -1L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386NEGL);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [0] _)
            // cond:
            // result: (MOVLconst [0])
 
            // match: (MULLconst [0] _)
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (MULLconst [1] x)
            // cond:
            // result: x
 
            // match: (MULLconst [1] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 1L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [3] x)
            // cond:
            // result: (LEAL2 x x)
 
            // match: (MULLconst [3] x)
            // cond:
            // result: (LEAL2 x x)
            while (true)
            {
                if (v.AuxInt != 3L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL2);
                v.AddArg(x);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [5] x)
            // cond:
            // result: (LEAL4 x x)
 
            // match: (MULLconst [5] x)
            // cond:
            // result: (LEAL4 x x)
            while (true)
            {
                if (v.AuxInt != 5L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL4);
                v.AddArg(x);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [7] x)
            // cond:
            // result: (LEAL8 (NEGL <v.Type> x) x)
 
            // match: (MULLconst [7] x)
            // cond:
            // result: (LEAL8 (NEGL <v.Type> x) x)
            while (true)
            {
                if (v.AuxInt != 7L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL8);
                var v0 = b.NewValue0(v.Pos, Op386NEGL, v.Type);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [9] x)
            // cond:
            // result: (LEAL8 x x)
 
            // match: (MULLconst [9] x)
            // cond:
            // result: (LEAL8 x x)
            while (true)
            {
                if (v.AuxInt != 9L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL8);
                v.AddArg(x);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [11] x)
            // cond:
            // result: (LEAL2 x (LEAL4 <v.Type> x x))
 
            // match: (MULLconst [11] x)
            // cond:
            // result: (LEAL2 x (LEAL4 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 11L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL2);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [13] x)
            // cond:
            // result: (LEAL4 x (LEAL2 <v.Type> x x))
 
            // match: (MULLconst [13] x)
            // cond:
            // result: (LEAL4 x (LEAL2 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 13L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL4);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MULLconst_10(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MULLconst [21] x)
            // cond:
            // result: (LEAL4 x (LEAL4 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 21L)
                {
                    break;
                }
                var x = v.Args[0L];
                v.reset(Op386LEAL4);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [25] x)
            // cond:
            // result: (LEAL8 x (LEAL2 <v.Type> x x))
 
            // match: (MULLconst [25] x)
            // cond:
            // result: (LEAL8 x (LEAL2 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 25L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL8);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [37] x)
            // cond:
            // result: (LEAL4 x (LEAL8 <v.Type> x x))
 
            // match: (MULLconst [37] x)
            // cond:
            // result: (LEAL4 x (LEAL8 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 37L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL4);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [41] x)
            // cond:
            // result: (LEAL8 x (LEAL4 <v.Type> x x))
 
            // match: (MULLconst [41] x)
            // cond:
            // result: (LEAL8 x (LEAL4 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 41L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL8);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [73] x)
            // cond:
            // result: (LEAL8 x (LEAL8 <v.Type> x x))
 
            // match: (MULLconst [73] x)
            // cond:
            // result: (LEAL8 x (LEAL8 <v.Type> x x))
            while (true)
            {
                if (v.AuxInt != 73L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(Op386LEAL8);
                v.AddArg(x);
                v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c+1) && c >= 15
            // result: (SUBL (SHLLconst <v.Type> [log2(c+1)] x) x)
 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c+1) && c >= 15
            // result: (SUBL (SHLLconst <v.Type> [log2(c+1)] x) x)
            while (true)
            {
                var c = v.AuxInt;
                x = v.Args[0L];
                if (!(isPowerOfTwo(c + 1L) && c >= 15L))
                {
                    break;
                }
                v.reset(Op386SUBL);
                v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
                v0.AuxInt = log2(c + 1L);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-1) && c >= 17
            // result: (LEAL1 (SHLLconst <v.Type> [log2(c-1)] x) x)
 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-1) && c >= 17
            // result: (LEAL1 (SHLLconst <v.Type> [log2(c-1)] x) x)
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(isPowerOfTwo(c - 1L) && c >= 17L))
                {
                    break;
                }
                v.reset(Op386LEAL1);
                v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
                v0.AuxInt = log2(c - 1L);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-2) && c >= 34
            // result: (LEAL2 (SHLLconst <v.Type> [log2(c-2)] x) x)
 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-2) && c >= 34
            // result: (LEAL2 (SHLLconst <v.Type> [log2(c-2)] x) x)
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(isPowerOfTwo(c - 2L) && c >= 34L))
                {
                    break;
                }
                v.reset(Op386LEAL2);
                v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
                v0.AuxInt = log2(c - 2L);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-4) && c >= 68
            // result: (LEAL4 (SHLLconst <v.Type> [log2(c-4)] x) x)
 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-4) && c >= 68
            // result: (LEAL4 (SHLLconst <v.Type> [log2(c-4)] x) x)
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(isPowerOfTwo(c - 4L) && c >= 68L))
                {
                    break;
                }
                v.reset(Op386LEAL4);
                v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
                v0.AuxInt = log2(c - 4L);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-8) && c >= 136
            // result: (LEAL8 (SHLLconst <v.Type> [log2(c-8)] x) x)
 
            // match: (MULLconst [c] x)
            // cond: isPowerOfTwo(c-8) && c >= 136
            // result: (LEAL8 (SHLLconst <v.Type> [log2(c-8)] x) x)
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(isPowerOfTwo(c - 8L) && c >= 136L))
                {
                    break;
                }
                v.reset(Op386LEAL8);
                v0 = b.NewValue0(v.Pos, Op386SHLLconst, v.Type);
                v0.AuxInt = log2(c - 8L);
                v0.AddArg(x);
                v.AddArg(v0);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386MULLconst_20(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (MULLconst [c] x)
            // cond: c%3 == 0 && isPowerOfTwo(c/3)
            // result: (SHLLconst [log2(c/3)] (LEAL2 <v.Type> x x))
            while (true)
            {
                var c = v.AuxInt;
                var x = v.Args[0L];
                if (!(c % 3L == 0L && isPowerOfTwo(c / 3L)))
                {
                    break;
                }
                v.reset(Op386SHLLconst);
                v.AuxInt = log2(c / 3L);
                var v0 = b.NewValue0(v.Pos, Op386LEAL2, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: c%5 == 0 && isPowerOfTwo(c/5)
            // result: (SHLLconst [log2(c/5)] (LEAL4 <v.Type> x x))
 
            // match: (MULLconst [c] x)
            // cond: c%5 == 0 && isPowerOfTwo(c/5)
            // result: (SHLLconst [log2(c/5)] (LEAL4 <v.Type> x x))
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(c % 5L == 0L && isPowerOfTwo(c / 5L)))
                {
                    break;
                }
                v.reset(Op386SHLLconst);
                v.AuxInt = log2(c / 5L);
                v0 = b.NewValue0(v.Pos, Op386LEAL4, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [c] x)
            // cond: c%9 == 0 && isPowerOfTwo(c/9)
            // result: (SHLLconst [log2(c/9)] (LEAL8 <v.Type> x x))
 
            // match: (MULLconst [c] x)
            // cond: c%9 == 0 && isPowerOfTwo(c/9)
            // result: (SHLLconst [log2(c/9)] (LEAL8 <v.Type> x x))
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(c % 9L == 0L && isPowerOfTwo(c / 9L)))
                {
                    break;
                }
                v.reset(Op386SHLLconst);
                v.AuxInt = log2(c / 9L);
                v0 = b.NewValue0(v.Pos, Op386LEAL8, v.Type);
                v0.AddArg(x);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (MULLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [int64(int32(c*d))])
 
            // match: (MULLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [int64(int32(c*d))])
            while (true)
            {
                c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = int64(int32(c * d));
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386NEGL_0(ref Value v)
        { 
            // match: (NEGL (MOVLconst [c]))
            // cond:
            // result: (MOVLconst [int64(int32(-c))])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = int64(int32(-c));
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386NOTL_0(ref Value v)
        { 
            // match: (NOTL (MOVLconst [c]))
            // cond:
            // result: (MOVLconst [^c])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = ~c;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORL_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (ORL x (MOVLconst [c]))
            // cond:
            // result: (ORLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386ORLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL (MOVLconst [c]) x)
            // cond:
            // result: (ORLconst [c] x)
 
            // match: (ORL (MOVLconst [c]) x)
            // cond:
            // result: (ORLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386ORLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL (SHLLconst [c] x) (SHRLconst [d] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
 
            // match: (ORL (SHLLconst [c] x) (SHRLconst [d] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                var d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(d == 32L - c))
                {
                    break;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL (SHRLconst [d] x) (SHLLconst [c] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
 
            // match: (ORL (SHRLconst [d] x) (SHLLconst [c] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(d == 32L - c))
                {
                    break;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL <t> (SHLLconst x [c]) (SHRWconst x [d]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
 
            // match: (ORL <t> (SHLLconst x [c]) (SHRWconst x [d]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRWconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 16L && d == 16L - c && t.Size() == 2L))
                {
                    break;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL <t> (SHRWconst x [d]) (SHLLconst x [c]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
 
            // match: (ORL <t> (SHRWconst x [d]) (SHLLconst x [c]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 16L && d == 16L - c && t.Size() == 2L))
                {
                    break;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL <t> (SHLLconst x [c]) (SHRBconst x [d]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
 
            // match: (ORL <t> (SHLLconst x [c]) (SHRBconst x [d]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRBconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 8L && d == 8L - c && t.Size() == 1L))
                {
                    break;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL <t> (SHRBconst x [d]) (SHLLconst x [c]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
 
            // match: (ORL <t> (SHRBconst x [d]) (SHLLconst x [c]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRBconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 8L && d == 8L - c && t.Size() == 1L))
                {
                    break;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (ORL x x)
            // cond:
            // result: x
 
            // match: (ORL x x)
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
            // match: (ORL x0:(MOVBload [i0] {s} p mem) s0:(SHLLconst [8] x1:(MOVBload [i1] {s} p mem)))
            // cond: i1 == i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWload [i0] {s} p mem)
 
            // match: (ORL x0:(MOVBload [i0] {s} p mem) s0:(SHLLconst [8] x1:(MOVBload [i1] {s} p mem)))
            // cond: i1 == i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWload [i0] {s} p mem)
            while (true)
            {
                _ = v.Args[1L];
                var x0 = v.Args[0L];
                if (x0.Op != Op386MOVBload)
                {
                    break;
                }
                var i0 = x0.AuxInt;
                var s = x0.Aux;
                _ = x0.Args[1L];
                var p = x0.Args[0L];
                var mem = x0.Args[1L];
                var s0 = v.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                var x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBload)
                {
                    break;
                }
                var i1 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[1L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (mem != x1.Args[1L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                var v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORL_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBload [i1] {s} p mem)) x0:(MOVBload [i0] {s} p mem))
            // cond: i1 == i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWload [i0] {s} p mem)
            while (true)
            {
                _ = v.Args[1L];
                var s0 = v.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                var x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBload)
                {
                    break;
                }
                var i1 = x1.AuxInt;
                var s = x1.Aux;
                _ = x1.Args[1L];
                var p = x1.Args[0L];
                var mem = x1.Args[1L];
                var x0 = v.Args[1L];
                if (x0.Op != Op386MOVBload)
                {
                    break;
                }
                var i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[1L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (mem != x0.Args[1L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                var v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWload [i0] {s} p mem) s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem))) s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
 
            // match: (ORL o0:(ORL x0:(MOVWload [i0] {s} p mem) s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem))) s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
            while (true)
            {
                _ = v.Args[1L];
                var o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWload)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[1L];
                p = x0.Args[0L];
                mem = x0.Args[1L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBload)
                {
                    break;
                }
                var i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[1L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (mem != x1.Args[1L])
                {
                    break;
                }
                var s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                var x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBload)
                {
                    break;
                }
                var i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[1L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (mem != x2.Args[1L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem)) x0:(MOVWload [i0] {s} p mem)) s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem)) x0:(MOVWload [i0] {s} p mem)) s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBload)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[1L];
                p = x1.Args[0L];
                mem = x1.Args[1L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWload)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[1L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (mem != x0.Args[1L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBload)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[1L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (mem != x2.Args[1L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)) o0:(ORL x0:(MOVWload [i0] {s} p mem) s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)) o0:(ORL x0:(MOVWload [i0] {s} p mem) s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBload)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[1L];
                p = x2.Args[0L];
                mem = x2.Args[1L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWload)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[1L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (mem != x0.Args[1L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBload)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[1L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (mem != x1.Args[1L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem)) x0:(MOVWload [i0] {s} p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBload [i3] {s} p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBload [i2] {s} p mem)) x0:(MOVWload [i0] {s} p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLload [i0] {s} p mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBload)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[1L];
                p = x2.Args[0L];
                mem = x2.Args[1L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBload)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[1L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (mem != x1.Args[1L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWload)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[1L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (mem != x0.Args[1L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                x0 = v.Args[0L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                p = x0.Args[0L];
                var idx = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = v.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                x0 = v.Args[0L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                idx = x0.Args[0L];
                p = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = v.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                x0 = v.Args[0L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                p = x0.Args[0L];
                idx = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = v.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL x0:(MOVBloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                x0 = v.Args[0L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                idx = x0.Args[0L];
                p = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = v.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)) x0:(MOVBloadidx1 [i0] {s} p idx mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)) x0:(MOVBloadidx1 [i0] {s} p idx mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s0 = v.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                p = x1.Args[0L];
                idx = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = v.Args[1L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORL_20(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)) x0:(MOVBloadidx1 [i0] {s} p idx mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                var s0 = v.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                var x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i1 = x1.AuxInt;
                var s = x1.Aux;
                _ = x1.Args[2L];
                var idx = x1.Args[0L];
                var p = x1.Args[1L];
                var mem = x1.Args[2L];
                var x0 = v.Args[1L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                var v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)) x0:(MOVBloadidx1 [i0] {s} idx p mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} p idx mem)) x0:(MOVBloadidx1 [i0] {s} idx p mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s0 = v.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                p = x1.Args[0L];
                idx = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = v.Args[1L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)) x0:(MOVBloadidx1 [i0] {s} idx p mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s0:(SHLLconst [8] x1:(MOVBloadidx1 [i1] {s} idx p mem)) x0:(MOVBloadidx1 [i0] {s} idx p mem))
            // cond: i1==i0+1   && x0.Uses == 1   && x1.Uses == 1   && s0.Uses == 1   && mergePoint(b,x0,x1) != nil   && clobber(x0)   && clobber(x1)   && clobber(s0)
            // result: @mergePoint(b,x0,x1) (MOVWloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s0 = v.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 8L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i1 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                idx = x1.Args[0L];
                p = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = v.Args[1L];
                if (x0.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i1 == i0 + 1L && x0.Uses == 1L && x1.Uses == 1L && s0.Uses == 1L && mergePoint(b, x0, x1) != null && clobber(x0) && clobber(x1) && clobber(s0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1);
                v0 = b.NewValue0(v.Pos, Op386MOVWloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                var o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                p = x0.Args[0L];
                idx = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                var s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                var x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                idx = x0.Args[0L];
                p = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                p = x0.Args[0L];
                idx = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                idx = x0.Args[0L];
                p = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                p = x1.Args[0L];
                idx = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                idx = x1.Args[0L];
                p = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                p = x1.Args[0L];
                idx = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORL_30(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                var o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                var s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                var x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i2 = x1.AuxInt;
                var s = x1.Aux;
                _ = x1.Args[2L];
                var idx = x1.Args[0L];
                var p = x1.Args[1L];
                var mem = x1.Args[2L];
                var x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                var i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                var s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                var x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (p != x2.Args[0L])
                {
                    break;
                }
                if (idx != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                var v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                p = x0.Args[0L];
                idx = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                idx = x0.Args[0L];
                p = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                p = x0.Args[0L];
                idx = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                s = x0.Aux;
                _ = x0.Args[2L];
                idx = x0.Args[0L];
                p = x0.Args[1L];
                mem = x0.Args[2L];
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                p = x1.Args[0L];
                idx = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                idx = x1.Args[0L];
                p = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                p = x1.Args[0L];
                idx = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)) s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                o0 = v.Args[0L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                s = x1.Aux;
                _ = x1.Args[2L];
                idx = x1.Args[0L];
                p = x1.Args[1L];
                mem = x1.Args[2L];
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s1 = v.Args[1L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                if (x2.Aux != s)
                {
                    break;
                }
                _ = x2.Args[2L];
                if (idx != x2.Args[0L])
                {
                    break;
                }
                if (p != x2.Args[1L])
                {
                    break;
                }
                if (mem != x2.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORL_40(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                var s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                var x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i3 = x2.AuxInt;
                var s = x2.Aux;
                _ = x2.Args[2L];
                var idx = x2.Args[0L];
                var p = x2.Args[1L];
                var mem = x2.Args[2L];
                var o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                var x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                var i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                var s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                var x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                var v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                idx = x2.Args[0L];
                p = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} p idx mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                idx = x2.Args[0L];
                p = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL x0:(MOVWloadidx1 [i0] {s} idx p mem) s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem))))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                idx = x2.Args[0L];
                p = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                x0 = o0.Args[0L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                s0 = o0.Args[1L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                idx = x2.Args[0L];
                p = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORL_50(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} p idx mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                var s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                var x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i3 = x2.AuxInt;
                var s = x2.Aux;
                _ = x2.Args[2L];
                var idx = x2.Args[0L];
                var p = x2.Args[1L];
                var mem = x2.Args[2L];
                var o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                var s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                var x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                var i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                var x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                var i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (p != x0.Args[0L])
                {
                    break;
                }
                if (idx != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                var v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} p idx mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                idx = x2.Args[0L];
                p = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (p != x1.Args[0L])
                {
                    break;
                }
                if (idx != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} p idx mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                p = x2.Args[0L];
                idx = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            } 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
 
            // match: (ORL s1:(SHLLconst [24] x2:(MOVBloadidx1 [i3] {s} idx p mem)) o0:(ORL s0:(SHLLconst [16] x1:(MOVBloadidx1 [i2] {s} idx p mem)) x0:(MOVWloadidx1 [i0] {s} idx p mem)))
            // cond: i2 == i0+2   && i3 == i0+3   && x0.Uses == 1   && x1.Uses == 1   && x2.Uses == 1   && s0.Uses == 1   && s1.Uses == 1   && o0.Uses == 1   && mergePoint(b,x0,x1,x2) != nil   && clobber(x0)   && clobber(x1)   && clobber(x2)   && clobber(s0)   && clobber(s1)   && clobber(o0)
            // result: @mergePoint(b,x0,x1,x2) (MOVLloadidx1 <v.Type> [i0] {s} p idx mem)
            while (true)
            {
                _ = v.Args[1L];
                s1 = v.Args[0L];
                if (s1.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s1.AuxInt != 24L)
                {
                    break;
                }
                x2 = s1.Args[0L];
                if (x2.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i3 = x2.AuxInt;
                s = x2.Aux;
                _ = x2.Args[2L];
                idx = x2.Args[0L];
                p = x2.Args[1L];
                mem = x2.Args[2L];
                o0 = v.Args[1L];
                if (o0.Op != Op386ORL)
                {
                    break;
                }
                _ = o0.Args[1L];
                s0 = o0.Args[0L];
                if (s0.Op != Op386SHLLconst)
                {
                    break;
                }
                if (s0.AuxInt != 16L)
                {
                    break;
                }
                x1 = s0.Args[0L];
                if (x1.Op != Op386MOVBloadidx1)
                {
                    break;
                }
                i2 = x1.AuxInt;
                if (x1.Aux != s)
                {
                    break;
                }
                _ = x1.Args[2L];
                if (idx != x1.Args[0L])
                {
                    break;
                }
                if (p != x1.Args[1L])
                {
                    break;
                }
                if (mem != x1.Args[2L])
                {
                    break;
                }
                x0 = o0.Args[1L];
                if (x0.Op != Op386MOVWloadidx1)
                {
                    break;
                }
                i0 = x0.AuxInt;
                if (x0.Aux != s)
                {
                    break;
                }
                _ = x0.Args[2L];
                if (idx != x0.Args[0L])
                {
                    break;
                }
                if (p != x0.Args[1L])
                {
                    break;
                }
                if (mem != x0.Args[2L])
                {
                    break;
                }
                if (!(i2 == i0 + 2L && i3 == i0 + 3L && x0.Uses == 1L && x1.Uses == 1L && x2.Uses == 1L && s0.Uses == 1L && s1.Uses == 1L && o0.Uses == 1L && mergePoint(b, x0, x1, x2) != null && clobber(x0) && clobber(x1) && clobber(x2) && clobber(s0) && clobber(s1) && clobber(o0)))
                {
                    break;
                }
                b = mergePoint(b, x0, x1, x2);
                v0 = b.NewValue0(v.Pos, Op386MOVLloadidx1, v.Type);
                v.reset(OpCopy);
                v.AddArg(v0);
                v0.AuxInt = i0;
                v0.Aux = s;
                v0.AddArg(p);
                v0.AddArg(idx);
                v0.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ORLconst_0(ref Value v)
        { 
            // match: (ORLconst [c] x)
            // cond: int32(c)==0
            // result: x
            while (true)
            {
                var c = v.AuxInt;
                var x = v.Args[0L];
                if (!(int32(c) == 0L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (ORLconst [c] _)
            // cond: int32(c)==-1
            // result: (MOVLconst [-1])
 
            // match: (ORLconst [c] _)
            // cond: int32(c)==-1
            // result: (MOVLconst [-1])
            while (true)
            {
                c = v.AuxInt;
                if (!(int32(c) == -1L))
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = -1L;
                return true;
            } 
            // match: (ORLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [c|d])
 
            // match: (ORLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [c|d])
            while (true)
            {
                c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = c | d;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ROLBconst_0(ref Value v)
        { 
            // match: (ROLBconst [c] (ROLBconst [d] x))
            // cond:
            // result: (ROLBconst [(c+d)& 7] x)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ROLBconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                v.reset(Op386ROLBconst);
                v.AuxInt = (c + d) & 7L;
                v.AddArg(x);
                return true;
            } 
            // match: (ROLBconst [0] x)
            // cond:
            // result: x
 
            // match: (ROLBconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ROLLconst_0(ref Value v)
        { 
            // match: (ROLLconst [c] (ROLLconst [d] x))
            // cond:
            // result: (ROLLconst [(c+d)&31] x)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ROLLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                v.reset(Op386ROLLconst);
                v.AuxInt = (c + d) & 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (ROLLconst [0] x)
            // cond:
            // result: x
 
            // match: (ROLLconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386ROLWconst_0(ref Value v)
        { 
            // match: (ROLWconst [c] (ROLWconst [d] x))
            // cond:
            // result: (ROLWconst [(c+d)&15] x)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386ROLWconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                v.reset(Op386ROLWconst);
                v.AuxInt = (c + d) & 15L;
                v.AddArg(x);
                return true;
            } 
            // match: (ROLWconst [0] x)
            // cond:
            // result: x
 
            // match: (ROLWconst [0] x)
            // cond:
            // result: x
            while (true)
            {
                if (v.AuxInt != 0L)
                {
                    break;
                }
                x = v.Args[0L];
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SARB_0(ref Value v)
        { 
            // match: (SARB x (MOVLconst [c]))
            // cond:
            // result: (SARBconst [min(c&31,7)] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SARBconst);
                v.AuxInt = min(c & 31L, 7L);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SARBconst_0(ref Value v)
        { 
            // match: (SARBconst x [0])
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
            // match: (SARBconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [d>>uint64(c)])
 
            // match: (SARBconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [d>>uint64(c)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = d >> (int)(uint64(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SARL_0(ref Value v)
        { 
            // match: (SARL x (MOVLconst [c]))
            // cond:
            // result: (SARLconst [c&31] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SARLconst);
                v.AuxInt = c & 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (SARL x (ANDLconst [31] y))
            // cond:
            // result: (SARL x y)
 
            // match: (SARL x (ANDLconst [31] y))
            // cond:
            // result: (SARL x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ANDLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 31L)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(Op386SARL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SARLconst_0(ref Value v)
        { 
            // match: (SARLconst x [0])
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
            // match: (SARLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [d>>uint64(c)])
 
            // match: (SARLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [d>>uint64(c)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = d >> (int)(uint64(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SARW_0(ref Value v)
        { 
            // match: (SARW x (MOVLconst [c]))
            // cond:
            // result: (SARWconst [min(c&31,15)] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SARWconst);
                v.AuxInt = min(c & 31L, 15L);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SARWconst_0(ref Value v)
        { 
            // match: (SARWconst x [0])
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
            // match: (SARWconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [d>>uint64(c)])
 
            // match: (SARWconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [d>>uint64(c)])
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = d >> (int)(uint64(c));
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SBBL_0(ref Value v)
        { 
            // match: (SBBL x (MOVLconst [c]) f)
            // cond:
            // result: (SBBLconst [c] x f)
            while (true)
            {
                _ = v.Args[2L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                var f = v.Args[2L];
                v.reset(Op386SBBLconst);
                v.AuxInt = c;
                v.AddArg(x);
                v.AddArg(f);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SBBLcarrymask_0(ref Value v)
        { 
            // match: (SBBLcarrymask (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SBBLcarrymask (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [-1])
 
            // match: (SBBLcarrymask (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [-1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = -1L;
                return true;
            } 
            // match: (SBBLcarrymask (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SBBLcarrymask (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SBBLcarrymask (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [-1])
 
            // match: (SBBLcarrymask (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [-1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = -1L;
                return true;
            } 
            // match: (SBBLcarrymask (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SBBLcarrymask (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETA_0(ref Value v)
        { 
            // match: (SETA (InvertFlags x))
            // cond:
            // result: (SETB x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETB);
                v.AddArg(x);
                return true;
            } 
            // match: (SETA (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETA (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETA (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETA (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETA (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETA (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETA (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETA (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETA (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETA (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETAE_0(ref Value v)
        { 
            // match: (SETAE (InvertFlags x))
            // cond:
            // result: (SETBE x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETBE);
                v.AddArg(x);
                return true;
            } 
            // match: (SETAE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETAE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETAE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETAE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETAE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETAE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETAE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETAE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETAE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETAE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETB_0(ref Value v)
        { 
            // match: (SETB (InvertFlags x))
            // cond:
            // result: (SETA x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETA);
                v.AddArg(x);
                return true;
            } 
            // match: (SETB (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETB (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETB (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETB (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETB (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETB (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETB (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETB (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETB (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETB (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETBE_0(ref Value v)
        { 
            // match: (SETBE (InvertFlags x))
            // cond:
            // result: (SETAE x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETAE);
                v.AddArg(x);
                return true;
            } 
            // match: (SETBE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETBE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETBE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETBE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETBE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETBE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETBE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETBE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETBE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETBE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETEQ_0(ref Value v)
        { 
            // match: (SETEQ (InvertFlags x))
            // cond:
            // result: (SETEQ x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETEQ);
                v.AddArg(x);
                return true;
            } 
            // match: (SETEQ (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETEQ (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETEQ (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETEQ (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETEQ (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETEQ (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETEQ (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETEQ (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETEQ (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETEQ (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETG_0(ref Value v)
        { 
            // match: (SETG (InvertFlags x))
            // cond:
            // result: (SETL x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETL);
                v.AddArg(x);
                return true;
            } 
            // match: (SETG (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETG (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETG (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETG (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETG (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETG (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETG (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETG (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETG (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETG (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETGE_0(ref Value v)
        { 
            // match: (SETGE (InvertFlags x))
            // cond:
            // result: (SETLE x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETLE);
                v.AddArg(x);
                return true;
            } 
            // match: (SETGE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETGE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETGE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETGE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETGE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETGE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETGE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETGE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETGE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETGE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETL_0(ref Value v)
        { 
            // match: (SETL (InvertFlags x))
            // cond:
            // result: (SETG x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETG);
                v.AddArg(x);
                return true;
            } 
            // match: (SETL (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETL (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETL (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETL (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETL (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETL (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETL (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETL (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETL (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETL (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETLE_0(ref Value v)
        { 
            // match: (SETLE (InvertFlags x))
            // cond:
            // result: (SETGE x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETGE);
                v.AddArg(x);
                return true;
            } 
            // match: (SETLE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETLE (FlagEQ))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETLE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETLE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETLE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETLE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETLE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETLE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETLE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETLE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SETNE_0(ref Value v)
        { 
            // match: (SETNE (InvertFlags x))
            // cond:
            // result: (SETNE x)
            while (true)
            {
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386InvertFlags)
                {
                    break;
                }
                var x = v_0.Args[0L];
                v.reset(Op386SETNE);
                v.AddArg(x);
                return true;
            } 
            // match: (SETNE (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SETNE (FlagEQ))
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagEQ)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            } 
            // match: (SETNE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETNE (FlagLT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETNE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETNE (FlagLT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagLT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETNE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETNE (FlagGT_ULT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_ULT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            } 
            // match: (SETNE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
 
            // match: (SETNE (FlagGT_UGT))
            // cond:
            // result: (MOVLconst [1])
            while (true)
            {
                v_0 = v.Args[0L];
                if (v_0.Op != Op386FlagGT_UGT)
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 1L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SHLL_0(ref Value v)
        { 
            // match: (SHLL x (MOVLconst [c]))
            // cond:
            // result: (SHLLconst [c&31] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SHLLconst);
                v.AuxInt = c & 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (SHLL x (ANDLconst [31] y))
            // cond:
            // result: (SHLL x y)
 
            // match: (SHLL x (ANDLconst [31] y))
            // cond:
            // result: (SHLL x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ANDLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 31L)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(Op386SHLL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SHLLconst_0(ref Value v)
        { 
            // match: (SHLLconst x [0])
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

            return false;
        }
        private static bool rewriteValue386_Op386SHRB_0(ref Value v)
        { 
            // match: (SHRB x (MOVLconst [c]))
            // cond: c&31 < 8
            // result: (SHRBconst [c&31] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(c & 31L < 8L))
                {
                    break;
                }
                v.reset(Op386SHRBconst);
                v.AuxInt = c & 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (SHRB _ (MOVLconst [c]))
            // cond: c&31 >= 8
            // result: (MOVLconst [0])
 
            // match: (SHRB _ (MOVLconst [c]))
            // cond: c&31 >= 8
            // result: (MOVLconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(c & 31L >= 8L))
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SHRBconst_0(ref Value v)
        { 
            // match: (SHRBconst x [0])
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

            return false;
        }
        private static bool rewriteValue386_Op386SHRL_0(ref Value v)
        { 
            // match: (SHRL x (MOVLconst [c]))
            // cond:
            // result: (SHRLconst [c&31] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SHRLconst);
                v.AuxInt = c & 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (SHRL x (ANDLconst [31] y))
            // cond:
            // result: (SHRL x y)
 
            // match: (SHRL x (ANDLconst [31] y))
            // cond:
            // result: (SHRL x y)
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386ANDLconst)
                {
                    break;
                }
                if (v_1.AuxInt != 31L)
                {
                    break;
                }
                var y = v_1.Args[0L];
                v.reset(Op386SHRL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SHRLconst_0(ref Value v)
        { 
            // match: (SHRLconst x [0])
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

            return false;
        }
        private static bool rewriteValue386_Op386SHRW_0(ref Value v)
        { 
            // match: (SHRW x (MOVLconst [c]))
            // cond: c&31 < 16
            // result: (SHRWconst [c&31] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                if (!(c & 31L < 16L))
                {
                    break;
                }
                v.reset(Op386SHRWconst);
                v.AuxInt = c & 31L;
                v.AddArg(x);
                return true;
            } 
            // match: (SHRW _ (MOVLconst [c]))
            // cond: c&31 >= 16
            // result: (MOVLconst [0])
 
            // match: (SHRW _ (MOVLconst [c]))
            // cond: c&31 >= 16
            // result: (MOVLconst [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(c & 31L >= 16L))
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SHRWconst_0(ref Value v)
        { 
            // match: (SHRWconst x [0])
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

            return false;
        }
        private static bool rewriteValue386_Op386SUBL_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (SUBL x (MOVLconst [c]))
            // cond:
            // result: (SUBLconst x [c])
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SUBLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (SUBL (MOVLconst [c]) x)
            // cond:
            // result: (NEGL (SUBLconst <v.Type> x [c]))
 
            // match: (SUBL (MOVLconst [c]) x)
            // cond:
            // result: (NEGL (SUBLconst <v.Type> x [c]))
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386NEGL);
                var v0 = b.NewValue0(v.Pos, Op386SUBLconst, v.Type);
                v0.AuxInt = c;
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            } 
            // match: (SUBL x x)
            // cond:
            // result: (MOVLconst [0])
 
            // match: (SUBL x x)
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SUBLcarry_0(ref Value v)
        { 
            // match: (SUBLcarry x (MOVLconst [c]))
            // cond:
            // result: (SUBLconstcarry [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386SUBLconstcarry);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386SUBLconst_0(ref Value v)
        { 
            // match: (SUBLconst [c] x)
            // cond: int32(c) == 0
            // result: x
            while (true)
            {
                var c = v.AuxInt;
                var x = v.Args[0L];
                if (!(int32(c) == 0L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (SUBLconst [c] x)
            // cond:
            // result: (ADDLconst [int64(int32(-c))] x)
 
            // match: (SUBLconst [c] x)
            // cond:
            // result: (ADDLconst [int64(int32(-c))] x)
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                v.reset(Op386ADDLconst);
                v.AuxInt = int64(int32(-c));
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_Op386XORL_0(ref Value v)
        { 
            // match: (XORL x (MOVLconst [c]))
            // cond:
            // result: (XORLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var v_1 = v.Args[1L];
                if (v_1.Op != Op386MOVLconst)
                {
                    break;
                }
                var c = v_1.AuxInt;
                v.reset(Op386XORLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL (MOVLconst [c]) x)
            // cond:
            // result: (XORLconst [c] x)
 
            // match: (XORL (MOVLconst [c]) x)
            // cond:
            // result: (XORLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v.Args[1L];
                v.reset(Op386XORLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL (SHLLconst [c] x) (SHRLconst [d] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
 
            // match: (XORL (SHLLconst [c] x) (SHRLconst [d] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRLconst)
                {
                    break;
                }
                var d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(d == 32L - c))
                {
                    break;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL (SHRLconst [d] x) (SHLLconst [c] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
 
            // match: (XORL (SHRLconst [d] x) (SHLLconst [c] x))
            // cond: d == 32-c
            // result: (ROLLconst [c] x)
            while (true)
            {
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(d == 32L - c))
                {
                    break;
                }
                v.reset(Op386ROLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL <t> (SHLLconst x [c]) (SHRWconst x [d]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
 
            // match: (XORL <t> (SHLLconst x [c]) (SHRWconst x [d]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRWconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 16L && d == 16L - c && t.Size() == 2L))
                {
                    break;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL <t> (SHRWconst x [d]) (SHLLconst x [c]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
 
            // match: (XORL <t> (SHRWconst x [d]) (SHLLconst x [c]))
            // cond: c < 16 && d == 16-c && t.Size() == 2
            // result: (ROLWconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRWconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 16L && d == 16L - c && t.Size() == 2L))
                {
                    break;
                }
                v.reset(Op386ROLWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL <t> (SHLLconst x [c]) (SHRBconst x [d]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
 
            // match: (XORL <t> (SHLLconst x [c]) (SHRBconst x [d]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHRBconst)
                {
                    break;
                }
                d = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 8L && d == 8L - c && t.Size() == 1L))
                {
                    break;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL <t> (SHRBconst x [d]) (SHLLconst x [c]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
 
            // match: (XORL <t> (SHRBconst x [d]) (SHLLconst x [c]))
            // cond: c < 8 && d == 8-c && t.Size() == 1
            // result: (ROLBconst x [c])
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                v_0 = v.Args[0L];
                if (v_0.Op != Op386SHRBconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                x = v_0.Args[0L];
                v_1 = v.Args[1L];
                if (v_1.Op != Op386SHLLconst)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (x != v_1.Args[0L])
                {
                    break;
                }
                if (!(c < 8L && d == 8L - c && t.Size() == 1L))
                {
                    break;
                }
                v.reset(Op386ROLBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (XORL x x)
            // cond:
            // result: (MOVLconst [0])
 
            // match: (XORL x x)
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                _ = v.Args[1L];
                x = v.Args[0L];
                if (x != v.Args[1L])
                {
                    break;
                }
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_Op386XORLconst_0(ref Value v)
        { 
            // match: (XORLconst [c] (XORLconst [d] x))
            // cond:
            // result: (XORLconst [c ^ d] x)
            while (true)
            {
                var c = v.AuxInt;
                var v_0 = v.Args[0L];
                if (v_0.Op != Op386XORLconst)
                {
                    break;
                }
                var d = v_0.AuxInt;
                var x = v_0.Args[0L];
                v.reset(Op386XORLconst);
                v.AuxInt = c ^ d;
                v.AddArg(x);
                return true;
            } 
            // match: (XORLconst [c] x)
            // cond: int32(c)==0
            // result: x
 
            // match: (XORLconst [c] x)
            // cond: int32(c)==0
            // result: x
            while (true)
            {
                c = v.AuxInt;
                x = v.Args[0L];
                if (!(int32(c) == 0L))
                {
                    break;
                }
                v.reset(OpCopy);
                v.Type = x.Type;
                v.AddArg(x);
                return true;
            } 
            // match: (XORLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [c^d])
 
            // match: (XORLconst [c] (MOVLconst [d]))
            // cond:
            // result: (MOVLconst [c^d])
            while (true)
            {
                c = v.AuxInt;
                v_0 = v.Args[0L];
                if (v_0.Op != Op386MOVLconst)
                {
                    break;
                }
                d = v_0.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = c ^ d;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpAdd16_0(ref Value v)
        { 
            // match: (Add16 x y)
            // cond:
            // result: (ADDL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAdd32_0(ref Value v)
        { 
            // match: (Add32 x y)
            // cond:
            // result: (ADDL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAdd32F_0(ref Value v)
        { 
            // match: (Add32F x y)
            // cond:
            // result: (ADDSS x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDSS);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAdd32carry_0(ref Value v)
        { 
            // match: (Add32carry x y)
            // cond:
            // result: (ADDLcarry x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDLcarry);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAdd32withcarry_0(ref Value v)
        { 
            // match: (Add32withcarry x y c)
            // cond:
            // result: (ADCL x y c)
            while (true)
            {
                _ = v.Args[2L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                var c = v.Args[2L];
                v.reset(Op386ADCL);
                v.AddArg(x);
                v.AddArg(y);
                v.AddArg(c);
                return true;
            }

        }
        private static bool rewriteValue386_OpAdd64F_0(ref Value v)
        { 
            // match: (Add64F x y)
            // cond:
            // result: (ADDSD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDSD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAdd8_0(ref Value v)
        { 
            // match: (Add8 x y)
            // cond:
            // result: (ADDL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAddPtr_0(ref Value v)
        { 
            // match: (AddPtr x y)
            // cond:
            // result: (ADDL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ADDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAddr_0(ref Value v)
        { 
            // match: (Addr {sym} base)
            // cond:
            // result: (LEAL {sym} base)
            while (true)
            {
                var sym = v.Aux;
                var @base = v.Args[0L];
                v.reset(Op386LEAL);
                v.Aux = sym;
                v.AddArg(base);
                return true;
            }

        }
        private static bool rewriteValue386_OpAnd16_0(ref Value v)
        { 
            // match: (And16 x y)
            // cond:
            // result: (ANDL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAnd32_0(ref Value v)
        { 
            // match: (And32 x y)
            // cond:
            // result: (ANDL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAnd8_0(ref Value v)
        { 
            // match: (And8 x y)
            // cond:
            // result: (ANDL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAndB_0(ref Value v)
        { 
            // match: (AndB x y)
            // cond:
            // result: (ANDL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpAvg32u_0(ref Value v)
        { 
            // match: (Avg32u x y)
            // cond:
            // result: (AVGLU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386AVGLU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpBswap32_0(ref Value v)
        { 
            // match: (Bswap32 x)
            // cond:
            // result: (BSWAPL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386BSWAPL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpClosureCall_0(ref Value v)
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
                v.reset(Op386CALLclosure);
                v.AuxInt = argwid;
                v.AddArg(entry);
                v.AddArg(closure);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValue386_OpCom16_0(ref Value v)
        { 
            // match: (Com16 x)
            // cond:
            // result: (NOTL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386NOTL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCom32_0(ref Value v)
        { 
            // match: (Com32 x)
            // cond:
            // result: (NOTL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386NOTL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCom8_0(ref Value v)
        { 
            // match: (Com8 x)
            // cond:
            // result: (NOTL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386NOTL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpConst16_0(ref Value v)
        { 
            // match: (Const16 [val])
            // cond:
            // result: (MOVLconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValue386_OpConst32_0(ref Value v)
        { 
            // match: (Const32 [val])
            // cond:
            // result: (MOVLconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValue386_OpConst32F_0(ref Value v)
        { 
            // match: (Const32F [val])
            // cond:
            // result: (MOVSSconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(Op386MOVSSconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValue386_OpConst64F_0(ref Value v)
        { 
            // match: (Const64F [val])
            // cond:
            // result: (MOVSDconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(Op386MOVSDconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValue386_OpConst8_0(ref Value v)
        { 
            // match: (Const8 [val])
            // cond:
            // result: (MOVLconst [val])
            while (true)
            {
                var val = v.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = val;
                return true;
            }

        }
        private static bool rewriteValue386_OpConstBool_0(ref Value v)
        { 
            // match: (ConstBool [b])
            // cond:
            // result: (MOVLconst [b])
            while (true)
            {
                var b = v.AuxInt;
                v.reset(Op386MOVLconst);
                v.AuxInt = b;
                return true;
            }

        }
        private static bool rewriteValue386_OpConstNil_0(ref Value v)
        { 
            // match: (ConstNil)
            // cond:
            // result: (MOVLconst [0])
            while (true)
            {
                v.reset(Op386MOVLconst);
                v.AuxInt = 0L;
                return true;
            }

        }
        private static bool rewriteValue386_OpConvert_0(ref Value v)
        { 
            // match: (Convert <t> x mem)
            // cond:
            // result: (MOVLconvert <t> x mem)
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(Op386MOVLconvert);
                v.Type = t;
                v.AddArg(x);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValue386_OpCvt32Fto32_0(ref Value v)
        { 
            // match: (Cvt32Fto32 x)
            // cond:
            // result: (CVTTSS2SL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386CVTTSS2SL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCvt32Fto64F_0(ref Value v)
        { 
            // match: (Cvt32Fto64F x)
            // cond:
            // result: (CVTSS2SD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386CVTSS2SD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCvt32to32F_0(ref Value v)
        { 
            // match: (Cvt32to32F x)
            // cond:
            // result: (CVTSL2SS x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386CVTSL2SS);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCvt32to64F_0(ref Value v)
        { 
            // match: (Cvt32to64F x)
            // cond:
            // result: (CVTSL2SD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386CVTSL2SD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCvt64Fto32_0(ref Value v)
        { 
            // match: (Cvt64Fto32 x)
            // cond:
            // result: (CVTTSD2SL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386CVTTSD2SL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpCvt64Fto32F_0(ref Value v)
        { 
            // match: (Cvt64Fto32F x)
            // cond:
            // result: (CVTSD2SS x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386CVTSD2SS);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv16_0(ref Value v)
        { 
            // match: (Div16 x y)
            // cond:
            // result: (DIVW  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVW);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv16u_0(ref Value v)
        { 
            // match: (Div16u x y)
            // cond:
            // result: (DIVWU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVWU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv32_0(ref Value v)
        { 
            // match: (Div32 x y)
            // cond:
            // result: (DIVL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv32F_0(ref Value v)
        { 
            // match: (Div32F x y)
            // cond:
            // result: (DIVSS x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVSS);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv32u_0(ref Value v)
        { 
            // match: (Div32u x y)
            // cond:
            // result: (DIVLU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVLU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv64F_0(ref Value v)
        { 
            // match: (Div64F x y)
            // cond:
            // result: (DIVSD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVSD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div8 x y)
            // cond:
            // result: (DIVW  (SignExt8to16 x) (SignExt8to16 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVW);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpDiv8u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Div8u x y)
            // cond:
            // result: (DIVWU (ZeroExt8to16 x) (ZeroExt8to16 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386DIVWU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpEq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq16 x y)
            // cond:
            // result: (SETEQ (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQ);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpEq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq32 x y)
            // cond:
            // result: (SETEQ (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQ);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpEq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq32F x y)
            // cond:
            // result: (SETEQF (UCOMISS x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpEq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq64F x y)
            // cond:
            // result: (SETEQF (UCOMISD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpEq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Eq8 x y)
            // cond:
            // result: (SETEQ (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQ);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpEqB_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (EqB x y)
            // cond:
            // result: (SETEQ (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQ);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpEqPtr_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (EqPtr x y)
            // cond:
            // result: (SETEQ (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETEQ);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq16 x y)
            // cond:
            // result: (SETGE (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGE);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq16U x y)
            // cond:
            // result: (SETAE (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETAE);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq32 x y)
            // cond:
            // result: (SETGE (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq32F x y)
            // cond:
            // result: (SETGEF (UCOMISS x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGEF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq32U x y)
            // cond:
            // result: (SETAE (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETAE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq64F x y)
            // cond:
            // result: (SETGEF (UCOMISD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGEF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq8 x y)
            // cond:
            // result: (SETGE (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGE);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGeq8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Geq8U x y)
            // cond:
            // result: (SETAE (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETAE);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGetCallerPC_0(ref Value v)
        { 
            // match: (GetCallerPC)
            // cond:
            // result: (LoweredGetCallerPC)
            while (true)
            {
                v.reset(Op386LoweredGetCallerPC);
                return true;
            }

        }
        private static bool rewriteValue386_OpGetCallerSP_0(ref Value v)
        { 
            // match: (GetCallerSP)
            // cond:
            // result: (LoweredGetCallerSP)
            while (true)
            {
                v.reset(Op386LoweredGetCallerSP);
                return true;
            }

        }
        private static bool rewriteValue386_OpGetClosurePtr_0(ref Value v)
        { 
            // match: (GetClosurePtr)
            // cond:
            // result: (LoweredGetClosurePtr)
            while (true)
            {
                v.reset(Op386LoweredGetClosurePtr);
                return true;
            }

        }
        private static bool rewriteValue386_OpGetG_0(ref Value v)
        { 
            // match: (GetG mem)
            // cond:
            // result: (LoweredGetG mem)
            while (true)
            {
                var mem = v.Args[0L];
                v.reset(Op386LoweredGetG);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater16 x y)
            // cond:
            // result: (SETG (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETG);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater16U x y)
            // cond:
            // result: (SETA (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETA);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater32 x y)
            // cond:
            // result: (SETG (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETG);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater32F x y)
            // cond:
            // result: (SETGF (UCOMISS x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater32U x y)
            // cond:
            // result: (SETA (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETA);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater64F x y)
            // cond:
            // result: (SETGF (UCOMISD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater8 x y)
            // cond:
            // result: (SETG (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETG);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpGreater8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Greater8U x y)
            // cond:
            // result: (SETA (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETA);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpHmul32_0(ref Value v)
        { 
            // match: (Hmul32 x y)
            // cond:
            // result: (HMULL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386HMULL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpHmul32u_0(ref Value v)
        { 
            // match: (Hmul32u x y)
            // cond:
            // result: (HMULLU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386HMULLU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpInterCall_0(ref Value v)
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
                v.reset(Op386CALLinter);
                v.AuxInt = argwid;
                v.AddArg(entry);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValue386_OpIsInBounds_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (IsInBounds idx len)
            // cond:
            // result: (SETB (CMPL idx len))
            while (true)
            {
                _ = v.Args[1L];
                var idx = v.Args[0L];
                var len = v.Args[1L];
                v.reset(Op386SETB);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(idx);
                v0.AddArg(len);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpIsNonNil_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (IsNonNil p)
            // cond:
            // result: (SETNE (TESTL p p))
            while (true)
            {
                var p = v.Args[0L];
                v.reset(Op386SETNE);
                var v0 = b.NewValue0(v.Pos, Op386TESTL, types.TypeFlags);
                v0.AddArg(p);
                v0.AddArg(p);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpIsSliceInBounds_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (IsSliceInBounds idx len)
            // cond:
            // result: (SETBE (CMPL idx len))
            while (true)
            {
                _ = v.Args[1L];
                var idx = v.Args[0L];
                var len = v.Args[1L];
                v.reset(Op386SETBE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(idx);
                v0.AddArg(len);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq16 x y)
            // cond:
            // result: (SETLE (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETLE);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq16U x y)
            // cond:
            // result: (SETBE (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETBE);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq32 x y)
            // cond:
            // result: (SETLE (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETLE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq32F x y)
            // cond:
            // result: (SETGEF (UCOMISS y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGEF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq32U x y)
            // cond:
            // result: (SETBE (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETBE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq64F x y)
            // cond:
            // result: (SETGEF (UCOMISD y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGEF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq8 x y)
            // cond:
            // result: (SETLE (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETLE);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLeq8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Leq8U x y)
            // cond:
            // result: (SETBE (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETBE);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less16 x y)
            // cond:
            // result: (SETL (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETL);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess16U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less16U x y)
            // cond:
            // result: (SETB (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETB);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less32 x y)
            // cond:
            // result: (SETL (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETL);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less32F x y)
            // cond:
            // result: (SETGF (UCOMISS y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess32U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less32U x y)
            // cond:
            // result: (SETB (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETB);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less64F x y)
            // cond:
            // result: (SETGF (UCOMISD y x))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETGF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
                v0.AddArg(y);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less8 x y)
            // cond:
            // result: (SETL (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETL);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLess8U_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Less8U x y)
            // cond:
            // result: (SETB (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETB);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpLoad_0(ref Value v)
        { 
            // match: (Load <t> ptr mem)
            // cond: (is32BitInt(t) || isPtr(t))
            // result: (MOVLload ptr mem)
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                if (!(is32BitInt(t) || isPtr(t)))
                {
                    break;
                }
                v.reset(Op386MOVLload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: is16BitInt(t)
            // result: (MOVWload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is16BitInt(t)
            // result: (MOVWload ptr mem)
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(is16BitInt(t)))
                {
                    break;
                }
                v.reset(Op386MOVWload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: (t.IsBoolean() || is8BitInt(t))
            // result: (MOVBload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: (t.IsBoolean() || is8BitInt(t))
            // result: (MOVBload ptr mem)
            while (true)
            {
                t = v.Type;
                _ = v.Args[1L];
                ptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(t.IsBoolean() || is8BitInt(t)))
                {
                    break;
                }
                v.reset(Op386MOVBload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: is32BitFloat(t)
            // result: (MOVSSload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is32BitFloat(t)
            // result: (MOVSSload ptr mem)
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
                v.reset(Op386MOVSSload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Load <t> ptr mem)
            // cond: is64BitFloat(t)
            // result: (MOVSDload ptr mem)
 
            // match: (Load <t> ptr mem)
            // cond: is64BitFloat(t)
            // result: (MOVSDload ptr mem)
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
                v.reset(Op386MOVSDload);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpLsh16x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh16x16 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh16x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh16x32 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh16x64_0(ref Value v)
        { 
            // match: (Lsh16x64 x (Const64 [c]))
            // cond: uint64(c) < 16
            // result: (SHLLconst x [c])
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
                if (!(uint64(c) < 16L))
                {
                    break;
                }
                v.reset(Op386SHLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Lsh16x64 _ (Const64 [c]))
            // cond: uint64(c) >= 16
            // result: (Const16 [0])
 
            // match: (Lsh16x64 _ (Const64 [c]))
            // cond: uint64(c) >= 16
            // result: (Const16 [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint64(c) >= 16L))
                {
                    break;
                }
                v.reset(OpConst16);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpLsh16x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh16x8 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh32x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh32x16 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh32x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh32x32 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh32x64_0(ref Value v)
        { 
            // match: (Lsh32x64 x (Const64 [c]))
            // cond: uint64(c) < 32
            // result: (SHLLconst x [c])
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
                if (!(uint64(c) < 32L))
                {
                    break;
                }
                v.reset(Op386SHLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Lsh32x64 _ (Const64 [c]))
            // cond: uint64(c) >= 32
            // result: (Const32 [0])
 
            // match: (Lsh32x64 _ (Const64 [c]))
            // cond: uint64(c) >= 32
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint64(c) >= 32L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpLsh32x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh32x8 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh8x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh8x16 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh8x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh8x32 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpLsh8x64_0(ref Value v)
        { 
            // match: (Lsh8x64 x (Const64 [c]))
            // cond: uint64(c) < 8
            // result: (SHLLconst x [c])
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
                if (!(uint64(c) < 8L))
                {
                    break;
                }
                v.reset(Op386SHLLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Lsh8x64 _ (Const64 [c]))
            // cond: uint64(c) >= 8
            // result: (Const8 [0])
 
            // match: (Lsh8x64 _ (Const64 [c]))
            // cond: uint64(c) >= 8
            // result: (Const8 [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint64(c) >= 8L))
                {
                    break;
                }
                v.reset(OpConst8);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpLsh8x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Lsh8x8 <t> x y)
            // cond:
            // result: (ANDL (SHLL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHLL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpMod16_0(ref Value v)
        { 
            // match: (Mod16 x y)
            // cond:
            // result: (MODW  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MODW);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMod16u_0(ref Value v)
        { 
            // match: (Mod16u x y)
            // cond:
            // result: (MODWU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MODWU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMod32_0(ref Value v)
        { 
            // match: (Mod32 x y)
            // cond:
            // result: (MODL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MODL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMod32u_0(ref Value v)
        { 
            // match: (Mod32u x y)
            // cond:
            // result: (MODLU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MODLU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMod8_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod8 x y)
            // cond:
            // result: (MODW  (SignExt8to16 x) (SignExt8to16 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MODW);
                var v0 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpSignExt8to16, typ.Int16);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpMod8u_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Mod8u x y)
            // cond:
            // result: (MODWU (ZeroExt8to16 x) (ZeroExt8to16 y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MODWU);
                var v0 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
                v0.AddArg(x);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, OpZeroExt8to16, typ.UInt16);
                v1.AddArg(y);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpMove_0(ref Value v)
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
                v.reset(Op386MOVBstore);
                v.AddArg(dst);
                var v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [2] dst src mem)
            // cond:
            // result: (MOVWstore dst (MOVWload src mem) mem)
 
            // match: (Move [2] dst src mem)
            // cond:
            // result: (MOVWstore dst (MOVWload src mem) mem)
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
                v.reset(Op386MOVWstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [4] dst src mem)
            // cond:
            // result: (MOVLstore dst (MOVLload src mem) mem)
 
            // match: (Move [4] dst src mem)
            // cond:
            // result: (MOVLstore dst (MOVLload src mem) mem)
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
                v.reset(Op386MOVLstore);
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [3] dst src mem)
            // cond:
            // result: (MOVBstore [2] dst (MOVBload [2] src mem)         (MOVWstore dst (MOVWload src mem) mem))
 
            // match: (Move [3] dst src mem)
            // cond:
            // result: (MOVBstore [2] dst (MOVBload [2] src mem)         (MOVWstore dst (MOVWload src mem) mem))
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
                v.reset(Op386MOVBstore);
                v.AuxInt = 2L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
                v0.AuxInt = 2L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386MOVWstore, types.TypeMem);
                v1.AddArg(dst);
                var v2 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [5] dst src mem)
            // cond:
            // result: (MOVBstore [4] dst (MOVBload [4] src mem)         (MOVLstore dst (MOVLload src mem) mem))
 
            // match: (Move [5] dst src mem)
            // cond:
            // result: (MOVBstore [4] dst (MOVBload [4] src mem)         (MOVLstore dst (MOVLload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 5L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVBstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVBload, typ.UInt8);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [6] dst src mem)
            // cond:
            // result: (MOVWstore [4] dst (MOVWload [4] src mem)         (MOVLstore dst (MOVLload src mem) mem))
 
            // match: (Move [6] dst src mem)
            // cond:
            // result: (MOVWstore [4] dst (MOVWload [4] src mem)         (MOVLstore dst (MOVLload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 6L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVWstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVWload, typ.UInt16);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [7] dst src mem)
            // cond:
            // result: (MOVLstore [3] dst (MOVLload [3] src mem)         (MOVLstore dst (MOVLload src mem) mem))
 
            // match: (Move [7] dst src mem)
            // cond:
            // result: (MOVLstore [3] dst (MOVLload [3] src mem)         (MOVLstore dst (MOVLload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 7L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVLstore);
                v.AuxInt = 3L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v0.AuxInt = 3L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [8] dst src mem)
            // cond:
            // result: (MOVLstore [4] dst (MOVLload [4] src mem)         (MOVLstore dst (MOVLload src mem) mem))
 
            // match: (Move [8] dst src mem)
            // cond:
            // result: (MOVLstore [4] dst (MOVLload [4] src mem)         (MOVLstore dst (MOVLload src mem) mem))
            while (true)
            {
                if (v.AuxInt != 8L)
                {
                    break;
                }
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                v.reset(Op386MOVLstore);
                v.AuxInt = 4L;
                v.AddArg(dst);
                v0 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v0.AuxInt = 4L;
                v0.AddArg(src);
                v0.AddArg(mem);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
                v1.AddArg(dst);
                v2 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v2.AddArg(src);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Move [s] dst src mem)
            // cond: s > 8 && s%4 != 0
            // result: (Move [s-s%4]         (ADDLconst <dst.Type> dst [s%4])         (ADDLconst <src.Type> src [s%4])         (MOVLstore dst (MOVLload src mem) mem))
 
            // match: (Move [s] dst src mem)
            // cond: s > 8 && s%4 != 0
            // result: (Move [s-s%4]         (ADDLconst <dst.Type> dst [s%4])         (ADDLconst <src.Type> src [s%4])         (MOVLstore dst (MOVLload src mem) mem))
            while (true)
            {
                var s = v.AuxInt;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!(s > 8L && s % 4L != 0L))
                {
                    break;
                }
                v.reset(OpMove);
                v.AuxInt = s - s % 4L;
                v0 = b.NewValue0(v.Pos, Op386ADDLconst, dst.Type);
                v0.AuxInt = s % 4L;
                v0.AddArg(dst);
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, Op386ADDLconst, src.Type);
                v1.AuxInt = s % 4L;
                v1.AddArg(src);
                v.AddArg(v1);
                v2 = b.NewValue0(v.Pos, Op386MOVLstore, types.TypeMem);
                v2.AddArg(dst);
                var v3 = b.NewValue0(v.Pos, Op386MOVLload, typ.UInt32);
                v3.AddArg(src);
                v3.AddArg(mem);
                v2.AddArg(v3);
                v2.AddArg(mem);
                v.AddArg(v2);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpMove_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Move [s] dst src mem)
            // cond: s > 8 && s <= 4*128 && s%4 == 0     && !config.noDuffDevice
            // result: (DUFFCOPY [10*(128-s/4)] dst src mem)
            while (true)
            {
                var s = v.AuxInt;
                _ = v.Args[2L];
                var dst = v.Args[0L];
                var src = v.Args[1L];
                var mem = v.Args[2L];
                if (!(s > 8L && s <= 4L * 128L && s % 4L == 0L && !config.noDuffDevice))
                {
                    break;
                }
                v.reset(Op386DUFFCOPY);
                v.AuxInt = 10L * (128L - s / 4L);
                v.AddArg(dst);
                v.AddArg(src);
                v.AddArg(mem);
                return true;
            } 
            // match: (Move [s] dst src mem)
            // cond: (s > 4*128 || config.noDuffDevice) && s%4 == 0
            // result: (REPMOVSL dst src (MOVLconst [s/4]) mem)
 
            // match: (Move [s] dst src mem)
            // cond: (s > 4*128 || config.noDuffDevice) && s%4 == 0
            // result: (REPMOVSL dst src (MOVLconst [s/4]) mem)
            while (true)
            {
                s = v.AuxInt;
                _ = v.Args[2L];
                dst = v.Args[0L];
                src = v.Args[1L];
                mem = v.Args[2L];
                if (!((s > 4L * 128L || config.noDuffDevice) && s % 4L == 0L))
                {
                    break;
                }
                v.reset(Op386REPMOVSL);
                v.AddArg(dst);
                v.AddArg(src);
                var v0 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
                v0.AuxInt = s / 4L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpMul16_0(ref Value v)
        { 
            // match: (Mul16 x y)
            // cond:
            // result: (MULL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MULL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMul32_0(ref Value v)
        { 
            // match: (Mul32 x y)
            // cond:
            // result: (MULL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MULL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMul32F_0(ref Value v)
        { 
            // match: (Mul32F x y)
            // cond:
            // result: (MULSS x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MULSS);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMul32uhilo_0(ref Value v)
        { 
            // match: (Mul32uhilo x y)
            // cond:
            // result: (MULLQU x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MULLQU);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMul64F_0(ref Value v)
        { 
            // match: (Mul64F x y)
            // cond:
            // result: (MULSD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MULSD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpMul8_0(ref Value v)
        { 
            // match: (Mul8 x y)
            // cond:
            // result: (MULL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386MULL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeg16_0(ref Value v)
        { 
            // match: (Neg16 x)
            // cond:
            // result: (NEGL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386NEGL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeg32_0(ref Value v)
        { 
            // match: (Neg32 x)
            // cond:
            // result: (NEGL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386NEGL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeg32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neg32F x)
            // cond: !config.use387
            // result: (PXOR x (MOVSSconst <typ.Float32> [f2i(math.Copysign(0, -1))]))
            while (true)
            {
                var x = v.Args[0L];
                if (!(!config.use387))
                {
                    break;
                }
                v.reset(Op386PXOR);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386MOVSSconst, typ.Float32);
                v0.AuxInt = f2i(math.Copysign(0L, -1L));
                v.AddArg(v0);
                return true;
            } 
            // match: (Neg32F x)
            // cond: config.use387
            // result: (FCHS x)
 
            // match: (Neg32F x)
            // cond: config.use387
            // result: (FCHS x)
            while (true)
            {
                x = v.Args[0L];
                if (!(config.use387))
                {
                    break;
                }
                v.reset(Op386FCHS);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpNeg64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Neg64F x)
            // cond: !config.use387
            // result: (PXOR x (MOVSDconst <typ.Float64> [f2i(math.Copysign(0, -1))]))
            while (true)
            {
                var x = v.Args[0L];
                if (!(!config.use387))
                {
                    break;
                }
                v.reset(Op386PXOR);
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386MOVSDconst, typ.Float64);
                v0.AuxInt = f2i(math.Copysign(0L, -1L));
                v.AddArg(v0);
                return true;
            } 
            // match: (Neg64F x)
            // cond: config.use387
            // result: (FCHS x)
 
            // match: (Neg64F x)
            // cond: config.use387
            // result: (FCHS x)
            while (true)
            {
                x = v.Args[0L];
                if (!(config.use387))
                {
                    break;
                }
                v.reset(Op386FCHS);
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpNeg8_0(ref Value v)
        { 
            // match: (Neg8 x)
            // cond:
            // result: (NEGL x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386NEGL);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeq16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq16 x y)
            // cond:
            // result: (SETNE (CMPW x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNE);
                var v0 = b.NewValue0(v.Pos, Op386CMPW, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeq32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq32 x y)
            // cond:
            // result: (SETNE (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeq32F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq32F x y)
            // cond:
            // result: (SETNEF (UCOMISS x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNEF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISS, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeq64F_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq64F x y)
            // cond:
            // result: (SETNEF (UCOMISD x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNEF);
                var v0 = b.NewValue0(v.Pos, Op386UCOMISD, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeq8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Neq8 x y)
            // cond:
            // result: (SETNE (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNE);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeqB_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (NeqB x y)
            // cond:
            // result: (SETNE (CMPB x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNE);
                var v0 = b.NewValue0(v.Pos, Op386CMPB, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNeqPtr_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (NeqPtr x y)
            // cond:
            // result: (SETNE (CMPL x y))
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SETNE);
                var v0 = b.NewValue0(v.Pos, Op386CMPL, types.TypeFlags);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpNilCheck_0(ref Value v)
        { 
            // match: (NilCheck ptr mem)
            // cond:
            // result: (LoweredNilCheck ptr mem)
            while (true)
            {
                _ = v.Args[1L];
                var ptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(Op386LoweredNilCheck);
                v.AddArg(ptr);
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValue386_OpNot_0(ref Value v)
        { 
            // match: (Not x)
            // cond:
            // result: (XORLconst [1] x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386XORLconst);
                v.AuxInt = 1L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpOffPtr_0(ref Value v)
        { 
            // match: (OffPtr [off] ptr)
            // cond:
            // result: (ADDLconst [off] ptr)
            while (true)
            {
                var off = v.AuxInt;
                var ptr = v.Args[0L];
                v.reset(Op386ADDLconst);
                v.AuxInt = off;
                v.AddArg(ptr);
                return true;
            }

        }
        private static bool rewriteValue386_OpOr16_0(ref Value v)
        { 
            // match: (Or16 x y)
            // cond:
            // result: (ORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpOr32_0(ref Value v)
        { 
            // match: (Or32 x y)
            // cond:
            // result: (ORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpOr8_0(ref Value v)
        { 
            // match: (Or8 x y)
            // cond:
            // result: (ORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpOrB_0(ref Value v)
        { 
            // match: (OrB x y)
            // cond:
            // result: (ORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpRound32F_0(ref Value v)
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
        private static bool rewriteValue386_OpRound64F_0(ref Value v)
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
        private static bool rewriteValue386_OpRsh16Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh16Ux16 <t> x y)
            // cond:
            // result: (ANDL (SHRW <t> x y) (SBBLcarrymask <t> (CMPWconst y [16])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRW, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v2.AuxInt = 16L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh16Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh16Ux32 <t> x y)
            // cond:
            // result: (ANDL (SHRW <t> x y) (SBBLcarrymask <t> (CMPLconst y [16])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRW, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v2.AuxInt = 16L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh16Ux64_0(ref Value v)
        { 
            // match: (Rsh16Ux64 x (Const64 [c]))
            // cond: uint64(c) < 16
            // result: (SHRWconst x [c])
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
                if (!(uint64(c) < 16L))
                {
                    break;
                }
                v.reset(Op386SHRWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh16Ux64 _ (Const64 [c]))
            // cond: uint64(c) >= 16
            // result: (Const16 [0])
 
            // match: (Rsh16Ux64 _ (Const64 [c]))
            // cond: uint64(c) >= 16
            // result: (Const16 [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint64(c) >= 16L))
                {
                    break;
                }
                v.reset(OpConst16);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpRsh16Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh16Ux8 <t> x y)
            // cond:
            // result: (ANDL (SHRW <t> x y) (SBBLcarrymask <t> (CMPBconst y [16])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRW, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v2.AuxInt = 16L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh16x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh16x16 <t> x y)
            // cond:
            // result: (SARW <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPWconst y [16])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARW);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v3.AuxInt = 16L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh16x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh16x32 <t> x y)
            // cond:
            // result: (SARW <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPLconst y [16])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARW);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v3.AuxInt = 16L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh16x64_0(ref Value v)
        { 
            // match: (Rsh16x64 x (Const64 [c]))
            // cond: uint64(c) < 16
            // result: (SARWconst x [c])
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
                if (!(uint64(c) < 16L))
                {
                    break;
                }
                v.reset(Op386SARWconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh16x64 x (Const64 [c]))
            // cond: uint64(c) >= 16
            // result: (SARWconst x [15])
 
            // match: (Rsh16x64 x (Const64 [c]))
            // cond: uint64(c) >= 16
            // result: (SARWconst x [15])
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
                if (!(uint64(c) >= 16L))
                {
                    break;
                }
                v.reset(Op386SARWconst);
                v.AuxInt = 15L;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpRsh16x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh16x8 <t> x y)
            // cond:
            // result: (SARW <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPBconst y [16])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARW);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v3.AuxInt = 16L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh32Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh32Ux16 <t> x y)
            // cond:
            // result: (ANDL (SHRL <t> x y) (SBBLcarrymask <t> (CMPWconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh32Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh32Ux32 <t> x y)
            // cond:
            // result: (ANDL (SHRL <t> x y) (SBBLcarrymask <t> (CMPLconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh32Ux64_0(ref Value v)
        { 
            // match: (Rsh32Ux64 x (Const64 [c]))
            // cond: uint64(c) < 32
            // result: (SHRLconst x [c])
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
                if (!(uint64(c) < 32L))
                {
                    break;
                }
                v.reset(Op386SHRLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh32Ux64 _ (Const64 [c]))
            // cond: uint64(c) >= 32
            // result: (Const32 [0])
 
            // match: (Rsh32Ux64 _ (Const64 [c]))
            // cond: uint64(c) >= 32
            // result: (Const32 [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint64(c) >= 32L))
                {
                    break;
                }
                v.reset(OpConst32);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpRsh32Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh32Ux8 <t> x y)
            // cond:
            // result: (ANDL (SHRL <t> x y) (SBBLcarrymask <t> (CMPBconst y [32])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRL, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v2.AuxInt = 32L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh32x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh32x16 <t> x y)
            // cond:
            // result: (SARL <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPWconst y [32])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARL);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh32x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh32x32 <t> x y)
            // cond:
            // result: (SARL <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPLconst y [32])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARL);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh32x64_0(ref Value v)
        { 
            // match: (Rsh32x64 x (Const64 [c]))
            // cond: uint64(c) < 32
            // result: (SARLconst x [c])
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
                if (!(uint64(c) < 32L))
                {
                    break;
                }
                v.reset(Op386SARLconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh32x64 x (Const64 [c]))
            // cond: uint64(c) >= 32
            // result: (SARLconst x [31])
 
            // match: (Rsh32x64 x (Const64 [c]))
            // cond: uint64(c) >= 32
            // result: (SARLconst x [31])
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
                if (!(uint64(c) >= 32L))
                {
                    break;
                }
                v.reset(Op386SARLconst);
                v.AuxInt = 31L;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpRsh32x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh32x8 <t> x y)
            // cond:
            // result: (SARL <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPBconst y [32])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARL);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v3.AuxInt = 32L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh8Ux16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh8Ux16 <t> x y)
            // cond:
            // result: (ANDL (SHRB <t> x y) (SBBLcarrymask <t> (CMPWconst y [8])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRB, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v2.AuxInt = 8L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh8Ux32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh8Ux32 <t> x y)
            // cond:
            // result: (ANDL (SHRB <t> x y) (SBBLcarrymask <t> (CMPLconst y [8])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRB, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v2.AuxInt = 8L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh8Ux64_0(ref Value v)
        { 
            // match: (Rsh8Ux64 x (Const64 [c]))
            // cond: uint64(c) < 8
            // result: (SHRBconst x [c])
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
                if (!(uint64(c) < 8L))
                {
                    break;
                }
                v.reset(Op386SHRBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh8Ux64 _ (Const64 [c]))
            // cond: uint64(c) >= 8
            // result: (Const8 [0])
 
            // match: (Rsh8Ux64 _ (Const64 [c]))
            // cond: uint64(c) >= 8
            // result: (Const8 [0])
            while (true)
            {
                _ = v.Args[1L];
                v_1 = v.Args[1L];
                if (v_1.Op != OpConst64)
                {
                    break;
                }
                c = v_1.AuxInt;
                if (!(uint64(c) >= 8L))
                {
                    break;
                }
                v.reset(OpConst8);
                v.AuxInt = 0L;
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpRsh8Ux8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh8Ux8 <t> x y)
            // cond:
            // result: (ANDL (SHRB <t> x y) (SBBLcarrymask <t> (CMPBconst y [8])))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386ANDL);
                var v0 = b.NewValue0(v.Pos, Op386SHRB, t);
                v0.AddArg(x);
                v0.AddArg(y);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v2 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v2.AuxInt = 8L;
                v2.AddArg(y);
                v1.AddArg(v2);
                v.AddArg(v1);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh8x16_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh8x16 <t> x y)
            // cond:
            // result: (SARB <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPWconst y [8])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARB);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPWconst, types.TypeFlags);
                v3.AuxInt = 8L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh8x32_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh8x32 <t> x y)
            // cond:
            // result: (SARB <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPLconst y [8])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARB);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v3.AuxInt = 8L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpRsh8x64_0(ref Value v)
        { 
            // match: (Rsh8x64 x (Const64 [c]))
            // cond: uint64(c) < 8
            // result: (SARBconst x [c])
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
                if (!(uint64(c) < 8L))
                {
                    break;
                }
                v.reset(Op386SARBconst);
                v.AuxInt = c;
                v.AddArg(x);
                return true;
            } 
            // match: (Rsh8x64 x (Const64 [c]))
            // cond: uint64(c) >= 8
            // result: (SARBconst x [7])
 
            // match: (Rsh8x64 x (Const64 [c]))
            // cond: uint64(c) >= 8
            // result: (SARBconst x [7])
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
                if (!(uint64(c) >= 8L))
                {
                    break;
                }
                v.reset(Op386SARBconst);
                v.AuxInt = 7L;
                v.AddArg(x);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpRsh8x8_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Rsh8x8 <t> x y)
            // cond:
            // result: (SARB <t> x (ORL <y.Type> y (NOTL <y.Type> (SBBLcarrymask <y.Type> (CMPBconst y [8])))))
            while (true)
            {
                var t = v.Type;
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SARB);
                v.Type = t;
                v.AddArg(x);
                var v0 = b.NewValue0(v.Pos, Op386ORL, y.Type);
                v0.AddArg(y);
                var v1 = b.NewValue0(v.Pos, Op386NOTL, y.Type);
                var v2 = b.NewValue0(v.Pos, Op386SBBLcarrymask, y.Type);
                var v3 = b.NewValue0(v.Pos, Op386CMPBconst, types.TypeFlags);
                v3.AuxInt = 8L;
                v3.AddArg(y);
                v2.AddArg(v3);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpSignExt16to32_0(ref Value v)
        { 
            // match: (SignExt16to32 x)
            // cond:
            // result: (MOVWLSX x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386MOVWLSX);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpSignExt8to16_0(ref Value v)
        { 
            // match: (SignExt8to16 x)
            // cond:
            // result: (MOVBLSX x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386MOVBLSX);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpSignExt8to32_0(ref Value v)
        { 
            // match: (SignExt8to32 x)
            // cond:
            // result: (MOVBLSX x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386MOVBLSX);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpSignmask_0(ref Value v)
        { 
            // match: (Signmask x)
            // cond:
            // result: (SARLconst x [31])
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386SARLconst);
                v.AuxInt = 31L;
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpSlicemask_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Slicemask <t> x)
            // cond:
            // result: (SARLconst (NEGL <t> x) [31])
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(Op386SARLconst);
                v.AuxInt = 31L;
                var v0 = b.NewValue0(v.Pos, Op386NEGL, t);
                v0.AddArg(x);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteValue386_OpSqrt_0(ref Value v)
        { 
            // match: (Sqrt x)
            // cond:
            // result: (SQRTSD x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386SQRTSD);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpStaticCall_0(ref Value v)
        { 
            // match: (StaticCall [argwid] {target} mem)
            // cond:
            // result: (CALLstatic [argwid] {target} mem)
            while (true)
            {
                var argwid = v.AuxInt;
                var target = v.Aux;
                var mem = v.Args[0L];
                v.reset(Op386CALLstatic);
                v.AuxInt = argwid;
                v.Aux = target;
                v.AddArg(mem);
                return true;
            }

        }
        private static bool rewriteValue386_OpStore_0(ref Value v)
        { 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 8 && is64BitFloat(val.Type)
            // result: (MOVSDstore ptr val mem)
            while (true)
            {
                var t = v.Aux;
                _ = v.Args[2L];
                var ptr = v.Args[0L];
                var val = v.Args[1L];
                var mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 8L && is64BitFloat(val.Type)))
                {
                    break;
                }
                v.reset(Op386MOVSDstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4 && is32BitFloat(val.Type)
            // result: (MOVSSstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4 && is32BitFloat(val.Type)
            // result: (MOVSSstore ptr val mem)
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
                v.reset(Op386MOVSSstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4
            // result: (MOVLstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 4
            // result: (MOVLstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 4L))
                {
                    break;
                }
                v.reset(Op386MOVLstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 2
            // result: (MOVWstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 2
            // result: (MOVWstore ptr val mem)
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
                v.reset(Op386MOVWstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            } 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 1
            // result: (MOVBstore ptr val mem)
 
            // match: (Store {t} ptr val mem)
            // cond: t.(*types.Type).Size() == 1
            // result: (MOVBstore ptr val mem)
            while (true)
            {
                t = v.Aux;
                _ = v.Args[2L];
                ptr = v.Args[0L];
                val = v.Args[1L];
                mem = v.Args[2L];
                if (!(t._<ref types.Type>().Size() == 1L))
                {
                    break;
                }
                v.reset(Op386MOVBstore);
                v.AddArg(ptr);
                v.AddArg(val);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpSub16_0(ref Value v)
        { 
            // match: (Sub16 x y)
            // cond:
            // result: (SUBL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpSub32_0(ref Value v)
        { 
            // match: (Sub32 x y)
            // cond:
            // result: (SUBL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpSub32F_0(ref Value v)
        { 
            // match: (Sub32F x y)
            // cond:
            // result: (SUBSS x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBSS);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpSub32carry_0(ref Value v)
        { 
            // match: (Sub32carry x y)
            // cond:
            // result: (SUBLcarry x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBLcarry);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpSub32withcarry_0(ref Value v)
        { 
            // match: (Sub32withcarry x y c)
            // cond:
            // result: (SBBL x y c)
            while (true)
            {
                _ = v.Args[2L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                var c = v.Args[2L];
                v.reset(Op386SBBL);
                v.AddArg(x);
                v.AddArg(y);
                v.AddArg(c);
                return true;
            }

        }
        private static bool rewriteValue386_OpSub64F_0(ref Value v)
        { 
            // match: (Sub64F x y)
            // cond:
            // result: (SUBSD x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBSD);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpSub8_0(ref Value v)
        { 
            // match: (Sub8 x y)
            // cond:
            // result: (SUBL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpSubPtr_0(ref Value v)
        { 
            // match: (SubPtr x y)
            // cond:
            // result: (SUBL  x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386SUBL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpTrunc16to8_0(ref Value v)
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
        private static bool rewriteValue386_OpTrunc32to16_0(ref Value v)
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
        private static bool rewriteValue386_OpTrunc32to8_0(ref Value v)
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
        private static bool rewriteValue386_OpXor16_0(ref Value v)
        { 
            // match: (Xor16 x y)
            // cond:
            // result: (XORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386XORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpXor32_0(ref Value v)
        { 
            // match: (Xor32 x y)
            // cond:
            // result: (XORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386XORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpXor8_0(ref Value v)
        { 
            // match: (Xor8 x y)
            // cond:
            // result: (XORL x y)
            while (true)
            {
                _ = v.Args[1L];
                var x = v.Args[0L];
                var y = v.Args[1L];
                v.reset(Op386XORL);
                v.AddArg(x);
                v.AddArg(y);
                return true;
            }

        }
        private static bool rewriteValue386_OpZero_0(ref Value v)
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
            // match: (Zero [1] destptr mem)
            // cond:
            // result: (MOVBstoreconst [0] destptr mem)
 
            // match: (Zero [1] destptr mem)
            // cond:
            // result: (MOVBstoreconst [0] destptr mem)
            while (true)
            {
                if (v.AuxInt != 1L)
                {
                    break;
                }
                _ = v.Args[1L];
                var destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVBstoreconst);
                v.AuxInt = 0L;
                v.AddArg(destptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [2] destptr mem)
            // cond:
            // result: (MOVWstoreconst [0] destptr mem)
 
            // match: (Zero [2] destptr mem)
            // cond:
            // result: (MOVWstoreconst [0] destptr mem)
            while (true)
            {
                if (v.AuxInt != 2L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVWstoreconst);
                v.AuxInt = 0L;
                v.AddArg(destptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [4] destptr mem)
            // cond:
            // result: (MOVLstoreconst [0] destptr mem)
 
            // match: (Zero [4] destptr mem)
            // cond:
            // result: (MOVLstoreconst [0] destptr mem)
            while (true)
            {
                if (v.AuxInt != 4L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = 0L;
                v.AddArg(destptr);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [3] destptr mem)
            // cond:
            // result: (MOVBstoreconst [makeValAndOff(0,2)] destptr         (MOVWstoreconst [0] destptr mem))
 
            // match: (Zero [3] destptr mem)
            // cond:
            // result: (MOVBstoreconst [makeValAndOff(0,2)] destptr         (MOVWstoreconst [0] destptr mem))
            while (true)
            {
                if (v.AuxInt != 3L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVBstoreconst);
                v.AuxInt = makeValAndOff(0L, 2L);
                v.AddArg(destptr);
                var v0 = b.NewValue0(v.Pos, Op386MOVWstoreconst, types.TypeMem);
                v0.AuxInt = 0L;
                v0.AddArg(destptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                return true;
            } 
            // match: (Zero [5] destptr mem)
            // cond:
            // result: (MOVBstoreconst [makeValAndOff(0,4)] destptr         (MOVLstoreconst [0] destptr mem))
 
            // match: (Zero [5] destptr mem)
            // cond:
            // result: (MOVBstoreconst [makeValAndOff(0,4)] destptr         (MOVLstoreconst [0] destptr mem))
            while (true)
            {
                if (v.AuxInt != 5L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVBstoreconst);
                v.AuxInt = makeValAndOff(0L, 4L);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v0.AuxInt = 0L;
                v0.AddArg(destptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                return true;
            } 
            // match: (Zero [6] destptr mem)
            // cond:
            // result: (MOVWstoreconst [makeValAndOff(0,4)] destptr         (MOVLstoreconst [0] destptr mem))
 
            // match: (Zero [6] destptr mem)
            // cond:
            // result: (MOVWstoreconst [makeValAndOff(0,4)] destptr         (MOVLstoreconst [0] destptr mem))
            while (true)
            {
                if (v.AuxInt != 6L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVWstoreconst);
                v.AuxInt = makeValAndOff(0L, 4L);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v0.AuxInt = 0L;
                v0.AddArg(destptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                return true;
            } 
            // match: (Zero [7] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,3)] destptr         (MOVLstoreconst [0] destptr mem))
 
            // match: (Zero [7] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,3)] destptr         (MOVLstoreconst [0] destptr mem))
            while (true)
            {
                if (v.AuxInt != 7L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = makeValAndOff(0L, 3L);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v0.AuxInt = 0L;
                v0.AddArg(destptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                return true;
            } 
            // match: (Zero [s] destptr mem)
            // cond: s%4 != 0 && s > 4
            // result: (Zero [s-s%4] (ADDLconst destptr [s%4])         (MOVLstoreconst [0] destptr mem))
 
            // match: (Zero [s] destptr mem)
            // cond: s%4 != 0 && s > 4
            // result: (Zero [s-s%4] (ADDLconst destptr [s%4])         (MOVLstoreconst [0] destptr mem))
            while (true)
            {
                var s = v.AuxInt;
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(s % 4L != 0L && s > 4L))
                {
                    break;
                }
                v.reset(OpZero);
                v.AuxInt = s - s % 4L;
                v0 = b.NewValue0(v.Pos, Op386ADDLconst, typ.UInt32);
                v0.AuxInt = s % 4L;
                v0.AddArg(destptr);
                v.AddArg(v0);
                var v1 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(destptr);
                v1.AddArg(mem);
                v.AddArg(v1);
                return true;
            } 
            // match: (Zero [8] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,4)] destptr         (MOVLstoreconst [0] destptr mem))
 
            // match: (Zero [8] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,4)] destptr         (MOVLstoreconst [0] destptr mem))
            while (true)
            {
                if (v.AuxInt != 8L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = makeValAndOff(0L, 4L);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v0.AuxInt = 0L;
                v0.AddArg(destptr);
                v0.AddArg(mem);
                v.AddArg(v0);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpZero_10(ref Value v)
        {
            var b = v.Block;
            _ = b;
            var config = b.Func.Config;
            _ = config;
            var typ = ref b.Func.Config.Types;
            _ = typ; 
            // match: (Zero [12] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,8)] destptr         (MOVLstoreconst [makeValAndOff(0,4)] destptr             (MOVLstoreconst [0] destptr mem)))
            while (true)
            {
                if (v.AuxInt != 12L)
                {
                    break;
                }
                _ = v.Args[1L];
                var destptr = v.Args[0L];
                var mem = v.Args[1L];
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = makeValAndOff(0L, 8L);
                v.AddArg(destptr);
                var v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v0.AuxInt = makeValAndOff(0L, 4L);
                v0.AddArg(destptr);
                var v1 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v1.AuxInt = 0L;
                v1.AddArg(destptr);
                v1.AddArg(mem);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            } 
            // match: (Zero [16] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,12)] destptr         (MOVLstoreconst [makeValAndOff(0,8)] destptr             (MOVLstoreconst [makeValAndOff(0,4)] destptr                 (MOVLstoreconst [0] destptr mem))))
 
            // match: (Zero [16] destptr mem)
            // cond:
            // result: (MOVLstoreconst [makeValAndOff(0,12)] destptr         (MOVLstoreconst [makeValAndOff(0,8)] destptr             (MOVLstoreconst [makeValAndOff(0,4)] destptr                 (MOVLstoreconst [0] destptr mem))))
            while (true)
            {
                if (v.AuxInt != 16L)
                {
                    break;
                }
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                v.reset(Op386MOVLstoreconst);
                v.AuxInt = makeValAndOff(0L, 12L);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v0.AuxInt = makeValAndOff(0L, 8L);
                v0.AddArg(destptr);
                v1 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v1.AuxInt = makeValAndOff(0L, 4L);
                v1.AddArg(destptr);
                var v2 = b.NewValue0(v.Pos, Op386MOVLstoreconst, types.TypeMem);
                v2.AuxInt = 0L;
                v2.AddArg(destptr);
                v2.AddArg(mem);
                v1.AddArg(v2);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            } 
            // match: (Zero [s] destptr mem)
            // cond: s > 16 && s <= 4*128 && s%4 == 0   && !config.noDuffDevice
            // result: (DUFFZERO [1*(128-s/4)] destptr (MOVLconst [0]) mem)
 
            // match: (Zero [s] destptr mem)
            // cond: s > 16 && s <= 4*128 && s%4 == 0   && !config.noDuffDevice
            // result: (DUFFZERO [1*(128-s/4)] destptr (MOVLconst [0]) mem)
            while (true)
            {
                var s = v.AuxInt;
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                if (!(s > 16L && s <= 4L * 128L && s % 4L == 0L && !config.noDuffDevice))
                {
                    break;
                }
                v.reset(Op386DUFFZERO);
                v.AuxInt = 1L * (128L - s / 4L);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
                v0.AuxInt = 0L;
                v.AddArg(v0);
                v.AddArg(mem);
                return true;
            } 
            // match: (Zero [s] destptr mem)
            // cond: (s > 4*128 || (config.noDuffDevice && s > 16))   && s%4 == 0
            // result: (REPSTOSL destptr (MOVLconst [s/4]) (MOVLconst [0]) mem)
 
            // match: (Zero [s] destptr mem)
            // cond: (s > 4*128 || (config.noDuffDevice && s > 16))   && s%4 == 0
            // result: (REPSTOSL destptr (MOVLconst [s/4]) (MOVLconst [0]) mem)
            while (true)
            {
                s = v.AuxInt;
                _ = v.Args[1L];
                destptr = v.Args[0L];
                mem = v.Args[1L];
                if (!((s > 4L * 128L || (config.noDuffDevice && s > 16L)) && s % 4L == 0L))
                {
                    break;
                }
                v.reset(Op386REPSTOSL);
                v.AddArg(destptr);
                v0 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
                v0.AuxInt = s / 4L;
                v.AddArg(v0);
                v1 = b.NewValue0(v.Pos, Op386MOVLconst, typ.UInt32);
                v1.AuxInt = 0L;
                v.AddArg(v1);
                v.AddArg(mem);
                return true;
            }

            return false;
        }
        private static bool rewriteValue386_OpZeroExt16to32_0(ref Value v)
        { 
            // match: (ZeroExt16to32 x)
            // cond:
            // result: (MOVWLZX x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386MOVWLZX);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpZeroExt8to16_0(ref Value v)
        { 
            // match: (ZeroExt8to16 x)
            // cond:
            // result: (MOVBLZX x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386MOVBLZX);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpZeroExt8to32_0(ref Value v)
        { 
            // match: (ZeroExt8to32 x)
            // cond:
            // result: (MOVBLZX x)
            while (true)
            {
                var x = v.Args[0L];
                v.reset(Op386MOVBLZX);
                v.AddArg(x);
                return true;
            }

        }
        private static bool rewriteValue386_OpZeromask_0(ref Value v)
        {
            var b = v.Block;
            _ = b; 
            // match: (Zeromask <t> x)
            // cond:
            // result: (XORLconst [-1] (SBBLcarrymask <t> (CMPLconst x [1])))
            while (true)
            {
                var t = v.Type;
                var x = v.Args[0L];
                v.reset(Op386XORLconst);
                v.AuxInt = -1L;
                var v0 = b.NewValue0(v.Pos, Op386SBBLcarrymask, t);
                var v1 = b.NewValue0(v.Pos, Op386CMPLconst, types.TypeFlags);
                v1.AuxInt = 1L;
                v1.AddArg(x);
                v0.AddArg(v1);
                v.AddArg(v0);
                return true;
            }

        }
        private static bool rewriteBlock386(ref Block b)
        {
            var config = b.Func.Config;
            _ = config;
            var fe = b.Func.fe;
            _ = fe;
            var typ = ref config.Types;
            _ = typ;

            if (b.Kind == Block386EQ) 
                // match: (EQ (InvertFlags cmp) yes no)
                // cond:
                // result: (EQ cmp yes no)
                while (true)
                {
                    var v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    var cmp = v.Args[0L];
                    b.Kind = Block386EQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (EQ (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (EQ (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (EQ (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (EQ (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (EQ (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (EQ (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (EQ (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (EQ (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (EQ (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == Block386GE) 
                // match: (GE (InvertFlags cmp) yes no)
                // cond:
                // result: (LE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386LE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (GE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (GE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (GE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (GE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (GE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (GE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (GE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (GE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == Block386GT) 
                // match: (GT (InvertFlags cmp) yes no)
                // cond:
                // result: (LT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386LT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (GT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (GT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (GT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (GT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (GT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (GT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (GT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (GT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (GT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (GT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == BlockIf) 
                // match: (If (SETL cmp) yes no)
                // cond:
                // result: (LT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETL)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386LT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETLE cmp) yes no)
                // cond:
                // result: (LE  cmp yes no)
 
                // match: (If (SETLE cmp) yes no)
                // cond:
                // result: (LE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETLE)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386LE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETG cmp) yes no)
                // cond:
                // result: (GT  cmp yes no)
 
                // match: (If (SETG cmp) yes no)
                // cond:
                // result: (GT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETG)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386GT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETGE cmp) yes no)
                // cond:
                // result: (GE  cmp yes no)
 
                // match: (If (SETGE cmp) yes no)
                // cond:
                // result: (GE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETGE)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386GE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETEQ cmp) yes no)
                // cond:
                // result: (EQ  cmp yes no)
 
                // match: (If (SETEQ cmp) yes no)
                // cond:
                // result: (EQ  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETEQ)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386EQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETNE cmp) yes no)
                // cond:
                // result: (NE  cmp yes no)
 
                // match: (If (SETNE cmp) yes no)
                // cond:
                // result: (NE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETNE)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386NE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETB cmp) yes no)
                // cond:
                // result: (ULT cmp yes no)
 
                // match: (If (SETB cmp) yes no)
                // cond:
                // result: (ULT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETB)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386ULT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETBE cmp) yes no)
                // cond:
                // result: (ULE cmp yes no)
 
                // match: (If (SETBE cmp) yes no)
                // cond:
                // result: (ULE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETBE)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386ULE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETA cmp) yes no)
                // cond:
                // result: (UGT cmp yes no)
 
                // match: (If (SETA cmp) yes no)
                // cond:
                // result: (UGT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETA)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETAE cmp) yes no)
                // cond:
                // result: (UGE cmp yes no)
 
                // match: (If (SETAE cmp) yes no)
                // cond:
                // result: (UGE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETAE)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETGF cmp) yes no)
                // cond:
                // result: (UGT  cmp yes no)
 
                // match: (If (SETGF cmp) yes no)
                // cond:
                // result: (UGT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETGF)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETGEF cmp) yes no)
                // cond:
                // result: (UGE  cmp yes no)
 
                // match: (If (SETGEF cmp) yes no)
                // cond:
                // result: (UGE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETGEF)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETEQF cmp) yes no)
                // cond:
                // result: (EQF  cmp yes no)
 
                // match: (If (SETEQF cmp) yes no)
                // cond:
                // result: (EQF  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETEQF)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386EQF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If (SETNEF cmp) yes no)
                // cond:
                // result: (NEF  cmp yes no)
 
                // match: (If (SETNEF cmp) yes no)
                // cond:
                // result: (NEF  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386SETNEF)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386NEF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (If cond yes no)
                // cond:
                // result: (NE (TESTB cond cond) yes no)
 
                // match: (If cond yes no)
                // cond:
                // result: (NE (TESTB cond cond) yes no)
                while (true)
                {
                    v = b.Control;
                    _ = v;
                    var cond = b.Control;
                    b.Kind = Block386NE;
                    var v0 = b.NewValue0(v.Pos, Op386TESTB, types.TypeFlags);
                    v0.AddArg(cond);
                    v0.AddArg(cond);
                    b.SetControl(v0);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == Block386LE) 
                // match: (LE (InvertFlags cmp) yes no)
                // cond:
                // result: (GE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386GE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (LE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (LE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (LE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (LE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (LE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (LE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (LE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == Block386LT) 
                // match: (LT (InvertFlags cmp) yes no)
                // cond:
                // result: (GT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386GT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (LT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (LT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (LT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (LT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (LT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (LT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (LT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (LT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (LT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == Block386NE) 
                // match: (NE (TESTB (SETL cmp) (SETL cmp)) yes no)
                // cond:
                // result: (LT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    var v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETL)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    var v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETL)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386LT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETL cmp) (SETL cmp)) yes no)
                // cond:
                // result: (LT  cmp yes no)
 
                // match: (NE (TESTB (SETL cmp) (SETL cmp)) yes no)
                // cond:
                // result: (LT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETL)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETL)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386LT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETLE cmp) (SETLE cmp)) yes no)
                // cond:
                // result: (LE  cmp yes no)
 
                // match: (NE (TESTB (SETLE cmp) (SETLE cmp)) yes no)
                // cond:
                // result: (LE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETLE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETLE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386LE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETLE cmp) (SETLE cmp)) yes no)
                // cond:
                // result: (LE  cmp yes no)
 
                // match: (NE (TESTB (SETLE cmp) (SETLE cmp)) yes no)
                // cond:
                // result: (LE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETLE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETLE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386LE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETG cmp) (SETG cmp)) yes no)
                // cond:
                // result: (GT  cmp yes no)
 
                // match: (NE (TESTB (SETG cmp) (SETG cmp)) yes no)
                // cond:
                // result: (GT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETG)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETG)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386GT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETG cmp) (SETG cmp)) yes no)
                // cond:
                // result: (GT  cmp yes no)
 
                // match: (NE (TESTB (SETG cmp) (SETG cmp)) yes no)
                // cond:
                // result: (GT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETG)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETG)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386GT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETGE cmp) (SETGE cmp)) yes no)
                // cond:
                // result: (GE  cmp yes no)
 
                // match: (NE (TESTB (SETGE cmp) (SETGE cmp)) yes no)
                // cond:
                // result: (GE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETGE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETGE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386GE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETGE cmp) (SETGE cmp)) yes no)
                // cond:
                // result: (GE  cmp yes no)
 
                // match: (NE (TESTB (SETGE cmp) (SETGE cmp)) yes no)
                // cond:
                // result: (GE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETGE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETGE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386GE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETEQ cmp) (SETEQ cmp)) yes no)
                // cond:
                // result: (EQ  cmp yes no)
 
                // match: (NE (TESTB (SETEQ cmp) (SETEQ cmp)) yes no)
                // cond:
                // result: (EQ  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETEQ)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETEQ)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386EQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETEQ cmp) (SETEQ cmp)) yes no)
                // cond:
                // result: (EQ  cmp yes no)
 
                // match: (NE (TESTB (SETEQ cmp) (SETEQ cmp)) yes no)
                // cond:
                // result: (EQ  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETEQ)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETEQ)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386EQ;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETNE cmp) (SETNE cmp)) yes no)
                // cond:
                // result: (NE  cmp yes no)
 
                // match: (NE (TESTB (SETNE cmp) (SETNE cmp)) yes no)
                // cond:
                // result: (NE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETNE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETNE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386NE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETNE cmp) (SETNE cmp)) yes no)
                // cond:
                // result: (NE  cmp yes no)
 
                // match: (NE (TESTB (SETNE cmp) (SETNE cmp)) yes no)
                // cond:
                // result: (NE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETNE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETNE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386NE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETB cmp) (SETB cmp)) yes no)
                // cond:
                // result: (ULT cmp yes no)
 
                // match: (NE (TESTB (SETB cmp) (SETB cmp)) yes no)
                // cond:
                // result: (ULT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETB)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETB)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386ULT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETB cmp) (SETB cmp)) yes no)
                // cond:
                // result: (ULT cmp yes no)
 
                // match: (NE (TESTB (SETB cmp) (SETB cmp)) yes no)
                // cond:
                // result: (ULT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETB)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETB)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386ULT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETBE cmp) (SETBE cmp)) yes no)
                // cond:
                // result: (ULE cmp yes no)
 
                // match: (NE (TESTB (SETBE cmp) (SETBE cmp)) yes no)
                // cond:
                // result: (ULE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETBE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETBE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386ULE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETBE cmp) (SETBE cmp)) yes no)
                // cond:
                // result: (ULE cmp yes no)
 
                // match: (NE (TESTB (SETBE cmp) (SETBE cmp)) yes no)
                // cond:
                // result: (ULE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETBE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETBE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386ULE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETA cmp) (SETA cmp)) yes no)
                // cond:
                // result: (UGT cmp yes no)
 
                // match: (NE (TESTB (SETA cmp) (SETA cmp)) yes no)
                // cond:
                // result: (UGT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETA)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETA)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETA cmp) (SETA cmp)) yes no)
                // cond:
                // result: (UGT cmp yes no)
 
                // match: (NE (TESTB (SETA cmp) (SETA cmp)) yes no)
                // cond:
                // result: (UGT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETA)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETA)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETAE cmp) (SETAE cmp)) yes no)
                // cond:
                // result: (UGE cmp yes no)
 
                // match: (NE (TESTB (SETAE cmp) (SETAE cmp)) yes no)
                // cond:
                // result: (UGE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETAE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETAE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETAE cmp) (SETAE cmp)) yes no)
                // cond:
                // result: (UGE cmp yes no)
 
                // match: (NE (TESTB (SETAE cmp) (SETAE cmp)) yes no)
                // cond:
                // result: (UGE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETAE)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETAE)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETGF cmp) (SETGF cmp)) yes no)
                // cond:
                // result: (UGT  cmp yes no)
 
                // match: (NE (TESTB (SETGF cmp) (SETGF cmp)) yes no)
                // cond:
                // result: (UGT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETGF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETGF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETGF cmp) (SETGF cmp)) yes no)
                // cond:
                // result: (UGT  cmp yes no)
 
                // match: (NE (TESTB (SETGF cmp) (SETGF cmp)) yes no)
                // cond:
                // result: (UGT  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETGF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETGF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETGEF cmp) (SETGEF cmp)) yes no)
                // cond:
                // result: (UGE  cmp yes no)
 
                // match: (NE (TESTB (SETGEF cmp) (SETGEF cmp)) yes no)
                // cond:
                // result: (UGE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETGEF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETGEF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETGEF cmp) (SETGEF cmp)) yes no)
                // cond:
                // result: (UGE  cmp yes no)
 
                // match: (NE (TESTB (SETGEF cmp) (SETGEF cmp)) yes no)
                // cond:
                // result: (UGE  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETGEF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETGEF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETEQF cmp) (SETEQF cmp)) yes no)
                // cond:
                // result: (EQF  cmp yes no)
 
                // match: (NE (TESTB (SETEQF cmp) (SETEQF cmp)) yes no)
                // cond:
                // result: (EQF  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETEQF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETEQF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386EQF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETEQF cmp) (SETEQF cmp)) yes no)
                // cond:
                // result: (EQF  cmp yes no)
 
                // match: (NE (TESTB (SETEQF cmp) (SETEQF cmp)) yes no)
                // cond:
                // result: (EQF  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETEQF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETEQF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386EQF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETNEF cmp) (SETNEF cmp)) yes no)
                // cond:
                // result: (NEF  cmp yes no)
 
                // match: (NE (TESTB (SETNEF cmp) (SETNEF cmp)) yes no)
                // cond:
                // result: (NEF  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETNEF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETNEF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386NEF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (TESTB (SETNEF cmp) (SETNEF cmp)) yes no)
                // cond:
                // result: (NEF  cmp yes no)
 
                // match: (NE (TESTB (SETNEF cmp) (SETNEF cmp)) yes no)
                // cond:
                // result: (NEF  cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386TESTB)
                    {
                        break;
                    }
                    _ = v.Args[1L];
                    v_0 = v.Args[0L];
                    if (v_0.Op != Op386SETNEF)
                    {
                        break;
                    }
                    cmp = v_0.Args[0L];
                    v_1 = v.Args[1L];
                    if (v_1.Op != Op386SETNEF)
                    {
                        break;
                    }
                    if (cmp != v_1.Args[0L])
                    {
                        break;
                    }
                    b.Kind = Block386NEF;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (InvertFlags cmp) yes no)
                // cond:
                // result: (NE cmp yes no)
 
                // match: (NE (InvertFlags cmp) yes no)
                // cond:
                // result: (NE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386NE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (NE (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (NE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (NE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (NE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (NE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (NE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (NE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == Block386UGE) 
                // match: (UGE (InvertFlags cmp) yes no)
                // cond:
                // result: (ULE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386ULE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (UGE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (UGE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (UGE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (UGE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (UGE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (UGE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (UGE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (UGE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (UGE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (UGE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == Block386UGT) 
                // match: (UGT (InvertFlags cmp) yes no)
                // cond:
                // result: (ULT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386ULT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (UGT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (UGT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (UGT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (UGT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (UGT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (UGT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (UGT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (UGT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (UGT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (UGT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                }
            else if (b.Kind == Block386ULE) 
                // match: (ULE (InvertFlags cmp) yes no)
                // cond:
                // result: (UGE cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386UGE;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (ULE (FlagEQ) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (ULE (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (ULE (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (ULE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (ULE (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (ULE (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
            else if (b.Kind == Block386ULT) 
                // match: (ULT (InvertFlags cmp) yes no)
                // cond:
                // result: (UGT cmp yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386InvertFlags)
                    {
                        break;
                    }
                    cmp = v.Args[0L];
                    b.Kind = Block386UGT;
                    b.SetControl(cmp);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (ULT (FlagEQ) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagEQ)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (ULT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (ULT (FlagLT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (ULT (FlagLT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagLT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                } 
                // match: (ULT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
 
                // match: (ULT (FlagGT_ULT) yes no)
                // cond:
                // result: (First nil yes no)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_ULT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    return true;
                } 
                // match: (ULT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
 
                // match: (ULT (FlagGT_UGT) yes no)
                // cond:
                // result: (First nil no yes)
                while (true)
                {
                    v = b.Control;
                    if (v.Op != Op386FlagGT_UGT)
                    {
                        break;
                    }
                    b.Kind = BlockFirst;
                    b.SetControl(null);
                    b.Aux = null;
                    b.swapSuccessors();
                    return true;
                }
                        return false;
        }
    }
}}}}
