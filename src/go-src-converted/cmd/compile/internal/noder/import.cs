// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:13:51 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\import.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using io = go.io_package;
using os = go.os_package;
using pathpkg = go.path_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using @base = go.cmd.compile.@internal.@base_package;
using importer = go.cmd.compile.@internal.importer_package;
using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using types2 = go.cmd.compile.@internal.types2_package;
using archive = go.cmd.@internal.archive_package;
using bio = go.cmd.@internal.bio_package;
using goobj = go.cmd.@internal.goobj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class noder_package {

    // Temporary import helper to get type2-based type-checking going.
private partial struct gcimports {
    public map<@string, ptr<types2.Package>> packages;
}

private static (ptr<types2.Package>, error) Import(this ptr<gcimports> _addr_m, @string path) {
    ptr<types2.Package> _p0 = default!;
    error _p0 = default!;
    ref gcimports m = ref _addr_m.val;

    return _addr_m.ImportFrom(path, "", 0)!;
}

private static (ptr<types2.Package>, error) ImportFrom(this ptr<gcimports> _addr_m, @string path, @string srcDir, types2.ImportMode mode) => func((_, panic, _) => {
    ptr<types2.Package> _p0 = default!;
    error _p0 = default!;
    ref gcimports m = ref _addr_m.val;

    if (mode != 0) {
        panic("mode must be 0");
    }
    var (path, err) = resolveImportPath(path);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    Func<@string, (io.ReadCloser, error)> lookup = path => _addr_openPackage(path)!;
    return _addr_importer.Import(m.packages, path, srcDir, lookup)!;

});

private static bool isDriveLetter(byte b) {
    return 'a' <= b && b <= 'z' || 'A' <= b && b <= 'Z';
}

// is this path a local name? begins with ./ or ../ or /
private static bool islocalname(@string name) {
    return strings.HasPrefix(name, "/") || runtime.GOOS == "windows" && len(name) >= 3 && isDriveLetter(name[0]) && name[1] == ':' && name[2] == '/' || strings.HasPrefix(name, "./") || name == "." || strings.HasPrefix(name, "../") || name == "..";
}

private static (ptr<os.File>, error) openPackage(@string path) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;

    if (islocalname(path)) {
        if (@base.Flag.NoLocalImports) {
            return (_addr_null!, error.As(errors.New("local imports disallowed"))!);
        }
        if (@base.Flag.Cfg.PackageFile != null) {
            return _addr_os.Open(@base.Flag.Cfg.PackageFile[path])!;
        }
        {
            var file__prev2 = file;

            var (file, err) = os.Open(fmt.Sprintf("%s.a", path));

            if (err == null) {
                return (_addr_file!, error.As(null!)!);
            }

            file = file__prev2;

        }

        {
            var file__prev2 = file;

            (file, err) = os.Open(fmt.Sprintf("%s.o", path));

            if (err == null) {
                return (_addr_file!, error.As(null!)!);
            }

            file = file__prev2;

        }

        return (_addr_null!, error.As(errors.New("file not found"))!);

    }
    {
        var q = pathpkg.Clean(path);

        if (q != path) {
            return (_addr_null!, error.As(fmt.Errorf("non-canonical import path %q (should be %q)", path, q))!);
        }
    }


    if (@base.Flag.Cfg.PackageFile != null) {
        return _addr_os.Open(@base.Flag.Cfg.PackageFile[path])!;
    }
    foreach (var (_, dir) in @base.Flag.Cfg.ImportDirs) {
        {
            var file__prev1 = file;

            (file, err) = os.Open(fmt.Sprintf("%s/%s.a", dir, path));

            if (err == null) {
                return (_addr_file!, error.As(null!)!);
            }

            file = file__prev1;

        }

        {
            var file__prev1 = file;

            (file, err) = os.Open(fmt.Sprintf("%s/%s.o", dir, path));

            if (err == null) {
                return (_addr_file!, error.As(null!)!);
            }

            file = file__prev1;

        }

    }    if (buildcfg.GOROOT != "") {
        @string suffix = "";
        if (@base.Flag.InstallSuffix != "") {
            suffix = "_" + @base.Flag.InstallSuffix;
        }
        else if (@base.Flag.Race) {
            suffix = "_race";
        }
        else if (@base.Flag.MSan) {
            suffix = "_msan";
        }
        {
            var file__prev2 = file;

            (file, err) = os.Open(fmt.Sprintf("%s/pkg/%s_%s%s/%s.a", buildcfg.GOROOT, buildcfg.GOOS, buildcfg.GOARCH, suffix, path));

            if (err == null) {
                return (_addr_file!, error.As(null!)!);
            }

            file = file__prev2;

        }

        {
            var file__prev2 = file;

            (file, err) = os.Open(fmt.Sprintf("%s/pkg/%s_%s%s/%s.o", buildcfg.GOROOT, buildcfg.GOOS, buildcfg.GOARCH, suffix, path));

            if (err == null) {
                return (_addr_file!, error.As(null!)!);
            }

            file = file__prev2;

        }

    }
    return (_addr_null!, error.As(errors.New("file not found"))!);

}

