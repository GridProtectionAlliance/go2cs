// Package fsys is an abstraction for reading files that
// allows for virtual overlays on top of the files on disk.
// package fsys -- go2cs converted at 2022 March 06 23:16:32 UTC
// import "cmd/go/internal/fsys" ==> using fsys = go.cmd.go.@internal.fsys_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\fsys\fsys.go
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using fs = go.io.fs_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;
using System;


namespace go.cmd.go.@internal;

public static partial class fsys_package {

    // OverlayFile is the path to a text file in the OverlayJSON format.
    // It is the value of the -overlay flag.
public static @string OverlayFile = default;

// OverlayJSON is the format overlay files are expected to be in.
// The Replace map maps from overlaid paths to replacement paths:
// the Go command will forward all reads trying to open
// each overlaid path to its replacement path, or consider the overlaid
// path not to exist if the replacement path is empty.
public partial struct OverlayJSON {
    public map<@string, @string> Replace;
}

private partial struct node {
    public @string actualFilePath; // empty if a directory
    public map<@string, ptr<node>> children; // path element → file or directory
}

private static bool isDir(this ptr<node> _addr_n) {
    ref node n = ref _addr_n.val;

    return n.actualFilePath == "" && n.children != null;
}

private static bool isDeleted(this ptr<node> _addr_n) {
    ref node n = ref _addr_n.val;

    return n.actualFilePath == "" && n.children == null;
}

// TODO(matloob): encapsulate these in an io/fs-like interface
private static map<@string, ptr<node>> overlay = default; // path -> file or directory node
private static @string cwd = default; // copy of base.Cwd() to avoid dependency

// Canonicalize a path for looking it up in the overlay.
// Important: filepath.Join(cwd, path) doesn't always produce
// the correct absolute path if path is relative, because on
// Windows producing the correct absolute path requires making
// a syscall. So this should only be used when looking up paths
// in the overlay, or canonicalizing the paths in the overlay.
private static @string canonicalize(@string path) {
    if (path == "") {
        return "";
    }
    if (filepath.IsAbs(path)) {
        return filepath.Clean(path);
    }
    {
        var v = filepath.VolumeName(cwd);

        if (v != "" && path[0] == filepath.Separator) { 
            // On Windows filepath.Join(cwd, path) doesn't always work. In general
            // filepath.Abs needs to make a syscall on Windows. Elsewhere in cmd/go
            // use filepath.Join(cwd, path), but cmd/go specifically supports Windows
            // paths that start with "\" which implies the path is relative to the
            // volume of the working directory. See golang.org/issue/8130.
            return filepath.Join(v, path);

        }
    } 

    // Make the path absolute.
    return filepath.Join(cwd, path);

}

// Init initializes the overlay, if one is being used.
public static error Init(@string wd) {
    if (overlay != null) { 
        // already initialized
        return error.As(null!)!;

    }
    cwd = wd;

    if (OverlayFile == "") {
        return error.As(null!)!;
    }
    var (b, err) = os.ReadFile(OverlayFile);
    if (err != null) {
        return error.As(fmt.Errorf("reading overlay file: %v", err))!;
    }
    ref OverlayJSON overlayJSON = ref heap(out ptr<OverlayJSON> _addr_overlayJSON);
    {
        var err = json.Unmarshal(b, _addr_overlayJSON);

        if (err != null) {
            return error.As(fmt.Errorf("parsing overlay JSON: %v", err))!;
        }
    }


    return error.As(initFromJSON(overlayJSON))!;

}

private static error initFromJSON(OverlayJSON overlayJSON) { 
    // Canonicalize the paths in the overlay map.
    // Use reverseCanonicalized to check for collisions:
    // no two 'from' paths should canonicalize to the same path.
    overlay = make_map<@string, ptr<node>>();
    var reverseCanonicalized = make_map<@string, @string>(); // inverse of canonicalize operation, to check for duplicates
    // Build a table of file and directory nodes from the replacement map.

    // Remove any potential non-determinism from iterating over map by sorting it.
    var replaceFrom = make_slice<@string>(0, len(overlayJSON.Replace));
    foreach (var (k) in overlayJSON.Replace) {
        replaceFrom = append(replaceFrom, k);
    }    sort.Strings(replaceFrom);

    foreach (var (_, from) in replaceFrom) {
        var to = overlayJSON.Replace[from]; 
        // Canonicalize paths and check for a collision.
        if (from == "") {
            return error.As(fmt.Errorf("empty string key in overlay file Replace map"))!;
        }
        var cfrom = canonicalize(from);
        if (to != "") { 
            // Don't canonicalize "", meaning to delete a file, because then it will turn into ".".
            to = canonicalize(to);

        }
        {
            var (otherFrom, seen) = reverseCanonicalized[cfrom];

            if (seen) {
                return error.As(fmt.Errorf("paths %q and %q both canonicalize to %q in overlay file Replace map", otherFrom, from, cfrom))!;
            }

        }

        reverseCanonicalized[cfrom] = from;
        from = cfrom; 

        // Create node for overlaid file.
        var dir = filepath.Dir(from);
        var @base = filepath.Base(from);
        {
            var (n, ok) = overlay[from];

            if (ok) { 
                // All 'from' paths in the overlay are file paths. Since the from paths
                // are in a map, they are unique, so if the node already exists we added
                // it below when we create parent directory nodes. That is, that
                // both a file and a path to one of its parent directories exist as keys
                // in the Replace map.
                //
                // This only applies if the overlay directory has any files or directories
                // in it: placeholder directories that only contain deleted files don't
                // count. They are safe to be overwritten with actual files.
                foreach (var (_, f) in n.children) {
                    if (!f.isDeleted()) {
                        return error.As(fmt.Errorf("invalid overlay: path %v is used as both file and directory", from))!;
                    }
                }

            }

        }

        overlay[from] = addr(new node(actualFilePath:to)); 

        // Add parent directory nodes to overlay structure.
        var childNode = overlay[from];
        while (true) {
            var dirNode = overlay[dir];
            if (dirNode == null || dirNode.isDeleted()) {
                dirNode = addr(new node(children:make(map[string]*node)));
                overlay[dir] = dirNode;
            }
            if (childNode.isDeleted()) { 
                // Only create one parent for a deleted file:
                // the directory only conditionally exists if
                // there are any non-deleted children, so
                // we don't create their parents.
                if (dirNode.isDir()) {
                    dirNode.children[base] = childNode;
                }

                break;

            }

            if (!dirNode.isDir()) { 
                // This path already exists as a file, so it can't be a parent
                // directory. See comment at error above.
                return error.As(fmt.Errorf("invalid overlay: path %v is used as both file and directory", dir))!;

            }

            dirNode.children[base] = childNode;
            var parent = filepath.Dir(dir);
            if (parent == dir) {
                break; // reached the top; there is no parent
            }

            (dir, base) = (parent, filepath.Base(dir));            childNode = dirNode;

        }

    }    return error.As(null!)!;

}

// IsDir returns true if path is a directory on disk or in the
// overlay.
public static (bool, error) IsDir(@string path) {
    bool _p0 = default;
    error _p0 = default!;

    path = canonicalize(path);

    {
        var (_, ok) = parentIsOverlayFile(path);

        if (ok) {
            return (false, error.As(null!)!);
        }
    }


    {
        var (n, ok) = overlay[path];

        if (ok) {
            return (n.isDir(), error.As(null!)!);
        }
    }


    var (fi, err) = os.Stat(path);
    if (err != null) {
        return (false, error.As(err)!);
    }
    return (fi.IsDir(), error.As(null!)!);

}

// parentIsOverlayFile returns whether name or any of
// its parents are files in the overlay, and the first parent found,
// including name itself, that's a file in the overlay.
private static (@string, bool) parentIsOverlayFile(@string name) {
    @string _p0 = default;
    bool _p0 = default;

    if (overlay != null) { 
        // Check if name can't possibly be a directory because
        // it or one of its parents is overlaid with a file.
        // TODO(matloob): Maybe save this to avoid doing it every time?
        var prefix = name;
        while (true) {
            var node = overlay[prefix];
            if (node != null && !node.isDir()) {
                return (prefix, true);
            }
            var parent = filepath.Dir(prefix);
            if (parent == prefix) {
                break;
            }
            prefix = parent;
        }

    }
    return ("", false);

}

// errNotDir is used to communicate from ReadDir to IsDirWithGoFiles
// that the argument is not a directory, so that IsDirWithGoFiles doesn't
// return an error.
private static var errNotDir = errors.New("not a directory");

// readDir reads a dir on disk, returning an error that is errNotDir if the dir is not a directory.
// Unfortunately, the error returned by ioutil.ReadDir if dir is not a directory
// can vary depending on the OS (Linux, Mac, Windows return ENOTDIR; BSD returns EINVAL).
private static (slice<fs.FileInfo>, error) readDir(@string dir) {
    slice<fs.FileInfo> _p0 = default;
    error _p0 = default!;

    var (fis, err) = ioutil.ReadDir(dir);
    if (err == null) {
        return (fis, error.As(null!)!);
    }
    if (os.IsNotExist(err)) {
        return (null, error.As(err)!);
    }
    {
        var (dirfi, staterr) = os.Stat(dir);

        if (staterr == null && !dirfi.IsDir()) {
            return (null, error.As(addr(new fs.PathError(Op:"ReadDir",Path:dir,Err:errNotDir))!)!);
        }
    }

    return (null, error.As(err)!);

}

// ReadDir provides a slice of fs.FileInfo entries corresponding
// to the overlaid files in the directory.
public static (slice<fs.FileInfo>, error) ReadDir(@string dir) {
    slice<fs.FileInfo> _p0 = default;
    error _p0 = default!;

    dir = canonicalize(dir);
    {
        var (_, ok) = parentIsOverlayFile(dir);

        if (ok) {
            return (null, error.As(addr(new fs.PathError(Op:"ReadDir",Path:dir,Err:errNotDir))!)!);
        }
    }


    var dirNode = overlay[dir];
    if (dirNode == null) {
        return readDir(dir);
    }
    if (dirNode.isDeleted()) {
        return (null, error.As(addr(new fs.PathError(Op:"ReadDir",Path:dir,Err:fs.ErrNotExist))!)!);
    }
    var (diskfis, err) = readDir(dir);
    if (err != null && !os.IsNotExist(err) && !errors.Is(err, errNotDir)) {
        return (null, error.As(err)!);
    }
    var files = make_map<@string, fs.FileInfo>();
    {
        var f__prev1 = f;

        foreach (var (_, __f) in diskfis) {
            f = __f;
            files[f.Name()] = f;
        }
        f = f__prev1;
    }

    foreach (var (name, to) in dirNode.children) {

        if (to.isDir()) 
            files[name] = fakeDir(name);
        else if (to.isDeleted()) 
            delete(files, name);
        else 
            // This is a regular file.
            var (f, err) = os.Lstat(to.actualFilePath);
            if (err != null) {
                files[name] = missingFile(name);
                continue;
            }
            else if (f.IsDir()) {
                return (null, error.As(fmt.Errorf("for overlay of %q to %q: overlay Replace entries can't point to dirctories", filepath.Join(dir, name), to.actualFilePath))!);
            } 
            // Add a fileinfo for the overlaid file, so that it has
            // the original file's name, but the overlaid file's metadata.
            files[name] = new fakeFile(name,f);
        
    }    var sortedFiles = diskfis[..(int)0];
    {
        var f__prev1 = f;

        foreach (var (_, __f) in files) {
            f = __f;
            sortedFiles = append(sortedFiles, f);
        }
        f = f__prev1;
    }

    sort.Slice(sortedFiles, (i, j) => sortedFiles[i].Name() < sortedFiles[j].Name());
    return (sortedFiles, error.As(null!)!);

}

// OverlayPath returns the path to the overlaid contents of the
// file, the empty string if the overlay deletes the file, or path
// itself if the file is not in the overlay, the file is a directory
// in the overlay, or there is no overlay.
// It returns true if the path is overlaid with a regular file
// or deleted, and false otherwise.
public static (@string, bool) OverlayPath(@string path) {
    @string _p0 = default;
    bool _p0 = default;

    {
        var (p, ok) = overlay[canonicalize(path)];

        if (ok && !p.isDir()) {
            return (p.actualFilePath, ok);
        }
    }


    return (path, false);

}

// Open opens the file at or overlaid on the given path.
public static (ptr<os.File>, error) Open(@string path) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;

