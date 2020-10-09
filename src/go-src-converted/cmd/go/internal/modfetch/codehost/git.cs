// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package codehost -- go2cs converted at 2020 October 09 05:47:07 UTC
// import "cmd/go/internal/modfetch/codehost" ==> using codehost = go.cmd.go.@internal.modfetch.codehost_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\codehost\git.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using url = go.net.url_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using lockedfile = go.cmd.go.@internal.lockedfile_package;
using par = go.cmd.go.@internal.par_package;
using web = go.cmd.go.@internal.web_package;

using semver = go.golang.org.x.mod.semver_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal {
namespace modfetch
{
    public static partial class codehost_package
    {
        // LocalGitRepo is like Repo but accepts both Git remote references
        // and paths to repositories on the local file system.
        public static (Repo, error) LocalGitRepo(@string remote)
        {
            Repo _p0 = default;
            error _p0 = default!;

            return newGitRepoCached(remote, true);
        }

        // A notExistError wraps another error to retain its original text
        // but makes it opaquely equivalent to os.ErrNotExist.
        private partial struct notExistError
        {
            public error err;
        }

        private static @string Error(this notExistError e)
        {
            return e.err.Error();
        }
        private static bool Is(this notExistError _p0, error err)
        {
            return err == os.ErrNotExist;
        }

        private static readonly @string gitWorkDirType = (@string)"git3";



        private static par.Cache gitRepoCache = default;

        private static (Repo, error) newGitRepoCached(@string remote, bool localOK)
        {
            Repo _p0 = default;
            error _p0 = default!;

            private partial struct key
            {
                public @string remote;
                public bool localOK;
            }
            private partial struct cached
            {
                public Repo repo;
                public error err;
            }

            cached c = gitRepoCache.Do(new key(remote,localOK), () =>
            {
                var (repo, err) = newGitRepo(remote, localOK);
                return new cached(repo,err);
            })._<cached>();

            return (c.repo, error.As(c.err)!);

        }

        private static (Repo, error) newGitRepo(@string remote, bool localOK) => func((defer, _, __) =>
        {
            Repo _p0 = default;
            error _p0 = default!;

            ptr<gitRepo> r = addr(new gitRepo(remote:remote));
            if (strings.Contains(remote, "://"))
            { 
                // This is a remote path.
                error err = default!;
                r.dir, r.mu.Path, err = WorkDir(gitWorkDirType, r.remote);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                var (unlock, err) = r.mu.Lock();
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                defer(unlock());

                {
                    var (_, err) = os.Stat(filepath.Join(r.dir, "objects"));

                    if (err != null)
                    {
                        {
                            (_, err) = Run(r.dir, "git", "init", "--bare");

                            if (err != null)
                            {
                                os.RemoveAll(r.dir);
                                return (null, error.As(err)!);
                            } 
                            // We could just say git fetch https://whatever later,
                            // but this lets us say git fetch origin instead, which
                            // is a little nicer. More importantly, using a named remote
                            // avoids a problem with Git LFS. See golang.org/issue/25605.

                        } 
                        // We could just say git fetch https://whatever later,
                        // but this lets us say git fetch origin instead, which
                        // is a little nicer. More importantly, using a named remote
                        // avoids a problem with Git LFS. See golang.org/issue/25605.
                        {
                            (_, err) = Run(r.dir, "git", "remote", "add", "origin", "--", r.remote);

                            if (err != null)
                            {
                                os.RemoveAll(r.dir);
                                return (null, error.As(err)!);
                            }

                        }

                    }

                }

                r.remoteURL = r.remote;
                r.remote = "origin";

            }
            else
            { 
                // Local path.
                // Disallow colon (not in ://) because sometimes
                // that's rcp-style host:path syntax and sometimes it's not (c:\work).
                // The go command has always insisted on URL syntax for ssh.
                if (strings.Contains(remote, ":"))
                {
                    return (null, error.As(fmt.Errorf("git remote cannot use host:path syntax"))!);
                }

                if (!localOK)
                {
                    return (null, error.As(fmt.Errorf("git remote must not be local directory"))!);
                }

                r.local = true;
                var (info, err) = os.Stat(remote);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                if (!info.IsDir())
                {
                    return (null, error.As(fmt.Errorf("%s exists but is not a directory", remote))!);
                }

                r.dir = remote;
                r.mu.Path = r.dir + ".lock";

            }

            return (r, error.As(null!)!);

        });

        private partial struct gitRepo
        {
            public @string remote;
            public @string remoteURL;
            public bool local;
            public @string dir;
            public lockedfile.Mutex mu; // protects fetchLevel and git repo state

            public long fetchLevel;
            public par.Cache statCache;
            public sync.Once refsOnce; // refs maps branch and tag refs (e.g., "HEAD", "refs/heads/master")
// to commits (e.g., "37ffd2e798afde829a34e8955b716ab730b2a6d6")
            public map<@string, @string> refs;
            public error refsErr;
            public sync.Once localTagsOnce;
            public map<@string, bool> localTags;
        }

 
        // How much have we fetched into the git repo (in this process)?
        private static readonly var fetchNone = iota; // nothing yet
        private static readonly var fetchSome = 0; // shallow fetches of individual hashes
        private static readonly var fetchAll = 1; // "fetch -t origin": get all remote branches and tags

        // loadLocalTags loads tag references from the local git cache
        // into the map r.localTags.
        // Should only be called as r.localTagsOnce.Do(r.loadLocalTags).
        private static void loadLocalTags(this ptr<gitRepo> _addr_r)
        {
            ref gitRepo r = ref _addr_r.val;
 
            // The git protocol sends all known refs and ls-remote filters them on the client side,
            // so we might as well record both heads and tags in one shot.
            // Most of the time we only care about tags but sometimes we care about heads too.
            var (out, err) = Run(r.dir, "git", "tag", "-l");
            if (err != null)
            {
                return ;
            }

            r.localTags = make_map<@string, bool>();
            foreach (var (_, line) in strings.Split(string(out), "\n"))
            {
                if (line != "")
                {
                    r.localTags[line] = true;
                }

            }

        }

        // loadRefs loads heads and tags references from the remote into the map r.refs.
        // Should only be called as r.refsOnce.Do(r.loadRefs).
        private static void loadRefs(this ptr<gitRepo> _addr_r)
        {
            ref gitRepo r = ref _addr_r.val;
 
            // The git protocol sends all known refs and ls-remote filters them on the client side,
            // so we might as well record both heads and tags in one shot.
            // Most of the time we only care about tags but sometimes we care about heads too.
            var (out, gitErr) = Run(r.dir, "git", "ls-remote", "-q", r.remote);
            if (gitErr != null)
            {
                {
                    ptr<RunError> (rerr, ok) = gitErr._<ptr<RunError>>();

                    if (ok)
                    {
                        if (bytes.Contains(rerr.Stderr, (slice<byte>)"fatal: could not read Username"))
                        {
                            rerr.HelpText = "Confirm the import path was entered correctly.\nIf this is a private repository, see https://golang.org/doc/faq#git_https for additional information.";
                        }

                    } 

                    // If the remote URL doesn't exist at all, ideally we should treat the whole
                    // repository as nonexistent by wrapping the error in a notExistError.
                    // For HTTP and HTTPS, that's easy to detect: we'll try to fetch the URL
                    // ourselves and see what code it serves.

                } 

                // If the remote URL doesn't exist at all, ideally we should treat the whole
                // repository as nonexistent by wrapping the error in a notExistError.
                // For HTTP and HTTPS, that's easy to detect: we'll try to fetch the URL
                // ourselves and see what code it serves.
                {
                    var (u, err) = url.Parse(r.remoteURL);

                    if (err == null && (u.Scheme == "http" || u.Scheme == "https"))
                    {
                        {
                            var (_, err) = web.GetBytes(u);

                            if (errors.Is(err, os.ErrNotExist))
                            {
                                gitErr = new notExistError(gitErr);
                            }

                        }

                    }

                }


                r.refsErr = gitErr;
                return ;

            }

            r.refs = make_map<@string, @string>();
            foreach (var (_, line) in strings.Split(string(out), "\n"))
            {
                var f = strings.Fields(line);
                if (len(f) != 2L)
                {
                    continue;
                }

                if (f[1L] == "HEAD" || strings.HasPrefix(f[1L], "refs/heads/") || strings.HasPrefix(f[1L], "refs/tags/"))
                {
                    r.refs[f[1L]] = f[0L];
                }

            }
            foreach (var (ref, hash) in r.refs)
            {
                if (strings.HasSuffix(ref, "^{}"))
                { // record unwrapped annotated tag as value of tag
                    r.refs[strings.TrimSuffix(ref, "^{}")] = hash;
                    delete(r.refs, ref);

                }

            }

        }

        private static (slice<@string>, error) Tags(this ptr<gitRepo> _addr_r, @string prefix)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;

            r.refsOnce.Do(r.loadRefs);
            if (r.refsErr != null)
            {
                return (null, error.As(r.refsErr)!);
            }

            @string tags = new slice<@string>(new @string[] {  });
            foreach (var (ref) in r.refs)
            {
                if (!strings.HasPrefix(ref, "refs/tags/"))
                {
                    continue;
                }

                var tag = ref[len("refs/tags/")..];
                if (!strings.HasPrefix(tag, prefix))
                {
                    continue;
                }

                tags = append(tags, tag);

            }
            sort.Strings(tags);
            return (tags, error.As(null!)!);

        }

        private static (ptr<RevInfo>, error) Latest(this ptr<gitRepo> _addr_r)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;

            r.refsOnce.Do(r.loadRefs);
            if (r.refsErr != null)
            {
                return (_addr_null!, error.As(r.refsErr)!);
            }

            if (r.refs["HEAD"] == "")
            {
                return (_addr_null!, error.As(ErrNoCommits)!);
            }

            return _addr_r.Stat(r.refs["HEAD"])!;

        }

        // findRef finds some ref name for the given hash,
        // for use when the server requires giving a ref instead of a hash.
        // There may be multiple ref names for a given hash,
        // in which case this returns some name - it doesn't matter which.
        private static (@string, bool) findRef(this ptr<gitRepo> _addr_r, @string hash)
        {
            @string @ref = default;
            bool ok = default;
            ref gitRepo r = ref _addr_r.val;

            r.refsOnce.Do(r.loadRefs);
            foreach (var (ref, h) in r.refs)
            {
                if (h == hash)
                {
                    return (ref, true);
                }

            }
            return ("", false);

        }

        // minHashDigits is the minimum number of digits to require
        // before accepting a hex digit sequence as potentially identifying
        // a specific commit in a git repo. (Of course, users can always
        // specify more digits, and many will paste in all 40 digits,
        // but many of git's commands default to printing short hashes
        // as 7 digits.)
        private static readonly long minHashDigits = (long)7L;

        // stat stats the given rev in the local repository,
        // or else it fetches more info from the remote repository and tries again.


        // stat stats the given rev in the local repository,
        // or else it fetches more info from the remote repository and tries again.
        private static (ptr<RevInfo>, error) stat(this ptr<gitRepo> _addr_r, @string rev) => func((defer, _, __) =>
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;

            if (r.local)
            {
                return _addr_r.statLocal(rev, rev)!;
            } 

            // Fast path: maybe rev is a hash we already have locally.
            var didStatLocal = false;
            if (len(rev) >= minHashDigits && len(rev) <= 40L && AllHex(rev))
            {
                {
                    var info__prev2 = info;

                    var (info, err) = r.statLocal(rev, rev);

                    if (err == null)
                    {
                        return (_addr_info!, error.As(null!)!);
                    }

                    info = info__prev2;

                }

                didStatLocal = true;

            } 

            // Maybe rev is a tag we already have locally.
            // (Note that we're excluding branches, which can be stale.)
            r.localTagsOnce.Do(r.loadLocalTags);
            if (r.localTags[rev])
            {
                return _addr_r.statLocal(rev, "refs/tags/" + rev)!;
            } 

            // Maybe rev is the name of a tag or branch on the remote server.
            // Or maybe it's the prefix of a hash of a named ref.
            // Try to resolve to both a ref (git name) and full (40-hex-digit) commit hash.
            r.refsOnce.Do(r.loadRefs);
            @string @ref = default;            @string hash = default;

            if (r.refs["refs/tags/" + rev] != "")
            {
                ref = "refs/tags/" + rev;
                hash = r.refs[ref]; 
                // Keep rev as is: tags are assumed not to change meaning.
            }
            else if (r.refs["refs/heads/" + rev] != "")
            {
                ref = "refs/heads/" + rev;
                hash = r.refs[ref];
                rev = hash; // Replace rev, because meaning of refs/heads/foo can change.
            }
            else if (rev == "HEAD" && r.refs["HEAD"] != "")
            {
                ref = "HEAD";
                hash = r.refs[ref];
                rev = hash; // Replace rev, because meaning of HEAD can change.
            }
            else if (len(rev) >= minHashDigits && len(rev) <= 40L && AllHex(rev))
            { 
                // At the least, we have a hash prefix we can look up after the fetch below.
                // Maybe we can map it to a full hash using the known refs.
                var prefix = rev; 
                // Check whether rev is prefix of known ref hash.
                foreach (var (k, h) in r.refs)
                {
                    if (strings.HasPrefix(h, prefix))
                    {
                        if (hash != "" && hash != h)
                        { 
                            // Hash is an ambiguous hash prefix.
                            // More information will not change that.
                            return (_addr_null!, error.As(fmt.Errorf("ambiguous revision %s", rev))!);

                        }

                        if (ref == "" || ref > k)
                        { // Break ties deterministically when multiple refs point at same hash.
                            ref = k;

                        }

                        rev = h;
                        hash = h;

                    }

                }
            else
                if (hash == "" && len(rev) == 40L)
                { // Didn't find a ref, but rev is a full hash.
                    hash = rev;

                }

            }            {
                return (_addr_null!, error.As(addr(new UnknownRevisionError(Rev:rev))!)!);
            } 

            // Protect r.fetchLevel and the "fetch more and more" sequence.
            var (unlock, err) = r.mu.Lock();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            defer(unlock()); 

            // Perhaps r.localTags did not have the ref when we loaded local tags,
            // but we've since done fetches that pulled down the hash we need
            // (or already have the hash we need, just without its tag).
            // Either way, try a local stat before falling back to network I/O.
            if (!didStatLocal)
            {
                {
                    var info__prev2 = info;

                    (info, err) = r.statLocal(rev, hash);

                    if (err == null)
                    {
                        if (strings.HasPrefix(ref, "refs/tags/"))
                        { 
                            // Make sure tag exists, so it will be in localTags next time the go command is run.
                            Run(r.dir, "git", "tag", strings.TrimPrefix(ref, "refs/tags/"), hash);

                        }

                        return (_addr_info!, error.As(null!)!);

                    }

                    info = info__prev2;

                }

            } 

            // If we know a specific commit we need and its ref, fetch it.
            // We do NOT fetch arbitrary hashes (when we don't know the ref)
            // because we want to avoid ever importing a commit that isn't
            // reachable from refs/tags/* or refs/heads/* or HEAD.
            // Both Gerrit and GitHub expose every CL/PR as a named ref,
            // and we don't want those commits masquerading as being real
            // pseudo-versions in the main repo.
            if (r.fetchLevel <= fetchSome && ref != "" && hash != "" && !r.local)
            {
                r.fetchLevel = fetchSome;
                @string refspec = default;
                if (ref != "" && ref != "HEAD")
                { 
                    // If we do know the ref name, save the mapping locally
                    // so that (if it is a tag) it can show up in localTags
                    // on a future call. Also, some servers refuse to allow
                    // full hashes in ref specs, so prefer a ref name if known.
                    refspec = ref + ":" + ref;

                }
                else
                { 
                    // Fetch the hash but give it a local name (refs/dummy),
                    // because that triggers the fetch behavior of creating any
                    // other known remote tags for the hash. We never use
                    // refs/dummy (it's not refs/tags/dummy) and it will be
                    // overwritten in the next command, and that's fine.
                    ref = hash;
                    refspec = hash + ":refs/dummy";

                }

                var (_, err) = Run(r.dir, "git", "fetch", "-f", "--depth=1", r.remote, refspec);
                if (err == null)
                {
                    return _addr_r.statLocal(rev, ref)!;
                } 
                // Don't try to be smart about parsing the error.
                // It's too complex and varies too much by git version.
                // No matter what went wrong, fall back to a complete fetch.
            } 

            // Last resort.
            // Fetch all heads and tags and hope the hash we want is in the history.
            {
                var err = r.fetchRefsLocked();

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }


            return _addr_r.statLocal(rev, rev)!;

        });

        // fetchRefsLocked fetches all heads and tags from the origin, along with the
        // ancestors of those commits.
        //
        // We only fetch heads and tags, not arbitrary other commits: we don't want to
        // pull in off-branch commits (such as rejected GitHub pull requests) that the
        // server may be willing to provide. (See the comments within the stat method
        // for more detail.)
        //
        // fetchRefsLocked requires that r.mu remain locked for the duration of the call.
        private static error fetchRefsLocked(this ptr<gitRepo> _addr_r)
        {
            ref gitRepo r = ref _addr_r.val;

            if (r.fetchLevel < fetchAll)
            { 
                // NOTE: To work around a bug affecting Git clients up to at least 2.23.0
                // (2019-08-16), we must first expand the set of local refs, and only then
                // unshallow the repository as a separate fetch operation. (See
                // golang.org/issue/34266 and
                // https://github.com/git/git/blob/4c86140027f4a0d2caaa3ab4bd8bfc5ce3c11c8a/transport.c#L1303-L1309.)

                {
                    var (_, err) = Run(r.dir, "git", "fetch", "-f", r.remote, "refs/heads/*:refs/heads/*", "refs/tags/*:refs/tags/*");

                    if (err != null)
                    {
                        return error.As(err)!;
                    }

                }


                {
                    (_, err) = os.Stat(filepath.Join(r.dir, "shallow"));

                    if (err == null)
                    {
                        {
                            (_, err) = Run(r.dir, "git", "fetch", "--unshallow", "-f", r.remote);

                            if (err != null)
                            {
                                return error.As(err)!;
                            }

                        }

                    }

                }


                r.fetchLevel = fetchAll;

            }

            return error.As(null!)!;

        }

        // statLocal returns a RevInfo describing rev in the local git repository.
        // It uses version as info.Version.
        private static (ptr<RevInfo>, error) statLocal(this ptr<gitRepo> _addr_r, @string version, @string rev)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;

            var (out, err) = Run(r.dir, "git", "-c", "log.showsignature=false", "log", "-n1", "--format=format:%H %ct %D", rev, "--");
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new UnknownRevisionError(Rev:rev))!)!);
            }

            var f = strings.Fields(string(out));
            if (len(f) < 2L)
            {
                return (_addr_null!, error.As(fmt.Errorf("unexpected response from git log: %q", out))!);
            }

            var hash = f[0L];
            if (strings.HasPrefix(hash, version))
            {
                version = hash; // extend to full hash
            }

            var (t, err) = strconv.ParseInt(f[1L], 10L, 64L);
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("invalid time from git log: %q", out))!);
            }

            ptr<RevInfo> info = addr(new RevInfo(Name:hash,Short:ShortenSHA1(hash),Time:time.Unix(t,0).UTC(),Version:hash,)); 

            // Add tags. Output looks like:
            //    ede458df7cd0fdca520df19a33158086a8a68e81 1523994202 HEAD -> master, tag: v1.2.4-annotated, tag: v1.2.3, origin/master, origin/HEAD
            for (long i = 2L; i < len(f); i++)
            {
                if (f[i] == "tag:")
                {
                    i++;
                    if (i < len(f))
                    {
                        info.Tags = append(info.Tags, strings.TrimSuffix(f[i], ","));
                    }

                }

            }

            sort.Strings(info.Tags); 

            // Used hash as info.Version above.
            // Use caller's suggested version if it appears in the tag list
            // (filters out branch names, HEAD).
            foreach (var (_, tag) in info.Tags)
            {
                if (version == tag)
                {
                    info.Version = version;
                }

            }
            return (_addr_info!, error.As(null!)!);

        }

        private static (ptr<RevInfo>, error) Stat(this ptr<gitRepo> _addr_r, @string rev)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;

            if (rev == "latest")
            {
                return _addr_r.Latest()!;
            }

            private partial struct cached
            {
                public Repo repo;
                public error err;
            }
            cached c = r.statCache.Do(rev, () =>
            {
                var (info, err) = r.stat(rev);
                return _addr_new cached(info,err)!;
            })._<cached>();
            return (_addr_c.info!, error.As(c.err)!);

        }

        private static (slice<byte>, error) ReadFile(this ptr<gitRepo> _addr_r, @string rev, @string file, long maxSize)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;
 
            // TODO: Could use git cat-file --batch.
            var (info, err) = r.Stat(rev); // download rev into local git repo
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (out, err) = Run(r.dir, "git", "cat-file", "blob", info.Name + ":" + file);
            if (err != null)
            {
                return (null, error.As(os.ErrNotExist)!);
            }

            return (out, error.As(null!)!);

        }

        private static (map<@string, ptr<FileRev>>, error) ReadFileRevs(this ptr<gitRepo> _addr_r, slice<@string> revs, @string file, long maxSize) => func((defer, _, __) =>
        {
            map<@string, ptr<FileRev>> _p0 = default;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;
 
            // Create space to hold results.
            var files = make_map<@string, ptr<FileRev>>();
            foreach (var (_, rev) in revs)
            {
                ptr<FileRev> f = addr(new FileRev(Rev:rev));
                files[rev] = f;
            } 

            // Collect locally-known revs.
            var (need, err) = r.readFileRevs(revs, file, files);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (len(need) == 0L)
            {
                return (files, error.As(null!)!);
            } 

            // Build list of known remote refs that might help.
            slice<@string> redo = default;
            r.refsOnce.Do(r.loadRefs);
            if (r.refsErr != null)
            {
                return (null, error.As(r.refsErr)!);
            }

            foreach (var (_, tag) in need)
            {
                if (r.refs["refs/tags/" + tag] != "")
                {
                    redo = append(redo, tag);
                }

            }
            if (len(redo) == 0L)
            {
                return (files, error.As(null!)!);
            } 

            // Protect r.fetchLevel and the "fetch more and more" sequence.
            // See stat method above.
            var (unlock, err) = r.mu.Lock();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(unlock());

            {
                var err = r.fetchRefsLocked();

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


            {
                var (_, err) = r.readFileRevs(redo, file, files);

                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }


            return (files, error.As(null!)!);

        });

        private static (slice<@string>, error) readFileRevs(this ptr<gitRepo> _addr_r, slice<@string> tags, @string file, map<@string, ptr<FileRev>> fileMap)
        {
            slice<@string> missing = default;
            error err = default!;
            ref gitRepo r = ref _addr_r.val;

            ref bytes.Buffer stdin = ref heap(out ptr<bytes.Buffer> _addr_stdin);
            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in tags)
                {
                    tag = __tag;
                    fmt.Fprintf(_addr_stdin, "refs/tags/%s\n", tag);
                    fmt.Fprintf(_addr_stdin, "refs/tags/%s:%s\n", tag, file);
                }

                tag = tag__prev1;
            }

            var (data, err) = RunWithStdin(r.dir, _addr_stdin, "git", "cat-file", "--batch");
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            Func<(@string, slice<byte>, bool)> next = () =>
            {
                @string line = default;
                var i = bytes.IndexByte(data, '\n');
                if (i < 0L)
                {
                    return ("", error.As(null!)!, false);
                }

                line = string(bytes.TrimSpace(data[..i]));
                data = data[i + 1L..];
                if (strings.HasSuffix(line, " missing"))
                {
                    return ("missing", error.As(null!)!, true);
                }

                var f = strings.Fields(line);
                if (len(f) != 3L)
                {
                    return ("", error.As(null!)!, false);
                }

                var (n, err) = strconv.Atoi(f[2L]);
                if (err != null || n > len(data))
                {
                    return ("", error.As(null!)!, false);
                }

                body = data[..n];
                data = data[n..];
                if (len(data) > 0L && data[0L] == '\r')
                {
                    data = data[1L..];
                }

                if (len(data) > 0L && data[0L] == '\n')
                {
                    data = data[1L..];
                }

                return (f[1L], error.As(body)!, true);

            }
