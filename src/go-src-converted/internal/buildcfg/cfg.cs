// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buildcfg provides access to the build configuration
// described by the current environment. It is for use by build tools
// such as cmd/go or cmd/compile and for setting up go/build's Default context.
//
// Note that it does NOT provide access to the build configuration used to
// build the currently-running binary. For that, use runtime.GOOS etc
// as well as internal/goexperiment.
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using strings = strings_package;
using path;

partial class buildcfg_package {

public static @string GOROOT = os.Getenv("GOROOT"u8); // cached for efficiency
public static @string GOARCH = envOr("GOARCH"u8, defaultGOARCH);
public static @string GOOS = envOr("GOOS"u8, defaultGOOS);
public static @string GO386 = envOr("GO386"u8, defaultGO386);
public static nint GOAMD64 = goamd64();
public static goarmFeatures GOARM = goarm();
public static Goarm64Features GOARM64 = goarm64();
public static @string GOMIPS = gomips();
public static @string GOMIPS64 = gomips64();
public static nint GOPPC64 = goppc64();
public static nint GORISCV64 = goriscv64();
public static gowasmFeatures GOWASM = gowasm();
public static slice<@string> ToolTags = toolTags();
public static @string GO_LDSO = defaultGO_LDSO;
public static @string Version = version;

// Error is one of the errors found (if any) in the build configuration.
public static error Error;

// Check exits the program with a fatal error if Error is non-nil.
public static void Check() {
    if (Error != default!) {
        fmt.Fprintf(~os.Stderr, "%s: %v\n"u8, filepath.Base(os.Args[0]), Error);
        os.Exit(2);
    }
}

internal static @string envOr(@string key, @string value) {
    {
        @string x = os.Getenv(key); if (x != ""u8) {
            return x;
        }
    }
    return value;
}

internal static nint goamd64() {
    {
        @string v = envOr("GOAMD64"u8, defaultGOAMD64);
        var exprᴛ1 = v;
        if (exprᴛ1 == "v1"u8) {
            return 1;
        }
        if (exprᴛ1 == "v2"u8) {
            return 2;
        }
        if (exprᴛ1 == "v3"u8) {
            return 3;
        }
        if (exprᴛ1 == "v4"u8) {
            return 4;
        }
    }

    Error = fmt.Errorf("invalid GOAMD64: must be v1, v2, v3, v4"u8);
    return ((nint)(defaultGOAMD64[len("v")] - (rune)'0'));
}

[GoType] partial struct goarmFeatures {
    public nint Version;
    public bool SoftFloat;
}

internal static @string String(this goarmFeatures g) {
    @string armStr = strconv.Itoa(g.Version);
    if (g.SoftFloat){
        armStr += ",softfloat"u8;
    } else {
        armStr += ",hardfloat"u8;
    }
    return armStr;
}

internal static goarmFeatures /*g*/ goarm() {
    goarmFeatures g = default!;

    @string softFloatOpt = ",softfloat"u8;
    @string hardFloatOpt = ",hardfloat"u8;
    @string def = defaultGOARM;
    if (GOOS == "android"u8 && GOARCH == "arm"u8) {
        // Android arm devices always support GOARM=7.
        def = "7"u8;
    }
    @string v = envOr("GOARM"u8, def);
    var floatSpecified = false;
    if (strings.HasSuffix(v, softFloatOpt)) {
        g.SoftFloat = true;
        floatSpecified = true;
        v = v[..(int)(len(v) - len(softFloatOpt))];
    }
    if (strings.HasSuffix(v, hardFloatOpt)) {
        floatSpecified = true;
        v = v[..(int)(len(v) - len(hardFloatOpt))];
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == "5"u8) {
        g.Version = 5;
    }
    else if (exprᴛ1 == "6"u8) {
        g.Version = 6;
    }
    else if (exprᴛ1 == "7"u8) {
        g.Version = 7;
    }
    else { /* default: */
        Error = fmt.Errorf("invalid GOARM: must start with 5, 6, or 7, and may optionally end in either %q or %q"u8, hardFloatOpt, softFloatOpt);
        g.Version = ((nint)(def[0] - (rune)'0'));
    }

    // 5 defaults to softfloat. 6 and 7 default to hardfloat.
    if (!floatSpecified && g.Version == 5) {
        g.SoftFloat = true;
    }
    return g;
}

[GoType] partial struct Goarm64Features {
    public @string Version;
    // Large Systems Extension
    public bool LSE;
    // ARM v8.0 Cryptographic Extension. It includes the following features:
    // * FEAT_AES, which includes the AESD and AESE instructions.
    // * FEAT_PMULL, which includes the PMULL, PMULL2 instructions.
    // * FEAT_SHA1, which includes the SHA1* instructions.
    // * FEAT_SHA256, which includes the SHA256* instructions.
    public bool Crypto;
}

public static @string String(this Goarm64Features g) {
    @string arm64Str = g.Version;
    if (g.LSE) {
        arm64Str += ",lse"u8;
    }
    if (g.Crypto) {
        arm64Str += ",crypto"u8;
    }
    return arm64Str;
}

public static (Goarm64Features g, error e) ParseGoarm64(@string v) {
    Goarm64Features g = default!;
    error e = default!;

    @string lseOpt = ",lse"u8;
    @string cryptoOpt = ",crypto"u8;
    g.LSE = false;
    g.Crypto = false;
    // We allow any combination of suffixes, in any order
    while (ᐧ) {
        if (strings.HasSuffix(v, lseOpt)) {
            g.LSE = true;
            v = v[..(int)(len(v) - len(lseOpt))];
            continue;
        }
        if (strings.HasSuffix(v, cryptoOpt)) {
            g.Crypto = true;
            v = v[..(int)(len(v) - len(cryptoOpt))];
            continue;
        }
        break;
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == "v8.0"u8) {
        g.Version = v;
    }
    else if (exprᴛ1 == "v8.1"u8 || exprᴛ1 == "v8.2"u8 || exprᴛ1 == "v8.3"u8 || exprᴛ1 == "v8.4"u8 || exprᴛ1 == "v8.5"u8 || exprᴛ1 == "v8.6"u8 || exprᴛ1 == "v8.7"u8 || exprᴛ1 == "v8.8"u8 || exprᴛ1 == "v8.9"u8 || exprᴛ1 == "v9.0"u8 || exprᴛ1 == "v9.1"u8 || exprᴛ1 == "v9.2"u8 || exprᴛ1 == "v9.3"u8 || exprᴛ1 == "v9.4"u8 || exprᴛ1 == "v9.5"u8) {
        g.Version = v;
        g.LSE = true;
    }
    else { /* default: */
        e = fmt.Errorf("invalid GOARM64: must start with v8.{0-9} or v9.{0-5} and may optionally end in %q and/or %q"u8, // LSE extension is mandatory starting from 8.1

            lseOpt, cryptoOpt);
        g.Version = defaultGOARM64;
    }

    return (g, e);
}

internal static Goarm64Features /*g*/ goarm64() {
    Goarm64Features g = default!;

    (g, Error) = ParseGoarm64(envOr("GOARM64"u8, defaultGOARM64));
    return g;
}

// Returns true if g supports giving ARM64 ISA
// Note that this function doesn't accept / test suffixes (like ",lse" or ",crypto")
public static bool Supports(this Goarm64Features g, @string s) {
    // We only accept "v{8-9}.{0-9}. Everything else is malformed.
    if (len(s) != 4) {
        return false;
    }
    var major = s[1];
    var minor = s[3];
    // We only accept "v{8-9}.{0-9}. Everything else is malformed.
    if (major < (rune)'8' || major > (rune)'9' || minor < (rune)'0' || minor > (rune)'9' || s[0] != (rune)'v' || s[2] != (rune)'.') {
        return false;
    }
    var g_major = g.Version[1];
    var g_minor = g.Version[3];
    if (major == g_major){
        return minor <= g_minor;
    } else 
    if (g_major == (rune)'9'){
        // v9.0 diverged from v8.5. This means we should compare with g_minor increased by five.
        return minor <= g_minor + 5;
    } else {
        return false;
    }
}

internal static @string gomips() {
    {
        @string v = envOr("GOMIPS"u8, defaultGOMIPS);
        var exprᴛ1 = v;
        if (exprᴛ1 == "hardfloat"u8 || exprᴛ1 == "softfloat"u8) {
            return v;
        }
    }

    Error = fmt.Errorf("invalid GOMIPS: must be hardfloat, softfloat"u8);
    return defaultGOMIPS;
}

internal static @string gomips64() {
    {
        @string v = envOr("GOMIPS64"u8, defaultGOMIPS64);
        var exprᴛ1 = v;
        if (exprᴛ1 == "hardfloat"u8 || exprᴛ1 == "softfloat"u8) {
            return v;
        }
    }

    Error = fmt.Errorf("invalid GOMIPS64: must be hardfloat, softfloat"u8);
    return defaultGOMIPS64;
}

internal static nint goppc64() {
    {
        @string v = envOr("GOPPC64"u8, defaultGOPPC64);
        var exprᴛ1 = v;
        if (exprᴛ1 == "power8"u8) {
            return 8;
        }
        if (exprᴛ1 == "power9"u8) {
            return 9;
        }
        if (exprᴛ1 == "power10"u8) {
            return 10;
        }
    }

    Error = fmt.Errorf("invalid GOPPC64: must be power8, power9, power10"u8);
    return ((nint)(defaultGOPPC64[len("power")] - (rune)'0'));
}

internal static nint goriscv64() {
    {
        @string vΔ1 = envOr("GORISCV64"u8, defaultGORISCV64);
        var exprᴛ1 = vΔ1;
        if (exprᴛ1 == "rva20u64"u8) {
            return 20;
        }
        if (exprᴛ1 == "rva22u64"u8) {
            return 22;
        }
    }

    Error = fmt.Errorf("invalid GORISCV64: must be rva20u64, rva22u64"u8);
    @string v = defaultGORISCV64[(int)(len("rva"))..];
    nint i = strings.IndexFunc(v, (rune r) => r < (rune)'0' || r > (rune)'9');
    var (year, _) = strconv.Atoi(v[..(int)(i)]);
    return year;
}

[GoType] partial struct gowasmFeatures {
    public bool SatConv;
    public bool SignExt;
}

internal static @string String(this gowasmFeatures f) {
    slice<@string> flags = default!;
    if (f.SatConv) {
        flags = append(flags, "satconv"u8);
    }
    if (f.SignExt) {
        flags = append(flags, "signext"u8);
    }
    return strings.Join(flags, ","u8);
}

internal static gowasmFeatures /*f*/ gowasm() {
    gowasmFeatures f = default!;

    foreach (var (_, opt) in strings.Split(envOr("GOWASM"u8, ""u8), ","u8)) {
        var exprᴛ1 = opt;
        if (exprᴛ1 == "satconv"u8) {
            f.SatConv = true;
        }
        else if (exprᴛ1 == "signext"u8) {
            f.SignExt = true;
        }
        else if (exprᴛ1 == ""u8) {
        }
        else { /* default: */
            Error = fmt.Errorf("invalid GOWASM: no such feature %q"u8, // ignore
 opt);
        }

    }
    return f;
}

public static @string Getgoextlinkenabled() {
    return envOr("GO_EXTLINK_ENABLED"u8, defaultGO_EXTLINK_ENABLED);
}

internal static slice<@string> toolTags() {
    var tags = experimentTags();
    tags = append(tags, gogoarchTags().ꓸꓸꓸ);
    return tags;
}

internal static slice<@string> experimentTags() {
    slice<@string> list = default!;
    // For each experiment that has been enabled in the toolchain, define a
    // build tag with the same name but prefixed by "goexperiment." which can be
    // used for compiling alternative files for the experiment. This allows
    // changes for the experiment, like extra struct fields in the runtime,
    // without affecting the base non-experiment code at all.
    foreach (var (_, exp) in Experiment.Enabled()) {
        list = append(list, "goexperiment."u8 + exp);
    }
    return list;
}

// GOGOARCH returns the name and value of the GO$GOARCH setting.
// For example, if GOARCH is "amd64" it might return "GOAMD64", "v2".
public static (@string name, @string value) GOGOARCH() {
    @string name = default!;
    @string value = default!;

    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "386"u8) {
        return ("GO386", GO386);
    }
    if (exprᴛ1 == "amd64"u8) {
        return ("GOAMD64", fmt.Sprintf("v%d"u8, GOAMD64));
    }
    if (exprᴛ1 == "arm"u8) {
        return ("GOARM", GOARM.String());
    }
    if (exprᴛ1 == "arm64"u8) {
        return ("GOARM64", GOARM64.String());
    }
    if (exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8) {
        return ("GOMIPS", GOMIPS);
    }
    if (exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8) {
        return ("GOMIPS64", GOMIPS64);
    }
    if (exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8) {
        return ("GOPPC64", fmt.Sprintf("power%d"u8, GOPPC64));
    }
    if (exprᴛ1 == "wasm"u8) {
        return ("GOWASM", GOWASM.String());
    }