    return _addr_OpenFile(path, os.O_RDONLY, 0)!;
}

// OpenFile opens the file at or overlaid on the given path with the flag and perm.
public static (ptr<os.File>, error) OpenFile(@string path, nint flag, os.FileMode perm) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;

    var cpath = canonicalize(path);
    {
        var (node, ok) = overlay[cpath];

        if (ok) { 
            // Opening a file in the overlay.
            if (node.isDir()) {
                return (_addr_null!, error.As(addr(new fs.PathError(Op:"OpenFile",Path:path,Err:errors.New("fsys.OpenFile doesn't support opening directories yet")))!)!);
            } 
            // We can't open overlaid paths for write.
            if (perm != os.FileMode(os.O_RDONLY)) {
                return (_addr_null!, error.As(addr(new fs.PathError(Op:"OpenFile",Path:path,Err:errors.New("overlaid files can't be opened for write")))!)!);
            }

            return _addr_os.OpenFile(node.actualFilePath, flag, perm)!;

        }
    }

    {
        var (parent, ok) = parentIsOverlayFile(filepath.Dir(cpath));

        if (ok) { 
            // The file is deleted explicitly in the Replace map,
            // or implicitly because one of its parent directories was
            // replaced by a file.
            return (_addr_null!, error.As(addr(new fs.PathError(Op:"Open",Path:path,Err:fmt.Errorf("file %s does not exist: parent directory %s is replaced by a file in overlay",path,parent),))!)!);

        }
    }

    return _addr_os.OpenFile(cpath, flag, perm)!;

}