// myheight tracks the local package's height based on packages
// imported so far.
private static nint myheight = default;

// resolveImportPath resolves an import path as it appears in a Go
// source file to the package's full path.
private static (@string, error) resolveImportPath(@string path) {
    @string _p0 = default;
    error _p0 = default!;
 
    // The package name main is no longer reserved,
    // but we reserve the import path "main" to identify
    // the main package, just as we reserve the import
    // path "math" to identify the standard math package.
    if (path == "main") {
        return ("", error.As(errors.New("cannot import \"main\""))!);
    }
    if (@base.Ctxt.Pkgpath != "" && path == @base.Ctxt.Pkgpath) {
        return ("", error.As(fmt.Errorf("import %q while compiling that package (import cycle)", path))!);
    }
    {
        var (mapped, ok) = @base.Flag.Cfg.ImportMap[path];

        if (ok) {
            path = mapped;
        }
    }


    if (islocalname(path)) {
        if (path[0] == '/') {
            return ("", error.As(errors.New("import path cannot be absolute path"))!);
        }
        var prefix = @base.Flag.D;
        if (prefix == "") { 
            // Questionable, but when -D isn't specified, historically we
            // resolve local import paths relative to the directory the
            // compiler's current directory, not the respective source
            // file's directory.
            prefix = @base.Ctxt.Pathname;

        }
        path = pathpkg.Join(prefix, path);

        {
            var err = checkImportPath(path, true);

            if (err != null) {
                return ("", error.As(err)!);
            }

        }

    }
    return (path, error.As(null!)!);

}

