// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cpu implements processor feature detection for
// various CPU architectures.
// package cpu -- go2cs converted at 2022 March 06 23:38:18 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu.go
using os = go.os_package;
using strings = go.strings_package;

namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

    // Initialized reports whether the CPU features were initialized.
    //
    // For some GOOS/GOARCH combinations initialization of the CPU features depends
    // on reading an operating specific file, e.g. /proc/self/auxv on linux/arm
    // Initialized will report false if reading the file fails.
public static bool Initialized = default;

// CacheLinePad is used to pad structs to avoid false sharing.
public partial struct CacheLinePad {
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
// those as well. The struct is padded to avoid false sharing.
public static var PPC64 = default;

// S390X contains the supported CPU features of the current IBM Z
// (s390x) platform. If the current platform is not IBM Z then all
// feature flags are false.
//
// S390X is padded to avoid false sharing. Further HasVX is only set
// if the OS supports vector registers in addition to the STFLE
// feature bit being set.
public static var S390X = default;

private static void init() {
    archInit();
    initOptions();
    processOptions();
}

// options contains the cpu debug options that can be used in GODEBUG.
// Options are arch dependent and are added by the arch specific initOptions functions.
// Features that are mandatory for the specific GOARCH should have the Required field set
// (e.g. SSE2 on amd64).
private static slice<option> options = default;

// Option names should be lower case. e.g. avx instead of AVX.
private partial struct option {
    public @string Name;
    public ptr<bool> Feature;
    public bool Specified; // whether feature value was specified in GODEBUG
    public bool Enable; // whether feature should be enabled
    public bool Required; // whether feature is mandatory and can not be disabled
}

private static void processOptions() {
    var env = os.Getenv("GODEBUG");
field:

    while (env != "") {
        @string field = "";
        var i = strings.IndexByte(env, ',');
        if (i < 0) {
            (field, env) = (env, "");
        }
        else
 {
            (field, env) = (env[..(int)i], env[(int)i + 1..]);
        }
        if (len(field) < 4 || field[..(int)4] != "cpu.") {
            continue;
        }
        i = strings.IndexByte(field, '=');
        if (i < 0) {
            print("GODEBUG sys/cpu: no value specified for \"", field, "\"\n");
            continue;
        }
        var key = field[(int)4..(int)i];
        var value = field[(int)i + 1..]; // e.g. "SSE2", "on"

        bool enable = default;
        switch (value) {
            case "on": 
                enable = true;
                break;
            case "off": 
                enable = false;
                break;
            default: 
                print("GODEBUG sys/cpu: value \"", value, "\" not supported for cpu option \"", key, "\"\n");
                _continuefield = true;
                break;
                break;
        }

        if (key == "all") {
            {
                var i__prev2 = i;

                foreach (var (__i) in options) {
                    i = __i;
                    options[i].Specified = true;
                    options[i].Enable = enable || options[i].Required;
                }

                i = i__prev2;
            }

            _continuefield = true;
            break;
        }
        {
            var i__prev2 = i;

            foreach (var (__i) in options) {
                i = __i;
                if (options[i].Name == key) {
                    options[i].Specified = true;
                    options[i].Enable = enable;
                    _continuefield = true;
                    break;
                }

            }

            i = i__prev2;
        }

        print("GODEBUG sys/cpu: unknown cpu feature \"", key, "\"\n");

    }
    foreach (var (_, o) in options) {
        if (!o.Specified) {
            continue;
        }
        if (o.Enable && !o.Feature.val) {
            print("GODEBUG sys/cpu: can not enable \"", o.Name, "\", missing CPU support\n");
            continue;
        }
        if (!o.Enable && o.Required) {
            print("GODEBUG sys/cpu: can not disable \"", o.Name, "\", required CPU feature\n");
            continue;
        }
        o.Feature.val = o.Enable;

    }
}

} // end cpu_package
