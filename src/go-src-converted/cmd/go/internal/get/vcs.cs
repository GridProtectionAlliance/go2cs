// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package get -- go2cs converted at 2020 October 08 04:36:56 UTC
// import "cmd/go/internal/get" ==> using get = go.cmd.go.@internal.get_package
// Original source: C:\Go\src\cmd\go\internal\get\vcs.go
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using lazyregexp = go.@internal.lazyregexp_package;
using singleflight = go.@internal.singleflight_package;
using log = go.log_package;
using urlpkg = go.net.url_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using web = go.cmd.go.@internal.web_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class get_package
    {
        // A vcsCmd describes how to use a version control system
        // like Mercurial, Git, or Subversion.
        private partial struct vcsCmd
        {
            public @string name;
            public @string cmd; // name of binary to invoke command

            public slice<@string> createCmd; // commands to download a fresh copy of a repository
            public slice<@string> downloadCmd; // commands to download updates into an existing repository

            public slice<tagCmd> tagCmd; // commands to list tags
            public slice<tagCmd> tagLookupCmd; // commands to lookup tags before running tagSyncCmd
            public slice<@string> tagSyncCmd; // commands to sync to specific tag
            public slice<@string> tagSyncDefault; // commands to sync to default tag

            public slice<@string> scheme;
            public @string pingCmd;
            public Func<ptr<vcsCmd>, @string, (@string, error)> remoteRepo;
            public Func<ptr<vcsCmd>, @string, @string, (@string, error)> resolveRepo;
        }

        private static map defaultSecureScheme = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"https":true,"git+ssh":true,"bzr+ssh":true,"svn+ssh":true,"ssh":true,};

        private static bool isSecure(this ptr<vcsCmd> _addr_v, @string repo)
        {
            ref vcsCmd v = ref _addr_v.val;

            var (u, err) = urlpkg.Parse(repo);
            if (err != null)
            { 
                // If repo is not a URL, it's not secure.
                return false;

            }

            return v.isSecureScheme(u.Scheme);

        }

        private static bool isSecureScheme(this ptr<vcsCmd> _addr_v, @string scheme)
        {
            ref vcsCmd v = ref _addr_v.val;

            switch (v.cmd)
            {
                case "git": 
                    // GIT_ALLOW_PROTOCOL is an environment variable defined by Git. It is a
                    // colon-separated list of schemes that are allowed to be used with git
                    // fetch/clone. Any scheme not mentioned will be considered insecure.
                    {
                        var allow = os.Getenv("GIT_ALLOW_PROTOCOL");

                        if (allow != "")
                        {
                            foreach (var (_, s) in strings.Split(allow, ":"))
                            {
                                if (s == scheme)
                                {
                                    return true;
                                }

                            }
                            return false;

                        }

                    }

                    break;
            }
            return defaultSecureScheme[scheme];

        }

        // A tagCmd describes a command to list available tags
        // that can be passed to tagSyncCmd.
        private partial struct tagCmd
        {
            public @string cmd; // command to list tags
            public @string pattern; // regexp to extract tags from list
        }

        // vcsList lists the known version control systems
        private static ptr<vcsCmd> vcsList = new slice<ptr<vcsCmd>>(new ptr<vcsCmd>[] { vcsHg, vcsGit, vcsSvn, vcsBzr, vcsFossil });

        // vcsByCmd returns the version control system for the given
        // command name (hg, git, svn, bzr).
        private static ptr<vcsCmd> vcsByCmd(@string cmd)
        {
            foreach (var (_, vcs) in vcsList)
            {
                if (vcs.cmd == cmd)
                {
                    return _addr_vcs!;
                }

            }
            return _addr_null!;

        }

        // vcsHg describes how to use Mercurial.
        private static ptr<vcsCmd> vcsHg = addr(new vcsCmd(name:"Mercurial",cmd:"hg",createCmd:[]string{"clone -U -- {repo} {dir}"},downloadCmd:[]string{"pull"},tagCmd:[]tagCmd{{"tags",`^(\S+)`},{"branches",`^(\S+)`},},tagSyncCmd:[]string{"update -r {tag}"},tagSyncDefault:[]string{"update default"},scheme:[]string{"https","http","ssh"},pingCmd:"identify -- {scheme}://{repo}",remoteRepo:hgRemoteRepo,));

        private static (@string, error) hgRemoteRepo(ptr<vcsCmd> _addr_vcsHg, @string rootDir)
        {
            @string remoteRepo = default;
            error err = default!;
            ref vcsCmd vcsHg = ref _addr_vcsHg.val;

            var (out, err) = vcsHg.runOutput(rootDir, "paths default");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (strings.TrimSpace(string(out)), error.As(null!)!);

        }

        // vcsGit describes how to use Git.
        private static ptr<vcsCmd> vcsGit = addr(new vcsCmd(name:"Git",cmd:"git",createCmd:[]string{"clone -- {repo} {dir}","-go-internal-cd {dir} submodule update --init --recursive"},downloadCmd:[]string{"pull --ff-only","submodule update --init --recursive"},tagCmd:[]tagCmd{{"show-ref",`(?:tags|origin)/(\S+)$`},},tagLookupCmd:[]tagCmd{{"show-ref tags/{tag} origin/{tag}",`((?:tags|origin)/\S+)$`},},tagSyncCmd:[]string{"checkout {tag}","submodule update --init --recursive"},tagSyncDefault:[]string{"submodule update --init --recursive"},scheme:[]string{"git","https","http","git+ssh","ssh"},pingCmd:"ls-remote {scheme}://{repo}",remoteRepo:gitRemoteRepo,));

        // scpSyntaxRe matches the SCP-like addresses used by Git to access
        // repositories by SSH.
        private static var scpSyntaxRe = lazyregexp.New("^([a-zA-Z0-9_]+)@([a-zA-Z0-9._-]+):(.*)$");

        private static (@string, error) gitRemoteRepo(ptr<vcsCmd> _addr_vcsGit, @string rootDir)
        {
            @string remoteRepo = default;
            error err = default!;
            ref vcsCmd vcsGit = ref _addr_vcsGit.val;

            @string cmd = "config remote.origin.url";
            var errParse = errors.New("unable to parse output of git " + cmd);
            var errRemoteOriginNotFound = errors.New("remote origin not found");
            var (outb, err) = vcsGit.run1(rootDir, cmd, null, false);
            if (err != null)
            { 
                // if it doesn't output any message, it means the config argument is correct,
                // but the config value itself doesn't exist
                if (outb != null && len(outb) == 0L)
                {
                    return ("", error.As(errRemoteOriginNotFound)!);
                }

                return ("", error.As(err)!);

            }

            var @out = strings.TrimSpace(string(outb));

            ptr<urlpkg.URL> repoURL;
            {
                var m = scpSyntaxRe.FindStringSubmatch(out);

                if (m != null)
                { 
                    // Match SCP-like syntax and convert it to a URL.
                    // Eg, "git@github.com:user/repo" becomes
                    // "ssh://git@github.com/user/repo".
                    repoURL = addr(new urlpkg.URL(Scheme:"ssh",User:urlpkg.User(m[1]),Host:m[2],Path:m[3],));

                }
                else
                {
                    repoURL, err = urlpkg.Parse(out);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                } 

                // Iterate over insecure schemes too, because this function simply
                // reports the state of the repo. If we can't see insecure schemes then
                // we can't report the actual repo URL.

            } 

            // Iterate over insecure schemes too, because this function simply
            // reports the state of the repo. If we can't see insecure schemes then
            // we can't report the actual repo URL.
            foreach (var (_, s) in vcsGit.scheme)
            {
                if (repoURL.Scheme == s)
                {
                    return (repoURL.String(), error.As(null!)!);
                }

            }
            return ("", error.As(errParse)!);

        }

        // vcsBzr describes how to use Bazaar.
        private static ptr<vcsCmd> vcsBzr = addr(new vcsCmd(name:"Bazaar",cmd:"bzr",createCmd:[]string{"branch -- {repo} {dir}"},downloadCmd:[]string{"pull --overwrite"},tagCmd:[]tagCmd{{"tags",`^(\S+)`}},tagSyncCmd:[]string{"update -r {tag}"},tagSyncDefault:[]string{"update -r revno:-1"},scheme:[]string{"https","http","bzr","bzr+ssh"},pingCmd:"info -- {scheme}://{repo}",remoteRepo:bzrRemoteRepo,resolveRepo:bzrResolveRepo,));

        private static (@string, error) bzrRemoteRepo(ptr<vcsCmd> _addr_vcsBzr, @string rootDir)
        {
            @string remoteRepo = default;
            error err = default!;
            ref vcsCmd vcsBzr = ref _addr_vcsBzr.val;

            var (outb, err) = vcsBzr.runOutput(rootDir, "config parent_location");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (strings.TrimSpace(string(outb)), error.As(null!)!);

        }

        private static (@string, error) bzrResolveRepo(ptr<vcsCmd> _addr_vcsBzr, @string rootDir, @string remoteRepo)
        {
            @string realRepo = default;
            error err = default!;
            ref vcsCmd vcsBzr = ref _addr_vcsBzr.val;

            var (outb, err) = vcsBzr.runOutput(rootDir, "info " + remoteRepo);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var @out = string(outb); 

            // Expect:
            // ...
            //   (branch root|repository branch): <URL>
            // ...

            var found = false;
            foreach (var (_, prefix) in new slice<@string>(new @string[] { "\n  branch root: ", "\n  repository branch: " }))
            {
                var i = strings.Index(out, prefix);
                if (i >= 0L)
                {
                    out = out[i + len(prefix)..];
                    found = true;
                    break;
                }

            }
            if (!found)
            {
                return ("", error.As(fmt.Errorf("unable to parse output of bzr info"))!);
            }

            i = strings.Index(out, "\n");
            if (i < 0L)
            {
                return ("", error.As(fmt.Errorf("unable to parse output of bzr info"))!);
            }

            out = out[..i];
            return (strings.TrimSpace(out), error.As(null!)!);

        }

        // vcsSvn describes how to use Subversion.
        private static ptr<vcsCmd> vcsSvn = addr(new vcsCmd(name:"Subversion",cmd:"svn",createCmd:[]string{"checkout -- {repo} {dir}"},downloadCmd:[]string{"update"},scheme:[]string{"https","http","svn","svn+ssh"},pingCmd:"info -- {scheme}://{repo}",remoteRepo:svnRemoteRepo,));

        private static (@string, error) svnRemoteRepo(ptr<vcsCmd> _addr_vcsSvn, @string rootDir)
        {
            @string remoteRepo = default;
            error err = default!;
            ref vcsCmd vcsSvn = ref _addr_vcsSvn.val;

            var (outb, err) = vcsSvn.runOutput(rootDir, "info");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var @out = string(outb); 

            // Expect:
            //
            //     ...
            //     URL: <URL>
            //     ...
            //
            // Note that we're not using the Repository Root line,
            // because svn allows checking out subtrees.
            // The URL will be the URL of the subtree (what we used with 'svn co')
            // while the Repository Root may be a much higher parent.
            var i = strings.Index(out, "\nURL: ");
            if (i < 0L)
            {
                return ("", error.As(fmt.Errorf("unable to parse output of svn info"))!);
            }

            out = out[i + len("\nURL: ")..];
            i = strings.Index(out, "\n");
            if (i < 0L)
            {
                return ("", error.As(fmt.Errorf("unable to parse output of svn info"))!);
            }

            out = out[..i];
            return (strings.TrimSpace(out), error.As(null!)!);

        }

        // fossilRepoName is the name go get associates with a fossil repository. In the
        // real world the file can be named anything.
        private static readonly @string fossilRepoName = (@string)".fossil";

        // vcsFossil describes how to use Fossil (fossil-scm.org)


        // vcsFossil describes how to use Fossil (fossil-scm.org)
        private static ptr<vcsCmd> vcsFossil = addr(new vcsCmd(name:"Fossil",cmd:"fossil",createCmd:[]string{"-go-internal-mkdir {dir} clone -- {repo} "+filepath.Join("{dir}",fossilRepoName),"-go-internal-cd {dir} open .fossil"},downloadCmd:[]string{"up"},tagCmd:[]tagCmd{{"tag ls",`(.*)`}},tagSyncCmd:[]string{"up tag:{tag}"},tagSyncDefault:[]string{"up trunk"},scheme:[]string{"https","http"},remoteRepo:fossilRemoteRepo,));

        private static (@string, error) fossilRemoteRepo(ptr<vcsCmd> _addr_vcsFossil, @string rootDir)
        {
            @string remoteRepo = default;
            error err = default!;
            ref vcsCmd vcsFossil = ref _addr_vcsFossil.val;

            var (out, err) = vcsFossil.runOutput(rootDir, "remote-url");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (strings.TrimSpace(string(out)), error.As(null!)!);

        }

        private static @string String(this ptr<vcsCmd> _addr_v)
        {
            ref vcsCmd v = ref _addr_v.val;

            return v.name;
        }

        // run runs the command line cmd in the given directory.
        // keyval is a list of key, value pairs. run expands
        // instances of {key} in cmd into value, but only after
        // splitting cmd into individual arguments.
        // If an error occurs, run prints the command line and the
        // command's combined stdout+stderr to standard error.
        // Otherwise run discards the command's output.
        private static error run(this ptr<vcsCmd> _addr_v, @string dir, @string cmd, params @string[] keyval)
        {
            keyval = keyval.Clone();
            ref vcsCmd v = ref _addr_v.val;

            var (_, err) = v.run1(dir, cmd, keyval, true);
            return error.As(err)!;
        }

        // runVerboseOnly is like run but only generates error output to standard error in verbose mode.
        private static error runVerboseOnly(this ptr<vcsCmd> _addr_v, @string dir, @string cmd, params @string[] keyval)
        {
            keyval = keyval.Clone();
            ref vcsCmd v = ref _addr_v.val;

            var (_, err) = v.run1(dir, cmd, keyval, false);
            return error.As(err)!;
        }

        // runOutput is like run but returns the output of the command.
        private static (slice<byte>, error) runOutput(this ptr<vcsCmd> _addr_v, @string dir, @string cmd, params @string[] keyval)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            keyval = keyval.Clone();
            ref vcsCmd v = ref _addr_v.val;

            return v.run1(dir, cmd, keyval, true);
        }

        // run1 is the generalized implementation of run and runOutput.
        private static (slice<byte>, error) run1(this ptr<vcsCmd> _addr_v, @string dir, @string cmdline, slice<@string> keyval, bool verbose)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref vcsCmd v = ref _addr_v.val;

            var m = make_map<@string, @string>();
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < len(keyval))
                {
                    m[keyval[i]] = keyval[i + 1L];
                    i += 2L;
                }


                i = i__prev1;
            }
            var args = strings.Fields(cmdline);
            {
                long i__prev1 = i;

                foreach (var (__i, __arg) in args)
                {
                    i = __i;
                    arg = __arg;
                    args[i] = expand(m, arg);
                }

                i = i__prev1;
            }

            if (len(args) >= 2L && args[0L] == "-go-internal-mkdir")
            {
                error err = default!;
                if (filepath.IsAbs(args[1L]))
                {
                    err = error.As(os.Mkdir(args[1L], os.ModePerm))!;
                }
                else
                {
                    err = error.As(os.Mkdir(filepath.Join(dir, args[1L]), os.ModePerm))!;
                }

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                args = args[2L..];

            }

            if (len(args) >= 2L && args[0L] == "-go-internal-cd")
            {
                if (filepath.IsAbs(args[1L]))
                {
                    dir = args[1L];
                }
                else
                {
                    dir = filepath.Join(dir, args[1L]);
                }

                args = args[2L..];

            }

            var (_, err) = exec.LookPath(v.cmd);
            if (err != null)
            {
                fmt.Fprintf(os.Stderr, "go: missing %s command. See https://golang.org/s/gogetcmd\n", v.name);
                return (null, error.As(err)!);
            }

            var cmd = exec.Command(v.cmd, args);
            cmd.Dir = dir;
            cmd.Env = @base.AppendPWD(os.Environ(), cmd.Dir);
            if (cfg.BuildX)
            {
                fmt.Fprintf(os.Stderr, "cd %s\n", dir);
                fmt.Fprintf(os.Stderr, "%s %s\n", v.cmd, strings.Join(args, " "));
            }

            var (out, err) = cmd.Output();
            if (err != null)
            {
                if (verbose || cfg.BuildV)
                {
                    fmt.Fprintf(os.Stderr, "# cd %s; %s %s\n", dir, v.cmd, strings.Join(args, " "));
                    {
                        ptr<exec.ExitError> (ee, ok) = err._<ptr<exec.ExitError>>();

                        if (ok && len(ee.Stderr) > 0L)
                        {
                            os.Stderr.Write(ee.Stderr);
                        }
                        else
                        {
                            fmt.Fprintf(os.Stderr, err.Error());
                        }

                    }

                }

            }

            return (out, error.As(err)!);

        }

        // ping pings to determine scheme to use.
        private static error ping(this ptr<vcsCmd> _addr_v, @string scheme, @string repo)
        {
            ref vcsCmd v = ref _addr_v.val;

            return error.As(v.runVerboseOnly(".", v.pingCmd, "scheme", scheme, "repo", repo))!;
        }

        // create creates a new copy of repo in dir.
        // The parent of dir must exist; dir must not.
        private static error create(this ptr<vcsCmd> _addr_v, @string dir, @string repo)
        {
            ref vcsCmd v = ref _addr_v.val;

            foreach (var (_, cmd) in v.createCmd)
            {
                {
                    var err = v.run(".", cmd, "dir", dir, "repo", repo);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            }
            return error.As(null!)!;

        }

        // download downloads any new changes for the repo in dir.
        private static error download(this ptr<vcsCmd> _addr_v, @string dir)
        {
            ref vcsCmd v = ref _addr_v.val;

            foreach (var (_, cmd) in v.downloadCmd)
            {
                {
                    var err = v.run(dir, cmd);

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }

            }
            return error.As(null!)!;

        }

        // tags returns the list of available tags for the repo in dir.
        private static (slice<@string>, error) tags(this ptr<vcsCmd> _addr_v, @string dir)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref vcsCmd v = ref _addr_v.val;

            slice<@string> tags = default;
            foreach (var (_, tc) in v.tagCmd)
            {
                var (out, err) = v.runOutput(dir, tc.cmd);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                var re = regexp.MustCompile("(?m-s)" + tc.pattern);
                foreach (var (_, m) in re.FindAllStringSubmatch(string(out), -1L))
                {
                    tags = append(tags, m[1L]);
                }

            }
            return (tags, error.As(null!)!);

        }

        // tagSync syncs the repo in dir to the named tag,
        // which either is a tag returned by tags or is v.tagDefault.
        private static error tagSync(this ptr<vcsCmd> _addr_v, @string dir, @string tag)
        {
            ref vcsCmd v = ref _addr_v.val;

            if (v.tagSyncCmd == null)
            {
                return error.As(null!)!;
            }

            if (tag != "")
            {
                foreach (var (_, tc) in v.tagLookupCmd)
                {
                    var (out, err) = v.runOutput(dir, tc.cmd, "tag", tag);
                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                    var re = regexp.MustCompile("(?m-s)" + tc.pattern);
                    var m = re.FindStringSubmatch(string(out));
                    if (len(m) > 1L)
                    {
                        tag = m[1L];
                        break;
                    }

                }

            }

            if (tag == "" && v.tagSyncDefault != null)
            {
                {
                    var cmd__prev1 = cmd;

                    foreach (var (_, __cmd) in v.tagSyncDefault)
                    {
                        cmd = __cmd;
                        {
                            var err__prev2 = err;

                            var err = v.run(dir, cmd);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                            err = err__prev2;

                        }

                    }

                    cmd = cmd__prev1;
                }

                return error.As(null!)!;

            }

            {
                var cmd__prev1 = cmd;

                foreach (var (_, __cmd) in v.tagSyncCmd)
                {
                    cmd = __cmd;
                    {
                        var err__prev1 = err;

                        err = v.run(dir, cmd, "tag", tag);

                        if (err != null)
                        {
                            return error.As(err)!;
                        }

                        err = err__prev1;

                    }

                }

                cmd = cmd__prev1;
            }

            return error.As(null!)!;

        }

        // A vcsPath describes how to convert an import path into a
        // version control system and repository name.
        private partial struct vcsPath
        {
            public @string prefix; // prefix this description applies to
            public ptr<lazyregexp.Regexp> regexp; // compiled pattern for import path
            public @string repo; // repository to use (expand with match of re)
            public @string vcs; // version control system to use (expand with match of re)
            public Func<map<@string, @string>, error> check; // additional checks
            public bool schemelessRepo; // if true, the repo pattern lacks a scheme
        }

        // vcsFromDir inspects dir and its parents to determine the
        // version control system and code repository to use.
        // On return, root is the import path
        // corresponding to the root of the repository.
        private static (ptr<vcsCmd>, @string, error) vcsFromDir(@string dir, @string srcRoot)
        {
            ptr<vcsCmd> vcs = default!;
            @string root = default;
            error err = default!;
 
            // Clean and double-check that dir is in (a subdirectory of) srcRoot.
            dir = filepath.Clean(dir);
            srcRoot = filepath.Clean(srcRoot);
            if (len(dir) <= len(srcRoot) || dir[len(srcRoot)] != filepath.Separator)
            {
                return (_addr_null!, "", error.As(fmt.Errorf("directory %q is outside source root %q", dir, srcRoot))!);
            }

            ptr<vcsCmd> vcsRet;
            @string rootRet = default;

            var origDir = dir;
            while (len(dir) > len(srcRoot))
            {
                foreach (var (_, vcs) in vcsList)
                {
                    {
                        var (_, err) = os.Stat(filepath.Join(dir, "." + vcs.cmd));

                        if (err == null)
                        {
                            var root = filepath.ToSlash(dir[len(srcRoot) + 1L..]); 
                            // Record first VCS we find, but keep looking,
                            // to detect mistakes like one kind of VCS inside another.
                            if (vcsRet == null)
                            {
                                vcsRet = vcs;
                                rootRet = root;
                                continue;
                            } 
                            // Allow .git inside .git, which can arise due to submodules.
                            if (vcsRet == vcs && vcs.cmd == "git")
                            {
                                continue;
                            } 
                            // Otherwise, we have one VCS inside a different VCS.
                            return (_addr_null!, "", error.As(fmt.Errorf("directory %q uses %s, but parent %q uses %s", filepath.Join(srcRoot, rootRet), vcsRet.cmd, filepath.Join(srcRoot, root), vcs.cmd))!);

                        }

                    }

                } 

                // Move to parent.
                var ndir = filepath.Dir(dir);
                if (len(ndir) >= len(dir))
                { 
                    // Shouldn't happen, but just in case, stop.
                    break;

                }

                dir = ndir;

            }


            if (vcsRet != null)
            {
                return (_addr_vcsRet!, rootRet, error.As(null!)!);
            }

            return (_addr_null!, "", error.As(fmt.Errorf("directory %q is not using a known version control system", origDir))!);

        }

        // checkNestedVCS checks for an incorrectly-nested VCS-inside-VCS
        // situation for dir, checking parents up until srcRoot.
        private static error checkNestedVCS(ptr<vcsCmd> _addr_vcs, @string dir, @string srcRoot)
        {
            ref vcsCmd vcs = ref _addr_vcs.val;

            if (len(dir) <= len(srcRoot) || dir[len(srcRoot)] != filepath.Separator)
            {
                return error.As(fmt.Errorf("directory %q is outside source root %q", dir, srcRoot))!;
            }

            var otherDir = dir;
            while (len(otherDir) > len(srcRoot))
            {
                foreach (var (_, otherVCS) in vcsList)
                {
                    {
                        var (_, err) = os.Stat(filepath.Join(otherDir, "." + otherVCS.cmd));

                        if (err == null)
                        { 
                            // Allow expected vcs in original dir.
                            if (otherDir == dir && otherVCS == vcs)
                            {
                                continue;
                            } 
                            // Allow .git inside .git, which can arise due to submodules.
                            if (otherVCS == vcs && vcs.cmd == "git")
                            {
                                continue;
                            } 
                            // Otherwise, we have one VCS inside a different VCS.
                            return error.As(fmt.Errorf("directory %q uses %s, but parent %q uses %s", dir, vcs.cmd, otherDir, otherVCS.cmd))!;

                        }

                    }

                } 
                // Move to parent.
                var newDir = filepath.Dir(otherDir);
                if (len(newDir) >= len(otherDir))
                { 
                    // Shouldn't happen, but just in case, stop.
                    break;

                }

                otherDir = newDir;

            }


            return error.As(null!)!;

        }

        // RepoRoot describes the repository root for a tree of source code.
        public partial struct RepoRoot
        {
            public @string Repo; // repository URL, including scheme
            public @string Root; // import path corresponding to root of repo
            public bool IsCustom; // defined by served <meta> tags (as opposed to hard-coded pattern)
            public @string VCS; // vcs type ("mod", "git", ...)

            public ptr<vcsCmd> vcs; // internal: vcs command access
        }

        private static @string httpPrefix(@string s)
        {
            foreach (var (_, prefix) in new array<@string>(new @string[] { "http:", "https:" }))
            {
                if (strings.HasPrefix(s, prefix))
                {
                    return prefix;
                }

            }
            return "";

        }

        // ModuleMode specifies whether to prefer modules when looking up code sources.
        public partial struct ModuleMode // : long
        {
        }

        public static readonly ModuleMode IgnoreMod = (ModuleMode)iota;
        public static readonly var PreferMod = (var)0;


        // RepoRootForImportPath analyzes importPath to determine the
        // version control system, and code repository to use.
        public static (ptr<RepoRoot>, error) RepoRootForImportPath(@string importPath, ModuleMode mod, web.SecurityMode security)
        {
            ptr<RepoRoot> _p0 = default!;
            error _p0 = default!;

            var (rr, err) = repoRootFromVCSPaths(importPath, security, vcsPaths);
            if (err == errUnknownSite)
            {
                rr, err = repoRootForImportDynamic(importPath, mod, security);
                if (err != null)
                {
                    err = load.ImportErrorf(importPath, "unrecognized import path %q: %v", importPath, err);
                }

            }

            if (err != null)
            {
                var (rr1, err1) = repoRootFromVCSPaths(importPath, security, vcsPathsAfterDynamic);
                if (err1 == null)
                {
                    rr = rr1;
                    err = null;
                }

            } 

            // Should have been taken care of above, but make sure.
            if (err == null && strings.Contains(importPath, "...") && strings.Contains(rr.Root, "..."))
            { 
                // Do not allow wildcards in the repo root.
                rr = null;
                err = load.ImportErrorf(importPath, "cannot expand ... in %q", importPath);

            }

            return (_addr_rr!, error.As(err)!);

        }

        private static var errUnknownSite = errors.New("dynamic lookup required to find mapping");

        // repoRootFromVCSPaths attempts to map importPath to a repoRoot
        // using the mappings defined in vcsPaths.
        private static (ptr<RepoRoot>, error) repoRootFromVCSPaths(@string importPath, web.SecurityMode security, slice<ptr<vcsPath>> vcsPaths)
        {
            ptr<RepoRoot> _p0 = default!;
            error _p0 = default!;
 
            // A common error is to use https://packagepath because that's what
            // hg and git require. Diagnose this helpfully.
            {
                var prefix = httpPrefix(importPath);

                if (prefix != "")
                { 
                    // The importPath has been cleaned, so has only one slash. The pattern
                    // ignores the slashes; the error message puts them back on the RHS at least.
                    return (_addr_null!, error.As(fmt.Errorf("%q not allowed in import path", prefix + "//"))!);

                }

            }

            foreach (var (_, srv) in vcsPaths)
            {
                if (!strings.HasPrefix(importPath, srv.prefix))
                {
                    continue;
                }

                var m = srv.regexp.FindStringSubmatch(importPath);
                if (m == null)
                {
                    if (srv.prefix != "")
                    {
                        return (_addr_null!, error.As(load.ImportErrorf(importPath, "invalid %s import path %q", srv.prefix, importPath))!);
                    }

                    continue;

                } 

                // Build map of named subexpression matches for expand.
                map match = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"prefix":srv.prefix,"import":importPath,};
                foreach (var (i, name) in srv.regexp.SubexpNames())
                {
                    if (name != "" && match[name] == "")
                    {
                        match[name] = m[i];
                    }

                }
                if (srv.vcs != "")
                {
                    match["vcs"] = expand(match, srv.vcs);
                }

                if (srv.repo != "")
                {
                    match["repo"] = expand(match, srv.repo);
                }

                if (srv.check != null)
                {
                    {
                        var err = srv.check(match);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                    }

                }

                var vcs = vcsByCmd(match["vcs"]);
                if (vcs == null)
                {
                    return (_addr_null!, error.As(fmt.Errorf("unknown version control system %q", match["vcs"]))!);
                }

                @string repoURL = default;
                if (!srv.schemelessRepo)
                {
                    repoURL = match["repo"];
                }
                else
                {
                    var scheme = vcs.scheme[0L]; // default to first scheme
                    var repo = match["repo"];
                    if (vcs.pingCmd != "")
                    { 
                        // If we know how to test schemes, scan to find one.
                        foreach (var (_, s) in vcs.scheme)
                        {
                            if (security == web.SecureOnly && !vcs.isSecureScheme(s))
                            {
                                continue;
                            }

                            if (vcs.ping(s, repo) == null)
                            {
                                scheme = s;
                                break;
                            }

                        }

                    }

                    repoURL = scheme + "://" + repo;

                }

                ptr<RepoRoot> rr = addr(new RepoRoot(Repo:repoURL,Root:match["root"],VCS:vcs.cmd,vcs:vcs,));
                return (_addr_rr!, error.As(null!)!);

            }
            return (_addr_null!, error.As(errUnknownSite)!);

        }

        // urlForImportPath returns a partially-populated URL for the given Go import path.
        //
        // The URL leaves the Scheme field blank so that web.Get will try any scheme
        // allowed by the selected security mode.
        private static (ptr<urlpkg.URL>, error) urlForImportPath(@string importPath)
        {
            ptr<urlpkg.URL> _p0 = default!;
            error _p0 = default!;

            var slash = strings.Index(importPath, "/");
            if (slash < 0L)
            {
                slash = len(importPath);
            }

            var host = importPath[..slash];
            var path = importPath[slash..];
            if (!strings.Contains(host, "."))
            {
                return (_addr_null!, error.As(errors.New("import path does not begin with hostname"))!);
            }

            if (len(path) == 0L)
            {
                path = "/";
            }

            return (addr(new urlpkg.URL(Host:host,Path:path,RawQuery:"go-get=1")), error.As(null!)!);

        }

        // repoRootForImportDynamic finds a *RepoRoot for a custom domain that's not
        // statically known by repoRootForImportPathStatic.
        //
        // This handles custom import paths like "name.tld/pkg/foo" or just "name.tld".
        private static (ptr<RepoRoot>, error) repoRootForImportDynamic(@string importPath, ModuleMode mod, web.SecurityMode security) => func((defer, _, __) =>
        {
            ptr<RepoRoot> _p0 = default!;
            error _p0 = default!;

            var (url, err) = urlForImportPath(importPath);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var (resp, err) = web.Get(security, url);
            if (err != null)
            {
                @string msg = "https fetch: %v";
                if (security == web.Insecure)
                {
                    msg = "http/" + msg;
                }

                return (_addr_null!, error.As(fmt.Errorf(msg, err))!);

            }

            var body = resp.Body;
            defer(body.Close());
            var (imports, err) = parseMetaGoImports(body, mod);
            if (len(imports) == 0L)
            {
                {
                    var respErr = resp.Err();

                    if (respErr != null)
                    { 
                        // If the server's status was not OK, prefer to report that instead of
                        // an XML parse error.
                        return (_addr_null!, error.As(respErr)!);

                    }

                }

            }

            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("parsing %s: %v", importPath, err))!);
            } 
            // Find the matched meta import.
            var (mmi, err) = matchGoImport(imports, importPath);
            if (err != null)
            {
                {
                    ImportMismatchError (_, ok) = err._<ImportMismatchError>();

                    if (!ok)
                    {
                        return (_addr_null!, error.As(fmt.Errorf("parse %s: %v", url, err))!);
                    }

                }

                return (_addr_null!, error.As(fmt.Errorf("parse %s: no go-import meta tags (%s)", resp.URL, err))!);

            }

            if (cfg.BuildV)
            {
                log.Printf("get %q: found meta tag %#v at %s", importPath, mmi, url);
            } 
            // If the import was "uni.edu/bob/project", which said the
            // prefix was "uni.edu" and the RepoRoot was "evilroot.com",
            // make sure we don't trust Bob and check out evilroot.com to
            // "uni.edu" yet (possibly overwriting/preempting another
            // non-evil student). Instead, first verify the root and see
            // if it matches Bob's claim.
            if (mmi.Prefix != importPath)
            {
                if (cfg.BuildV)
                {
                    log.Printf("get %q: verifying non-authoritative meta tag", importPath);
                }

                slice<metaImport> imports = default;
                url, imports, err = metaImportsForPrefix(mmi.Prefix, mod, security);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

                var (metaImport2, err) = matchGoImport(imports, importPath);
                if (err != null || mmi != metaImport2)
                {
                    return (_addr_null!, error.As(fmt.Errorf("%s and %s disagree about go-import for %s", resp.URL, url, mmi.Prefix))!);
                }

            }

            {
                var err = validateRepoRoot(mmi.RepoRoot);

                if (err != null)
                {
                    return (_addr_null!, error.As(fmt.Errorf("%s: invalid repo root %q: %v", resp.URL, mmi.RepoRoot, err))!);
                }

            }

            var vcs = vcsByCmd(mmi.VCS);
            if (vcs == null && mmi.VCS != "mod")
            {
                return (_addr_null!, error.As(fmt.Errorf("%s: unknown vcs %q", resp.URL, mmi.VCS))!);
            }

            ptr<RepoRoot> rr = addr(new RepoRoot(Repo:mmi.RepoRoot,Root:mmi.Prefix,IsCustom:true,VCS:mmi.VCS,vcs:vcs,));
            return (_addr_rr!, error.As(null!)!);

        });

        // validateRepoRoot returns an error if repoRoot does not seem to be
        // a valid URL with scheme.
        private static error validateRepoRoot(@string repoRoot)
        {
            var (url, err) = urlpkg.Parse(repoRoot);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (url.Scheme == "")
            {
                return error.As(errors.New("no scheme"))!;
            }

            if (url.Scheme == "file")
            {
                return error.As(errors.New("file scheme disallowed"))!;
            }

            return error.As(null!)!;

        }

        private static singleflight.Group fetchGroup = default;
        private static sync.Mutex fetchCacheMu = default;        private static map fetchCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, fetchResult>{};

        // metaImportsForPrefix takes a package's root import path as declared in a <meta> tag
        // and returns its HTML discovery URL and the parsed metaImport lines
        // found on the page.
        //
        // The importPath is of the form "golang.org/x/tools".
        // It is an error if no imports are found.
        // url will still be valid if err != nil.
        // The returned url will be of the form "https://golang.org/x/tools?go-get=1"
        private static (ptr<urlpkg.URL>, slice<metaImport>, error) metaImportsForPrefix(@string importPrefix, ModuleMode mod, web.SecurityMode security) => func((defer, _, __) =>
        {
            ptr<urlpkg.URL> _p0 = default!;
            slice<metaImport> _p0 = default;
            error _p0 = default!;

            Func<fetchResult, (fetchResult, error)> setCache = res =>
            {
                fetchCacheMu.Lock();
                defer(fetchCacheMu.Unlock());
                fetchCache[importPrefix] = res;
                return (_addr_res!, null);
            }
;

            var (resi, _, _) = fetchGroup.Do(importPrefix, () =>
            {
                fetchCacheMu.Lock();
                {
                    var res__prev1 = res;

                    var (res, ok) = fetchCache[importPrefix];

                    if (ok)
                    {
                        fetchCacheMu.Unlock();
                        return (_addr_res!, null);
                    }

                    res = res__prev1;

                }

                fetchCacheMu.Unlock();

                var (url, err) = urlForImportPath(importPrefix);
                if (err != null)
                {
                    return _addr_setCache(new fetchResult(err:err))!;
                }

                var (resp, err) = web.Get(security, url);
                if (err != null)
                {
                    return _addr_setCache(new fetchResult(url:url,err:fmt.Errorf("fetching %s: %v",importPrefix,err)))!;
                }

                var body = resp.Body;
                defer(body.Close());
                var (imports, err) = parseMetaGoImports(body, mod);
                if (len(imports) == 0L)
                {
                    {
                        var respErr = resp.Err();

                        if (respErr != null)
                        { 
                            // If the server's status was not OK, prefer to report that instead of
                            // an XML parse error.
                            return _addr_setCache(new fetchResult(url:url,err:respErr))!;

                        }

                    }

                }

                if (err != null)
                {
                    return _addr_setCache(new fetchResult(url:url,err:fmt.Errorf("parsing %s: %v",resp.URL,err)))!;
                }

                if (len(imports) == 0L)
                {
                    err = fmt.Errorf("fetching %s: no go-import meta tag found in %s", importPrefix, resp.URL);
                }

                return _addr_setCache(new fetchResult(url:url,imports:imports,err:err))!;

            });
            fetchResult res = resi._<fetchResult>();
            return (_addr_res.url!, res.imports, error.As(res.err)!);

        });

        private partial struct fetchResult
        {
            public ptr<urlpkg.URL> url;
            public slice<metaImport> imports;
            public error err;
        }

        // metaImport represents the parsed <meta name="go-import"
        // content="prefix vcs reporoot" /> tags from HTML files.
        private partial struct metaImport
        {
            public @string Prefix;
            public @string VCS;
            public @string RepoRoot;
        }

        // pathPrefix reports whether sub is a prefix of s,
        // only considering entire path components.
        private static bool pathPrefix(@string s, @string sub)
        { 
            // strings.HasPrefix is necessary but not sufficient.
            if (!strings.HasPrefix(s, sub))
            {
                return false;
            } 
            // The remainder after the prefix must either be empty or start with a slash.
            var rem = s[len(sub)..];
            return rem == "" || rem[0L] == '/';

        }

        // A ImportMismatchError is returned where metaImport/s are present
        // but none match our import path.
        public partial struct ImportMismatchError
        {
            public @string importPath;
            public slice<@string> mismatches; // the meta imports that were discarded for not matching our importPath
        }

        public static @string Error(this ImportMismatchError m)
        {
            var formattedStrings = make_slice<@string>(len(m.mismatches));
            foreach (var (i, pre) in m.mismatches)
            {
                formattedStrings[i] = fmt.Sprintf("meta tag %s did not match import path %s", pre, m.importPath);
            }
            return strings.Join(formattedStrings, ", ");

        }

        // matchGoImport returns the metaImport from imports matching importPath.
        // An error is returned if there are multiple matches.
        // An ImportMismatchError is returned if none match.
        private static (metaImport, error) matchGoImport(slice<metaImport> imports, @string importPath)
        {
            metaImport _p0 = default;
            error _p0 = default!;

            long match = -1L;

            ImportMismatchError errImportMismatch = new ImportMismatchError(importPath:importPath);
            foreach (var (i, im) in imports)
            {
                if (!pathPrefix(importPath, im.Prefix))
                {
                    errImportMismatch.mismatches = append(errImportMismatch.mismatches, im.Prefix);
                    continue;
                }

                if (match >= 0L)
                {
                    if (imports[match].VCS == "mod" && im.VCS != "mod")
                    { 
                        // All the mod entries precede all the non-mod entries.
                        // We have a mod entry and don't care about the rest,
                        // matching or not.
                        break;

                    }

                    return (new metaImport(), error.As(fmt.Errorf("multiple meta tags match import path %q", importPath))!);

                }

                match = i;

            }
            if (match == -1L)
            {
                return (new metaImport(), error.As(errImportMismatch)!);
            }

            return (imports[match], error.As(null!)!);

        }

        // expand rewrites s to replace {k} with match[k] for each key k in match.
        private static @string expand(map<@string, @string> match, @string s)
        { 
            // We want to replace each match exactly once, and the result of expansion
            // must not depend on the iteration order through the map.
            // A strings.Replacer has exactly the properties we're looking for.
            var oldNew = make_slice<@string>(0L, 2L * len(match));
            foreach (var (k, v) in match)
            {
                oldNew = append(oldNew, "{" + k + "}", v);
            }
            return strings.NewReplacer(oldNew).Replace(s);

        }

        // vcsPaths defines the meaning of import paths referring to
        // commonly-used VCS hosting sites (github.com/user/dir)
        // and import paths referring to a fully-qualified importPath
        // containing a VCS type (foo.com/repo.git/dir)
        private static ptr<vcsPath> vcsPaths = new slice<ptr<vcsPath>>(new ptr<vcsPath>[] { {prefix:"github.com/",regexp:lazyregexp.New(`^(?P<root>github\.com/[A-Za-z0-9_.\-]+/[A-Za-z0-9_.\-]+)(/[\p{L}0-9_.\-]+)*$`),vcs:"git",repo:"https://{root}",check:noVCSSuffix,}, {prefix:"bitbucket.org/",regexp:lazyregexp.New(`^(?P<root>bitbucket\.org/(?P<bitname>[A-Za-z0-9_.\-]+/[A-Za-z0-9_.\-]+))(/[A-Za-z0-9_.\-]+)*$`),repo:"https://{root}",check:bitbucketVCS,}, {prefix:"hub.jazz.net/git/",regexp:lazyregexp.New(`^(?P<root>hub\.jazz\.net/git/[a-z0-9]+/[A-Za-z0-9_.\-]+)(/[A-Za-z0-9_.\-]+)*$`),vcs:"git",repo:"https://{root}",check:noVCSSuffix,}, {prefix:"git.apache.org/",regexp:lazyregexp.New(`^(?P<root>git\.apache\.org/[a-z0-9_.\-]+\.git)(/[A-Za-z0-9_.\-]+)*$`),vcs:"git",repo:"https://{root}",}, {prefix:"git.openstack.org/",regexp:lazyregexp.New(`^(?P<root>git\.openstack\.org/[A-Za-z0-9_.\-]+/[A-Za-z0-9_.\-]+)(\.git)?(/[A-Za-z0-9_.\-]+)*$`),vcs:"git",repo:"https://{root}",}, {prefix:"chiselapp.com/",regexp:lazyregexp.New(`^(?P<root>chiselapp\.com/user/[A-Za-z0-9]+/repository/[A-Za-z0-9_.\-]+)$`),vcs:"fossil",repo:"https://{root}",}, {regexp:lazyregexp.New(`(?P<root>(?P<repo>([a-z0-9.\-]+\.)+[a-z0-9.\-]+(:[0-9]+)?(/~?[A-Za-z0-9_.\-]+)+?)\.(?P<vcs>bzr|fossil|git|hg|svn))(/~?[A-Za-z0-9_.\-]+)*$`),schemelessRepo:true,} });

        // vcsPathsAfterDynamic gives additional vcsPaths entries
        // to try after the dynamic HTML check.
        // This gives those sites a chance to introduce <meta> tags
        // as part of a graceful transition away from the hard-coded logic.
        private static ptr<vcsPath> vcsPathsAfterDynamic = new slice<ptr<vcsPath>>(new ptr<vcsPath>[] { {prefix:"launchpad.net/",regexp:lazyregexp.New(`^(?P<root>launchpad\.net/((?P<project>[A-Za-z0-9_.\-]+)(?P<series>/[A-Za-z0-9_.\-]+)?|~[A-Za-z0-9_.\-]+/(\+junk|[A-Za-z0-9_.\-]+)/[A-Za-z0-9_.\-]+))(/[A-Za-z0-9_.\-]+)*$`),vcs:"bzr",repo:"https://{root}",check:launchpadVCS,} });

        // noVCSSuffix checks that the repository name does not
        // end in .foo for any version control system foo.
        // The usual culprit is ".git".
        private static error noVCSSuffix(map<@string, @string> match)
        {
            var repo = match["repo"];
            foreach (var (_, vcs) in vcsList)
            {
                if (strings.HasSuffix(repo, "." + vcs.cmd))
                {
                    return error.As(fmt.Errorf("invalid version control suffix in %s path", match["prefix"]))!;
                }

            }
            return error.As(null!)!;

        }

        // bitbucketVCS determines the version control system for a
        // Bitbucket repository, by using the Bitbucket API.
        private static error bitbucketVCS(map<@string, @string> match)
        {
            {
                var err__prev1 = err;

                var err = noVCSSuffix(match);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            ref var resp = ref heap(out ptr<var> _addr_resp);
            ptr<urlpkg.URL> url = addr(new urlpkg.URL(Scheme:"https",Host:"api.bitbucket.org",Path:expand(match,"/2.0/repositories/{bitname}"),RawQuery:"fields=scm",));
            var (data, err) = web.GetBytes(url);
            if (err != null)
            {
                {
                    ptr<web.HTTPError> (httpErr, ok) = err._<ptr<web.HTTPError>>();

                    if (ok && httpErr.StatusCode == 403L)
                    { 
                        // this may be a private repository. If so, attempt to determine which
                        // VCS it uses. See issue 5375.
                        var root = match["root"];
                        foreach (var (_, vcs) in new slice<@string>(new @string[] { "git", "hg" }))
                        {
                            if (vcsByCmd(vcs).ping("https", root) == null)
                            {
                                resp.SCM = vcs;
                                break;
                            }

                        }

                    }
            else


                }


                if (resp.SCM == "")
                {
                    return error.As(err)!;
                }

            }            {
                {
                    var err__prev2 = err;

                    err = json.Unmarshal(data, _addr_resp);

                    if (err != null)
                    {
                        return error.As(fmt.Errorf("decoding %s: %v", url, err))!;
                    }

                    err = err__prev2;

                }

            }

            if (vcsByCmd(resp.SCM) != null)
            {
                match["vcs"] = resp.SCM;
                if (resp.SCM == "git")
                {
                    match["repo"] += ".git";
                }

                return error.As(null!)!;

            }

            return error.As(fmt.Errorf("unable to detect version control system for bitbucket.org/ path"))!;

        }

        // launchpadVCS solves the ambiguity for "lp.net/project/foo". In this case,
        // "foo" could be a series name registered in Launchpad with its own branch,
        // and it could also be the name of a directory within the main project
        // branch one level up.
        private static error launchpadVCS(map<@string, @string> match)
        {
            if (match["project"] == "" || match["series"] == "")
            {
                return error.As(null!)!;
            }

            ptr<urlpkg.URL> url = addr(new urlpkg.URL(Scheme:"https",Host:"code.launchpad.net",Path:expand(match,"/{project}{series}/.bzr/branch-format"),));
            var (_, err) = web.GetBytes(url);
            if (err != null)
            {
                match["root"] = expand(match, "launchpad.net/{project}");
                match["repo"] = expand(match, "https://{root}");
            }

            return error.As(null!)!;

        }
    }
}}}}
