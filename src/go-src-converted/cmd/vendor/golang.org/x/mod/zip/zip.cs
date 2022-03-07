// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package zip provides functions for creating and extracting module zip files.
//
// Module zip files have several restrictions listed below. These are necessary
// to ensure that module zip files can be extracted consistently on supported
// platforms and file systems.
//
// • All file paths within a zip file must start with "<module>@<version>/",
// where "<module>" is the module path and "<version>" is the version.
// The module path must be valid (see golang.org/x/mod/module.CheckPath).
// The version must be valid and canonical (see
// golang.org/x/mod/module.CanonicalVersion). The path must have a major
// version suffix consistent with the version (see
// golang.org/x/mod/module.Check). The part of the file path after the
// "<module>@<version>/" prefix must be valid (see
// golang.org/x/mod/module.CheckFilePath).
//
// • No two file paths may be equal under Unicode case-folding (see
// strings.EqualFold).
//
// • A go.mod file may or may not appear in the top-level directory. If present,
// it must be named "go.mod", not any other case. Files named "go.mod"
// are not allowed in any other directory.
//
// • The total size in bytes of a module zip file may be at most MaxZipFile
// bytes (500 MiB). The total uncompressed size of the files within the
// zip may also be at most MaxZipFile bytes.
//
// • Each file's uncompressed size must match its declared 64-bit uncompressed
// size in the zip file header.
//
// • If the zip contains files named "<module>@<version>/go.mod" or
// "<module>@<version>/LICENSE", their sizes in bytes may be at most
// MaxGoMod or MaxLICENSE, respectively (both are 16 MiB).
//
// • Empty directories are ignored. File permissions and timestamps are also
// ignored.
//
// • Symbolic links and other irregular files are not allowed.
//
// Note that this package does not provide hashing functionality. See
// golang.org/x/mod/sumdb/dirhash.
// package zip -- go2cs converted at 2022 March 06 23:26:22 UTC
// import "cmd/vendor/golang.org/x/mod/zip" ==> using zip = go.cmd.vendor.golang.org.x.mod.zip_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\zip\zip.go
using zip = go.archive.zip_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using module = go.golang.org.x.mod.module_package;
using System;


namespace go.cmd.vendor.golang.org.x.mod;

