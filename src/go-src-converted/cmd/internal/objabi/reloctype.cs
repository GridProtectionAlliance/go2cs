// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/l.h
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package objabi -- go2cs converted at 2022 March 13 05:43:22 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Program Files\Go\src\cmd\internal\objabi\reloctype.go
namespace go.cmd.@internal;

public static partial class objabi_package {

public partial struct RelocType { // : short
}

//go:generate stringer -type=RelocType
public static readonly RelocType R_ADDR = 1 + iota; 
// R_ADDRPOWER relocates a pair of "D-form" instructions (instructions with 16-bit
// immediates in the low half of the instruction word), usually addis followed by
// another add or a load, inserting the "high adjusted" 16 bits of the address of
// the referenced symbol into the immediate field of the first instruction and the
// low 16 bits into that of the second instruction.
public static readonly var R_ADDRPOWER = 0; 
// R_ADDRARM64 relocates an adrp, add pair to compute the address of the
// referenced symbol.
public static readonly var R_ADDRARM64 = 1; 
// R_ADDRMIPS (only used on mips/mips64) resolves to the low 16 bits of an external
// address, by encoding it into the instruction.
public static readonly var R_ADDRMIPS = 2; 
// R_ADDROFF resolves to a 32-bit offset from the beginning of the section
// holding the data being relocated to the referenced symbol.
public static readonly var R_ADDROFF = 3;
public static readonly var R_SIZE = 4;
public static readonly var R_CALL = 5;
public static readonly var R_CALLARM = 6;
public static readonly var R_CALLARM64 = 7;
public static readonly var R_CALLIND = 8;
public static readonly var R_CALLPOWER = 9; 
// R_CALLMIPS (only used on mips64) resolves to non-PC-relative target address
// of a CALL (JAL) instruction, by encoding the address into the instruction.
public static readonly var R_CALLMIPS = 10; 
// R_CALLRISCV marks RISC-V CALLs for stack checking.
public static readonly var R_CALLRISCV = 11;
public static readonly var R_CONST = 12;
public static readonly var R_PCREL = 13; 
// R_TLS_LE, used on 386, amd64, and ARM, resolves to the offset of the
// thread-local symbol from the thread local base and is used to implement the
// "local exec" model for tls access (r.Sym is not set on intel platforms but is
// set to a TLS symbol -- runtime.tlsg -- in the linker when externally linking).
public static readonly var R_TLS_LE = 14; 
// R_TLS_IE, used 386, amd64, and ARM resolves to the PC-relative offset to a GOT
// slot containing the offset from the thread-local symbol from the thread local
// base and is used to implemented the "initial exec" model for tls access (r.Sym
// is not set on intel platforms but is set to a TLS symbol -- runtime.tlsg -- in
// the linker when externally linking).
public static readonly var R_TLS_IE = 15;
public static readonly var R_GOTOFF = 16;
public static readonly var R_PLT0 = 17;
public static readonly var R_PLT1 = 18;
public static readonly var R_PLT2 = 19;
public static readonly var R_USEFIELD = 20; 
// R_USETYPE resolves to an *rtype, but no relocation is created. The
// linker uses this as a signal that the pointed-to type information
// should be linked into the final binary, even if there are no other
// direct references. (This is used for types reachable by reflection.)
public static readonly var R_USETYPE = 21; 
// R_USEIFACE marks a type is converted to an interface in the function this
// relocation is applied to. The target is a type descriptor.
// This is a marker relocation (0-sized), for the linker's reachabililty
// analysis.
public static readonly var R_USEIFACE = 22; 
// R_USEIFACEMETHOD marks an interface method that is used in the function
// this relocation is applied to. The target is an interface type descriptor.
// The addend is the offset of the method in the type descriptor.
// This is a marker relocation (0-sized), for the linker's reachabililty
// analysis.
public static readonly var R_USEIFACEMETHOD = 23; 
// R_METHODOFF resolves to a 32-bit offset from the beginning of the section
// holding the data being relocated to the referenced symbol.
// It is a variant of R_ADDROFF used when linking from the uncommonType of a
// *rtype, and may be set to zero by the linker if it determines the method
// text is unreachable by the linked program.
public static readonly var R_METHODOFF = 24; 
// R_KEEP tells the linker to keep the referred-to symbol in the final binary
// if the symbol containing the R_KEEP relocation is in the final binary.
public static readonly var R_KEEP = 25;
public static readonly var R_POWER_TOC = 26;
public static readonly var R_GOTPCREL = 27; 
// R_JMPMIPS (only used on mips64) resolves to non-PC-relative target address
// of a JMP instruction, by encoding the address into the instruction.
// The stack nosplit check ignores this since it is not a function call.
public static readonly var R_JMPMIPS = 28; 

// R_DWARFSECREF resolves to the offset of the symbol from its section.
// Target of relocation must be size 4 (in current implementation).
public static readonly var R_DWARFSECREF = 29; 

// R_DWARFFILEREF resolves to an index into the DWARF .debug_line
// file table for the specified file symbol. Must be applied to an
// attribute of form DW_FORM_data4.
public static readonly var R_DWARFFILEREF = 30; 

// Platform dependent relocations. Architectures with fixed width instructions
// have the inherent issue that a 32-bit (or 64-bit!) displacement cannot be
// stuffed into a 32-bit instruction, so an address needs to be spread across
// several instructions, and in turn this requires a sequence of relocations, each
// updating a part of an instruction. This leads to relocation codes that are
// inherently processor specific.

// Arm64.

// Set a MOV[NZ] immediate field to bits [15:0] of the offset from the thread
// local base to the thread local variable defined by the referenced (thread
// local) symbol. Error if the offset does not fit into 16 bits.
public static readonly var R_ARM64_TLS_LE = 31; 

// Relocates an ADRP; LD64 instruction sequence to load the offset between
// the thread local base and the thread local variable defined by the
// referenced (thread local) symbol from the GOT.
public static readonly var R_ARM64_TLS_IE = 32; 

// R_ARM64_GOTPCREL relocates an adrp, ld64 pair to compute the address of the GOT
// slot of the referenced symbol.
public static readonly var R_ARM64_GOTPCREL = 33; 

// R_ARM64_GOT resolves a GOT-relative instruction sequence, usually an adrp
// followed by another ld instruction.
public static readonly var R_ARM64_GOT = 34; 

// R_ARM64_PCREL resolves a PC-relative addresses instruction sequence, usually an
// adrp followed by another add instruction.
public static readonly var R_ARM64_PCREL = 35; 

// R_ARM64_LDST8 sets a LD/ST immediate value to bits [11:0] of a local address.
public static readonly var R_ARM64_LDST8 = 36; 

// R_ARM64_LDST16 sets a LD/ST immediate value to bits [11:1] of a local address.
public static readonly var R_ARM64_LDST16 = 37; 

// R_ARM64_LDST32 sets a LD/ST immediate value to bits [11:2] of a local address.
public static readonly var R_ARM64_LDST32 = 38; 

// R_ARM64_LDST64 sets a LD/ST immediate value to bits [11:3] of a local address.
public static readonly var R_ARM64_LDST64 = 39; 

// R_ARM64_LDST128 sets a LD/ST immediate value to bits [11:4] of a local address.
public static readonly var R_ARM64_LDST128 = 40; 

// PPC64.

// R_POWER_TLS_LE is used to implement the "local exec" model for tls
// access. It resolves to the offset of the thread-local symbol from the
// thread pointer (R13) and is split against a pair of instructions to
// support a 32 bit displacement.
public static readonly var R_POWER_TLS_LE = 41; 

// R_POWER_TLS_IE is used to implement the "initial exec" model for tls access. It
// relocates a D-form, DS-form instruction sequence like R_ADDRPOWER_DS. It
// inserts to the offset of GOT slot for the thread-local symbol from the TOC (the
// GOT slot is filled by the dynamic linker with the offset of the thread-local
// symbol from the thread pointer (R13)).
public static readonly var R_POWER_TLS_IE = 42; 

// R_POWER_TLS marks an X-form instruction such as "ADD R3,R13,R4" as completing
// a sequence of GOT-relative relocations to compute a TLS address. This can be
// used by the system linker to to rewrite the GOT-relative TLS relocation into a
// simpler thread-pointer relative relocation. See table 3.26 and 3.28 in the
// ppc64 elfv2 1.4 ABI on this transformation.  Likewise, the second argument
// (usually called RB in X-form instructions) is assumed to be R13.
public static readonly var R_POWER_TLS = 43; 

// R_ADDRPOWER_DS is similar to R_ADDRPOWER above, but assumes the second
// instruction is a "DS-form" instruction, which has an immediate field occupying
// bits [15:2] of the instruction word. Bits [15:2] of the address of the
// relocated symbol are inserted into this field; it is an error if the last two
// bits of the address are not 0.
public static readonly var R_ADDRPOWER_DS = 44; 

// R_ADDRPOWER_PCREL relocates a D-form, DS-form instruction sequence like
// R_ADDRPOWER_DS but inserts the offset of the GOT slot for the referenced symbol
// from the TOC rather than the symbol's address.
public static readonly var R_ADDRPOWER_GOT = 45; 

// R_ADDRPOWER_PCREL relocates two D-form instructions like R_ADDRPOWER, but
// inserts the displacement from the place being relocated to the address of the
// relocated symbol instead of just its address.
public static readonly var R_ADDRPOWER_PCREL = 46; 

// R_ADDRPOWER_TOCREL relocates two D-form instructions like R_ADDRPOWER, but
// inserts the offset from the TOC to the address of the relocated symbol
// rather than the symbol's address.
public static readonly var R_ADDRPOWER_TOCREL = 47; 

// R_ADDRPOWER_TOCREL relocates a D-form, DS-form instruction sequence like
// R_ADDRPOWER_DS but inserts the offset from the TOC to the address of the
// relocated symbol rather than the symbol's address.
public static readonly var R_ADDRPOWER_TOCREL_DS = 48; 

// RISC-V.

// R_RISCV_PCREL_ITYPE resolves a 32-bit PC-relative address using an
// AUIPC + I-type instruction pair.
public static readonly var R_RISCV_PCREL_ITYPE = 49; 

// R_RISCV_PCREL_STYPE resolves a 32-bit PC-relative address using an
// AUIPC + S-type instruction pair.
public static readonly var R_RISCV_PCREL_STYPE = 50; 

// R_RISCV_TLS_IE_ITYPE resolves a 32-bit TLS initial-exec TOC offset
// address using an AUIPC + I-type instruction pair.
public static readonly var R_RISCV_TLS_IE_ITYPE = 51; 

// R_RISCV_TLS_IE_STYPE resolves a 32-bit TLS initial-exec TOC offset
// address using an AUIPC + S-type instruction pair.
public static readonly var R_RISCV_TLS_IE_STYPE = 52; 

// R_PCRELDBL relocates s390x 2-byte aligned PC-relative addresses.
// TODO(mundaym): remove once variants can be serialized - see issue 14218.
public static readonly var R_PCRELDBL = 53; 

// R_ADDRMIPSU (only used on mips/mips64) resolves to the sign-adjusted "upper" 16
// bits (bit 16-31) of an external address, by encoding it into the instruction.
public static readonly var R_ADDRMIPSU = 54; 
// R_ADDRMIPSTLS (only used on mips64) resolves to the low 16 bits of a TLS
// address (offset from thread pointer), by encoding it into the instruction.
public static readonly var R_ADDRMIPSTLS = 55; 

// R_ADDRCUOFF resolves to a pointer-sized offset from the start of the
// symbol's DWARF compile unit.
public static readonly var R_ADDRCUOFF = 56; 

// R_WASMIMPORT resolves to the index of the WebAssembly function import.
public static readonly var R_WASMIMPORT = 57; 

// R_XCOFFREF (only used on aix/ppc64) prevents garbage collection by ld
// of a symbol. This isn't a real relocation, it can be placed in anywhere
// in a symbol and target any symbols.
public static readonly R_WEAK R_XCOFFREF = -1 << 15;

public static readonly var R_WEAKADDR = R_WEAK | R_ADDR;
public static readonly var R_WEAKADDROFF = R_WEAK | R_ADDROFF;

// IsDirectCall reports whether r is a relocation for a direct call.
// A direct call is a CALL instruction that takes the target address
// as an immediate. The address is embedded into the instruction, possibly
// with limited width. An indirect call is a CALL instruction that takes
// the target address in register or memory.
public static bool IsDirectCall(this RelocType r) {

    if (r == R_CALL || r == R_CALLARM || r == R_CALLARM64 || r == R_CALLMIPS || r == R_CALLPOWER || r == R_CALLRISCV) 
        return true;
        return false;
}

// IsDirectJump reports whether r is a relocation for a direct jump.
// A direct jump is a JMP instruction that takes the target address
// as an immediate. The address is embedded into the instruction, possibly
// with limited width. An indirect jump is a JMP instruction that takes
// the target address in register or memory.
public static bool IsDirectJump(this RelocType r) {

    if (r == R_JMPMIPS) 
        return true;
        return false;
}

// IsDirectCallOrJump reports whether r is a relocation for a direct
// call or a direct jump.
public static bool IsDirectCallOrJump(this RelocType r) {
    return r.IsDirectCall() || r.IsDirectJump();
}

} // end objabi_package
