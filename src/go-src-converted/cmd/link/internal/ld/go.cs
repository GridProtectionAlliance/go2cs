// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go-specific code shared across loaders (5l, 6l, 8l).

// package ld -- go2cs converted at 2020 October 09 05:49:40 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\go.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // go-specific code shared across loaders (5l, 6l, 8l).

        // replace all "". with pkg.
        private static @string expandpkg(@string t0, @string pkg)
        {
            return strings.Replace(t0, "\"\".", pkg + ".", -1L);
        }

        private static ptr<sym.Symbol> resolveABIAlias(ptr<sym.Symbol> _addr_s) => func((_, panic, __) =>
        {
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Type != sym.SABIALIAS)
            {
                return _addr_s!;
            }

            var target = s.R[0L].Sym;
            if (target.Type == sym.SABIALIAS)
            {
                panic(fmt.Sprintf("ABI alias %s references another ABI alias %s", s, target));
            }

            return _addr_target!;

        });

        // TODO:
        //    generate debugging section in binary.
        //    once the dust settles, try to move some code to
        //        libmach, so that other linkers and ar can share.

        private static void ldpkg(ptr<Link> _addr_ctxt, ptr<bio.Reader> _addr_f, ptr<sym.Library> _addr_lib, long length, @string filename)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref bio.Reader f = ref _addr_f.val;
            ref sym.Library lib = ref _addr_lib.val;

            if (flagG.val)
            {
                return ;
            }

            if (int64(int(length)) != length)
            {
                fmt.Fprintf(os.Stderr, "%s: too much pkg data in %s\n", os.Args[0L], filename);
                if (flagU.val)
                {
                    errorexit();
                }

                return ;

            }

            var bdata = make_slice<byte>(length);
            {
                var (_, err) = io.ReadFull(f, bdata);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "%s: short pkg read %s\n", os.Args[0L], filename);
                    if (flagU.val)
                    {
                        errorexit();
                    }

                    return ;

                }

            }

            var data = string(bdata); 

            // process header lines
            while (data != "")
            {
                @string line = default;
                {
                    var i__prev1 = i;

                    var i = strings.Index(data, "\n");

                    if (i >= 0L)
                    {
                        line = data[..i];
                        data = data[i + 1L..];

                    }
                    else
                    {
                        line = data;
                        data = "";

                    }

                    i = i__prev1;

                }

                if (line == "safe")
                {
                    lib.Safe = true;
                }

                if (line == "main")
                {
                    lib.Main = true;
                }

                if (line == "")
                {
                    break;
                }

            } 

            // look for cgo section
 

            // look for cgo section
            var p0 = strings.Index(data, "\n$$  // cgo");
            long p1 = default;
            if (p0 >= 0L)
            {
                p0 += p1;
                i = strings.IndexByte(data[p0 + 1L..], '\n');
                if (i < 0L)
                {
                    fmt.Fprintf(os.Stderr, "%s: found $$ // cgo but no newline in %s\n", os.Args[0L], filename);
                    if (flagU.val)
                    {
                        errorexit();
                    }

                    return ;

                }

                p0 += 1L + i;

                p1 = strings.Index(data[p0..], "\n$$");
                if (p1 < 0L)
                {
                    p1 = strings.Index(data[p0..], "\n!\n");
                }

                if (p1 < 0L)
                {
                    fmt.Fprintf(os.Stderr, "%s: cannot find end of // cgo section in %s\n", os.Args[0L], filename);
                    if (flagU.val)
                    {
                        errorexit();
                    }

                    return ;

                }

                p1 += p0;
                loadcgo(_addr_ctxt, filename, objabi.PathToPrefix(lib.Pkg), data[p0..p1]);

            }

        }

        private static void loadcgo(ptr<Link> _addr_ctxt, @string file, @string pkg, @string p)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            ref slice<slice<@string>> directives = ref heap(out ptr<slice<slice<@string>>> _addr_directives);
            {
                var err = json.NewDecoder(strings.NewReader(p)).Decode(_addr_directives);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "%s: %s: failed decoding cgo directives: %v\n", os.Args[0L], file, err);
                    nerrors++;
                    return ;
                } 

                // Find cgo_export symbols. They are roots in the deadcode pass.

            } 

            // Find cgo_export symbols. They are roots in the deadcode pass.
            foreach (var (_, f) in directives)
            {
                switch (f[0L])
                {
                    case "cgo_export_static": 

                    case "cgo_export_dynamic": 
                        if (len(f) < 2L || len(f) > 3L)
                        {
                            continue;
                        }

                        var local = f[1L];

                        if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModePlugin) 
                            if (local == "main")
                            {
                                continue;
                            }

                                            local = expandpkg(local, pkg);
                        if (f[0L] == "cgo_export_static")
                        {
                            ctxt.cgo_export_static[local] = true;
                        }
                        else
                        {
                            ctxt.cgo_export_dynamic[local] = true;
                        }

                        break;
                }

            } 

            // Record the directives. We'll process them later after Symbols are created.
            ctxt.cgodata = append(ctxt.cgodata, new cgodata(file,pkg,directives));

        }

        // Set symbol attributes or flags based on cgo directives.
        // Any newly discovered HOSTOBJ syms are added to 'hostObjSyms'.
        private static loader.Sym setCgoAttr(ptr<Link> _addr_ctxt, Func<@string, long, loader.Sym> lookup, @string file, @string pkg, slice<slice<@string>> directives, object hostObjSyms)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var l = ctxt.loader;
            foreach (var (_, f) in directives)
            {
                switch (f[0L])
                {
                    case "cgo_import_dynamic": 
                        if (len(f) < 2L || len(f) > 4L)
                        {
                            break;
                        }

                        var local = f[1L];
                        var remote = local;
                        if (len(f) > 2L)
                        {
                            remote = f[2L];
                        }

                        @string lib = "";
                        if (len(f) > 3L)
                        {
                            lib = f[3L];
                        }

                        if (FlagD.val)
                        {
                            fmt.Fprintf(os.Stderr, "%s: %s: cannot use dynamic imports with -d flag\n", os.Args[0L], file);
                            nerrors++;
                            return ;
                        }

                        if (local == "_" && remote == "_")
                        { 
                            // allow #pragma dynimport _ _ "foo.so"
                            // to force a link of foo.so.
                            havedynamic = 1L;

                            if (ctxt.HeadType == objabi.Hdarwin)
                            {
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

                            if (i >= 0L)
                            {
                                remote = remote[..i];
                                q = remote[i + 1L..];

                            }

                        }

                        var s = lookup(local, 0L);
                        var st = l.SymType(s);
                        if (st == 0L || st == sym.SXREF || st == sym.SBSS || st == sym.SNOPTRBSS || st == sym.SHOSTOBJ)
                        {
                            l.SetSymDynimplib(s, lib);
                            l.SetSymExtname(s, remote);
                            l.SetSymDynimpvers(s, q);
                            if (st != sym.SHOSTOBJ)
                            {
                                var su = l.MakeSymbolUpdater(s);
                                su.SetType(sym.SDYNIMPORT);
                            }
                            else
                            {
                                hostObjSyms[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                            }

                            havedynamic = 1L;

                        }

                        continue;
                        break;
                    case "cgo_import_static": 
                        if (len(f) != 2L)
                        {
                            break;
                        }

                        local = f[1L];

                        s = lookup(local, 0L);
                        su = l.MakeSymbolUpdater(s);
                        su.SetType(sym.SHOSTOBJ);
                        su.SetSize(0L);
                        hostObjSyms[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                        continue;
                        break;
                    case "cgo_export_static": 

                    case "cgo_export_dynamic": 
                        if (len(f) < 2L || len(f) > 3L)
                        {
                            break;
                        }

                        local = f[1L];
                        remote = local;
                        if (len(f) > 2L)
                        {
                            remote = f[2L];
                        }

                        local = expandpkg(local, pkg); 

                        // The compiler arranges for an ABI0 wrapper
                        // to be available for all cgo-exported
                        // functions. Link.loadlib will resolve any
                        // ABI aliases we find here (since we may not
                        // yet know it's an alias).
                        s = lookup(local, 0L);

                        if (l.SymType(s) == sym.SHOSTOBJ)
                        {
                            hostObjSyms[s] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                        }


                        if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModePlugin) 
                            if (s == lookup("main", 0L))
                            {
                                continue;
                            }

                        // export overrides import, for openbsd/cgo.
                        // see issue 4878.
                        if (l.SymDynimplib(s) != "")
                        {
                            l.SetSymDynimplib(s, "");
                            l.SetSymDynimpvers(s, "");
                            l.SetSymExtname(s, "");
                            su = ;
                            su = l.MakeSymbolUpdater(s);
                            su.SetType(0L);
                        }

                        if (!(l.AttrCgoExportStatic(s) || l.AttrCgoExportDynamic(s)))
                        {
                            l.SetSymExtname(s, remote);
                        }
                        else if (l.SymExtname(s) != remote)
                        {
                            fmt.Fprintf(os.Stderr, "%s: conflicting cgo_export directives: %s as %s and %s\n", os.Args[0L], l.SymName(s), l.SymExtname(s), remote);
                            nerrors++;
                            return ;
                        }

                        if (f[0L] == "cgo_export_static")
                        {
                            l.SetAttrCgoExportStatic(s, true);
                        }
                        else
                        {
                            l.SetAttrCgoExportDynamic(s, true);
                        }

                        continue;
                        break;
                    case "cgo_dynamic_linker": 
                        if (len(f) != 2L)
                        {
                            break;
                        }

                        if (flagInterpreter == "".val)
                        {
                            if (interpreter != "" && interpreter != f[1L])
                            {
                                fmt.Fprintf(os.Stderr, "%s: conflict dynlinker: %s and %s\n", os.Args[0L], interpreter, f[1L]);
                                nerrors++;
                                return ;
                            }

                            interpreter = f[1L];

                        }

                        continue;
                        break;
                    case "cgo_ldflag": 
                        if (len(f) != 2L)
                        {
                            break;
                        }

                        ldflag = append(ldflag, f[1L]);
                        continue;
                        break;
                }

                fmt.Fprintf(os.Stderr, "%s: %s: invalid cgo directive: %q\n", os.Args[0L], file, f);
                nerrors++;

            }
            return ;

        }

        private static var seenlib = make_map<@string, bool>();

        private static void adddynlib(ptr<Link> _addr_ctxt, @string lib)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (seenlib[lib] || ctxt.LinkMode == LinkExternal)
            {
                return ;
            }

            seenlib[lib] = true;

            if (ctxt.IsELF)
            {
                var dsu = ctxt.loader.MakeSymbolUpdater(ctxt.DynStr2);
                if (dsu.Size() == 0L)
                {
                    dsu.Addstring("");
                }

                var du = ctxt.loader.MakeSymbolUpdater(ctxt.Dynamic2);
                Elfwritedynent2(ctxt.Arch, du, DT_NEEDED, uint64(dsu.Addstring(lib)));

            }
            else
            {
                Errorf(null, "adddynlib: unsupported binary format");
            }

        }

        public static void Adddynsym2(ptr<loader.Loader> _addr_ldr, ptr<Target> _addr_target, ptr<ArchSyms> _addr_syms, loader.Sym s)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref Target target = ref _addr_target.val;
            ref ArchSyms syms = ref _addr_syms.val;

            if (ldr.SymDynid(s) >= 0L || target.LinkMode == LinkExternal)
            {
                return ;
            }

            if (target.IsELF)
            {
                elfadddynsym2(ldr, target, syms, s);
            }
            else if (target.HeadType == objabi.Hdarwin)
            {
                ldr.Errorf(s, "adddynsym: missed symbol (Extname=%s)", ldr.SymExtname(s));
            }
            else if (target.HeadType == objabi.Hwindows)
            { 
                // already taken care of
            }
            else
            {
                ldr.Errorf(s, "adddynsym: unsupported binary format");
            }

        }

        private static void fieldtrack(ptr<sys.Arch> _addr_arch, ptr<loader.Loader> _addr_l)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Loader l = ref _addr_l.val;

            bytes.Buffer buf = default;
            for (var i = loader.Sym(1L); i < loader.Sym(l.NSym()); i++)
            {
                {
                    var name = l.SymName(i);

                    if (strings.HasPrefix(name, "go.track."))
                    {
                        var bld = l.MakeSymbolUpdater(i);
                        bld.SetSpecial(true);
                        bld.SetNotInSymbolTable(true);
                        if (bld.Reachable())
                        {
                            buf.WriteString(name[9L..]);
                            {
                                var p = l.Reachparent[i];

                                while (p != 0L)
                                {
                                    buf.WriteString("\t");
                                    buf.WriteString(l.SymName(p));
                                    p = l.Reachparent[p];
                                }

                            }
                            buf.WriteString("\n");

                            bld.SetType(sym.SCONST);
                            bld.SetValue(0L);

                        }

                    }

                }

            }

            l.Reachparent = null; // we are done with it
            if (flagFieldTrack == "".val)
            {
                return ;
            }

            var s = l.Lookup(flagFieldTrack.val, 0L);
            if (s == 0L || !l.AttrReachable(s))
            {
                return ;
            }

            bld = l.MakeSymbolUpdater(s);
            bld.SetType(sym.SDATA);
            addstrdata(arch, l, flagFieldTrack.val, buf.String());

        }

        private static void addexport(this ptr<Link> _addr_ctxt) => func((_, panic, __) =>
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Track undefined external symbols during external link.
            if (ctxt.LinkMode == LinkExternal)
            {
                foreach (var (_, s) in ctxt.Textp2)
                {
                    if (ctxt.loader.AttrSpecial(s) || ctxt.loader.AttrSubSymbol(s))
                    {
                        continue;
                    }

                    var relocs = ctxt.loader.Relocs(s);
                    for (long i = 0L; i < relocs.Count(); i++)
                    {
                        {
                            var rs = relocs.At2(i).Sym();

                            if (rs != 0L)
                            {
                                if (ctxt.loader.SymType(rs) == sym.Sxxx && !ctxt.loader.AttrLocal(rs))
                                { 
                                    // sanity check
                                    if (len(ctxt.loader.Data(rs)) != 0L)
                                    {
                                        panic("expected no data on undef symbol");
                                    }

                                    var su = ctxt.loader.MakeSymbolUpdater(rs);
                                    su.SetType(sym.SUNDEFEXT);

                                }

                            }

                        }

                    }


                }

            } 

            // TODO(aix)
            if (ctxt.HeadType == objabi.Hdarwin || ctxt.HeadType == objabi.Haix)
            {
                return ;
            }

            foreach (var (_, exp) in ctxt.dynexp2)
            {
                Adddynsym2(_addr_ctxt.loader, _addr_ctxt.Target, _addr_ctxt.ArchSyms, exp);
            }
            foreach (var (_, lib) in dynlib)
            {
                adddynlib(_addr_ctxt, lib);
            }

        });

        public partial struct Pkg
        {
            public bool mark;
            public bool @checked;
            public @string path;
            public slice<ptr<Pkg>> impby;
        }

        private static slice<ptr<Pkg>> pkgall = default;

        private static ptr<Pkg> cycle(this ptr<Pkg> _addr_p)
        {
            ref Pkg p = ref _addr_p.val;

            if (p.@checked)
            {
                return _addr_null!;
            }

            if (p.mark)
            {
                nerrors++;
                fmt.Printf("import cycle:\n");
                fmt.Printf("\t%s\n", p.path);
                return _addr_p!;
            }

            p.mark = true;
            foreach (var (_, q) in p.impby)
            {
                {
                    var bad = q.cycle();

                    if (bad != null)
                    {
                        p.mark = false;
                        p.@checked = true;
                        fmt.Printf("\timports %s\n", p.path);
                        if (bad == p)
                        {
                            return _addr_null!;
                        }

                        return _addr_bad!;

                    }

                }

            }
            p.@checked = true;
            p.mark = false;
            return _addr_null!;

        }

        private static void importcycles()
        {
            foreach (var (_, p) in pkgall)
            {
                p.cycle();
            }

        }
    }
}}}}
