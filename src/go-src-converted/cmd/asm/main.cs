// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 05:54:11 UTC
// Original source: C:\Program Files\Go\src\cmd\asm\main.go
namespace go;

using bufio = bufio_package;
using flag = flag_package;
using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using log = log_package;
using os = os_package;

using arch = cmd.asm.@internal.arch_package;
using asm = cmd.asm.@internal.asm_package;
using flags = cmd.asm.@internal.flags_package;
using lex = cmd.asm.@internal.lex_package;

using bio = cmd.@internal.bio_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using System;

public static partial class main_package {

private static void Main() => func((defer, _, _) => {
    log.SetFlags(0);
    log.SetPrefix("asm: ");

    buildcfg.Check();
    var GOARCH = buildcfg.GOARCH;

    var architecture = arch.Set(GOARCH);
    if (architecture == null) {
        log.Fatalf("unrecognized architecture %s", GOARCH);
    }
    flags.Parse();

    var ctxt = obj.Linknew(architecture.LinkArch);
    ctxt.Debugasm = flags.PrintOut;
    ctxt.Debugvlog = flags.DebugV;
    ctxt.Flag_dynlink = flags.Dynlink.val;
    ctxt.Flag_linkshared = flags.Linkshared.val;
    ctxt.Flag_shared = flags.Shared || flags.Dynlink.val;
    ctxt.IsAsm = true;
    ctxt.Pkgpath = flags.Importpath.val;
    switch (flags.Spectre.val) {
        case "": 

            break;
        case "index": 

            break;
        case "all": 

        case "ret": 
            ctxt.Retpoline = true;
            break;
        default: 
            log.Printf("unknown setting -spectre=%s", flags.Spectre.val);
            os.Exit(2);
            break;
    }

    ctxt.Bso = bufio.NewWriter(os.Stdout);
    defer(ctxt.Bso.Flush());

    architecture.Init(ctxt); 

    // Create object file, write header.
    var (buf, err) = bio.Create(flags.OutputFile.val);
    if (err != null) {
        log.Fatal(err);
    }
    defer(buf.Close());

    if (!flags.SymABIs.val) {
        buf.WriteString(objabi.HeaderString());
        fmt.Fprintf(buf, "!\n");
    }
    bool ok = default;    bool diag = default;

    @string failedFile = default;
    foreach (var (_, f) in flag.Args()) {
        var lexer = lex.NewLexer(f);
        var parser = asm.NewParser(ctxt, architecture, lexer, flags.CompilingRuntime.val);
        ctxt.DiagFunc = (format, args) => {
            diag = true;
            log.Printf(format, args);
        };
        if (flags.SymABIs.val) {
            ok = parser.ParseSymABIs(buf);
        }
        else
 {
            ptr<object> pList = @new<obj.Plist>();
            pList.Firstpc, ok = parser.Parse(); 
            // reports errors to parser.Errorf
            if (ok) {
                obj.Flushplist(ctxt, pList, null, flags.Importpath.val);
            }
        }
        if (!ok) {
            failedFile = f;
            break;
        }
    }    if (ok && !flags.SymABIs.val) {
        ctxt.NumberSyms();
        obj.WriteObjFile(ctxt, buf);
    }
    if (!ok || diag) {
        if (failedFile != "") {
            log.Printf("assembly of %s failed", failedFile);
        }
        else
 {
            log.Print("assembly failed");
        }
        buf.Close();
        os.Remove(flags.OutputFile.val);
        os.Exit(1);
    }
});

} // end main_package
