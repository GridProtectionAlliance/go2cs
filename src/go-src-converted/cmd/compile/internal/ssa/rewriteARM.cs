// Code generated from gen/ARM.rules; DO NOT EDIT.
// generated with: cd gen; go run *.go

// package ssa -- go2cs converted at 2022 March 06 22:57:34 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\rewriteARM.go
using buildcfg = go.@internal.buildcfg_package;
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private static bool rewriteValueARM(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;


    if (v.Op == OpARMADC) 
        return rewriteValueARM_OpARMADC(_addr_v);
    else if (v.Op == OpARMADCconst) 
        return rewriteValueARM_OpARMADCconst(_addr_v);
    else if (v.Op == OpARMADCshiftLL) 
        return rewriteValueARM_OpARMADCshiftLL(_addr_v);
    else if (v.Op == OpARMADCshiftLLreg) 
        return rewriteValueARM_OpARMADCshiftLLreg(_addr_v);
    else if (v.Op == OpARMADCshiftRA) 
        return rewriteValueARM_OpARMADCshiftRA(_addr_v);
    else if (v.Op == OpARMADCshiftRAreg) 
        return rewriteValueARM_OpARMADCshiftRAreg(_addr_v);
    else if (v.Op == OpARMADCshiftRL) 
        return rewriteValueARM_OpARMADCshiftRL(_addr_v);
    else if (v.Op == OpARMADCshiftRLreg) 
        return rewriteValueARM_OpARMADCshiftRLreg(_addr_v);
    else if (v.Op == OpARMADD) 
        return rewriteValueARM_OpARMADD(_addr_v);
    else if (v.Op == OpARMADDD) 
        return rewriteValueARM_OpARMADDD(_addr_v);
    else if (v.Op == OpARMADDF) 
        return rewriteValueARM_OpARMADDF(_addr_v);
    else if (v.Op == OpARMADDS) 
        return rewriteValueARM_OpARMADDS(_addr_v);
    else if (v.Op == OpARMADDSshiftLL) 
        return rewriteValueARM_OpARMADDSshiftLL(_addr_v);
    else if (v.Op == OpARMADDSshiftLLreg) 
        return rewriteValueARM_OpARMADDSshiftLLreg(_addr_v);
    else if (v.Op == OpARMADDSshiftRA) 
        return rewriteValueARM_OpARMADDSshiftRA(_addr_v);
    else if (v.Op == OpARMADDSshiftRAreg) 
        return rewriteValueARM_OpARMADDSshiftRAreg(_addr_v);
    else if (v.Op == OpARMADDSshiftRL) 
        return rewriteValueARM_OpARMADDSshiftRL(_addr_v);
    else if (v.Op == OpARMADDSshiftRLreg) 
        return rewriteValueARM_OpARMADDSshiftRLreg(_addr_v);
    else if (v.Op == OpARMADDconst) 
        return rewriteValueARM_OpARMADDconst(_addr_v);
    else if (v.Op == OpARMADDshiftLL) 
        return rewriteValueARM_OpARMADDshiftLL(_addr_v);
    else if (v.Op == OpARMADDshiftLLreg) 
        return rewriteValueARM_OpARMADDshiftLLreg(_addr_v);
    else if (v.Op == OpARMADDshiftRA) 
        return rewriteValueARM_OpARMADDshiftRA(_addr_v);
    else if (v.Op == OpARMADDshiftRAreg) 
        return rewriteValueARM_OpARMADDshiftRAreg(_addr_v);
    else if (v.Op == OpARMADDshiftRL) 
        return rewriteValueARM_OpARMADDshiftRL(_addr_v);
    else if (v.Op == OpARMADDshiftRLreg) 
        return rewriteValueARM_OpARMADDshiftRLreg(_addr_v);
    else if (v.Op == OpARMAND) 
        return rewriteValueARM_OpARMAND(_addr_v);
    else if (v.Op == OpARMANDconst) 
        return rewriteValueARM_OpARMANDconst(_addr_v);
    else if (v.Op == OpARMANDshiftLL) 
        return rewriteValueARM_OpARMANDshiftLL(_addr_v);
    else if (v.Op == OpARMANDshiftLLreg) 
        return rewriteValueARM_OpARMANDshiftLLreg(_addr_v);
    else if (v.Op == OpARMANDshiftRA) 
        return rewriteValueARM_OpARMANDshiftRA(_addr_v);
    else if (v.Op == OpARMANDshiftRAreg) 
        return rewriteValueARM_OpARMANDshiftRAreg(_addr_v);
    else if (v.Op == OpARMANDshiftRL) 
        return rewriteValueARM_OpARMANDshiftRL(_addr_v);
    else if (v.Op == OpARMANDshiftRLreg) 
        return rewriteValueARM_OpARMANDshiftRLreg(_addr_v);
    else if (v.Op == OpARMBFX) 
        return rewriteValueARM_OpARMBFX(_addr_v);
    else if (v.Op == OpARMBFXU) 
        return rewriteValueARM_OpARMBFXU(_addr_v);
    else if (v.Op == OpARMBIC) 
        return rewriteValueARM_OpARMBIC(_addr_v);
    else if (v.Op == OpARMBICconst) 
        return rewriteValueARM_OpARMBICconst(_addr_v);
    else if (v.Op == OpARMBICshiftLL) 
        return rewriteValueARM_OpARMBICshiftLL(_addr_v);
    else if (v.Op == OpARMBICshiftLLreg) 
        return rewriteValueARM_OpARMBICshiftLLreg(_addr_v);
    else if (v.Op == OpARMBICshiftRA) 
        return rewriteValueARM_OpARMBICshiftRA(_addr_v);
    else if (v.Op == OpARMBICshiftRAreg) 
        return rewriteValueARM_OpARMBICshiftRAreg(_addr_v);
    else if (v.Op == OpARMBICshiftRL) 
        return rewriteValueARM_OpARMBICshiftRL(_addr_v);
    else if (v.Op == OpARMBICshiftRLreg) 
        return rewriteValueARM_OpARMBICshiftRLreg(_addr_v);
    else if (v.Op == OpARMCMN) 
        return rewriteValueARM_OpARMCMN(_addr_v);
    else if (v.Op == OpARMCMNconst) 
        return rewriteValueARM_OpARMCMNconst(_addr_v);
    else if (v.Op == OpARMCMNshiftLL) 
        return rewriteValueARM_OpARMCMNshiftLL(_addr_v);
    else if (v.Op == OpARMCMNshiftLLreg) 
        return rewriteValueARM_OpARMCMNshiftLLreg(_addr_v);
    else if (v.Op == OpARMCMNshiftRA) 
        return rewriteValueARM_OpARMCMNshiftRA(_addr_v);
    else if (v.Op == OpARMCMNshiftRAreg) 
        return rewriteValueARM_OpARMCMNshiftRAreg(_addr_v);
    else if (v.Op == OpARMCMNshiftRL) 
        return rewriteValueARM_OpARMCMNshiftRL(_addr_v);
    else if (v.Op == OpARMCMNshiftRLreg) 
        return rewriteValueARM_OpARMCMNshiftRLreg(_addr_v);
    else if (v.Op == OpARMCMOVWHSconst) 
        return rewriteValueARM_OpARMCMOVWHSconst(_addr_v);
    else if (v.Op == OpARMCMOVWLSconst) 
        return rewriteValueARM_OpARMCMOVWLSconst(_addr_v);
    else if (v.Op == OpARMCMP) 
        return rewriteValueARM_OpARMCMP(_addr_v);
    else if (v.Op == OpARMCMPD) 
        return rewriteValueARM_OpARMCMPD(_addr_v);
    else if (v.Op == OpARMCMPF) 
        return rewriteValueARM_OpARMCMPF(_addr_v);
    else if (v.Op == OpARMCMPconst) 
        return rewriteValueARM_OpARMCMPconst(_addr_v);
    else if (v.Op == OpARMCMPshiftLL) 
        return rewriteValueARM_OpARMCMPshiftLL(_addr_v);
    else if (v.Op == OpARMCMPshiftLLreg) 
        return rewriteValueARM_OpARMCMPshiftLLreg(_addr_v);
    else if (v.Op == OpARMCMPshiftRA) 
        return rewriteValueARM_OpARMCMPshiftRA(_addr_v);
    else if (v.Op == OpARMCMPshiftRAreg) 
        return rewriteValueARM_OpARMCMPshiftRAreg(_addr_v);
    else if (v.Op == OpARMCMPshiftRL) 
        return rewriteValueARM_OpARMCMPshiftRL(_addr_v);
    else if (v.Op == OpARMCMPshiftRLreg) 
        return rewriteValueARM_OpARMCMPshiftRLreg(_addr_v);
    else if (v.Op == OpARMEqual) 
        return rewriteValueARM_OpARMEqual(_addr_v);
    else if (v.Op == OpARMGreaterEqual) 
        return rewriteValueARM_OpARMGreaterEqual(_addr_v);
    else if (v.Op == OpARMGreaterEqualU) 
        return rewriteValueARM_OpARMGreaterEqualU(_addr_v);
    else if (v.Op == OpARMGreaterThan) 
        return rewriteValueARM_OpARMGreaterThan(_addr_v);
    else if (v.Op == OpARMGreaterThanU) 
        return rewriteValueARM_OpARMGreaterThanU(_addr_v);
    else if (v.Op == OpARMLessEqual) 
        return rewriteValueARM_OpARMLessEqual(_addr_v);
    else if (v.Op == OpARMLessEqualU) 
        return rewriteValueARM_OpARMLessEqualU(_addr_v);
    else if (v.Op == OpARMLessThan) 
        return rewriteValueARM_OpARMLessThan(_addr_v);
    else if (v.Op == OpARMLessThanU) 
        return rewriteValueARM_OpARMLessThanU(_addr_v);
    else if (v.Op == OpARMMOVBUload) 
        return rewriteValueARM_OpARMMOVBUload(_addr_v);
    else if (v.Op == OpARMMOVBUloadidx) 
        return rewriteValueARM_OpARMMOVBUloadidx(_addr_v);
    else if (v.Op == OpARMMOVBUreg) 
        return rewriteValueARM_OpARMMOVBUreg(_addr_v);
    else if (v.Op == OpARMMOVBload) 
        return rewriteValueARM_OpARMMOVBload(_addr_v);
    else if (v.Op == OpARMMOVBloadidx) 
        return rewriteValueARM_OpARMMOVBloadidx(_addr_v);
    else if (v.Op == OpARMMOVBreg) 
        return rewriteValueARM_OpARMMOVBreg(_addr_v);
    else if (v.Op == OpARMMOVBstore) 
        return rewriteValueARM_OpARMMOVBstore(_addr_v);
    else if (v.Op == OpARMMOVBstoreidx) 
        return rewriteValueARM_OpARMMOVBstoreidx(_addr_v);
    else if (v.Op == OpARMMOVDload) 
        return rewriteValueARM_OpARMMOVDload(_addr_v);
    else if (v.Op == OpARMMOVDstore) 
        return rewriteValueARM_OpARMMOVDstore(_addr_v);
    else if (v.Op == OpARMMOVFload) 
        return rewriteValueARM_OpARMMOVFload(_addr_v);
    else if (v.Op == OpARMMOVFstore) 
        return rewriteValueARM_OpARMMOVFstore(_addr_v);
    else if (v.Op == OpARMMOVHUload) 
        return rewriteValueARM_OpARMMOVHUload(_addr_v);
    else if (v.Op == OpARMMOVHUloadidx) 
        return rewriteValueARM_OpARMMOVHUloadidx(_addr_v);
    else if (v.Op == OpARMMOVHUreg) 
        return rewriteValueARM_OpARMMOVHUreg(_addr_v);
    else if (v.Op == OpARMMOVHload) 
        return rewriteValueARM_OpARMMOVHload(_addr_v);
    else if (v.Op == OpARMMOVHloadidx) 
        return rewriteValueARM_OpARMMOVHloadidx(_addr_v);
    else if (v.Op == OpARMMOVHreg) 
        return rewriteValueARM_OpARMMOVHreg(_addr_v);
    else if (v.Op == OpARMMOVHstore) 
        return rewriteValueARM_OpARMMOVHstore(_addr_v);
    else if (v.Op == OpARMMOVHstoreidx) 
        return rewriteValueARM_OpARMMOVHstoreidx(_addr_v);
    else if (v.Op == OpARMMOVWload) 
        return rewriteValueARM_OpARMMOVWload(_addr_v);
    else if (v.Op == OpARMMOVWloadidx) 
        return rewriteValueARM_OpARMMOVWloadidx(_addr_v);
    else if (v.Op == OpARMMOVWloadshiftLL) 
        return rewriteValueARM_OpARMMOVWloadshiftLL(_addr_v);
    else if (v.Op == OpARMMOVWloadshiftRA) 
        return rewriteValueARM_OpARMMOVWloadshiftRA(_addr_v);
    else if (v.Op == OpARMMOVWloadshiftRL) 
        return rewriteValueARM_OpARMMOVWloadshiftRL(_addr_v);
    else if (v.Op == OpARMMOVWnop) 
        return rewriteValueARM_OpARMMOVWnop(_addr_v);
    else if (v.Op == OpARMMOVWreg) 
        return rewriteValueARM_OpARMMOVWreg(_addr_v);
    else if (v.Op == OpARMMOVWstore) 
        return rewriteValueARM_OpARMMOVWstore(_addr_v);
    else if (v.Op == OpARMMOVWstoreidx) 
        return rewriteValueARM_OpARMMOVWstoreidx(_addr_v);
    else if (v.Op == OpARMMOVWstoreshiftLL) 
        return rewriteValueARM_OpARMMOVWstoreshiftLL(_addr_v);
    else if (v.Op == OpARMMOVWstoreshiftRA) 
        return rewriteValueARM_OpARMMOVWstoreshiftRA(_addr_v);
    else if (v.Op == OpARMMOVWstoreshiftRL) 
        return rewriteValueARM_OpARMMOVWstoreshiftRL(_addr_v);
    else if (v.Op == OpARMMUL) 
        return rewriteValueARM_OpARMMUL(_addr_v);
    else if (v.Op == OpARMMULA) 
        return rewriteValueARM_OpARMMULA(_addr_v);
    else if (v.Op == OpARMMULD) 
        return rewriteValueARM_OpARMMULD(_addr_v);
    else if (v.Op == OpARMMULF) 
        return rewriteValueARM_OpARMMULF(_addr_v);
    else if (v.Op == OpARMMULS) 
        return rewriteValueARM_OpARMMULS(_addr_v);
    else if (v.Op == OpARMMVN) 
        return rewriteValueARM_OpARMMVN(_addr_v);
    else if (v.Op == OpARMMVNshiftLL) 
        return rewriteValueARM_OpARMMVNshiftLL(_addr_v);
    else if (v.Op == OpARMMVNshiftLLreg) 
        return rewriteValueARM_OpARMMVNshiftLLreg(_addr_v);
    else if (v.Op == OpARMMVNshiftRA) 
        return rewriteValueARM_OpARMMVNshiftRA(_addr_v);
    else if (v.Op == OpARMMVNshiftRAreg) 
        return rewriteValueARM_OpARMMVNshiftRAreg(_addr_v);
    else if (v.Op == OpARMMVNshiftRL) 
        return rewriteValueARM_OpARMMVNshiftRL(_addr_v);
    else if (v.Op == OpARMMVNshiftRLreg) 
        return rewriteValueARM_OpARMMVNshiftRLreg(_addr_v);
    else if (v.Op == OpARMNEGD) 
        return rewriteValueARM_OpARMNEGD(_addr_v);
    else if (v.Op == OpARMNEGF) 
        return rewriteValueARM_OpARMNEGF(_addr_v);
    else if (v.Op == OpARMNMULD) 
        return rewriteValueARM_OpARMNMULD(_addr_v);
    else if (v.Op == OpARMNMULF) 
        return rewriteValueARM_OpARMNMULF(_addr_v);
    else if (v.Op == OpARMNotEqual) 
        return rewriteValueARM_OpARMNotEqual(_addr_v);
    else if (v.Op == OpARMOR) 
        return rewriteValueARM_OpARMOR(_addr_v);
    else if (v.Op == OpARMORconst) 
        return rewriteValueARM_OpARMORconst(_addr_v);
    else if (v.Op == OpARMORshiftLL) 
        return rewriteValueARM_OpARMORshiftLL(_addr_v);
    else if (v.Op == OpARMORshiftLLreg) 
        return rewriteValueARM_OpARMORshiftLLreg(_addr_v);
    else if (v.Op == OpARMORshiftRA) 
        return rewriteValueARM_OpARMORshiftRA(_addr_v);
    else if (v.Op == OpARMORshiftRAreg) 
        return rewriteValueARM_OpARMORshiftRAreg(_addr_v);
    else if (v.Op == OpARMORshiftRL) 
        return rewriteValueARM_OpARMORshiftRL(_addr_v);
    else if (v.Op == OpARMORshiftRLreg) 
        return rewriteValueARM_OpARMORshiftRLreg(_addr_v);
    else if (v.Op == OpARMRSB) 
        return rewriteValueARM_OpARMRSB(_addr_v);
    else if (v.Op == OpARMRSBSshiftLL) 
        return rewriteValueARM_OpARMRSBSshiftLL(_addr_v);
    else if (v.Op == OpARMRSBSshiftLLreg) 
        return rewriteValueARM_OpARMRSBSshiftLLreg(_addr_v);
    else if (v.Op == OpARMRSBSshiftRA) 
        return rewriteValueARM_OpARMRSBSshiftRA(_addr_v);
    else if (v.Op == OpARMRSBSshiftRAreg) 
        return rewriteValueARM_OpARMRSBSshiftRAreg(_addr_v);
    else if (v.Op == OpARMRSBSshiftRL) 
        return rewriteValueARM_OpARMRSBSshiftRL(_addr_v);
    else if (v.Op == OpARMRSBSshiftRLreg) 
        return rewriteValueARM_OpARMRSBSshiftRLreg(_addr_v);
    else if (v.Op == OpARMRSBconst) 
        return rewriteValueARM_OpARMRSBconst(_addr_v);
    else if (v.Op == OpARMRSBshiftLL) 
        return rewriteValueARM_OpARMRSBshiftLL(_addr_v);
    else if (v.Op == OpARMRSBshiftLLreg) 
        return rewriteValueARM_OpARMRSBshiftLLreg(_addr_v);
    else if (v.Op == OpARMRSBshiftRA) 
        return rewriteValueARM_OpARMRSBshiftRA(_addr_v);
    else if (v.Op == OpARMRSBshiftRAreg) 
        return rewriteValueARM_OpARMRSBshiftRAreg(_addr_v);
    else if (v.Op == OpARMRSBshiftRL) 
        return rewriteValueARM_OpARMRSBshiftRL(_addr_v);
    else if (v.Op == OpARMRSBshiftRLreg) 
        return rewriteValueARM_OpARMRSBshiftRLreg(_addr_v);
    else if (v.Op == OpARMRSCconst) 
        return rewriteValueARM_OpARMRSCconst(_addr_v);
    else if (v.Op == OpARMRSCshiftLL) 
        return rewriteValueARM_OpARMRSCshiftLL(_addr_v);
    else if (v.Op == OpARMRSCshiftLLreg) 
        return rewriteValueARM_OpARMRSCshiftLLreg(_addr_v);
    else if (v.Op == OpARMRSCshiftRA) 
        return rewriteValueARM_OpARMRSCshiftRA(_addr_v);
    else if (v.Op == OpARMRSCshiftRAreg) 
        return rewriteValueARM_OpARMRSCshiftRAreg(_addr_v);
    else if (v.Op == OpARMRSCshiftRL) 
        return rewriteValueARM_OpARMRSCshiftRL(_addr_v);
    else if (v.Op == OpARMRSCshiftRLreg) 
        return rewriteValueARM_OpARMRSCshiftRLreg(_addr_v);
    else if (v.Op == OpARMSBC) 
        return rewriteValueARM_OpARMSBC(_addr_v);
    else if (v.Op == OpARMSBCconst) 
        return rewriteValueARM_OpARMSBCconst(_addr_v);
    else if (v.Op == OpARMSBCshiftLL) 
        return rewriteValueARM_OpARMSBCshiftLL(_addr_v);
    else if (v.Op == OpARMSBCshiftLLreg) 
        return rewriteValueARM_OpARMSBCshiftLLreg(_addr_v);
    else if (v.Op == OpARMSBCshiftRA) 
        return rewriteValueARM_OpARMSBCshiftRA(_addr_v);
    else if (v.Op == OpARMSBCshiftRAreg) 
        return rewriteValueARM_OpARMSBCshiftRAreg(_addr_v);
    else if (v.Op == OpARMSBCshiftRL) 
        return rewriteValueARM_OpARMSBCshiftRL(_addr_v);
    else if (v.Op == OpARMSBCshiftRLreg) 
        return rewriteValueARM_OpARMSBCshiftRLreg(_addr_v);
    else if (v.Op == OpARMSLL) 
        return rewriteValueARM_OpARMSLL(_addr_v);
    else if (v.Op == OpARMSLLconst) 
        return rewriteValueARM_OpARMSLLconst(_addr_v);
    else if (v.Op == OpARMSRA) 
        return rewriteValueARM_OpARMSRA(_addr_v);
    else if (v.Op == OpARMSRAcond) 
        return rewriteValueARM_OpARMSRAcond(_addr_v);
    else if (v.Op == OpARMSRAconst) 
        return rewriteValueARM_OpARMSRAconst(_addr_v);
    else if (v.Op == OpARMSRL) 
        return rewriteValueARM_OpARMSRL(_addr_v);
    else if (v.Op == OpARMSRLconst) 
        return rewriteValueARM_OpARMSRLconst(_addr_v);
    else if (v.Op == OpARMSUB) 
        return rewriteValueARM_OpARMSUB(_addr_v);
    else if (v.Op == OpARMSUBD) 
        return rewriteValueARM_OpARMSUBD(_addr_v);
    else if (v.Op == OpARMSUBF) 
        return rewriteValueARM_OpARMSUBF(_addr_v);
    else if (v.Op == OpARMSUBS) 
        return rewriteValueARM_OpARMSUBS(_addr_v);
    else if (v.Op == OpARMSUBSshiftLL) 
        return rewriteValueARM_OpARMSUBSshiftLL(_addr_v);
    else if (v.Op == OpARMSUBSshiftLLreg) 
        return rewriteValueARM_OpARMSUBSshiftLLreg(_addr_v);
    else if (v.Op == OpARMSUBSshiftRA) 
        return rewriteValueARM_OpARMSUBSshiftRA(_addr_v);
    else if (v.Op == OpARMSUBSshiftRAreg) 
        return rewriteValueARM_OpARMSUBSshiftRAreg(_addr_v);
    else if (v.Op == OpARMSUBSshiftRL) 
        return rewriteValueARM_OpARMSUBSshiftRL(_addr_v);
    else if (v.Op == OpARMSUBSshiftRLreg) 
        return rewriteValueARM_OpARMSUBSshiftRLreg(_addr_v);
    else if (v.Op == OpARMSUBconst) 
        return rewriteValueARM_OpARMSUBconst(_addr_v);
    else if (v.Op == OpARMSUBshiftLL) 
        return rewriteValueARM_OpARMSUBshiftLL(_addr_v);
    else if (v.Op == OpARMSUBshiftLLreg) 
        return rewriteValueARM_OpARMSUBshiftLLreg(_addr_v);
    else if (v.Op == OpARMSUBshiftRA) 
        return rewriteValueARM_OpARMSUBshiftRA(_addr_v);
    else if (v.Op == OpARMSUBshiftRAreg) 
        return rewriteValueARM_OpARMSUBshiftRAreg(_addr_v);
    else if (v.Op == OpARMSUBshiftRL) 
        return rewriteValueARM_OpARMSUBshiftRL(_addr_v);
    else if (v.Op == OpARMSUBshiftRLreg) 
        return rewriteValueARM_OpARMSUBshiftRLreg(_addr_v);
    else if (v.Op == OpARMTEQ) 
        return rewriteValueARM_OpARMTEQ(_addr_v);
    else if (v.Op == OpARMTEQconst) 
        return rewriteValueARM_OpARMTEQconst(_addr_v);
    else if (v.Op == OpARMTEQshiftLL) 
        return rewriteValueARM_OpARMTEQshiftLL(_addr_v);
    else if (v.Op == OpARMTEQshiftLLreg) 
        return rewriteValueARM_OpARMTEQshiftLLreg(_addr_v);
    else if (v.Op == OpARMTEQshiftRA) 
        return rewriteValueARM_OpARMTEQshiftRA(_addr_v);
    else if (v.Op == OpARMTEQshiftRAreg) 
        return rewriteValueARM_OpARMTEQshiftRAreg(_addr_v);
    else if (v.Op == OpARMTEQshiftRL) 
        return rewriteValueARM_OpARMTEQshiftRL(_addr_v);
    else if (v.Op == OpARMTEQshiftRLreg) 
        return rewriteValueARM_OpARMTEQshiftRLreg(_addr_v);
    else if (v.Op == OpARMTST) 
        return rewriteValueARM_OpARMTST(_addr_v);
    else if (v.Op == OpARMTSTconst) 
        return rewriteValueARM_OpARMTSTconst(_addr_v);
    else if (v.Op == OpARMTSTshiftLL) 
        return rewriteValueARM_OpARMTSTshiftLL(_addr_v);
    else if (v.Op == OpARMTSTshiftLLreg) 
        return rewriteValueARM_OpARMTSTshiftLLreg(_addr_v);
    else if (v.Op == OpARMTSTshiftRA) 
        return rewriteValueARM_OpARMTSTshiftRA(_addr_v);
    else if (v.Op == OpARMTSTshiftRAreg) 
        return rewriteValueARM_OpARMTSTshiftRAreg(_addr_v);
    else if (v.Op == OpARMTSTshiftRL) 
        return rewriteValueARM_OpARMTSTshiftRL(_addr_v);
    else if (v.Op == OpARMTSTshiftRLreg) 
        return rewriteValueARM_OpARMTSTshiftRLreg(_addr_v);
    else if (v.Op == OpARMXOR) 
        return rewriteValueARM_OpARMXOR(_addr_v);
    else if (v.Op == OpARMXORconst) 
        return rewriteValueARM_OpARMXORconst(_addr_v);
    else if (v.Op == OpARMXORshiftLL) 
        return rewriteValueARM_OpARMXORshiftLL(_addr_v);
    else if (v.Op == OpARMXORshiftLLreg) 
        return rewriteValueARM_OpARMXORshiftLLreg(_addr_v);
    else if (v.Op == OpARMXORshiftRA) 
        return rewriteValueARM_OpARMXORshiftRA(_addr_v);
    else if (v.Op == OpARMXORshiftRAreg) 
        return rewriteValueARM_OpARMXORshiftRAreg(_addr_v);
    else if (v.Op == OpARMXORshiftRL) 
        return rewriteValueARM_OpARMXORshiftRL(_addr_v);
    else if (v.Op == OpARMXORshiftRLreg) 
        return rewriteValueARM_OpARMXORshiftRLreg(_addr_v);
    else if (v.Op == OpARMXORshiftRR) 
        return rewriteValueARM_OpARMXORshiftRR(_addr_v);
    else if (v.Op == OpAbs) 
        v.Op = OpARMABSD;
        return true;
    else if (v.Op == OpAdd16) 
        v.Op = OpARMADD;
        return true;
    else if (v.Op == OpAdd32) 
        v.Op = OpARMADD;
        return true;
    else if (v.Op == OpAdd32F) 
        v.Op = OpARMADDF;
        return true;
    else if (v.Op == OpAdd32carry) 
        v.Op = OpARMADDS;
        return true;
    else if (v.Op == OpAdd32withcarry) 
        v.Op = OpARMADC;
        return true;
    else if (v.Op == OpAdd64F) 
        v.Op = OpARMADDD;
        return true;
    else if (v.Op == OpAdd8) 
        v.Op = OpARMADD;
        return true;
    else if (v.Op == OpAddPtr) 
        v.Op = OpARMADD;
        return true;
    else if (v.Op == OpAddr) 
        return rewriteValueARM_OpAddr(_addr_v);
    else if (v.Op == OpAnd16) 
        v.Op = OpARMAND;
        return true;
    else if (v.Op == OpAnd32) 
        v.Op = OpARMAND;
        return true;
    else if (v.Op == OpAnd8) 
        v.Op = OpARMAND;
        return true;
    else if (v.Op == OpAndB) 
        v.Op = OpARMAND;
        return true;
    else if (v.Op == OpAvg32u) 
        return rewriteValueARM_OpAvg32u(_addr_v);
    else if (v.Op == OpBitLen32) 
        return rewriteValueARM_OpBitLen32(_addr_v);
    else if (v.Op == OpBswap32) 
        return rewriteValueARM_OpBswap32(_addr_v);
    else if (v.Op == OpClosureCall) 
        v.Op = OpARMCALLclosure;
        return true;
    else if (v.Op == OpCom16) 
        v.Op = OpARMMVN;
        return true;
    else if (v.Op == OpCom32) 
        v.Op = OpARMMVN;
        return true;
    else if (v.Op == OpCom8) 
        v.Op = OpARMMVN;
        return true;
    else if (v.Op == OpConst16) 
        return rewriteValueARM_OpConst16(_addr_v);
    else if (v.Op == OpConst32) 
        return rewriteValueARM_OpConst32(_addr_v);
    else if (v.Op == OpConst32F) 
        return rewriteValueARM_OpConst32F(_addr_v);
    else if (v.Op == OpConst64F) 
        return rewriteValueARM_OpConst64F(_addr_v);
    else if (v.Op == OpConst8) 
        return rewriteValueARM_OpConst8(_addr_v);
    else if (v.Op == OpConstBool) 
        return rewriteValueARM_OpConstBool(_addr_v);
    else if (v.Op == OpConstNil) 
        return rewriteValueARM_OpConstNil(_addr_v);
    else if (v.Op == OpCtz16) 
        return rewriteValueARM_OpCtz16(_addr_v);
    else if (v.Op == OpCtz16NonZero) 
        v.Op = OpCtz32;
        return true;
    else if (v.Op == OpCtz32) 
        return rewriteValueARM_OpCtz32(_addr_v);
    else if (v.Op == OpCtz32NonZero) 
        v.Op = OpCtz32;
        return true;
    else if (v.Op == OpCtz8) 
        return rewriteValueARM_OpCtz8(_addr_v);
    else if (v.Op == OpCtz8NonZero) 
        v.Op = OpCtz32;
        return true;
    else if (v.Op == OpCvt32Fto32) 
        v.Op = OpARMMOVFW;
        return true;
    else if (v.Op == OpCvt32Fto32U) 
        v.Op = OpARMMOVFWU;
        return true;
    else if (v.Op == OpCvt32Fto64F) 
        v.Op = OpARMMOVFD;
        return true;
    else if (v.Op == OpCvt32Uto32F) 
        v.Op = OpARMMOVWUF;
        return true;
    else if (v.Op == OpCvt32Uto64F) 
        v.Op = OpARMMOVWUD;
        return true;
    else if (v.Op == OpCvt32to32F) 
        v.Op = OpARMMOVWF;
        return true;
    else if (v.Op == OpCvt32to64F) 
        v.Op = OpARMMOVWD;
        return true;
    else if (v.Op == OpCvt64Fto32) 
        v.Op = OpARMMOVDW;
        return true;
    else if (v.Op == OpCvt64Fto32F) 
        v.Op = OpARMMOVDF;
        return true;
    else if (v.Op == OpCvt64Fto32U) 
        v.Op = OpARMMOVDWU;
        return true;
    else if (v.Op == OpCvtBoolToUint8) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpDiv16) 
        return rewriteValueARM_OpDiv16(_addr_v);
    else if (v.Op == OpDiv16u) 
        return rewriteValueARM_OpDiv16u(_addr_v);
    else if (v.Op == OpDiv32) 
        return rewriteValueARM_OpDiv32(_addr_v);
    else if (v.Op == OpDiv32F) 
        v.Op = OpARMDIVF;
        return true;
    else if (v.Op == OpDiv32u) 
        return rewriteValueARM_OpDiv32u(_addr_v);
    else if (v.Op == OpDiv64F) 
        v.Op = OpARMDIVD;
        return true;
    else if (v.Op == OpDiv8) 
        return rewriteValueARM_OpDiv8(_addr_v);
    else if (v.Op == OpDiv8u) 
        return rewriteValueARM_OpDiv8u(_addr_v);
    else if (v.Op == OpEq16) 
        return rewriteValueARM_OpEq16(_addr_v);
    else if (v.Op == OpEq32) 
        return rewriteValueARM_OpEq32(_addr_v);
    else if (v.Op == OpEq32F) 
        return rewriteValueARM_OpEq32F(_addr_v);
    else if (v.Op == OpEq64F) 
        return rewriteValueARM_OpEq64F(_addr_v);
    else if (v.Op == OpEq8) 
        return rewriteValueARM_OpEq8(_addr_v);
    else if (v.Op == OpEqB) 
        return rewriteValueARM_OpEqB(_addr_v);
    else if (v.Op == OpEqPtr) 
        return rewriteValueARM_OpEqPtr(_addr_v);
    else if (v.Op == OpFMA) 
        return rewriteValueARM_OpFMA(_addr_v);
    else if (v.Op == OpGetCallerPC) 
        v.Op = OpARMLoweredGetCallerPC;
        return true;
    else if (v.Op == OpGetCallerSP) 
        v.Op = OpARMLoweredGetCallerSP;
        return true;
    else if (v.Op == OpGetClosurePtr) 
        v.Op = OpARMLoweredGetClosurePtr;
        return true;
    else if (v.Op == OpHmul32) 
        v.Op = OpARMHMUL;
        return true;
    else if (v.Op == OpHmul32u) 
        v.Op = OpARMHMULU;
        return true;
    else if (v.Op == OpInterCall) 
        v.Op = OpARMCALLinter;
        return true;
    else if (v.Op == OpIsInBounds) 
        return rewriteValueARM_OpIsInBounds(_addr_v);
    else if (v.Op == OpIsNonNil) 
        return rewriteValueARM_OpIsNonNil(_addr_v);
    else if (v.Op == OpIsSliceInBounds) 
        return rewriteValueARM_OpIsSliceInBounds(_addr_v);
    else if (v.Op == OpLeq16) 
        return rewriteValueARM_OpLeq16(_addr_v);
    else if (v.Op == OpLeq16U) 
        return rewriteValueARM_OpLeq16U(_addr_v);
    else if (v.Op == OpLeq32) 
        return rewriteValueARM_OpLeq32(_addr_v);
    else if (v.Op == OpLeq32F) 
        return rewriteValueARM_OpLeq32F(_addr_v);
    else if (v.Op == OpLeq32U) 
        return rewriteValueARM_OpLeq32U(_addr_v);
    else if (v.Op == OpLeq64F) 
        return rewriteValueARM_OpLeq64F(_addr_v);
    else if (v.Op == OpLeq8) 
        return rewriteValueARM_OpLeq8(_addr_v);
    else if (v.Op == OpLeq8U) 
        return rewriteValueARM_OpLeq8U(_addr_v);
    else if (v.Op == OpLess16) 
        return rewriteValueARM_OpLess16(_addr_v);
    else if (v.Op == OpLess16U) 
        return rewriteValueARM_OpLess16U(_addr_v);
    else if (v.Op == OpLess32) 
        return rewriteValueARM_OpLess32(_addr_v);
    else if (v.Op == OpLess32F) 
        return rewriteValueARM_OpLess32F(_addr_v);
    else if (v.Op == OpLess32U) 
        return rewriteValueARM_OpLess32U(_addr_v);
    else if (v.Op == OpLess64F) 
        return rewriteValueARM_OpLess64F(_addr_v);
    else if (v.Op == OpLess8) 
        return rewriteValueARM_OpLess8(_addr_v);
    else if (v.Op == OpLess8U) 
        return rewriteValueARM_OpLess8U(_addr_v);
    else if (v.Op == OpLoad) 
        return rewriteValueARM_OpLoad(_addr_v);
    else if (v.Op == OpLocalAddr) 
        return rewriteValueARM_OpLocalAddr(_addr_v);
    else if (v.Op == OpLsh16x16) 
        return rewriteValueARM_OpLsh16x16(_addr_v);
    else if (v.Op == OpLsh16x32) 
        return rewriteValueARM_OpLsh16x32(_addr_v);
    else if (v.Op == OpLsh16x64) 
        return rewriteValueARM_OpLsh16x64(_addr_v);
    else if (v.Op == OpLsh16x8) 
        return rewriteValueARM_OpLsh16x8(_addr_v);
    else if (v.Op == OpLsh32x16) 
        return rewriteValueARM_OpLsh32x16(_addr_v);
    else if (v.Op == OpLsh32x32) 
        return rewriteValueARM_OpLsh32x32(_addr_v);
    else if (v.Op == OpLsh32x64) 
        return rewriteValueARM_OpLsh32x64(_addr_v);
    else if (v.Op == OpLsh32x8) 
        return rewriteValueARM_OpLsh32x8(_addr_v);
    else if (v.Op == OpLsh8x16) 
        return rewriteValueARM_OpLsh8x16(_addr_v);
    else if (v.Op == OpLsh8x32) 
        return rewriteValueARM_OpLsh8x32(_addr_v);
    else if (v.Op == OpLsh8x64) 
        return rewriteValueARM_OpLsh8x64(_addr_v);
    else if (v.Op == OpLsh8x8) 
        return rewriteValueARM_OpLsh8x8(_addr_v);
    else if (v.Op == OpMod16) 
        return rewriteValueARM_OpMod16(_addr_v);
    else if (v.Op == OpMod16u) 
        return rewriteValueARM_OpMod16u(_addr_v);
    else if (v.Op == OpMod32) 
        return rewriteValueARM_OpMod32(_addr_v);
    else if (v.Op == OpMod32u) 
        return rewriteValueARM_OpMod32u(_addr_v);
    else if (v.Op == OpMod8) 
        return rewriteValueARM_OpMod8(_addr_v);
    else if (v.Op == OpMod8u) 
        return rewriteValueARM_OpMod8u(_addr_v);
    else if (v.Op == OpMove) 
        return rewriteValueARM_OpMove(_addr_v);
    else if (v.Op == OpMul16) 
        v.Op = OpARMMUL;
        return true;
    else if (v.Op == OpMul32) 
        v.Op = OpARMMUL;
        return true;
    else if (v.Op == OpMul32F) 
        v.Op = OpARMMULF;
        return true;
    else if (v.Op == OpMul32uhilo) 
        v.Op = OpARMMULLU;
        return true;
    else if (v.Op == OpMul64F) 
        v.Op = OpARMMULD;
        return true;
    else if (v.Op == OpMul8) 
        v.Op = OpARMMUL;
        return true;
    else if (v.Op == OpNeg16) 
        return rewriteValueARM_OpNeg16(_addr_v);
    else if (v.Op == OpNeg32) 
        return rewriteValueARM_OpNeg32(_addr_v);
    else if (v.Op == OpNeg32F) 
        v.Op = OpARMNEGF;
        return true;
    else if (v.Op == OpNeg64F) 
        v.Op = OpARMNEGD;
        return true;
    else if (v.Op == OpNeg8) 
        return rewriteValueARM_OpNeg8(_addr_v);
    else if (v.Op == OpNeq16) 
        return rewriteValueARM_OpNeq16(_addr_v);
    else if (v.Op == OpNeq32) 
        return rewriteValueARM_OpNeq32(_addr_v);
    else if (v.Op == OpNeq32F) 
        return rewriteValueARM_OpNeq32F(_addr_v);
    else if (v.Op == OpNeq64F) 
        return rewriteValueARM_OpNeq64F(_addr_v);
    else if (v.Op == OpNeq8) 
        return rewriteValueARM_OpNeq8(_addr_v);
    else if (v.Op == OpNeqB) 
        v.Op = OpARMXOR;
        return true;
    else if (v.Op == OpNeqPtr) 
        return rewriteValueARM_OpNeqPtr(_addr_v);
    else if (v.Op == OpNilCheck) 
        v.Op = OpARMLoweredNilCheck;
        return true;
    else if (v.Op == OpNot) 
        return rewriteValueARM_OpNot(_addr_v);
    else if (v.Op == OpOffPtr) 
        return rewriteValueARM_OpOffPtr(_addr_v);
    else if (v.Op == OpOr16) 
        v.Op = OpARMOR;
        return true;
    else if (v.Op == OpOr32) 
        v.Op = OpARMOR;
        return true;
    else if (v.Op == OpOr8) 
        v.Op = OpARMOR;
        return true;
    else if (v.Op == OpOrB) 
        v.Op = OpARMOR;
        return true;
    else if (v.Op == OpPanicBounds) 
        return rewriteValueARM_OpPanicBounds(_addr_v);
    else if (v.Op == OpPanicExtend) 
        return rewriteValueARM_OpPanicExtend(_addr_v);
    else if (v.Op == OpRotateLeft16) 
        return rewriteValueARM_OpRotateLeft16(_addr_v);
    else if (v.Op == OpRotateLeft32) 
        return rewriteValueARM_OpRotateLeft32(_addr_v);
    else if (v.Op == OpRotateLeft8) 
        return rewriteValueARM_OpRotateLeft8(_addr_v);
    else if (v.Op == OpRound32F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRound64F) 
        v.Op = OpCopy;
        return true;
    else if (v.Op == OpRsh16Ux16) 
        return rewriteValueARM_OpRsh16Ux16(_addr_v);
    else if (v.Op == OpRsh16Ux32) 
        return rewriteValueARM_OpRsh16Ux32(_addr_v);
    else if (v.Op == OpRsh16Ux64) 
        return rewriteValueARM_OpRsh16Ux64(_addr_v);
    else if (v.Op == OpRsh16Ux8) 
        return rewriteValueARM_OpRsh16Ux8(_addr_v);
    else if (v.Op == OpRsh16x16) 
        return rewriteValueARM_OpRsh16x16(_addr_v);
    else if (v.Op == OpRsh16x32) 
        return rewriteValueARM_OpRsh16x32(_addr_v);
    else if (v.Op == OpRsh16x64) 
        return rewriteValueARM_OpRsh16x64(_addr_v);
    else if (v.Op == OpRsh16x8) 
        return rewriteValueARM_OpRsh16x8(_addr_v);
    else if (v.Op == OpRsh32Ux16) 
        return rewriteValueARM_OpRsh32Ux16(_addr_v);
    else if (v.Op == OpRsh32Ux32) 
        return rewriteValueARM_OpRsh32Ux32(_addr_v);
    else if (v.Op == OpRsh32Ux64) 
        return rewriteValueARM_OpRsh32Ux64(_addr_v);
    else if (v.Op == OpRsh32Ux8) 
        return rewriteValueARM_OpRsh32Ux8(_addr_v);
    else if (v.Op == OpRsh32x16) 
        return rewriteValueARM_OpRsh32x16(_addr_v);
    else if (v.Op == OpRsh32x32) 
        return rewriteValueARM_OpRsh32x32(_addr_v);
    else if (v.Op == OpRsh32x64) 
        return rewriteValueARM_OpRsh32x64(_addr_v);
    else if (v.Op == OpRsh32x8) 
        return rewriteValueARM_OpRsh32x8(_addr_v);
    else if (v.Op == OpRsh8Ux16) 
        return rewriteValueARM_OpRsh8Ux16(_addr_v);
    else if (v.Op == OpRsh8Ux32) 
        return rewriteValueARM_OpRsh8Ux32(_addr_v);
    else if (v.Op == OpRsh8Ux64) 
        return rewriteValueARM_OpRsh8Ux64(_addr_v);
    else if (v.Op == OpRsh8Ux8) 
        return rewriteValueARM_OpRsh8Ux8(_addr_v);
    else if (v.Op == OpRsh8x16) 
        return rewriteValueARM_OpRsh8x16(_addr_v);
    else if (v.Op == OpRsh8x32) 
        return rewriteValueARM_OpRsh8x32(_addr_v);
    else if (v.Op == OpRsh8x64) 
        return rewriteValueARM_OpRsh8x64(_addr_v);
    else if (v.Op == OpRsh8x8) 
        return rewriteValueARM_OpRsh8x8(_addr_v);
    else if (v.Op == OpSelect0) 
        return rewriteValueARM_OpSelect0(_addr_v);
    else if (v.Op == OpSelect1) 
        return rewriteValueARM_OpSelect1(_addr_v);
    else if (v.Op == OpSignExt16to32) 
        v.Op = OpARMMOVHreg;
        return true;
    else if (v.Op == OpSignExt8to16) 
        v.Op = OpARMMOVBreg;
        return true;
    else if (v.Op == OpSignExt8to32) 
        v.Op = OpARMMOVBreg;
        return true;
    else if (v.Op == OpSignmask) 
        return rewriteValueARM_OpSignmask(_addr_v);
    else if (v.Op == OpSlicemask) 
        return rewriteValueARM_OpSlicemask(_addr_v);
    else if (v.Op == OpSqrt) 
        v.Op = OpARMSQRTD;
        return true;
    else if (v.Op == OpSqrt32) 
        v.Op = OpARMSQRTF;
        return true;
    else if (v.Op == OpStaticCall) 
        v.Op = OpARMCALLstatic;
        return true;
    else if (v.Op == OpStore) 
        return rewriteValueARM_OpStore(_addr_v);
    else if (v.Op == OpSub16) 
        v.Op = OpARMSUB;
        return true;
    else if (v.Op == OpSub32) 
        v.Op = OpARMSUB;
        return true;
    else if (v.Op == OpSub32F) 
        v.Op = OpARMSUBF;
        return true;
    else if (v.Op == OpSub32carry) 
        v.Op = OpARMSUBS;
        return true;
    else if (v.Op == OpSub32withcarry) 
        v.Op = OpARMSBC;
        return true;
    else if (v.Op == OpSub64F) 
        v.Op = OpARMSUBD;
        return true;
    else if (v.Op == OpSub8) 
        v.Op = OpARMSUB;
        return true;
    else if (v.Op == OpSubPtr) 
        v.Op = OpARMSUB;
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
        v.Op = OpARMLoweredWB;
        return true;
    else if (v.Op == OpXor16) 
        v.Op = OpARMXOR;
        return true;
    else if (v.Op == OpXor32) 
        v.Op = OpARMXOR;
        return true;
    else if (v.Op == OpXor8) 
        v.Op = OpARMXOR;
        return true;
    else if (v.Op == OpZero) 
        return rewriteValueARM_OpZero(_addr_v);
    else if (v.Op == OpZeroExt16to32) 
        v.Op = OpARMMOVHUreg;
        return true;
    else if (v.Op == OpZeroExt8to16) 
        v.Op = OpARMMOVBUreg;
        return true;
    else if (v.Op == OpZeroExt8to32) 
        v.Op = OpARMMOVBUreg;
        return true;
    else if (v.Op == OpZeromask) 
        return rewriteValueARM_OpZeromask(_addr_v);
        return false;

}
private static bool rewriteValueARM_OpARMADC(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADC (MOVWconst [c]) x flags)
    // result: (ADCconst [c] x flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_0.AuxInt);
                var x = v_1;
                var flags = v_2;
                v.reset(OpARMADCconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADC x (SLLconst [c] y) flags)
    // result: (ADCshiftLL x y [c] flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                flags = v_2;
                v.reset(OpARMADCshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg3(x, y, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADC x (SRLconst [c] y) flags)
    // result: (ADCshiftRL x y [c] flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                flags = v_2;
                v.reset(OpARMADCshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg3(x, y, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADC x (SRAconst [c] y) flags)
    // result: (ADCshiftRA x y [c] flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                flags = v_2;
                v.reset(OpARMADCshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg3(x, y, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADC x (SLL y z) flags)
    // result: (ADCshiftLLreg x y z flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                flags = v_2;
                v.reset(OpARMADCshiftLLreg);
                v.AddArg4(x, y, z, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADC x (SRL y z) flags)
    // result: (ADCshiftRLreg x y z flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                flags = v_2;
                v.reset(OpARMADCshiftRLreg);
                v.AddArg4(x, y, z, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADC x (SRA y z) flags)
    // result: (ADCshiftRAreg x y z flags)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                flags = v_2;
                v.reset(OpARMADCshiftRAreg);
                v.AddArg4(x, y, z, flags);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADCconst [c] (ADDconst [d] x) flags)
    // result: (ADCconst [c+d] x flags)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var flags = v_1;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg2(x, flags);
        return true;

    } 
    // match: (ADCconst [c] (SUBconst [d] x) flags)
    // result: (ADCconst [c-d] x flags)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        flags = v_1;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADCshiftLL (MOVWconst [c]) x [d] flags)
    // result: (ADCconst [c] (SLLconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (ADCshiftLL x (MOVWconst [c]) [d] flags)
    // result: (ADCconst x [c<<uint64(d)] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADCshiftLLreg (MOVWconst [c]) x y flags)
    // result: (ADCconst [c] (SLL <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (ADCshiftLLreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (ADCshiftLL x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADCshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADCshiftRA (MOVWconst [c]) x [d] flags)
    // result: (ADCconst [c] (SRAconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (ADCshiftRA x (MOVWconst [c]) [d] flags)
    // result: (ADCconst x [c>>uint64(d)] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADCshiftRAreg (MOVWconst [c]) x y flags)
    // result: (ADCconst [c] (SRA <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (ADCshiftRAreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (ADCshiftRA x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADCshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADCshiftRL (MOVWconst [c]) x [d] flags)
    // result: (ADCconst [c] (SRLconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (ADCshiftRL x (MOVWconst [c]) [d] flags)
    // result: (ADCconst x [int32(uint32(c)>>uint64(d))] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADCshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADCshiftRLreg (MOVWconst [c]) x y flags)
    // result: (ADCconst [c] (SRL <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMADCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (ADCshiftRLreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (ADCshiftRL x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADCshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADD x (MOVWconst [c]))
    // result: (ADDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMADDconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (SLLconst [c] y))
    // result: (ADDshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMADDshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (SRLconst [c] y))
    // result: (ADDshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMADDshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (SRAconst [c] y))
    // result: (ADDshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMADDshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (SLL y z))
    // result: (ADDshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMADDshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (SRL y z))
    // result: (ADDshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMADDshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (SRA y z))
    // result: (ADDshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMADDshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD x (RSBconst [0] y))
    // result: (SUB x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMRSBconst || auxIntToInt32(v_1.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                y = v_1.Args[0];
                v.reset(OpARMSUB);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD <t> (RSBconst [c] x) (RSBconst [d] y))
    // result: (RSBconst [c+d] (ADD <t> x y))
    while (true) {
        var t = v.Type;
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMRSBconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_0.AuxInt);
                x = v_0.Args[0];
                if (v_1.Op != OpARMRSBconst) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMRSBconst);
                v.AuxInt = int32ToAuxInt(c + d);
                var v0 = b.NewValue0(v.Pos, OpARMADD, t);
                v0.AddArg2(x, y);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADD (MUL x y) a)
    // result: (MULA x y a)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMMUL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                y = v_0.Args[1];
                x = v_0.Args[0];
                var a = v_1;
                v.reset(OpARMMULA);
                v.AddArg3(x, y, a);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDD a (MULD x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULAD a x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var a = v_0;
                if (v_1.Op != OpARMMULD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = v_1.Args[1];
                var x = v_1.Args[0];
                if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
                    continue;
                }

                v.reset(OpARMMULAD);
                v.AddArg3(a, x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDD a (NMULD x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULSD a x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                a = v_0;
                if (v_1.Op != OpARMNMULD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                y = v_1.Args[1];
                x = v_1.Args[0];
                if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
                    continue;
                }

                v.reset(OpARMMULSD);
                v.AddArg3(a, x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDF(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDF a (MULF x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULAF a x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var a = v_0;
                if (v_1.Op != OpARMMULF) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var y = v_1.Args[1];
                var x = v_1.Args[0];
                if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
                    continue;
                }

                v.reset(OpARMMULAF);
                v.AddArg3(a, x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDF a (NMULF x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULSF a x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                a = v_0;
                if (v_1.Op != OpARMNMULF) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                y = v_1.Args[1];
                x = v_1.Args[0];
                if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
                    continue;
                }

                v.reset(OpARMMULSF);
                v.AddArg3(a, x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (ADDS x (MOVWconst [c]))
    // result: (ADDSconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMADDSconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDS x (SLLconst [c] y))
    // result: (ADDSshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMADDSshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDS x (SRLconst [c] y))
    // result: (ADDSshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMADDSshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDS x (SRAconst [c] y))
    // result: (ADDSshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMADDSshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDS x (SLL y z))
    // result: (ADDSshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMADDSshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDS x (SRL y z))
    // result: (ADDSshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMADDSshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (ADDS x (SRA y z))
    // result: (ADDSshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMADDSshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDSshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDSshiftLL (MOVWconst [c]) x [d])
    // result: (ADDSconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDSshiftLL x (MOVWconst [c]) [d])
    // result: (ADDSconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDSshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDSshiftLLreg (MOVWconst [c]) x y)
    // result: (ADDSconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDSshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ADDSshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADDSshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDSshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDSshiftRA (MOVWconst [c]) x [d])
    // result: (ADDSconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDSshiftRA x (MOVWconst [c]) [d])
    // result: (ADDSconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDSshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDSshiftRAreg (MOVWconst [c]) x y)
    // result: (ADDSconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDSshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ADDSshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADDSshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDSshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDSshiftRL (MOVWconst [c]) x [d])
    // result: (ADDSconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDSshiftRL x (MOVWconst [c]) [d])
    // result: (ADDSconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDSshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDSshiftRLreg (MOVWconst [c]) x y)
    // result: (ADDSconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMADDSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDSshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ADDSshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADDSshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ADDconst [off1] (MOVWaddr [off2] {sym} ptr))
    // result: (MOVWaddr [off1+off2] {sym} ptr)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var ptr = v_0.Args[0];
        v.reset(OpARMMOVWaddr);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg(ptr);
        return true;

    } 
    // match: (ADDconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (ADDconst [c] x)
    // cond: !isARMImmRot(uint32(c)) && isARMImmRot(uint32(-c))
    // result: (SUBconst [-c] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(!isARMImmRot(uint32(c)) && isARMImmRot(uint32(-c)))) {
            break;
        }
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDconst [c] x)
    // cond: buildcfg.GOARM==7 && !isARMImmRot(uint32(c)) && uint32(c)>0xffff && uint32(-c)<=0xffff
    // result: (SUBconst [-c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(buildcfg.GOARM == 7 && !isARMImmRot(uint32(c)) && uint32(c) > 0xffff && uint32(-c) <= 0xffff)) {
            break;
        }
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c+d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c + d);
        return true;

    } 
    // match: (ADDconst [c] (ADDconst [d] x))
    // result: (ADDconst [c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDconst [c] (SUBconst [d] x))
    // result: (ADDconst [c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDconst [c] (RSBconst [d] x))
    // result: (RSBconst [c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMRSBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ADDshiftLL (MOVWconst [c]) x [d])
    // result: (ADDconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDshiftLL x (MOVWconst [c]) [d])
    // result: (ADDconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (ADDshiftLL [c] (SRLconst x [32-c]) x)
    // result: (SRRconst [32-c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != 32 - c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(32 - c);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDshiftLL <typ.UInt16> [8] (BFXU <typ.UInt16> [int32(armBFAuxInt(8, 8))] x) x)
    // result: (REV16 x)
    while (true) {
        if (v.Type != typ.UInt16 || auxIntToInt32(v.AuxInt) != 8 || v_0.Op != OpARMBFXU || v_0.Type != typ.UInt16 || auxIntToInt32(v_0.AuxInt) != int32(armBFAuxInt(8, 8))) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMREV16);
        v.AddArg(x);
        return true;

    } 
    // match: (ADDshiftLL <typ.UInt16> [8] (SRLconst <typ.UInt16> [24] (SLLconst [16] x)) x)
    // cond: buildcfg.GOARM>=6
    // result: (REV16 x)
    while (true) {
        if (v.Type != typ.UInt16 || auxIntToInt32(v.AuxInt) != 8 || v_0.Op != OpARMSRLconst || v_0.Type != typ.UInt16 || auxIntToInt32(v_0.AuxInt) != 24) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpARMSLLconst || auxIntToInt32(v_0_0.AuxInt) != 16) {
            break;
        }
        x = v_0_0.Args[0];
        if (x != v_1 || !(buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMREV16);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDshiftLLreg (MOVWconst [c]) x y)
    // result: (ADDconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ADDshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADDshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDshiftRA (MOVWconst [c]) x [d])
    // result: (ADDconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDshiftRA x (MOVWconst [c]) [d])
    // result: (ADDconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDshiftRAreg (MOVWconst [c]) x y)
    // result: (ADDconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ADDshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADDshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDshiftRL (MOVWconst [c]) x [d])
    // result: (ADDconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDshiftRL x (MOVWconst [c]) [d])
    // result: (ADDconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: (ADDshiftRL [c] (SLLconst x [32-c]) x)
    // result: (SRRconst [ c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != 32 - c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMADDshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ADDshiftRLreg (MOVWconst [c]) x y)
    // result: (ADDconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ADDshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ADDshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMADDshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMAND(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (AND x (MOVWconst [c]))
    // result: (ANDconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMANDconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (SLLconst [c] y))
    // result: (ANDshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMANDshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (SRLconst [c] y))
    // result: (ANDshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMANDshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (SRAconst [c] y))
    // result: (ANDshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMANDshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (SLL y z))
    // result: (ANDshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMANDshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (SRL y z))
    // result: (ANDshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMANDshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (SRA y z))
    // result: (ANDshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMANDshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x x)
    // result: x
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (AND x (MVN y))
    // result: (BIC x y)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMVN) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                y = v_1.Args[0];
                v.reset(OpARMBIC);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (MVNshiftLL y [c]))
    // result: (BICshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMVNshiftLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMBICshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (MVNshiftRL y [c]))
    // result: (BICshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMVNshiftRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMBICshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (AND x (MVNshiftRA y [c]))
    // result: (BICshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMVNshiftRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMBICshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ANDconst [0] _)
    // result: (MOVWconst [0])
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (ANDconst [c] x)
    // cond: int32(c)==-1
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (!(int32(c) == -1)) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (ANDconst [c] x)
    // cond: !isARMImmRot(uint32(c)) && isARMImmRot(^uint32(c))
    // result: (BICconst [int32(^uint32(c))] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(!isARMImmRot(uint32(c)) && isARMImmRot(~uint32(c)))) {
            break;
        }
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(int32(~uint32(c)));
        v.AddArg(x);
        return true;

    } 
    // match: (ANDconst [c] x)
    // cond: buildcfg.GOARM==7 && !isARMImmRot(uint32(c)) && uint32(c)>0xffff && ^uint32(c)<=0xffff
    // result: (BICconst [int32(^uint32(c))] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(buildcfg.GOARM == 7 && !isARMImmRot(uint32(c)) && uint32(c) > 0xffff && ~uint32(c) <= 0xffff)) {
            break;
        }
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(int32(~uint32(c)));
        v.AddArg(x);
        return true;

    } 
    // match: (ANDconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c&d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c & d);
        return true;

    } 
    // match: (ANDconst [c] (ANDconst [d] x))
    // result: (ANDconst [c&d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMANDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c & d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDshiftLL (MOVWconst [c]) x [d])
    // result: (ANDconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ANDshiftLL x (MOVWconst [c]) [d])
    // result: (ANDconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (ANDshiftLL y:(SLLconst x [c]) x [c])
    // result: y
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var y = v_0;
        if (y.Op != OpARMSLLconst || auxIntToInt32(y.AuxInt) != c) {
            break;
        }
        x = y.Args[0];
        if (x != v_1) {
            break;
        }
        v.copyOf(y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDshiftLLreg (MOVWconst [c]) x y)
    // result: (ANDconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ANDshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ANDshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMANDshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDshiftRA (MOVWconst [c]) x [d])
    // result: (ANDconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ANDshiftRA x (MOVWconst [c]) [d])
    // result: (ANDconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (ANDshiftRA y:(SRAconst x [c]) x [c])
    // result: y
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var y = v_0;
        if (y.Op != OpARMSRAconst || auxIntToInt32(y.AuxInt) != c) {
            break;
        }
        x = y.Args[0];
        if (x != v_1) {
            break;
        }
        v.copyOf(y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDshiftRAreg (MOVWconst [c]) x y)
    // result: (ANDconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ANDshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ANDshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMANDshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDshiftRL (MOVWconst [c]) x [d])
    // result: (ANDconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ANDshiftRL x (MOVWconst [c]) [d])
    // result: (ANDconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: (ANDshiftRL y:(SRLconst x [c]) x [c])
    // result: y
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var y = v_0;
        if (y.Op != OpARMSRLconst || auxIntToInt32(y.AuxInt) != c) {
            break;
        }
        x = y.Args[0];
        if (x != v_1) {
            break;
        }
        v.copyOf(y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMANDshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ANDshiftRLreg (MOVWconst [c]) x y)
    // result: (ANDconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ANDshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ANDshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMANDshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBFX(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (BFX [c] (MOVWconst [d]))
    // result: (MOVWconst [d<<(32-uint32(c&0xff)-uint32(c>>8))>>(32-uint32(c>>8))])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(d << (int)((32 - uint32(c & 0xff) - uint32(c >> 8))) >> (int)((32 - uint32(c >> 8))));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBFXU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (BFXU [c] (MOVWconst [d]))
    // result: (MOVWconst [int32(uint32(d)<<(32-uint32(c&0xff)-uint32(c>>8))>>(32-uint32(c>>8)))])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(d) << (int)((32 - uint32(c & 0xff) - uint32(c >> 8))) >> (int)((32 - uint32(c >> 8)))));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBIC(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BIC x (MOVWconst [c]))
    // result: (BICconst [c] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (BIC x (SLLconst [c] y))
    // result: (BICshiftLL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var y = v_1.Args[0];
        v.reset(OpARMBICshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (BIC x (SRLconst [c] y))
    // result: (BICshiftRL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMBICshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (BIC x (SRAconst [c] y))
    // result: (BICshiftRA x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMBICshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (BIC x (SLL y z))
    // result: (BICshiftLLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLL) {
            break;
        }
        var z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMBICshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (BIC x (SRL y z))
    // result: (BICshiftRLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRL) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMBICshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (BIC x (SRA y z))
    // result: (BICshiftRAreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRA) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMBICshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (BIC x x)
    // result: (MOVWconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (BICconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (BICconst [c] _)
    // cond: int32(c)==-1
    // result: (MOVWconst [0])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (!(int32(c) == -1)) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (BICconst [c] x)
    // cond: !isARMImmRot(uint32(c)) && isARMImmRot(^uint32(c))
    // result: (ANDconst [int32(^uint32(c))] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(!isARMImmRot(uint32(c)) && isARMImmRot(~uint32(c)))) {
            break;
        }
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(int32(~uint32(c)));
        v.AddArg(x);
        return true;

    } 
    // match: (BICconst [c] x)
    // cond: buildcfg.GOARM==7 && !isARMImmRot(uint32(c)) && uint32(c)>0xffff && ^uint32(c)<=0xffff
    // result: (ANDconst [int32(^uint32(c))] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(buildcfg.GOARM == 7 && !isARMImmRot(uint32(c)) && uint32(c) > 0xffff && ~uint32(c) <= 0xffff)) {
            break;
        }
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(int32(~uint32(c)));
        v.AddArg(x);
        return true;

    } 
    // match: (BICconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d&^c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(d & ~c);
        return true;

    } 
    // match: (BICconst [c] (BICconst [d] x))
    // result: (BICconst [c|d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMBICconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(c | d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BICshiftLL x (MOVWconst [c]) [d])
    // result: (BICconst x [c<<uint64(d)])
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (BICshiftLL (SLLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BICshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (BICshiftLL x y [c])
    while (true) {
        var x = v_0;
        var y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMBICshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BICshiftRA x (MOVWconst [c]) [d])
    // result: (BICconst x [c>>uint64(d)])
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (BICshiftRA (SRAconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRAconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BICshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (BICshiftRA x y [c])
    while (true) {
        var x = v_0;
        var y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMBICshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BICshiftRL x (MOVWconst [c]) [d])
    // result: (BICconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMBICconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: (BICshiftRL (SRLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMBICshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (BICshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (BICshiftRL x y [c])
    while (true) {
        var x = v_0;
        var y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMBICshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMN(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMN x (MOVWconst [c]))
    // result: (CMNconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMCMNconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (CMN x (SLLconst [c] y))
    // result: (CMNshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMCMNshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (CMN x (SRLconst [c] y))
    // result: (CMNshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMCMNshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (CMN x (SRAconst [c] y))
    // result: (CMNshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMCMNshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (CMN x (SLL y z))
    // result: (CMNshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMCMNshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (CMN x (SRL y z))
    // result: (CMNshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMCMNshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (CMN x (SRA y z))
    // result: (CMNshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMCMNshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMNconst (MOVWconst [x]) [y])
    // result: (FlagConstant [addFlags32(x,y)])
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(addFlags32(x, y));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMNshiftLL (MOVWconst [c]) x [d])
    // result: (CMNconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMNshiftLL x (MOVWconst [c]) [d])
    // result: (CMNconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMNshiftLLreg (MOVWconst [c]) x y)
    // result: (CMNconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMNshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (CMNshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMCMNshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMNshiftRA (MOVWconst [c]) x [d])
    // result: (CMNconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMNshiftRA x (MOVWconst [c]) [d])
    // result: (CMNconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMNshiftRAreg (MOVWconst [c]) x y)
    // result: (CMNconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMNshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (CMNshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMCMNshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMNshiftRL (MOVWconst [c]) x [d])
    // result: (CMNconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMNshiftRL x (MOVWconst [c]) [d])
    // result: (CMNconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMNshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMNshiftRLreg (MOVWconst [c]) x y)
    // result: (CMNconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMCMNconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMNshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (CMNshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMCMNshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMOVWHSconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMOVWHSconst _ (FlagConstant [fc]) [c])
    // cond: fc.uge()
    // result: (MOVWconst [c])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_1.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_1.AuxInt);
        if (!(fc.uge())) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    } 
    // match: (CMOVWHSconst x (FlagConstant [fc]) [c])
    // cond: fc.ult()
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMFlagConstant) {
            break;
        }
        fc = auxIntToFlagConstant(v_1.AuxInt);
        if (!(fc.ult())) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (CMOVWHSconst x (InvertFlags flags) [c])
    // result: (CMOVWLSconst x flags [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMInvertFlags) {
            break;
        }
        var flags = v_1.Args[0];
        v.reset(OpARMCMOVWLSconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMOVWLSconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMOVWLSconst _ (FlagConstant [fc]) [c])
    // cond: fc.ule()
    // result: (MOVWconst [c])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_1.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_1.AuxInt);
        if (!(fc.ule())) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    } 
    // match: (CMOVWLSconst x (FlagConstant [fc]) [c])
    // cond: fc.ugt()
    // result: x
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMFlagConstant) {
            break;
        }
        fc = auxIntToFlagConstant(v_1.AuxInt);
        if (!(fc.ugt())) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (CMOVWLSconst x (InvertFlags flags) [c])
    // result: (CMOVWHSconst x flags [c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMInvertFlags) {
            break;
        }
        var flags = v_1.Args[0];
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMP(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMP x (MOVWconst [c]))
    // result: (CMPconst [c] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMPconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (CMP (MOVWconst [c]) x)
    // result: (InvertFlags (CMPconst [c] x))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x y)
    // cond: canonLessThan(x,y)
    // result: (InvertFlags (CMP y x))
    while (true) {
        x = v_0;
        var y = v_1;
        if (!(canonLessThan(x, y))) {
            break;
        }
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x (SLLconst [c] y))
    // result: (CMPshiftLL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMCMPshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (CMP (SLLconst [c] y) x)
    // result: (InvertFlags (CMPshiftLL x y [c]))
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMPshiftLL, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x (SRLconst [c] y))
    // result: (CMPshiftRL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMCMPshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (CMP (SRLconst [c] y) x)
    // result: (InvertFlags (CMPshiftRL x y [c]))
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMPshiftRL, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x (SRAconst [c] y))
    // result: (CMPshiftRA x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMCMPshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (CMP (SRAconst [c] y) x)
    // result: (InvertFlags (CMPshiftRA x y [c]))
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMPshiftRA, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x (SLL y z))
    // result: (CMPshiftLLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLL) {
            break;
        }
        var z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMCMPshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (CMP (SLL y z) x)
    // result: (InvertFlags (CMPshiftLLreg x y z))
    while (true) {
        if (v_0.Op != OpARMSLL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
        v0.AddArg3(x, y, z);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x (SRL y z))
    // result: (CMPshiftRLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRL) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMCMPshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (CMP (SRL y z) x)
    // result: (InvertFlags (CMPshiftRLreg x y z))
    while (true) {
        if (v_0.Op != OpARMSRL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
        v0.AddArg3(x, y, z);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMP x (SRA y z))
    // result: (CMPshiftRAreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRA) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMCMPshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (CMP (SRA y z) x)
    // result: (InvertFlags (CMPshiftRAreg x y z))
    while (true) {
        if (v_0.Op != OpARMSRA) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMInvertFlags);
        v0 = b.NewValue0(v.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
        v0.AddArg3(x, y, z);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMPD x (MOVDconst [0]))
    // result: (CMPD0 x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVDconst || auxIntToFloat64(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpARMCMPD0);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPF(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (CMPF x (MOVFconst [0]))
    // result: (CMPF0 x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVFconst || auxIntToFloat64(v_1.AuxInt) != 0) {
            break;
        }
        v.reset(OpARMCMPF0);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (CMPconst (MOVWconst [x]) [y])
    // result: (FlagConstant [subFlags32(x,y)])
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(subFlags32(x, y));
        return true;

    } 
    // match: (CMPconst (MOVBUreg _) [c])
    // cond: 0xff < c
    // result: (FlagConstant [subFlags32(0, 1)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVBUreg || !(0xff < c)) {
            break;
        }
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(subFlags32(0, 1));
        return true;

    } 
    // match: (CMPconst (MOVHUreg _) [c])
    // cond: 0xffff < c
    // result: (FlagConstant [subFlags32(0, 1)])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVHUreg || !(0xffff < c)) {
            break;
        }
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(subFlags32(0, 1));
        return true;

    } 
    // match: (CMPconst (ANDconst _ [m]) [n])
    // cond: 0 <= m && m < n
    // result: (FlagConstant [subFlags32(0, 1)])
    while (true) {
        var n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMANDconst) {
            break;
        }
        var m = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= m && m < n)) {
            break;
        }
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(subFlags32(0, 1));
        return true;

    } 
    // match: (CMPconst (SRLconst _ [c]) [n])
    // cond: 0 <= n && 0 < c && c <= 32 && (1<<uint32(32-c)) <= uint32(n)
    // result: (FlagConstant [subFlags32(0, 1)])
    while (true) {
        n = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        if (!(0 <= n && 0 < c && c <= 32 && (1 << (int)(uint32(32 - c))) <= uint32(n))) {
            break;
        }
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(subFlags32(0, 1));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPshiftLL (MOVWconst [c]) x [d])
    // result: (InvertFlags (CMPconst [c] (SLLconst <x.Type> x [d])))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        var v1 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v1.AuxInt = int32ToAuxInt(d);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMPshiftLL x (MOVWconst [c]) [d])
    // result: (CMPconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMPconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPshiftLLreg (MOVWconst [c]) x y)
    // result: (InvertFlags (CMPconst [c] (SLL <x.Type> x y)))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        var v1 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMPshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (CMPshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMCMPshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPshiftRA (MOVWconst [c]) x [d])
    // result: (InvertFlags (CMPconst [c] (SRAconst <x.Type> x [d])))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        var v1 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v1.AuxInt = int32ToAuxInt(d);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMPshiftRA x (MOVWconst [c]) [d])
    // result: (CMPconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMPconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPshiftRAreg (MOVWconst [c]) x y)
    // result: (InvertFlags (CMPconst [c] (SRA <x.Type> x y)))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        var v1 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMPshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (CMPshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMCMPshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPshiftRL (MOVWconst [c]) x [d])
    // result: (InvertFlags (CMPconst [c] (SRLconst <x.Type> x [d])))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        var v1 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v1.AuxInt = int32ToAuxInt(d);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMPshiftRL x (MOVWconst [c]) [d])
    // result: (CMPconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMCMPconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMCMPshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (CMPshiftRLreg (MOVWconst [c]) x y)
    // result: (InvertFlags (CMPconst [c] (SRL <x.Type> x y)))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMInvertFlags);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(c);
        var v1 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (CMPshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (CMPshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMCMPshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Equal (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.eq())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.eq()));
        return true;

    } 
    // match: (Equal (InvertFlags x))
    // result: (Equal x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMEqual);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMGreaterEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (GreaterEqual (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.ge())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.ge()));
        return true;

    } 
    // match: (GreaterEqual (InvertFlags x))
    // result: (LessEqual x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMLessEqual);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMGreaterEqualU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (GreaterEqualU (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.uge())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.uge()));
        return true;

    } 
    // match: (GreaterEqualU (InvertFlags x))
    // result: (LessEqualU x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMLessEqualU);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMGreaterThan(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (GreaterThan (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.gt())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.gt()));
        return true;

    } 
    // match: (GreaterThan (InvertFlags x))
    // result: (LessThan x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMLessThan);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMGreaterThanU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (GreaterThanU (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.ugt())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.ugt()));
        return true;

    } 
    // match: (GreaterThanU (InvertFlags x))
    // result: (LessThanU x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMLessThanU);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMLessEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LessEqual (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.le())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.le()));
        return true;

    } 
    // match: (LessEqual (InvertFlags x))
    // result: (GreaterEqual x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMGreaterEqual);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMLessEqualU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LessEqualU (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.ule())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.ule()));
        return true;

    } 
    // match: (LessEqualU (InvertFlags x))
    // result: (GreaterEqualU x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMGreaterEqualU);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMLessThan(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LessThan (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.lt())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.lt()));
        return true;

    } 
    // match: (LessThan (InvertFlags x))
    // result: (GreaterThan x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMGreaterThan);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMLessThanU(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LessThanU (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.ult())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.ult()));
        return true;

    } 
    // match: (LessThanU (InvertFlags x))
    // result: (GreaterThanU x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMGreaterThanU);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBUload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBUload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVBUload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVBUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVBUload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVBUload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVBUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVBUreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVBstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVBUreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUload [0] {sym} (ADD ptr idx) mem)
    // cond: sym == nil
    // result: (MOVBUloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVBUloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVBUload [off] {sym} (SB) _)
    // cond: symIsRO(sym)
    // result: (MOVWconst [int32(read8(sym, int64(off)))])
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpSB || !(symIsRO(sym))) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(read8(sym, int64(off))));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBUloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBUloadidx ptr idx (MOVBstoreidx ptr2 idx x _))
    // cond: isSamePtr(ptr, ptr2)
    // result: (MOVBUreg x)
    while (true) {
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVBstoreidx) {
            break;
        }
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVBUreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUloadidx ptr (MOVWconst [c]) mem)
    // result: (MOVBUload [c] ptr mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVBUload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBUloadidx (MOVWconst [c]) ptr mem)
    // result: (MOVBUload [c] ptr mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVBUload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBUreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVBUreg x:(MOVBUload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpARMMOVBUload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUreg (ANDconst [c] x))
    // result: (ANDconst [c&0xff] x)
    while (true) {
        if (v_0.Op != OpARMANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c & 0xff);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUreg x:(MOVBUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVBUreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBUreg (MOVWconst [c]))
    // result: (MOVWconst [int32(uint8(c))])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint8(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVBload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVBload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVBload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVBload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBload [off] {sym} ptr (MOVBstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVBreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVBstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVBreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBload [0] {sym} (ADD ptr idx) mem)
    // cond: sym == nil
    // result: (MOVBloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVBloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBloadidx ptr idx (MOVBstoreidx ptr2 idx x _))
    // cond: isSamePtr(ptr, ptr2)
    // result: (MOVBreg x)
    while (true) {
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVBstoreidx) {
            break;
        }
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVBreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBloadidx ptr (MOVWconst [c]) mem)
    // result: (MOVBload [c] ptr mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVBload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVBloadidx (MOVWconst [c]) ptr mem)
    // result: (MOVBload [c] ptr mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVBload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVBreg x:(MOVBload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpARMMOVBload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBreg (ANDconst [c] x))
    // cond: c & 0x80 == 0
    // result: (ANDconst [c&0x7f] x)
    while (true) {
        if (v_0.Op != OpARMANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c & 0x80 == 0)) {
            break;
        }
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c & 0x7f);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBreg x:(MOVBreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVBreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVBreg (MOVWconst [c]))
    // result: (MOVWconst [int32(int8(c))])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(int8(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // result: (MOVBstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVBstore [off1] {sym} (SUBconst [off2] ptr) val mem)
    // result: (MOVBstore [off1-off2] {sym} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVBstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVBstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVBreg) {
            break;
        }
        var x = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVBUreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVBUreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVHreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVHreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVBstore [off] {sym} ptr (MOVHUreg x) mem)
    // result: (MOVBstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVHUreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVBstore [0] {sym} (ADD ptr idx) val mem)
    // cond: sym == nil
    // result: (MOVBstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVBstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVBstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVBstoreidx ptr (MOVWconst [c]) val mem)
    // result: (MOVBstore [c] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVBstoreidx (MOVWconst [c]) ptr val mem)
    // result: (MOVBstore [c] ptr val mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVDload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVDload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVDload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVDload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVDload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVDload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVDload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVDload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVDload [off] {sym} ptr (MOVDstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVDstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVDstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVDstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // result: (MOVDstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        v.reset(OpARMMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVDstore [off1] {sym} (SUBconst [off2] ptr) val mem)
    // result: (MOVDstore [off1-off2] {sym} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        v.reset(OpARMMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVDstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVDstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVDstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVFload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVFload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVFload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVFload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVFload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVFload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVFload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVFload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVFload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVFload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVFload [off] {sym} ptr (MOVFstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVFstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVFstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVFstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // result: (MOVFstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        v.reset(OpARMMOVFstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVFstore [off1] {sym} (SUBconst [off2] ptr) val mem)
    // result: (MOVFstore [off1-off2] {sym} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        v.reset(OpARMMOVFstore);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVFstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVFstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVFstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHUload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVHUload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVHUload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVHUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVHUload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVHUload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHUload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVHUload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVHUreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVHstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVHUreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUload [0] {sym} (ADD ptr idx) mem)
    // cond: sym == nil
    // result: (MOVHUloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVHUloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVHUload [off] {sym} (SB) _)
    // cond: symIsRO(sym)
    // result: (MOVWconst [int32(read16(sym, int64(off), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpSB || !(symIsRO(sym))) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(read16(sym, int64(off), config.ctxt.Arch.ByteOrder)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHUloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHUloadidx ptr idx (MOVHstoreidx ptr2 idx x _))
    // cond: isSamePtr(ptr, ptr2)
    // result: (MOVHUreg x)
    while (true) {
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVHstoreidx) {
            break;
        }
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVHUreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUloadidx ptr (MOVWconst [c]) mem)
    // result: (MOVHUload [c] ptr mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVHUload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHUloadidx (MOVWconst [c]) ptr mem)
    // result: (MOVHUload [c] ptr mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVHUload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHUreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVHUreg x:(MOVBUload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpARMMOVBUload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg x:(MOVHUload _ _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVHUload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg (ANDconst [c] x))
    // result: (ANDconst [c&0xffff] x)
    while (true) {
        if (v_0.Op != OpARMANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c & 0xffff);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg x:(MOVBUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVBUreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg x:(MOVHUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVHUreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHUreg (MOVWconst [c]))
    // result: (MOVWconst [int32(uint16(c))])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint16(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVHload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVHload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVHload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVHload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHload [off] {sym} ptr (MOVHstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: (MOVHreg x)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVHstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVHreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHload [0] {sym} (ADD ptr idx) mem)
    // cond: sym == nil
    // result: (MOVHloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVHloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHloadidx ptr idx (MOVHstoreidx ptr2 idx x _))
    // cond: isSamePtr(ptr, ptr2)
    // result: (MOVHreg x)
    while (true) {
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVHstoreidx) {
            break;
        }
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(isSamePtr(ptr, ptr2))) {
            break;
        }
        v.reset(OpARMMOVHreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHloadidx ptr (MOVWconst [c]) mem)
    // result: (MOVHload [c] ptr mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVHload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVHloadidx (MOVWconst [c]) ptr mem)
    // result: (MOVHload [c] ptr mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVHload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVHreg x:(MOVBload _ _))
    // result: (MOVWreg x)
    while (true) {
        var x = v_0;
        if (x.Op != OpARMMOVBload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVBUload _ _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVBUload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVHload _ _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVHload) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg (ANDconst [c] x))
    // cond: c & 0x8000 == 0
    // result: (ANDconst [c&0x7fff] x)
    while (true) {
        if (v_0.Op != OpARMANDconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        if (!(c & 0x8000 == 0)) {
            break;
        }
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c & 0x7fff);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVBreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVBreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVBUreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVBUreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg x:(MOVHreg _))
    // result: (MOVWreg x)
    while (true) {
        x = v_0;
        if (x.Op != OpARMMOVHreg) {
            break;
        }
        v.reset(OpARMMOVWreg);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVHreg (MOVWconst [c]))
    // result: (MOVWconst [int32(int16(c))])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(int16(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // result: (MOVHstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVHstore [off1] {sym} (SUBconst [off2] ptr) val mem)
    // result: (MOVHstore [off1-off2] {sym} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVHstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVHstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVHstore [off] {sym} ptr (MOVHreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVHreg) {
            break;
        }
        var x = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVHstore [off] {sym} ptr (MOVHUreg x) mem)
    // result: (MOVHstore [off] {sym} ptr x mem)
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVHUreg) {
            break;
        }
        x = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(off);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, x, mem);
        return true;

    } 
    // match: (MOVHstore [0] {sym} (ADD ptr idx) val mem)
    // cond: sym == nil
    // result: (MOVHstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVHstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVHstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVHstoreidx ptr (MOVWconst [c]) val mem)
    // result: (MOVHstore [c] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVHstoreidx (MOVWconst [c]) ptr val mem)
    // result: (MOVHstore [c] ptr val mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWload(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var config = b.Func.Config; 
    // match: (MOVWload [off1] {sym} (ADDconst [off2] ptr) mem)
    // result: (MOVWload [off1+off2] {sym} ptr mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var mem = v_1;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWload [off1] {sym} (SUBconst [off2] ptr) mem)
    // result: (MOVWload [off1-off2] {sym} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        mem = v_1;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWload [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVWload [off1+off2] {mergeSym(sym1,sym2)} ptr mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWload [off] {sym} ptr (MOVWstore [off2] {sym2} ptr2 x _))
    // cond: sym == sym2 && off == off2 && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        ptr = v_0;
        if (v_1.Op != OpARMMOVWstore) {
            break;
        }
        off2 = auxIntToInt32(v_1.AuxInt);
        sym2 = auxToSym(v_1.Aux);
        var x = v_1.Args[1];
        var ptr2 = v_1.Args[0];
        if (!(sym == sym2 && off == off2 && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (MOVWload [0] {sym} (ADD ptr idx) mem)
    // cond: sym == nil
    // result: (MOVWloadidx ptr idx mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWloadidx);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWload [0] {sym} (ADDshiftLL ptr idx [c]) mem)
    // cond: sym == nil
    // result: (MOVWloadshiftLL ptr idx [c] mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDshiftLL) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWloadshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWload [0] {sym} (ADDshiftRL ptr idx [c]) mem)
    // cond: sym == nil
    // result: (MOVWloadshiftRL ptr idx [c] mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDshiftRL) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWloadshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWload [0] {sym} (ADDshiftRA ptr idx [c]) mem)
    // cond: sym == nil
    // result: (MOVWloadshiftRA ptr idx [c] mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDshiftRA) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[1];
        ptr = v_0.Args[0];
        mem = v_1;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWloadshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWload [off] {sym} (SB) _)
    // cond: symIsRO(sym)
    // result: (MOVWconst [int32(read32(sym, int64(off), config.ctxt.Arch.ByteOrder))])
    while (true) {
        off = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpSB || !(symIsRO(sym))) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(read32(sym, int64(off), config.ctxt.Arch.ByteOrder)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWloadidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWloadidx ptr idx (MOVWstoreidx ptr2 idx x _))
    // cond: isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVWstoreidx) {
            break;
        }
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (MOVWloadidx ptr (MOVWconst [c]) mem)
    // result: (MOVWload [c] ptr mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWloadidx (MOVWconst [c]) ptr mem)
    // result: (MOVWload [c] ptr mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (MOVWloadidx ptr (SLLconst idx [c]) mem)
    // result: (MOVWloadshiftLL ptr idx [c] mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        idx = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVWloadshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWloadidx (SLLconst idx [c]) ptr mem)
    // result: (MOVWloadshiftLL ptr idx [c] mem)
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[0];
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVWloadshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWloadidx ptr (SRLconst idx [c]) mem)
    // result: (MOVWloadshiftRL ptr idx [c] mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        idx = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVWloadshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWloadidx (SRLconst idx [c]) ptr mem)
    // result: (MOVWloadshiftRL ptr idx [c] mem)
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[0];
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVWloadshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWloadidx ptr (SRAconst idx [c]) mem)
    // result: (MOVWloadshiftRA ptr idx [c] mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        idx = v_1.Args[0];
        mem = v_2;
        v.reset(OpARMMOVWloadshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    } 
    // match: (MOVWloadidx (SRAconst idx [c]) ptr mem)
    // result: (MOVWloadshiftRA ptr idx [c] mem)
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[0];
        ptr = v_1;
        mem = v_2;
        v.reset(OpARMMOVWloadshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, idx, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWloadshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWloadshiftLL ptr idx [c] (MOVWstoreshiftLL ptr2 idx [d] x _))
    // cond: c==d && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVWstoreshiftLL) {
            break;
        }
        var d = auxIntToInt32(v_2.AuxInt);
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(c == d && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (MOVWloadshiftLL ptr (MOVWconst [c]) [d] mem)
    // result: (MOVWload [int32(uint32(c)<<uint64(d))] ptr mem)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) << (int)(uint64(d))));
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWloadshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWloadshiftRA ptr idx [c] (MOVWstoreshiftRA ptr2 idx [d] x _))
    // cond: c==d && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVWstoreshiftRA) {
            break;
        }
        var d = auxIntToInt32(v_2.AuxInt);
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(c == d && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (MOVWloadshiftRA ptr (MOVWconst [c]) [d] mem)
    // result: (MOVWload [c>>uint64(d)] ptr mem)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWloadshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWloadshiftRL ptr idx [c] (MOVWstoreshiftRL ptr2 idx [d] x _))
    // cond: c==d && isSamePtr(ptr, ptr2)
    // result: x
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        var ptr = v_0;
        var idx = v_1;
        if (v_2.Op != OpARMMOVWstoreshiftRL) {
            break;
        }
        var d = auxIntToInt32(v_2.AuxInt);
        var x = v_2.Args[2];
        var ptr2 = v_2.Args[0];
        if (idx != v_2.Args[1] || !(c == d && isSamePtr(ptr, ptr2))) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (MOVWloadshiftRL ptr (MOVWconst [c]) [d] mem)
    // result: (MOVWload [int32(uint32(c)>>uint64(d))] ptr mem)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var mem = v_2;
        v.reset(OpARMMOVWload);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWnop(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVWnop (MOVWconst [c]))
    // result: (MOVWconst [c])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MOVWreg x)
    // cond: x.Uses == 1
    // result: (MOVWnop x)
    while (true) {
        var x = v_0;
        if (!(x.Uses == 1)) {
            break;
        }
        v.reset(OpARMMOVWnop);
        v.AddArg(x);
        return true;

    } 
    // match: (MOVWreg (MOVWconst [c]))
    // result: (MOVWconst [c])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWstore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstore [off1] {sym} (ADDconst [off2] ptr) val mem)
    // result: (MOVWstore [off1+off2] {sym} ptr val mem)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        var sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var ptr = v_0.Args[0];
        var val = v_1;
        var mem = v_2;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstore [off1] {sym} (SUBconst [off2] ptr) val mem)
    // result: (MOVWstore [off1-off2] {sym} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 - off2);
        v.Aux = symToAux(sym);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstore [off1] {sym1} (MOVWaddr [off2] {sym2} ptr) val mem)
    // cond: canMergeSym(sym1,sym2)
    // result: (MOVWstore [off1+off2] {mergeSym(sym1,sym2)} ptr val mem)
    while (true) {
        off1 = auxIntToInt32(v.AuxInt);
        var sym1 = auxToSym(v.Aux);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        off2 = auxIntToInt32(v_0.AuxInt);
        var sym2 = auxToSym(v_0.Aux);
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(canMergeSym(sym1, sym2))) {
            break;
        }
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(off1 + off2);
        v.Aux = symToAux(mergeSym(sym1, sym2));
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstore [0] {sym} (ADD ptr idx) val mem)
    // cond: sym == nil
    // result: (MOVWstoreidx ptr idx val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADD) {
            break;
        }
        var idx = v_0.Args[1];
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWstoreidx);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstore [0] {sym} (ADDshiftLL ptr idx [c]) val mem)
    // cond: sym == nil
    // result: (MOVWstoreshiftLL ptr idx [c] val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDshiftLL) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[1];
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWstoreshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstore [0] {sym} (ADDshiftRL ptr idx [c]) val mem)
    // cond: sym == nil
    // result: (MOVWstoreshiftRL ptr idx [c] val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDshiftRL) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[1];
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWstoreshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstore [0] {sym} (ADDshiftRA ptr idx [c]) val mem)
    // cond: sym == nil
    // result: (MOVWstoreshiftRA ptr idx [c] val mem)
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        sym = auxToSym(v.Aux);
        if (v_0.Op != OpARMADDshiftRA) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[1];
        ptr = v_0.Args[0];
        val = v_1;
        mem = v_2;
        if (!(sym == null)) {
            break;
        }
        v.reset(OpARMMOVWstoreshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWstoreidx(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstoreidx ptr (MOVWconst [c]) val mem)
    // result: (MOVWstore [c] ptr val mem)
    while (true) {
        var ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx (MOVWconst [c]) ptr val mem)
    // result: (MOVWstore [c] ptr val mem)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        ptr = v_1;
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx ptr (SLLconst idx [c]) val mem)
    // result: (MOVWstoreshiftLL ptr idx [c] val mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var idx = v_1.Args[0];
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstoreshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx (SLLconst idx [c]) ptr val mem)
    // result: (MOVWstoreshiftLL ptr idx [c] val mem)
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[0];
        ptr = v_1;
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstoreshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx ptr (SRLconst idx [c]) val mem)
    // result: (MOVWstoreshiftRL ptr idx [c] val mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        idx = v_1.Args[0];
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstoreshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx (SRLconst idx [c]) ptr val mem)
    // result: (MOVWstoreshiftRL ptr idx [c] val mem)
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[0];
        ptr = v_1;
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstoreshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx ptr (SRAconst idx [c]) val mem)
    // result: (MOVWstoreshiftRA ptr idx [c] val mem)
    while (true) {
        ptr = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        idx = v_1.Args[0];
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstoreshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    } 
    // match: (MOVWstoreidx (SRAconst idx [c]) ptr val mem)
    // result: (MOVWstoreshiftRA ptr idx [c] val mem)
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        idx = v_0.Args[0];
        ptr = v_1;
        val = v_2;
        mem = v_3;
        v.reset(OpARMMOVWstoreshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg4(ptr, idx, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWstoreshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstoreshiftLL ptr (MOVWconst [c]) [d] val mem)
    // result: (MOVWstore [int32(uint32(c)<<uint64(d))] ptr val mem)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        var ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) << (int)(uint64(d))));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWstoreshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstoreshiftRA ptr (MOVWconst [c]) [d] val mem)
    // result: (MOVWstore [c>>uint64(d)] ptr val mem)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        var ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMOVWstoreshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MOVWstoreshiftRL ptr (MOVWconst [c]) [d] val mem)
    // result: (MOVWstore [int32(uint32(c)>>uint64(d))] ptr val mem)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        var ptr = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var val = v_2;
        var mem = v_3;
        v.reset(OpARMMOVWstore);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMUL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MUL x (MOVWconst [c]))
    // cond: int32(c) == -1
    // result: (RSBconst [0] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                if (!(int32(c) == -1)) {
                    continue;
                }

                v.reset(OpARMRSBconst);
                v.AuxInt = int32ToAuxInt(0);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL _ (MOVWconst [0]))
    // result: (MOVWconst [0])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_1.Op != OpARMMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.reset(OpARMMOVWconst);
                v.AuxInt = int32ToAuxInt(0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [1]))
    // result: x
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst || auxIntToInt32(v_1.AuxInt) != 1) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                v.copyOf(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: isPowerOfTwo32(c)
    // result: (SLLconst [int32(log32(c))] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(isPowerOfTwo32(c))) {
                    continue;
                }

                v.reset(OpARMSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log32(c)));
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: isPowerOfTwo32(c-1) && c >= 3
    // result: (ADDshiftLL x x [int32(log32(c-1))])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(isPowerOfTwo32(c - 1) && c >= 3)) {
                    continue;
                }

                v.reset(OpARMADDshiftLL);
                v.AuxInt = int32ToAuxInt(int32(log32(c - 1)));
                v.AddArg2(x, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: isPowerOfTwo32(c+1) && c >= 7
    // result: (RSBshiftLL x x [int32(log32(c+1))])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(isPowerOfTwo32(c + 1) && c >= 7)) {
                    continue;
                }

                v.reset(OpARMRSBshiftLL);
                v.AuxInt = int32ToAuxInt(int32(log32(c + 1)));
                v.AddArg2(x, x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: c%3 == 0 && isPowerOfTwo32(c/3)
    // result: (SLLconst [int32(log32(c/3))] (ADDshiftLL <x.Type> x x [1]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(c % 3 == 0 && isPowerOfTwo32(c / 3))) {
                    continue;
                }

                v.reset(OpARMSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log32(c / 3)));
                var v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
                v0.AuxInt = int32ToAuxInt(1);
                v0.AddArg2(x, x);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: c%5 == 0 && isPowerOfTwo32(c/5)
    // result: (SLLconst [int32(log32(c/5))] (ADDshiftLL <x.Type> x x [2]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(c % 5 == 0 && isPowerOfTwo32(c / 5))) {
                    continue;
                }

                v.reset(OpARMSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log32(c / 5)));
                v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
                v0.AuxInt = int32ToAuxInt(2);
                v0.AddArg2(x, x);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: c%7 == 0 && isPowerOfTwo32(c/7)
    // result: (SLLconst [int32(log32(c/7))] (RSBshiftLL <x.Type> x x [3]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(c % 7 == 0 && isPowerOfTwo32(c / 7))) {
                    continue;
                }

                v.reset(OpARMSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log32(c / 7)));
                v0 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
                v0.AuxInt = int32ToAuxInt(3);
                v0.AddArg2(x, x);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL x (MOVWconst [c]))
    // cond: c%9 == 0 && isPowerOfTwo32(c/9)
    // result: (SLLconst [int32(log32(c/9))] (ADDshiftLL <x.Type> x x [3]))
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                if (!(c % 9 == 0 && isPowerOfTwo32(c / 9))) {
                    continue;
                }

                v.reset(OpARMSLLconst);
                v.AuxInt = int32ToAuxInt(int32(log32(c / 9)));
                v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
                v0.AuxInt = int32ToAuxInt(3);
                v0.AddArg2(x, x);
                v.AddArg(v0);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (MUL (MOVWconst [c]) (MOVWconst [d]))
    // result: (MOVWconst [c*d])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_0.AuxInt);
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                }

                var d = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMMOVWconst);
                v.AuxInt = int32ToAuxInt(c * d);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMULA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: c == -1
    // result: (SUB a x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var a = v_2;
        if (!(c == -1)) {
            break;
        }
        v.reset(OpARMSUB);
        v.AddArg2(a, x);
        return true;

    } 
    // match: (MULA _ (MOVWconst [0]) a)
    // result: a
    while (true) {
        if (v_1.Op != OpARMMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        a = v_2;
        v.copyOf(a);
        return true;

    } 
    // match: (MULA x (MOVWconst [1]) a)
    // result: (ADD x a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst || auxIntToInt32(v_1.AuxInt) != 1) {
            break;
        }
        a = v_2;
        v.reset(OpARMADD);
        v.AddArg2(x, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: isPowerOfTwo32(c)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c))] x) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpARMADD);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c)));
        v0.AddArg(x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: isPowerOfTwo32(c-1) && c >= 3
    // result: (ADD (ADDshiftLL <x.Type> x x [int32(log32(c-1))]) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(isPowerOfTwo32(c - 1) && c >= 3)) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: isPowerOfTwo32(c+1) && c >= 7
    // result: (ADD (RSBshiftLL <x.Type> x x [int32(log32(c+1))]) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(isPowerOfTwo32(c + 1) && c >= 7)) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c + 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: c%3 == 0 && isPowerOfTwo32(c/3)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/3))] (ADDshiftLL <x.Type> x x [1])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 3 == 0 && isPowerOfTwo32(c / 3))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 3)));
        var v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(1);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: c%5 == 0 && isPowerOfTwo32(c/5)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/5))] (ADDshiftLL <x.Type> x x [2])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 5 == 0 && isPowerOfTwo32(c / 5))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 5)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(2);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: c%7 == 0 && isPowerOfTwo32(c/7)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/7))] (RSBshiftLL <x.Type> x x [3])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 7 == 0 && isPowerOfTwo32(c / 7))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 7)));
        v1 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA x (MOVWconst [c]) a)
    // cond: c%9 == 0 && isPowerOfTwo32(c/9)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/9))] (ADDshiftLL <x.Type> x x [3])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 9 == 0 && isPowerOfTwo32(c / 9))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 9)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: c == -1
    // result: (SUB a x)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c == -1)) {
            break;
        }
        v.reset(OpARMSUB);
        v.AddArg2(a, x);
        return true;

    } 
    // match: (MULA (MOVWconst [0]) _ a)
    // result: a
    while (true) {
        if (v_0.Op != OpARMMOVWconst || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        a = v_2;
        v.copyOf(a);
        return true;

    } 
    // match: (MULA (MOVWconst [1]) x a)
    // result: (ADD x a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst || auxIntToInt32(v_0.AuxInt) != 1) {
            break;
        }
        x = v_1;
        a = v_2;
        v.reset(OpARMADD);
        v.AddArg2(x, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: isPowerOfTwo32(c)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c))] x) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c)));
        v0.AddArg(x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: isPowerOfTwo32(c-1) && c >= 3
    // result: (ADD (ADDshiftLL <x.Type> x x [int32(log32(c-1))]) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(isPowerOfTwo32(c - 1) && c >= 3)) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: isPowerOfTwo32(c+1) && c >= 7
    // result: (ADD (RSBshiftLL <x.Type> x x [int32(log32(c+1))]) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(isPowerOfTwo32(c + 1) && c >= 7)) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c + 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: c%3 == 0 && isPowerOfTwo32(c/3)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/3))] (ADDshiftLL <x.Type> x x [1])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 3 == 0 && isPowerOfTwo32(c / 3))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 3)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(1);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: c%5 == 0 && isPowerOfTwo32(c/5)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/5))] (ADDshiftLL <x.Type> x x [2])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 5 == 0 && isPowerOfTwo32(c / 5))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 5)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(2);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: c%7 == 0 && isPowerOfTwo32(c/7)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/7))] (RSBshiftLL <x.Type> x x [3])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 7 == 0 && isPowerOfTwo32(c / 7))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 7)));
        v1 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) x a)
    // cond: c%9 == 0 && isPowerOfTwo32(c/9)
    // result: (ADD (SLLconst <x.Type> [int32(log32(c/9))] (ADDshiftLL <x.Type> x x [3])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 9 == 0 && isPowerOfTwo32(c / 9))) {
            break;
        }
        v.reset(OpARMADD);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 9)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULA (MOVWconst [c]) (MOVWconst [d]) a)
    // result: (ADDconst [c*d] a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c * d);
        v.AddArg(a);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMULD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULD (NEGD x) y)
    // cond: buildcfg.GOARM >= 6
    // result: (NMULD x y)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMNEGD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_0.Args[0];
                var y = v_1;
                if (!(buildcfg.GOARM >= 6)) {
                    continue;
                }

                v.reset(OpARMNMULD);
                v.AddArg2(x, y);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMULF(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MULF (NEGF x) y)
    // cond: buildcfg.GOARM >= 6
    // result: (NMULF x y)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMNEGF) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_0.Args[0];
                var y = v_1;
                if (!(buildcfg.GOARM >= 6)) {
                    continue;
                }

                v.reset(OpARMNMULF);
                v.AddArg2(x, y);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMULS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: c == -1
    // result: (ADD a x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        var a = v_2;
        if (!(c == -1)) {
            break;
        }
        v.reset(OpARMADD);
        v.AddArg2(a, x);
        return true;

    } 
    // match: (MULS _ (MOVWconst [0]) a)
    // result: a
    while (true) {
        if (v_1.Op != OpARMMOVWconst || auxIntToInt32(v_1.AuxInt) != 0) {
            break;
        }
        a = v_2;
        v.copyOf(a);
        return true;

    } 
    // match: (MULS x (MOVWconst [1]) a)
    // result: (RSB x a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst || auxIntToInt32(v_1.AuxInt) != 1) {
            break;
        }
        a = v_2;
        v.reset(OpARMRSB);
        v.AddArg2(x, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: isPowerOfTwo32(c)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c))] x) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpARMRSB);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c)));
        v0.AddArg(x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: isPowerOfTwo32(c-1) && c >= 3
    // result: (RSB (ADDshiftLL <x.Type> x x [int32(log32(c-1))]) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(isPowerOfTwo32(c - 1) && c >= 3)) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: isPowerOfTwo32(c+1) && c >= 7
    // result: (RSB (RSBshiftLL <x.Type> x x [int32(log32(c+1))]) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(isPowerOfTwo32(c + 1) && c >= 7)) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c + 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: c%3 == 0 && isPowerOfTwo32(c/3)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/3))] (ADDshiftLL <x.Type> x x [1])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 3 == 0 && isPowerOfTwo32(c / 3))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 3)));
        var v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(1);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: c%5 == 0 && isPowerOfTwo32(c/5)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/5))] (ADDshiftLL <x.Type> x x [2])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 5 == 0 && isPowerOfTwo32(c / 5))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 5)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(2);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: c%7 == 0 && isPowerOfTwo32(c/7)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/7))] (RSBshiftLL <x.Type> x x [3])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 7 == 0 && isPowerOfTwo32(c / 7))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 7)));
        v1 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS x (MOVWconst [c]) a)
    // cond: c%9 == 0 && isPowerOfTwo32(c/9)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/9))] (ADDshiftLL <x.Type> x x [3])) a)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        if (!(c % 9 == 0 && isPowerOfTwo32(c / 9))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 9)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: c == -1
    // result: (ADD a x)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c == -1)) {
            break;
        }
        v.reset(OpARMADD);
        v.AddArg2(a, x);
        return true;

    } 
    // match: (MULS (MOVWconst [0]) _ a)
    // result: a
    while (true) {
        if (v_0.Op != OpARMMOVWconst || auxIntToInt32(v_0.AuxInt) != 0) {
            break;
        }
        a = v_2;
        v.copyOf(a);
        return true;

    } 
    // match: (MULS (MOVWconst [1]) x a)
    // result: (RSB x a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst || auxIntToInt32(v_0.AuxInt) != 1) {
            break;
        }
        x = v_1;
        a = v_2;
        v.reset(OpARMRSB);
        v.AddArg2(x, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: isPowerOfTwo32(c)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c))] x) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c)));
        v0.AddArg(x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: isPowerOfTwo32(c-1) && c >= 3
    // result: (RSB (ADDshiftLL <x.Type> x x [int32(log32(c-1))]) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(isPowerOfTwo32(c - 1) && c >= 3)) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c - 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: isPowerOfTwo32(c+1) && c >= 7
    // result: (RSB (RSBshiftLL <x.Type> x x [int32(log32(c+1))]) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(isPowerOfTwo32(c + 1) && c >= 7)) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c + 1)));
        v0.AddArg2(x, x);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: c%3 == 0 && isPowerOfTwo32(c/3)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/3))] (ADDshiftLL <x.Type> x x [1])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 3 == 0 && isPowerOfTwo32(c / 3))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 3)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(1);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: c%5 == 0 && isPowerOfTwo32(c/5)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/5))] (ADDshiftLL <x.Type> x x [2])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 5 == 0 && isPowerOfTwo32(c / 5))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 5)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(2);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: c%7 == 0 && isPowerOfTwo32(c/7)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/7))] (RSBshiftLL <x.Type> x x [3])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 7 == 0 && isPowerOfTwo32(c / 7))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 7)));
        v1 = b.NewValue0(v.Pos, OpARMRSBshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) x a)
    // cond: c%9 == 0 && isPowerOfTwo32(c/9)
    // result: (RSB (SLLconst <x.Type> [int32(log32(c/9))] (ADDshiftLL <x.Type> x x [3])) a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_1;
        a = v_2;
        if (!(c % 9 == 0 && isPowerOfTwo32(c / 9))) {
            break;
        }
        v.reset(OpARMRSB);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(int32(log32(c / 9)));
        v1 = b.NewValue0(v.Pos, OpARMADDshiftLL, x.Type);
        v1.AuxInt = int32ToAuxInt(3);
        v1.AddArg2(x, x);
        v0.AddArg(v1);
        v.AddArg2(v0, a);
        return true;

    } 
    // match: (MULS (MOVWconst [c]) (MOVWconst [d]) a)
    // result: (SUBconst [c*d] a)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_1.AuxInt);
        a = v_2;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c * d);
        v.AddArg(a);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVN(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MVN (MOVWconst [c]))
    // result: (MOVWconst [^c])
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(~c);
        return true;

    } 
    // match: (MVN (SLLconst [c] x))
    // result: (MVNshiftLL x [c])
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpARMMVNshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (MVN (SRLconst [c] x))
    // result: (MVNshiftRL x [c])
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMMVNshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (MVN (SRAconst [c] x))
    // result: (MVNshiftRA x [c])
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMMVNshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (MVN (SLL x y))
    // result: (MVNshiftLLreg x y)
    while (true) {
        if (v_0.Op != OpARMSLL) {
            break;
        }
        var y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpARMMVNshiftLLreg);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (MVN (SRL x y))
    // result: (MVNshiftRLreg x y)
    while (true) {
        if (v_0.Op != OpARMSRL) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpARMMVNshiftRLreg);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (MVN (SRA x y))
    // result: (MVNshiftRAreg x y)
    while (true) {
        if (v_0.Op != OpARMSRA) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        v.reset(OpARMMVNshiftRAreg);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVNshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MVNshiftLL (MOVWconst [c]) [d])
    // result: (MOVWconst [^(c<<uint64(d))])
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(~(c << (int)(uint64(d))));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVNshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MVNshiftLLreg x (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (MVNshiftLL x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMMVNshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVNshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MVNshiftRA (MOVWconst [c]) [d])
    // result: (MOVWconst [int32(c)>>uint64(d)])
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(c) >> (int)(uint64(d)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVNshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MVNshiftRAreg x (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (MVNshiftRA x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMMVNshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVNshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (MVNshiftRL (MOVWconst [c]) [d])
    // result: (MOVWconst [^int32(uint32(c)>>uint64(d))])
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(~int32(uint32(c) >> (int)(uint64(d))));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMMVNshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (MVNshiftRLreg x (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (MVNshiftRL x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMMVNshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMNEGD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEGD (MULD x y))
    // cond: buildcfg.GOARM >= 6
    // result: (NMULD x y)
    while (true) {
        if (v_0.Op != OpARMMULD) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        if (!(buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMNMULD);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMNEGF(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NEGF (MULF x y))
    // cond: buildcfg.GOARM >= 6
    // result: (NMULF x y)
    while (true) {
        if (v_0.Op != OpARMMULF) {
            break;
        }
        var y = v_0.Args[1];
        var x = v_0.Args[0];
        if (!(buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMNMULF);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMNMULD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (NMULD (NEGD x) y)
    // result: (MULD x y)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMNEGD) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_0.Args[0];
                var y = v_1;
                v.reset(OpARMMULD);
                v.AddArg2(x, y);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMNMULF(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (NMULF (NEGF x) y)
    // result: (MULF x y)
    while (true) {
        {
            nint _i0 = 0;

            while (_i0 <= 1) {
                if (v_0.Op != OpARMNEGF) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var x = v_0.Args[0];
                var y = v_1;
                v.reset(OpARMMULF);
                v.AddArg2(x, y);
                return true;

            }

        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMNotEqual(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (NotEqual (FlagConstant [fc]))
    // result: (MOVWconst [b2i32(fc.ne())])
    while (true) {
        if (v_0.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(fc.ne()));
        return true;

    } 
    // match: (NotEqual (InvertFlags x))
    // result: (NotEqual x)
    while (true) {
        if (v_0.Op != OpARMInvertFlags) {
            break;
        }
        var x = v_0.Args[0];
        v.reset(OpARMNotEqual);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (OR x (MOVWconst [c]))
    // result: (ORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMORconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x (SLLconst [c] y))
    // result: (ORshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMORshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x (SRLconst [c] y))
    // result: (ORshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMORshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x (SRAconst [c] y))
    // result: (ORshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMORshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x (SLL y z))
    // result: (ORshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMORshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x (SRL y z))
    // result: (ORshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMORshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x (SRA y z))
    // result: (ORshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMORshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (OR x x)
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
private static bool rewriteValueARM_OpARMORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (ORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (ORconst [c] _)
    // cond: int32(c)==-1
    // result: (MOVWconst [-1])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (!(int32(c) == -1)) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(-1);
        return true;

    } 
    // match: (ORconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c|d])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c | d);
        return true;

    } 
    // match: (ORconst [c] (ORconst [d] x))
    // result: (ORconst [c|d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMORconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c | d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMORshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (ORshiftLL (MOVWconst [c]) x [d])
    // result: (ORconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ORshiftLL x (MOVWconst [c]) [d])
    // result: (ORconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: ( ORshiftLL [c] (SRLconst x [32-c]) x)
    // result: (SRRconst [32-c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != 32 - c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(32 - c);
        v.AddArg(x);
        return true;

    } 
    // match: (ORshiftLL <typ.UInt16> [8] (BFXU <typ.UInt16> [int32(armBFAuxInt(8, 8))] x) x)
    // result: (REV16 x)
    while (true) {
        if (v.Type != typ.UInt16 || auxIntToInt32(v.AuxInt) != 8 || v_0.Op != OpARMBFXU || v_0.Type != typ.UInt16 || auxIntToInt32(v_0.AuxInt) != int32(armBFAuxInt(8, 8))) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMREV16);
        v.AddArg(x);
        return true;

    } 
    // match: (ORshiftLL <typ.UInt16> [8] (SRLconst <typ.UInt16> [24] (SLLconst [16] x)) x)
    // cond: buildcfg.GOARM>=6
    // result: (REV16 x)
    while (true) {
        if (v.Type != typ.UInt16 || auxIntToInt32(v.AuxInt) != 8 || v_0.Op != OpARMSRLconst || v_0.Type != typ.UInt16 || auxIntToInt32(v_0.AuxInt) != 24) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpARMSLLconst || auxIntToInt32(v_0_0.AuxInt) != 16) {
            break;
        }
        x = v_0_0.Args[0];
        if (x != v_1 || !(buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMREV16);
        v.AddArg(x);
        return true;

    } 
    // match: (ORshiftLL y:(SLLconst x [c]) x [c])
    // result: y
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var y = v_0;
        if (y.Op != OpARMSLLconst || auxIntToInt32(y.AuxInt) != c) {
            break;
        }
        x = y.Args[0];
        if (x != v_1) {
            break;
        }
        v.copyOf(y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMORshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ORshiftLLreg (MOVWconst [c]) x y)
    // result: (ORconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ORshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ORshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMORshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMORshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ORshiftRA (MOVWconst [c]) x [d])
    // result: (ORconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ORshiftRA x (MOVWconst [c]) [d])
    // result: (ORconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (ORshiftRA y:(SRAconst x [c]) x [c])
    // result: y
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var y = v_0;
        if (y.Op != OpARMSRAconst || auxIntToInt32(y.AuxInt) != c) {
            break;
        }
        x = y.Args[0];
        if (x != v_1) {
            break;
        }
        v.copyOf(y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMORshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ORshiftRAreg (MOVWconst [c]) x y)
    // result: (ORconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ORshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ORshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMORshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMORshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ORshiftRL (MOVWconst [c]) x [d])
    // result: (ORconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (ORshiftRL x (MOVWconst [c]) [d])
    // result: (ORconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: ( ORshiftRL [c] (SLLconst x [32-c]) x)
    // result: (SRRconst [ c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != 32 - c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (ORshiftRL y:(SRLconst x [c]) x [c])
    // result: y
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        var y = v_0;
        if (y.Op != OpARMSRLconst || auxIntToInt32(y.AuxInt) != c) {
            break;
        }
        x = y.Args[0];
        if (x != v_1) {
            break;
        }
        v.copyOf(y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMORshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (ORshiftRLreg (MOVWconst [c]) x y)
    // result: (ORconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (ORshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (ORshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMORshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RSB (MOVWconst [c]) x)
    // result: (SUBconst [c] x)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (RSB x (MOVWconst [c]))
    // result: (RSBconst [c] x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (RSB x (SLLconst [c] y))
    // result: (RSBshiftLL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var y = v_1.Args[0];
        v.reset(OpARMRSBshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (RSB (SLLconst [c] y) x)
    // result: (SUBshiftLL x y [c])
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMSUBshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (RSB x (SRLconst [c] y))
    // result: (RSBshiftRL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMRSBshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (RSB (SRLconst [c] y) x)
    // result: (SUBshiftRL x y [c])
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMSUBshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (RSB x (SRAconst [c] y))
    // result: (RSBshiftRA x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMRSBshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (RSB (SRAconst [c] y) x)
    // result: (SUBshiftRA x y [c])
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMSUBshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (RSB x (SLL y z))
    // result: (RSBshiftLLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLL) {
            break;
        }
        var z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMRSBshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (RSB (SLL y z) x)
    // result: (SUBshiftLLreg x y z)
    while (true) {
        if (v_0.Op != OpARMSLL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMSUBshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (RSB x (SRL y z))
    // result: (RSBshiftRLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRL) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMRSBshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (RSB (SRL y z) x)
    // result: (SUBshiftRLreg x y z)
    while (true) {
        if (v_0.Op != OpARMSRL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMSUBshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (RSB x (SRA y z))
    // result: (RSBshiftRAreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRA) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMRSBshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (RSB (SRA y z) x)
    // result: (SUBshiftRAreg x y z)
    while (true) {
        if (v_0.Op != OpARMSRA) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMSUBshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (RSB x x)
    // result: (MOVWconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (RSB (MUL x y) a)
    // cond: buildcfg.GOARM == 7
    // result: (MULS x y a)
    while (true) {
        if (v_0.Op != OpARMMUL) {
            break;
        }
        y = v_0.Args[1];
        x = v_0.Args[0];
        var a = v_1;
        if (!(buildcfg.GOARM == 7)) {
            break;
        }
        v.reset(OpARMMULS);
        v.AddArg3(x, y, a);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBSshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBSshiftLL (MOVWconst [c]) x [d])
    // result: (SUBSconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBSshiftLL x (MOVWconst [c]) [d])
    // result: (RSBSconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBSshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBSshiftLLreg (MOVWconst [c]) x y)
    // result: (SUBSconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBSshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (RSBSshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSBSshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBSshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBSshiftRA (MOVWconst [c]) x [d])
    // result: (SUBSconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBSshiftRA x (MOVWconst [c]) [d])
    // result: (RSBSconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBSshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBSshiftRAreg (MOVWconst [c]) x y)
    // result: (SUBSconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBSshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (RSBSshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSBSshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBSshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBSshiftRL (MOVWconst [c]) x [d])
    // result: (SUBSconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBSshiftRL x (MOVWconst [c]) [d])
    // result: (RSBSconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBSshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBSshiftRLreg (MOVWconst [c]) x y)
    // result: (SUBSconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBSshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (RSBSshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSBSshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (RSBconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c-d])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c - d);
        return true;

    } 
    // match: (RSBconst [c] (RSBconst [d] x))
    // result: (ADDconst [c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMRSBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg(x);
        return true;

    } 
    // match: (RSBconst [c] (ADDconst [d] x))
    // result: (RSBconst [c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg(x);
        return true;

    } 
    // match: (RSBconst [c] (SUBconst [d] x))
    // result: (RSBconst [c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBshiftLL (MOVWconst [c]) x [d])
    // result: (SUBconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBshiftLL x (MOVWconst [c]) [d])
    // result: (RSBconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (RSBshiftLL (SLLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBshiftLLreg (MOVWconst [c]) x y)
    // result: (SUBconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (RSBshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSBshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBshiftRA (MOVWconst [c]) x [d])
    // result: (SUBconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBshiftRA x (MOVWconst [c]) [d])
    // result: (RSBconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (RSBshiftRA (SRAconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRAconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBshiftRAreg (MOVWconst [c]) x y)
    // result: (SUBconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (RSBshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSBshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBshiftRL (MOVWconst [c]) x [d])
    // result: (SUBconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBshiftRL x (MOVWconst [c]) [d])
    // result: (RSBconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: (RSBshiftRL (SRLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSBshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSBshiftRLreg (MOVWconst [c]) x y)
    // result: (SUBconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (RSBshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (RSBshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSBshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (RSCconst [c] (ADDconst [d] x) flags)
    // result: (RSCconst [c-d] x flags)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var flags = v_1;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg2(x, flags);
        return true;

    } 
    // match: (RSCconst [c] (SUBconst [d] x) flags)
    // result: (RSCconst [c+d] x flags)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        flags = v_1;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSCshiftLL (MOVWconst [c]) x [d] flags)
    // result: (SBCconst [c] (SLLconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (RSCshiftLL x (MOVWconst [c]) [d] flags)
    // result: (RSCconst x [c<<uint64(d)] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSCshiftLLreg (MOVWconst [c]) x y flags)
    // result: (SBCconst [c] (SLL <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (RSCshiftLLreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (RSCshiftLL x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSCshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSCshiftRA (MOVWconst [c]) x [d] flags)
    // result: (SBCconst [c] (SRAconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (RSCshiftRA x (MOVWconst [c]) [d] flags)
    // result: (RSCconst x [c>>uint64(d)] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSCshiftRAreg (MOVWconst [c]) x y flags)
    // result: (SBCconst [c] (SRA <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (RSCshiftRAreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (RSCshiftRA x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSCshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSCshiftRL (MOVWconst [c]) x [d] flags)
    // result: (SBCconst [c] (SRLconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (RSCshiftRL x (MOVWconst [c]) [d] flags)
    // result: (RSCconst x [int32(uint32(c)>>uint64(d))] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMRSCshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RSCshiftRLreg (MOVWconst [c]) x y flags)
    // result: (SBCconst [c] (SRL <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (RSCshiftRLreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (RSCshiftRL x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMRSCshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBC(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SBC (MOVWconst [c]) x flags)
    // result: (RSCconst [c] x flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, flags);
        return true;

    } 
    // match: (SBC x (MOVWconst [c]) flags)
    // result: (SBCconst [c] x flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, flags);
        return true;

    } 
    // match: (SBC x (SLLconst [c] y) flags)
    // result: (SBCshiftLL x y [c] flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var y = v_1.Args[0];
        flags = v_2;
        v.reset(OpARMSBCshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    } 
    // match: (SBC (SLLconst [c] y) x flags)
    // result: (RSCshiftLL x y [c] flags)
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        flags = v_2;
        v.reset(OpARMRSCshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    } 
    // match: (SBC x (SRLconst [c] y) flags)
    // result: (SBCshiftRL x y [c] flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        flags = v_2;
        v.reset(OpARMSBCshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    } 
    // match: (SBC (SRLconst [c] y) x flags)
    // result: (RSCshiftRL x y [c] flags)
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        flags = v_2;
        v.reset(OpARMRSCshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    } 
    // match: (SBC x (SRAconst [c] y) flags)
    // result: (SBCshiftRA x y [c] flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        flags = v_2;
        v.reset(OpARMSBCshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    } 
    // match: (SBC (SRAconst [c] y) x flags)
    // result: (RSCshiftRA x y [c] flags)
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        flags = v_2;
        v.reset(OpARMRSCshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    } 
    // match: (SBC x (SLL y z) flags)
    // result: (SBCshiftLLreg x y z flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLL) {
            break;
        }
        var z = v_1.Args[1];
        y = v_1.Args[0];
        flags = v_2;
        v.reset(OpARMSBCshiftLLreg);
        v.AddArg4(x, y, z, flags);
        return true;

    } 
    // match: (SBC (SLL y z) x flags)
    // result: (RSCshiftLLreg x y z flags)
    while (true) {
        if (v_0.Op != OpARMSLL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        flags = v_2;
        v.reset(OpARMRSCshiftLLreg);
        v.AddArg4(x, y, z, flags);
        return true;

    } 
    // match: (SBC x (SRL y z) flags)
    // result: (SBCshiftRLreg x y z flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRL) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        flags = v_2;
        v.reset(OpARMSBCshiftRLreg);
        v.AddArg4(x, y, z, flags);
        return true;

    } 
    // match: (SBC (SRL y z) x flags)
    // result: (RSCshiftRLreg x y z flags)
    while (true) {
        if (v_0.Op != OpARMSRL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        flags = v_2;
        v.reset(OpARMRSCshiftRLreg);
        v.AddArg4(x, y, z, flags);
        return true;

    } 
    // match: (SBC x (SRA y z) flags)
    // result: (SBCshiftRAreg x y z flags)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRA) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        flags = v_2;
        v.reset(OpARMSBCshiftRAreg);
        v.AddArg4(x, y, z, flags);
        return true;

    } 
    // match: (SBC (SRA y z) x flags)
    // result: (RSCshiftRAreg x y z flags)
    while (true) {
        if (v_0.Op != OpARMSRA) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        flags = v_2;
        v.reset(OpARMRSCshiftRAreg);
        v.AddArg4(x, y, z, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SBCconst [c] (ADDconst [d] x) flags)
    // result: (SBCconst [c-d] x flags)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        var flags = v_1;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c - d);
        v.AddArg2(x, flags);
        return true;

    } 
    // match: (SBCconst [c] (SUBconst [d] x) flags)
    // result: (SBCconst [c+d] x flags)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        flags = v_1;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c + d);
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SBCshiftLL (MOVWconst [c]) x [d] flags)
    // result: (RSCconst [c] (SLLconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (SBCshiftLL x (MOVWconst [c]) [d] flags)
    // result: (SBCconst x [c<<uint64(d)] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SBCshiftLLreg (MOVWconst [c]) x y flags)
    // result: (RSCconst [c] (SLL <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (SBCshiftLLreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (SBCshiftLL x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSBCshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SBCshiftRA (MOVWconst [c]) x [d] flags)
    // result: (RSCconst [c] (SRAconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (SBCshiftRA x (MOVWconst [c]) [d] flags)
    // result: (SBCconst x [c>>uint64(d)] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SBCshiftRAreg (MOVWconst [c]) x y flags)
    // result: (RSCconst [c] (SRA <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (SBCshiftRAreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (SBCshiftRA x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSBCshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SBCshiftRL (MOVWconst [c]) x [d] flags)
    // result: (RSCconst [c] (SRLconst <x.Type> x [d]) flags)
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var flags = v_2;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (SBCshiftRL x (MOVWconst [c]) [d] flags)
    // result: (SBCconst x [int32(uint32(c)>>uint64(d))] flags)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        flags = v_2;
        v.reset(OpARMSBCconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg2(x, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSBCshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_3 = v.Args[3];
    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SBCshiftRLreg (MOVWconst [c]) x y flags)
    // result: (RSCconst [c] (SRL <x.Type> x y) flags)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        var flags = v_3;
        v.reset(OpARMRSCconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg2(v0, flags);
        return true;

    } 
    // match: (SBCshiftRLreg x y (MOVWconst [c]) flags)
    // cond: 0 <= c && c < 32
    // result: (SBCshiftRL x y [c] flags)
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        flags = v_3;
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSBCshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg3(x, y, flags);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SLL x (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SLLconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSLLconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSLLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SLLconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d<<uint64(c)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(d << (int)(uint64(c)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRA x (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SRAconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSRAcond(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRAcond x _ (FlagConstant [fc]))
    // cond: fc.uge()
    // result: (SRAconst x [31])
    while (true) {
        var x = v_0;
        if (v_2.Op != OpARMFlagConstant) {
            break;
        }
        var fc = auxIntToFlagConstant(v_2.AuxInt);
        if (!(fc.uge())) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;

    } 
    // match: (SRAcond x y (FlagConstant [fc]))
    // cond: fc.ult()
    // result: (SRA x y)
    while (true) {
        x = v_0;
        var y = v_1;
        if (v_2.Op != OpARMFlagConstant) {
            break;
        }
        fc = auxIntToFlagConstant(v_2.AuxInt);
        if (!(fc.ult())) {
            break;
        }
        v.reset(OpARMSRA);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSRAconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRAconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d>>uint64(c)])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(d >> (int)(uint64(c)));
        return true;

    } 
    // match: (SRAconst (SLLconst x [c]) [d])
    // cond: buildcfg.GOARM==7 && uint64(d)>=uint64(c) && uint64(d)<=31
    // result: (BFX [(d-c)|(32-d)<<8] x)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(buildcfg.GOARM == 7 && uint64(d) >= uint64(c) && uint64(d) <= 31)) {
            break;
        }
        v.reset(OpARMBFX);
        v.AuxInt = int32ToAuxInt((d - c) | (32 - d) << 8);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SRL x (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SRLconst x [c])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSRLconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSRLconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SRLconst [c] (MOVWconst [d]))
    // result: (MOVWconst [int32(uint32(d)>>uint64(c))])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(d) >> (int)(uint64(c))));
        return true;

    } 
    // match: (SRLconst (SLLconst x [c]) [d])
    // cond: buildcfg.GOARM==7 && uint64(d)>=uint64(c) && uint64(d)<=31
    // result: (BFXU [(d-c)|(32-d)<<8] x)
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        var x = v_0.Args[0];
        if (!(buildcfg.GOARM == 7 && uint64(d) >= uint64(c) && uint64(d) <= 31)) {
            break;
        }
        v.reset(OpARMBFXU);
        v.AuxInt = int32ToAuxInt((d - c) | (32 - d) << 8);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUB (MOVWconst [c]) x)
    // result: (RSBconst [c] x)
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (SUB x (MOVWconst [c]))
    // result: (SUBconst [c] x)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (SUB x (SLLconst [c] y))
    // result: (SUBshiftLL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var y = v_1.Args[0];
        v.reset(OpARMSUBshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUB (SLLconst [c] y) x)
    // result: (RSBshiftLL x y [c])
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUB x (SRLconst [c] y))
    // result: (SUBshiftRL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMSUBshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUB (SRLconst [c] y) x)
    // result: (RSBshiftRL x y [c])
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUB x (SRAconst [c] y))
    // result: (SUBshiftRA x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMSUBshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUB (SRAconst [c] y) x)
    // result: (RSBshiftRA x y [c])
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUB x (SLL y z))
    // result: (SUBshiftLLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLL) {
            break;
        }
        var z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMSUBshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUB (SLL y z) x)
    // result: (RSBshiftLLreg x y z)
    while (true) {
        if (v_0.Op != OpARMSLL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUB x (SRL y z))
    // result: (SUBshiftRLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRL) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMSUBshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUB (SRL y z) x)
    // result: (RSBshiftRLreg x y z)
    while (true) {
        if (v_0.Op != OpARMSRL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUB x (SRA y z))
    // result: (SUBshiftRAreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRA) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMSUBshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUB (SRA y z) x)
    // result: (RSBshiftRAreg x y z)
    while (true) {
        if (v_0.Op != OpARMSRA) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUB x x)
    // result: (MOVWconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (SUB a (MUL x y))
    // cond: buildcfg.GOARM == 7
    // result: (MULS x y a)
    while (true) {
        var a = v_0;
        if (v_1.Op != OpARMMUL) {
            break;
        }
        y = v_1.Args[1];
        x = v_1.Args[0];
        if (!(buildcfg.GOARM == 7)) {
            break;
        }
        v.reset(OpARMMULS);
        v.AddArg3(x, y, a);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBD(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBD a (MULD x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULSD a x y)
    while (true) {
        var a = v_0;
        if (v_1.Op != OpARMMULD) {
            break;
        }
        var y = v_1.Args[1];
        var x = v_1.Args[0];
        if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMMULSD);
        v.AddArg3(a, x, y);
        return true;

    } 
    // match: (SUBD a (NMULD x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULAD a x y)
    while (true) {
        a = v_0;
        if (v_1.Op != OpARMNMULD) {
            break;
        }
        y = v_1.Args[1];
        x = v_1.Args[0];
        if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMMULAD);
        v.AddArg3(a, x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBF(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBF a (MULF x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULSF a x y)
    while (true) {
        var a = v_0;
        if (v_1.Op != OpARMMULF) {
            break;
        }
        var y = v_1.Args[1];
        var x = v_1.Args[0];
        if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMMULSF);
        v.AddArg3(a, x, y);
        return true;

    } 
    // match: (SUBF a (NMULF x y))
    // cond: a.Uses == 1 && buildcfg.GOARM >= 6
    // result: (MULAF a x y)
    while (true) {
        a = v_0;
        if (v_1.Op != OpARMNMULF) {
            break;
        }
        y = v_1.Args[1];
        x = v_1.Args[0];
        if (!(a.Uses == 1 && buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMMULAF);
        v.AddArg3(a, x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBS(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (SUBS x (MOVWconst [c]))
    // result: (SUBSconst [c] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (SUBS x (SLLconst [c] y))
    // result: (SUBSshiftLL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        var y = v_1.Args[0];
        v.reset(OpARMSUBSshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUBS (SLLconst [c] y) x)
    // result: (RSBSshiftLL x y [c])
    while (true) {
        if (v_0.Op != OpARMSLLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBSshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUBS x (SRLconst [c] y))
    // result: (SUBSshiftRL x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMSUBSshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUBS (SRLconst [c] y) x)
    // result: (RSBSshiftRL x y [c])
    while (true) {
        if (v_0.Op != OpARMSRLconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBSshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUBS x (SRAconst [c] y))
    // result: (SUBSshiftRA x y [c])
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        y = v_1.Args[0];
        v.reset(OpARMSUBSshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUBS (SRAconst [c] y) x)
    // result: (RSBSshiftRA x y [c])
    while (true) {
        if (v_0.Op != OpARMSRAconst) {
            break;
        }
        c = auxIntToInt32(v_0.AuxInt);
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBSshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    } 
    // match: (SUBS x (SLL y z))
    // result: (SUBSshiftLLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSLL) {
            break;
        }
        var z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMSUBSshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUBS (SLL y z) x)
    // result: (RSBSshiftLLreg x y z)
    while (true) {
        if (v_0.Op != OpARMSLL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBSshiftLLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUBS x (SRL y z))
    // result: (SUBSshiftRLreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRL) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMSUBSshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUBS (SRL y z) x)
    // result: (RSBSshiftRLreg x y z)
    while (true) {
        if (v_0.Op != OpARMSRL) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBSshiftRLreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUBS x (SRA y z))
    // result: (SUBSshiftRAreg x y z)
    while (true) {
        x = v_0;
        if (v_1.Op != OpARMSRA) {
            break;
        }
        z = v_1.Args[1];
        y = v_1.Args[0];
        v.reset(OpARMSUBSshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    } 
    // match: (SUBS (SRA y z) x)
    // result: (RSBSshiftRAreg x y z)
    while (true) {
        if (v_0.Op != OpARMSRA) {
            break;
        }
        z = v_0.Args[1];
        y = v_0.Args[0];
        x = v_1;
        v.reset(OpARMRSBSshiftRAreg);
        v.AddArg3(x, y, z);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBSshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBSshiftLL (MOVWconst [c]) x [d])
    // result: (RSBSconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBSshiftLL x (MOVWconst [c]) [d])
    // result: (SUBSconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBSshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBSshiftLLreg (MOVWconst [c]) x y)
    // result: (RSBSconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBSshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SUBSshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSUBSshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBSshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBSshiftRA (MOVWconst [c]) x [d])
    // result: (RSBSconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBSshiftRA x (MOVWconst [c]) [d])
    // result: (SUBSconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBSshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBSshiftRAreg (MOVWconst [c]) x y)
    // result: (RSBSconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBSshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SUBSshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSUBSshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBSshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBSshiftRL (MOVWconst [c]) x [d])
    // result: (RSBSconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBSshiftRL x (MOVWconst [c]) [d])
    // result: (SUBSconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBSconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBSshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBSshiftRLreg (MOVWconst [c]) x y)
    // result: (RSBSconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMRSBSconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBSshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SUBSshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSUBSshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (SUBconst [off1] (MOVWaddr [off2] {sym} ptr))
    // result: (MOVWaddr [off2-off1] {sym} ptr)
    while (true) {
        var off1 = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWaddr) {
            break;
        }
        var off2 = auxIntToInt32(v_0.AuxInt);
        var sym = auxToSym(v_0.Aux);
        var ptr = v_0.Args[0];
        v.reset(OpARMMOVWaddr);
        v.AuxInt = int32ToAuxInt(off2 - off1);
        v.Aux = symToAux(sym);
        v.AddArg(ptr);
        return true;

    } 
    // match: (SUBconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (SUBconst [c] x)
    // cond: !isARMImmRot(uint32(c)) && isARMImmRot(uint32(-c))
    // result: (ADDconst [-c] x)
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(!isARMImmRot(uint32(c)) && isARMImmRot(uint32(-c)))) {
            break;
        }
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;

    } 
    // match: (SUBconst [c] x)
    // cond: buildcfg.GOARM==7 && !isARMImmRot(uint32(c)) && uint32(c)>0xffff && uint32(-c)<=0xffff
    // result: (ADDconst [-c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (!(buildcfg.GOARM == 7 && !isARMImmRot(uint32(c)) && uint32(c) > 0xffff && uint32(-c) <= 0xffff)) {
            break;
        }
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(-c);
        v.AddArg(x);
        return true;

    } 
    // match: (SUBconst [c] (MOVWconst [d]))
    // result: (MOVWconst [d-c])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(d - c);
        return true;

    } 
    // match: (SUBconst [c] (SUBconst [d] x))
    // result: (ADDconst [-c-d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSUBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(-c - d);
        v.AddArg(x);
        return true;

    } 
    // match: (SUBconst [c] (ADDconst [d] x))
    // result: (ADDconst [-c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMADDconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(-c + d);
        v.AddArg(x);
        return true;

    } 
    // match: (SUBconst [c] (RSBconst [d] x))
    // result: (RSBconst [-c+d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMRSBconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(-c + d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBshiftLL (MOVWconst [c]) x [d])
    // result: (RSBconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBshiftLL x (MOVWconst [c]) [d])
    // result: (SUBconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (SUBshiftLL (SLLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBshiftLLreg (MOVWconst [c]) x y)
    // result: (RSBconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SUBshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSUBshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBshiftRA (MOVWconst [c]) x [d])
    // result: (RSBconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBshiftRA x (MOVWconst [c]) [d])
    // result: (SUBconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (SUBshiftRA (SRAconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRAconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBshiftRAreg (MOVWconst [c]) x y)
    // result: (RSBconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SUBshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSUBshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBshiftRL (MOVWconst [c]) x [d])
    // result: (RSBconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBshiftRL x (MOVWconst [c]) [d])
    // result: (SUBconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSUBconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: (SUBshiftRL (SRLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMSUBshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (SUBshiftRLreg (MOVWconst [c]) x y)
    // result: (RSBconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (SUBshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (SUBshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMSUBshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQ(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (TEQ x (MOVWconst [c]))
    // result: (TEQconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMTEQconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TEQ x (SLLconst [c] y))
    // result: (TEQshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMTEQshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TEQ x (SRLconst [c] y))
    // result: (TEQshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMTEQshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TEQ x (SRAconst [c] y))
    // result: (TEQshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMTEQshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TEQ x (SLL y z))
    // result: (TEQshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMTEQshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TEQ x (SRL y z))
    // result: (TEQshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMTEQshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TEQ x (SRA y z))
    // result: (TEQshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMTEQshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (TEQconst (MOVWconst [x]) [y])
    // result: (FlagConstant [logicFlags32(x^y)])
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(logicFlags32(x ^ y));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TEQshiftLL (MOVWconst [c]) x [d])
    // result: (TEQconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (TEQshiftLL x (MOVWconst [c]) [d])
    // result: (TEQconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TEQshiftLLreg (MOVWconst [c]) x y)
    // result: (TEQconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (TEQshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (TEQshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMTEQshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TEQshiftRA (MOVWconst [c]) x [d])
    // result: (TEQconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (TEQshiftRA x (MOVWconst [c]) [d])
    // result: (TEQconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TEQshiftRAreg (MOVWconst [c]) x y)
    // result: (TEQconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (TEQshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (TEQshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMTEQshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TEQshiftRL (MOVWconst [c]) x [d])
    // result: (TEQconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (TEQshiftRL x (MOVWconst [c]) [d])
    // result: (TEQconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTEQshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TEQshiftRLreg (MOVWconst [c]) x y)
    // result: (TEQconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMTEQconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (TEQshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (TEQshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMTEQshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTST(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (TST x (MOVWconst [c]))
    // result: (TSTconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMTSTconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TST x (SLLconst [c] y))
    // result: (TSTshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMTSTshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TST x (SRLconst [c] y))
    // result: (TSTshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMTSTshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TST x (SRAconst [c] y))
    // result: (TSTshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMTSTshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TST x (SLL y z))
    // result: (TSTshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMTSTshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TST x (SRL y z))
    // result: (TSTshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMTSTshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (TST x (SRA y z))
    // result: (TSTshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMTSTshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (TSTconst (MOVWconst [x]) [y])
    // result: (FlagConstant [logicFlags32(x&y)])
    while (true) {
        var y = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var x = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMFlagConstant);
        v.AuxInt = flagConstantToAuxInt(logicFlags32(x & y));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TSTshiftLL (MOVWconst [c]) x [d])
    // result: (TSTconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (TSTshiftLL x (MOVWconst [c]) [d])
    // result: (TSTconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TSTshiftLLreg (MOVWconst [c]) x y)
    // result: (TSTconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (TSTshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (TSTshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMTSTshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TSTshiftRA (MOVWconst [c]) x [d])
    // result: (TSTconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (TSTshiftRA x (MOVWconst [c]) [d])
    // result: (TSTconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TSTshiftRAreg (MOVWconst [c]) x y)
    // result: (TSTconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (TSTshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (TSTshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMTSTshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TSTshiftRL (MOVWconst [c]) x [d])
    // result: (TSTconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (TSTshiftRL x (MOVWconst [c]) [d])
    // result: (TSTconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMTSTshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (TSTshiftRLreg (MOVWconst [c]) x y)
    // result: (TSTconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMTSTconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (TSTshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (TSTshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMTSTshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXOR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (XOR x (MOVWconst [c]))
    // result: (XORconst [c] x)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            nint _i0 = 0;

            while (_i0 <= 1) {
                var x = v_0;
                if (v_1.Op != OpARMMOVWconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var c = auxIntToInt32(v_1.AuxInt);
                v.reset(OpARMXORconst);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg(x);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SLLconst [c] y))
    // result: (XORshiftLL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                var y = v_1.Args[0];
                v.reset(OpARMXORshiftLL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SRLconst [c] y))
    // result: (XORshiftRL x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRLconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMXORshiftRL);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SRAconst [c] y))
    // result: (XORshiftRA x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRAconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMXORshiftRA);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SRRconst [c] y))
    // result: (XORshiftRR x y [c])
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRRconst) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                c = auxIntToInt32(v_1.AuxInt);
                y = v_1.Args[0];
                v.reset(OpARMXORshiftRR);
                v.AuxInt = int32ToAuxInt(c);
                v.AddArg2(x, y);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SLL y z))
    // result: (XORshiftLLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSLL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                var z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMXORshiftLLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SRL y z))
    // result: (XORshiftRLreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRL) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMXORshiftRLreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x (SRA y z))
    // result: (XORshiftRAreg x y z)
    while (true) {
        {
            nint _i0__prev2 = _i0;

            _i0 = 0;

            while (_i0 <= 1) {
                x = v_0;
                if (v_1.Op != OpARMSRA) {
                    continue;
                (_i0, v_0, v_1) = (_i0 + 1, v_1, v_0);
                }

                z = v_1.Args[1];
                y = v_1.Args[0];
                v.reset(OpARMXORshiftRAreg);
                v.AddArg3(x, y, z);
                return true;

            }


            _i0 = _i0__prev2;
        }
        break;

    } 
    // match: (XOR x x)
    // result: (MOVWconst [0])
    while (true) {
        x = v_0;
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORconst(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (XORconst [0] x)
    // result: x
    while (true) {
        if (auxIntToInt32(v.AuxInt) != 0) {
            break;
        }
        var x = v_0;
        v.copyOf(x);
        return true;

    } 
    // match: (XORconst [c] (MOVWconst [d]))
    // result: (MOVWconst [c^d])
    while (true) {
        var c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(c ^ d);
        return true;

    } 
    // match: (XORconst [c] (XORconst [d] x))
    // result: (XORconst [c^d] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMXORconst) {
            break;
        }
        d = auxIntToInt32(v_0.AuxInt);
        x = v_0.Args[0];
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c ^ d);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftLL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (XORshiftLL (MOVWconst [c]) x [d])
    // result: (XORconst [c] (SLLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftLL x (MOVWconst [c]) [d])
    // result: (XORconst x [c<<uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c << (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftLL [c] (SRLconst x [32-c]) x)
    // result: (SRRconst [32-c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != 32 - c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(32 - c);
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftLL <typ.UInt16> [8] (BFXU <typ.UInt16> [int32(armBFAuxInt(8, 8))] x) x)
    // result: (REV16 x)
    while (true) {
        if (v.Type != typ.UInt16 || auxIntToInt32(v.AuxInt) != 8 || v_0.Op != OpARMBFXU || v_0.Type != typ.UInt16 || auxIntToInt32(v_0.AuxInt) != int32(armBFAuxInt(8, 8))) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMREV16);
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftLL <typ.UInt16> [8] (SRLconst <typ.UInt16> [24] (SLLconst [16] x)) x)
    // cond: buildcfg.GOARM>=6
    // result: (REV16 x)
    while (true) {
        if (v.Type != typ.UInt16 || auxIntToInt32(v.AuxInt) != 8 || v_0.Op != OpARMSRLconst || v_0.Type != typ.UInt16 || auxIntToInt32(v_0.AuxInt) != 24) {
            break;
        }
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpARMSLLconst || auxIntToInt32(v_0_0.AuxInt) != 16) {
            break;
        }
        x = v_0_0.Args[0];
        if (x != v_1 || !(buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMREV16);
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftLL (SLLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftLLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORshiftLLreg (MOVWconst [c]) x y)
    // result: (XORconst [c] (SLL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftLLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (XORshiftLL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMXORshiftLL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftRA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORshiftRA (MOVWconst [c]) x [d])
    // result: (XORconst [c] (SRAconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRAconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftRA x (MOVWconst [c]) [d])
    // result: (XORconst x [c>>uint64(d)])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c >> (int)(uint64(d)));
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftRA (SRAconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRAconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftRAreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORshiftRAreg (MOVWconst [c]) x y)
    // result: (XORconst [c] (SRA <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRA, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftRAreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (XORshiftRA x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMXORshiftRA);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftRL(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORshiftRL (MOVWconst [c]) x [d])
    // result: (XORconst [c] (SRLconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftRL x (MOVWconst [c]) [d])
    // result: (XORconst x [int32(uint32(c)>>uint64(d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d))));
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftRL [c] (SLLconst x [32-c]) x)
    // result: (SRRconst [ c] x)
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSLLconst || auxIntToInt32(v_0.AuxInt) != 32 - c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg(x);
        return true;

    } 
    // match: (XORshiftRL (SRLconst x [c]) x [c])
    // result: (MOVWconst [0])
    while (true) {
        c = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMSRLconst || auxIntToInt32(v_0.AuxInt) != c) {
            break;
        }
        x = v_0.Args[0];
        if (x != v_1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftRLreg(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORshiftRLreg (MOVWconst [c]) x y)
    // result: (XORconst [c] (SRL <x.Type> x y))
    while (true) {
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        var y = v_2;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftRLreg x y (MOVWconst [c]))
    // cond: 0 <= c && c < 32
    // result: (XORshiftRL x y [c])
    while (true) {
        x = v_0;
        y = v_1;
        if (v_2.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_2.AuxInt);
        if (!(0 <= c && c < 32)) {
            break;
        }
        v.reset(OpARMXORshiftRL);
        v.AuxInt = int32ToAuxInt(c);
        v.AddArg2(x, y);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpARMXORshiftRR(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (XORshiftRR (MOVWconst [c]) x [d])
    // result: (XORconst [c] (SRRconst <x.Type> x [d]))
    while (true) {
        var d = auxIntToInt32(v.AuxInt);
        if (v_0.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0.AuxInt);
        var x = v_1;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(c);
        var v0 = b.NewValue0(v.Pos, OpARMSRRconst, x.Type);
        v0.AuxInt = int32ToAuxInt(d);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (XORshiftRR x (MOVWconst [c]) [d])
    // result: (XORconst x [int32(uint32(c)>>uint64(d)|uint32(c)<<uint64(32-d))])
    while (true) {
        d = auxIntToInt32(v.AuxInt);
        x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) >> (int)(uint64(d)) | uint32(c) << (int)(uint64(32 - d))));
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Addr {sym} base)
    // result: (MOVWaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpARMMOVWaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }

}
private static bool rewriteValueARM_OpAvg32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Avg32u <t> x y)
    // result: (ADD (SRLconst <t> (SUB <t> x y) [1]) y)
    while (true) {
        var t = v.Type;
        var x = v_0;
        var y = v_1;
        v.reset(OpARMADD);
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, t);
        v0.AuxInt = int32ToAuxInt(1);
        var v1 = b.NewValue0(v.Pos, OpARMSUB, t);
        v1.AddArg2(x, y);
        v0.AddArg(v1);
        v.AddArg2(v0, y);
        return true;
    }

}
private static bool rewriteValueARM_OpBitLen32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (BitLen32 <t> x)
    // result: (RSBconst [32] (CLZ <t> x))
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpARMCLZ, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpBswap32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Bswap32 <t> x)
    // cond: buildcfg.GOARM==5
    // result: (XOR <t> (SRLconst <t> (BICconst <t> (XOR <t> x (SRRconst <t> [16] x)) [0xff0000]) [8]) (SRRconst <t> x [8]))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(buildcfg.GOARM == 5)) {
            break;
        }
        v.reset(OpARMXOR);
        v.Type = t;
        var v0 = b.NewValue0(v.Pos, OpARMSRLconst, t);
        v0.AuxInt = int32ToAuxInt(8);
        var v1 = b.NewValue0(v.Pos, OpARMBICconst, t);
        v1.AuxInt = int32ToAuxInt(0xff0000);
        var v2 = b.NewValue0(v.Pos, OpARMXOR, t);
        var v3 = b.NewValue0(v.Pos, OpARMSRRconst, t);
        v3.AuxInt = int32ToAuxInt(16);
        v3.AddArg(x);
        v2.AddArg2(x, v3);
        v1.AddArg(v2);
        v0.AddArg(v1);
        var v4 = b.NewValue0(v.Pos, OpARMSRRconst, t);
        v4.AuxInt = int32ToAuxInt(8);
        v4.AddArg(x);
        v.AddArg2(v0, v4);
        return true;

    } 
    // match: (Bswap32 x)
    // cond: buildcfg.GOARM>=6
    // result: (REV x)
    while (true) {
        x = v_0;
        if (!(buildcfg.GOARM >= 6)) {
            break;
        }
        v.reset(OpARMREV);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpConst16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const16 [val])
    // result: (MOVWconst [int32(val)])
    while (true) {
        var val = auxIntToInt16(v.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(val));
        return true;
    }

}
private static bool rewriteValueARM_OpConst32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const32 [val])
    // result: (MOVWconst [int32(val)])
    while (true) {
        var val = auxIntToInt32(v.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(val));
        return true;
    }

}
private static bool rewriteValueARM_OpConst32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const32F [val])
    // result: (MOVFconst [float64(val)])
    while (true) {
        var val = auxIntToFloat32(v.AuxInt);
        v.reset(OpARMMOVFconst);
        v.AuxInt = float64ToAuxInt(float64(val));
        return true;
    }

}
private static bool rewriteValueARM_OpConst64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const64F [val])
    // result: (MOVDconst [float64(val)])
    while (true) {
        var val = auxIntToFloat64(v.AuxInt);
        v.reset(OpARMMOVDconst);
        v.AuxInt = float64ToAuxInt(float64(val));
        return true;
    }

}
private static bool rewriteValueARM_OpConst8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (Const8 [val])
    // result: (MOVWconst [int32(val)])
    while (true) {
        var val = auxIntToInt8(v.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(val));
        return true;
    }

}
private static bool rewriteValueARM_OpConstBool(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstBool [t])
    // result: (MOVWconst [b2i32(t)])
    while (true) {
        var t = auxIntToBool(v.AuxInt);
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(b2i32(t));
        return true;
    }

}
private static bool rewriteValueARM_OpConstNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;
 
    // match: (ConstNil)
    // result: (MOVWconst [0])
    while (true) {
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;
    }

}
private static bool rewriteValueARM_OpCtz16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz16 <t> x)
    // cond: buildcfg.GOARM<=6
    // result: (RSBconst [32] (CLZ <t> (SUBconst <typ.UInt32> (AND <typ.UInt32> (ORconst <typ.UInt32> [0x10000] x) (RSBconst <typ.UInt32> [0] (ORconst <typ.UInt32> [0x10000] x))) [1])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(buildcfg.GOARM <= 6)) {
            break;
        }
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpARMCLZ, t);
        var v1 = b.NewValue0(v.Pos, OpARMSUBconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpARMAND, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpARMORconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0x10000);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpARMRSBconst, typ.UInt32);
        v4.AuxInt = int32ToAuxInt(0);
        v4.AddArg(v3);
        v2.AddArg2(v3, v4);
        v1.AddArg(v2);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Ctz16 <t> x)
    // cond: buildcfg.GOARM==7
    // result: (CLZ <t> (RBIT <typ.UInt32> (ORconst <typ.UInt32> [0x10000] x)))
    while (true) {
        t = v.Type;
        x = v_0;
        if (!(buildcfg.GOARM == 7)) {
            break;
        }
        v.reset(OpARMCLZ);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpARMRBIT, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpARMORconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0x10000);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpCtz32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Ctz32 <t> x)
    // cond: buildcfg.GOARM<=6
    // result: (RSBconst [32] (CLZ <t> (SUBconst <t> (AND <t> x (RSBconst <t> [0] x)) [1])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(buildcfg.GOARM <= 6)) {
            break;
        }
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpARMCLZ, t);
        var v1 = b.NewValue0(v.Pos, OpARMSUBconst, t);
        v1.AuxInt = int32ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpARMAND, t);
        var v3 = b.NewValue0(v.Pos, OpARMRSBconst, t);
        v3.AuxInt = int32ToAuxInt(0);
        v3.AddArg(x);
        v2.AddArg2(x, v3);
        v1.AddArg(v2);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Ctz32 <t> x)
    // cond: buildcfg.GOARM==7
    // result: (CLZ <t> (RBIT <t> x))
    while (true) {
        t = v.Type;
        x = v_0;
        if (!(buildcfg.GOARM == 7)) {
            break;
        }
        v.reset(OpARMCLZ);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpARMRBIT, t);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpCtz8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Ctz8 <t> x)
    // cond: buildcfg.GOARM<=6
    // result: (RSBconst [32] (CLZ <t> (SUBconst <typ.UInt32> (AND <typ.UInt32> (ORconst <typ.UInt32> [0x100] x) (RSBconst <typ.UInt32> [0] (ORconst <typ.UInt32> [0x100] x))) [1])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (!(buildcfg.GOARM <= 6)) {
            break;
        }
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(32);
        var v0 = b.NewValue0(v.Pos, OpARMCLZ, t);
        var v1 = b.NewValue0(v.Pos, OpARMSUBconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(1);
        var v2 = b.NewValue0(v.Pos, OpARMAND, typ.UInt32);
        var v3 = b.NewValue0(v.Pos, OpARMORconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(0x100);
        v3.AddArg(x);
        var v4 = b.NewValue0(v.Pos, OpARMRSBconst, typ.UInt32);
        v4.AuxInt = int32ToAuxInt(0);
        v4.AddArg(v3);
        v2.AddArg2(v3, v4);
        v1.AddArg(v2);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    } 
    // match: (Ctz8 <t> x)
    // cond: buildcfg.GOARM==7
    // result: (CLZ <t> (RBIT <typ.UInt32> (ORconst <typ.UInt32> [0x100] x)))
    while (true) {
        t = v.Type;
        x = v_0;
        if (!(buildcfg.GOARM == 7)) {
            break;
        }
        v.reset(OpARMCLZ);
        v.Type = t;
        v0 = b.NewValue0(v.Pos, OpARMRBIT, typ.UInt32);
        v1 = b.NewValue0(v.Pos, OpARMORconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0x100);
        v1.AddArg(x);
        v0.AddArg(v1);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpDiv16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16 x y)
    // result: (Div32 (SignExt16to32 x) (SignExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpDiv32);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpDiv16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div16u x y)
    // result: (Div32u (ZeroExt16to32 x) (ZeroExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpDiv32u);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpDiv32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32 x y)
    // result: (SUB (XOR <typ.UInt32> (Select0 <typ.UInt32> (CALLudiv (SUB <typ.UInt32> (XOR x <typ.UInt32> (Signmask x)) (Signmask x)) (SUB <typ.UInt32> (XOR y <typ.UInt32> (Signmask y)) (Signmask y)))) (Signmask (XOR <typ.UInt32> x y))) (Signmask (XOR <typ.UInt32> x y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSUB);
        var v0 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpSelect0, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpARMCALLudiv, types.NewTuple(typ.UInt32, typ.UInt32));
        var v3 = b.NewValue0(v.Pos, OpARMSUB, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        v5.AddArg(x);
        v4.AddArg2(x, v5);
        v3.AddArg2(v4, v5);
        var v6 = b.NewValue0(v.Pos, OpARMSUB, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        var v8 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        v8.AddArg(y);
        v7.AddArg2(y, v8);
        v6.AddArg2(v7, v8);
        v2.AddArg2(v3, v6);
        v1.AddArg(v2);
        var v9 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        var v10 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        v10.AddArg2(x, y);
        v9.AddArg(v10);
        v0.AddArg2(v1, v9);
        v.AddArg2(v0, v9);
        return true;
    }

}
private static bool rewriteValueARM_OpDiv32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div32u x y)
    // result: (Select0 <typ.UInt32> (CALLudiv x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect0);
        v.Type = typ.UInt32;
        var v0 = b.NewValue0(v.Pos, OpARMCALLudiv, types.NewTuple(typ.UInt32, typ.UInt32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpDiv8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8 x y)
    // result: (Div32 (SignExt8to32 x) (SignExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpDiv32);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpDiv8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Div8u x y)
    // result: (Div32u (ZeroExt8to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpDiv32u);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpEq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq16 x y)
    // result: (Equal (CMP (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpEq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32 x y)
    // result: (Equal (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpEq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq32F x y)
    // result: (Equal (CMPF x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPF, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpEq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Eq64F x y)
    // result: (Equal (CMPD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPD, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpEq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Eq8 x y)
    // result: (Equal (CMP (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpEqB(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (EqB x y)
    // result: (XORconst [1] (XOR <typ.Bool> x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(1);
        var v0 = b.NewValue0(v.Pos, OpARMXOR, typ.Bool);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpEqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (EqPtr x y)
    // result: (Equal (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpFMA(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (FMA x y z)
    // result: (FMULAD z x y)
    while (true) {
        var x = v_0;
        var y = v_1;
        var z = v_2;
        v.reset(OpARMFMULAD);
        v.AddArg3(z, x, y);
        return true;
    }

}
private static bool rewriteValueARM_OpIsInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsInBounds idx len)
    // result: (LessThanU (CMP idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpARMLessThanU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpIsNonNil(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsNonNil ptr)
    // result: (NotEqual (CMPconst [0] ptr))
    while (true) {
        var ptr = v_0;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(0);
        v0.AddArg(ptr);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpIsSliceInBounds(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (IsSliceInBounds idx len)
    // result: (LessEqualU (CMP idx len))
    while (true) {
        var idx = v_0;
        var len = v_1;
        v.reset(OpARMLessEqualU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(idx, len);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16 x y)
    // result: (LessEqual (CMP (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq16U x y)
    // result: (LessEqualU (CMP (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessEqualU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32 x y)
    // result: (LessEqual (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32F x y)
    // result: (GreaterEqual (CMPF y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMGreaterEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPF, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq32U x y)
    // result: (LessEqualU (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessEqualU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Leq64F x y)
    // result: (GreaterEqual (CMPD y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMGreaterEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPD, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8 x y)
    // result: (LessEqual (CMP (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLeq8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Leq8U x y)
    // result: (LessEqualU (CMP (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessEqualU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16 x y)
    // result: (LessThan (CMP (SignExt16to32 x) (SignExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessThan);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess16U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less16U x y)
    // result: (LessThanU (CMP (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessThanU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32 x y)
    // result: (LessThan (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessThan);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32F x y)
    // result: (GreaterThan (CMPF y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMGreaterThan);
        var v0 = b.NewValue0(v.Pos, OpARMCMPF, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess32U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less32U x y)
    // result: (LessThanU (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessThanU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Less64F x y)
    // result: (GreaterThan (CMPD y x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMGreaterThan);
        var v0 = b.NewValue0(v.Pos, OpARMCMPD, types.TypeFlags);
        v0.AddArg2(y, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8 x y)
    // result: (LessThan (CMP (SignExt8to32 x) (SignExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessThan);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLess8U(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Less8U x y)
    // result: (LessThanU (CMP (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMLessThanU);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLoad(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Load <t> ptr mem)
    // cond: t.IsBoolean()
    // result: (MOVBUload ptr mem)
    while (true) {
        var t = v.Type;
        var ptr = v_0;
        var mem = v_1;
        if (!(t.IsBoolean())) {
            break;
        }
        v.reset(OpARMMOVBUload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is8BitInt(t) && isSigned(t))
    // result: (MOVBload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpARMMOVBload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is8BitInt(t) && !isSigned(t))
    // result: (MOVBUload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is8BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpARMMOVBUload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is16BitInt(t) && isSigned(t))
    // result: (MOVHload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t) && isSigned(t))) {
            break;
        }
        v.reset(OpARMMOVHload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is16BitInt(t) && !isSigned(t))
    // result: (MOVHUload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is16BitInt(t) && !isSigned(t))) {
            break;
        }
        v.reset(OpARMMOVHUload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: (is32BitInt(t) || isPtr(t))
    // result: (MOVWload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitInt(t) || isPtr(t))) {
            break;
        }
        v.reset(OpARMMOVWload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is32BitFloat(t)
    // result: (MOVFload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is32BitFloat(t))) {
            break;
        }
        v.reset(OpARMMOVFload);
        v.AddArg2(ptr, mem);
        return true;

    } 
    // match: (Load <t> ptr mem)
    // cond: is64BitFloat(t)
    // result: (MOVDload ptr mem)
    while (true) {
        t = v.Type;
        ptr = v_0;
        mem = v_1;
        if (!(is64BitFloat(t))) {
            break;
        }
        v.reset(OpARMMOVDload);
        v.AddArg2(ptr, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpLocalAddr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (LocalAddr {sym} base _)
    // result: (MOVWaddr {sym} base)
    while (true) {
        var sym = auxToSym(v.Aux);
        var @base = v_0;
        v.reset(OpARMMOVWaddr);
        v.Aux = symToAux(sym);
        v.AddArg(base);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x16 x y)
    // result: (CMOVWHSconst (SLL <x.Type> x (ZeroExt16to32 y)) (CMPconst [256] (ZeroExt16to32 y)) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(v1);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh16x32 x y)
    // result: (CMOVWHSconst (SLL <x.Type> x y) (CMPconst [256] y) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh16x64 x (Const64 [c]))
    // cond: uint64(c) < 16
    // result: (SLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(OpARMSLLconst);
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
private static bool rewriteValueARM_OpLsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh16x8 x y)
    // result: (SLL x (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSLL);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x16 x y)
    // result: (CMOVWHSconst (SLL <x.Type> x (ZeroExt16to32 y)) (CMPconst [256] (ZeroExt16to32 y)) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(v1);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh32x32 x y)
    // result: (CMOVWHSconst (SLL <x.Type> x y) (CMPconst [256] y) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh32x64 x (Const64 [c]))
    // cond: uint64(c) < 32
    // result: (SLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(OpARMSLLconst);
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
private static bool rewriteValueARM_OpLsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh32x8 x y)
    // result: (SLL x (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSLL);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x16 x y)
    // result: (CMOVWHSconst (SLL <x.Type> x (ZeroExt16to32 y)) (CMPconst [256] (ZeroExt16to32 y)) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(v1);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Lsh8x32 x y)
    // result: (CMOVWHSconst (SLL <x.Type> x y) (CMPconst [256] y) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSLL, x.Type);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpLsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Lsh8x64 x (Const64 [c]))
    // cond: uint64(c) < 8
    // result: (SLLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(OpARMSLLconst);
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
private static bool rewriteValueARM_OpLsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Lsh8x8 x y)
    // result: (SLL x (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSLL);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpMod16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16 x y)
    // result: (Mod32 (SignExt16to32 x) (SignExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpMod16u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod16u x y)
    // result: (Mod32u (ZeroExt16to32 x) (ZeroExt16to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32u);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpMod32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32 x y)
    // result: (SUB (XOR <typ.UInt32> (Select1 <typ.UInt32> (CALLudiv (SUB <typ.UInt32> (XOR <typ.UInt32> x (Signmask x)) (Signmask x)) (SUB <typ.UInt32> (XOR <typ.UInt32> y (Signmask y)) (Signmask y)))) (Signmask x)) (Signmask x))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSUB);
        var v0 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        var v1 = b.NewValue0(v.Pos, OpSelect1, typ.UInt32);
        var v2 = b.NewValue0(v.Pos, OpARMCALLudiv, types.NewTuple(typ.UInt32, typ.UInt32));
        var v3 = b.NewValue0(v.Pos, OpARMSUB, typ.UInt32);
        var v4 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        var v5 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        v5.AddArg(x);
        v4.AddArg2(x, v5);
        v3.AddArg2(v4, v5);
        var v6 = b.NewValue0(v.Pos, OpARMSUB, typ.UInt32);
        var v7 = b.NewValue0(v.Pos, OpARMXOR, typ.UInt32);
        var v8 = b.NewValue0(v.Pos, OpSignmask, typ.Int32);
        v8.AddArg(y);
        v7.AddArg2(y, v8);
        v6.AddArg2(v7, v8);
        v2.AddArg2(v3, v6);
        v1.AddArg(v2);
        v0.AddArg2(v1, v5);
        v.AddArg2(v0, v5);
        return true;
    }

}
private static bool rewriteValueARM_OpMod32u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod32u x y)
    // result: (Select1 <typ.UInt32> (CALLudiv x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpSelect1);
        v.Type = typ.UInt32;
        var v0 = b.NewValue0(v.Pos, OpARMCALLudiv, types.NewTuple(typ.UInt32, typ.UInt32));
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpMod8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8 x y)
    // result: (Mod32 (SignExt8to32 x) (SignExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpMod8u(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Mod8u x y)
    // result: (Mod32u (ZeroExt8to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpMod32u);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpMove(ptr<Value> _addr_v) {
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
    // result: (MOVBstore dst (MOVBUload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var dst = v_0;
        var src = v_1;
        mem = v_2;
        v.reset(OpARMMOVBstore);
        var v0 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;

    } 
    // match: (Move [2] {t} dst src mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore dst (MOVHUload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        var t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpARMMOVHstore);
        v0 = b.NewValue0(v.Pos, OpARMMOVHUload, typ.UInt16);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;

    } 
    // match: (Move [2] dst src mem)
    // result: (MOVBstore [1] dst (MOVBUload [1] src mem) (MOVBstore dst (MOVBUload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(1);
        v0 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(1);
        v0.AddArg2(src, mem);
        var v1 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        var v2 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [4] {t} dst src mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore dst (MOVWload src mem) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpARMMOVWstore);
        v0 = b.NewValue0(v.Pos, OpARMMOVWload, typ.UInt32);
        v0.AddArg2(src, mem);
        v.AddArg3(dst, v0, mem);
        return true;

    } 
    // match: (Move [4] {t} dst src mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [2] dst (MOVHUload [2] src mem) (MOVHstore dst (MOVHUload src mem) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpARMMOVHUload, typ.UInt16);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpARMMOVHstore, types.TypeMem);
        v2 = b.NewValue0(v.Pos, OpARMMOVHUload, typ.UInt16);
        v2.AddArg2(src, mem);
        v1.AddArg3(dst, v2, mem);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [4] dst src mem)
    // result: (MOVBstore [3] dst (MOVBUload [3] src mem) (MOVBstore [2] dst (MOVBUload [2] src mem) (MOVBstore [1] dst (MOVBUload [1] src mem) (MOVBstore dst (MOVBUload src mem) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(3);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(2);
        v2 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v2.AuxInt = int32ToAuxInt(2);
        v2.AddArg2(src, mem);
        var v3 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(1);
        var v4 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v4.AuxInt = int32ToAuxInt(1);
        v4.AddArg2(src, mem);
        var v5 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        var v6 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v6.AddArg2(src, mem);
        v5.AddArg3(dst, v6, mem);
        v3.AddArg3(dst, v4, v5);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [3] dst src mem)
    // result: (MOVBstore [2] dst (MOVBUload [2] src mem) (MOVBstore [1] dst (MOVBUload [1] src mem) (MOVBstore dst (MOVBUload src mem) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        dst = v_0;
        src = v_1;
        mem = v_2;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v0.AuxInt = int32ToAuxInt(2);
        v0.AddArg2(src, mem);
        v1 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(1);
        v2 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v2.AuxInt = int32ToAuxInt(1);
        v2.AddArg2(src, mem);
        v3 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v4 = b.NewValue0(v.Pos, OpARMMOVBUload, typ.UInt8);
        v4.AddArg2(src, mem);
        v3.AddArg3(dst, v4, mem);
        v1.AddArg3(dst, v2, v3);
        v.AddArg3(dst, v0, v1);
        return true;

    } 
    // match: (Move [s] {t} dst src mem)
    // cond: s%4 == 0 && s > 4 && s <= 512 && t.Alignment()%4 == 0 && !config.noDuffDevice && logLargeCopy(v, s)
    // result: (DUFFCOPY [8 * (128 - s/4)] dst src mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(s % 4 == 0 && s > 4 && s <= 512 && t.Alignment() % 4 == 0 && !config.noDuffDevice && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpARMDUFFCOPY);
        v.AuxInt = int64ToAuxInt(8 * (128 - s / 4));
        v.AddArg3(dst, src, mem);
        return true;

    } 
    // match: (Move [s] {t} dst src mem)
    // cond: ((s > 512 || config.noDuffDevice) || t.Alignment()%4 != 0) && logLargeCopy(v, s)
    // result: (LoweredMove [t.Alignment()] dst src (ADDconst <src.Type> src [int32(s-moveSize(t.Alignment(), config))]) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        dst = v_0;
        src = v_1;
        mem = v_2;
        if (!(((s > 512 || config.noDuffDevice) || t.Alignment() % 4 != 0) && logLargeCopy(v, s))) {
            break;
        }
        v.reset(OpARMLoweredMove);
        v.AuxInt = int64ToAuxInt(t.Alignment());
        v0 = b.NewValue0(v.Pos, OpARMADDconst, src.Type);
        v0.AuxInt = int32ToAuxInt(int32(s - moveSize(t.Alignment(), config)));
        v0.AddArg(src);
        v.AddArg4(dst, src, v0, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpNeg16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Neg16 x)
    // result: (RSBconst [0] x)
    while (true) {
        var x = v_0;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueARM_OpNeg32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Neg32 x)
    // result: (RSBconst [0] x)
    while (true) {
        var x = v_0;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueARM_OpNeg8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Neg8 x)
    // result: (RSBconst [0] x)
    while (true) {
        var x = v_0;
        v.reset(OpARMRSBconst);
        v.AuxInt = int32ToAuxInt(0);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueARM_OpNeq16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq16 x y)
    // result: (NotEqual (CMP (ZeroExt16to32 x) (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpNeq32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32 x y)
    // result: (NotEqual (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpNeq32F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq32F x y)
    // result: (NotEqual (CMPF x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPF, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpNeq64F(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Neq64F x y)
    // result: (NotEqual (CMPD x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMPD, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpNeq8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Neq8 x y)
    // result: (NotEqual (CMP (ZeroExt8to32 x) (ZeroExt8to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpNeqPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (NeqPtr x y)
    // result: (NotEqual (CMP x y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMNotEqual);
        var v0 = b.NewValue0(v.Pos, OpARMCMP, types.TypeFlags);
        v0.AddArg2(x, y);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpNot(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Not x)
    // result: (XORconst [1] x)
    while (true) {
        var x = v_0;
        v.reset(OpARMXORconst);
        v.AuxInt = int32ToAuxInt(1);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueARM_OpOffPtr(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (OffPtr [off] ptr:(SP))
    // result: (MOVWaddr [int32(off)] ptr)
    while (true) {
        var off = auxIntToInt64(v.AuxInt);
        var ptr = v_0;
        if (ptr.Op != OpSP) {
            break;
        }
        v.reset(OpARMMOVWaddr);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;

    } 
    // match: (OffPtr [off] ptr)
    // result: (ADDconst [int32(off)] ptr)
    while (true) {
        off = auxIntToInt64(v.AuxInt);
        ptr = v_0;
        v.reset(OpARMADDconst);
        v.AuxInt = int32ToAuxInt(int32(off));
        v.AddArg(ptr);
        return true;
    }

}
private static bool rewriteValueARM_OpPanicBounds(ptr<Value> _addr_v) {
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
        v.reset(OpARMLoweredPanicBoundsA);
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
        v.reset(OpARMLoweredPanicBoundsB);
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
        v.reset(OpARMLoweredPanicBoundsC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg3(x, y, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpPanicExtend(ptr<Value> _addr_v) {
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
        v.reset(OpARMLoweredPanicExtendA);
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
        v.reset(OpARMLoweredPanicExtendB);
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
        v.reset(OpARMLoweredPanicExtendC);
        v.AuxInt = int64ToAuxInt(kind);
        v.AddArg4(hi, lo, y, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpRotateLeft16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft16 <t> x (MOVWconst [c]))
    // result: (Or16 (Lsh16x32 <t> x (MOVWconst [c&15])) (Rsh16Ux32 <t> x (MOVWconst [-c&15])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOr16);
        var v0 = b.NewValue0(v.Pos, OpLsh16x32, t);
        var v1 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(c & 15);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh16Ux32, t);
        var v3 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(-c & 15);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpRotateLeft32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (RotateLeft32 x (MOVWconst [c]))
    // result: (SRRconst [-c&31] x)
    while (true) {
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpARMSRRconst);
        v.AuxInt = int32ToAuxInt(-c & 31);
        v.AddArg(x);
        return true;

    } 
    // match: (RotateLeft32 x y)
    // result: (SRR x (RSBconst [0] <y.Type> y))
    while (true) {
        x = v_0;
        var y = v_1;
        v.reset(OpARMSRR);
        var v0 = b.NewValue0(v.Pos, OpARMRSBconst, y.Type);
        v0.AuxInt = int32ToAuxInt(0);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpRotateLeft8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (RotateLeft8 <t> x (MOVWconst [c]))
    // result: (Or8 (Lsh8x32 <t> x (MOVWconst [c&7])) (Rsh8Ux32 <t> x (MOVWconst [-c&7])))
    while (true) {
        var t = v.Type;
        var x = v_0;
        if (v_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_1.AuxInt);
        v.reset(OpOr8);
        var v0 = b.NewValue0(v.Pos, OpLsh8x32, t);
        var v1 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(c & 7);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpRsh8Ux32, t);
        var v3 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v3.AuxInt = int32ToAuxInt(-c & 7);
        v2.AddArg2(x, v3);
        v.AddArg2(v0, v2);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpRsh16Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux16 x y)
    // result: (CMOVWHSconst (SRL <x.Type> (ZeroExt16to32 x) (ZeroExt16to32 y)) (CMPconst [256] (ZeroExt16to32 y)) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(256);
        v3.AddArg(v2);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh16Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux32 x y)
    // result: (CMOVWHSconst (SRL <x.Type> (ZeroExt16to32 x) y) (CMPconst [256] y) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(y);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh16Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux64 x (Const64 [c]))
    // cond: uint64(c) < 16
    // result: (SRLconst (SLLconst <typ.UInt32> x [16]) [int32(c+16)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(OpARMSRLconst);
        v.AuxInt = int32ToAuxInt(int32(c + 16));
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg(x);
        v.AddArg(v0);
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
private static bool rewriteValueARM_OpRsh16Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16Ux8 x y)
    // result: (SRL (ZeroExt16to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRL);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh16x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x16 x y)
    // result: (SRAcond (SignExt16to32 x) (ZeroExt16to32 y) (CMPconst [256] (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRAcond);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(v1);
        v.AddArg3(v0, v1, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh16x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x32 x y)
    // result: (SRAcond (SignExt16to32 x) y (CMPconst [256] y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRAcond);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(y);
        v.AddArg3(v0, y, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh16x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x64 x (Const64 [c]))
    // cond: uint64(c) < 16
    // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [int32(c+16)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 16)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(int32(c + 16));
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh16x64 x (Const64 [c]))
    // cond: uint64(c) >= 16
    // result: (SRAconst (SLLconst <typ.UInt32> x [16]) [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 16)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(16);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpRsh16x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh16x8 x y)
    // result: (SRA (SignExt16to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt16to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh32Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux16 x y)
    // result: (CMOVWHSconst (SRL <x.Type> x (ZeroExt16to32 y)) (CMPconst [256] (ZeroExt16to32 y)) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        v0.AddArg2(x, v1);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(v1);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh32Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32Ux32 x y)
    // result: (CMOVWHSconst (SRL <x.Type> x y) (CMPconst [256] y) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        v0.AddArg2(x, y);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh32Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh32Ux64 x (Const64 [c]))
    // cond: uint64(c) < 32
    // result: (SRLconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(OpARMSRLconst);
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
private static bool rewriteValueARM_OpRsh32Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32Ux8 x y)
    // result: (SRL x (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRL);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh32x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x16 x y)
    // result: (SRAcond x (ZeroExt16to32 y) (CMPconst [256] (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRAcond);
        var v0 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v0.AddArg(y);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(v0);
        v.AddArg3(x, v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh32x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Rsh32x32 x y)
    // result: (SRAcond x y (CMPconst [256] y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRAcond);
        var v0 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v0.AuxInt = int32ToAuxInt(256);
        v0.AddArg(y);
        v.AddArg3(x, y, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh32x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Rsh32x64 x (Const64 [c]))
    // cond: uint64(c) < 32
    // result: (SRAconst x [int32(c)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 32)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(int32(c));
        v.AddArg(x);
        return true;

    } 
    // match: (Rsh32x64 x (Const64 [c]))
    // cond: uint64(c) >= 32
    // result: (SRAconst x [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 32)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpRsh32x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh32x8 x y)
    // result: (SRA x (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRA);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(y);
        v.AddArg2(x, v0);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh8Ux16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux16 x y)
    // result: (CMOVWHSconst (SRL <x.Type> (ZeroExt8to32 x) (ZeroExt16to32 y)) (CMPconst [256] (ZeroExt16to32 y)) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        var v2 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v2.AddArg(y);
        v0.AddArg2(v1, v2);
        var v3 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v3.AuxInt = int32ToAuxInt(256);
        v3.AddArg(v2);
        v.AddArg2(v0, v3);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh8Ux32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux32 x y)
    // result: (CMOVWHSconst (SRL <x.Type> (ZeroExt8to32 x) y) (CMPconst [256] y) [0])
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMCMOVWHSconst);
        v.AuxInt = int32ToAuxInt(0);
        var v0 = b.NewValue0(v.Pos, OpARMSRL, x.Type);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(x);
        v0.AddArg2(v1, y);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(y);
        v.AddArg2(v0, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh8Ux64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux64 x (Const64 [c]))
    // cond: uint64(c) < 8
    // result: (SRLconst (SLLconst <typ.UInt32> x [24]) [int32(c+24)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(OpARMSRLconst);
        v.AuxInt = int32ToAuxInt(int32(c + 24));
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(24);
        v0.AddArg(x);
        v.AddArg(v0);
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
private static bool rewriteValueARM_OpRsh8Ux8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8Ux8 x y)
    // result: (SRL (ZeroExt8to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRL);
        var v0 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh8x16(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x16 x y)
    // result: (SRAcond (SignExt8to32 x) (ZeroExt16to32 y) (CMPconst [256] (ZeroExt16to32 y)))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRAcond);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt16to32, typ.UInt32);
        v1.AddArg(y);
        var v2 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v2.AuxInt = int32ToAuxInt(256);
        v2.AddArg(v1);
        v.AddArg3(v0, v1, v2);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh8x32(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x32 x y)
    // result: (SRAcond (SignExt8to32 x) y (CMPconst [256] y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRAcond);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpARMCMPconst, types.TypeFlags);
        v1.AuxInt = int32ToAuxInt(256);
        v1.AddArg(y);
        v.AddArg3(v0, y, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpRsh8x64(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x64 x (Const64 [c]))
    // cond: uint64(c) < 8
    // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [int32(c+24)])
    while (true) {
        var x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        var c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) < 8)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(int32(c + 24));
        var v0 = b.NewValue0(v.Pos, OpARMSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(24);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    } 
    // match: (Rsh8x64 x (Const64 [c]))
    // cond: uint64(c) >= 8
    // result: (SRAconst (SLLconst <typ.UInt32> x [24]) [31])
    while (true) {
        x = v_0;
        if (v_1.Op != OpConst64) {
            break;
        }
        c = auxIntToInt64(v_1.AuxInt);
        if (!(uint64(c) >= 8)) {
            break;
        }
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v0 = b.NewValue0(v.Pos, OpARMSLLconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(24);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpRsh8x8(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_1 = v.Args[1];
    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Rsh8x8 x y)
    // result: (SRA (SignExt8to32 x) (ZeroExt8to32 y))
    while (true) {
        var x = v_0;
        var y = v_1;
        v.reset(OpARMSRA);
        var v0 = b.NewValue0(v.Pos, OpSignExt8to32, typ.Int32);
        v0.AddArg(x);
        var v1 = b.NewValue0(v.Pos, OpZeroExt8to32, typ.UInt32);
        v1.AddArg(y);
        v.AddArg2(v0, v1);
        return true;
    }

}
private static bool rewriteValueARM_OpSelect0(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Select0 (CALLudiv x (MOVWconst [1])))
    // result: x
    while (true) {
        if (v_0.Op != OpARMCALLudiv) {
            break;
        }
        _ = v_0.Args[1];
        var x = v_0.Args[0];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpARMMOVWconst || auxIntToInt32(v_0_1.AuxInt) != 1) {
            break;
        }
        v.copyOf(x);
        return true;

    } 
    // match: (Select0 (CALLudiv x (MOVWconst [c])))
    // cond: isPowerOfTwo32(c)
    // result: (SRLconst [int32(log32(c))] x)
    while (true) {
        if (v_0.Op != OpARMCALLudiv) {
            break;
        }
        _ = v_0.Args[1];
        x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0_1.AuxInt);
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpARMSRLconst);
        v.AuxInt = int32ToAuxInt(int32(log32(c)));
        v.AddArg(x);
        return true;

    } 
    // match: (Select0 (CALLudiv (MOVWconst [c]) (MOVWconst [d])))
    // cond: d != 0
    // result: (MOVWconst [int32(uint32(c)/uint32(d))])
    while (true) {
        if (v_0.Op != OpARMCALLudiv) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) / uint32(d)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpSelect1(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Select1 (CALLudiv _ (MOVWconst [1])))
    // result: (MOVWconst [0])
    while (true) {
        if (v_0.Op != OpARMCALLudiv) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpARMMOVWconst || auxIntToInt32(v_0_1.AuxInt) != 1) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(0);
        return true;

    } 
    // match: (Select1 (CALLudiv x (MOVWconst [c])))
    // cond: isPowerOfTwo32(c)
    // result: (ANDconst [c-1] x)
    while (true) {
        if (v_0.Op != OpARMCALLudiv) {
            break;
        }
        _ = v_0.Args[1];
        var x = v_0.Args[0];
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpARMMOVWconst) {
            break;
        }
        var c = auxIntToInt32(v_0_1.AuxInt);
        if (!(isPowerOfTwo32(c))) {
            break;
        }
        v.reset(OpARMANDconst);
        v.AuxInt = int32ToAuxInt(c - 1);
        v.AddArg(x);
        return true;

    } 
    // match: (Select1 (CALLudiv (MOVWconst [c]) (MOVWconst [d])))
    // cond: d != 0
    // result: (MOVWconst [int32(uint32(c)%uint32(d))])
    while (true) {
        if (v_0.Op != OpARMCALLudiv) {
            break;
        }
        _ = v_0.Args[1];
        var v_0_0 = v_0.Args[0];
        if (v_0_0.Op != OpARMMOVWconst) {
            break;
        }
        c = auxIntToInt32(v_0_0.AuxInt);
        v_0_1 = v_0.Args[1];
        if (v_0_1.Op != OpARMMOVWconst) {
            break;
        }
        var d = auxIntToInt32(v_0_1.AuxInt);
        if (!(d != 0)) {
            break;
        }
        v.reset(OpARMMOVWconst);
        v.AuxInt = int32ToAuxInt(int32(uint32(c) % uint32(d)));
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpSignmask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0]; 
    // match: (Signmask x)
    // result: (SRAconst x [31])
    while (true) {
        var x = v_0;
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        v.AddArg(x);
        return true;
    }

}
private static bool rewriteValueARM_OpSlicemask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block; 
    // match: (Slicemask <t> x)
    // result: (SRAconst (RSBconst <t> [0] x) [31])
    while (true) {
        var t = v.Type;
        var x = v_0;
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        var v0 = b.NewValue0(v.Pos, OpARMRSBconst, t);
        v0.AuxInt = int32ToAuxInt(0);
        v0.AddArg(x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteValueARM_OpStore(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_2 = v.Args[2];
    var v_1 = v.Args[1];
    var v_0 = v.Args[0]; 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 1
    // result: (MOVBstore ptr val mem)
    while (true) {
        var t = auxToType(v.Aux);
        var ptr = v_0;
        var val = v_1;
        var mem = v_2;
        if (!(t.Size() == 1)) {
            break;
        }
        v.reset(OpARMMOVBstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 2
    // result: (MOVHstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 2)) {
            break;
        }
        v.reset(OpARMMOVHstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && !is32BitFloat(val.Type)
    // result: (MOVWstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && !is32BitFloat(val.Type))) {
            break;
        }
        v.reset(OpARMMOVWstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 4 && is32BitFloat(val.Type)
    // result: (MOVFstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 4 && is32BitFloat(val.Type))) {
            break;
        }
        v.reset(OpARMMOVFstore);
        v.AddArg3(ptr, val, mem);
        return true;

    } 
    // match: (Store {t} ptr val mem)
    // cond: t.Size() == 8 && is64BitFloat(val.Type)
    // result: (MOVDstore ptr val mem)
    while (true) {
        t = auxToType(v.Aux);
        ptr = v_0;
        val = v_1;
        mem = v_2;
        if (!(t.Size() == 8 && is64BitFloat(val.Type))) {
            break;
        }
        v.reset(OpARMMOVDstore);
        v.AddArg3(ptr, val, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpZero(ptr<Value> _addr_v) {
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
    // match: (Zero [1] ptr mem)
    // result: (MOVBstore ptr (MOVWconst [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 1) {
            break;
        }
        var ptr = v_0;
        mem = v_1;
        v.reset(OpARMMOVBstore);
        var v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [2] {t} ptr mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore ptr (MOVWconst [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        var t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpARMMOVHstore);
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [2] ptr mem)
    // result: (MOVBstore [1] ptr (MOVWconst [0]) (MOVBstore [0] ptr (MOVWconst [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 2) {
            break;
        }
        ptr = v_0;
        mem = v_1;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(1);
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        var v1 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg3(ptr, v0, mem);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [4] {t} ptr mem)
    // cond: t.Alignment()%4 == 0
    // result: (MOVWstore ptr (MOVWconst [0]) mem)
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 4 == 0)) {
            break;
        }
        v.reset(OpARMMOVWstore);
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [4] {t} ptr mem)
    // cond: t.Alignment()%2 == 0
    // result: (MOVHstore [2] ptr (MOVWconst [0]) (MOVHstore [0] ptr (MOVWconst [0]) mem))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(t.Alignment() % 2 == 0)) {
            break;
        }
        v.reset(OpARMMOVHstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpARMMOVHstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(0);
        v1.AddArg3(ptr, v0, mem);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [4] ptr mem)
    // result: (MOVBstore [3] ptr (MOVWconst [0]) (MOVBstore [2] ptr (MOVWconst [0]) (MOVBstore [1] ptr (MOVWconst [0]) (MOVBstore [0] ptr (MOVWconst [0]) mem))))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 4) {
            break;
        }
        ptr = v_0;
        mem = v_1;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(3);
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(2);
        var v2 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(1);
        var v3 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v3.AuxInt = int32ToAuxInt(0);
        v3.AddArg3(ptr, v0, mem);
        v2.AddArg3(ptr, v0, v3);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [3] ptr mem)
    // result: (MOVBstore [2] ptr (MOVWconst [0]) (MOVBstore [1] ptr (MOVWconst [0]) (MOVBstore [0] ptr (MOVWconst [0]) mem)))
    while (true) {
        if (auxIntToInt64(v.AuxInt) != 3) {
            break;
        }
        ptr = v_0;
        mem = v_1;
        v.reset(OpARMMOVBstore);
        v.AuxInt = int32ToAuxInt(2);
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v1 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v1.AuxInt = int32ToAuxInt(1);
        v2 = b.NewValue0(v.Pos, OpARMMOVBstore, types.TypeMem);
        v2.AuxInt = int32ToAuxInt(0);
        v2.AddArg3(ptr, v0, mem);
        v1.AddArg3(ptr, v0, v2);
        v.AddArg3(ptr, v0, v1);
        return true;

    } 
    // match: (Zero [s] {t} ptr mem)
    // cond: s%4 == 0 && s > 4 && s <= 512 && t.Alignment()%4 == 0 && !config.noDuffDevice
    // result: (DUFFZERO [4 * (128 - s/4)] ptr (MOVWconst [0]) mem)
    while (true) {
        var s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!(s % 4 == 0 && s > 4 && s <= 512 && t.Alignment() % 4 == 0 && !config.noDuffDevice)) {
            break;
        }
        v.reset(OpARMDUFFZERO);
        v.AuxInt = int64ToAuxInt(4 * (128 - s / 4));
        v0 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v0.AuxInt = int32ToAuxInt(0);
        v.AddArg3(ptr, v0, mem);
        return true;

    } 
    // match: (Zero [s] {t} ptr mem)
    // cond: (s > 512 || config.noDuffDevice) || t.Alignment()%4 != 0
    // result: (LoweredZero [t.Alignment()] ptr (ADDconst <ptr.Type> ptr [int32(s-moveSize(t.Alignment(), config))]) (MOVWconst [0]) mem)
    while (true) {
        s = auxIntToInt64(v.AuxInt);
        t = auxToType(v.Aux);
        ptr = v_0;
        mem = v_1;
        if (!((s > 512 || config.noDuffDevice) || t.Alignment() % 4 != 0)) {
            break;
        }
        v.reset(OpARMLoweredZero);
        v.AuxInt = int64ToAuxInt(t.Alignment());
        v0 = b.NewValue0(v.Pos, OpARMADDconst, ptr.Type);
        v0.AuxInt = int32ToAuxInt(int32(s - moveSize(t.Alignment(), config)));
        v0.AddArg(ptr);
        v1 = b.NewValue0(v.Pos, OpARMMOVWconst, typ.UInt32);
        v1.AuxInt = int32ToAuxInt(0);
        v.AddArg4(ptr, v0, v1, mem);
        return true;

    }
    return false;

}
private static bool rewriteValueARM_OpZeromask(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    var v_0 = v.Args[0];
    var b = v.Block;
    var typ = _addr_b.Func.Config.Types; 
    // match: (Zeromask x)
    // result: (SRAconst (RSBshiftRL <typ.Int32> x x [1]) [31])
    while (true) {
        var x = v_0;
        v.reset(OpARMSRAconst);
        v.AuxInt = int32ToAuxInt(31);
        var v0 = b.NewValue0(v.Pos, OpARMRSBshiftRL, typ.Int32);
        v0.AuxInt = int32ToAuxInt(1);
        v0.AddArg2(x, x);
        v.AddArg(v0);
        return true;
    }

}
private static bool rewriteBlockARM(ptr<Block> _addr_b) {
    ref Block b = ref _addr_b.val;


    if (b.Kind == BlockARMEQ) 
        // match: (EQ (FlagConstant [fc]) yes no)
        // cond: fc.eq()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            var v_0 = b.Controls[0];
            var fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.eq())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (EQ (FlagConstant [fc]) yes no)
        // cond: !fc.eq()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.eq())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (EQ (InvertFlags cmp) yes no)
        // result: (EQ cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            var cmp = v_0.Args[0];
            b.resetWithControl(BlockARMEQ, cmp);
            return true;
        } 
        // match: (EQ (CMP x (RSBconst [0] y)))
        // result: (EQ (CMN x y))
        while (b.Controls[0].Op == OpARMCMP) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            var x = v_0.Args[0];
            var v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                break;
            }
            var y = v_0_1.Args[0];
            var v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMN x (RSBconst [0] y)))
        // result: (EQ (CMP x y))
        while (b.Controls[0].Op == OpARMCMN) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            var v_0_0 = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            {
                nint _i0__prev2 = _i0;

                nint _i0 = 0;

                while (_i0 <= 1) {
                    x = v_0_0;
                    if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                        continue;
                    (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                    }

                    y = v_0_1.Args[0];
                    v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMEQ, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (EQ (CMPconst [0] l:(SUB x y)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMP x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            var l = v_0.Args[0];
            if (l.Op != OpARMSUB) {
                break;
            }
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(MULS x y a)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMP a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULS) {
                break;
            }
            var a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            var v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBconst) {
                break;
            }
            var c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLLreg) {
                break;
            }
            var z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(SUBshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMPshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADD x y)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMN x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADD) {
                break;
            }
            _ = l.Args[1];
            var l_0 = l.Args[0];
            var l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMEQ, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (EQ (CMPconst [0] l:(MULA x y a)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMN a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULA) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ADDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (CMNshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(AND x y)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TST x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMAND) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTST, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMEQ, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (EQ (CMPconst [0] l:(ANDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ANDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ANDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ANDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ANDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ANDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(ANDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TSTshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XOR x y)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQ x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXOR) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTEQ, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMEQ, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (EQ (CMPconst [0] l:(XORconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XORshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XORshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XORshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XORshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XORshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        } 
        // match: (EQ (CMPconst [0] l:(XORshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (EQ (TEQshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMEQ, v0);
            return true;
        }
    else if (b.Kind == BlockARMGE) 
        // match: (GE (FlagConstant [fc]) yes no)
        // cond: fc.ge()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.ge())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GE (FlagConstant [fc]) yes no)
        // cond: !fc.ge()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.ge())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GE (InvertFlags cmp) yes no)
        // result: (LE cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMLE, cmp);
            return true;
        } 
        // match: (GE (CMP x (RSBconst [0] y)))
        // result: (GE (CMN x y))
        while (b.Controls[0].Op == OpARMCMP) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            x = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                break;
            }
            y = v_0_1.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGE, v0);
            return true;
        } 
        // match: (GE (CMN x (RSBconst [0] y)))
        // result: (GE (CMP x y))
        while (b.Controls[0].Op == OpARMCMN) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = v_0_0;
                    if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                        continue;
                    (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                    }

                    y = v_0_1.Args[0];
                    v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGE, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GE (CMPconst [0] l:(SUB x y)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMP x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUB) {
                break;
            }
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(MULS x y a)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMP a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULS) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(SUBshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMPshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADD x y)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMN x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADD) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGEnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GE (CMPconst [0] l:(MULA x y a)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMN a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULA) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ADDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (CMNshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(AND x y)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TST x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMAND) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTST, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGEnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GE (CMPconst [0] l:(ANDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ANDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ANDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ANDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ANDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ANDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(ANDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TSTshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XOR x y)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQ x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXOR) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTEQ, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGEnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GE (CMPconst [0] l:(XORconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XORshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XORshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XORshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XORshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XORshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        } 
        // match: (GE (CMPconst [0] l:(XORshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GEnoov (TEQshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGEnoov, v0);
            return true;
        }
    else if (b.Kind == BlockARMGEnoov) 
        // match: (GEnoov (FlagConstant [fc]) yes no)
        // cond: fc.geNoov()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.geNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GEnoov (FlagConstant [fc]) yes no)
        // cond: !fc.geNoov()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.geNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GEnoov (InvertFlags cmp) yes no)
        // result: (LEnoov cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMLEnoov, cmp);
            return true;
        }
    else if (b.Kind == BlockARMGT) 
        // match: (GT (FlagConstant [fc]) yes no)
        // cond: fc.gt()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.gt())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GT (FlagConstant [fc]) yes no)
        // cond: !fc.gt()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.gt())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GT (InvertFlags cmp) yes no)
        // result: (LT cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMLT, cmp);
            return true;
        } 
        // match: (GT (CMP x (RSBconst [0] y)))
        // result: (GT (CMN x y))
        while (b.Controls[0].Op == OpARMCMP) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            x = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                break;
            }
            y = v_0_1.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGT, v0);
            return true;
        } 
        // match: (GT (CMN x (RSBconst [0] y)))
        // result: (GT (CMP x y))
        while (b.Controls[0].Op == OpARMCMN) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = v_0_0;
                    if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                        continue;
                    (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                    }

                    y = v_0_1.Args[0];
                    v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGT, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GT (CMPconst [0] l:(SUB x y)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMP x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUB) {
                break;
            }
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(MULS x y a)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMP a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULS) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(SUBshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMPshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADD x y)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMN x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADD) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGTnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GT (CMPconst [0] l:(ADDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ADDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMNshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(MULA x y a)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (CMN a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULA) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(AND x y)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TST x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMAND) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTST, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGTnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GT (CMPconst [0] l:(ANDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ANDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ANDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ANDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ANDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ANDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(ANDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TSTshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XOR x y)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQ x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXOR) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTEQ, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMGTnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (GT (CMPconst [0] l:(XORconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XORshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XORshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XORshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XORshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XORshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        } 
        // match: (GT (CMPconst [0] l:(XORshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (GTnoov (TEQshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMGTnoov, v0);
            return true;
        }
    else if (b.Kind == BlockARMGTnoov) 
        // match: (GTnoov (FlagConstant [fc]) yes no)
        // cond: fc.gtNoov()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.gtNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (GTnoov (FlagConstant [fc]) yes no)
        // cond: !fc.gtNoov()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.gtNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (GTnoov (InvertFlags cmp) yes no)
        // result: (LTnoov cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMLTnoov, cmp);
            return true;
        }
    else if (b.Kind == BlockIf) 
        // match: (If (Equal cc) yes no)
        // result: (EQ cc yes no)
        while (b.Controls[0].Op == OpARMEqual) {
            v_0 = b.Controls[0];
            var cc = v_0.Args[0];
            b.resetWithControl(BlockARMEQ, cc);
            return true;
        } 
        // match: (If (NotEqual cc) yes no)
        // result: (NE cc yes no)
        while (b.Controls[0].Op == OpARMNotEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMNE, cc);
            return true;
        } 
        // match: (If (LessThan cc) yes no)
        // result: (LT cc yes no)
        while (b.Controls[0].Op == OpARMLessThan) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMLT, cc);
            return true;
        } 
        // match: (If (LessThanU cc) yes no)
        // result: (ULT cc yes no)
        while (b.Controls[0].Op == OpARMLessThanU) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMULT, cc);
            return true;
        } 
        // match: (If (LessEqual cc) yes no)
        // result: (LE cc yes no)
        while (b.Controls[0].Op == OpARMLessEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMLE, cc);
            return true;
        } 
        // match: (If (LessEqualU cc) yes no)
        // result: (ULE cc yes no)
        while (b.Controls[0].Op == OpARMLessEqualU) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMULE, cc);
            return true;
        } 
        // match: (If (GreaterThan cc) yes no)
        // result: (GT cc yes no)
        while (b.Controls[0].Op == OpARMGreaterThan) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMGT, cc);
            return true;
        } 
        // match: (If (GreaterThanU cc) yes no)
        // result: (UGT cc yes no)
        while (b.Controls[0].Op == OpARMGreaterThanU) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMUGT, cc);
            return true;
        } 
        // match: (If (GreaterEqual cc) yes no)
        // result: (GE cc yes no)
        while (b.Controls[0].Op == OpARMGreaterEqual) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMGE, cc);
            return true;
        } 
        // match: (If (GreaterEqualU cc) yes no)
        // result: (UGE cc yes no)
        while (b.Controls[0].Op == OpARMGreaterEqualU) {
            v_0 = b.Controls[0];
            cc = v_0.Args[0];
            b.resetWithControl(BlockARMUGE, cc);
            return true;
        } 
        // match: (If cond yes no)
        // result: (NE (CMPconst [0] cond) yes no)
        while (true) {
            var cond = b.Controls[0];
            v0 = b.NewValue0(cond.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(0);
            v0.AddArg(cond);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        }
    else if (b.Kind == BlockARMLE) 
        // match: (LE (FlagConstant [fc]) yes no)
        // cond: fc.le()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.le())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LE (FlagConstant [fc]) yes no)
        // cond: !fc.le()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.le())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LE (InvertFlags cmp) yes no)
        // result: (GE cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMGE, cmp);
            return true;
        } 
        // match: (LE (CMP x (RSBconst [0] y)))
        // result: (LE (CMN x y))
        while (b.Controls[0].Op == OpARMCMP) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            x = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                break;
            }
            y = v_0_1.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLE, v0);
            return true;
        } 
        // match: (LE (CMN x (RSBconst [0] y)))
        // result: (LE (CMP x y))
        while (b.Controls[0].Op == OpARMCMN) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = v_0_0;
                    if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                        continue;
                    (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                    }

                    y = v_0_1.Args[0];
                    v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLE, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LE (CMPconst [0] l:(SUB x y)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMP x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUB) {
                break;
            }
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(MULS x y a)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMP a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULS) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(SUBshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMPshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADD x y)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMN x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADD) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLEnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LE (CMPconst [0] l:(MULA x y a)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMN a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULA) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ADDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (CMNshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(AND x y)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TST x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMAND) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTST, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLEnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LE (CMPconst [0] l:(ANDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ANDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ANDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ANDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ANDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ANDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(ANDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TSTshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XOR x y)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQ x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXOR) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTEQ, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLEnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LE (CMPconst [0] l:(XORconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XORshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XORshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XORshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XORshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XORshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        } 
        // match: (LE (CMPconst [0] l:(XORshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LEnoov (TEQshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLEnoov, v0);
            return true;
        }
    else if (b.Kind == BlockARMLEnoov) 
        // match: (LEnoov (FlagConstant [fc]) yes no)
        // cond: fc.leNoov()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.leNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LEnoov (FlagConstant [fc]) yes no)
        // cond: !fc.leNoov()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.leNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LEnoov (InvertFlags cmp) yes no)
        // result: (GEnoov cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMGEnoov, cmp);
            return true;
        }
    else if (b.Kind == BlockARMLT) 
        // match: (LT (FlagConstant [fc]) yes no)
        // cond: fc.lt()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.lt())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LT (FlagConstant [fc]) yes no)
        // cond: !fc.lt()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.lt())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LT (InvertFlags cmp) yes no)
        // result: (GT cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMGT, cmp);
            return true;
        } 
        // match: (LT (CMP x (RSBconst [0] y)))
        // result: (LT (CMN x y))
        while (b.Controls[0].Op == OpARMCMP) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            x = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                break;
            }
            y = v_0_1.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLT, v0);
            return true;
        } 
        // match: (LT (CMN x (RSBconst [0] y)))
        // result: (LT (CMP x y))
        while (b.Controls[0].Op == OpARMCMN) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = v_0_0;
                    if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                        continue;
                    (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                    }

                    y = v_0_1.Args[0];
                    v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLT, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LT (CMPconst [0] l:(SUB x y)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMP x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUB) {
                break;
            }
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(MULS x y a)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMP a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULS) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(SUBshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMPshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADD x y)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMN x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADD) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLTnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LT (CMPconst [0] l:(MULA x y a)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMN a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULA) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ADDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (CMNshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(AND x y)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TST x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMAND) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTST, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLTnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LT (CMPconst [0] l:(ANDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ANDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ANDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ANDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ANDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ANDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(ANDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TSTshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XOR x y)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQ x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXOR) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTEQ, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMLTnoov, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (LT (CMPconst [0] l:(XORconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XORshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XORshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XORshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XORshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XORshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        } 
        // match: (LT (CMPconst [0] l:(XORshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (LTnoov (TEQshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMLTnoov, v0);
            return true;
        }
    else if (b.Kind == BlockARMLTnoov) 
        // match: (LTnoov (FlagConstant [fc]) yes no)
        // cond: fc.ltNoov()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.ltNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (LTnoov (FlagConstant [fc]) yes no)
        // cond: !fc.ltNoov()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.ltNoov())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (LTnoov (InvertFlags cmp) yes no)
        // result: (GTnoov cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMGTnoov, cmp);
            return true;
        }
    else if (b.Kind == BlockARMNE) 
        // match: (NE (CMPconst [0] (Equal cc)) yes no)
        // result: (EQ cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMEQ, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (NotEqual cc)) yes no)
        // result: (NE cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMNotEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMNE, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (LessThan cc)) yes no)
        // result: (LT cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMLessThan) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMLT, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (LessThanU cc)) yes no)
        // result: (ULT cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMLessThanU) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMULT, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (LessEqual cc)) yes no)
        // result: (LE cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMLessEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMLE, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (LessEqualU cc)) yes no)
        // result: (ULE cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMLessEqualU) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMULE, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (GreaterThan cc)) yes no)
        // result: (GT cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMGreaterThan) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMGT, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (GreaterThanU cc)) yes no)
        // result: (UGT cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMGreaterThanU) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMUGT, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (GreaterEqual cc)) yes no)
        // result: (GE cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMGreaterEqual) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMGE, cc);
            return true;
        } 
        // match: (NE (CMPconst [0] (GreaterEqualU cc)) yes no)
        // result: (UGE cc yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            v_0_0 = v_0.Args[0];
            if (v_0_0.Op != OpARMGreaterEqualU) {
                break;
            }
            cc = v_0_0.Args[0];
            b.resetWithControl(BlockARMUGE, cc);
            return true;
        } 
        // match: (NE (FlagConstant [fc]) yes no)
        // cond: fc.ne()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.ne())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (NE (FlagConstant [fc]) yes no)
        // cond: !fc.ne()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.ne())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (NE (InvertFlags cmp) yes no)
        // result: (NE cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMNE, cmp);
            return true;
        } 
        // match: (NE (CMP x (RSBconst [0] y)))
        // result: (NE (CMN x y))
        while (b.Controls[0].Op == OpARMCMP) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            x = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                break;
            }
            y = v_0_1.Args[0];
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMN x (RSBconst [0] y)))
        // result: (NE (CMP x y))
        while (b.Controls[0].Op == OpARMCMN) {
            v_0 = b.Controls[0];
            _ = v_0.Args[1];
            v_0_0 = v_0.Args[0];
            v_0_1 = v_0.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = v_0_0;
                    if (v_0_1.Op != OpARMRSBconst || auxIntToInt32(v_0_1.AuxInt) != 0) {
                        continue;
                    (_i0, v_0_0, v_0_1) = (_i0 + 1, v_0_1, v_0_0);
                    }

                    y = v_0_1.Args[0];
                    v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMNE, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (NE (CMPconst [0] l:(SUB x y)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMP x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUB) {
                break;
            }
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(MULS x y a)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMP a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULS) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMP, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(SUBshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMPshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMSUBshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMPshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADD x y)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMN x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADD) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMNE, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (NE (CMPconst [0] l:(MULA x y a)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMN a (MUL <x.Type> x y)) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMMULA) {
                break;
            }
            a = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMN, types.TypeFlags);
            v1 = b.NewValue0(v_0.Pos, OpARMMUL, x.Type);
            v1.AddArg2(x, y);
            v0.AddArg2(a, v1);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ADDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (CMNshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMADDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMCMNshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(AND x y)) yes no)
        // cond: l.Uses==1
        // result: (NE (TST x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMAND) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTST, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMNE, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (NE (CMPconst [0] l:(ANDconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ANDshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ANDshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ANDshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ANDshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ANDshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(ANDshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (TSTshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMANDshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTSTshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XOR x y)) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQ x y) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXOR) {
                break;
            }
            _ = l.Args[1];
            l_0 = l.Args[0];
            l_1 = l.Args[1];
            {
                nint _i0__prev2 = _i0;

                _i0 = 0;

                while (_i0 <= 1) {
                    x = l_0;
                    y = l_1;
                    if (!(l.Uses == 1)) {
                        continue;
                    (_i0, l_0, l_1) = (_i0 + 1, l_1, l_0);
                    }

                    v0 = b.NewValue0(v_0.Pos, OpARMTEQ, types.TypeFlags);
                    v0.AddArg2(x, y);
                    b.resetWithControl(BlockARMNE, v0);
                    return true;

                }


                _i0 = _i0__prev2;
            }
            break;

        } 
        // match: (NE (CMPconst [0] l:(XORconst [c] x)) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQconst [c] x) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORconst) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQconst, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg(x);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XORshiftLL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQshiftLL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XORshiftRL x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQshiftRL x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRL) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRL, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XORshiftRA x y [c])) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQshiftRA x y [c]) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRA) {
                break;
            }
            c = auxIntToInt32(l.AuxInt);
            y = l.Args[1];
            x = l.Args[0];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRA, types.TypeFlags);
            v0.AuxInt = int32ToAuxInt(c);
            v0.AddArg2(x, y);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XORshiftLLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQshiftLLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftLLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftLLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XORshiftRLreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQshiftRLreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRLreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRLreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        } 
        // match: (NE (CMPconst [0] l:(XORshiftRAreg x y z)) yes no)
        // cond: l.Uses==1
        // result: (NE (TEQshiftRAreg x y z) yes no)
        while (b.Controls[0].Op == OpARMCMPconst) {
            v_0 = b.Controls[0];
            if (auxIntToInt32(v_0.AuxInt) != 0) {
                break;
            }
            l = v_0.Args[0];
            if (l.Op != OpARMXORshiftRAreg) {
                break;
            }
            z = l.Args[2];
            x = l.Args[0];
            y = l.Args[1];
            if (!(l.Uses == 1)) {
                break;
            }
            v0 = b.NewValue0(v_0.Pos, OpARMTEQshiftRAreg, types.TypeFlags);
            v0.AddArg3(x, y, z);
            b.resetWithControl(BlockARMNE, v0);
            return true;
        }
    else if (b.Kind == BlockARMUGE) 
        // match: (UGE (FlagConstant [fc]) yes no)
        // cond: fc.uge()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.uge())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (UGE (FlagConstant [fc]) yes no)
        // cond: !fc.uge()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.uge())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGE (InvertFlags cmp) yes no)
        // result: (ULE cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMULE, cmp);
            return true;
        }
    else if (b.Kind == BlockARMUGT) 
        // match: (UGT (FlagConstant [fc]) yes no)
        // cond: fc.ugt()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.ugt())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (UGT (FlagConstant [fc]) yes no)
        // cond: !fc.ugt()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.ugt())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (UGT (InvertFlags cmp) yes no)
        // result: (ULT cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMULT, cmp);
            return true;
        }
    else if (b.Kind == BlockARMULE) 
        // match: (ULE (FlagConstant [fc]) yes no)
        // cond: fc.ule()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.ule())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULE (FlagConstant [fc]) yes no)
        // cond: !fc.ule()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.ule())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (ULE (InvertFlags cmp) yes no)
        // result: (UGE cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMUGE, cmp);
            return true;
        }
    else if (b.Kind == BlockARMULT) 
        // match: (ULT (FlagConstant [fc]) yes no)
        // cond: fc.ult()
        // result: (First yes no)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(fc.ult())) {
                break;
            }
            b.Reset(BlockFirst);
            return true;
        } 
        // match: (ULT (FlagConstant [fc]) yes no)
        // cond: !fc.ult()
        // result: (First no yes)
        while (b.Controls[0].Op == OpARMFlagConstant) {
            v_0 = b.Controls[0];
            fc = auxIntToFlagConstant(v_0.AuxInt);
            if (!(!fc.ult())) {
                break;
            }
            b.Reset(BlockFirst);
            b.swapSuccessors();
            return true;
        } 
        // match: (ULT (InvertFlags cmp) yes no)
        // result: (UGT cmp yes no)
        while (b.Controls[0].Op == OpARMInvertFlags) {
            v_0 = b.Controls[0];
            cmp = v_0.Args[0];
            b.resetWithControl(BlockARMUGT, cmp);
            return true;
        }
        return false;

}

} // end ssa_package
