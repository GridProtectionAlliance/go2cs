// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2022 March 13 06:33:29 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\sym\reloc.go
namespace go.cmd.link.@internal;

using objabi = cmd.@internal.objabi_package;
using sys = cmd.@internal.sys_package;
using elf = debug.elf_package;


// RelocVariant is a linker-internal variation on a relocation.

public static partial class sym_package {

public partial struct RelocVariant { // : byte
}

public static readonly RelocVariant RV_NONE = iota;
public static readonly var RV_POWER_LO = 0;
public static readonly var RV_POWER_HI = 1;
public static readonly var RV_POWER_HA = 2;
public static readonly var RV_POWER_DS = 3; 

// RV_390_DBL is a s390x-specific relocation variant that indicates that
// the value to be placed into the relocatable field should first be
// divided by 2.
public static readonly var RV_390_DBL = 4;

public static readonly RelocVariant RV_CHECK_OVERFLOW = 1 << 7;
public static readonly RelocVariant RV_TYPE_MASK = RV_CHECK_OVERFLOW - 1;

public static @string RelocName(ptr<sys.Arch> _addr_arch, objabi.RelocType r) => func((_, panic, _) => {
    ref sys.Arch arch = ref _addr_arch.val;
 
    // We didn't have some relocation types at Go1.4.
    // Uncomment code when we include those in bootstrap code.


    if (r >= objabi.MachoRelocOffset)     else if (r >= objabi.ElfRelocOffset) // ELF
        var nr = r - objabi.ElfRelocOffset;

        if (arch.Family == sys.AMD64) 
            return elf.R_X86_64(nr).String();
        else if (arch.Family == sys.ARM) 
            return elf.R_ARM(nr).String();
        else if (arch.Family == sys.ARM64) 
            return elf.R_AARCH64(nr).String();
        else if (arch.Family == sys.I386) 
            return elf.R_386(nr).String();
        else if (arch.Family == sys.MIPS || arch.Family == sys.MIPS64) 
            return elf.R_MIPS(nr).String();
        else if (arch.Family == sys.PPC64) 
            return elf.R_PPC64(nr).String();
        else if (arch.Family == sys.S390X) 
            return elf.R_390(nr).String();
        else 
            panic("unreachable");
                return r.String();
});

} // end sym_package
