// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2022 March 13 06:32:18 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\proxy.go
namespace go.cmd.go.@internal;

using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using fs = io.fs_package;
using url = net.url_package;
using path = path_package;
using path = path_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;

using @base = cmd.go.@internal.@base_package;
using cfg = cmd.go.@internal.cfg_package;
using codehost = cmd.go.@internal.modfetch.codehost_package;
using web = cmd.go.@internal.web_package;

using module = golang.org.x.mod.module_package;
using semver = golang.org.x.mod.semver_package;
using System;

public static partial class modfetch_package {

public static ptr<base.Command> HelpGoproxy = addr(new base.Command(UsageLine:"goproxy",Short:"module proxy protocol",Long:`
A Go module proxy is any web server that can respond to GET requests for
URLs of a specified form. The requests have no query parameters, so even
a site serving from a fixed file system (including a file:/// URL)
can be a module proxy.

For details on the GOPROXY protocol, see
https://golang.org/ref/mod#goproxy-protocol.
`,));

private static var proxyOnce = default;

private partial struct proxySpec {
    public @string url; // fallBackOnError is true if a request should be attempted on the next proxy
// in the list after any error from this proxy. If fallBackOnError is false,
// the request will only be attempted on the next proxy if the error is
// equivalent to os.ErrNotFound, which is true for 404 and 410 responses.
    public bool fallBackOnError;
}

private static (slice<proxySpec>, error) proxyList() {
    slice<proxySpec> _p0 = default;
    error _p0 = default!;

    proxyOnce.Do(() => {
        if (cfg.GONOPROXY != "" && cfg.GOPROXY != "direct") {
            proxyOnce.list = append(proxyOnce.list, new proxySpec(url:"noproxy"));
        }
        var goproxy = cfg.GOPROXY;
        while (goproxy != "") {
            @string url = default;
            var fallBackOnError = false;
            {
                var i = strings.IndexAny(goproxy, ",|");

                if (i >= 0) {
                    url = goproxy[..(int)i];
                    fallBackOnError = goproxy[i] == '|';
                    goproxy = goproxy[(int)i + 1..];
                }
                else
 {
                    url = goproxy;
                    goproxy = "";
                }

            }

            url = strings.TrimSpace(url);
            if (url == "") {
                continue;
            }
            if (url == "off") { 
                // "off" always fails hard, so can stop walking list.
                proxyOnce.list = append(proxyOnce.list, new proxySpec(url:"off"));
                break;
            }
            if (url == "direct") {
                proxyOnce.list = append(proxyOnce.list, new proxySpec(url:"direct")); 
                // For now, "direct" is the end of the line. We may decide to add some
                // sort of fallback behavior for them in the future, so ignore
                // subsequent entries for forward-compatibility.
                break;
            } 

            // Single-word tokens are reserved for built-in behaviors, and anything
            // containing the string ":/" or matching an absolute file path must be a
            // complete URL. For all other paths, implicitly add "https://".
            if (strings.ContainsAny(url, ".:/") && !strings.Contains(url, ":/") && !filepath.IsAbs(url) && !path.IsAbs(url)) {
                url = "https://" + url;
            } 

            // Check that newProxyRepo accepts the URL.
            // It won't do anything with the path.
            {
                var (_, err) = newProxyRepo(url, "golang.org/x/text");

                if (err != null) {
                    proxyOnce.err = err;
                    return ;
                }

            }

            proxyOnce.list = append(proxyOnce.list, new proxySpec(url:url,fallBackOnError:fallBackOnError,));
        }

        if (len(proxyOnce.list) == 0 || len(proxyOnce.list) == 1 && proxyOnce.list[0].url == "noproxy") { 
            // There were no proxies, other than the implicit "noproxy" added when
            // GONOPROXY is set. This can happen if GOPROXY is a non-empty string
            // like "," or " ".
            proxyOnce.err = fmt.Errorf("GOPROXY list is not the empty string, but contains no entries");
        }
    });

    return (proxyOnce.list, error.As(proxyOnce.err)!);
}

// TryProxies iterates f over each configured proxy (including "noproxy" and
// "direct" if applicable) until f returns no error or until f returns an
// error that is not equivalent to fs.ErrNotExist on a proxy configured
// not to fall back on errors.
//
// TryProxies then returns that final error.
//
// If GOPROXY is set to "off", TryProxies invokes f once with the argument
// "off".
public static error TryProxies(Func<@string, error> f) => func((_, panic, _) => {
    var (proxies, err) = proxyList();
    if (err != null) {
        return error.As(err)!;
    }
    if (len(proxies) == 0) {
        panic("GOPROXY list is empty");
    }
    const var notExistRank = iota;
    const var proxyRank = 0;
    const var directRank = 1;
    error bestErr = default!;
    var bestErrRank = notExistRank;
    foreach (var (_, proxy) in proxies) {
        var err = f(proxy.url);
        if (err == null) {
            return error.As(null!)!;
        }
        var isNotExistErr = errors.Is(err, fs.ErrNotExist);

        if (proxy.url == "direct" || (proxy.url == "noproxy" && err != errUseProxy)) {
            bestErr = error.As(err)!;
            bestErrRank = directRank;
        }
        else if (bestErrRank <= proxyRank && !isNotExistErr) {
            bestErr = error.As(err)!;
            bestErrRank = proxyRank;
        }
        else if (bestErrRank == notExistRank) {
            bestErr = error.As(err)!;
        }
        if (!proxy.fallBackOnError && !isNotExistErr) {
            break;
        }
    }    return error.As(bestErr)!;
});

private partial struct proxyRepo {
    public ptr<url.URL> url;
    public @string path;
    public @string redactedURL;
}

private static (Repo, error) newProxyRepo(@string baseURL, @string path) {
    Repo _p0 = default;
    error _p0 = default!;

    var (base, err) = url.Parse(baseURL);
    if (err != null) {
        return (null, error.As(err)!);
    }
    switch (@base.Scheme) {
        case "http": 

        case "https": 

            break;
        case "file": 
            if (base != (new url.URL(Scheme:base.Scheme,Path:base.Path,RawPath:base.RawPath)).val) {
                return (null, error.As(fmt.Errorf("invalid file:// proxy URL with non-path elements: %s", @base.Redacted()))!);
            }
            break;
        case "": 
            return (null, error.As(fmt.Errorf("invalid proxy URL missing scheme: %s", @base.Redacted()))!);
            break;
        default: 
            return (null, error.As(fmt.Errorf("invalid proxy URL scheme (must be https, http, file): %s", @base.Redacted()))!);
            break;
    }

    var (enc, err) = module.EscapePath(path);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var redactedURL = @base.Redacted();
    @base.Path = strings.TrimSuffix(@base.Path, "/") + "/" + enc;
    @base.RawPath = strings.TrimSuffix(@base.RawPath, "/") + "/" + pathEscape(enc);
    return (addr(new proxyRepo(base,path,redactedURL)), error.As(null!)!);
}

private static @string ModulePath(this ptr<proxyRepo> _addr_p) {
    ref proxyRepo p = ref _addr_p.val;

    return p.path;
}

// versionError returns err wrapped in a ModuleError for p.path.
private static error versionError(this ptr<proxyRepo> _addr_p, @string version, error err) {
    ref proxyRepo p = ref _addr_p.val;

    if (version != "" && version != module.CanonicalVersion(version)) {
        return error.As(addr(new module.ModuleError(Path:p.path,Err:&module.InvalidVersionError{Version:version,Pseudo:module.IsPseudoVersion(version),Err:err,},))!)!;
    }
    return error.As(addr(new module.ModuleError(Path:p.path,Version:version,Err:err,))!)!;
}

private static (slice<byte>, error) getBytes(this ptr<proxyRepo> _addr_p, @string path) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    var (body, err) = p.getBody(path);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(body.Close());
    return io.ReadAll(body);
});

