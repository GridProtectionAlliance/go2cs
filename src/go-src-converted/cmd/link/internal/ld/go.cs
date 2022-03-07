// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go-specific code shared across loaders (5l, 6l, 8l).

// package ld -- go2cs converted at 2022 March 06 23:21:25 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\go.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

    // go-specific code shared across loaders (5l, 6l, 8l).

    // replace all "". with pkg.
private static @string expandpkg(@string t0, @string pkg) {
    return strings.Replace(t0, "\"\".", pkg + ".", -1);
}

// TODO:
//    generate debugging section in binary.
//    once the dust settles, try to move some code to
//        libmach, so that other linkers and ar can share.

private static void ldpkg(ptr<Link> _addr_ctxt, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, long length, @string filename) {
    ref Link ctxt = ref _addr_ctxt.val;
    ref bio.Reader f = ref _addr_f.val;
    ref sym.Library lib = ref _addr_lib.val;

    if (flagG.val) {
        return ;
    }
    if (int64(int(length)) != length) {
        fmt.Fprintf(os.Stderr, "%s: too much pkg data in %s\n", os.Args[0], filename);
        return ;
    }
    var bdata = make_slice<byte>(length);
    {
        var (_, err) = io.ReadFull(f, bdata);

        if (err != null) {
            fmt.Fprintf(os.Stderr, "%s: short pkg read %s\n", os.Args[0], filename);
            return ;
        }
    }

    var data = string(bdata); 

    // process header lines
    while (data != "") {
        @string line = default;
        {
            var i__prev1 = i;

            var i = strings.Index(data, "\n");

            if (i >= 0) {
                (line, data) = (data[..(int)i], data[(int)i + 1..]);
            }
            else
 {
                (line, data) = (data, "");
            }

            i = i__prev1;

        }

        if (line == "main") {
            lib.Main = true;
        }
        if (line == "") {
            break;
        }
    } 

    // look for cgo section
    var p0 = strings.Index(data, "\n$$  // cgo");
    nint p1 = default;
    if (p0 >= 0) {
        p0 += p1;
        i = strings.IndexByte(data[(int)p0 + 1..], '\n');
        if (i < 0) {
            fmt.Fprintf(os.Stderr, "%s: found $$ // cgo but no newline in %s\n", os.Args[0], filename);
            return ;
        }
        p0 += 1 + i;

        p1 = strings.Index(data[(int)p0..], "\n$$");
        if (p1 < 0) {
            p1 = strings.Index(data[(int)p0..], "\n!\n");
        }
        if (p1 < 0) {
            fmt.Fprintf(os.Stderr, "%s: cannot find end of // cgo section in %s\n", os.Args[0], filename);
            return ;
        }
        p1 += p0;
        loadcgo(_addr_ctxt, filename, objabi.PathToPrefix(lib.Pkg), data[(int)p0..(int)p1]);

    }
}

private static void loadcgo(ptr<Link> _addr_ctxt, @string file, @string pkg, @string p) {
    ref Link ctxt = ref _addr_ctxt.val;

    ref slice<slice<@string>> directives = ref heap(out ptr<slice<slice<@string>>> _addr_directives);
    {
        var err = json.NewDecoder(strings.NewReader(p)).Decode(_addr_directives);

        if (err != null) {
            fmt.Fprintf(os.Stderr, "%s: %s: failed decoding cgo directives: %v\n", os.Args[0], file, err);
            nerrors++;
            return ;
        }
    } 

    // Record the directives. We'll process them later after Symbols are created.
    ctxt.cgodata = append(ctxt.cgodata, new cgodata(file,pkg,directives));

}

