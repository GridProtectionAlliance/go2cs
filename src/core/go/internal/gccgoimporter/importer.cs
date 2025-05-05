// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gccgoimporter implements Import for gccgo-generated object files.
namespace go.go.@internal;

// import "go/internal/gccgoimporter"
using bytes = bytes_package;
using elf = debug.elf_package;
using fmt = fmt_package;
using types = go.types_package;
using xcoff = @internal.xcoff_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using @internal;
using debug;
using go;
using path;

partial class gccgoimporter_package {

// A PackageInit describes an imported package that needs initialization.
[GoType] partial struct PackageInit {
    public @string Name; // short package name
    public @string InitFunc; // name of init function
    public nint Priority;   // priority of init function, see InitData.Priority
}

// The gccgo-specific init data for a package.
[GoType] partial struct InitData {
    // Initialization priority of this package relative to other packages.
    // This is based on the maximum depth of the package's dependency graph;
    // it is guaranteed to be greater than that of its dependencies.
    public nint Priority;
    // The list of packages which this package depends on to be initialized,
    // including itself if needed. This is the subset of the transitive closure of
    // the package's dependencies that need initialization.
    public slice<PackageInit> Inits;
}

// Locate the file from which to read export data.
// This is intended to replicate the logic in gofrontend.
internal static (@string, error) findExportFile(slice<@string> searchpaths, @string pkgpath) {
    foreach (var (_, spath) in searchpaths) {
        @string pkgfullpath = filepath.Join(spath, pkgpath);
        var (pkgdir, name) = filepath.Split(pkgfullpath);
        foreach (var (_, filepath) in new @string[]{
            pkgfullpath,
            pkgfullpath + ".gox"u8,
            pkgdir + "lib"u8 + name + ".so"u8,
            pkgdir + "lib"u8 + name + ".a"u8,
            pkgfullpath + ".o"u8
        }.array()) {
            (fi, err) = os.Stat(filepath);
            if (err == default! && !fi.IsDir()) {
                return (filepath, default!);
            }
        }
    }
    return ("", fmt.Errorf("%s: could not find export data (tried %s)"u8, pkgpath, strings.Join(searchpaths, ":"u8)));
}

internal static readonly @string gccgov1Magic = "v1;\n"u8;
internal static readonly @string gccgov2Magic = "v2;\n"u8;
internal static readonly @string gccgov3Magic = "v3;\n"u8;
internal static readonly @string goimporterMagic = "\n$$ "u8;
internal static readonly @string archiveMagic = "!<ar"u8;
internal static readonly @string aixbigafMagic = "<big"u8;

// Opens the export data file at the given path. If this is an ELF file,
// searches for and opens the .go_export section. If this is an archive,
// reads the export data from the first member, which is assumed to be an ELF file.
// This is intended to replicate the logic in gofrontend.
internal static (io.ReadSeeker reader, io.Closer closer, error err) openExportFile(@string fpath) => func((defer, _) => {
    io.ReadSeeker reader = default!;
    io.Closer closer = default!;
    error err = default!;

    (f, err) = os.Open(fpath);
    if (err != default!) {
        return (reader, closer, err);
    }
    closer = ~f;
    var errʗ1 = err;
    var fʗ1 = f;
    defer(() => {
        if (errʗ1 != default! && closer != default!) {
            fʗ1.Close();
        }
    });
    array<byte> magic = new(4);
    (_, err) = f.ReadAt(magic[..], 0);
    if (err != default!) {
        return (reader, closer, err);
    }
    io.ReaderAt objreader = default!;
    var exprᴛ1 = ((@string)(magic[..]));
    if (exprᴛ1 == gccgov1Magic || exprᴛ1 == gccgov2Magic || exprᴛ1 == gccgov3Magic || exprᴛ1 == goimporterMagic) {
        reader = ~f;
        return (reader, closer, err);
    }
    if (exprᴛ1 == archiveMagic || exprᴛ1 == aixbigafMagic) {
        (reader, err) = arExportData(~f);
        return (reader, closer, err);
    }
    { /* default: */
        objreader = ~f;
    }

    // Raw export data.
    (ef, err) = elf.NewFile(objreader);
    if (err == default!) {
        var sec = ef.Section(".go_export"u8);
        if (sec == nil) {
            err = fmt.Errorf("%s: .go_export section not found"u8, fpath);
            return (reader, closer, err);
        }
        reader = sec.Open();
        return (reader, closer, err);
    }
    (xf, err) = xcoff.NewFile(objreader);
    if (err == default!) {
        var sdat = xf.CSect(".go_export"u8);
        if (sdat == default!) {
            err = fmt.Errorf("%s: .go_export section not found"u8, fpath);
            return (reader, closer, err);
        }
        reader = ~bytes.NewReader(sdat);
        return (reader, closer, err);
    }
    err = fmt.Errorf("%s: unrecognized file format"u8, fpath);
    return (reader, closer, err);
});

public delegate (ж<types.Package>, error) Importer(types.Package imports, @string path, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup);

[GoType("dyn")] partial interface GetImporter_type {
    @string Name();
}

public static Importer GetImporter(slice<@string> searchpaths, types.Package>InitData initmap) => func((defer, _) => {
    var initmapʗ1 = initmap;
    var searchpathsʗ1 = searchpaths;
    return (types.Package imports, @string pkgpath, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup) => {
        // TODO(gri): Use srcDir.
        // Or not. It's possible that srcDir will fade in importance as
        // the go command and other tools provide a translation table
        // for relative imports (like ./foo or vendored imports).
        if (pkgpath == "unsafe"u8) {
            return (types.Unsafe, default!);
        }
        io.ReadSeeker reader = default!;
        @string fpath = default!;
        io.ReadCloser rc = default!;
        if (lookup != default!) {
            {
                var p = imports[pkgpath]; if (p != nil && p.Complete()) {
                    return (p, default!);
                }
            }
            (rc, err) = lookup(pkgpath);
            if (err != default!) {
                return (default!, err);
            }
        }
        if (rc != default!){
            var rcʗ1 = rc;
            defer(rcʗ1.Close);
            var (rs, ok) = rc._<io.ReadSeeker>(ᐧ);
            if (!ok) {
                return (default!, fmt.Errorf("gccgo importer requires lookup to return an io.ReadSeeker, have %T"u8, rc));
            }
            reader = rs;
            fpath = "<lookup "u8 + pkgpath + ">"u8;
            // Take name from Name method (like on os.File) if present.
            {
                var (n, okΔ1) = rc._<GetImporter_type>(ᐧ); if (okΔ1) {
                    fpath = n.Name();
                }
            }
        } else {
            (fpath, err) = findExportFile(searchpaths, pkgpath);
            if (err != default!) {
                return (default!, err);
            }
            (r, closer, errΔ1) = openExportFile(fpath);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            if (closer != default!) {
                var closerʗ1 = closer;
                defer(closerʗ1.Close);
            }
            reader = r;
        }
        @string magics = default!;
        (magics, err) = readMagic(reader);
        if (err != default!) {
            return;
        }
        if (magics == archiveMagic || magics == aixbigafMagic) {
            (reader, err) = arExportData(reader);
            if (err != default!) {
                return;
            }
            (magics, err) = readMagic(reader);
            if (err != default!) {
                return;
            }
        }
        var exprᴛ1 = magics;
        if (exprᴛ1 == gccgov1Magic || exprᴛ1 == gccgov2Magic || exprᴛ1 == gccgov3Magic) {
            ref var p = ref heap(new parser(), out var Ꮡp);
            p.init(fpath, reader, imports);
            pkg = p.parsePackage();
            if (initmap != default!) {
                initmap[pkg] = p.initdata;
            }
        }
        else { /* default: */
            err = fmt.Errorf("unrecognized magic string: %q"u8, // Excluded for now: Standard gccgo doesn't support this import format currently.
 // case goimporterMagic:
 // 	var data []byte
 // 	data, err = io.ReadAll(reader)
 // 	if err != nil {
 // 		return
 // 	}
 // 	var n int
 // 	n, pkg, err = importer.ImportData(imports, data)
 // 	if err != nil {
 // 		return
 // 	}
 // 	if initmap != nil {
 // 		suffixreader := bytes.NewReader(data[n:])
 // 		var p parser
 // 		p.init(fpath, suffixreader, nil)
 // 		p.parseInitData()
 // 		initmap[pkg] = p.initdata
 // 	}
 magics);
        }

        return;
    };
});

// readMagic reads the four bytes at the start of a ReadSeeker and
// returns them as a string.
internal static (@string, error) readMagic(io.ReadSeeker reader) {
    array<byte> magic = new(4);
    {
        var (_, err) = reader.Read(magic[..]); if (err != default!) {
            return ("", err);
        }
    }
    {
        var (_, err) = reader.Seek(0, io.SeekStart); if (err != default!) {
            return ("", err);
        }
    }
    return (((@string)(magic[..])), default!);
}

} // end gccgoimporter_package
