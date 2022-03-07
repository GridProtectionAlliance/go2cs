// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2022 March 06 23:12:12 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\gc\main.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using @base = go.cmd.compile.@internal.@base_package;
using deadcode = go.cmd.compile.@internal.deadcode_package;
using devirtualize = go.cmd.compile.@internal.devirtualize_package;
using dwarfgen = go.cmd.compile.@internal.dwarfgen_package;
using escape = go.cmd.compile.@internal.escape_package;
using inline = go.cmd.compile.@internal.inline_package;
using ir = go.cmd.compile.@internal.ir_package;
using logopt = go.cmd.compile.@internal.logopt_package;
using noder = go.cmd.compile.@internal.noder_package;
using pkginit = go.cmd.compile.@internal.pkginit_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using log = go.log_package;
using os = go.os_package;
using runtime = go.runtime_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class gc_package {

private static void hidePanic() => func((_, panic, _) => {
    if (@base.Debug.Panic == 0 && @base.Errors() > 0) { 
        // If we've already complained about things
        // in the program, don't bother complaining
        // about a panic too; let the user clean up
        // the code and try again.
        {
            var err = recover();

            if (err != null) {
                if (err == "-h") {
                    panic(err);
                }
                @base.ErrorExit();

            }
        }

    }
});

// Main parses flags and Go source files specified in the command-line
// arguments, type-checks the parsed Go package, compiles functions to machine
// code, and finally writes the compiled package definition to disk.
public static void Main(Action<ptr<ssagen.ArchInfo>> archInit) => func((defer, _, _) => {
    @base.Timer.Start("fe", "init");

    defer(hidePanic());

    archInit(_addr_ssagen.Arch);

    @base.Ctxt = obj.Linknew(ssagen.Arch.LinkArch);
    @base.Ctxt.DiagFunc = @base.Errorf;
    @base.Ctxt.DiagFlush = @base.FlushErrors;
    @base.Ctxt.Bso = bufio.NewWriter(os.Stdout); 

    // UseBASEntries is preferred because it shaves about 2% off build time, but LLDB, dsymutil, and dwarfdump
    // on Darwin don't support it properly, especially since macOS 10.14 (Mojave).  This is exposed as a flag
    // to allow testing with LLVM tools on Linux, and to help with reporting this bug to the LLVM project.
    // See bugs 31188 and 21945 (CLs 170638, 98075, 72371).
    @base.Ctxt.UseBASEntries = @base.Ctxt.Headtype != objabi.Hdarwin;

    types.LocalPkg = types.NewPkg("", "");
    types.LocalPkg.Prefix = "\"\""; 

    // We won't know localpkg's height until after import
    // processing. In the mean time, set to MaxPkgHeight to ensure
    // height comparisons at least work until then.
    types.LocalPkg.Height = types.MaxPkgHeight; 

    // pseudo-package, for scoping
    types.BuiltinPkg = types.NewPkg("go.builtin", ""); // TODO(gri) name this package go.builtin?
    types.BuiltinPkg.Prefix = "go.builtin"; // not go%2ebuiltin

    // pseudo-package, accessed by import "unsafe"
    ir.Pkgs.Unsafe = types.NewPkg("unsafe", "unsafe"); 

    // Pseudo-package that contains the compiler's builtin
    // declarations for package runtime. These are declared in a
    // separate package to avoid conflicts with package runtime's
    // actual declarations, which may differ intentionally but
    // insignificantly.
    ir.Pkgs.Runtime = types.NewPkg("go.runtime", "runtime");
    ir.Pkgs.Runtime.Prefix = "runtime"; 

    // pseudo-packages used in symbol tables
    ir.Pkgs.Itab = types.NewPkg("go.itab", "go.itab");
    ir.Pkgs.Itab.Prefix = "go.itab"; // not go%2eitab

    // pseudo-package used for methods with anonymous receivers
    ir.Pkgs.Go = types.NewPkg("go", "");

    @base.DebugSSA = ssa.PhaseOption;
    @base.ParseFlags(); 

    // Record flags that affect the build result. (And don't
    // record flags that don't, since that would cause spurious
    // changes in the binary.)
    dwarfgen.RecordFlags("B", "N", "l", "msan", "race", "shared", "dynlink", "dwarf", "dwarflocationlists", "dwarfbasentries", "smallframes", "spectre");

    if (!@base.EnableTrace && @base.Flag.LowerT) {
        log.Fatalf("compiler not built with support for -t");
    }
    if (@base.Flag.LowerL <= 1) {
        @base.Flag.LowerL = 1 - @base.Flag.LowerL;
    }
    if (@base.Flag.SmallFrames) {
        ir.MaxStackVarSize = 128 * 1024;
        ir.MaxImplicitStackVarSize = 16 * 1024;
    }
    if (@base.Flag.Dwarf) {
        @base.Ctxt.DebugInfo = dwarfgen.Info;
        @base.Ctxt.GenAbstractFunc = dwarfgen.AbstractFunc;
        @base.Ctxt.DwFixups = obj.NewDwarfFixupTable(@base.Ctxt);
    }
    else
 { 
        // turn off inline generation if no dwarf at all
        @base.Flag.GenDwarfInl = 0;
        @base.Ctxt.Flag_locationlists = false;

    }
    if (@base.Ctxt.Flag_locationlists && len(@base.Ctxt.Arch.DWARFRegisters) == 0) {
        log.Fatalf("location lists requested but register mapping not available on %v", @base.Ctxt.Arch.Name);
    }
    types.ParseLangFlag();

    var symABIs = ssagen.NewSymABIs(@base.Ctxt.Pkgpath);
    if (@base.Flag.SymABIs != "") {
        symABIs.ReadSymABIs(@base.Flag.SymABIs);
    }
    if (@base.Compiling(@base.NoInstrumentPkgs)) {
        @base.Flag.Race = false;
        @base.Flag.MSan = false;
    }
    ssagen.Arch.LinkArch.Init(@base.Ctxt);
    startProfile();
    if (@base.Flag.Race || @base.Flag.MSan) {
        @base.Flag.Cfg.Instrumenting = true;
    }
    if (@base.Flag.Dwarf) {
        dwarf.EnableLogging(@base.Debug.DwarfInl != 0);
    }
    if (@base.Debug.SoftFloat != 0) {
        if (buildcfg.Experiment.RegabiArgs) {
            log.Fatalf("softfloat mode with GOEXPERIMENT=regabiargs not implemented ");
        }
        ssagen.Arch.SoftFloat = true;

    }
    if (@base.Flag.JSON != "") { // parse version,destination from json logging optimization.
        logopt.LogJsonOption(@base.Flag.JSON);

    }
    ir.EscFmt = escape.Fmt;
    ir.IsIntrinsicCall = ssagen.IsIntrinsicCall;
    inline.SSADumpInline = ssagen.DumpInline;
    ssagen.InitEnv();
    ssagen.InitTables();

    types.PtrSize = ssagen.Arch.LinkArch.PtrSize;
    types.RegSize = ssagen.Arch.LinkArch.RegSize;
    types.MaxWidth = ssagen.Arch.MAXWIDTH;

    typecheck.Target = @new<ir.Package>();

    typecheck.NeedITab = (t, iface) => {
        reflectdata.ITabAddr(t, iface);
    };
    typecheck.NeedRuntimeType = reflectdata.NeedRuntimeType; // TODO(rsc): TypeSym for lock?

    @base.AutogeneratedPos = makePos(_addr_src.NewFileBase("<autogenerated>", "<autogenerated>"), 1, 0);

    typecheck.InitUniverse(); 

    // Parse and typecheck input.
    noder.LoadPackage(flag.Args());

    dwarfgen.RecordPackageName(); 

    // Build init task.
    {
        var initTask = pkginit.Task();

        if (initTask != null) {
            typecheck.Export(initTask);
        }
    } 

    // Eliminate some obviously dead code.
    // Must happen after typechecking.
    {
        var n__prev1 = n;

        foreach (var (_, __n) in typecheck.Target.Decls) {
            n = __n;
            if (n.Op() == ir.ODCLFUNC) {
                deadcode.Func(n._<ptr<ir.Func>>());
            }
        }
        n = n__prev1;
    }

    if (typecheck.DirtyAddrtaken) {
        typecheck.ComputeAddrtaken(typecheck.Target.Decls);
        typecheck.DirtyAddrtaken = false;
    }
    typecheck.IncrementalAddrtaken = true;

    if (@base.Debug.TypecheckInl != 0) { 
        // Typecheck imported function bodies if Debug.l > 1,
        // otherwise lazily when used or re-exported.
        typecheck.AllImportedBodies();

    }
    @base.Timer.Start("fe", "inlining");
    if (@base.Flag.LowerL != 0) {
        inline.InlinePackage();
    }
    {
        var n__prev1 = n;

        foreach (var (_, __n) in typecheck.Target.Decls) {
            n = __n;
            if (n.Op() == ir.ODCLFUNC) {
                devirtualize.Func(n._<ptr<ir.Func>>());
            }
        }
        n = n__prev1;
    }

    ir.CurFunc = null; 

    // Generate ABI wrappers. Must happen before escape analysis
    // and doesn't benefit from dead-coding or inlining.
    symABIs.GenABIWrappers(); 

    // Escape analysis.
    // Required for moving heap allocations onto stack,
    // which in turn is required by the closure implementation,
    // which stores the addresses of stack variables into the closure.
    // If the closure does not escape, it needs to be on the stack
    // or else the stack copier will not update it.
    // Large values are also moved off stack in escape analysis;
    // because large values may contain pointers, it must happen early.
    @base.Timer.Start("fe", "escapes");
    escape.Funcs(typecheck.Target.Decls); 

    // Collect information for go:nowritebarrierrec
    // checking. This must happen before transforming closures during Walk
    // We'll do the final check after write barriers are
    // inserted.
    if (@base.Flag.CompilingRuntime) {
        ssagen.EnableNoWriteBarrierRecCheck();
    }
    typecheck.InitRuntime();
    ssagen.InitConfig(); 

    // Just before compilation, compile itabs found on
    // the right side of OCONVIFACE so that methods
    // can be de-virtualized during compilation.
    ir.CurFunc = null;
    reflectdata.CompileITabs(); 

    // Compile top level functions.
    // Don't use range--walk can add functions to Target.Decls.
    @base.Timer.Start("be", "compilefuncs");
    var fcount = int64(0);
    for (nint i = 0; i < len(typecheck.Target.Decls); i++) {
        {
            ptr<ir.Func> (fn, ok) = typecheck.Target.Decls[i]._<ptr<ir.Func>>();

            if (ok) {
                enqueueFunc(fn);
                fcount++;
            }

        }

    }
    @base.Timer.AddEvent(fcount, "funcs");

    compileFunctions();

    if (@base.Flag.CompilingRuntime) { 
        // Write barriers are now known. Check the call graph.
        ssagen.NoWriteBarrierRecCheck();

    }
    if (@base.Ctxt.DwFixups != null) {
        @base.Ctxt.DwFixups.Finalize(@base.Ctxt.Pkgpath, @base.Debug.DwarfInl != 0);
        @base.Ctxt.DwFixups = null;
        @base.Flag.GenDwarfInl = 0;
    }
    @base.Timer.Start("be", "dumpobj");
    dumpdata();
    @base.Ctxt.NumberSyms();
    dumpobj();
    if (@base.Flag.AsmHdr != "") {
        dumpasmhdr();
    }
    ssagen.CheckLargeStacks();
    typecheck.CheckFuncStack();

    if (len(compilequeue) != 0) {
        @base.Fatalf("%d uncompiled functions", len(compilequeue));
    }
    logopt.FlushLoggedOpts(@base.Ctxt, @base.Ctxt.Pkgpath);
    @base.ExitIfErrors();

    @base.FlushErrors();
    @base.Timer.Stop();

    if (@base.Flag.Bench != "") {
        {
            var err = writebench(@base.Flag.Bench);

            if (err != null) {
                log.Fatalf("cannot write benchmark data: %v", err);
            }

        }

    }
});

private static error writebench(@string filename) => func((_, panic, _) => {
    var (f, err) = os.OpenFile(filename, os.O_WRONLY | os.O_CREATE | os.O_APPEND, 0666);
    if (err != null) {
        return error.As(err)!;
    }
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    fmt.Fprintln(_addr_buf, "commit:", buildcfg.Version);
    fmt.Fprintln(_addr_buf, "goos:", runtime.GOOS);
    fmt.Fprintln(_addr_buf, "goarch:", runtime.GOARCH);
    @base.Timer.Write(_addr_buf, "BenchmarkCompile:" + @base.Ctxt.Pkgpath + ":");

    var (n, err) = f.Write(buf.Bytes());
    if (err != null) {
        return error.As(err)!;
    }
    if (n != buf.Len()) {
        panic("bad writer");
    }
    return error.As(f.Close())!;

});

private static src.XPos makePos(ptr<src.PosBase> _addr_b, nuint line, nuint col) {
    ref src.PosBase b = ref _addr_b.val;

    return @base.Ctxt.PosTable.XPos(src.MakePos(b, line, col));
}

} // end gc_package
