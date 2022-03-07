// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 23:15:35 UTC
// Original source: C:\Program Files\Go\src\cmd\dist\util.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

    // pathf is fmt.Sprintf for generating paths
    // (on windows it turns / into \ after the printf).
private static @string pathf(@string format, params object[] args) {
    args = args.Clone();

    return filepath.Clean(fmt.Sprintf(format, args));
}

// filter returns a slice containing the elements x from list for which f(x) == true.
private static slice<@string> filter(slice<@string> list, Func<@string, bool> f) {
    slice<@string> @out = default;
    foreach (var (_, x) in list) {
        if (f(x)) {
            out = append(out, x);
        }
    }    return out;

}

// uniq returns a sorted slice containing the unique elements of list.
private static slice<@string> uniq(slice<@string> list) {
    var @out = make_slice<@string>(len(list));
    copy(out, list);
    sort.Strings(out);
    var keep = out[..(int)0];
    foreach (var (_, x) in out) {
        if (len(keep) == 0 || keep[len(keep) - 1] != x) {
            keep = append(keep, x);
        }
    }    return keep;

}

public static readonly nint CheckExit = 1 << (int)(iota);
public static readonly var ShowOutput = 0;
public static readonly var Background = 1;


private static sync.Mutex outputLock = default;

// run runs the command line cmd in dir.
// If mode has ShowOutput set and Background unset, run passes cmd's output to
// stdout/stderr directly. Otherwise, run returns cmd's output as a string.
// If mode has CheckExit set and the command fails, run calls fatalf.
// If mode has Background set, this command is being run as a
// Background job. Only bgrun should use the Background mode,
// not other callers.
private static @string run(@string dir, nint mode, params @string[] cmd) {
    cmd = cmd.Clone();

    if (vflag > 1) {
        errprintf("run: %s\n", strings.Join(cmd, " "));
    }
    var xcmd = exec.Command(cmd[0], cmd[(int)1..]);
    xcmd.Dir = dir;
    slice<byte> data = default;
    error err = default!; 

    // If we want to show command output and this is not
    // a background command, assume it's the only thing
    // running, so we can just let it write directly stdout/stderr
    // as it runs without fear of mixing the output with some
    // other command's output. Not buffering lets the output
    // appear as it is printed instead of once the command exits.
    // This is most important for the invocation of 'go1.4 build -v bootstrap/...'.
    if (mode & (Background | ShowOutput) == ShowOutput) {
        xcmd.Stdout = os.Stdout;
        xcmd.Stderr = os.Stderr;
        err = error.As(xcmd.Run())!;
    }
    else
 {
        data, err = xcmd.CombinedOutput();
    }
    if (err != null && mode & CheckExit != 0) {
        outputLock.Lock();
        if (len(data) > 0) {
            xprintf("%s\n", data);
        }
        outputLock.Unlock();
        if (mode & Background != 0) { 
            // Prevent fatalf from waiting on our own goroutine's
            // bghelper to exit:
            bghelpers.Done();

        }
        fatalf("FAILED: %v: %v", strings.Join(cmd, " "), err);

    }
    if (mode & ShowOutput != 0) {
        outputLock.Lock();
        os.Stdout.Write(data);
        outputLock.Unlock();
    }
    if (vflag > 2) {
        errprintf("run: %s DONE\n", strings.Join(cmd, " "));
    }
    return string(data);

}

private static nint maxbg = 4;/* maximum number of jobs to run at once */

private static var bgwork = make_channel<Action>(1e5F);private static sync.WaitGroup bghelpers = default;private static sync.Once dieOnce = default;private static var dying = make_channel<object>();

private static void bginit() {
    bghelpers.Add(maxbg);
    for (nint i = 0; i < maxbg; i++) {
        go_(() => bghelper());
    }
}

private static void bghelper() => func((defer, _, _) => {
    defer(bghelpers.Done());
    while (true) {
        return ;
        return ;
        w();
    }
});

// bgrun is like run but runs the command in the background.
// CheckExit|ShowOutput mode is implied (since output cannot be returned).
// bgrun adds 1 to wg immediately, and calls Done when the work completes.
private static void bgrun(ptr<sync.WaitGroup> _addr_wg, @string dir, params @string[] cmd) => func((defer, _, _) => {
    cmd = cmd.Clone();
    ref sync.WaitGroup wg = ref _addr_wg.val;

    wg.Add(1);
    bgwork.Send(() => {
        defer(wg.Done());
        run(dir, CheckExit | ShowOutput | Background, cmd);
    });
});

// bgwait waits for pending bgruns to finish.
// bgwait must be called from only a single goroutine at a time.
private static void bgwait(ptr<sync.WaitGroup> _addr_wg) {
    ref sync.WaitGroup wg = ref _addr_wg.val;

    var done = make_channel<object>();
    go_(() => () => {
        wg.Wait();
        close(done);
    }());
}

