// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modconv -- go2cs converted at 2022 March 06 23:18:11 UTC
// import "cmd/go/internal/modconv" ==> using modconv = go.cmd.go.@internal.modconv_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modconv\dep.go
using fmt = go.fmt_package;
using lazyregexp = go.@internal.lazyregexp_package;
using url = go.net.url_package;
using path = go.path_package;
using strconv = go.strconv_package;
using strings = go.strings_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;

namespace go.cmd.go.@internal;

public static partial class modconv_package {

public static (ptr<modfile.File>, error) ParseGopkgLock(@string file, slice<byte> data) {
    ptr<modfile.File> _p0 = default!;
    error _p0 = default!;

    private partial struct pkg {
        public @string Path;
        public @string Version;
        public @string Source;
    }
    ptr<modfile.File> mf = @new<modfile.File>();
    slice<pkg> list = default;
    ptr<pkg> r;
    foreach (var (lineno, line) in strings.Split(string(data), "\n")) {
        lineno++;
        {
            var i__prev1 = i;

            var i = strings.Index(line, "#");

            if (i >= 0) {
                line = line[..(int)i];
            }
            i = i__prev1;

        }

        line = strings.TrimSpace(line);
        if (line == "[[projects]]") {
            list = append(list, new pkg());
            r = _addr_list[len(list) - 1];
            continue;
        }
        if (strings.HasPrefix(line, "[")) {
            r = null;
            continue;
        }
        if (r == null) {
            continue;
        }
        i = strings.Index(line, "=");
        if (i < 0) {
            continue;
        }
        var key = strings.TrimSpace(line[..(int)i]);
        var val = strings.TrimSpace(line[(int)i + 1..]);
        if (len(val) >= 2 && val[0] == '"' && val[len(val) - 1] == '"') {
            var (q, err) = strconv.Unquote(val); // Go unquoting, but close enough for now
            if (err != null) {
                return (_addr_null!, error.As(fmt.Errorf("%s:%d: invalid quoted string: %v", file, lineno, err))!);
            }
            val = q;

        }
        switch (key) {
            case "name": 
                r.Path = val;
                break;
            case "source": 
                r.Source = val;
                break;
            case "revision": 
                // Note: key "version" should take priority over "revision",
                // and it does, because dep writes toml keys in alphabetical order,
                // so we see version (if present) second.

            case "version": 
                // Note: key "version" should take priority over "revision",
                // and it does, because dep writes toml keys in alphabetical order,
                // so we see version (if present) second.
                if (key == "version") {
                    if (!semver.IsValid(val) || semver.Canonical(val) != val) {
                        break;
                    }
                }
                r.Version = val;

                break;
        }

    }    {
        ptr<pkg> r__prev1 = r;

        foreach (var (_, __r) in list) {
            r = __r;
            if (r.Path == "" || r.Version == "") {
                return (_addr_null!, error.As(fmt.Errorf("%s: empty [[projects]] stanza (%s)", file, r.Path))!);
            }
            mf.Require = append(mf.Require, addr(new modfile.Require(Mod:module.Version{Path:r.Path,Version:r.Version})));

            if (r.Source != "") { 
                // Convert "source" to import path, such as
                // git@test.com:x/y.git and https://test.com/x/y.git.
                // We get "test.com/x/y" at last.
                var (source, err) = decodeSource(r.Source);
                if (err != null) {
                    return (_addr_null!, error.As(err)!);
                }
                module.Version old = new module.Version(Path:r.Path,Version:r.Version);
                module.Version @new = new module.Version(Path:source,Version:r.Version);
                mf.Replace = append(mf.Replace, addr(new modfile.Replace(Old:old,New:new)));

            }
        }
        r = r__prev1;
    }

    return (_addr_mf!, error.As(null!)!);

}

private static var scpSyntaxReg = lazyregexp.New("^([a-zA-Z0-9_]+)@([a-zA-Z0-9._-]+):(.*)$");

private static (@string, error) decodeSource(@string source) {
    @string _p0 = default;
    error _p0 = default!;

    ptr<url.URL> u;
    @string p = default;
    {
        var m = scpSyntaxReg.FindStringSubmatch(source);

        if (m != null) { 
            // Match SCP-like syntax and convert it to a URL.
            // Eg, "git@github.com:user/repo" becomes
            // "ssh://git@github.com/user/repo".
            u = addr(new url.URL(Scheme:"ssh",User:url.User(m[1]),Host:m[2],Path:"/"+m[3],));

        }
        else
 {
            error err = default!;
            u, err = url.Parse(source);
            if (err != null) {
                return ("", error.As(fmt.Errorf("%q is not a valid URI", source))!);
            }
        }
    } 

    // If no scheme was passed, then the entire path will have been put into
    // u.Path. Either way, construct the normalized path correctly.
    if (u.Host == "") {
        p = source;
    }
    else
 {
        p = path.Join(u.Host, u.Path);
    }
    p = strings.TrimSuffix(p, ".git");
    p = strings.TrimSuffix(p, ".hg");
    return (p, error.As(null!)!);

}

} // end modconv_package