// IsDirWithGoFiles reports whether dir is a directory containing Go files
// either on disk or in the overlay.
public static (bool, error) IsDirWithGoFiles(@string dir) {
    bool _p0 = default;
    error _p0 = default!;

    var (fis, err) = ReadDir(dir);
    if (os.IsNotExist(err) || errors.Is(err, errNotDir)) {
        return (false, error.As(null!)!);
    }
    if (err != null) {
        return (false, error.As(err)!);
    }
    error firstErr = default!;
    {
        var fi__prev1 = fi;

        foreach (var (_, __fi) in fis) {
            fi = __fi;
            if (fi.IsDir()) {
                continue;
            } 

            // TODO(matloob): this enforces that the "from" in the map
            // has a .go suffix, but the actual destination file
            // doesn't need to have a .go suffix. Is this okay with the
            // compiler?
            if (!strings.HasSuffix(fi.Name(), ".go")) {
                continue;
            }

            if (fi.Mode().IsRegular()) {
                return (true, error.As(null!)!);
            } 

            // fi is the result of an Lstat, so it doesn't follow symlinks.
            // But it's okay if the file is a symlink pointing to a regular
            // file, so use os.Stat to follow symlinks and check that.
            var (actualFilePath, _) = OverlayPath(filepath.Join(dir, fi.Name()));
            var (fi, err) = os.Stat(actualFilePath);
            if (err == null && fi.Mode().IsRegular()) {
                return (true, error.As(null!)!);
            }

            if (err != null && firstErr == null) {
                firstErr = error.As(err)!;
            }

        }
        fi = fi__prev1;
    }

    return (false, error.As(firstErr)!);

}

