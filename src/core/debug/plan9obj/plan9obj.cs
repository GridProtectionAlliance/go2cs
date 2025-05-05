// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
/*
 * Plan 9 a.out constants and data structures
 */
namespace go.debug;

partial class plan9obj_package {

// Plan 9 Program header.
[GoType] partial struct prog {
    public uint32 Magic; /* magic number */
    public uint32 Text; /* size of text segment */
    public uint32 Data; /* size of initialized data */
    public uint32 Bss; /* size of uninitialized data */
    public uint32 Syms; /* size of symbol table */
    public uint32 Entry; /* entry point */
    public uint32 Spsz; /* size of pc/sp offset table */
    public uint32 Pcsz; /* size of pc/line number table */
}

// Plan 9 symbol table entries.
[GoType] partial struct sym {
    internal uint64 value;
    internal byte typ;
    internal slice<byte> name;
}

public static readonly UntypedInt Magic64 = /* 0x8000 */ 32768; // 64-bit expanded header
public static readonly UntypedInt Magic386 = /* (4*11+0)*11 + 7 */ 491;
public static readonly UntypedInt MagicAMD64 = /* (4*26+0)*26 + 7 + Magic64 */ 35479;
public static readonly UntypedInt MagicARM = /* (4*20+0)*20 + 7 */ 1607;

} // end plan9obj_package
