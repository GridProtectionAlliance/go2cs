// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fstest implements support for testing implementations and users of file systems.
// package fstest -- go2cs converted at 2022 March 06 23:19:29 UTC
// import "testing/fstest" ==> using fstest = go.testing.fstest_package
// Original source: C:\Program Files\Go\src\testing\fstest\testfs.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using fs = go.io.fs_package;
using path = go.path_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strings = go.strings_package;
using iotest = go.testing.iotest_package;
using System;


namespace go.testing;

public static partial class fstest_package {

    // TestFS tests a file system implementation.
    // It walks the entire tree of files in fsys,
    // opening and checking that each file behaves correctly.
    // It also checks that the file system contains at least the expected files.
    // As a special case, if no expected files are listed, fsys must be empty.
    // Otherwise, fsys must contain at least the listed files; it can also contain others.
    // The contents of fsys must not change concurrently with TestFS.
    //
    // If TestFS finds any misbehaviors, it returns an error reporting all of them.
    // The error text spans multiple lines, one per detected misbehavior.
    //
    // Typical usage inside a test is:
    //
    //    if err := fstest.TestFS(myFS, "file/that/should/be/present"); err != nil {
    //        t.Fatal(err)
    //    }
    //
public static error TestFS(fs.FS fsys, params @string[] expected) {
    expected = expected.Clone();

    {
        var err__prev1 = err;

        var err = testFS(fsys, expected);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }

    {
        var name__prev1 = name;

        foreach (var (_, __name) in expected) {
            name = __name;
            {
                var i = strings.Index(name, "/");

                if (i >= 0) {
                    var dir = name[..(int)i];
                    var dirSlash = name[..(int)i + 1];
                    slice<@string> subExpected = default;
                    {
                        var name__prev2 = name;

                        foreach (var (_, __name) in expected) {
                            name = __name;
                            if (strings.HasPrefix(name, dirSlash)) {
                                subExpected = append(subExpected, name[(int)len(dirSlash)..]);
                            }
                        }
                        name = name__prev2;
                    }

                    var (sub, err) = fs.Sub(fsys, dir);
                    if (err != null) {
                        return error.As(err)!;
                    }
                    {
                        var err__prev2 = err;

                        err = testFS(sub, subExpected);

                        if (err != null) {
                            return error.As(fmt.Errorf("testing fs.Sub(fsys, %s): %v", dir, err))!;
                        }
                        err = err__prev2;

                    }

                    break; // one sub-test is enough
                }
            }

        }
        name = name__prev1;
    }

    return error.As(null!)!;

}

private static error testFS(fs.FS fsys, params @string[] expected) {
    expected = expected.Clone();

    fsTester t = new fsTester(fsys:fsys);
    t.checkDir(".");
    t.checkOpen(".");
    var found = make_map<@string, bool>();
    foreach (var (_, dir) in t.dirs) {
        found[dir] = true;
    }    foreach (var (_, file) in t.files) {
        found[file] = true;
    }    delete(found, ".");
    if (len(expected) == 0 && len(found) > 0) {
        slice<@string> list = default;
        foreach (var (k) in found) {
            if (k != ".") {
                list = append(list, k);
            }
        }        sort.Strings(list);
        if (len(list) > 15) {
            list = append(list[..(int)10], "...");
        }
        t.errorf("expected empty file system but found files:\n%s", strings.Join(list, "\n"));

    }
    foreach (var (_, name) in expected) {
        if (!found[name]) {
            t.errorf("expected but not found: %s", name);
        }
    }    if (len(t.errText) == 0) {
        return error.As(null!)!;
    }
    return error.As(errors.New("TestFS found errors:\n" + string(t.errText)))!;

}

// An fsTester holds state for running the test.
private partial struct fsTester {
    public fs.FS fsys;
    public slice<byte> errText;
    public slice<@string> dirs;
    public slice<@string> files;
}

// errorf adds an error line to errText.
private static void errorf(this ptr<fsTester> _addr_t, @string format, params object[] args) {
    args = args.Clone();
    ref fsTester t = ref _addr_t.val;

    if (len(t.errText) > 0) {
        t.errText = append(t.errText, '\n');
    }
    t.errText = append(t.errText, fmt.Sprintf(format, args));

}

private static fs.ReadDirFile openDir(this ptr<fsTester> _addr_t, @string dir) {
    ref fsTester t = ref _addr_t.val;

    var (f, err) = t.fsys.Open(dir);
    if (err != null) {
        t.errorf("%s: Open: %v", dir, err);
        return null;
    }
    fs.ReadDirFile (d, ok) = f._<fs.ReadDirFile>();
    if (!ok) {
        f.Close();
        t.errorf("%s: Open returned File type %T, not a fs.ReadDirFile", dir, f);
        return null;
    }
    return d;

}

// checkDir checks the directory dir, which is expected to exist
// (it is either the root or was found in a directory listing with IsDir true).
private static void checkDir(this ptr<fsTester> _addr_t, @string dir) => func((defer, _, _) => {
    ref fsTester t = ref _addr_t.val;
 
    // Read entire directory.
    t.dirs = append(t.dirs, dir);
    var d = t.openDir(dir);
    if (d == null) {
        return ;
    }
    var (list, err) = d.ReadDir(-1);
    if (err != null) {
        d.Close();
        t.errorf("%s: ReadDir(-1): %v", dir, err);
        return ;
    }
    @string prefix = default;
    if (dir == ".") {
        prefix = "";
    }
    else
 {
        prefix = dir + "/";
    }
    foreach (var (_, info) in list) {
        var name = info.Name();

        if (name == "." || name == ".." || name == "") 
            t.errorf("%s: ReadDir: child has invalid name: %#q", dir, name);
            continue;
        else if (strings.Contains(name, "/")) 
            t.errorf("%s: ReadDir: child name contains slash: %#q", dir, name);
            continue;
        else if (strings.Contains(name, "\\")) 
            t.errorf("%s: ReadDir: child name contains backslash: %#q", dir, name);
            continue;
                var path = prefix + name;
        t.checkStat(path, info);
        t.checkOpen(path);
        if (info.IsDir()) {
            t.checkDir(path);
        }
        else
 {
            t.checkFile(path);
        }
    }    var (list2, err) = d.ReadDir(-1);
    if (len(list2) > 0 || err != null) {
        d.Close();
        t.errorf("%s: ReadDir(-1) at EOF = %d entries, %v, wanted 0 entries, nil", dir, len(list2), err);
        return ;
    }
    list2, err = d.ReadDir(1);
    if (len(list2) > 0 || err != io.EOF) {
        d.Close();
        t.errorf("%s: ReadDir(1) at EOF = %d entries, %v, wanted 0 entries, EOF", dir, len(list2), err);
        return ;
    }
    {
        var err = d.Close();

        if (err != null) {
            t.errorf("%s: Close: %v", dir, err);
        }
    } 

    // Check that closing twice doesn't crash.
    // The return value doesn't matter.
    d.Close(); 

    // Reopen directory, read a second time, make sure contents match.
    d = t.openDir(dir);

    if (d == null) {
        return ;
    }
    defer(d.Close());
    list2, err = d.ReadDir(-1);
    if (err != null) {
        t.errorf("%s: second Open+ReadDir(-1): %v", dir, err);
        return ;
    }
    t.checkDirList(dir, "first Open+ReadDir(-1) vs second Open+ReadDir(-1)", list, list2); 

    // Reopen directory, read a third time in pieces, make sure contents match.
    d = t.openDir(dir);

    if (d == null) {
        return ;
    }
    defer(d.Close());
    list2 = null;
    while (true) {
        nint n = 1;
        if (len(list2) > 0) {
            n = 2;
        }
        var (frag, err) = d.ReadDir(n);
        if (len(frag) > n) {
            t.errorf("%s: third Open: ReadDir(%d) after %d: %d entries (too many)", dir, n, len(list2), len(frag));
            return ;
        }
        list2 = append(list2, frag);
        if (err == io.EOF) {
            break;
        }
        if (err != null) {
            t.errorf("%s: third Open: ReadDir(%d) after %d: %v", dir, n, len(list2), err);
            return ;
        }
        if (n == 0) {
            t.errorf("%s: third Open: ReadDir(%d) after %d: 0 entries but nil error", dir, n, len(list2));
            return ;
        }
    }
    t.checkDirList(dir, "first Open+ReadDir(-1) vs third Open+ReadDir(1,2) loop", list, list2); 

    // If fsys has ReadDir, check that it matches and is sorted.
    {
        fs.ReadDirFS (fsys, ok) = t.fsys._<fs.ReadDirFS>();

        if (ok) {
            (list2, err) = fsys.ReadDir(dir);
            if (err != null) {
                t.errorf("%s: fsys.ReadDir: %v", dir, err);
                return ;
            }
            t.checkDirList(dir, "first Open+ReadDir(-1) vs fsys.ReadDir", list, list2);

            {
                nint i__prev1 = i;

                for (nint i = 0; i + 1 < len(list2); i++) {
                    if (list2[i].Name() >= list2[i + 1].Name()) {
                        t.errorf("%s: fsys.ReadDir: list not sorted: %s before %s", dir, list2[i].Name(), list2[i + 1].Name());
                    }
                }


                i = i__prev1;
            }

        }
    } 

    // Check fs.ReadDir as well.
    list2, err = fs.ReadDir(t.fsys, dir);
    if (err != null) {
        t.errorf("%s: fs.ReadDir: %v", dir, err);
        return ;
    }
    t.checkDirList(dir, "first Open+ReadDir(-1) vs fs.ReadDir", list, list2);

    {
        nint i__prev1 = i;

        for (i = 0; i + 1 < len(list2); i++) {
            if (list2[i].Name() >= list2[i + 1].Name()) {
                t.errorf("%s: fs.ReadDir: list not sorted: %s before %s", dir, list2[i].Name(), list2[i + 1].Name());
            }
        }

        i = i__prev1;
    }

    t.checkGlob(dir, list);

});

// formatEntry formats an fs.DirEntry into a string for error messages and comparison.
private static @string formatEntry(fs.DirEntry entry) {
    return fmt.Sprintf("%s IsDir=%v Type=%v", entry.Name(), entry.IsDir(), entry.Type());
}

// formatInfoEntry formats an fs.FileInfo into a string like the result of formatEntry, for error messages and comparison.
private static @string formatInfoEntry(fs.FileInfo info) {
    return fmt.Sprintf("%s IsDir=%v Type=%v", info.Name(), info.IsDir(), info.Mode().Type());
}

// formatInfo formats an fs.FileInfo into a string for error messages and comparison.
private static @string formatInfo(fs.FileInfo info) {
    return fmt.Sprintf("%s IsDir=%v Mode=%v Size=%d ModTime=%v", info.Name(), info.IsDir(), info.Mode(), info.Size(), info.ModTime());
}

// checkGlob checks that various glob patterns work if the file system implements GlobFS.
private static void checkGlob(this ptr<fsTester> _addr_t, @string dir, slice<fs.DirEntry> list) {
    ref fsTester t = ref _addr_t.val;

    {
        fs.GlobFS (_, ok) = t.fsys._<fs.GlobFS>();

        if (!ok) {
            return ;
        }
    } 

    // Make a complex glob pattern prefix that only matches dir.
    @string glob = default;
    if (dir != ".") {
        var elem = strings.Split(dir, "/");
        foreach (var (i, e) in elem) {
            slice<int> pattern = default;
            foreach (var (j, r) in e) {
                if (r == '*' || r == '?' || r == '\\' || r == '[' || r == '-') {
                    pattern = append(pattern, '\\', r);
                    continue;
                }
                switch ((i + j) % 5) {
                    case 0: 
                        pattern = append(pattern, r);
                        break;
                    case 1: 
                        pattern = append(pattern, '[', r, ']');
                        break;
                    case 2: 
                        pattern = append(pattern, '[', r, '-', r, ']');
                        break;
                    case 3: 
                        pattern = append(pattern, '[', '\\', r, ']');
                        break;
                    case 4: 
                        pattern = append(pattern, '[', '\\', r, '-', '\\', r, ']');
                        break;
                }

            }
            elem[i] = string(pattern);

        }        glob = strings.Join(elem, "/") + "/";

    }
    {
        fs.GlobFS (_, err) = t.fsys._<fs.GlobFS>().Glob(glob + "nonexist/[]");

        if (err == null) {
            t.errorf("%s: Glob(%#q): bad pattern not detected", dir, glob + "nonexist/[]");
        }
    } 

    // Try to find a letter that appears in only some of the final names.
    var c = rune('a');
    while (c <= 'z') {
        var have = false;
        var haveNot = false;
        {
            var d__prev2 = d;

            foreach (var (_, __d) in list) {
                d = __d;
                if (strings.ContainsRune(d.Name(), c)) {
                    have = true;
                }
                else
 {
                    haveNot = true;
        c++;
                }

            }

            d = d__prev2;
        }

        if (have && haveNot) {
            break;
        }
    }
    if (c > 'z') {
        c = 'a';
    }
    glob += "*" + string(c) + "*";

    slice<@string> want = default;
    {
        var d__prev1 = d;

        foreach (var (_, __d) in list) {
            d = __d;
            if (strings.ContainsRune(d.Name(), c)) {
                want = append(want, path.Join(dir, d.Name()));
            }
        }
        d = d__prev1;
    }

    fs.GlobFS (names, err) = t.fsys._<fs.GlobFS>().Glob(glob);
    if (err != null) {
        t.errorf("%s: Glob(%#q): %v", dir, glob, err);
        return ;
    }
    if (reflect.DeepEqual(want, names)) {
        return ;
    }
    if (!sort.StringsAreSorted(names)) {
        t.errorf("%s: Glob(%#q): unsorted output:\n%s", dir, glob, strings.Join(names, "\n"));
        sort.Strings(names);
    }
    slice<@string> problems = default;
    while (len(want) > 0 || len(names) > 0) {

        if (len(want) > 0 && len(names) > 0 && want[0] == names[0]) 
            (want, names) = (want[(int)1..], names[(int)1..]);        else if (len(want) > 0 && (len(names) == 0 || want[0] < names[0])) 
            problems = append(problems, "missing: " + want[0]);
            want = want[(int)1..];
        else 
            problems = append(problems, "extra: " + names[0]);
            names = names[(int)1..];
        
    }
    t.errorf("%s: Glob(%#q): wrong output:\n%s", dir, glob, strings.Join(problems, "\n"));

}

// checkStat checks that a direct stat of path matches entry,
// which was found in the parent's directory listing.
private static void checkStat(this ptr<fsTester> _addr_t, @string path, fs.DirEntry entry) {
    ref fsTester t = ref _addr_t.val;

    var (file, err) = t.fsys.Open(path);
    if (err != null) {
        t.errorf("%s: Open: %v", path, err);
        return ;
    }
    var (info, err) = file.Stat();
    file.Close();
    if (err != null) {
        t.errorf("%s: Stat: %v", path, err);
        return ;
    }
    var fentry = formatEntry(entry);
    var fientry = formatInfoEntry(info); 
    // Note: mismatch here is OK for symlink, because Open dereferences symlink.
    if (fentry != fientry && entry.Type() & fs.ModeSymlink == 0) {
        t.errorf("%s: mismatch:\n\tentry = %s\n\tfile.Stat() = %s", path, fentry, fientry);
    }
    var (einfo, err) = entry.Info();
    if (err != null) {
        t.errorf("%s: entry.Info: %v", path, err);
        return ;
    }
    var finfo = formatInfo(info);
    if (entry.Type() & fs.ModeSymlink != 0) { 
        // For symlink, just check that entry.Info matches entry on common fields.
        // Open deferences symlink, so info itself may differ.
        var feentry = formatInfoEntry(einfo);
        if (fentry != feentry) {
            t.errorf("%s: mismatch\n\tentry = %s\n\tentry.Info() = %s\n", path, fentry, feentry);
        }
    }
    else
 {
        var feinfo = formatInfo(einfo);
        if (feinfo != finfo) {
            t.errorf("%s: mismatch:\n\tentry.Info() = %s\n\tfile.Stat() = %s\n", path, feinfo, finfo);
        }
    }
    var (info2, err) = fs.Stat(t.fsys, path);
    if (err != null) {
        t.errorf("%s: fs.Stat: %v", path, err);
        return ;
    }
    var finfo2 = formatInfo(info2);
    if (finfo2 != finfo) {
        t.errorf("%s: fs.Stat(...) = %s\n\twant %s", path, finfo2, finfo);
    }
    {
        fs.StatFS (fsys, ok) = t.fsys._<fs.StatFS>();

        if (ok) {
            (info2, err) = fsys.Stat(path);
            if (err != null) {
                t.errorf("%s: fsys.Stat: %v", path, err);
                return ;
            }
            finfo2 = formatInfo(info2);
            if (finfo2 != finfo) {
                t.errorf("%s: fsys.Stat(...) = %s\n\twant %s", path, finfo2, finfo);
            }
        }
    }

}

// checkDirList checks that two directory lists contain the same files and file info.
// The order of the lists need not match.
private static void checkDirList(this ptr<fsTester> _addr_t, @string dir, @string desc, slice<fs.DirEntry> list1, slice<fs.DirEntry> list2) {
    ref fsTester t = ref _addr_t.val;

    var old = make_map<@string, fs.DirEntry>();
    Action<fs.DirEntry> checkMode = entry => {
        if (entry.IsDir() != (entry.Type() & fs.ModeDir != 0)) {
            if (entry.IsDir()) {
                t.errorf("%s: ReadDir returned %s with IsDir() = true, Type() & ModeDir = 0", dir, entry.Name());
            }
            else
 {
                t.errorf("%s: ReadDir returned %s with IsDir() = false, Type() & ModeDir = ModeDir", dir, entry.Name());
            }

        }
    };

    {
        var entry1__prev1 = entry1;

        foreach (var (_, __entry1) in list1) {
            entry1 = __entry1;
            old[entry1.Name()] = entry1;
            checkMode(entry1);
        }
        entry1 = entry1__prev1;
    }

    slice<@string> diffs = default;
    foreach (var (_, entry2) in list2) {
        var entry1 = old[entry2.Name()];
        if (entry1 == null) {
            checkMode(entry2);
            diffs = append(diffs, "+ " + formatEntry(entry2));
            continue;
        }
        if (formatEntry(entry1) != formatEntry(entry2)) {
            diffs = append(diffs, "- " + formatEntry(entry1), "+ " + formatEntry(entry2));
        }
        delete(old, entry2.Name());

    }    {
        var entry1__prev1 = entry1;

        foreach (var (_, __entry1) in old) {
            entry1 = __entry1;
            diffs = append(diffs, "- " + formatEntry(entry1));
        }
        entry1 = entry1__prev1;
    }

    if (len(diffs) == 0) {
        return ;
    }
    sort.Slice(diffs, (i, j) => {
        var fi = strings.Fields(diffs[i]);
        var fj = strings.Fields(diffs[j]); 
        // sort by name (i < j) and then +/- (j < i, because + < -)
        return fi[1] + " " + fj[0] < fj[1] + " " + fi[0];

    });

    t.errorf("%s: diff %s:\n\t%s", dir, desc, strings.Join(diffs, "\n\t"));

}

// checkFile checks that basic file reading works correctly.
private static void checkFile(this ptr<fsTester> _addr_t, @string file) => func((defer, _, _) => {
    ref fsTester t = ref _addr_t.val;

    t.files = append(t.files, file); 

    // Read entire file.
    var (f, err) = t.fsys.Open(file);
    if (err != null) {
        t.errorf("%s: Open: %v", file, err);
        return ;
    }
    var (data, err) = io.ReadAll(f);
    if (err != null) {
        f.Close();
        t.errorf("%s: Open+ReadAll: %v", file, err);
        return ;
    }
    {
        var err__prev1 = err;

        var err = f.Close();

        if (err != null) {
            t.errorf("%s: Close: %v", file, err);
        }
        err = err__prev1;

    } 

    // Check that closing twice doesn't crash.
    // The return value doesn't matter.
    f.Close(); 

    // Check that ReadFile works if present.
    {
        fs.ReadFileFS (fsys, ok) = t.fsys._<fs.ReadFileFS>();

        if (ok) {
            var (data2, err) = fsys.ReadFile(file);
            if (err != null) {
                t.errorf("%s: fsys.ReadFile: %v", file, err);
                return ;
            }
            t.checkFileRead(file, "ReadAll vs fsys.ReadFile", data, data2); 

            // Modify the data and check it again. Modifying the
            // returned byte slice should not affect the next call.
            foreach (var (i) in data2) {
                data2[i]++;
            }
            data2, err = fsys.ReadFile(file);
            if (err != null) {
                t.errorf("%s: second call to fsys.ReadFile: %v", file, err);
                return ;
            }

            t.checkFileRead(file, "Readall vs second fsys.ReadFile", data, data2);

            t.checkBadPath(file, "ReadFile", name => {
                var (_, err) = fsys.ReadFile(name);

                return err;
            });

        }
    } 

    // Check that fs.ReadFile works with t.fsys.
    (data2, err) = fs.ReadFile(t.fsys, file);
    if (err != null) {
        t.errorf("%s: fs.ReadFile: %v", file, err);
        return ;
    }
    t.checkFileRead(file, "ReadAll vs fs.ReadFile", data, data2); 

    // Use iotest.TestReader to check small reads, Seek, ReadAt.
    f, err = t.fsys.Open(file);
    if (err != null) {
        t.errorf("%s: second Open: %v", file, err);
        return ;
    }
    defer(f.Close());
    {
        var err__prev1 = err;

        err = iotest.TestReader(f, data);

        if (err != null) {
            t.errorf("%s: failed TestReader:\n\t%s", file, strings.ReplaceAll(err.Error(), "\n", "\n\t"));
        }
        err = err__prev1;

    }

});

private static void checkFileRead(this ptr<fsTester> _addr_t, @string file, @string desc, slice<byte> data1, slice<byte> data2) {
    ref fsTester t = ref _addr_t.val;

    if (string(data1) != string(data2)) {
        t.errorf("%s: %s: different data returned\n\t%q\n\t%q", file, desc, data1, data2);
        return ;
    }
}

// checkBadPath checks that various invalid forms of file's name cannot be opened using t.fsys.Open.
private static void checkOpen(this ptr<fsTester> _addr_t, @string file) {
    ref fsTester t = ref _addr_t.val;

    t.checkBadPath(file, "Open", file => {
        var (f, err) = t.fsys.Open(file);
        if (err == null) {
            f.Close();
        }
        return err;

    });

}

// checkBadPath checks that various invalid forms of file's name cannot be opened using open.
private static error checkBadPath(this ptr<fsTester> _addr_t, @string file, @string desc, Func<@string, error> open) {
    ref fsTester t = ref _addr_t.val;

    @string bad = new slice<@string>(new @string[] { "/"+file, file+"/." });
    if (file == ".") {
        bad = append(bad, "/");
    }
    {
        var i__prev1 = i;

        var i = strings.Index(file, "/");

        if (i >= 0) {
            bad = append(bad, file[..(int)i] + "//" + file[(int)i + 1..], file[..(int)i] + "/./" + file[(int)i + 1..], file[..(int)i] + "\\" + file[(int)i + 1..], file[..(int)i] + "/../" + file);
        }
        i = i__prev1;

    }

    {
        var i__prev1 = i;

        i = strings.LastIndex(file, "/");

        if (i >= 0) {
            bad = append(bad, file[..(int)i] + "//" + file[(int)i + 1..], file[..(int)i] + "/./" + file[(int)i + 1..], file[..(int)i] + "\\" + file[(int)i + 1..], file + "/../" + file[(int)i + 1..]);
        }
        i = i__prev1;

    }


    foreach (var (_, b) in bad) {
        {
            var err = open(b);

            if (err == null) {
                t.errorf("%s: %s(%s) succeeded, want error", file, desc, b);
            }

        }

    }
}

} // end fstest_package