// walk recursively descends path, calling walkFn. Copied, with some
// modifications from path/filepath.walk.
private static error walk(@string path, fs.FileInfo info, filepath.WalkFunc walkFn) {
    if (!info.IsDir()) {
        return error.As(walkFn(path, info, null))!;
    }
    var (fis, readErr) = ReadDir(path);
    var walkErr = walkFn(path, info, readErr); 
    // If readErr != nil, walk can't walk into this directory.
    // walkErr != nil means walkFn want walk to skip this directory or stop walking.
    // Therefore, if one of readErr and walkErr isn't nil, walk will return.
    if (readErr != null || walkErr != null) { 
        // The caller's behavior is controlled by the return value, which is decided
        // by walkFn. walkFn may ignore readErr and return nil.
        // If walkFn returns SkipDir, it will be handled by the caller.
        // So walk should return whatever walkFn returns.
        return error.As(walkErr)!;

    }
    foreach (var (_, fi) in fis) {
        var filename = filepath.Join(path, fi.Name());
        walkErr = walk(filename, fi, walkFn);

        if (walkErr != null) {
            if (!fi.IsDir() || walkErr != filepath.SkipDir) {
                return error.As(walkErr)!;
            }
        }
    }    return error.As(null!)!;

}

// Walk walks the file tree rooted at root, calling walkFn for each file or
// directory in the tree, including root.
public static error Walk(@string root, filepath.WalkFunc walkFn) {
    var (info, err) = Lstat(root);
    if (err != null) {
        err = walkFn(root, null, err);
    }
    else
 {
        err = walk(root, info, walkFn);
    }
    if (err == filepath.SkipDir) {
        return error.As(null!)!;
    }
    return error.As(err)!;

}