private static (io.ReadCloser, error) getBody(this ptr<proxyRepo> _addr_p, @string path) {
    io.ReadCloser _p0 = default;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    var fullPath = pathpkg.Join(p.url.Path, path);

    ref var target = ref heap(p.url.val, out ptr<var> _addr_target);
    target.Path = fullPath;
    target.RawPath = pathpkg.Join(target.RawPath, pathEscape(path));

    var (resp, err) = web.Get(web.DefaultSecurity, _addr_target);
    if (err != null) {
        return (null, error.As(err)!);
    }
    {
        var err = resp.Err();

        if (err != null) {
            resp.Body.Close();
            return (null, error.As(err)!);
        }
    }
    return (resp.Body, error.As(null!)!);
}

private static (slice<@string>, error) Versions(this ptr<proxyRepo> _addr_p, @string prefix) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    var (data, err) = p.getBytes("@v/list");
    if (err != null) {
        return (null, error.As(p.versionError("", err))!);
    }
    slice<@string> list = default;
    foreach (var (_, line) in strings.Split(string(data), "\n")) {
        var f = strings.Fields(line);
        if (len(f) >= 1 && semver.IsValid(f[0]) && strings.HasPrefix(f[0], prefix) && !module.IsPseudoVersion(f[0])) {
            list = append(list, f[0]);
        }
    }    semver.Sort(list);
    return (list, error.As(null!)!);
}