// TODO(mdempsky): Return an error instead.
private static ptr<types.Pkg> importfile(ptr<syntax.ImportDecl> _addr_decl) => func((defer, _, _) => {
    ref syntax.ImportDecl decl = ref _addr_decl.val;

    if (decl.Path.Kind != syntax.StringLit) {
        @base.Errorf("import path must be a string");
        return _addr_null!;
    }
    var (path, err) = strconv.Unquote(decl.Path.Value);
    if (err != null) {
        @base.Errorf("import path must be a string");
        return _addr_null!;
    }
    {
        var err = checkImportPath(path, false);

        if (err != null) {
            @base.Errorf("%s", err.Error());
            return _addr_null!;
        }
    }


    path, err = resolveImportPath(path);
    if (err != null) {
        @base.Errorf("%s", err);
        return _addr_null!;
    }
    var importpkg = types.NewPkg(path, "");
    if (importpkg.Direct) {
        return _addr_importpkg!; // already fully loaded
    }
    importpkg.Direct = true;
    typecheck.Target.Imports = append(typecheck.Target.Imports, importpkg);

    if (path == "unsafe") {
        return _addr_importpkg!; // initialized with universe
    }
    var (f, err) = openPackage(path);
    if (err != null) {
        @base.Errorf("could not import %q: %v", path, err);
        @base.ErrorExit();
    }
    var imp = bio.NewReader(f);
    defer(imp.Close());
    var file = f.Name(); 

    // check object header
    var (p, err) = imp.ReadString('\n');
    if (err != null) {
        @base.Errorf("import %s: reading input: %v", file, err);
        @base.ErrorExit();
    }
    if (p == "!<arch>\n") { // package archive
        // package export block should be first
        var sz = archive.ReadHeader(imp.Reader, "__.PKGDEF");
        if (sz <= 0) {
            @base.Errorf("import %s: not a package file", file);
            @base.ErrorExit();
        }
        p, err = imp.ReadString('\n');
        if (err != null) {
            @base.Errorf("import %s: reading input: %v", file, err);
            @base.ErrorExit();
        }
    }
    if (!strings.HasPrefix(p, "go object ")) {
        @base.Errorf("import %s: not a go object file: %s", file, p);
        @base.ErrorExit();
    }
    var q = objabi.HeaderString();
    if (p != q) {
        @base.Errorf("import %s: object is [%s] expected [%s]", file, p, q);
        @base.ErrorExit();
    }
    while (true) {
        p, err = imp.ReadString('\n');
        if (err != null) {
            @base.Errorf("import %s: reading input: %v", file, err);
            @base.ErrorExit();
        }
        if (p == "\n") {
            break; // header ends with blank line
        }
    } 

    // Expect $$B\n to signal binary import format.

    // look for $$
    byte c = default;
    while (true) {
        c, err = imp.ReadByte();
        if (err != null) {
            break;
        }
        if (c == '$') {
            c, err = imp.ReadByte();
            if (c == '$' || err != null) {
                break;
            }
        }
    } 

    // get character after $$
    if (err == null) {
        c, _ = imp.ReadByte();
    }
    goobj.FingerprintType fingerprint = default;
    switch (c) {
        case '\n': 
            @base.Errorf("cannot import %s: old export format no longer supported (recompile library)", path);
            return _addr_null!;
            break;
        case 'B': 
            if (@base.Debug.Export != 0) {
                fmt.Printf("importing %s (%s)\n", path, file);
            }
            imp.ReadByte(); // skip \n after $$B

            c, err = imp.ReadByte();
            if (err != null) {
                @base.Errorf("import %s: reading input: %v", file, err);
                @base.ErrorExit();
            }
            if (c != 'i') {
                @base.Errorf("import %s: unexpected package format byte: %v", file, c);
                @base.ErrorExit();
            }
            fingerprint = typecheck.ReadImports(importpkg, imp);

            break;
        default: 
            @base.Errorf("no import in %q", path);
            @base.ErrorExit();
            break;
    } 

    // assume files move (get installed) so don't record the full path
    if (@base.Flag.Cfg.PackageFile != null) { 
        // If using a packageFile map, assume path_ can be recorded directly.
        @base.Ctxt.AddImport(path, fingerprint);

    }
    else
 { 
        // For file "/Users/foo/go/pkg/darwin_amd64/math.a" record "math.a".
        @base.Ctxt.AddImport(file[(int)len(file) - len(path) - len(".a")..], fingerprint);

    }
    if (importpkg.Height >= myheight) {
        myheight = importpkg.Height + 1;
    }
    return _addr_importpkg!;

});

// The linker uses the magic symbol prefixes "go." and "type."
// Avoid potential confusion between import paths and symbols
// by rejecting these reserved imports for now. Also, people
// "can do weird things in GOPATH and we'd prefer they didn't
// do _that_ weird thing" (per rsc). See also #4257.
private static @string reservedimports = new slice<@string>(new @string[] { "go", "type" });

private static error checkImportPath(@string path, bool allowSpace) {
    if (path == "") {
        return error.As(errors.New("import path is empty"))!;
    }
    if (strings.Contains(path, "\x00")) {
        return error.As(errors.New("import path contains NUL"))!;
    }
    foreach (var (_, ri) in reservedimports) {
        if (path == ri) {
            return error.As(fmt.Errorf("import path %q is reserved and cannot be used", path))!;
        }
    }    foreach (var (_, r) in path) {

        if (r == utf8.RuneError) 
            return error.As(fmt.Errorf("import path contains invalid UTF-8 sequence: %q", path))!;
        else if (r < 0x20 || r == 0x7f) 
            return error.As(fmt.Errorf("import path contains control character: %q", path))!;
        else if (r == '\\') 
            return error.As(fmt.Errorf("import path contains backslash; use slash: %q", path))!;
        else if (!allowSpace && unicode.IsSpace(r)) 
            return error.As(fmt.Errorf("import path contains space character: %q", path))!;
        else if (strings.ContainsRune("!\"#$%&'()*,:;<=>?[]^`{|}", r)) 
            return error.As(fmt.Errorf("import path contains invalid character '%c': %q", r, path))!;
        
    }    return error.As(null!)!;

}

