// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Objdump disassembles executable files.
//
// Usage:
//
//    go tool objdump [-s symregexp] binary
//
// Objdump prints a disassembly of all text symbols (code) in the binary.
// If the -s option is present, objdump only disassembles
// symbols with names matching the regular expression.
//
// Alternate usage:
//
//    go tool objdump binary start end
//
// In this mode, objdump disassembles the binary starting at the start address and
// stopping at the end address. The start and end addresses are program
// counters written in hexadecimal with optional leading 0x prefix.
// In this mode, objdump prints a sequence of stanzas of the form:
//
//    file:line
//     address: assembly
//     address: assembly
//     ...
//
// Each stanza gives the disassembly for a contiguous range of addresses
// all mapped to the same original source file and line number.
// This mode is intended for use by pprof.

// package main -- go2cs converted at 2022 March 13 06:35:40 UTC
// Original source: C:\Program Files\Go\src\cmd\objdump\main.go
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using log = log_package;
using os = os_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;

using objfile = cmd.@internal.objfile_package;

public static partial class main_package {

private static var printCode = flag.Bool("S", false, "print Go code alongside assembly");
private static var symregexp = flag.String("s", "", "only dump symbols matching this regexp");
private static var gnuAsm = flag.Bool("gnu", false, "print GNU assembly next to Go assembly (where supported)");
private static ptr<regexp.Regexp> symRE;

private static void usage() {
    fmt.Fprintf(os.Stderr, "usage: go tool objdump [-S] [-gnu] [-s symregexp] binary [start end]\n\n");
    flag.PrintDefaults();
    os.Exit(2);
}

private static void Main() => func((defer, _, _) => {
    log.SetFlags(0);
    log.SetPrefix("objdump: ");

    flag.Usage = usage;
    flag.Parse();
    if (flag.NArg() != 1 && flag.NArg() != 3) {
        usage();
    }
    if (symregexp != "".val) {
        var (re, err) = regexp.Compile(symregexp.val);
        if (err != null) {
            log.Fatalf("invalid -s regexp: %v", err);
        }
        symRE = re;
    }
    var (f, err) = objfile.Open(flag.Arg(0));
    if (err != null) {
        log.Fatal(err);
    }
    defer(f.Close());

    var (dis, err) = f.Disasm();
    if (err != null) {
        log.Fatalf("disassemble %s: %v", flag.Arg(0), err);
    }
    switch (flag.NArg()) {
        case 1: 
            // disassembly of entire object
            dis.Print(os.Stdout, symRE, 0, ~uint64(0), printCode.val, gnuAsm.val);
            break;
        case 3: 
            // disassembly of PC range
            var (start, err) = strconv.ParseUint(strings.TrimPrefix(flag.Arg(1), "0x"), 16, 64);
            if (err != null) {
                log.Fatalf("invalid start PC: %v", err);
            }
            var (end, err) = strconv.ParseUint(strings.TrimPrefix(flag.Arg(2), "0x"), 16, 64);
            if (err != null) {
                log.Fatalf("invalid end PC: %v", err);
            }
            dis.Print(os.Stdout, symRE, start, end, printCode.val, gnuAsm.val);
            break;
        default: 
            usage();
            break;
    }
});

} // end main_package