// Set symbol attributes or flags based on cgo directives.
// Any newly discovered HOSTOBJ syms are added to 'hostObjSyms'.
private static void setCgoAttr(ptr<Link> _addr_ctxt, @string file, @string pkg, slice<slice<@string>> directives, object hostObjSyms) {
    ref Link ctxt = ref _addr_ctxt.val;

    var l = ctxt.loader;
    foreach (var (_, f) in directives) {
        switch (f[0]) {
            case "cgo_import_dynamic": 
                           if (len(f) < 2 || len(f) > 4) {
                               break;
                           }
                           var local = f[1];
                           var remote = local;
                           if (len(f) > 2) {
                               remote = f[2];
                           }
                           @string lib = "";
                           if (len(f) > 3) {
                               lib = f[3];
                           }
                           if (FlagD.val) {
                               fmt.Fprintf(os.Stderr, "%s: %s: cannot use dynamic imports with -d flag\n", os.Args[0], file);
                               nerrors++;
                               return ;
                           }
                           if (local == "_" && remote == "_") { 
                               // allow #pragma dynimport _ _ "foo.so"
                               // to force a link of foo.so.
                               havedynamic = 1;

                               if (ctxt.HeadType == objabi.Hdarwin) {
                                   machoadddynlib(lib, ctxt.LinkMode);
                               }
                               else
                {
                                   dynlib = append(dynlib, lib);
                               }

                               continue;

                           }

                           local = expandpkg(local, pkg);
                           @string q = "";
                           {
                               var i = strings.Index(remote, "#");

                               if (i >= 0) {
                                   (remote, q) = (remote[..(int)i], remote[(int)i + 1..]);
                               }

                           }

                           var s = l.LookupOrCreateSym(local, 0);
                           var st = l.SymType(s);
                           if (st == 0 || st == sym.SXREF || st == sym.SBSS || st == sym.SNOPTRBSS || st == sym.SHOSTOBJ) {
                               l.SetSymDynimplib(s, lib);
                               l.SetSymExtname(s, remote);
                               l.SetSymDynimpvers(s, q);
                               if (st != sym.SHOSTOBJ) {
                                   var su = l.MakeSymbolUpdater(s);
                                   su.SetType(sym.SDYNIMPORT);
                               }
                               else
                {
                                   hostObjSyms[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                               }

                               havedynamic = 1;
                               if (lib != "" && ctxt.IsDarwin()) {
                                   machoadddynlib(lib, ctxt.LinkMode);
                               }

                           }

                           continue;

                break;
            case "cgo_import_static": 
                if (len(f) != 2) {
                    break;
                }
                local = f[1];

                s = l.LookupOrCreateSym(local, 0);
                su = l.MakeSymbolUpdater(s);
                su.SetType(sym.SHOSTOBJ);
                su.SetSize(0);
                hostObjSyms[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                continue;
                break;
            case "cgo_export_static": 

            case "cgo_export_dynamic": 
                           if (len(f) < 2 || len(f) > 4) {
                               break;
                           }
                           local = f[1];
                           remote = local;
                           if (len(f) > 2) {
                               remote = f[2];
                           }
                           local = expandpkg(local, pkg); 
                           // The compiler adds a fourth argument giving
                           // the definition ABI of function symbols.
                           var abi = obj.ABI0;
                           if (len(f) > 3) {
                               bool ok = default;
                               abi, ok = obj.ParseABI(f[3]);
                               if (!ok) {
                                   fmt.Fprintf(os.Stderr, "%s: bad ABI in cgo_export directive %s\n", os.Args[0], f);
                                   nerrors++;
                                   return ;
                               }
                           }

                           s = l.LookupOrCreateSym(local, sym.ABIToVersion(abi));

                           if (l.SymType(s) == sym.SHOSTOBJ) {
                               hostObjSyms[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                           }


                           if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModePlugin) 
                               if (s == l.Lookup("main", 0)) {
                                   continue;
                               }
                           // export overrides import, for openbsd/cgo.
                           // see issue 4878.
                           if (l.SymDynimplib(s) != "") {
                               l.SetSymDynimplib(s, "");
                               l.SetSymDynimpvers(s, "");
                               l.SetSymExtname(s, "");
                               su = ;
                               su = l.MakeSymbolUpdater(s);
                               su.SetType(0);
                           }

                           if (!(l.AttrCgoExportStatic(s) || l.AttrCgoExportDynamic(s))) {
                               l.SetSymExtname(s, remote);
                           }
                           else if (l.SymExtname(s) != remote) {
                               fmt.Fprintf(os.Stderr, "%s: conflicting cgo_export directives: %s as %s and %s\n", os.Args[0], l.SymName(s), l.SymExtname(s), remote);
                               nerrors++;
                               return ;
                           } 

                           // Mark exported symbols and also add them to
                           // the lists used for roots in the deadcode pass.
                           if (f[0] == "cgo_export_static") {
                               if (ctxt.LinkMode == LinkExternal && !l.AttrCgoExportStatic(s)) { 
                                   // Static cgo exports appear
                                   // in the exported symbol table.
                                   ctxt.dynexp = append(ctxt.dynexp, s);

                               }

                               if (ctxt.LinkMode == LinkInternal) { 
                                   // For internal linking, we're
                                   // responsible for resolving
                                   // relocations from host objects.
                                   // Record the right Go symbol
                                   // version to use.
                                   l.AddCgoExport(s);

                               }

                               l.SetAttrCgoExportStatic(s, true);

                           }
                           else
                {
                               if (ctxt.LinkMode == LinkInternal && !l.AttrCgoExportDynamic(s)) { 
                                   // Dynamic cgo exports appear
                                   // in the exported symbol table.
                                   ctxt.dynexp = append(ctxt.dynexp, s);

                               }

                               l.SetAttrCgoExportDynamic(s, true);

                           }

                           continue;

                break;
            case "cgo_dynamic_linker": 
                if (len(f) != 2) {
                    break;
                }
                if (flagInterpreter == "".val) {
                    if (interpreter != "" && interpreter != f[1]) {
                        fmt.Fprintf(os.Stderr, "%s: conflict dynlinker: %s and %s\n", os.Args[0], interpreter, f[1]);
                        nerrors++;
                        return ;
                    }
                    interpreter = f[1];
                }
                continue;
                break;
            case "cgo_ldflag": 
                if (len(f) != 2) {
                    break;
                }
                ldflag = append(ldflag, f[1]);
                continue;
                break;
        }

        fmt.Fprintf(os.Stderr, "%s: %s: invalid cgo directive: %q\n", os.Args[0], file, f);
        nerrors++;

    }    return ;

}

// openbsdTrimLibVersion indicates whether a shared library is
// versioned and if it is, returns the unversioned name. The
// OpenBSD library naming scheme is lib<name>.so.<major>.<minor>
private static (@string, bool) openbsdTrimLibVersion(@string lib) {
    @string _p0 = default;
    bool _p0 = default;

    var parts = strings.Split(lib, ".");
    if (len(parts) != 4) {
        return ("", false);
    }
    if (parts[1] != "so") {
        return ("", false);
    }
    {
        var (_, err) = strconv.Atoi(parts[2]);

        if (err != null) {
            return ("", false);
        }
    }

    {
        (_, err) = strconv.Atoi(parts[3]);

        if (err != null) {
            return ("", false);
        }
    }

    return (fmt.Sprintf("%s.%s", parts[0], parts[1]), true);

}

// dedupLibrariesOpenBSD dedups a list of shared libraries, treating versioned
// and unversioned libraries as equivalents. Versioned libraries are preferred
// and retained over unversioned libraries. This avoids the situation where
// the use of cgo results in a DT_NEEDED for a versioned library (for example,
// libc.so.96.1), while a dynamic import specifies an unversioned library (for
// example, libc.so) - this would otherwise result in two DT_NEEDED entries
// for the same library, resulting in a failure when ld.so attempts to load
// the Go binary.
private static slice<@string> dedupLibrariesOpenBSD(ptr<Link> _addr_ctxt, slice<@string> libs) {
    ref Link ctxt = ref _addr_ctxt.val;

    var libraries = make_map<@string, @string>();
    {
        var lib__prev1 = lib;

        foreach (var (_, __lib) in libs) {
            lib = __lib;
            {
                var (name, ok) = openbsdTrimLibVersion(lib);

                if (ok) { 
                    // Record unversioned name as seen.
                    seenlib[name] = true;
                    libraries[name] = lib;

                }                {
                    var (_, ok) = libraries[lib];


                    else if (!ok) {
                        libraries[lib] = lib;
                    }

                }


            }

        }
        lib = lib__prev1;
    }

    libs = null;
    {
        var lib__prev1 = lib;

        foreach (var (_, __lib) in libraries) {
            lib = __lib;
            libs = append(libs, lib);
        }
        lib = lib__prev1;
    }

    sort.Strings(libs);

    return libs;

}

private static slice<@string> dedupLibraries(ptr<Link> _addr_ctxt, slice<@string> libs) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (ctxt.Target.IsOpenbsd()) {
        return dedupLibrariesOpenBSD(_addr_ctxt, libs);
    }
    return libs;

}

private static var seenlib = make_map<@string, bool>();

private static void adddynlib(ptr<Link> _addr_ctxt, @string lib) {
    ref Link ctxt = ref _addr_ctxt.val;

    if (seenlib[lib] || ctxt.LinkMode == LinkExternal) {
        return ;
    }
    seenlib[lib] = true;

    if (ctxt.IsELF) {
        var dsu = ctxt.loader.MakeSymbolUpdater(ctxt.DynStr);
        if (dsu.Size() == 0) {
            dsu.Addstring("");
        }
        var du = ctxt.loader.MakeSymbolUpdater(ctxt.Dynamic);
        Elfwritedynent(ctxt.Arch, du, elf.DT_NEEDED, uint64(dsu.Addstring(lib)));

    }
    else
 {
        Errorf(null, "adddynlib: unsupported binary format");
    }
}

public static void Adddynsym(ptr<loader.Loader> _addr_ldr, ptr<Target> _addr_target, ptr<ArchSyms> _addr_syms, loader.Sym s) {
    ref loader.Loader ldr = ref _addr_ldr.val;
    ref Target target = ref _addr_target.val;
    ref ArchSyms syms = ref _addr_syms.val;

    if (ldr.SymDynid(s) >= 0 || target.LinkMode == LinkExternal) {
        return ;
    }
    if (target.IsELF) {
        elfadddynsym(ldr, target, syms, s);
    }
    else if (target.HeadType == objabi.Hdarwin) {
        ldr.Errorf(s, "adddynsym: missed symbol (Extname=%s)", ldr.SymExtname(s));
    }
    else if (target.HeadType == objabi.Hwindows) { 
        // already taken care of
    }
    else
 {
        ldr.Errorf(s, "adddynsym: unsupported binary format");
    }
}

private static void fieldtrack(ptr<sys.Arch> _addr_arch, ptr<loader.Loader> _addr_l) {
    ref sys.Arch arch = ref _addr_arch.val;
    ref loader.Loader l = ref _addr_l.val;

    bytes.Buffer buf = default;
    for (var i = loader.Sym(1); i < loader.Sym(l.NSym()); i++) {
        {
            var name = l.SymName(i);

            if (strings.HasPrefix(name, "go.track.")) {
                if (l.AttrReachable(i)) {
                    l.SetAttrSpecial(i, true);
                    l.SetAttrNotInSymbolTable(i, true);
                    buf.WriteString(name[(int)9..]);
                    {
                        var p = l.Reachparent[i];

                        while (p != 0) {
                            buf.WriteString("\t");
                            buf.WriteString(l.SymName(p));
                            p = l.Reachparent[p];
                        }

                    }
                    buf.WriteString("\n");

                }

            }

        }

    }
    l.Reachparent = null; // we are done with it
    if (flagFieldTrack == "".val) {
        return ;
    }
    var s = l.Lookup(flagFieldTrack.val, 0);
    if (s == 0 || !l.AttrReachable(s)) {
        return ;
    }
    var bld = l.MakeSymbolUpdater(s);
    bld.SetType(sym.SDATA);
    addstrdata(arch, l, flagFieldTrack.val, buf.String());

}

private static void addexport(this ptr<Link> _addr_ctxt) => func((_, panic, _) => {
    ref Link ctxt = ref _addr_ctxt.val;
 
    // Track undefined external symbols during external link.
    if (ctxt.LinkMode == LinkExternal) {
        {
            var s__prev1 = s;

            foreach (var (_, __s) in ctxt.Textp) {
                s = __s;
                if (ctxt.loader.AttrSpecial(s) || ctxt.loader.AttrSubSymbol(s)) {
                    continue;
                }
                var relocs = ctxt.loader.Relocs(s);
                for (nint i = 0; i < relocs.Count(); i++) {
                    {
                        var rs = relocs.At(i).Sym();

                        if (rs != 0) {
                            if (ctxt.loader.SymType(rs) == sym.Sxxx && !ctxt.loader.AttrLocal(rs)) { 
                                // sanity check
                                if (len(ctxt.loader.Data(rs)) != 0) {
                                    panic("expected no data on undef symbol");
                                }

                                var su = ctxt.loader.MakeSymbolUpdater(rs);
                                su.SetType(sym.SUNDEFEXT);

                            }

                        }

                    }

                }


            }

            s = s__prev1;
        }
    }
    if (ctxt.HeadType == objabi.Hdarwin || ctxt.HeadType == objabi.Haix) {
        return ;
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in ctxt.dynexp) {
            s = __s; 
            // Consistency check.
            if (!ctxt.loader.AttrReachable(s)) {
                panic("dynexp entry not reachable");
            }

            Adddynsym(_addr_ctxt.loader, _addr_ctxt.Target, _addr_ctxt.ArchSyms, s);

        }
        s = s__prev1;
    }

    foreach (var (_, lib) in dedupLibraries(_addr_ctxt, dynlib)) {
        adddynlib(_addr_ctxt, lib);
    }
});

public partial struct Pkg {
    public bool mark;
    public bool @checked;
    public @string path;
    public slice<ptr<Pkg>> impby;
}

private static slice<ptr<Pkg>> pkgall = default;

private static ptr<Pkg> cycle(this ptr<Pkg> _addr_p) {
    ref Pkg p = ref _addr_p.val;

    if (p.@checked) {
        return _addr_null!;
    }
    if (p.mark) {
        nerrors++;
        fmt.Printf("import cycle:\n");
        fmt.Printf("\t%s\n", p.path);
        return _addr_p!;
    }
    p.mark = true;
    foreach (var (_, q) in p.impby) {
        {
            var bad = q.cycle();

            if (bad != null) {
                p.mark = false;
                p.@checked = true;
                fmt.Printf("\timports %s\n", p.path);
                if (bad == p) {
                    return _addr_null!;
                }
                return _addr_bad!;
            }

        }

    }    p.@checked = true;
    p.mark = false;
    return _addr_null!;

}

private static void importcycles() {
    foreach (var (_, p) in pkgall) {
        p.cycle();
    }
}

} // end ld_package
