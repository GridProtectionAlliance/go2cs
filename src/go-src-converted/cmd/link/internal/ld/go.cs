// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go-specific code shared across loaders (5l, 6l, 8l).

// package ld -- go2cs converted at 2020 August 29 10:03:44 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\go.go
using bytes = go.bytes_package;
using bio = go.cmd.@internal.bio_package;
using objabi = go.cmd.@internal.objabi_package;
using sym = go.cmd.link.@internal.sym_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

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

        // TODO:
        //    generate debugging section in binary.
        //    once the dust settles, try to move some code to
        //        libmach, so that other linkers and ar can share.

        private static void ldpkg(ref Link ctxt, ref bio.Reader f, @string pkg, long length, @string filename, long whence)
        {
            if (flagG.Value)
            {
                return;
            }
            if (int64(int(length)) != length)
            {
                fmt.Fprintf(os.Stderr, "%s: too much pkg data in %s\n", os.Args[0L], filename);
                if (flagU.Value)
                {
                    errorexit();
                }
                return;
            } 

            // In a __.PKGDEF, we only care about the package name.
            // Don't read all the export data.
            if (length > 1000L && whence == Pkgdef)
            {
                length = 1000L;
            }
            var bdata = make_slice<byte>(length);
            {
                var (_, err) = io.ReadFull(f, bdata);

                if (err != null)
                {
                    fmt.Fprintf(os.Stderr, "%s: short pkg read %s\n", os.Args[0L], filename);
                    if (flagU.Value)
                    {
                        errorexit();
                    }
                    return;
                }

            }
            var data = string(bdata); 

            // process header lines
            var isSafe = false;
            var isMain = false;
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
                    isSafe = true;
                }
                if (line == "main")
                {
                    isMain = true;
                }
                if (line == "")
                {
                    break;
                }
            }


            if (whence == Pkgdef || whence == FileObj)
            {
                if (pkg == "main" && !isMain)
                {
                    Exitf("%s: not package main", filename);
                }
                if (flagU && whence != ArchiveObj && !isSafe.Value)
                {
                    Exitf("load of unsafe package %s", filename);
                }
            } 

            // __.PKGDEF has no cgo section - those are in the C compiler-generated object files.
            if (whence == Pkgdef)
            {
                return;
            } 

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
                    if (flagU.Value)
                    {
                        errorexit();
                    }
                    return;
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
                    if (flagU.Value)
                    {
                        errorexit();
                    }
                    return;
                }
                p1 += p0;

                loadcgo(ctxt, filename, pkg, data[p0..p1]);
            }
        }

        private static void loadcgo(ref Link ctxt, @string file, @string pkg, @string p)
        {
            @string next = default;
            @string q = default;
            @string lib = default;
            ref sym.Symbol s = default;

            @string p0 = "";
            while (p != "")
            {
                {
                    var i__prev1 = i;

                    var i = strings.Index(p, "\n");

                    if (i >= 0L)
                    {
                        p = p[..i];
                        next = p[i + 1L..];
                p = next;
                    }
                    else
                    {
                        next = "";
                    }

                    i = i__prev1;

                }

                p0 = p; // save for error message
                var f = tokenize(p);
                if (len(f) == 0L)
                {
                    continue;
                }
                if (f[0L] == "cgo_import_dynamic")
                {
                    if (len(f) < 2L || len(f) > 4L)
                    {
                        goto err;
                    }
                    var local = f[1L];
                    var remote = local;
                    if (len(f) > 2L)
                    {
                        remote = f[2L];
                    }
                    lib = "";
                    if (len(f) > 3L)
                    {
                        lib = f[3L];
                    }
                    if (FlagD.Value)
                    {
                        fmt.Fprintf(os.Stderr, "%s: %s: cannot use dynamic imports with -d flag\n", os.Args[0L], file);
                        nerrors++;
                        return;
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
                    q = "";
                    {
                        var i__prev2 = i;

                        i = strings.Index(remote, "#");

                        if (i >= 0L)
                        {
                            remote = remote[..i];
                            q = remote[i + 1L..];
                        }

                        i = i__prev2;

                    }
                    s = ctxt.Syms.Lookup(local, 0L);
                    if (s.Type == 0L || s.Type == sym.SXREF || s.Type == sym.SHOSTOBJ)
                    {
                        s.Dynimplib = lib;
                        s.Extname = remote;
                        s.Dynimpvers = q;
                        if (s.Type != sym.SHOSTOBJ)
                        {
                            s.Type = sym.SDYNIMPORT;
                        }
                        havedynamic = 1L;
                    }
                    continue;
                }
                if (f[0L] == "cgo_import_static")
                {
                    if (len(f) != 2L)
                    {
                        goto err;
                    }
                    local = f[1L];
                    s = ctxt.Syms.Lookup(local, 0L);
                    s.Type = sym.SHOSTOBJ;
                    s.Size = 0L;
                    continue;
                }
                if (f[0L] == "cgo_export_static" || f[0L] == "cgo_export_dynamic")
                {
                    if (len(f) < 2L || len(f) > 3L)
                    {
                        goto err;
                    }
                    local = f[1L];
                    remote = default;
                    if (len(f) > 2L)
                    {
                        remote = f[2L];
                    }
                    else
                    {
                        remote = local;
                    }
                    local = expandpkg(local, pkg);
                    s = ctxt.Syms.Lookup(local, 0L);


                    if (ctxt.BuildMode == BuildModeCShared || ctxt.BuildMode == BuildModeCArchive || ctxt.BuildMode == BuildModePlugin) 
                        if (s == ctxt.Syms.Lookup("main", 0L))
                        {
                            continue;
                        }
                    // export overrides import, for openbsd/cgo.
                    // see issue 4878.
                    if (s.Dynimplib != "")
                    {
                        s.Dynimplib = "";
                        s.Extname = "";
                        s.Dynimpvers = "";
                        s.Type = 0L;
                    }
                    if (!s.Attr.CgoExport())
                    {
                        s.Extname = remote;
                        dynexp = append(dynexp, s);
                    }
                    else if (s.Extname != remote)
                    {
                        fmt.Fprintf(os.Stderr, "%s: conflicting cgo_export directives: %s as %s and %s\n", os.Args[0L], s.Name, s.Extname, remote);
                        nerrors++;
                        return;
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
                }
                if (f[0L] == "cgo_dynamic_linker")
                {
                    if (len(f) != 2L)
                    {
                        goto err;
                    }
                    if (flagInterpreter == "".Value)
                    {
                        if (interpreter != "" && interpreter != f[1L])
                        {
                            fmt.Fprintf(os.Stderr, "%s: conflict dynlinker: %s and %s\n", os.Args[0L], interpreter, f[1L]);
                            nerrors++;
                            return;
                        }
                        interpreter = f[1L];
                    }
                    continue;
                }
                if (f[0L] == "cgo_ldflag")
                {
                    if (len(f) != 2L)
                    {
                        goto err;
                    }
                    ldflag = append(ldflag, f[1L]);
                    continue;
                }
            }


            return;

err:
            fmt.Fprintf(os.Stderr, "%s: %s: invalid dynimport line: %s\n", os.Args[0L], file, p0);
            nerrors++;
        }

        private static var seenlib = make_map<@string, bool>();

        private static void adddynlib(ref Link ctxt, @string lib)
        {
            if (seenlib[lib] || ctxt.LinkMode == LinkExternal)
            {
                return;
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

        public static void Adddynsym(ref Link ctxt, ref sym.Symbol s)
        {
            if (s.Dynid >= 0L || ctxt.LinkMode == LinkExternal)
            {
                return;
            }
            if (ctxt.IsELF)
            {
                elfadddynsym(ctxt, s);
            }
            else if (ctxt.HeadType == objabi.Hdarwin)
            {
                Errorf(s, "adddynsym: missed symbol (Extname=%s)", s.Extname);
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

        private static void fieldtrack(ref Link ctxt)
        { 
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
                                var p = s.Reachparent;

                                while (p != null)
                                {
                                    buf.WriteString("\t");
                                    buf.WriteString(p.Name);
                                    p = p.Reachparent;
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

            if (flagFieldTrack == "".Value)
            {
                return;
            }
            var s = ctxt.Syms.ROLookup(flagFieldTrack.Value, 0L);
            if (s == null || !s.Attr.Reachable())
            {
                return;
            }
            s.Type = sym.SDATA;
            addstrdata(ctxt, flagFieldTrack.Value, buf.String());
        }

        private static void addexport(this ref Link ctxt)
        {
            if (ctxt.HeadType == objabi.Hdarwin)
            {
                return;
            }
            foreach (var (_, exp) in dynexp)
            {
                Adddynsym(ctxt, exp);
            }
            foreach (var (_, lib) in dynlib)
            {
                adddynlib(ctxt, lib);
            }
        }

        public partial struct Pkg
        {
            public bool mark;
            public bool @checked;
            public @string path;
            public slice<ref Pkg> impby;
        }

        private static slice<ref Pkg> pkgall = default;

        private static ref Pkg cycle(this ref Pkg p)
        {
            if (p.@checked)
            {
                return null;
            }
            if (p.mark)
            {
                nerrors++;
                fmt.Printf("import cycle:\n");
                fmt.Printf("\t%s\n", p.path);
                return p;
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
                            return null;
                        }
                        return bad;
                    }

                }
            }
            p.@checked = true;
            p.mark = false;
            return null;
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
