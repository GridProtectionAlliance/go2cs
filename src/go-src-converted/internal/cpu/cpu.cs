// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cpu implements processor feature detection
// used by the Go standard library.
// package cpu -- go2cs converted at 2022 March 06 22:08:06 UTC
// import "internal/cpu" ==> using cpu = go.@internal.cpu_package
// Original source: C:\Program Files\Go\src\internal\cpu\cpu.go


namespace go.@internal;

public static partial class cpu_package {

    // DebugOptions is set to true by the runtime if the OS supports reading
    // GODEBUG early in runtime startup.
    // This should not be changed after it is initialized.
public static bool DebugOptions = default;

// CacheLinePad is used to pad structs to avoid false sharing.
public partial struct CacheLinePad {
    public array<byte> _;
}

// CacheLineSize is the CPU's assumed cache line size.
// There is currently no runtime detection of the real cache line size
// so we use the constant per GOARCH CacheLinePadSize as an approximation.
public static System.UIntPtr CacheLineSize = CacheLinePadSize;

// The booleans in X86 contain the correspondingly named cpuid feature bit.
// HasAVX and HasAVX2 are only set if the OS does support XMM and YMM registers
// in addition to the cpuid feature bit being set.
// The struct is padded to avoid false sharing.
public static var X86 = default;

// The booleans in ARM contain the correspondingly named cpu feature bit.
// The struct is padded to avoid false sharing.
public static var ARM = default;

// The booleans in ARM64 contain the correspondingly named cpu feature bit.
// The struct is padded to avoid false sharing.
public static var ARM64 = default;

public static var MIPS64X = default;

// For ppc64(le), it is safe to check only for ISA level starting on ISA v3.00,
// since there are no optional categories. There are some exceptions that also
// require kernel support to work (darn, scv), so there are feature bits for
// those as well. The minimum processor requirement is POWER8 (ISA 2.07).
// The struct is padded to avoid false sharing.
public static var PPC64 = default;

public static var S390X = default;

// Initialize examines the processor and sets the relevant variables above.
// This is called by the runtime package early in program initialization,
// before normal init functions are run. env is set by runtime if the OS supports
// cpu feature options in GODEBUG.
public static void Initialize(@string env) {
    doinit();
    processOptions(env);
}

// options contains the cpu debug options that can be used in GODEBUG.
// Options are arch dependent and are added by the arch specific doinit functions.
// Features that are mandatory for the specific GOARCH should not be added to options
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

// processOptions enables or disables CPU feature values based on the parsed env string.
// The env string is expected to be of the form cpu.feature1=value1,cpu.feature2=value2...
// where feature names is one of the architecture specific list stored in the
// cpu packages options variable and values are either 'on' or 'off'.
// If env contains cpu.all=off then all cpu features referenced through the options
// variable are disabled. Other feature names and values result in warning messages.
private static void processOptions(@string env) {
field:

    while (env != "") {
        @string field = "";
        var i = indexByte(env, ',');
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
        i = indexByte(field, '=');
        if (i < 0) {
            print("GODEBUG: no value specified for \"", field, "\"\n");
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
                print("GODEBUG: value \"", value, "\" not supported for cpu option \"", key, "\"\n");
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

        print("GODEBUG: unknown cpu feature \"", key, "\"\n");

    }
    foreach (var (_, o) in options) {
        if (!o.Specified) {
            continue;
        }
        if (o.Enable && !o.Feature.val) {
            print("GODEBUG: can not enable \"", o.Name, "\", missing CPU support\n");
            continue;
        }
        if (!o.Enable && o.Required) {
            print("GODEBUG: can not disable \"", o.Name, "\", required CPU feature\n");
            continue;
        }
        o.Feature.val = o.Enable;

    }
}

// indexByte returns the index of the first instance of c in s,
// or -1 if c is not present in s.
private static nint indexByte(@string s, byte c) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == c) {
            return i;
        }
    }
    return -1;

}

} // end cpu_package
