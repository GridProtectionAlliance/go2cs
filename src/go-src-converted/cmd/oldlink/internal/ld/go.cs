// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go-specific code shared across loaders (5l, 6l, 8l).

// package ld -- go2cs converted at 2020 October 08 04:41:11 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\go.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
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
            if (flagNewobj.val)
            { 
                // Record the directives. We'll process them later after Symbols are created.
                ctxt.cgodata = append(ctxt.cgodata, new cgodata(file,pkg,directives));

            }
            else
            {
                setCgoAttr(_addr_ctxt, ctxt.Syms.Lookup, file, pkg, directives);
            }

        }

        // Set symbol attributes or flags based on cgo directives.
        private static ptr<sym.Symbol> setCgoAttr(ptr<Link> _addr_ctxt, Func<@string, long, ptr<sym.Symbol>> lookup, @string file, @string pkg, slice<slice<@string>> directives)
        {
            ref Link ctxt = ref _addr_ctxt.val;

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
                        if (s.Type == 0L || s.Type == sym.SXREF || s.Type == sym.SBSS || s.Type == sym.SNOPTRBSS || s.Type == sym.SHOSTOBJ)
                        {
                            s.SetDynimplib(lib);
                            s.SetExtname(remote);
                            s.SetDynimpvers(q);
                            if (s.Type != sym.SHOSTOBJ)
                            {
                                s.Type = sym.SDYNIMPORT;
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
                        s.Type = sym.SHOSTOBJ;
                        s.Size = 0L;
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


                        if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModePlugin) 
                            if (s == lookup("main", 0L))
                            {
                                continue;
                            }

                        // export overrides import, for openbsd/cgo.
                        // see issue 4878.
                        if (s.Dynimplib() != "")
                        {
                            s.ResetDyninfo();
                            s.SetExtname("");
                            s.Type = 0L;
                        }

                        if (!s.Attr.CgoExport())
                        {
                            s.SetExtname(remote);
                        }
                        else if (s.Extname() != remote)
                        {
                            fmt.Fprintf(os.Stderr, "%s: conflicting cgo_export directives: %s as %s and %s\n", os.Args[0L], s.Name, s.Extname(), remote);
                            nerrors++;
                            return ;
                        }

                        if (f[0L] == "cgo_export_static")
                        {
                            s.Attr |= sym.AttrCgoExportStatic;
                        }
                        else
                        {
                            s.Attr |= sym.AttrCgoExportDynamic;
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
                var s = ctxt.Syms.Lookup(".dynstr", 0L);
                if (s.Size == 0L)
                {
                    Addstring(s, "");
                }

                Elfwritedynent(ctxt, ctxt.Syms.Lookup(".dynamic", 0L), DT_NEEDED, uint64(Addstring(s, lib)));

            }
            else
            {
                Errorf(null, "adddynlib: unsupported binary format");
            }

        }

        public static void Adddynsym(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Dynid >= 0L || ctxt.LinkMode == LinkExternal)
            {
                return ;
            }

            if (ctxt.IsELF)
            {
                elfadddynsym(ctxt, s);
            }
            else if (ctxt.HeadType == objabi.Hdarwin)
            {
                Errorf(s, "adddynsym: missed symbol (Extname=%s)", s.Extname());
            }
            else if (ctxt.HeadType == objabi.Hwindows)
            { 
                // already taken care of
            }
            else
            {
                Errorf(s, "adddynsym: unsupported binary format");
            }

        }

        private static void fieldtrack(ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // record field tracking references
            bytes.Buffer buf = default;
            {
                var s__prev1 = s;

                foreach (var (_, __s) in ctxt.Syms.Allsym)
                {
                    s = __s;
                    if (strings.HasPrefix(s.Name, "go.track."))
                    {
                        s.Attr |= sym.AttrSpecial; // do not lay out in data segment
                        s.Attr |= sym.AttrNotInSymbolTable;
                        if (s.Attr.Reachable())
                        {
                            buf.WriteString(s.Name[9L..]);
                            {
                                var p = ctxt.Reachparent[s];

                                while (p != null)
                                {
                                    buf.WriteString("\t");
                                    buf.WriteString(p.Name);
                                    p = ctxt.Reachparent[p];
                                }

                            }
                            buf.WriteString("\n");

                        }

                        s.Type = sym.SCONST;
                        s.Value = 0L;

                    }

                }

                s = s__prev1;
            }

            if (flagFieldTrack == "".val)
            {
                return ;
            }

            var s = ctxt.Syms.ROLookup(flagFieldTrack.val, 0L);
            if (s == null || !s.Attr.Reachable())
            {
                return ;
            }

            s.Type = sym.SDATA;
            addstrdata(ctxt, flagFieldTrack.val, buf.String());

        }

        private static void addexport(this ptr<Link> _addr_ctxt)
        {
            ref Link ctxt = ref _addr_ctxt.val;
 
            // Track undefined external symbols during external link.
            if (ctxt.LinkMode == LinkExternal)
            {
                foreach (var (_, s) in ctxt.Syms.Allsym)
                {
                    if (!s.Attr.Reachable() || s.Attr.Special() || s.Attr.SubSymbol())
                    {
                        continue;
                    }

                    if (s.Type != sym.STEXT)
                    {
                        continue;
                    }

                    foreach (var (i) in s.R)
                    {
                        var r = _addr_s.R[i];
                        if (r.Sym != null && r.Sym.Type == sym.Sxxx)
                        {
                            r.Sym.Type = sym.SUNDEFEXT;
                        }

                    }

                }

            } 

            // TODO(aix)
            if (ctxt.HeadType == objabi.Hdarwin || ctxt.HeadType == objabi.Haix)
            {
                return ;
            }

            foreach (var (_, exp) in dynexp)
            {
                Adddynsym(_addr_ctxt, _addr_exp);
            }
            foreach (var (_, lib) in dynlib)
            {
                adddynlib(_addr_ctxt, lib);
            }

        }

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