// lstat implements a version of os.Lstat that operates on the overlay filesystem.
public static (fs.FileInfo, error) Lstat(@string path) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;

    return overlayStat(path, os.Lstat, "lstat");
}

// Stat implements a version of os.Stat that operates on the overlay filesystem.
public static (fs.FileInfo, error) Stat(@string path) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;

    return overlayStat(path, os.Stat, "stat");
}

// overlayStat implements lstat or Stat (depending on whether os.Lstat or os.Stat is passed in).
private static (fs.FileInfo, error) overlayStat(@string path, Func<@string, (fs.FileInfo, error)> osStat, @string opName) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;

    var cpath = canonicalize(path);

    {
        var (_, ok) = parentIsOverlayFile(filepath.Dir(cpath));

        if (ok) {
            return (null, error.As(addr(new fs.PathError(Op:opName,Path:cpath,Err:fs.ErrNotExist))!)!);
        }
    }


    var (node, ok) = overlay[cpath];
    if (!ok) { 
        // The file or directory is not overlaid.
        return osStat(path);

    }

    if (node.isDeleted()) 
        return (null, error.As(addr(new fs.PathError(Op:"lstat",Path:cpath,Err:fs.ErrNotExist))!)!);
    else if (node.isDir()) 
        return (fakeDir(filepath.Base(path)), error.As(null!)!);
    else 
        var (fi, err) = osStat(node.actualFilePath);
        if (err != null) {
            return (null, error.As(err)!);
        }
        return (new fakeFile(name:filepath.Base(path),real:fi), error.As(null!)!);
    
}

// fakeFile provides an fs.FileInfo implementation for an overlaid file,
// so that the file has the name of the overlaid file, but takes all
// other characteristics of the replacement file.
private partial struct fakeFile {
    public @string name;
    public fs.FileInfo real;
}

private static @string Name(this fakeFile f) {
    return f.name;
}
private static long Size(this fakeFile f) {
    return f.real.Size();
}
private static fs.FileMode Mode(this fakeFile f) {
    return f.real.Mode();
}
private static time.Time ModTime(this fakeFile f) {
    return f.real.ModTime();
}
private static bool IsDir(this fakeFile f) {
    return f.real.IsDir();
}
private static void Sys(this fakeFile f) {
    return f.real.Sys();
}

// missingFile provides an fs.FileInfo for an overlaid file where the
// destination file in the overlay doesn't exist. It returns zero values
// for the fileInfo methods other than Name, set to the file's name, and Mode
// set to ModeIrregular.
private partial struct missingFile { // : @string
}

private static @string Name(this missingFile f) {
    return string(f);
}
private static long Size(this missingFile f) {
    return 0;
}
private static fs.FileMode Mode(this missingFile f) {
    return fs.ModeIrregular;
}
private static time.Time ModTime(this missingFile f) {
    return time.Unix(0, 0);
}
private static bool IsDir(this missingFile f) {
    return false;
}
private static void Sys(this missingFile f) {
    return null;
}

// fakeDir provides an fs.FileInfo implementation for directories that are
// implicitly created by overlaid files. Each directory in the
// path of an overlaid file is considered to exist in the overlay filesystem.
private partial struct fakeDir { // : @string
}

private static @string Name(this fakeDir f) {
    return string(f);
}
private static long Size(this fakeDir f) {
    return 0;
}
private static fs.FileMode Mode(this fakeDir f) {
    return fs.ModeDir | 0500;
}
private static time.Time ModTime(this fakeDir f) {
    return time.Unix(0, 0);
}
private static bool IsDir(this fakeDir f) {
    return true;
}
private static void Sys(this fakeDir f) {
    return null;
}

