// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2022 March 06 22:32:22 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Program Files\Go\src\cmd\internal\objabi\flag.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using System;


namespace go.cmd.@internal;

public static partial class objabi_package {

public static void Flagcount(@string name, @string usage, ptr<nint> _addr_val) {
    ref nint val = ref _addr_val.val;

    flag.Var((count.val)(val), name, usage);
}

public static void Flagfn1(@string name, @string usage, Action<@string> f) {
    flag.Var(fn1(f), name, usage);
}

public static void Flagprint(io.Writer w) {
    flag.CommandLine.SetOutput(w);
    flag.PrintDefaults();
}

public static void Flagparse(Action usage) {
    flag.Usage = usage;
    os.Args = expandArgs(os.Args);
    flag.Parse();
}

// expandArgs expands "response files" arguments in the provided slice.
//
// A "response file" argument starts with '@' and the rest of that
// argument is a filename with CR-or-CRLF-separated arguments. Each
// argument in the named files can also contain response file
// arguments. See Issue 18468.
//
// The returned slice 'out' aliases 'in' iff the input did not contain
// any response file arguments.
//
// TODO: handle relative paths of recursive expansions in different directories?
// Is there a spec for this? Are relative paths allowed?
private static slice<@string> expandArgs(slice<@string> @in) {
    slice<@string> @out = default;
 
    // out is nil until we see a "@" argument.
    {
        var i__prev1 = i;

        foreach (var (__i, __s) in in) {
            i = __i;
            s = __s;
            if (strings.HasPrefix(s, "@")) {
                if (out == null) {
                    out = make_slice<@string>(0, len(in) * 2);
                    out = append(out, in[..(int)i]);
                }
                var (slurp, err) = ioutil.ReadFile(s[(int)1..]);
                if (err != null) {
                    log.Fatal(err);
                }
                var args = strings.Split(strings.TrimSpace(strings.Replace(string(slurp), "\r", "", -1)), "\n");
                {
                    var i__prev2 = i;

                    foreach (var (__i, __arg) in args) {
                        i = __i;
                        arg = __arg;
                        args[i] = DecodeArg(arg);
                    }

                    i = i__prev2;
                }

                out = append(out, expandArgs(args));

            }
            else if (out != null) {
                out = append(out, s);
            }

        }
        i = i__prev1;
    }

    if (out == null) {
        return in;
    }
    return ;

}

public static void AddVersionFlag() {
    flag.Var(new versionFlag(), "V", "print version and exit");
}

private static @string buildID = default; // filled in by linker

private partial struct versionFlag {
}

private static bool IsBoolFlag(this versionFlag _p0) {
    return true;
}
private static void Get(this versionFlag _p0) {
    return null;
}
private static @string String(this versionFlag _p0) {
    return "";
}
private static error Set(this versionFlag _p0, @string s) {
    var name = os.Args[0];
    name = name[(int)strings.LastIndex(name, "/") + 1..];
    name = name[(int)strings.LastIndex(name, "\\") + 1..];
    name = strings.TrimSuffix(name, ".exe");

    @string p = "";

    if (s == "goexperiment") { 
        // test/run.go uses this to discover the full set of
        // experiment tags. Report everything.
        p = " X:" + strings.Join(buildcfg.AllExperiments(), ",");

    }
    else
 { 
        // If the enabled experiments differ from the defaults,
        // include that difference.
        {
            var goexperiment = buildcfg.GOEXPERIMENT();

            if (goexperiment != "") {
                p = " X:" + goexperiment;
            }

        }

    }
    if (s == "full") {
        if (strings.HasPrefix(buildcfg.Version, "devel")) {
            p += " buildID=" + buildID;
        }
    }
    fmt.Printf("%s version %s%s\n", name, buildcfg.Version, p);
    os.Exit(0);
    return error.As(null!)!;

}

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

private static void Get(this ptr<count> _addr_c) {
    ref count c = ref _addr_c.val;

    return int(c.val);
}

private static bool IsBoolFlag(this ptr<count> _addr_c) {
    ref count c = ref _addr_c.val;

    return true;
}

private static bool IsCountFlag(this ptr<count> _addr_c) {
    ref count c = ref _addr_c.val;

    return true;
}

public delegate void fn1(@string);

private static error Set(this fn1 f, @string s) {
    f(s);
    return error.As(null!)!;
}

private static @string String(this fn1 f) {
    return "";
}

// DecodeArg decodes an argument.
//
// This function is public for testing with the parallel encoder.
public static @string DecodeArg(@string arg) => func((_, panic, _) => { 
    // If no encoding, fastpath out.
    if (!strings.ContainsAny(arg, "\\\n")) {
        return arg;
    }
    bytes.Buffer b = default;
    bool wasBS = default;
    foreach (var (_, r) in arg) {
        if (wasBS) {
            switch (r) {
                case '\\': 
                    b.WriteByte('\\');
                    break;
                case 'n': 
                    b.WriteByte('\n');
                    break;
                default: 
                    // This shouldn't happen. The only backslashes that reach here
                    // should encode '\n' and '\\' exclusively.
                    panic("badly formatted input");
                    break;
            }

        }
        else if (r == '\\') {
            wasBS = true;
            continue;
        }
        else
 {
            b.WriteRune(r);
        }
        wasBS = false;

    }    return b.String();

});

} // end objabi_package
