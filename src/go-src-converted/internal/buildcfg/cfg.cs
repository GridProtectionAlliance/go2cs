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

// package buildcfg -- go2cs converted at 2022 March 13 05:43:21 UTC
// import "internal/buildcfg" ==> using buildcfg = go.@internal.buildcfg_package
// Original source: C:\Program Files\Go\src\internal\buildcfg\cfg.go
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;

public static partial class buildcfg_package {

private static @string defaultGOROOT = default;public static var GOROOT = envOr("GOROOT", defaultGOROOT);public static var GOARCH = envOr("GOARCH", defaultGOARCH);public static var GOOS = envOr("GOOS", defaultGOOS);public static var GO386 = envOr("GO386", defaultGO386);public static var GOARM = goarm();public static var GOMIPS = gomips();public static var GOMIPS64 = gomips64();public static var GOPPC64 = goppc64();public static var GOWASM = gowasm();public static var GO_LDSO = defaultGO_LDSO;public static var Version = version;

// Error is one of the errors found (if any) in the build configuration.
public static error Error = default!;

// Check exits the program with a fatal error if Error is non-nil.
public static void Check() {
    if (Error != null) {
        fmt.Fprintf(os.Stderr, "%s: %v\n", filepath.Base(os.Args[0]), Error);
        os.Exit(2);
    }
}

private static @string envOr(@string key, @string value) {
    {
        var x = os.Getenv(key);

        if (x != "") {
            return x;
        }
    }
    return value;
}

private static nint goarm() {
    var def = defaultGOARM;
    if (GOOS == "android" && GOARCH == "arm") { 
        // Android arm devices always support GOARM=7.
        def = "7";
    }
    {
        var v = envOr("GOARM", def);

        switch (v) {
            case "5": 
                return 5;
                break;
            case "6": 
                return 6;
                break;
            case "7": 
                return 7;
                break;
        }
    }
    Error = fmt.Errorf("invalid GOARM: must be 5, 6, 7");
    return int(def[0] - '0');
}

private static @string gomips() {
    {
        var v = envOr("GOMIPS", defaultGOMIPS);

        switch (v) {
            case "hardfloat": 

            case "softfloat": 
                return v;
                break;
        }
    }
    Error = fmt.Errorf("invalid GOMIPS: must be hardfloat, softfloat");
    return defaultGOMIPS;
}

private static @string gomips64() {
    {
        var v = envOr("GOMIPS64", defaultGOMIPS64);

        switch (v) {
            case "hardfloat": 

            case "softfloat": 
                return v;
                break;
        }
    }
    Error = fmt.Errorf("invalid GOMIPS64: must be hardfloat, softfloat");
    return defaultGOMIPS64;
}

private static nint goppc64() {
    {
        var v = envOr("GOPPC64", defaultGOPPC64);

        switch (v) {
            case "power8": 
                return 8;
                break;
            case "power9": 
                return 9;
                break;
        }
    }
    Error = fmt.Errorf("invalid GOPPC64: must be power8, power9");
    return int(defaultGOPPC64[len("power")] - '0');
}

private partial struct gowasmFeatures {
    public bool SignExt;
    public bool SatConv;
}

private static @string String(this gowasmFeatures f) {
    slice<@string> flags = default;
    if (f.SatConv) {
        flags = append(flags, "satconv");
    }
    if (f.SignExt) {
        flags = append(flags, "signext");
    }
    return strings.Join(flags, ",");
}

private static gowasmFeatures gowasm() {
    gowasmFeatures f = default;

    foreach (var (_, opt) in strings.Split(envOr("GOWASM", ""), ",")) {
        switch (opt) {
            case "satconv": 
                f.SatConv = true;
                break;
            case "signext": 
                f.SignExt = true;
                break;
            case "": 

                break;
            default: 
                Error = fmt.Errorf("invalid GOWASM: no such feature %q", opt);
                break;
        }
    }    return ;
}

public static @string Getgoextlinkenabled() {
    return envOr("GO_EXTLINK_ENABLED", defaultGO_EXTLINK_ENABLED);
}

} // end buildcfg_package
