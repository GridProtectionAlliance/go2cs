// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cpu implements processor feature detection for
// various CPU architectures.
// package cpu -- go2cs converted at 2020 October 08 05:01:48 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // Initialized reports whether the CPU features were initialized.
        //
        // For some GOOS/GOARCH combinations initialization of the CPU features depends
        // on reading an operating specific file, e.g. /proc/self/auxv on linux/arm
        // Initialized will report false if reading the file fails.
        public static bool Initialized = default;

        // CacheLinePad is used to pad structs to avoid false sharing.
        public partial struct CacheLinePad
        {
            public array<byte> _;
        }

        // X86 contains the supported CPU features of the
        // current X86/AMD64 platform. If the current platform
        // is not X86/AMD64 then all feature flags are false.
        //
        // X86 is padded to avoid false sharing. Further the HasAVX
        // and HasAVX2 are only set if the OS supports XMM and YMM
        // registers in addition to the CPUID feature bit being set.
        public static var X86 = default;

        // ARM64 contains the supported CPU features of the
        // current ARMv8(aarch64) platform. If the current platform
        // is not arm64 then all feature flags are false.
        public static var ARM64 = default;

        // ARM contains the supported CPU features of the current ARM (32-bit) platform.
        // All feature flags are false if:
        //   1. the current platform is not arm, or
        //   2. the current operating system is not Linux.
        public static var ARM = default;

        // MIPS64X contains the supported CPU features of the current mips64/mips64le
        // platforms. If the current platform is not mips64/mips64le or the current
        // operating system is not Linux then all feature flags are false.
        public static var MIPS64X = default;

        // PPC64 contains the supported CPU features of the current ppc64/ppc64le platforms.
        // If the current platform is not ppc64/ppc64le then all feature flags are false.
        //
        // For ppc64/ppc64le, it is safe to check only for ISA level starting on ISA v3.00,
        // since there are no optional categories. There are some exceptions that also
        // require kernel support to work (DARN, SCV), so there are feature bits for
        // those as well. The minimum processor requirement is POWER8 (ISA 2.07).
        // The struct is padded to avoid false sharing.
        public static var PPC64 = default;

        // S390X contains the supported CPU features of the current IBM Z
        // (s390x) platform. If the current platform is not IBM Z then all
        // feature flags are false.
        //
        // S390X is padded to avoid false sharing. Further HasVX is only set
        // if the OS supports vector registers in addition to the STFLE
        // feature bit being set.
        public static var S390X = default;
    }
}}}}}
