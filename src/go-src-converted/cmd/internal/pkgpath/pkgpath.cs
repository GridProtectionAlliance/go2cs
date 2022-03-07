// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package pkgpath determines the package path used by gccgo/GoLLVM symbols.
// This package is not used for the gc compiler.
// package pkgpath -- go2cs converted at 2022 March 06 22:47:31 UTC
// import "cmd/internal/pkgpath" ==> using pkgpath = go.cmd.@internal.pkgpath_package
// Original source: C:\Program Files\Go\src\cmd\internal\pkgpath\pkgpath.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using exec = go.@internal.execabs_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using strings = go.strings_package;
using System;


namespace go.cmd.@internal;

public static partial class pkgpath_package {

    // ToSymbolFunc returns a function that may be used to convert a
    // package path into a string suitable for use as a symbol.
    // cmd is the gccgo/GoLLVM compiler in use, and tmpdir is a temporary
    // directory to pass to ioutil.TempFile.
    // For example, this returns a function that converts "net/http"
    // into a string like "net..z2fhttp". The actual string varies for
    // different gccgo/GoLLVM versions, which is why this returns a function
    // that does the conversion appropriate for the compiler in use.
public static (Func<@string, @string>, error) ToSymbolFunc(@string cmd, @string tmpdir) => func((defer, _, _) => {
    Func<@string, @string> _p0 = default;
    error _p0 = default!;
 
    // To determine the scheme used by cmd, we compile a small
    // file and examine the assembly code. Older versions of gccgo
    // use a simple mangling scheme where there can be collisions
    // between packages whose paths are different but mangle to
    // the same string. More recent versions use a new mangler
    // that avoids these collisions.
    const @string filepat = "*_gccgo_manglechck.go";

    var (f, err) = ioutil.TempFile(tmpdir, filepat);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var gofilename = f.Name();
    f.Close();
    defer(os.Remove(gofilename));

    {
        var err = ioutil.WriteFile(gofilename, (slice<byte>)mangleCheckCode, 0644);

        if (err != null) {
            return (null, error.As(err)!);
        }
    }


    var command = exec.Command(cmd, "-S", "-o", "-", gofilename);
    var (buf, err) = command.Output();
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (bytes.Contains(buf, (slice<byte>)"go_0l_u00e4ufer.Run")) {
        return (toSymbolV3, error.As(null!)!);
    }
    else if (bytes.Contains(buf, (slice<byte>)"go.l..u00e4ufer.Run")) {
        return (toSymbolV2, error.As(null!)!);
    }
    else if (bytes.Contains(buf, (slice<byte>)"go.l__ufer.Run")) {
        return (toSymbolV1, error.As(null!)!);
    }
    else
 {
        return (null, error.As(errors.New(cmd + ": unrecognized mangling scheme"))!);
    }
});

// mangleCheckCode is the package we compile to determine the mangling scheme.
private static readonly @string mangleCheckCode = "\npackage läufer\nfunc Run(x int) int {\n  return 1\n}\n";

// toSymbolV1 converts a package path using the original mangling scheme.


// toSymbolV1 converts a package path using the original mangling scheme.
private static @string toSymbolV1(@string ppath) {
    Func<int, int> clean = r => {

        if ('A' <= r && r <= 'Z' || 'a' <= r && r <= 'z' || '0' <= r && r <= '9') 
            return r;
                return '_';

    };
    return strings.Map(clean, ppath);

}

// toSymbolV2 converts a package path using the second mangling scheme.
private static @string toSymbolV2(@string ppath) { 
    // This has to build at boostrap time, so it has to build
    // with Go 1.4, so we don't use strings.Builder.
    var bsl = make_slice<byte>(0, len(ppath));
    var changed = false;
    foreach (var (_, c) in ppath) {
        if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z') || ('0' <= c && c <= '9') || c == '_') {
            bsl = append(bsl, byte(c));
            continue;
        }
        @string enc = default;

        if (c == '.') 
            enc = ".x2e";
        else if (c < 0x80) 
            enc = fmt.Sprintf("..z%02x", c);
        else if (c < 0x10000) 
            enc = fmt.Sprintf("..u%04x", c);
        else 
            enc = fmt.Sprintf("..U%08x", c);
                bsl = append(bsl, enc);
        changed = true;

    }    if (!changed) {
        return ppath;
    }
    return string(bsl);

}

// v3UnderscoreCodes maps from a character that supports an underscore
// encoding to the underscore encoding character.
private static map v3UnderscoreCodes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<byte, byte>{'_':'_','.':'0','/':'1','*':'2',',':'3','{':'4','}':'5','[':'6',']':'7','(':'8',')':'9','"':'a',' ':'b',';':'c',};

// toSymbolV3 converts a package path using the third mangling scheme.
private static @string toSymbolV3(@string ppath) { 
    // This has to build at boostrap time, so it has to build
    // with Go 1.4, so we don't use strings.Builder.
    var bsl = make_slice<byte>(0, len(ppath));
    var changed = false;
    foreach (var (_, c) in ppath) {
        if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z') || ('0' <= c && c <= '9')) {
            bsl = append(bsl, byte(c));
            continue;
        }
        if (c < 0x80) {
            {
                var (u, ok) = v3UnderscoreCodes[byte(c)];

                if (ok) {
                    bsl = append(bsl, '_', u);
                    changed = true;
                    continue;
                }

            }

        }
        @string enc = default;

        if (c < 0x80) 
            enc = fmt.Sprintf("_x%02x", c);
        else if (c < 0x10000) 
            enc = fmt.Sprintf("_u%04x", c);
        else 
            enc = fmt.Sprintf("_U%08x", c);
                bsl = append(bsl, enc);
        changed = true;

    }    if (!changed) {
        return ppath;
    }
    return string(bsl);

}

} // end pkgpath_package
