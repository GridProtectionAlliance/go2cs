// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package flags implements top-level flags and the usage message for the assembler.

// package flags -- go2cs converted at 2022 March 13 05:54:20 UTC
// import "cmd/asm/internal/flags" ==> using flags = go.cmd.asm.@internal.flags_package
// Original source: C:\Program Files\Go\src\cmd\asm\internal\flags\flags.go
namespace go.cmd.asm.@internal;

using objabi = cmd.@internal.objabi_package;
using flag = flag_package;
using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;

public static partial class flags_package {

public static var Debug = flag.Bool("debug", false, "dump instructions as they are parsed");public static var OutputFile = flag.String("o", "", "output file; default foo.o for /a/b/c/foo.s as first argument");public static var TrimPath = flag.String("trimpath", "", "remove prefix from recorded source file paths");public static var Shared = flag.Bool("shared", false, "generate code that can be linked into a shared library");public static var Dynlink = flag.Bool("dynlink", false, "support references to Go symbols defined in other shared libraries");public static var Linkshared = flag.Bool("linkshared", false, "generate code that will be linked against Go shared libraries");public static var AllErrors = flag.Bool("e", false, "no limit on number of errors reported");public static var SymABIs = flag.Bool("gensymabis", false, "write symbol ABI information to output file, don't assemble");public static var Importpath = flag.String("p", "", "set expected package import to path");public static var Spectre = flag.String("spectre", "", "enable spectre mitigations in `list` (all, ret)");public static var CompilingRuntime = flag.Bool("compiling-runtime", false, "source to be compiled is part of the Go runtime");

public static MultiFlag D = default;public static MultiFlag I = default;public static nint PrintOut = default;public static bool DebugV = default;

private static void init() {
    flag.Var(_addr_D, "D", "predefined symbol with optional simple value -D=identifier=value; can be set multiple times");
    flag.Var(_addr_I, "I", "include directory; can be set multiple times");
    flag.BoolVar(_addr_DebugV, "v", false, "print debug output");
    objabi.AddVersionFlag(); // -V
    objabi.Flagcount("S", "print assembly and machine code", _addr_PrintOut);
}

// MultiFlag allows setting a value multiple times to collect a list, as in -I=dir1 -I=dir2.
public partial struct MultiFlag { // : slice<@string>
}

private static @string String(this ptr<MultiFlag> _addr_m) {
    ref MultiFlag m = ref _addr_m.val;

    if (len(m.val) == 0) {
        return "";
    }
    return fmt.Sprint(m.val);
}

private static error Set(this ptr<MultiFlag> _addr_m, @string val) {
    ref MultiFlag m = ref _addr_m.val;

    (m.val) = append(m.val, val);
    return error.As(null!)!;
}

public static void Usage() {
    fmt.Fprintf(os.Stderr, "usage: asm [options] file.s ...\n");
    fmt.Fprintf(os.Stderr, "Flags:\n");
    flag.PrintDefaults();
    os.Exit(2);
}

public static void Parse() {
    flag.Usage = Usage;
    flag.Parse();
    if (flag.NArg() == 0) {
        flag.Usage();
    }
    if (OutputFile == "".val) {
        if (flag.NArg() != 1) {
            flag.Usage();
        }
        var input = filepath.Base(flag.Arg(0));
        if (strings.HasSuffix(input, ".s")) {
            input = input[..(int)len(input) - 2];
        }
        OutputFile.val = fmt.Sprintf("%s.o", input);
    }
}

} // end flags_package