// Glob is like filepath.Glob but uses the overlay file system.
public static (slice<@string>, error) Glob(@string pattern) {
    slice<@string> matches = default;
    error err = default!;
 
    // Check pattern is well-formed.
    {
        var (_, err) = filepath.Match(pattern, "");

        if (err != null) {
            return (null, error.As(err)!);
        }
    }

    if (!hasMeta(pattern)) {
        _, err = Lstat(pattern);

        if (err != null) {
            return (null, error.As(null!)!);
        }
        return (new slice<@string>(new @string[] { pattern }), error.As(null!)!);

    }
    var (dir, file) = filepath.Split(pattern);
    nint volumeLen = 0;
    if (runtime.GOOS == "windows") {
        volumeLen, dir = cleanGlobPathWindows(dir);
    }
    else
 {
        dir = cleanGlobPath(dir);
    }
    if (!hasMeta(dir[(int)volumeLen..])) {
        return glob(dir, file, null);
    }
    if (dir == pattern) {
        return (null, error.As(filepath.ErrBadPattern)!);
    }
    slice<@string> m = default;
    m, err = Glob(dir);
    if (err != null) {
        return ;
    }
    foreach (var (_, d) in m) {
        matches, err = glob(d, file, matches);
        if (err != null) {
            return ;
        }
    }    return ;

}

// cleanGlobPath prepares path for glob matching.
private static @string cleanGlobPath(@string path) {

    if (path == "") 
        return ".";
    else if (path == string(filepath.Separator)) 
        // do nothing to the path
        return path;
    else 
        return path[(int)0..(int)len(path) - 1]; // chop off trailing separator
    }

private static nint volumeNameLen(@string path) {
    Func<byte, bool> isSlash = c => {
        return c == '\\' || c == '/';
    };
    if (len(path) < 2) {
        return 0;
    }
    var c = path[0];
    if (path[1] == ':' && ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z')) {
        return 2;
    }
    {
        var l = len(path);

        if (l >= 5 && isSlash(path[0]) && isSlash(path[1]) && !isSlash(path[2]) && path[2] != '.') { 
            // first, leading `\\` and next shouldn't be `\`. its server name.
            for (nint n = 3; n < l - 1; n++) { 
                // second, next '\' shouldn't be repeated.
                if (isSlash(path[n])) {
                    n++; 
                    // third, following something characters. its share name.
                    if (!isSlash(path[n])) {
                        if (path[n] == '.') {
                            break;
                        }
                        while (n < l) {
                            if (isSlash(path[n])) {
                                break;
                            n++;
                            }

                        }

                        return n;

                    }

                    break;

                }

            }


        }
    }

    return 0;

}

// cleanGlobPathWindows is windows version of cleanGlobPath.
private static (nint, @string) cleanGlobPathWindows(@string path) {
    nint prefixLen = default;
    @string cleaned = default;

    var vollen = volumeNameLen(path);

    if (path == "") 
        return (0, ".");
    else if (vollen + 1 == len(path) && os.IsPathSeparator(path[len(path) - 1])) // /, \, C:\ and C:/
        // do nothing to the path
        return (vollen + 1, path);
    else if (vollen == len(path) && len(path) == 2) // C:
        return (vollen, path + "."); // convert C: into C:.
    else 
        if (vollen >= len(path)) {
            vollen = len(path) - 1;
        }
        return (vollen, path[(int)0..(int)len(path) - 1]); // chop off trailing separator
    }

// glob searches for files matching pattern in the directory dir
// and appends them to matches. If the directory cannot be
// opened, it returns the existing matches. New matches are
// added in lexicographical order.
private static (slice<@string>, error) glob(@string dir, @string pattern, slice<@string> matches) {
    slice<@string> m = default;
    error e = default!;

    m = matches;
    var (fi, err) = Stat(dir);
    if (err != null) {
        return ; // ignore I/O error
    }
    if (!fi.IsDir()) {
        return ; // ignore I/O error
    }
    var (list, err) = ReadDir(dir);
    if (err != null) {
        return ; // ignore I/O error
    }
    slice<@string> names = default;
    foreach (var (_, info) in list) {
        names = append(names, info.Name());
    }    sort.Strings(names);

    foreach (var (_, n) in names) {
        var (matched, err) = filepath.Match(pattern, n);
        if (err != null) {
            return (m, error.As(err)!);
        }
        if (matched) {
            m = append(m, filepath.Join(dir, n));
        }
    }    return ;

}

// hasMeta reports whether path contains any of the magic characters
// recognized by filepath.Match.
private static bool hasMeta(@string path) {
    @string magicChars = "*?[";
    if (runtime.GOOS != "windows") {
        magicChars = "*?[\\";
    }
    return strings.ContainsAny(path, magicChars);

}

} // end fsys_package