private static (ptr<RevInfo>, error) latest(this ptr<proxyRepo> _addr_p) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    var (data, err) = p.getBytes("@v/list");
    if (err != null) {
        return (_addr_null!, error.As(p.versionError("", err))!);
    }
    time.Time bestTime = default;    bool bestTimeIsFromPseudo = default;    @string bestVersion = default;

    foreach (var (_, line) in strings.Split(string(data), "\n")) {
        var f = strings.Fields(line);
        if (len(f) >= 1 && semver.IsValid(f[0])) { 
            // If the proxy includes timestamps, prefer the timestamp it reports.
            // Otherwise, derive the timestamp from the pseudo-version.
            time.Time ft = default;            var ftIsFromPseudo = false;
            if (len(f) >= 2) {
                ft, _ = time.Parse(time.RFC3339, f[1]);
            }
            else if (module.IsPseudoVersion(f[0])) {
                ft, _ = module.PseudoVersionTime(f[0]);
                ftIsFromPseudo = true;
            }
            else
 { 
                // Repo.Latest promises that this method is only called where there are
                // no tagged versions. Ignore any tagged versions that were added in the
                // meantime.
                continue;
            }
            if (bestTime.Before(ft)) {
                bestTime = ft;
                bestTimeIsFromPseudo = ftIsFromPseudo;
                bestVersion = f[0];
            }
        }
    }    if (bestVersion == "") {
        return (_addr_null!, error.As(p.versionError("", codehost.ErrNoCommits))!);
    }
    if (bestTimeIsFromPseudo) { 
        // We parsed bestTime from the pseudo-version, but that's in UTC and we're
        // supposed to report the timestamp as reported by the VCS.
        // Stat the selected version to canonicalize the timestamp.
        //
        // TODO(bcmills): Should we also stat other versions to ensure that we
        // report the correct Name and Short for the revision?
        return _addr_p.Stat(bestVersion)!;
    }
    return (addr(new RevInfo(Version:bestVersion,Name:bestVersion,Short:bestVersion,Time:bestTime,)), error.As(null!)!);
}

private static (ptr<RevInfo>, error) Stat(this ptr<proxyRepo> _addr_p, @string rev) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    var (encRev, err) = module.EscapeVersion(rev);
    if (err != null) {
        return (_addr_null!, error.As(p.versionError(rev, err))!);
    }
    var (data, err) = p.getBytes("@v/" + encRev + ".info");
    if (err != null) {
        return (_addr_null!, error.As(p.versionError(rev, err))!);
    }
    ptr<RevInfo> info = @new<RevInfo>();
    {
        var err = json.Unmarshal(data, info);

        if (err != null) {
            return (_addr_null!, error.As(p.versionError(rev, fmt.Errorf("invalid response from proxy %q: %w", p.redactedURL, err)))!);
        }
    }
    if (info.Version != rev && rev == module.CanonicalVersion(rev) && module.Check(p.path, rev) == null) { 
        // If we request a correct, appropriate version for the module path, the
        // proxy must return either exactly that version or an error — not some
        // arbitrary other version.
        return (_addr_null!, error.As(p.versionError(rev, fmt.Errorf("proxy returned info for version %s instead of requested version", info.Version)))!);
    }
    return (_addr_info!, error.As(null!)!);
}

private static (ptr<RevInfo>, error) Latest(this ptr<proxyRepo> _addr_p) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    var (data, err) = p.getBytes("@latest");
    if (err != null) {
        if (!errors.Is(err, fs.ErrNotExist)) {
            return (_addr_null!, error.As(p.versionError("", err))!);
        }
        return _addr_p.latest()!;
    }
    ptr<RevInfo> info = @new<RevInfo>();
    {
        var err = json.Unmarshal(data, info);

        if (err != null) {
            return (_addr_null!, error.As(p.versionError("", fmt.Errorf("invalid response from proxy %q: %w", p.redactedURL, err)))!);
        }
    }
    return (_addr_info!, error.As(null!)!);
}

private static (slice<byte>, error) GoMod(this ptr<proxyRepo> _addr_p, @string version) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref proxyRepo p = ref _addr_p.val;

    if (version != module.CanonicalVersion(version)) {
        return (null, error.As(p.versionError(version, fmt.Errorf("internal error: version passed to GoMod is not canonical")))!);
    }
    var (encVer, err) = module.EscapeVersion(version);
    if (err != null) {
        return (null, error.As(p.versionError(version, err))!);
    }
    var (data, err) = p.getBytes("@v/" + encVer + ".mod");
    if (err != null) {
        return (null, error.As(p.versionError(version, err))!);
    }
    return (data, error.As(null!)!);
}

private static error Zip(this ptr<proxyRepo> _addr_p, io.Writer dst, @string version) => func((defer, _, _) => {
    ref proxyRepo p = ref _addr_p.val;

    if (version != module.CanonicalVersion(version)) {
        return error.As(p.versionError(version, fmt.Errorf("internal error: version passed to Zip is not canonical")))!;
    }
    var (encVer, err) = module.EscapeVersion(version);
    if (err != null) {
        return error.As(p.versionError(version, err))!;
    }
    var (body, err) = p.getBody("@v/" + encVer + ".zip");
    if (err != null) {
        return error.As(p.versionError(version, err))!;
    }
    defer(body.Close());

    ptr<io.LimitedReader> lr = addr(new io.LimitedReader(R:body,N:codehost.MaxZipFile+1));
    {
        var (_, err) = io.Copy(dst, lr);

        if (err != null) {
            return error.As(p.versionError(version, err))!;
        }
    }
    if (lr.N <= 0) {
        return error.As(p.versionError(version, fmt.Errorf("downloaded zip file too large")))!;
    }
    return error.As(null!)!;
});

// pathEscape escapes s so it can be used in a path.
// That is, it escapes things like ? and # (which really shouldn't appear anyway).
// It does not escape / to %2F: our REST API is designed so that / can be left as is.
private static @string pathEscape(@string s) {
    return strings.ReplaceAll(url.PathEscape(s), "%2F", "/");
}

} // end modfetch_package