// xgetwd returns the current directory.
private static @string xgetwd() {
    var (wd, err) = os.Getwd();
    if (err != null) {
        fatalf("%s", err);
    }
    return wd;

}

// xrealwd returns the 'real' name for the given path.
// real is defined as what xgetwd returns in that directory.
private static @string xrealwd(@string path) {
    var old = xgetwd();
    {
        var err__prev1 = err;

        var err = os.Chdir(path);

        if (err != null) {
            fatalf("chdir %s: %v", path, err);
        }
        err = err__prev1;

    }

    var real = xgetwd();
    {
        var err__prev1 = err;

        err = os.Chdir(old);

        if (err != null) {
            fatalf("chdir %s: %v", old, err);
        }
        err = err__prev1;

    }

    return real;

}

// isdir reports whether p names an existing directory.
private static bool isdir(@string p) {
    var (fi, err) = os.Stat(p);
    return err == null && fi.IsDir();
}

// isfile reports whether p names an existing file.
private static bool isfile(@string p) {
    var (fi, err) = os.Stat(p);
    return err == null && fi.Mode().IsRegular();
}

// mtime returns the modification time of the file p.
private static time.Time mtime(@string p) {
    var (fi, err) = os.Stat(p);
    if (err != null) {
        return new time.Time();
    }
    return fi.ModTime();

}

// readfile returns the content of the named file.
private static @string readfile(@string file) {
    var (data, err) = ioutil.ReadFile(file);
    if (err != null) {
        fatalf("%v", err);
    }
    return string(data);

}

private static readonly nint writeExec = 1 << (int)(iota);
private static readonly var writeSkipSame = 0;


// writefile writes text to the named file, creating it if needed.
// if exec is non-zero, marks the file as executable.
// If the file already exists and has the expected content,
// it is not rewritten, to avoid changing the time stamp.
private static void writefile(@string text, @string file, nint flag) {
    slice<byte> @new = (slice<byte>)text;
    if (flag & writeSkipSame != 0) {
        var (old, err) = ioutil.ReadFile(file);
        if (err == null && bytes.Equal(old, new)) {
            return ;
        }
    }
    var mode = os.FileMode(0666);
    if (flag & writeExec != 0) {
        mode = 0777;
    }
    xremove(file); // in case of symlink tricks by misc/reboot test
    var err = ioutil.WriteFile(file, new, mode);
    if (err != null) {
        fatalf("%v", err);
    }
}

// xmkdir creates the directory p.
private static void xmkdir(@string p) {
    var err = os.Mkdir(p, 0777);
    if (err != null) {
        fatalf("%v", err);
    }
}

// xmkdirall creates the directory p and its parents, as needed.
private static void xmkdirall(@string p) {
    var err = os.MkdirAll(p, 0777);
    if (err != null) {
        fatalf("%v", err);
    }
}

// xremove removes the file p.
private static void xremove(@string p) {
    if (vflag > 2) {
        errprintf("rm %s\n", p);
    }
    os.Remove(p);

}

// xremoveall removes the file or directory tree rooted at p.
private static void xremoveall(@string p) {
    if (vflag > 2) {
        errprintf("rm -r %s\n", p);
    }
    os.RemoveAll(p);

}

// xreaddir replaces dst with a list of the names of the files and subdirectories in dir.
// The names are relative to dir; they are not full paths.
private static slice<@string> xreaddir(@string dir) => func((defer, _, _) => {
    var (f, err) = os.Open(dir);
    if (err != null) {
        fatalf("%v", err);
    }
    defer(f.Close());
    var (names, err) = f.Readdirnames(-1);
    if (err != null) {
        fatalf("reading %s: %v", dir, err);
    }
    return names;

});

// xreaddir replaces dst with a list of the names of the files in dir.
// The names are relative to dir; they are not full paths.
private static slice<@string> xreaddirfiles(@string dir) => func((defer, _, _) => {
    var (f, err) = os.Open(dir);
    if (err != null) {
        fatalf("%v", err);
    }
    defer(f.Close());
    var (infos, err) = f.Readdir(-1);
    if (err != null) {
        fatalf("reading %s: %v", dir, err);
    }
    slice<@string> names = default;
    foreach (var (_, fi) in infos) {
        if (!fi.IsDir()) {
            names = append(names, fi.Name());
        }
    }    return names;

});

// xworkdir creates a new temporary directory to hold object files
// and returns the name of that directory.
private static @string xworkdir() {
    var (name, err) = ioutil.TempDir(os.Getenv("GOTMPDIR"), "go-tool-dist-");
    if (err != null) {
        fatalf("%v", err);
    }
    return name;

}