public static partial class zip_package {

 
// MaxZipFile is the maximum size in bytes of a module zip file. The
// go command will report an error if either the zip file or its extracted
// content is larger than this.
public static readonly nint MaxZipFile = 500 << 20; 

// MaxGoMod is the maximum size in bytes of a go.mod file within a
// module zip file.
public static readonly nint MaxGoMod = 16 << 20; 

// MaxLICENSE is the maximum size in bytes of a LICENSE file within a
// module zip file.
public static readonly nint MaxLICENSE = 16 << 20;


// File provides an abstraction for a file in a directory, zip, or anything
// else that looks like a file.
public partial interface File {
    (io.ReadCloser, error) Path(); // Lstat returns information about the file. If the file is a symbolic link,
// Lstat returns information about the link itself, not the file it points to.
    (io.ReadCloser, error) Lstat(); // Open provides access to the data within a regular file. Open may return
// an error if called on a directory or symbolic link.
    (io.ReadCloser, error) Open();
}

// CheckedFiles reports whether a set of files satisfy the name and size
// constraints required by module zip files. The constraints are listed in the
// package documentation.
//
// Functions that produce this report may include slightly different sets of
// files. See documentation for CheckFiles, CheckDir, and CheckZip for details.
public partial struct CheckedFiles {
    public slice<@string> Valid; // Omitted is a list of files that are ignored when creating a module zip
// file, along with the reason each file is ignored.
    public slice<FileError> Omitted; // Invalid is a list of files that should not be included in a module zip
// file, along with the reason each file is invalid.
    public slice<FileError> Invalid; // SizeError is non-nil if the total uncompressed size of the valid files
// exceeds the module zip size limit or if the zip file itself exceeds the
// limit.
    public error SizeError;
}

// Err returns an error if CheckedFiles does not describe a valid module zip
// file. SizeError is returned if that field is set. A FileErrorList is returned
// if there are one or more invalid files. Other errors may be returned in the
// future.
public static error Err(this CheckedFiles cf) {
    if (cf.SizeError != null) {
        return error.As(cf.SizeError)!;
    }
    if (len(cf.Invalid) > 0) {
        return error.As(FileErrorList(cf.Invalid))!;
    }
    return error.As(null!)!;

}

public partial struct FileErrorList { // : slice<FileError>
}

public static @string Error(this FileErrorList el) {
    ptr<strings.Builder> buf = addr(new strings.Builder());
    @string sep = "";
    foreach (var (_, e) in el) {
        buf.WriteString(sep);
        buf.WriteString(e.Error());
        sep = "\n";
    }    return buf.String();
}

public partial struct FileError {
    public @string Path;
    public error Err;
}

public static @string Error(this FileError e) {
    return fmt.Sprintf("%s: %s", e.Path, e.Err);
}

public static error Unwrap(this FileError e) {
    return error.As(e.Err)!;
}

 
// Predefined error messages for invalid files. Not exhaustive.
private static var errPathNotClean = errors.New("file path is not clean");private static var errPathNotRelative = errors.New("file path is not relative");private static var errGoModCase = errors.New("go.mod files must have lowercase names");private static var errGoModSize = fmt.Errorf("go.mod file too large (max size is %d bytes)", MaxGoMod);private static var errLICENSESize = fmt.Errorf("LICENSE file too large (max size is %d bytes)", MaxLICENSE);private static var errVCS = errors.New("directory is a version control repository");private static var errVendored = errors.New("file is in vendor directory");private static var errSubmoduleFile = errors.New("file is in another module");private static var errSubmoduleDir = errors.New("directory is in another module");private static var errHgArchivalTxt = errors.New("file is inserted by 'hg archive' and is always omitted");private static var errSymlink = errors.New("file is a symbolic link");private static var errNotRegular = errors.New("not a regular file");

// CheckFiles reports whether a list of files satisfy the name and size
// constraints listed in the package documentation. The returned CheckedFiles
// record contains lists of valid, invalid, and omitted files. Every file in
// the given list will be included in exactly one of those lists.
//
// CheckFiles returns an error if the returned CheckedFiles does not describe
// a valid module zip file (according to CheckedFiles.Err). The returned
// CheckedFiles is still populated when an error is returned.
//
// Note that CheckFiles will not open any files, so Create may still fail when
// CheckFiles is successful due to I/O errors and reported size differences.
public static (CheckedFiles, error) CheckFiles(slice<File> files) {
    CheckedFiles _p0 = default;
    error _p0 = default!;

    var (cf, _, _) = checkFiles(files);
    return (cf, error.As(cf.Err())!);
}

// checkFiles implements CheckFiles and also returns lists of valid files and
// their sizes, corresponding to cf.Valid. These lists are used in Crewate to
// avoid repeated calls to File.Lstat.
private static (CheckedFiles, slice<File>, slice<long>) checkFiles(slice<File> files) {
    CheckedFiles cf = default;
    slice<File> validFiles = default;
    slice<long> validSizes = default;

    var errPaths = make();
    Action<@string, bool, error> addError = (path, omitted, err) => {
        {
            var (_, ok) = errPaths[path];

            if (ok) {
                return ;
            }

        }

        errPaths[path] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
        FileError fe = new FileError(Path:path,Err:err);
        if (omitted) {
            cf.Omitted = append(cf.Omitted, fe);
        }
        else
 {
            cf.Invalid = append(cf.Invalid, fe);
        }
    }; 

    // Find directories containing go.mod files (other than the root).
    // Files in these directories will be omitted.
    // These directories will not be included in the output zip.
    var haveGoMod = make_map<@string, bool>();
    {
        var f__prev1 = f;

        foreach (var (_, __f) in files) {
            f = __f;
            var p = f.Path();
            var (dir, base) = path.Split(p);
            if (strings.EqualFold(base, "go.mod")) {
                var (info, err) = f.Lstat();
                if (err != null) {
                    addError(p, false, err);
                    continue;
                }
                if (info.Mode().IsRegular()) {
                    haveGoMod[dir] = true;
                }
            }
        }
        f = f__prev1;
    }

    Func<@string, bool> inSubmodule = p => {
        while (true) {
            var (dir, _) = path.Split(p);
            if (dir == "") {
                return false;
            }
            if (haveGoMod[dir]) {
                return true;
            }
            p = dir[..(int)len(dir) - 1];
        }
    };

    var collisions = make(collisionChecker);
    var maxSize = int64(MaxZipFile);
    {
        var f__prev1 = f;

        foreach (var (_, __f) in files) {
            f = __f;
            p = f.Path();
            if (p != path.Clean(p)) {
                addError(p, false, errPathNotClean);
                continue;
            }
            if (path.IsAbs(p)) {
                addError(p, false, errPathNotRelative);
                continue;
            }
            if (isVendoredPackage(p)) {
                addError(p, true, errVendored);
                continue;
            }
            if (inSubmodule(p)) {
                addError(p, true, errSubmoduleFile);
                continue;
            }
            if (p == ".hg_archival.txt") { 
                // Inserted by hg archive.
                // The go command drops this regardless of the VCS being used.
                addError(p, true, errHgArchivalTxt);
                continue;

            }

            {
                var err__prev1 = err;

                var err = module.CheckFilePath(p);

                if (err != null) {
                    addError(p, false, err);
                    continue;
                }

                err = err__prev1;

            }

            if (strings.ToLower(p) == "go.mod" && p != "go.mod") {
                addError(p, false, errGoModCase);
                continue;
            }

            (info, err) = f.Lstat();
            if (err != null) {
                addError(p, false, err);
                continue;
            }

            {
                var err__prev1 = err;

                err = collisions.check(p, info.IsDir());

                if (err != null) {
                    addError(p, false, err);
                    continue;
                }

                err = err__prev1;

            }

            if (info.Mode() & os.ModeType == os.ModeSymlink) { 
                // Skip symbolic links (golang.org/issue/27093).
                addError(p, true, errSymlink);
                continue;

            }

            if (!info.Mode().IsRegular()) {
                addError(p, true, errNotRegular);
                continue;
            }

            var size = info.Size();
            if (size >= 0 && size <= maxSize) {
                maxSize -= size;
            }
            else if (cf.SizeError == null) {
                cf.SizeError = fmt.Errorf("module source tree too large (max size is %d bytes)", MaxZipFile);
            }

            if (p == "go.mod" && size > MaxGoMod) {
                addError(p, false, errGoModSize);
                continue;
            }

            if (p == "LICENSE" && size > MaxLICENSE) {
                addError(p, false, errLICENSESize);
                continue;
            }

            cf.Valid = append(cf.Valid, p);
            validFiles = append(validFiles, f);
            validSizes = append(validSizes, info.Size());

        }
        f = f__prev1;
    }

    return (cf, validFiles, validSizes);

}

// CheckDir reports whether the files in dir satisfy the name and size
// constraints listed in the package documentation. The returned CheckedFiles
// record contains lists of valid, invalid, and omitted files. If a directory is
// omitted (for example, a nested module or vendor directory), it will appear in
// the omitted list, but its files won't be listed.
//
// CheckDir returns an error if it encounters an I/O error or if the returned
// CheckedFiles does not describe a valid module zip file (according to
// CheckedFiles.Err). The returned CheckedFiles is still populated when such
// an error is returned.
//
// Note that CheckDir will not open any files, so CreateFromDir may still fail
// when CheckDir is successful due to I/O errors.
public static (CheckedFiles, error) CheckDir(@string dir) {
    CheckedFiles _p0 = default;
    error _p0 = default!;
 
    // List files (as CreateFromDir would) and check which ones are omitted
    // or invalid.
    var (files, omitted, err) = listFilesInDir(dir);
    if (err != null) {
        return (new CheckedFiles(), error.As(err)!);
    }
    var (cf, cfErr) = CheckFiles(files);
    _ = cfErr; // ignore this error; we'll generate our own after rewriting paths.

    // Replace all paths with file system paths.
    // Paths returned by CheckFiles will be slash-separated paths relative to dir.
    // That's probably not appropriate for error messages.
    {
        var i__prev1 = i;

        foreach (var (__i) in cf.Valid) {
            i = __i;
            cf.Valid[i] = filepath.Join(dir, cf.Valid[i]);
        }
        i = i__prev1;
    }

    cf.Omitted = append(cf.Omitted, omitted);
    {
        var i__prev1 = i;

        foreach (var (__i) in cf.Omitted) {
            i = __i;
            cf.Omitted[i].Path = filepath.Join(dir, cf.Omitted[i].Path);
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in cf.Invalid) {
            i = __i;
            cf.Invalid[i].Path = filepath.Join(dir, cf.Invalid[i].Path);
        }
        i = i__prev1;
    }

    return (cf, error.As(cf.Err())!);

}

// CheckZip reports whether the files contained in a zip file satisfy the name
// and size constraints listed in the package documentation.
//
// CheckZip returns an error if the returned CheckedFiles does not describe
// a valid module zip file (according to CheckedFiles.Err). The returned
// CheckedFiles is still populated when an error is returned. CheckZip will
// also return an error if the module path or version is malformed or if it
// encounters an error reading the zip file.
//
// Note that CheckZip does not read individual files, so Unzip may still fail
// when CheckZip is successful due to I/O errors.
public static (CheckedFiles, error) CheckZip(module.Version m, @string zipFile) => func((defer, _, _) => {
    CheckedFiles _p0 = default;
    error _p0 = default!;

    var (f, err) = os.Open(zipFile);
    if (err != null) {
        return (new CheckedFiles(), error.As(err)!);
    }
    defer(f.Close());
    var (_, cf, err) = checkZip(m, _addr_f);
    return (cf, error.As(err)!);

});

// checkZip implements checkZip and also returns the *zip.Reader. This is
// used in Unzip to avoid redundant I/O.
private static (ptr<zip.Reader>, CheckedFiles, error) checkZip(module.Version m, ptr<os.File> _addr_f) {
    ptr<zip.Reader> _p0 = default!;
    CheckedFiles _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;
 
    // Make sure the module path and version are valid.
    {
        var vers = module.CanonicalVersion(m.Version);

        if (vers != m.Version) {
            return (_addr_null!, new CheckedFiles(), error.As(fmt.Errorf("version %q is not canonical (should be %q)", m.Version, vers))!);
        }
    }

    {
        var err__prev1 = err;

        var err = module.Check(m.Path, m.Version);

        if (err != null) {
            return (_addr_null!, new CheckedFiles(), error.As(err)!);
        }
        err = err__prev1;

    } 

    // Check the total file size.
    var (info, err) = f.Stat();
    if (err != null) {
        return (_addr_null!, new CheckedFiles(), error.As(err)!);
    }
    var zipSize = info.Size();
    if (zipSize > MaxZipFile) {
        CheckedFiles cf = new CheckedFiles(SizeError:fmt.Errorf("module zip file is too large (%d bytes; limit is %d bytes)",zipSize,MaxZipFile));
        return (_addr_null!, cf, error.As(cf.Err())!);
    }
    cf = default;
    Action<ptr<zip.File>, error> addError = (zf, err) => {
        cf.Invalid = append(cf.Invalid, new FileError(Path:zf.Name,Err:err));
    };
    var (z, err) = zip.NewReader(f, zipSize);
    if (err != null) {
        return (_addr_null!, new CheckedFiles(), error.As(err)!);
    }
    var prefix = fmt.Sprintf("%s@%s/", m.Path, m.Version);
    var collisions = make(collisionChecker);
    long size = default;
    foreach (var (_, zf) in z.File) {
        if (!strings.HasPrefix(zf.Name, prefix)) {
            addError(zf, fmt.Errorf("path does not have prefix %q", prefix));
            continue;
        }
        var name = zf.Name[(int)len(prefix)..];
        if (name == "") {
            continue;
        }
        var isDir = strings.HasSuffix(name, "/");
        if (isDir) {
            name = name[..(int)len(name) - 1];
        }
        if (path.Clean(name) != name) {
            addError(zf, errPathNotClean);
            continue;
        }
        {
            var err__prev1 = err;

            err = module.CheckFilePath(name);

            if (err != null) {
                addError(zf, err);
                continue;
            }

            err = err__prev1;

        }

        {
            var err__prev1 = err;

            err = collisions.check(name, isDir);

            if (err != null) {
                addError(zf, err);
                continue;
            }

            err = err__prev1;

        }

        if (isDir) {
            continue;
        }
        {
            var @base = path.Base(name);

            if (strings.EqualFold(base, "go.mod")) {
                if (base != name) {
                    addError(zf, fmt.Errorf("go.mod file not in module root directory"));
                    continue;
                }
                if (name != "go.mod") {
                    addError(zf, errGoModCase);
                    continue;
                }
            }

        }

        var sz = int64(zf.UncompressedSize64);
        if (sz >= 0 && MaxZipFile - size >= sz) {
            size += sz;
        }
        else if (cf.SizeError == null) {
            cf.SizeError = fmt.Errorf("total uncompressed size of module contents too large (max size is %d bytes)", MaxZipFile);
        }
        if (name == "go.mod" && sz > MaxGoMod) {
            addError(zf, fmt.Errorf("go.mod file too large (max size is %d bytes)", MaxGoMod));
            continue;
        }
        if (name == "LICENSE" && sz > MaxLICENSE) {
            addError(zf, fmt.Errorf("LICENSE file too large (max size is %d bytes)", MaxLICENSE));
            continue;
        }
        cf.Valid = append(cf.Valid, zf.Name);

    }    return (_addr_z!, cf, error.As(cf.Err())!);

}

// Create builds a zip archive for module m from an abstract list of files
// and writes it to w.
//
// Create verifies the restrictions described in the package documentation
// and should not produce an archive that Unzip cannot extract. Create does not
// include files in the output archive if they don't belong in the module zip.
// In particular, Create will not include files in modules found in
// subdirectories, most files in vendor directories, or irregular files (such
// as symbolic links) in the output archive.
public static error Create(io.Writer w, module.Version m, slice<File> files) => func((defer, _, _) => {
    error err = default!;

    defer(() => {
        if (err != null) {
            err = addr(new zipError(verb:"create zip",err:err));
        }
    }()); 

    // Check that the version is canonical, the module path is well-formed, and
    // the major version suffix matches the major version.
    {
        var vers = module.CanonicalVersion(m.Version);

        if (vers != m.Version) {
            return error.As(fmt.Errorf("version %q is not canonical (should be %q)", m.Version, vers))!;
        }
    }

    {
        var err__prev1 = err;

        var err = module.Check(m.Path, m.Version);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Check whether files are valid, not valid, or should be omitted.
    // Also check that the valid files don't exceed the maximum size.
    var (cf, validFiles, validSizes) = checkFiles(files);
    {
        var err__prev1 = err;

        err = cf.Err();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Create the module zip file.
    var zw = zip.NewWriter(w);
    var prefix = fmt.Sprintf("%s@%s/", m.Path, m.Version);

    Func<File, @string, long, error> addFile = (f, path, size) => {
        var (rc, err) = f.Open();
        if (err != null) {
            return error.As(err)!;
        }
        defer(rc.Close());
        var (w, err) = zw.Create(prefix + path);
        if (err != null) {
            return error.As(err)!;
        }
        ptr<io.LimitedReader> lr = addr(new io.LimitedReader(R:rc,N:size+1));
        {
            var err__prev1 = err;

            var (_, err) = io.Copy(w, lr);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }

        if (lr.N <= 0) {
            return error.As(fmt.Errorf("file %q is larger than declared size", path))!;
        }
        return error.As(null!)!;

    };

    foreach (var (i, f) in validFiles) {
        var p = f.Path();
        var size = validSizes[i];
        {
            var err__prev1 = err;

            err = addFile(f, p, size);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }

    }    return error.As(zw.Close())!;

});

// CreateFromDir creates a module zip file for module m from the contents of
// a directory, dir. The zip content is written to w.
//
// CreateFromDir verifies the restrictions described in the package
// documentation and should not produce an archive that Unzip cannot extract.
// CreateFromDir does not include files in the output archive if they don't
// belong in the module zip. In particular, CreateFromDir will not include
// files in modules found in subdirectories, most files in vendor directories,
// or irregular files (such as symbolic links) in the output archive.
// Additionally, unlike Create, CreateFromDir will not include directories
// named ".bzr", ".git", ".hg", or ".svn".
public static error CreateFromDir(io.Writer w, module.Version m, @string dir) => func((defer, _, _) => {
    error err = default!;

    defer(() => {
        {
            ptr<zipError> (zerr, ok) = err._<ptr<zipError>>();

            if (ok) {
                zerr.path = dir;
            }
            else if (err != null) {
                err = addr(new zipError(verb:"create zip",path:dir,err:err));
            }


        }

    }());

    var (files, _, err) = listFilesInDir(dir);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(Create(w, m, files))!;

});

private partial struct dirFile {
    public @string filePath;
    public @string slashPath;
    public os.FileInfo info;
}

private static @string Path(this dirFile f) {
    return f.slashPath;
}
private static (os.FileInfo, error) Lstat(this dirFile f) {
    os.FileInfo _p0 = default;
    error _p0 = default!;

    return (f.info, error.As(null!)!);
}
private static (io.ReadCloser, error) Open(this dirFile f) {
    io.ReadCloser _p0 = default;
    error _p0 = default!;

    return os.Open(f.filePath);
}

// isVendoredPackage attempts to report whether the given filename is contained
// in a package whose import path contains (but does not end with) the component
// "vendor".
//
// Unfortunately, isVendoredPackage reports false positives for files in any
// non-top-level package whose import path ends in "vendor".
private static bool isVendoredPackage(@string name) {
    nint i = default;
    if (strings.HasPrefix(name, "vendor/")) {
        i += len("vendor/");
    }    {
        var j = strings.Index(name, "/vendor/");


        else if (j >= 0) { 
            // This offset looks incorrect; this should probably be
            //
            //     i = j + len("/vendor/")
            //
            // (See https://golang.org/issue/31562 and https://golang.org/issue/37397.)
            // Unfortunately, we can't fix it without invalidating module checksums.
            i += len("/vendor/");

        }
        else
 {
            return false;
        }
    }

    return strings.Contains(name[(int)i..], "/");

}

// Unzip extracts the contents of a module zip file to a directory.
//
// Unzip checks all restrictions listed in the package documentation and returns
// an error if the zip archive is not valid. In some cases, files may be written
// to dir before an error is returned (for example, if a file's uncompressed
// size does not match its declared size).
//
// dir may or may not exist: Unzip will create it and any missing parent
// directories if it doesn't exist. If dir exists, it must be empty.
public static error Unzip(@string dir, module.Version m, @string zipFile) => func((defer, _, _) => {
    error err = default!;

    defer(() => {
        if (err != null) {
            err = addr(new zipError(verb:"unzip",path:zipFile,err:err));
        }
    }()); 

    // Check that the directory is empty. Don't create it yet in case there's
    // an error reading the zip.
    {
        var (files, _) = ioutil.ReadDir(dir);

        if (len(files) > 0) {
            return error.As(fmt.Errorf("target directory %v exists and is not empty", dir))!;
        }
    } 

    // Open the zip and check that it satisfies all restrictions.
    var (f, err) = os.Open(zipFile);
    if (err != null) {
        return error.As(err)!;
    }
    defer(f.Close());
    var (z, cf, err) = checkZip(m, _addr_f);
    if (err != null) {
        return error.As(err)!;
    }
    {
        var err__prev1 = err;

        var err = cf.Err();

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // Unzip, enforcing sizes declared in the zip file.
    var prefix = fmt.Sprintf("%s@%s/", m.Path, m.Version);
    {
        var err__prev1 = err;

        err = os.MkdirAll(dir, 0777);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    foreach (var (_, zf) in z.File) {
        var name = zf.Name[(int)len(prefix)..];
        if (name == "" || strings.HasSuffix(name, "/")) {
            continue;
        }
        var dst = filepath.Join(dir, name);
        {
            var err__prev1 = err;

            err = os.MkdirAll(filepath.Dir(dst), 0777);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }

        var (w, err) = os.OpenFile(dst, os.O_WRONLY | os.O_CREATE | os.O_EXCL, 0444);
        if (err != null) {
            return error.As(err)!;
        }
        var (r, err) = zf.Open();
        if (err != null) {
            w.Close();
            return error.As(err)!;
        }
        ptr<io.LimitedReader> lr = addr(new io.LimitedReader(R:r,N:int64(zf.UncompressedSize64)+1));
        _, err = io.Copy(w, lr);
        r.Close();
        if (err != null) {
            w.Close();
            return error.As(err)!;
        }
        {
            var err__prev1 = err;

            err = w.Close();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev1;

        }

        if (lr.N <= 0) {
            return error.As(fmt.Errorf("uncompressed size of file %s is larger than declared size (%d bytes)", zf.Name, zf.UncompressedSize64))!;
        }
    }    return error.As(null!)!;

});

// collisionChecker finds case-insensitive name collisions and paths that
// are listed as both files and directories.
//
// The keys of this map are processed with strToFold. pathInfo has the original
// path for each folded path.
private partial struct collisionChecker { // : map<@string, pathInfo>
}

private partial struct pathInfo {
    public @string path;
    public bool isDir;
}

private static error check(this collisionChecker cc, @string p, bool isDir) {
    var fold = strToFold(p);
    {
        var (other, ok) = cc[fold];

        if (ok) {
            if (p != other.path) {
                return error.As(fmt.Errorf("case-insensitive file name collision: %q and %q", other.path, p))!;
            }
            if (isDir != other.isDir) {
                return error.As(fmt.Errorf("entry %q is both a file and a directory", p))!;
            }
            if (!isDir) {
                return error.As(fmt.Errorf("multiple entries for file %q", p))!;
            } 
            // It's not an error if check is called with the same directory multiple
            // times. check is called recursively on parent directories, so check
            // may be called on the same directory many times.
        }
        else
 {
            cc[fold] = new pathInfo(path:p,isDir:isDir);
        }
    }


    {
        var parent = path.Dir(p);

        if (parent != ".") {
            return error.As(cc.check(parent, true))!;
        }
    }

    return error.As(null!)!;

}

// listFilesInDir walks the directory tree rooted at dir and returns a list of
// files, as well as a list of directories and files that were skipped (for
// example, nested modules and symbolic links).
private static (slice<File>, slice<FileError>, error) listFilesInDir(@string dir) {
    slice<File> files = default;
    slice<FileError> omitted = default;
    error err = default!;

    err = filepath.Walk(dir, (filePath, info, err) => {
        if (err != null) {
            return err;
        }
        var (relPath, err) = filepath.Rel(dir, filePath);
        if (err != null) {
            return err;
        }
        var slashPath = filepath.ToSlash(relPath); 

        // Skip some subdirectories inside vendor, but maintain bug
        // golang.org/issue/31562, described in isVendoredPackage.
        // We would like Create and CreateFromDir to produce the same result
        // for a set of files, whether expressed as a directory tree or zip.
        if (isVendoredPackage(slashPath)) {
            omitted = append(omitted, new FileError(Path:slashPath,Err:errVendored));
            return null;
        }
        if (info.IsDir()) {
            if (filePath == dir) { 
                // Don't skip the top-level directory.
                return null;

            } 

            // Skip VCS directories.
            // fossil repos are regular files with arbitrary names, so we don't try
            // to exclude them.
            switch (filepath.Base(filePath)) {
                case ".bzr": 

                case ".git": 

                case ".hg": 

                case ".svn": 
                    omitted = append(omitted, new FileError(Path:slashPath,Err:errVCS));
                    return filepath.SkipDir;
                    break;
            } 

            // Skip submodules (directories containing go.mod files).
            {
                var (goModInfo, err) = os.Lstat(filepath.Join(filePath, "go.mod"));

                if (err == null && !goModInfo.IsDir()) {
                    omitted = append(omitted, new FileError(Path:slashPath,Err:errSubmoduleDir));
                    return filepath.SkipDir;
                }

            }

            return null;

        }
        if (!info.Mode().IsRegular()) {
            omitted = append(omitted, new FileError(Path:slashPath,Err:errNotRegular));
            return null;
        }
        files = append(files, new dirFile(filePath:filePath,slashPath:slashPath,info:info,));
        return null;

    });
    if (err != null) {
        return (null, null, error.As(err)!);
    }
    return (files, omitted, error.As(null!)!);

}

private partial struct zipError {
    public @string verb;
    public @string path;
    public error err;
}

private static @string Error(this ptr<zipError> _addr_e) {
    ref zipError e = ref _addr_e.val;

    if (e.path == "") {
        return fmt.Sprintf("%s: %v", e.verb, e.err);
    }
    else
 {
        return fmt.Sprintf("%s %s: %v", e.verb, e.path, e.err);
    }
}

private static error Unwrap(this ptr<zipError> _addr_e) {
    ref zipError e = ref _addr_e.val;

    return error.As(e.err)!;
}

// strToFold returns a string with the property that
//    strings.EqualFold(s, t) iff strToFold(s) == strToFold(t)
// This lets us test a large set of strings for fold-equivalent
// duplicates without making a quadratic number of calls
// to EqualFold. Note that strings.ToUpper and strings.ToLower
// do not have the desired property in some corner cases.
private static @string strToFold(@string s) { 
    // Fast path: all ASCII, no upper case.
    // Most paths look like this already.
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c >= utf8.RuneSelf || 'A' <= c && c <= 'Z') {
            goto Slow;
        }
    }
    return s;

Slow:
    bytes.Buffer buf = default;
    foreach (var (_, r) in s) { 
        // SimpleFold(x) cycles to the next equivalent rune > x
        // or wraps around to smaller values. Iterate until it wraps,
        // and we've found the minimum value.
        while (true) {
            var r0 = r;
            r = unicode.SimpleFold(r0);
            if (r <= r0) {
                break;
            }
        } 
        // Exception to allow fast path above: A-Z => a-z
        if ('A' <= r && r <= 'Z') {
            r += 'a' - 'A';
        }
        buf.WriteRune(r);

    }    return buf.String();

}

} // end zip_package
