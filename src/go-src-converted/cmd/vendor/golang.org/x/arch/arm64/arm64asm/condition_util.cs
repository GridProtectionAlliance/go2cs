// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package arm64asm -- go2cs converted at 2022 March 13 06:37:52 UTC
// import "cmd/vendor/golang.org/x/arch/arm64/arm64asm" ==> using arm64asm = go.cmd.vendor.golang.org.x.arch.arm64.arm64asm_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\arch\arm64\arm64asm\condition_util.go
namespace go.cmd.vendor.golang.org.x.arch.arm64;

public static partial class arm64asm_package {

private static uint extract_bit(uint value, uint bit) {
    return (value >> (int)(bit)) & 1;
}

private static bool bfxpreferred_4(uint sf, uint opc1, uint imms, uint immr) {
    if (imms < immr) {
        return false;
    }
    if ((imms >> 5 == sf) && (imms & 0x1f == 0x1f)) {
        return false;
    }
    if (immr == 0) {
        if (sf == 0 && (imms == 7 || imms == 15)) {
            return false;
        }
        if (sf == 1 && opc1 == 0 && (imms == 7 || imms == 15 || imms == 31)) {
            return false;
        }
    }
    return true;
}

private static bool move_wide_preferred_4(uint sf, uint N, uint imms, uint immr) {
    if (sf == 1 && N != 1) {
        return false;
    }
    if (sf == 0 && !(N == 0 && ((imms >> 5) & 1) == 0)) {
        return false;
    }
    if (imms < 16) {
        return (-immr) % 16 <= (15 - imms);
    }
    var width = uint32(32);
    if (sf == 1) {
        width = uint32(64);
    }
    if (imms >= (width - 15)) {
        return (immr % 16) <= (imms - (width - 15));
    }
    return false;
}

public partial struct Sys { // : byte
}

public static readonly Sys Sys_AT = iota;
public static readonly var Sys_DC = 0;
public static readonly var Sys_IC = 1;
public static readonly var Sys_TLBI = 2;
public static readonly var Sys_SYS = 3;

private static Sys sys_op_4(uint op1, uint crn, uint crm, uint op2) { 
    // TODO: system instruction
    return Sys_SYS;
}

private static bool is_zero(uint x) {
    return x == 0;
}

private static bool is_ones_n16(uint x) {
    return x == 0xffff;
}

private static byte bit_count(uint x) {
    byte count = default;
    count = 0;

    while (x > 0) {
        if ((x & 1) == 1) {
            count++;
        x>>=1;
        }
    }
    return count;
}

} // end arm64asm_package