// fatalf prints an error message to standard error and exits.
private static void fatalf(@string format, params object[] args) {
    args = args.Clone();

    fmt.Fprintf(os.Stderr, "go tool dist: %s\n", fmt.Sprintf(format, args));

    dieOnce.Do(() => {
        close(dying);
    }); 

    // Wait for background goroutines to finish,
    // so that exit handler that removes the work directory
    // is not fighting with active writes or open files.
    bghelpers.Wait();

    xexit(2);

}

private static slice<Action> atexits = default;

// xexit exits the process with return code n.
private static void xexit(nint n) {
    for (var i = len(atexits) - 1; i >= 0; i--) {
        atexits[i]();
    }
    os.Exit(n);
}

// xatexit schedules the exit-handler f to be run when the program exits.
private static void xatexit(Action f) {
    atexits = append(atexits, f);
}

// xprintf prints a message to standard output.
private static void xprintf(@string format, params object[] args) {
    args = args.Clone();

    fmt.Printf(format, args);
}

// errprintf prints a message to standard output.
private static void errprintf(@string format, params object[] args) {
    args = args.Clone();

    fmt.Fprintf(os.Stderr, format, args);
}

// xsamefile reports whether f1 and f2 are the same file (or dir)
private static bool xsamefile(@string f1, @string f2) {
    var (fi1, err1) = os.Stat(f1);
    var (fi2, err2) = os.Stat(f2);
    if (err1 != null || err2 != null) {
        return f1 == f2;
    }
    return os.SameFile(fi1, fi2);

}

private static @string xgetgoarm() {
    if (goos == "android") { 
        // Assume all android devices have VFPv3.
        // These ports are also mostly cross-compiled, so it makes little
        // sense to auto-detect the setting.
        return "7";

    }
    if (goos == "windows") { 
        // windows/arm only works with ARMv7 executables.
        return "7";

    }
    if (gohostarch != "arm" || goos != gohostos) { 
        // Conservative default for cross-compilation.
        return "5";

    }
    var @out = run("", 0, os.Args[0], "-check-goarm");
    var v1ok = strings.Contains(out, "VFPv1 OK.");
    var v3ok = strings.Contains(out, "VFPv3 OK.");

    if (v1ok && v3ok) {
        return "7";
    }
    if (v1ok) {
        return "6";
    }
    return "5";

}

private static nint min(nint a, nint b) {
    if (a < b) {
        return a;
    }
    return b;

}

// elfIsLittleEndian detects if the ELF file is little endian.
private static bool elfIsLittleEndian(@string fn) => func((defer, panic, _) => { 
    // read the ELF file header to determine the endianness without using the
    // debug/elf package.
    var (file, err) = os.Open(fn);
    if (err != null) {
        fatalf("failed to open file to determine endianness: %v", err);
    }
    defer(file.Close());
    array<byte> hdr = new array<byte>(16);
    {
        var (_, err) = io.ReadFull(file, hdr[..]);

        if (err != null) {
            fatalf("failed to read ELF header to determine endianness: %v", err);
        }
    } 
    // hdr[5] is EI_DATA byte, 1 is ELFDATA2LSB and 2 is ELFDATA2MSB
    switch (hdr[5]) {
        case 1: 
            return true;
            break;
        case 2: 
            return false;
            break;
        default: 
            fatalf("unknown ELF endianness of %s: EI_DATA = %d", fn, hdr[5]);
            break;
    }
    panic("unreachable");

});

// count is a flag.Value that is like a flag.Bool and a flag.Int.
// If used as -name, it increments the count, but -name=x sets the count.
// Used for verbose flag -v.
private partial struct count { // : nint
}

private static @string String(this ptr<count> _addr_c) {
    ref count c = ref _addr_c.val;

    return fmt.Sprint(int(c.val));
}

private static error Set(this ptr<count> _addr_c, @string s) {
    ref count c = ref _addr_c.val;

    switch (s) {
        case "true": 
            c.val++;
            break;
        case "false": 
            c.val = 0;
            break;
        default: 
            var (n, err) = strconv.Atoi(s);
            if (err != null) {
                return error.As(fmt.Errorf("invalid count %q", s))!;
            }
            c.val = count(n);

            break;
    }
    return error.As(null!)!;

}

private static bool IsBoolFlag(this ptr<count> _addr_c) {
    ref count c = ref _addr_c.val;

    return true;
}

private static void xflagparse(nint maxargs) {
    flag.Var((count.val)(_addr_vflag), "v", "verbosity");
    flag.Parse();
    if (maxargs >= 0 && flag.NArg() > maxargs) {
        flag.Usage();
    }
}

} // end main_package