    return ("", "");
}

internal static slice<@string> gogoarchTags() {
    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "386"u8) {
        return new @string[]{GOARCH + "."u8 + GO386}.slice();
    }
    if (exprᴛ1 == "amd64"u8) {
        slice<@string> listΔ5 = default!;
        for (nint i = 1; i <= GOAMD64; i++) {
            list = append(listΔ5, fmt.Sprintf("%s.v%d"u8, GOARCH, i));
        }
        return listΔ5;
    }
    if (exprᴛ1 == "arm"u8) {
        slice<@string> listΔ6 = default!;
        for (nint i = 5; i <= GOARM.Version; i++) {
            list = append(listΔ6, fmt.Sprintf("%s.%d"u8, GOARCH, i));
        }
        return listΔ6;
    }
    if (exprᴛ1 == "arm64"u8) {
        slice<@string> listΔ7 = default!;
        nint major = ((nint)(GOARM64.Version[1] - (rune)'0'));
        nint minor = ((nint)(GOARM64.Version[3] - (rune)'0'));
        for (nint i = 0; i <= minor; i++) {
            list = append(listΔ7, fmt.Sprintf("%s.v%d.%d"u8, GOARCH, major, i));
        }
        if (major == 9) {
            // ARM64 v9.x also includes support of v8.x+5 (i.e. v9.1 includes v8.(1+5) = v8.6).
            for (nint i = 0; i <= minor + 5 && i <= 9; i++) {
                list = append(listΔ7, fmt.Sprintf("%s.v%d.%d"u8, GOARCH, 8, i));
            }
        }
        return listΔ7;
    }
    if (exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8) {
        return new @string[]{GOARCH + "."u8 + GOMIPS}.slice();
    }
    if (exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8) {
        return new @string[]{GOARCH + "."u8 + GOMIPS64}.slice();
    }
    if (exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8) {
        slice<@string> listΔ8 = default!;
        for (nint i = 8; i <= GOPPC64; i++) {
            list = append(listΔ8, fmt.Sprintf("%s.power%d"u8, GOARCH, i));
        }
        return listΔ8;
    }
    if (exprᴛ1 == "riscv64"u8) {
        var listΔ9 = new @string[]{GOARCH + "."u8 + "rva20u64"u8}.slice();
        if (GORISCV64 >= 22) {
            list = append(listΔ9, GOARCH + "."u8 + "rva22u64"u8);
        }
        return listΔ9;
    }
    if (exprᴛ1 == "wasm"u8) {
        slice<@string> list = default!;
        if (GOWASM.SatConv) {
            list = append(list, GOARCH + ".satconv"u8);
        }
        if (GOWASM.SignExt) {
            list = append(list, GOARCH + ".signext"u8);
        }
        return list;
    }

    return default!;
}

} // end buildcfg_package
