// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:48:38 UTC
// Original source: C:\Go\src\cmd\asm\main.go
using bufio = go.bufio_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using log = go.log_package;
using os = go.os_package;

using arch = go.cmd.asm.@internal.arch_package;
using asm = go.cmd.asm.@internal.asm_package;
using flags = go.cmd.asm.@internal.flags_package;
using lex = go.cmd.asm.@internal.lex_package;

using bio = go.cmd.@internal.bio_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void Main() => func((defer, _, __) =>
        {
            log.SetFlags(0L);
            log.SetPrefix("asm: ");

            var GOARCH = objabi.GOARCH;

            var architecture = arch.Set(GOARCH);
            if (architecture == null)
            {
                log.Fatalf("unrecognized architecture %s", GOARCH);
            }
            flags.Parse();

            var ctxt = obj.Linknew(architecture.LinkArch);
            if (flags.PrintOut.Value)
            {
                ctxt.Debugasm = true;
            }
            ctxt.Flag_dynlink = flags.Dynlink.Value;
            ctxt.Flag_shared = flags.Shared || flags.Dynlink.Value.Value;
            ctxt.Bso = bufio.NewWriter(os.Stdout);
            defer(ctxt.Bso.Flush());

            architecture.Init(ctxt); 

            // Create object file, write header.
            var (out, err) = os.Create(flags.OutputFile.Value);
            if (err != null)
            {
                log.Fatal(err);
            }
            defer(bio.MustClose(out));
            var buf = bufio.NewWriter(bio.MustWriter(out));

            fmt.Fprintf(buf, "go object %s %s %s\n", objabi.GOOS, objabi.GOARCH, objabi.Version);
            fmt.Fprintf(buf, "!\n");

            bool ok = default;            bool diag = default;

            @string failedFile = default;
            foreach (var (_, f) in flag.Args())
            {
                var lexer = lex.NewLexer(f);
                var parser = asm.NewParser(ctxt, architecture, lexer);
                ctxt.DiagFunc = (format, args) =>
                {
                    diag = true;
                    log.Printf(format, args);
                };
                ptr<object> pList = @new<obj.Plist>();
                pList.Firstpc, ok = parser.Parse();
                if (!ok)
                {
                    failedFile = f;
                    break;
                }
                obj.Flushplist(ctxt, pList, null, "");
            }            if (ok)
            {
                obj.WriteObjFile(ctxt, buf);
            }
            if (!ok || diag)
            {
                if (failedFile != "")
                {
                    log.Printf("assembly of %s failed", failedFile);
                }
                else
                {
                    log.Print("assembly failed");
                }
                @out.Close();
                os.Remove(flags.OutputFile.Value);
                os.Exit(1L);
            }
            buf.Flush();
        });
    }
}