;

            Func<(slice<@string>, error)> badGit = () =>
            {
                return (null, error.As(fmt.Errorf("malformed output from git cat-file --batch"))!);
            }
;

            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in tags)
                {
                    tag = __tag;
                    var (commitType, _, ok) = next();
                    if (!ok)
                    {
                        return badGit();
                    }

                    var (fileType, fileData, ok) = next();
                    if (!ok)
                    {
                        return badGit();
                    }

                    f = fileMap[tag];
                    f.Data = null;
                    f.Err = null;
                    switch (commitType)
                    {
                        case "missing": 
                            // Note: f.Err must not satisfy os.IsNotExist. That's reserved for the file not existing in a valid commit.
                            f.Err = fmt.Errorf("no such rev %s", tag);
                            missing = append(missing, tag);
                            break;
                        case "tag": 

                        case "commit": 
                            switch (fileType)
                            {
                                case "missing": 
                                    f.Err = addr(new os.PathError(Path:tag+":"+file,Op:"read",Err:os.ErrNotExist));
                                    break;
                                case "blob": 
                                    f.Data = fileData;
                                    break;
                                default: 
                                    f.Err = addr(new os.PathError(Path:tag+":"+file,Op:"read",Err:fmt.Errorf("unexpected non-blob type %q",fileType)));
                                    break;
                            }
                            break;
                        default: 
                            f.Err = fmt.Errorf("unexpected non-commit type %q for rev %s", commitType, tag);
                            break;
                    }

                }

                tag = tag__prev1;
            }

            if (len(bytes.TrimSpace(data)) != 0L)
            {
                return badGit();
            }

            return (missing, error.As(null!)!);

        }

        private static (@string, error) RecentTag(this ptr<gitRepo> _addr_r, @string rev, @string prefix, @string major) => func((defer, _, __) =>
        {
            @string tag = default;
            error err = default!;
            ref gitRepo r = ref _addr_r.val;

            var (info, err) = r.Stat(rev);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            rev = info.Name; // expand hash prefixes

            // describe sets tag and err using 'git for-each-ref' and reports whether the
            // result is definitive.
            Func<bool> describe = () =>
            {
                slice<byte> @out = default;
                out, err = Run(r.dir, "git", "for-each-ref", "--format", "%(refname)", "refs/tags", "--merged", rev);
                if (err != null)
                {
                    return true;
                } 

                // prefixed tags aren't valid semver tags so compare without prefix, but only tags with correct prefix
                @string highest = default;
                foreach (var (_, line) in strings.Split(string(out), "\n"))
                {
                    line = strings.TrimSpace(line); 
                    // git do support lstrip in for-each-ref format, but it was added in v2.13.0. Stripping here
                    // instead gives support for git v2.7.0.
                    if (!strings.HasPrefix(line, "refs/tags/"))
                    {
                        continue;
                    }

                    line = line[len("refs/tags/")..];

                    if (!strings.HasPrefix(line, prefix))
                    {
                        continue;
                    }

                    var semtag = line[len(prefix)..]; 
                    // Consider only tags that are valid and complete (not just major.minor prefixes).
                    // NOTE: Do not replace the call to semver.Compare with semver.Max.
                    // We want to return the actual tag, not a canonicalized version of it,
                    // and semver.Max currently canonicalizes (see golang.org/issue/32700).
                    {
                        var c = semver.Canonical(semtag);

                        if (c != "" && strings.HasPrefix(semtag, c) && (major == "" || semver.Major(c) == major) && semver.Compare(semtag, highest) > 0L)
                        {
                            highest = semtag;
                        }

                    }

                }
                if (highest != "")
                {
                    tag = prefix + highest;
                }

                return tag != "" && !AllHex(tag);

            }
;

            if (describe())
            {
                return (tag, error.As(err)!);
            } 

            // Git didn't find a version tag preceding the requested rev.
            // See whether any plausible tag exists.
            var (tags, err) = r.Tags(prefix + "v");
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            if (len(tags) == 0L)
            {
                return ("", error.As(null!)!);
            } 

            // There are plausible tags, but we don't know if rev is a descendent of any of them.
            // Fetch the history to find out.
            var (unlock, err) = r.mu.Lock();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            defer(unlock());

            {
                var err = r.fetchRefsLocked();

                if (err != null)
                {
                    return ("", error.As(err)!);
                } 

                // If we've reached this point, we have all of the commits that are reachable
                // from all heads and tags.
                //
                // The only refs we should be missing are those that are no longer reachable
                // (or never were reachable) from any branch or tag, including the master
                // branch, and we don't want to resolve them anyway (they're probably
                // unreachable for a reason).
                //
                // Try one last time in case some other goroutine fetched rev while we were
                // waiting on the lock.

            } 

            // If we've reached this point, we have all of the commits that are reachable
            // from all heads and tags.
            //
            // The only refs we should be missing are those that are no longer reachable
            // (or never were reachable) from any branch or tag, including the master
            // branch, and we don't want to resolve them anyway (they're probably
            // unreachable for a reason).
            //
            // Try one last time in case some other goroutine fetched rev while we were
            // waiting on the lock.
            describe();
            return (tag, error.As(err)!);

        });

        private static (bool, error) DescendsFrom(this ptr<gitRepo> _addr_r, @string rev, @string tag) => func((defer, _, __) =>
        {
            bool _p0 = default;
            error _p0 = default!;
            ref gitRepo r = ref _addr_r.val;
 
            // The "--is-ancestor" flag was added to "git merge-base" in version 1.8.0, so
            // this won't work with Git 1.7.1. According to golang.org/issue/28550, cmd/go
            // already doesn't work with Git 1.7.1, so at least it's not a regression.
            //
            // git merge-base --is-ancestor exits with status 0 if rev is an ancestor, or
            // 1 if not.
            var (_, err) = Run(r.dir, "git", "merge-base", "--is-ancestor", "--", tag, rev); 

            // Git reports "is an ancestor" with exit code 0 and "not an ancestor" with
            // exit code 1.
            // Unfortunately, if we've already fetched rev with a shallow history, git
            // merge-base has been observed to report a false-negative, so don't stop yet
            // even if the exit code is 1!
            if (err == null)
            {
                return (true, error.As(null!)!);
            } 

            // See whether the tag and rev even exist.
            var (tags, err) = r.Tags(tag);
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            if (len(tags) == 0L)
            {
                return (false, error.As(null!)!);
            } 

            // NOTE: r.stat is very careful not to fetch commits that we shouldn't know
            // about, like rejected GitHub pull requests, so don't try to short-circuit
            // that here.
            _, err = r.stat(rev);

            if (err != null)
            {
                return (false, error.As(err)!);
            } 

            // Now fetch history so that git can search for a path.
            var (unlock, err) = r.mu.Lock();
            if (err != null)
            {
                return (false, error.As(err)!);
            }

            defer(unlock());

            if (r.fetchLevel < fetchAll)
            { 
                // Fetch the complete history for all refs and heads. It would be more
                // efficient to only fetch the history from rev to tag, but that's much more
                // complicated, and any kind of shallow fetch is fairly likely to trigger
                // bugs in JGit servers and/or the go command anyway.
                {
                    var err = r.fetchRefsLocked();

                    if (err != null)
                    {
                        return (false, error.As(err)!);
                    }

                }

            }

            _, err = Run(r.dir, "git", "merge-base", "--is-ancestor", "--", tag, rev);
            if (err == null)
            {
                return (true, error.As(null!)!);
            }

            {
                ptr<exec.ExitError> (ee, ok) = err._<ptr<RunError>>().Err._<ptr<exec.ExitError>>();

                if (ok && ee.ExitCode() == 1L)
                {
                    return (false, error.As(null!)!);
                }

            }

            return (false, error.As(err)!);

        });

        private static (io.ReadCloser, error) ReadZip(this ptr<gitRepo> _addr_r, @string rev, @string subdir, long maxSize) => func((defer, _, __) =>
        {
            io.ReadCloser zip = default;
            error err = default!;
            ref gitRepo r = ref _addr_r.val;
 
            // TODO: Use maxSize or drop it.
            @string args = new slice<@string>(new @string[] {  });
            if (subdir != "")
            {
                args = append(args, "--", subdir);
            }

            var (info, err) = r.Stat(rev); // download rev into local git repo
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (unlock, err) = r.mu.Lock();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(unlock());

            {
                var err = ensureGitAttributes(r.dir);

                if (err != null)
                {
                    return (null, error.As(err)!);
                } 

                // Incredibly, git produces different archives depending on whether
                // it is running on a Windows system or not, in an attempt to normalize
                // text file line endings. Setting -c core.autocrlf=input means only
                // translate files on the way into the repo, not on the way out (archive).
                // The -c core.eol=lf should be unnecessary but set it anyway.

            } 

            // Incredibly, git produces different archives depending on whether
            // it is running on a Windows system or not, in an attempt to normalize
            // text file line endings. Setting -c core.autocrlf=input means only
            // translate files on the way into the repo, not on the way out (archive).
            // The -c core.eol=lf should be unnecessary but set it anyway.
            var (archive, err) = Run(r.dir, "git", "-c", "core.autocrlf=input", "-c", "core.eol=lf", "archive", "--format=zip", "--prefix=prefix/", info.Name, args);
            if (err != null)
            {
                if (bytes.Contains(err._<ptr<RunError>>().Stderr, (slice<byte>)"did not match any files"))
                {
                    return (null, error.As(os.ErrNotExist)!);
                }

                return (null, error.As(err)!);

            }

            return (ioutil.NopCloser(bytes.NewReader(archive)), error.As(null!)!);

        });

        // ensureGitAttributes makes sure export-subst and export-ignore features are
        // disabled for this repo. This is intended to be run prior to running git
        // archive so that zip files are generated that produce consistent ziphashes
        // for a given revision, independent of variables such as git version and the
        // size of the repo.
        //
        // See: https://github.com/golang/go/issues/27153
        private static error ensureGitAttributes(@string repoDir) => func((defer, _, __) =>
        {
            error err = default!;

            const @string attr = (@string)"\n* -export-subst -export-ignore\n";



            var d = repoDir + "/info";
            var p = d + "/attributes";

            {
                var err = os.MkdirAll(d, 0755L);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var (f, err) = os.OpenFile(p, os.O_CREATE | os.O_APPEND | os.O_RDWR, 0666L);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(() =>
            {
                var closeErr = f.Close();
                if (closeErr != null)
                {
                    err = closeErr;
                }

            }());

            var (b, err) = ioutil.ReadAll(f);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (!bytes.HasSuffix(b, (slice<byte>)attr))
            {
                var (_, err) = f.WriteString(attr);
                return error.As(err)!;
            }

            return error.As(null!)!;

        });
    }
}}}}}
