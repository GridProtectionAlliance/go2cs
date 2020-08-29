// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2020 August 29 08:46:19 UTC
// import "cmd/internal/sys" ==> using sys = go.cmd.@internal.sys_package
// Original source: C:\Go\src\cmd\internal\sys\arch.go
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class sys_package
    {
        // ArchFamily represents a family of one or more related architectures.
        // For example, amd64 and amd64p32 are both members of the AMD64 family,
        // and ppc64 and ppc64le are both members of the PPC64 family.
        public partial struct ArchFamily // : byte
        {
        }

        public static readonly ArchFamily NoArch = iota;
        public static readonly var AMD64 = 0;
        public static readonly var ARM = 1;
        public static readonly var ARM64 = 2;
        public static readonly var I386 = 3;
        public static readonly var MIPS = 4;
        public static readonly var MIPS64 = 5;
        public static readonly var PPC64 = 6;
        public static readonly var S390X = 7;

        // Arch represents an individual architecture.
        public partial struct Arch
        {
            public @string Name;
            public ArchFamily Family;
            public binary.ByteOrder ByteOrder; // PtrSize is the size in bytes of pointers and the
// predeclared "int", "uint", and "uintptr" types.
            public long PtrSize; // RegSize is the size in bytes of general purpose registers.
            public long RegSize; // MinLC is the minimum length of an instruction code.
            public long MinLC;
        }

        // InFamily reports whether a is a member of any of the specified
        // architecture families.
        private static bool InFamily(this ref Arch a, params ArchFamily[] xs)
        {
            foreach (var (_, x) in xs)
            {
                if (a.Family == x)
                {
                    return true;
                }
            }
            return false;
        }

        public static Arch Arch386 = ref new Arch(Name:"386",Family:I386,ByteOrder:binary.LittleEndian,PtrSize:4,RegSize:4,MinLC:1,);

        public static Arch ArchAMD64 = ref new Arch(Name:"amd64",Family:AMD64,ByteOrder:binary.LittleEndian,PtrSize:8,RegSize:8,MinLC:1,);

        public static Arch ArchAMD64P32 = ref new Arch(Name:"amd64p32",Family:AMD64,ByteOrder:binary.LittleEndian,PtrSize:4,RegSize:8,MinLC:1,);

        public static Arch ArchARM = ref new Arch(Name:"arm",Family:ARM,ByteOrder:binary.LittleEndian,PtrSize:4,RegSize:4,MinLC:4,);

        public static Arch ArchARM64 = ref new Arch(Name:"arm64",Family:ARM64,ByteOrder:binary.LittleEndian,PtrSize:8,RegSize:8,MinLC:4,);

        public static Arch ArchMIPS = ref new Arch(Name:"mips",Family:MIPS,ByteOrder:binary.BigEndian,PtrSize:4,RegSize:4,MinLC:4,);

        public static Arch ArchMIPSLE = ref new Arch(Name:"mipsle",Family:MIPS,ByteOrder:binary.LittleEndian,PtrSize:4,RegSize:4,MinLC:4,);

        public static Arch ArchMIPS64 = ref new Arch(Name:"mips64",Family:MIPS64,ByteOrder:binary.BigEndian,PtrSize:8,RegSize:8,MinLC:4,);

        public static Arch ArchMIPS64LE = ref new Arch(Name:"mips64le",Family:MIPS64,ByteOrder:binary.LittleEndian,PtrSize:8,RegSize:8,MinLC:4,);

        public static Arch ArchPPC64 = ref new Arch(Name:"ppc64",Family:PPC64,ByteOrder:binary.BigEndian,PtrSize:8,RegSize:8,MinLC:4,);

        public static Arch ArchPPC64LE = ref new Arch(Name:"ppc64le",Family:PPC64,ByteOrder:binary.LittleEndian,PtrSize:8,RegSize:8,MinLC:4,);

        public static Arch ArchS390X = ref new Arch(Name:"s390x",Family:S390X,ByteOrder:binary.BigEndian,PtrSize:8,RegSize:8,MinLC:2,);

        public static array<ref Arch> Archs = new array<ref Arch>(new ref Arch[] { Arch386, ArchAMD64, ArchAMD64P32, ArchARM, ArchARM64, ArchMIPS, ArchMIPSLE, ArchMIPS64, ArchMIPS64LE, ArchPPC64, ArchPPC64LE, ArchS390X });
    }
}}}
