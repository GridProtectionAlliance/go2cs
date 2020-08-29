// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gccgoimporter implements Import for gccgo-generated object files.
// package gccgoimporter -- go2cs converted at 2020 August 29 10:09:16 UTC
// import "go/internal/gccgoimporter" ==> using gccgoimporter = go.go.@internal.gccgoimporter_package
// Original source: C:\Go\src\go\internal\gccgoimporter\importer.go
// import "go/internal/gccgoimporter"

using bytes = go.bytes_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using io = go.io_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gccgoimporter_package
    {
        // A PackageInit describes an imported package that needs initialization.
        public partial struct PackageInit
        {
            public @string Name; // short package name
            public @string InitFunc; // name of init function
            public long Priority; // priority of init function, see InitData.Priority
        }

        // The gccgo-specific init data for a package.
        public partial struct InitData
        {
            public long Priority; // The list of packages which this package depends on to be initialized,
// including itself if needed. This is the subset of the transitive closure of
// the package's dependencies that need initialization.
            public slice<PackageInit> Inits;
        }

        // Locate the file from which to read export data.
        // This is intended to replicate the logic in gofrontend.
        private static (@string, error) findExportFile(slice<@string> searchpaths, @string pkgpath)
        {
            foreach (var (_, spath) in searchpaths)
            {
                var pkgfullpath = filepath.Join(spath, pkgpath);
                var (pkgdir, name) = filepath.Split(pkgfullpath);

                foreach (var (_, filepath) in new array<@string>(new @string[] { pkgfullpath, pkgfullpath+".gox", pkgdir+"lib"+name+".so", pkgdir+"lib"+name+".a", pkgfullpath+".o" }))
                {
                    var (fi, err) = os.Stat(filepath);
                    if (err == null && !fi.IsDir())
                    {
                        return (filepath, null);
                    }
                }
            }
            return ("", fmt.Errorf("%s: could not find export data (tried %s)", pkgpath, strings.Join(searchpaths, ":")));
        }

        private static readonly @string gccgov1Magic = "v1;\n";
        private static readonly @string gccgov2Magic = "v2;\n";
        private static readonly @string goimporterMagic = "\n$$ ";
        private static readonly @string archiveMagic = "!<ar";

        // Opens the export data file at the given path. If this is an ELF file,
        // searches for and opens the .go_export section. If this is an archive,
        // reads the export data from the first member, which is assumed to be an ELF file.
        // This is intended to replicate the logic in gofrontend.
        private static (io.ReadSeeker, io.Closer, error) openExportFile(@string fpath) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(fpath);
            if (err != null)
            {
                return;
            }
            closer = f;
            defer(() =>
            {
                if (err != null && closer != null)
                {
                    f.Close();
                }
            }());

            array<byte> magic = new array<byte>(4L);
            _, err = f.ReadAt(magic[..], 0L);
            if (err != null)
            {
                return;
            }
            io.ReaderAt elfreader = default;

            if (string(magic[..]) == gccgov1Magic || string(magic[..]) == gccgov2Magic || string(magic[..]) == goimporterMagic) 
                // Raw export data.
                reader = f;
                return;
            else if (string(magic[..]) == archiveMagic) 
                // TODO(pcc): Read the archive directly instead of using "ar".
                f.Close();
                closer = null;

                var cmd = exec.Command("ar", "p", fpath);
                slice<byte> @out = default;
                out, err = cmd.Output();
                if (err != null)
                {
                    return;
                }
                elfreader = bytes.NewReader(out);
            else 
                elfreader = f;
                        var (ef, err) = elf.NewFile(elfreader);
            if (err != null)
            {
                return;
            }
            var sec = ef.Section(".go_export");
            if (sec == null)
            {
                err = fmt.Errorf("%s: .go_export section not found", fpath);
                return;
            }
            reader = sec.Open();
            return;
        });

        // An Importer resolves import paths to Packages. The imports map records
        // packages already known, indexed by package path.
        // An importer must determine the canonical package path and check imports
        // to see if it is already present in the map. If so, the Importer can return
        // the map entry. Otherwise, the importer must load the package data for the
        // given path into a new *Package, record it in imports map, and return the
        // package.
        public delegate  error) Importer(map<@string,  ref types.Package>,  @string,  @string,  Func<@string,  (io.ReadCloser,  error)>,  (ref types.Package);

        public static Importer GetImporter(slice<@string> searchpaths, map<ref types.Package, InitData> initmap) => func((defer, _, __) =>
        {
            return (imports, pkgpath, srcDir, lookup) =>
            { 
                // TODO(gri): Use srcDir.
                // Or not. It's possible that srcDir will fade in importance as
                // the go command and other tools provide a translation table
                // for relative imports (like ./foo or vendored imports).
                if (pkgpath == "unsafe")
                {
                    return (types.Unsafe, null);
                }
                io.ReadSeeker reader = default;
                @string fpath = default;
                if (lookup != null)
                {
                    {
                        var p__prev2 = p;

                        var p = imports[pkgpath];

                        if (p != null && p.Complete())
                        {
                            return (p, null);
                        }

                        p = p__prev2;

                    }
                    var (rc, err) = lookup(pkgpath);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    defer(rc.Close());
                    io.ReadSeeker (rs, ok) = rc._<io.ReadSeeker>();
                    if (!ok)
                    {
                        return (null, fmt.Errorf("gccgo importer requires lookup to return an io.ReadSeeker, have %T", rc));
                    }
                    reader = rs;
                    fpath = "<lookup " + pkgpath + ">"; 
                    // Take name from Name method (like on os.File) if present.
                    {

                        if (ok)
                        {
                            fpath = n.Name();
                        }

                    }
                }
                else
                {
                    fpath, err = findExportFile(searchpaths, pkgpath);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    var (r, closer, err) = openExportFile(fpath);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    if (closer != null)
                    {
                        defer(closer.Close());
                    }
                    reader = r;
                }
                array<byte> magic = new array<byte>(4L);
                _, err = reader.Read(magic[..]);
                if (err != null)
                {
                    return;
                }
                _, err = reader.Seek(0L, io.SeekStart);
                if (err != null)
                {
                    return;
                }

                if (string(magic[..]) == gccgov1Magic || string(magic[..]) == gccgov2Magic) 
                    p = default;
                    p.init(fpath, reader, imports);
                    pkg = p.parsePackage();
                    if (initmap != null)
                    {
                        initmap[pkg] = p.initdata;
                    } 

                    // Excluded for now: Standard gccgo doesn't support this import format currently.
                    // case goimporterMagic:
                    //     var data []byte
                    //     data, err = ioutil.ReadAll(reader)
                    //     if err != nil {
                    //         return
                    //     }
                    //     var n int
                    //     n, pkg, err = importer.ImportData(imports, data)
                    //     if err != nil {
                    //         return
                    //     }

                    //     if initmap != nil {
                    //         suffixreader := bytes.NewReader(data[n:])
                    //         var p parser
                    //         p.init(fpath, suffixreader, nil)
                    //         p.parseInitData()
                    //         initmap[pkg] = p.initdata
                    //     }
                else 
                    err = fmt.Errorf("unrecognized magic string: %q", string(magic[..]));
                                return;
            }
;
        });
    }
}}}
