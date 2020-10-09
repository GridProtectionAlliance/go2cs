// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2020 October 09 05:47:21 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\proxy.go
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using url = go.net.url_package;
using os = go.os_package;
using path = go.path_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using codehost = go.cmd.go.@internal.modfetch.codehost_package;
using web = go.cmd.go.@internal.web_package;

using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        public static ptr<base.Command> HelpGoproxy = addr(new base.Command(UsageLine:"goproxy",Short:"module proxy protocol",Long:`
A Go module proxy is any web server that can respond to GET requests for
URLs of a specified form. The requests have no query parameters, so even
a site serving from a fixed file system (including a file:/// URL)
can be a module proxy.

The GET requests sent to a Go module proxy are:

GET $GOPROXY/<module>/@v/list returns a list of known versions of the given
module, one per line.

GET $GOPROXY/<module>/@v/<version>.info returns JSON-formatted metadata
about that version of the given module.

GET $GOPROXY/<module>/@v/<version>.mod returns the go.mod file
for that version of the given module.

GET $GOPROXY/<module>/@v/<version>.zip returns the zip archive
for that version of the given module.

GET $GOPROXY/<module>/@latest returns JSON-formatted metadata about the
latest known version of the given module in the same format as
<module>/@v/<version>.info. The latest version should be the version of
the module the go command may use if <module>/@v/list is empty or no
listed version is suitable. <module>/@latest is optional and may not
be implemented by a module proxy.

When resolving the latest version of a module, the go command will request
<module>/@v/list, then, if no suitable versions are found, <module>/@latest.
The go command prefers, in order: the semantically highest release version,
the semantically highest pre-release version, and the chronologically
most recent pseudo-version. In Go 1.12 and earlier, the go command considered
pseudo-versions in <module>/@v/list to be pre-release versions, but this is
no longer true since Go 1.13.

To avoid problems when serving from case-sensitive file systems,
the <module> and <version> elements are case-encoded, replacing every
uppercase letter with an exclamation mark followed by the corresponding
lower-case letter: github.com/Azure encodes as github.com/!azure.

The JSON-formatted metadata about a given module corresponds to
this Go data structure, which may be expanded in the future:

    type Info struct {
        Version string    // version string
        Time    time.Time // commit time
    }

The zip archive for a specific version of a given module is a
standard zip file that contains the file tree corresponding
to the module's source code and related files. The archive uses
slash-separated paths, and every file path in the archive must
begin with <module>@<version>/, where the module and version are
substituted directly, not case-encoded. The root of the module
file tree corresponds to the <module>@<version>/ prefix in the
archive.

Even when downloading directly from version control systems,
the go command synthesizes explicit info, mod, and zip files
and stores them in its local cache, $GOPATH/pkg/mod/cache/download,
the same as if it had downloaded them directly from a proxy.
The cache layout is the same as the proxy URL space, so
serving $GOPATH/pkg/mod/cache/download at (or copying it to)
https://example.com/proxy would let other users access those
cached module versions with GOPROXY=https://example.com/proxy.
`,));

        private static var proxyOnce = default;

        private partial struct proxySpec
        {
            public @string url; // fallBackOnError is true if a request should be attempted on the next proxy
// in the list after any error from this proxy. If fallBackOnError is false,
// the request will only be attempted on the next proxy if the error is
// equivalent to os.ErrNotFound, which is true for 404 and 410 responses.
            public bool fallBackOnError;
        }

        private static (slice<proxySpec>, error) proxyList()
        {
            slice<proxySpec> _p0 = default;
            error _p0 = default!;

            proxyOnce.Do(() =>
            {
                if (cfg.GONOPROXY != "" && cfg.GOPROXY != "direct")
                {
                    proxyOnce.list = append(proxyOnce.list, new proxySpec(url:"noproxy"));
                }

                var goproxy = cfg.GOPROXY;
                while (goproxy != "")
                {
                    @string url = default;
                    var fallBackOnError = false;
                    {
                        var i = strings.IndexAny(goproxy, ",|");

                        if (i >= 0L)
                        {
                            url = goproxy[..i];
                            fallBackOnError = goproxy[i] == '|';
                            goproxy = goproxy[i + 1L..];
                        }
                        else
                        {
                            url = goproxy;
                            goproxy = "";
                        }

                    }


                    url = strings.TrimSpace(url);
                    if (url == "")
                    {
                        continue;
                    }

                    if (url == "off")
                    { 
                        // "off" always fails hard, so can stop walking list.
                        proxyOnce.list = append(proxyOnce.list, new proxySpec(url:"off"));
                        break;

                    }

                    if (url == "direct")
                    {
                        proxyOnce.list = append(proxyOnce.list, new proxySpec(url:"direct")); 
                        // For now, "direct" is the end of the line. We may decide to add some
                        // sort of fallback behavior for them in the future, so ignore
                        // subsequent entries for forward-compatibility.
                        break;

                    } 

                    // Single-word tokens are reserved for built-in behaviors, and anything
                    // containing the string ":/" or matching an absolute file path must be a
                    // complete URL. For all other paths, implicitly add "https://".
                    if (strings.ContainsAny(url, ".:/") && !strings.Contains(url, ":/") && !filepath.IsAbs(url) && !path.IsAbs(url))
                    {
                        url = "https://" + url;
                    } 

                    // Check that newProxyRepo accepts the URL.
                    // It won't do anything with the path.
                    {
                        var (_, err) = newProxyRepo(url, "golang.org/x/text");

                        if (err != null)
                        {
                            proxyOnce.err = err;
                            return ;
                        }

                    }


                    proxyOnce.list = append(proxyOnce.list, new proxySpec(url:url,fallBackOnError:fallBackOnError,));

                }


                if (len(proxyOnce.list) == 0L || len(proxyOnce.list) == 1L && proxyOnce.list[0L].url == "noproxy")
                { 
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
        // error that is not equivalent to os.ErrNotExist on a proxy configured
        // not to fall back on errors.
        //
        // TryProxies then returns that final error.
        //
        // If GOPROXY is set to "off", TryProxies invokes f once with the argument
        // "off".
        public static error TryProxies(Func<@string, error> f) => func((_, panic, __) =>
        {
            var (proxies, err) = proxyList();
            if (err != null)
            {
                return error.As(err)!;
            }

            if (len(proxies) == 0L)
            {
                panic("GOPROXY list is empty");
            } 

            // We try to report the most helpful error to the user. "direct" and "noproxy"
            // errors are best, followed by proxy errors other than ErrNotExist, followed
            // by ErrNotExist.
            //
            // Note that errProxyOff, errNoproxy, and errUseProxy are equivalent to
            // ErrNotExist. errUseProxy should only be returned if "noproxy" is the only
            // proxy. errNoproxy should never be returned, since there should always be a
            // more useful error from "noproxy" first.
            const var notExistRank = iota;
            const var proxyRank = 0;
            const var directRank = 1;

            error bestErr = default!;
            var bestErrRank = notExistRank;
            foreach (var (_, proxy) in proxies)
            {
                var err = f(proxy.url);
                if (err == null)
                {
                    return error.As(null!)!;
                }

                var isNotExistErr = errors.Is(err, os.ErrNotExist);

                if (proxy.url == "direct" || (proxy.url == "noproxy" && err != errUseProxy))
                {
                    bestErr = error.As(err)!;
                    bestErrRank = directRank;
                }
                else if (bestErrRank <= proxyRank && !isNotExistErr)
                {
                    bestErr = error.As(err)!;
                    bestErrRank = proxyRank;
                }
                else if (bestErrRank == notExistRank)
                {
                    bestErr = error.As(err)!;
                }

                if (!proxy.fallBackOnError && !isNotExistErr)
                {
                    break;
                }

            }
            return error.As(bestErr)!;

        });

        private partial struct proxyRepo
        {
            public ptr<url.URL> url;
            public @string path;
        }

        private static (Repo, error) newProxyRepo(@string baseURL, @string path)
        {
            Repo _p0 = default;
            error _p0 = default!;

            var (base, err) = url.Parse(baseURL);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            switch (@base.Scheme)
            {
                case "http": 

                case "https": 
                    break;
                case "file": 
                    if (base != (new url.URL(Scheme:base.Scheme,Path:base.Path,RawPath:base.RawPath)).val)
                    {
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
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            @base.Path = strings.TrimSuffix(@base.Path, "/") + "/" + enc;
            @base.RawPath = strings.TrimSuffix(@base.RawPath, "/") + "/" + pathEscape(enc);
            return (addr(new proxyRepo(base,path)), error.As(null!)!);

        }

        private static @string ModulePath(this ptr<proxyRepo> _addr_p)
        {
            ref proxyRepo p = ref _addr_p.val;

            return p.path;
        }

        // versionError returns err wrapped in a ModuleError for p.path.
        private static error versionError(this ptr<proxyRepo> _addr_p, @string version, error err)
        {
            ref proxyRepo p = ref _addr_p.val;

            if (version != "" && version != module.CanonicalVersion(version))
            {
                return error.As(addr(new module.ModuleError(Path:p.path,Err:&module.InvalidVersionError{Version:version,Pseudo:IsPseudoVersion(version),Err:err,},))!)!;
            }

            return error.As(addr(new module.ModuleError(Path:p.path,Version:version,Err:err,))!)!;

        }

        private static (slice<byte>, error) getBytes(this ptr<proxyRepo> _addr_p, @string path) => func((defer, _, __) =>
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            var (body, err) = p.getBody(path);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(body.Close());
            return ioutil.ReadAll(body);

        });

        private static (io.ReadCloser, error) getBody(this ptr<proxyRepo> _addr_p, @string path)
        {
            io.ReadCloser _p0 = default;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            var fullPath = pathpkg.Join(p.url.Path, path);

            ref var target = ref heap(p.url.val, out ptr<var> _addr_target);
            target.Path = fullPath;
            target.RawPath = pathpkg.Join(target.RawPath, pathEscape(path));

            var (resp, err) = web.Get(web.DefaultSecurity, _addr_target);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            {
                var err = resp.Err();

                if (err != null)
                {
                    resp.Body.Close();
                    return (null, error.As(err)!);
                }

            }

            return (resp.Body, error.As(null!)!);

        }

        private static (slice<@string>, error) Versions(this ptr<proxyRepo> _addr_p, @string prefix)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            var (data, err) = p.getBytes("@v/list");
            if (err != null)
            {
                return (null, error.As(p.versionError("", err))!);
            }

            slice<@string> list = default;
            foreach (var (_, line) in strings.Split(string(data), "\n"))
            {
                var f = strings.Fields(line);
                if (len(f) >= 1L && semver.IsValid(f[0L]) && strings.HasPrefix(f[0L], prefix) && !IsPseudoVersion(f[0L]))
                {
                    list = append(list, f[0L]);
                }

            }
            SortVersions(list);
            return (list, error.As(null!)!);

        }

        private static (ptr<RevInfo>, error) latest(this ptr<proxyRepo> _addr_p)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            var (data, err) = p.getBytes("@v/list");
            if (err != null)
            {
                return (_addr_null!, error.As(p.versionError("", err))!);
            }

            time.Time bestTime = default;            bool bestTimeIsFromPseudo = default;            @string bestVersion = default;

            foreach (var (_, line) in strings.Split(string(data), "\n"))
            {
                var f = strings.Fields(line);
                if (len(f) >= 1L && semver.IsValid(f[0L]))
                { 
                    // If the proxy includes timestamps, prefer the timestamp it reports.
                    // Otherwise, derive the timestamp from the pseudo-version.
                    time.Time ft = default;                    var ftIsFromPseudo = false;
                    if (len(f) >= 2L)
                    {
                        ft, _ = time.Parse(time.RFC3339, f[1L]);
                    }
                    else if (IsPseudoVersion(f[0L]))
                    {
                        ft, _ = PseudoVersionTime(f[0L]);
                        ftIsFromPseudo = true;
                    }
                    else
                    { 
                        // Repo.Latest promises that this method is only called where there are
                        // no tagged versions. Ignore any tagged versions that were added in the
                        // meantime.
                        continue;

                    }

                    if (bestTime.Before(ft))
                    {
                        bestTime = ft;
                        bestTimeIsFromPseudo = ftIsFromPseudo;
                        bestVersion = f[0L];
                    }

                }

            }
            if (bestVersion == "")
            {
                return (_addr_null!, error.As(p.versionError("", codehost.ErrNoCommits))!);
            }

            if (bestTimeIsFromPseudo)
            { 
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

        private static (ptr<RevInfo>, error) Stat(this ptr<proxyRepo> _addr_p, @string rev)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            var (encRev, err) = module.EscapeVersion(rev);
            if (err != null)
            {
                return (_addr_null!, error.As(p.versionError(rev, err))!);
            }

            var (data, err) = p.getBytes("@v/" + encRev + ".info");
            if (err != null)
            {
                return (_addr_null!, error.As(p.versionError(rev, err))!);
            }

            ptr<RevInfo> info = @new<RevInfo>();
            {
                var err = json.Unmarshal(data, info);

                if (err != null)
                {
                    return (_addr_null!, error.As(p.versionError(rev, err))!);
                }

            }

            if (info.Version != rev && rev == module.CanonicalVersion(rev) && module.Check(p.path, rev) == null)
            { 
                // If we request a correct, appropriate version for the module path, the
                // proxy must return either exactly that version or an error — not some
                // arbitrary other version.
                return (_addr_null!, error.As(p.versionError(rev, fmt.Errorf("proxy returned info for version %s instead of requested version", info.Version)))!);

            }

            return (_addr_info!, error.As(null!)!);

        }

        private static (ptr<RevInfo>, error) Latest(this ptr<proxyRepo> _addr_p)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            var (data, err) = p.getBytes("@latest");
            if (err != null)
            {
                if (!errors.Is(err, os.ErrNotExist))
                {
                    return (_addr_null!, error.As(p.versionError("", err))!);
                }

                return _addr_p.latest()!;

            }

            ptr<RevInfo> info = @new<RevInfo>();
            {
                var err = json.Unmarshal(data, info);

                if (err != null)
                {
                    return (_addr_null!, error.As(p.versionError("", err))!);
                }

            }

            return (_addr_info!, error.As(null!)!);

        }

        private static (slice<byte>, error) GoMod(this ptr<proxyRepo> _addr_p, @string version)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref proxyRepo p = ref _addr_p.val;

            if (version != module.CanonicalVersion(version))
            {
                return (null, error.As(p.versionError(version, fmt.Errorf("internal error: version passed to GoMod is not canonical")))!);
            }

            var (encVer, err) = module.EscapeVersion(version);
            if (err != null)
            {
                return (null, error.As(p.versionError(version, err))!);
            }

            var (data, err) = p.getBytes("@v/" + encVer + ".mod");
            if (err != null)
            {
                return (null, error.As(p.versionError(version, err))!);
            }

            return (data, error.As(null!)!);

        }

        private static error Zip(this ptr<proxyRepo> _addr_p, io.Writer dst, @string version) => func((defer, _, __) =>
        {
            ref proxyRepo p = ref _addr_p.val;

            if (version != module.CanonicalVersion(version))
            {
                return error.As(p.versionError(version, fmt.Errorf("internal error: version passed to Zip is not canonical")))!;
            }

            var (encVer, err) = module.EscapeVersion(version);
            if (err != null)
            {
                return error.As(p.versionError(version, err))!;
            }

            var (body, err) = p.getBody("@v/" + encVer + ".zip");
            if (err != null)
            {
                return error.As(p.versionError(version, err))!;
            }

            defer(body.Close());

            ptr<io.LimitedReader> lr = addr(new io.LimitedReader(R:body,N:codehost.MaxZipFile+1));
            {
                var (_, err) = io.Copy(dst, lr);

                if (err != null)
                {
                    return error.As(p.versionError(version, err))!;
                }

            }

            if (lr.N <= 0L)
            {
                return error.As(p.versionError(version, fmt.Errorf("downloaded zip file too large")))!;
            }

            return error.As(null!)!;

        });

        // pathEscape escapes s so it can be used in a path.
        // That is, it escapes things like ? and # (which really shouldn't appear anyway).
        // It does not escape / to %2F: our REST API is designed so that / can be left as is.
        private static @string pathEscape(@string s)
        {
            return strings.ReplaceAll(url.PathEscape(s), "%2F", "/");
        }
    }
}}}}