private static void pkgnotused(src.XPos lineno, @string path, @string name) { 
    // If the package was imported with a name other than the final
    // import path element, show it explicitly in the error message.
    // Note that this handles both renamed imports and imports of
    // packages containing unconventional package declarations.
    // Note that this uses / always, even on Windows, because Go import
    // paths always use forward slashes.
    var elem = path;
    {
        var i = strings.LastIndex(elem, "/");

        if (i >= 0) {
            elem = elem[(int)i + 1..];
        }
    }

    if (name == "" || elem == name) {
        @base.ErrorfAt(lineno, "imported and not used: %q", path);
    }
    else
 {
        @base.ErrorfAt(lineno, "imported and not used: %q as %s", path, name);
    }
}

private static void mkpackage(@string pkgname) {
    if (types.LocalPkg.Name == "") {
        if (pkgname == "_") {
            @base.Errorf("invalid package name _");
        }
        types.LocalPkg.Name = pkgname;

    }
    else
 {
        if (pkgname != types.LocalPkg.Name) {
            @base.Errorf("package %s; expected %s", pkgname, types.LocalPkg.Name);
        }
    }
}

private static void clearImports() {
    private partial struct importedPkg {
        public src.XPos pos;
        public @string path;
        public @string name;
    }
    slice<importedPkg> unused = default;

    foreach (var (_, s) in types.LocalPkg.Syms) {
        var n = ir.AsNode(s.Def);
        if (n == null) {
            continue;
        }
        if (n.Op() == ir.OPACK) { 
            // throw away top-level package name left over
            // from previous file.
            // leave s->block set to cause redeclaration
            // errors if a conflicting top-level name is
            // introduced by a different file.
            ptr<ir.PkgName> p = n._<ptr<ir.PkgName>>();
            if (!p.Used && @base.SyntaxErrors() == 0) {
                unused = append(unused, new importedPkg(p.Pos(),p.Pkg.Path,s.Name));
            }

            s.Def = null;
            continue;

        }
        if (types.IsDotAlias(s)) { 
            // throw away top-level name left over
            // from previous import . "x"
            // We'll report errors after type checking in CheckDotImports.
            s.Def = null;
            continue;

        }
    }    sort.Slice(unused, (i, j) => unused[i].pos.Before(unused[j].pos));
    foreach (var (_, pkg) in unused) {
        pkgnotused(pkg.pos, pkg.path, pkg.name);
    }
}

// CheckDotImports reports errors for any unused dot imports.
public static void CheckDotImports() {
    foreach (var (_, pack) in dotImports) {
        if (!pack.Used) {
            @base.ErrorfAt(pack.Pos(), "imported and not used: %q", pack.Pkg.Path);
        }
    }    dotImports = null;
    typecheck.DotImportRefs = null;

}

// dotImports tracks all PkgNames that have been dot-imported.
private static slice<ptr<ir.PkgName>> dotImports = default;

// find all the exported symbols in package referenced by PkgName,
// and make them available in the current package
private static void importDot(ptr<ir.PkgName> _addr_pack) {
    ref ir.PkgName pack = ref _addr_pack.val;

    if (typecheck.DotImportRefs == null) {
        typecheck.DotImportRefs = make_map<ptr<ir.Ident>, ptr<ir.PkgName>>();
    }
    var opkg = pack.Pkg;
    foreach (var (_, s) in opkg.Syms) {
        if (s.Def == null) {
            {
                var (_, ok) = typecheck.DeclImporter[s];

                if (!ok) {
                    continue;
                }

            }

        }
        if (!types.IsExported(s.Name) || strings.ContainsRune(s.Name, 0xb7)) { // 0xb7 = center dot
            continue;

        }
        var s1 = typecheck.Lookup(s.Name);
        if (s1.Def != null) {
            var pkgerror = fmt.Sprintf("during import %q", opkg.Path);
            typecheck.Redeclared(@base.Pos, s1, pkgerror);
            continue;
        }
        var id = ir.NewIdent(src.NoXPos, s);
        typecheck.DotImportRefs[id] = pack;
        s1.Def = id;
        s1.Block = 1;

    }    dotImports = append(dotImports, pack);

}

// importName is like oldname,
// but it reports an error if sym is from another package and not exported.
private static ir.Node importName(ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    var n = oldname(sym);
    if (!types.IsExported(sym.Name) && sym.Pkg != types.LocalPkg) {
        n.SetDiag(true);
        @base.Errorf("cannot refer to unexported name %s.%s", sym.Pkg.Name, sym.Name);
    }
    return n;

}

} // end noder_package
