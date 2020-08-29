// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cpu implements processor feature detection
// used by the Go standard library.
// package cpu -- go2cs converted at 2020 August 29 08:22:20 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Go\src\internal\cpu\cpu.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class cpu_package
    {
        public static x86 X86 = default;

        // The booleans in x86 contain the correspondingly named cpuid feature bit.
        // HasAVX and HasAVX2 are only set if the OS does support XMM and YMM registers
        // in addition to the cpuid feature bit being set.
        // The struct is padded to avoid false sharing.
        private partial struct x86
        {
            public array<byte> _;
            public bool HasAES;
            public bool HasADX;
            public bool HasAVX;
            public bool HasAVX2;
            public bool HasBMI1;
            public bool HasBMI2;
            public bool HasERMS;
            public bool HasFMA;
            public bool HasOSXSAVE;
            public bool HasPCLMULQDQ;
            public bool HasPOPCNT;
            public bool HasSSE2;
            public bool HasSSE3;
            public bool HasSSSE3;
            public bool HasSSE41;
            public bool HasSSE42;
            public array<byte> _;
        }

        public static ppc64 PPC64 = default;

        // For ppc64x, it is safe to check only for ISA level starting on ISA v3.00,
        // since there are no optional categories. There are some exceptions that also
        // require kernel support to work (darn, scv), so there are capability bits for
        // those as well. The minimum processor requirement is POWER8 (ISA 2.07), so we
        // maintain some of the old capability checks for optional categories for
        // safety.
        // The struct is padded to avoid false sharing.
        private partial struct ppc64
        {
            public array<byte> _;
            public bool HasVMX; // Vector unit (Altivec)
            public bool HasDFP; // Decimal Floating Point unit
            public bool HasVSX; // Vector-scalar unit
            public bool HasHTM; // Hardware Transactional Memory
            public bool HasISEL; // Integer select
            public bool HasVCRYPTO; // Vector cryptography
            public bool HasHTMNOSC; // HTM: kernel-aborted transaction in syscalls
            public bool HasDARN; // Hardware random number generator (requires kernel enablement)
            public bool HasSCV; // Syscall vectored (requires kernel enablement)
            public bool IsPOWER8; // ISA v2.07 (POWER8)
            public bool IsPOWER9; // ISA v3.00 (POWER9)
            public array<byte> _;
        }

        public static arm64 ARM64 = default;

        // The booleans in arm64 contain the correspondingly named cpu feature bit.
        // The struct is padded to avoid false sharing.
        private partial struct arm64
        {
            public array<byte> _;
            public bool HasFP;
            public bool HasASIMD;
            public bool HasEVTSTRM;
            public bool HasAES;
            public bool HasPMULL;
            public bool HasSHA1;
            public bool HasSHA2;
            public bool HasCRC32;
            public bool HasATOMICS;
            public array<byte> _;
        }
    }
}}
