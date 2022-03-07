// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package codehost defines the interface implemented by a code hosting source,
// along with support code for use by implementations.
// package codehost -- go2cs converted at 2022 March 06 23:18:38 UTC
// import "cmd/go/internal/modfetch/codehost" ==> using codehost = go.cmd.go.@internal.modfetch.codehost_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\codehost\codehost.go
using bytes = go.bytes_package;
using sha256 = go.crypto.sha256_package;
using fmt = go.fmt_package;
using exec = go.@internal.execabs_package;
using io = go.io_package;
using fs = go.io.fs_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using cfg = go.cmd.go.@internal.cfg_package;
using lockedfile = go.cmd.go.@internal.lockedfile_package;
using str = go.cmd.go.@internal.str_package;
using System;


namespace go.cmd.go.@internal.modfetch;

public static partial class codehost_package {

    // Downloaded size limits.
public static readonly nint MaxGoMod = 16 << 20; // maximum size of go.mod file
public static readonly nint MaxLICENSE = 16 << 20; // maximum size of LICENSE file
public static readonly nint MaxZipFile = 500 << 20; // maximum size of downloaded zip file

// A Repo represents a code hosting source.
// Typical implementations include local version control repositories,
// remote version control servers, and code hosting sites.
// A Repo must be safe for simultaneous use by multiple goroutines.
public partial interface Repo {
    (bool, error) Tags(@string prefix); // Stat returns information about the revision rev.
// A revision can be any identifier known to the underlying service:
// commit hash, branch, tag, and so on.
    (bool, error) Stat(@string rev); // Latest returns the latest revision on the default branch,
// whatever that means in the underlying implementation.
    (bool, error) Latest(); // ReadFile reads the given file in the file tree corresponding to revision rev.
// It should refuse to read more than maxSize bytes.
//
// If the requested file does not exist it should return an error for which
// os.IsNotExist(err) returns true.
    (bool, error) ReadFile(@string rev, @string file, long maxSize); // ReadFileRevs reads a single file at multiple versions.
// It should refuse to read more than maxSize bytes.
// The result is a map from each requested rev strings
// to the associated FileRev. The map must have a non-nil
// entry for every requested rev (unless ReadFileRevs returned an error).
// A file simply being missing or even corrupted in revs[i]
// should be reported only in files[revs[i]].Err, not in the error result
// from ReadFileRevs.
// The overall call should return an error (and no map) only
// in the case of a problem with obtaining the data, such as
// a network failure.
// Implementations may assume that revs only contain tags,
// not direct commit hashes.
    (bool, error) ReadFileRevs(slice<@string> revs, @string file, long maxSize); // ReadZip downloads a zip file for the subdir subdirectory
// of the given revision to a new file in a given temporary directory.
// It should refuse to read more than maxSize bytes.
// It returns a ReadCloser for a streamed copy of the zip file.
// All files in the zip file are expected to be
// nested in a single top-level directory, whose name is not specified.
    (bool, error) ReadZip(@string rev, @string subdir, long maxSize); // RecentTag returns the most recent tag on rev or one of its predecessors
// with the given prefix. allowed may be used to filter out unwanted versions.
    (bool, error) RecentTag(@string rev, @string prefix, Func<@string, bool> allowed); // DescendsFrom reports whether rev or any of its ancestors has the given tag.
//
// DescendsFrom must return true for any tag returned by RecentTag for the
// same revision.
    (bool, error) DescendsFrom(@string rev, @string tag);
}

// A Rev describes a single revision in a source code repository.
public partial struct RevInfo {
    public @string Name; // complete ID in underlying repository
    public @string Short; // shortened ID, for use in pseudo-version
    public @string Version; // version used in lookup
    public time.Time Time; // commit time
    public slice<@string> Tags; // known tags for commit
}

// A FileRev describes the result of reading a file at a given revision.
public partial struct FileRev {
    public @string Rev; // requested revision
    public slice<byte> Data; // file data
    public error Err; // error if any; os.IsNotExist(Err)==true if rev exists but file does not exist in that rev
}

// UnknownRevisionError is an error equivalent to fs.ErrNotExist, but for a
// revision rather than a file.
public partial struct UnknownRevisionError {
    public @string Rev;
}

private static @string Error(this ptr<UnknownRevisionError> _addr_e) {
    ref UnknownRevisionError e = ref _addr_e.val;

    return "unknown revision " + e.Rev;
}
public static bool Is(this UnknownRevisionError _p0, error err) {
    return err == fs.ErrNotExist;
}

// ErrNoCommits is an error equivalent to fs.ErrNotExist indicating that a given
// repository or module contains no commits.
public static error ErrNoCommits = error.As(new noCommitsError())!;

private partial struct noCommitsError {
}

private static @string Error(this noCommitsError _p0) {
    return "no commits";
}
private static bool Is(this noCommitsError _p0, error err) {
    return err == fs.ErrNotExist;
}

// AllHex reports whether the revision rev is entirely lower-case hexadecimal digits.
public static bool AllHex(@string rev) {
    for (nint i = 0; i < len(rev); i++) {
        var c = rev[i];
        if ('0' <= c && c <= '9' || 'a' <= c && c <= 'f') {
            continue;
        }
        return false;

    }
    return true;

}

// ShortenSHA1 shortens a SHA1 hash (40 hex digits) to the canonical length
// used in pseudo-versions (12 hex digits).
public static @string ShortenSHA1(@string rev) {
    if (AllHex(rev) && len(rev) == 40) {
        return rev[..(int)12];
    }
    return rev;

}

// WorkDir returns the name of the cached work directory to use for the
// given repository type and name.
public static (@string, @string, error) WorkDir(@string typ, @string name) => func((defer, _, _) => {
    @string dir = default;
    @string lockfile = default;
    error err = default!;

    if (cfg.GOMODCACHE == "") {
        return ("", "", error.As(fmt.Errorf("neither GOPATH nor GOMODCACHE are set"))!);
    }
    if (strings.Contains(typ, ":")) {
        return ("", "", error.As(fmt.Errorf("codehost.WorkDir: type cannot contain colon"))!);
    }
    var key = typ + ":" + name;
    dir = filepath.Join(cfg.GOMODCACHE, "cache/vcs", fmt.Sprintf("%x", sha256.Sum256((slice<byte>)key)));

    if (cfg.BuildX) {
        fmt.Fprintf(os.Stderr, "mkdir -p %s # %s %s\n", filepath.Dir(dir), typ, name);
    }
    {
        var err__prev1 = err;

        var err = os.MkdirAll(filepath.Dir(dir), 0777);

        if (err != null) {
            return ("", "", error.As(err)!);
        }
        err = err__prev1;

    }


    lockfile = dir + ".lock";
    if (cfg.BuildX) {
        fmt.Fprintf(os.Stderr, "# lock %s", lockfile);
    }
    var (unlock, err) = lockedfile.MutexAt(lockfile).Lock();
    if (err != null) {
        return ("", "", error.As(fmt.Errorf("codehost.WorkDir: can't find or create lock file: %v", err))!);
    }
    defer(unlock());

    var (data, err) = os.ReadFile(dir + ".info");
    var (info, err2) = os.Stat(dir);
    if (err == null && err2 == null && info.IsDir()) { 
        // Info file and directory both already exist: reuse.
        var have = strings.TrimSuffix(string(data), "\n");
        if (have != key) {
            return ("", "", error.As(fmt.Errorf("%s exists with wrong content (have %q want %q)", dir + ".info", have, key))!);
        }
        if (cfg.BuildX) {
            fmt.Fprintf(os.Stderr, "# %s for %s %s\n", dir, typ, name);
        }
        return (dir, lockfile, error.As(null!)!);

    }
    if (cfg.BuildX) {
        fmt.Fprintf(os.Stderr, "mkdir -p %s # %s %s\n", dir, typ, name);
    }
    os.RemoveAll(dir);
    {
        var err__prev1 = err;

        err = os.MkdirAll(dir, 0777);

        if (err != null) {
            return ("", "", error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = os.WriteFile(dir + ".info", (slice<byte>)key, 0666);

        if (err != null) {
            os.RemoveAll(dir);
            return ("", "", error.As(err)!);
        }
        err = err__prev1;

    }

    return (dir, lockfile, error.As(null!)!);

});

public partial struct RunError {
    public @string Cmd;
    public error Err;
    public slice<byte> Stderr;
    public @string HelpText;
}

private static @string Error(this ptr<RunError> _addr_e) {
    ref RunError e = ref _addr_e.val;

    var text = e.Cmd + ": " + e.Err.Error();
    var stderr = bytes.TrimRight(e.Stderr, "\n");
    if (len(stderr) > 0) {
        text += ":\n\t" + strings.ReplaceAll(string(stderr), "\n", "\n\t");
    }
    if (len(e.HelpText) > 0) {
        text += "\n" + e.HelpText;
    }
    return text;

}

private static sync.Map dirLock = default;

// Run runs the command line in the given directory
// (an empty dir means the current directory).
// It returns the standard output and, for a non-zero exit,
// a *RunError indicating the command, exit status, and standard error.
// Standard error is unavailable for commands that exit successfully.
public static (slice<byte>, error) Run(@string dir, params object[] cmdline) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    cmdline = cmdline.Clone();

    return RunWithStdin(dir, null, cmdline);
}

// bashQuoter escapes characters that have special meaning in double-quoted strings in the bash shell.
// See https://www.gnu.org/software/bash/manual/html_node/Double-Quotes.html.
private static var bashQuoter = strings.NewReplacer("\"", "\\\"", "$", "\\$", "`", "\\`", "\\", "\\\\");

public static (slice<byte>, error) RunWithStdin(@string dir, io.Reader stdin, params object[] cmdline) => func((defer, panic, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    cmdline = cmdline.Clone();

    if (dir != "") {
        var (muIface, ok) = dirLock.Load(dir);
        if (!ok) {
            muIface, _ = dirLock.LoadOrStore(dir, @new<sync.Mutex>());
        }
        ptr<sync.Mutex> mu = muIface._<ptr<sync.Mutex>>();
        mu.Lock();
        defer(mu.Unlock());

    }
    var cmd = str.StringList(cmdline);
    if (os.Getenv("TESTGOVCS") == "panic") {
        panic(fmt.Sprintf("use of vcs: %v", cmd));
    }
    if (cfg.BuildX) {
        ptr<object> text = @new<strings.Builder>();
        if (dir != "") {
            text.WriteString("cd ");
            text.WriteString(dir);
            text.WriteString("; ");
        }
        foreach (var (i, arg) in cmd) {
            if (i > 0) {
                text.WriteByte(' ');
            }

            if (strings.ContainsAny(arg, "'")) 
                // Quote args that could be mistaken for quoted args.
                text.WriteByte('"');
                text.WriteString(bashQuoter.Replace(arg));
                text.WriteByte('"');
            else if (strings.ContainsAny(arg, "$`\\*?[\"\t\n\v\f\r \u0085\u00a0")) 
                // Quote args that contain special characters, glob patterns, or spaces.
                text.WriteByte('\'');
                text.WriteString(arg);
                text.WriteByte('\'');
            else 
                text.WriteString(arg);
            
        }        fmt.Fprintf(os.Stderr, "%s\n", text);
        var start = time.Now();
        defer(() => {
            fmt.Fprintf(os.Stderr, "%.3fs # %s\n", time.Since(start).Seconds(), text);
        }());

    }
    ref bytes.Buffer stderr = ref heap(out ptr<bytes.Buffer> _addr_stderr);
    ref bytes.Buffer stdout = ref heap(out ptr<bytes.Buffer> _addr_stdout);
    var c = exec.Command(cmd[0], cmd[(int)1..]);
    c.Dir = dir;
    c.Stdin = stdin;
    _addr_c.Stderr = _addr_stderr;
    c.Stderr = ref _addr_c.Stderr.val;
    _addr_c.Stdout = _addr_stdout;
    c.Stdout = ref _addr_c.Stdout.val;
    var err = c.Run();
    if (err != null) {
        err = addr(new RunError(Cmd:strings.Join(cmd," ")+" in "+dir,Stderr:stderr.Bytes(),Err:err));
    }
    return (stdout.Bytes(), error.As(err)!);

});

} // end codehost_package
