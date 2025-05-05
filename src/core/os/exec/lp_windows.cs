// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using errors = errors_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using syscall = syscall_package;
using io;
using path;

partial class exec_package {

// ErrNotFound is the error resulting if a path search failed to find an executable file.
public static error ErrNotFound = errors.New("executable file not found in %PATH%"u8);

internal static error chkStat(@string file) {
    (d, err) = os.Stat(file);
    if (err != default!) {
        return err;
    }
    if (d.IsDir()) {
        return fs.ErrPermission;
    }
    return default!;
}

internal static bool hasExt(@string file) {
    nint i = strings.LastIndex(file, "."u8);
    if (i < 0) {
        return false;
    }
    return strings.LastIndexAny(file, @":\/"u8) < i;
}

internal static (@string, error) findExecutable(@string file, slice<@string> exts) {
    if (len(exts) == 0) {
        return (file, chkStat(file));
    }
    if (hasExt(file)) {
        if (chkStat(file) == default!) {
            return (file, default!);
        }
    }
    // Keep checking exts below, so that programs with weird names
    // like "foo.bat.exe" will resolve instead of failing.
    foreach (var (_, e) in exts) {
        {
            @string f = file + e; if (chkStat(f) == default!) {
                return (f, default!);
            }
        }
    }
    if (hasExt(file)) {
        return ("", fs.ErrNotExist);
    }
    return ("", ErrNotFound);
}

// LookPath searches for an executable named file in the
// directories named by the PATH environment variable.
// LookPath also uses PATHEXT environment variable to match
// a suitable candidate.
// If file contains a slash, it is tried directly and the PATH is not consulted.
// Otherwise, on success, the result is an absolute path.
//
// In older versions of Go, LookPath could return a path relative to the current directory.
// As of Go 1.19, LookPath will instead return that path along with an error satisfying
// [errors.Is](err, [ErrDot]). See the package documentation for more details.
public static (@string, error) LookPath(@string file) {
    return lookPath(file, pathExt());
}

// lookExtensions finds windows executable by its dir and path.
// It uses LookPath to try appropriate extensions.
// lookExtensions does not search PATH, instead it converts `prog` into `.\prog`.
//
// If the path already has an extension found in PATHEXT,
// lookExtensions returns it directly without searching
// for additional extensions. For example,
// "C:\foo\example.com" would be returned as-is even if the
// program is actually "C:\foo\example.com.exe".
internal static (@string, error) lookExtensions(@string path, @string dir) {
    if (filepath.Base(path) == path) {
        path = "."u8 + ((@string)filepath.Separator) + path;
    }
    var exts = pathExt();
    {
        @string extΔ1 = filepath.Ext(path); if (extΔ1 != ""u8) {
            foreach (var (_, e) in exts) {
                if (strings.EqualFold(extΔ1, e)) {
                    // Assume that path has already been resolved.
                    return (path, default!);
                }
            }
        }
    }
    if (dir == ""u8) {
        return lookPath(path, exts);
    }
    if (filepath.VolumeName(path) != ""u8) {
        return lookPath(path, exts);
    }
    if (len(path) > 1 && os.IsPathSeparator(path[0])) {
        return lookPath(path, exts);
    }
    @string dirandpath = filepath.Join(dir, path);
    // We assume that LookPath will only add file extension.
    var (lp, err) = lookPath(dirandpath, exts);
    if (err != default!) {
        return ("", err);
    }
    @string ext = strings.TrimPrefix(lp, dirandpath);
    return (path + ext, default!);
}

internal static slice<@string> pathExt() {
    slice<@string> exts = default!;
    @string x = os.Getenv(@"PATHEXT"u8);
    if (x != ""u8){
        foreach (var (_, e) in strings.Split(strings.ToLower(x), @";"u8)) {
            if (e == ""u8) {
                continue;
            }
            if (e[0] != (rune)'.') {
                e = "."u8 + e;
            }
            exts = append(exts, e);
        }
    } else {
        exts = new @string[]{".com", ".exe", ".bat", ".cmd"}.slice();
    }
    return exts;
}

// lookPath implements LookPath for the given PATHEXT list.
internal static (@string, error) lookPath(@string file, slice<@string> exts) {
    if (strings.ContainsAny(file, @":\/"u8)) {
        var (f, err) = findExecutable(file, exts);
        if (err == default!) {
            return (f, default!);
        }
        return ("", new ΔError(file, err));
    }
    // On Windows, creating the NoDefaultCurrentDirectoryInExePath
    // environment variable (with any value or no value!) signals that
    // path lookups should skip the current directory.
    // In theory we are supposed to call NeedCurrentDirectoryForExePathW
    // "as the registry location of this environment variable can change"
    // but that seems exceedingly unlikely: it would break all users who
    // have configured their environment this way!
    // https://docs.microsoft.com/en-us/windows/win32/api/processenv/nf-processenv-needcurrentdirectoryforexepathw
    // See also go.dev/issue/43947.
    @string dotf = default!;
    
    error dotErr = default!;
    {
        var (_, found) = syscall.Getenv("NoDefaultCurrentDirectoryInExePath"u8); if (!found) {
            {
                var (f, err) = findExecutable(filepath.Join("."u8, file), exts); if (err == default!) {
                    if (execerrdot.Value() == "0"u8) {
                        execerrdot.IncNonDefault();
                        return (f, default!);
                    }
                    (dotf, ᏑdotErr) = (f, new ΔError(file, ErrDot)); dotErr = ref ᏑdotErr.val;
                }
            }
        }
    }
    @string path = os.Getenv("path"u8);
    foreach (var (_, dir) in filepath.SplitList(path)) {
        if (dir == ""u8) {
            // Skip empty entries, consistent with what PowerShell does.
            // (See https://go.dev/issue/61493#issuecomment-1649724826.)
            continue;
        }
        {
            var (f, err) = findExecutable(filepath.Join(dir, file), exts); if (err == default!) {
                if (dotErr != default!) {
                    // https://go.dev/issue/53536: if we resolved a relative path implicitly,
                    // and it is the same executable that would be resolved from the explicit %PATH%,
                    // prefer the explicit name for the executable (and, likely, no error) instead
                    // of the equivalent implicit name with ErrDot.
                    //
                    // Otherwise, return the ErrDot for the implicit path as soon as we find
                    // out that the explicit one doesn't match.
                    (dotfi, dotfiErr) = os.Lstat(dotf);
                    (fi, fiErr) = os.Lstat(f);
                    if (dotfiErr != default! || fiErr != default! || !os.SameFile(dotfi, fi)) {
                        return (dotf, dotErr);
                    }
                }
                if (!filepath.IsAbs(f)) {
                    if (execerrdot.Value() != "0"u8) {
                        // If this is the same relative path that we already found,
                        // dotErr is non-nil and we already checked it above.
                        // Otherwise, record this path as the one to which we must resolve,
                        // with or without a dotErr.
                        if (dotErr == default!) {
                            (dotf, ᏑdotErr) = (f, new ΔError(file, ErrDot)); dotErr = ref ᏑdotErr.val;
                        }
                        continue;
                    }
                    execerrdot.IncNonDefault();
                }
                return (f, default!);
            }
        }
    }
    if (dotErr != default!) {
        return (dotf, dotErr);
    }
    return ("", new ΔError(file, ErrNotFound));
}

} // end exec_package
