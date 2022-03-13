// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package codehost -- go2cs converted at 2022 March 13 06:32:08 UTC
// import "cmd/go/internal/modfetch/codehost" ==> using codehost = go.cmd.go.@internal.modfetch.codehost_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\modfetch\codehost\vcs.go
namespace go.cmd.go.@internal.modfetch;

using errors = errors_package;
using fmt = fmt_package;
using lazyregexp = @internal.lazyregexp_package;
using io = io_package;
using fs = io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;

using lockedfile = cmd.go.@internal.lockedfile_package;
using par = cmd.go.@internal.par_package;
using str = cmd.go.@internal.str_package;


// A VCSError indicates an error using a version control system.
// The implication of a VCSError is that we know definitively where
// to get the code, but we can't access it due to the error.
// The caller should report this error instead of continuing to probe
// other possible module paths.
//
// TODO(golang.org/issue/31730): See if we can invert this. (Return a
// distinguished error for “repo not found” and treat everything else
// as terminal.)

using System;
public static partial class codehost_package {

public partial struct VCSError {
    public error Err;
}

private static @string Error(this ptr<VCSError> _addr_e) {
    ref VCSError e = ref _addr_e.val;

    return e.Err.Error();
}

private static error vcsErrorf(@string format, params object[] a) {
    a = a.Clone();

    return error.As(addr(new VCSError(Err:fmt.Errorf(format,a...)))!)!;
}

public static (Repo, error) NewRepo(@string vcs, @string remote) {
    Repo _p0 = default;
    error _p0 = default!;

    private partial struct key {
        public @string vcs;
        public @string remote;
    }
    private partial struct cached {
        public Repo repo;
        public error err;
    }
    cached c = vcsRepoCache.Do(new key(vcs,remote), () => {
        var (repo, err) = newVCSRepo(vcs, remote);
        if (err != null) {
            err = addr(new VCSError(err));
        }
        return new cached(repo,err);
    })._<cached>();

    return (c.repo, error.As(c.err)!);
}

private static par.Cache vcsRepoCache = default;

private partial struct vcsRepo {
    public lockedfile.Mutex mu; // protects all commands, so we don't have to decide which are safe on a per-VCS basis

    public @string remote;
    public ptr<vcsCmd> cmd;
    public @string dir;
    public sync.Once tagsOnce;
    public map<@string, bool> tags;
    public sync.Once branchesOnce;
    public map<@string, bool> branches;
    public sync.Once fetchOnce;
    public error fetchErr;
}

private static (Repo, error) newVCSRepo(@string vcs, @string remote) => func((defer, _, _) => {
    Repo _p0 = default;
    error _p0 = default!;

    if (vcs == "git") {
        return newGitRepo(remote, false);
    }
    var cmd = vcsCmds[vcs];
    if (cmd == null) {
        return (null, error.As(fmt.Errorf("unknown vcs: %s %s", vcs, remote))!);
    }
    if (!strings.Contains(remote, "://")) {
        return (null, error.As(fmt.Errorf("invalid vcs remote: %s %s", vcs, remote))!);
    }
    ptr<vcsRepo> r = addr(new vcsRepo(remote:remote,cmd:cmd));
    error err = default!;
    r.dir, r.mu.Path, err = WorkDir(vcsWorkDirType + vcs, r.remote);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (cmd.init == null) {
        return (r, error.As(null!)!);
    }
    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(unlock());

    {
        var (_, err) = os.Stat(filepath.Join(r.dir, "." + vcs));

        if (err != null) {
            {
                (_, err) = Run(r.dir, cmd.init(r.remote));

                if (err != null) {
                    os.RemoveAll(r.dir);
                    return (null, error.As(err)!);
                }

            }
        }
    }
    return (r, error.As(null!)!);
});

private static readonly @string vcsWorkDirType = "vcs1.";



private partial struct vcsCmd {
    public @string vcs; // vcs name "hg"
    public Func<@string, slice<@string>> init; // cmd to init repo to track remote
    public Func<@string, slice<@string>> tags; // cmd to list local tags
    public ptr<lazyregexp.Regexp> tagRE; // regexp to extract tag names from output of tags cmd
    public Func<@string, slice<@string>> branches; // cmd to list local branches
    public ptr<lazyregexp.Regexp> branchRE; // regexp to extract branch names from output of tags cmd
    public ptr<lazyregexp.Regexp> badLocalRevRE; // regexp of names that must not be served out of local cache without doing fetch first
    public Func<@string, @string, slice<@string>> statLocal; // cmd to stat local rev
    public Func<@string, @string, (ptr<RevInfo>, error)> parseStat; // cmd to parse output of statLocal
    public slice<@string> fetch; // cmd to fetch everything from remote
    public @string latest; // name of latest commit on remote (tip, HEAD, etc)
    public Func<@string, @string, @string, slice<@string>> readFile; // cmd to read rev's file
    public Func<@string, @string, @string, @string, slice<@string>> readZip; // cmd to read rev's subdir as zip file
    public Func<io.Writer, @string, @string, @string, @string, error> doReadZip; // arbitrary function to read rev's subdir as zip file
}

private static var re = lazyregexp.New;

private static map vcsCmds = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<vcsCmd>>{"hg":{vcs:"hg",init:func(remotestring)[]string{return[]string{"hg","clone","-U","--",remote,"."}},tags:func(remotestring)[]string{return[]string{"hg","tags","-q"}},tagRE:re(`(?m)^[^\n]+$`),branches:func(remotestring)[]string{return[]string{"hg","branches","-c","-q"}},branchRE:re(`(?m)^[^\n]+$`),badLocalRevRE:re(`(?m)^(tip)$`),statLocal:func(rev,remotestring)[]string{return[]string{"hg","log","-l1","-r",rev,"--template","{node} {date|hgdate} {tags}"}},parseStat:hgParseStat,fetch:[]string{"hg","pull","-f"},latest:"tip",readFile:func(rev,file,remotestring)[]string{return[]string{"hg","cat","-r",rev,file}},readZip:func(rev,subdir,remote,targetstring)[]string{pattern:=[]string{}ifsubdir!=""{pattern=[]string{"-I",subdir+"/**"}}returnstr.StringList("hg","archive","-t","zip","--no-decode","-r",rev,"--prefix=prefix/",pattern,"--",target)},},"svn":{vcs:"svn",init:nil,tags:func(remotestring)[]string{return[]string{"svn","list","--",strings.TrimSuffix(remote,"/trunk")+"/tags"}},tagRE:re(`(?m)^(.*?)/?$`),statLocal:func(rev,remotestring)[]string{suffix:="@"+revifrev=="latest"{suffix=""}return[]string{"svn","log","-l1","--xml","--",remote+suffix}},parseStat:svnParseStat,latest:"latest",readFile:func(rev,file,remotestring)[]string{return[]string{"svn","cat","--",remote+"/"+file+"@"+rev}},doReadZip:svnReadZip,},"bzr":{vcs:"bzr",init:func(remotestring)[]string{return[]string{"bzr","branch","--use-existing-dir","--",remote,"."}},fetch:[]string{"bzr","pull","--overwrite-tags",},tags:func(remotestring)[]string{return[]string{"bzr","tags"}},tagRE:re(`(?m)^\S+`),badLocalRevRE:re(`^revno:-`),statLocal:func(rev,remotestring)[]string{return[]string{"bzr","log","-l1","--long","--show-ids","-r",rev}},parseStat:bzrParseStat,latest:"revno:-1",readFile:func(rev,file,remotestring)[]string{return[]string{"bzr","cat","-r",rev,file}},readZip:func(rev,subdir,remote,targetstring)[]string{extra:=[]string{}ifsubdir!=""{extra=[]string{"./"+subdir}}returnstr.StringList("bzr","export","--format=zip","-r",rev,"--root=prefix/","--",target,extra)},},"fossil":{vcs:"fossil",init:func(remotestring)[]string{return[]string{"fossil","clone","--",remote,".fossil"}},fetch:[]string{"fossil","pull","-R",".fossil"},tags:func(remotestring)[]string{return[]string{"fossil","tag","-R",".fossil","list"}},tagRE:re(`XXXTODO`),statLocal:func(rev,remotestring)[]string{return[]string{"fossil","info","-R",".fossil",rev}},parseStat:fossilParseStat,latest:"trunk",readFile:func(rev,file,remotestring)[]string{return[]string{"fossil","cat","-R",".fossil","-r",rev,file}},readZip:func(rev,subdir,remote,targetstring)[]string{extra:=[]string{}ifsubdir!=""&&!strings.ContainsAny(subdir,"*?[],"){extra=[]string{"--include",subdir}}returnstr.StringList("fossil","zip","-R",".fossil","--name","prefix",extra,"--",rev,target)},},};

private static void loadTags(this ptr<vcsRepo> _addr_r) {
    ref vcsRepo r = ref _addr_r.val;

    var (out, err) = Run(r.dir, r.cmd.tags(r.remote));
    if (err != null) {
        return ;
    }
    r.tags = make_map<@string, bool>();
    foreach (var (_, tag) in r.cmd.tagRE.FindAllString(string(out), -1)) {
        if (r.cmd.badLocalRevRE != null && r.cmd.badLocalRevRE.MatchString(tag)) {
            continue;
        }
        r.tags[tag] = true;
    }
}

private static void loadBranches(this ptr<vcsRepo> _addr_r) {
    ref vcsRepo r = ref _addr_r.val;

    if (r.cmd.branches == null) {
        return ;
    }
    var (out, err) = Run(r.dir, r.cmd.branches(r.remote));
    if (err != null) {
        return ;
    }
    r.branches = make_map<@string, bool>();
    foreach (var (_, branch) in r.cmd.branchRE.FindAllString(string(out), -1)) {
        if (r.cmd.badLocalRevRE != null && r.cmd.badLocalRevRE.MatchString(branch)) {
            continue;
        }
        r.branches[branch] = true;
    }
}

private static (slice<@string>, error) Tags(this ptr<vcsRepo> _addr_r, @string prefix) => func((defer, _, _) => {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;

    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(unlock());

    r.tagsOnce.Do(r.loadTags);

    @string tags = new slice<@string>(new @string[] {  });
    foreach (var (tag) in r.tags) {
        if (strings.HasPrefix(tag, prefix)) {
            tags = append(tags, tag);
        }
    }    sort.Strings(tags);
    return (tags, error.As(null!)!);
});

private static (ptr<RevInfo>, error) Stat(this ptr<vcsRepo> _addr_r, @string rev) => func((defer, _, _) => {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;

    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(unlock());

    if (rev == "latest") {
        rev = r.cmd.latest;
    }
    r.branchesOnce.Do(r.loadBranches);
    var revOK = (r.cmd.badLocalRevRE == null || !r.cmd.badLocalRevRE.MatchString(rev)) && !r.branches[rev];
    if (revOK) {
        {
            var info__prev2 = info;

            var (info, err) = r.statLocal(rev);

            if (err == null) {
                return (_addr_info!, error.As(null!)!);
            }

            info = info__prev2;

        }
    }
    r.fetchOnce.Do(r.fetch);
    if (r.fetchErr != null) {
        return (_addr_null!, error.As(r.fetchErr)!);
    }
    (info, err) = r.statLocal(rev);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (!revOK) {
        info.Version = info.Name;
    }
    return (_addr_info!, error.As(null!)!);
});

private static void fetch(this ptr<vcsRepo> _addr_r) {
    ref vcsRepo r = ref _addr_r.val;

    if (len(r.cmd.fetch) > 0) {
        _, r.fetchErr = Run(r.dir, r.cmd.fetch);
    }
}

private static (ptr<RevInfo>, error) statLocal(this ptr<vcsRepo> _addr_r, @string rev) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;

    var (out, err) = Run(r.dir, r.cmd.statLocal(rev, r.remote));
    if (err != null) {
        return (_addr_null!, error.As(addr(new UnknownRevisionError(Rev:rev))!)!);
    }
    return _addr_r.cmd.parseStat(rev, string(out))!;
}

private static (ptr<RevInfo>, error) Latest(this ptr<vcsRepo> _addr_r) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;

    return _addr_r.Stat("latest")!;
}

private static (slice<byte>, error) ReadFile(this ptr<vcsRepo> _addr_r, @string rev, @string file, long maxSize) => func((defer, _, _) => {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;

    if (rev == "latest") {
        rev = r.cmd.latest;
    }
    var (_, err) = r.Stat(rev); // download rev into local repo
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(unlock());

    var (out, err) = Run(r.dir, r.cmd.readFile(rev, file, r.remote));
    if (err != null) {
        return (null, error.As(fs.ErrNotExist)!);
    }
    return (out, error.As(null!)!);
});

private static (map<@string, ptr<FileRev>>, error) ReadFileRevs(this ptr<vcsRepo> _addr_r, slice<@string> revs, @string file, long maxSize) => func((defer, _, _) => {
    map<@string, ptr<FileRev>> _p0 = default;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;
 
    // We don't technically need to lock here since we're returning an error
    // uncondititonally, but doing so anyway will help to avoid baking in
    // lock-inversion bugs.
    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(unlock());

    return (null, error.As(vcsErrorf("ReadFileRevs not implemented"))!);
});

private static (@string, error) RecentTag(this ptr<vcsRepo> _addr_r, @string rev, @string prefix, Func<@string, bool> allowed) => func((defer, _, _) => {
    @string tag = default;
    error err = default!;
    ref vcsRepo r = ref _addr_r.val;
 
    // We don't technically need to lock here since we're returning an error
    // uncondititonally, but doing so anyway will help to avoid baking in
    // lock-inversion bugs.
    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return ("", error.As(err)!);
    }
    defer(unlock());

    return ("", error.As(vcsErrorf("RecentTag not implemented"))!);
});

private static (bool, error) DescendsFrom(this ptr<vcsRepo> _addr_r, @string rev, @string tag) => func((defer, _, _) => {
    bool _p0 = default;
    error _p0 = default!;
    ref vcsRepo r = ref _addr_r.val;

    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (false, error.As(err)!);
    }
    defer(unlock());

    return (false, error.As(vcsErrorf("DescendsFrom not implemented"))!);
});

private static (io.ReadCloser, error) ReadZip(this ptr<vcsRepo> _addr_r, @string rev, @string subdir, long maxSize) => func((defer, _, _) => {
    io.ReadCloser zip = default;
    error err = default!;
    ref vcsRepo r = ref _addr_r.val;

    if (r.cmd.readZip == null && r.cmd.doReadZip == null) {
        return (null, error.As(vcsErrorf("ReadZip not implemented for %s", r.cmd.vcs))!);
    }
    var (unlock, err) = r.mu.Lock();
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(unlock());

    if (rev == "latest") {
        rev = r.cmd.latest;
    }
    var (f, err) = os.CreateTemp("", "go-readzip-*.zip");
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (r.cmd.doReadZip != null) {
        ptr<limitedWriter> lw = addr(new limitedWriter(W:f,N:maxSize,ErrLimitReached:errors.New("ReadZip: encoded file exceeds allowed size"),));
        err = r.cmd.doReadZip(lw, r.dir, rev, subdir, r.remote);
        if (err == null) {
            _, err = f.Seek(0, io.SeekStart);
        }
    }
    else if (r.cmd.vcs == "fossil") { 
        // If you run
        //    fossil zip -R .fossil --name prefix trunk /tmp/x.zip
        // fossil fails with "unable to create directory /tmp" [sic].
        // Change the command to run in /tmp instead,
        // replacing the -R argument with an absolute path.
        var args = r.cmd.readZip(rev, subdir, r.remote, filepath.Base(f.Name()));
        foreach (var (i) in args) {
            if (args[i] == ".fossil") {
                args[i] = filepath.Join(r.dir, ".fossil");
            }
        }
    else
        _, err = Run(filepath.Dir(f.Name()), args);
    } {
        _, err = Run(r.dir, r.cmd.readZip(rev, subdir, r.remote, f.Name()));
    }
    if (err != null) {
        f.Close();
        os.Remove(f.Name());
        return (null, error.As(err)!);
    }
    return (addr(new deleteCloser(f)), error.As(null!)!);
});

// deleteCloser is a file that gets deleted on Close.
private partial struct deleteCloser {
    public ref ptr<os.File> File> => ref File>_ptr;
}

private static error Close(this ptr<deleteCloser> _addr_d) => func((defer, _, _) => {
    ref deleteCloser d = ref _addr_d.val;

    defer(os.Remove(d.File.Name()));
    return error.As(d.File.Close())!;
});

private static (ptr<RevInfo>, error) hgParseStat(@string rev, @string @out) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;

    var f = strings.Fields(string(out));
    if (len(f) < 3) {
        return (_addr_null!, error.As(vcsErrorf("unexpected response from hg log: %q", out))!);
    }
    var hash = f[0];
    var version = rev;
    if (strings.HasPrefix(hash, version)) {
        version = hash; // extend to full hash
    }
    var (t, err) = strconv.ParseInt(f[1], 10, 64);
    if (err != null) {
        return (_addr_null!, error.As(vcsErrorf("invalid time from hg log: %q", out))!);
    }
    slice<@string> tags = default;
    foreach (var (_, tag) in f[(int)3..]) {
        if (tag != "tip") {
            tags = append(tags, tag);
        }
    }    sort.Strings(tags);

    ptr<RevInfo> info = addr(new RevInfo(Name:hash,Short:ShortenSHA1(hash),Time:time.Unix(t,0).UTC(),Version:version,Tags:tags,));
    return (_addr_info!, error.As(null!)!);
}

private static (ptr<RevInfo>, error) bzrParseStat(@string rev, @string @out) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;

    long revno = default;
    time.Time tm = default;
    foreach (var (_, line) in strings.Split(out, "\n")) {
        if (line == "" || line[0] == ' ' || line[0] == '\t') { 
            // End of header, start of commit message.
            break;
        }
        if (line[0] == '-') {
            continue;
        }
        var i = strings.Index(line, ":");
        if (i < 0) { 
            // End of header, start of commit message.
            break;
        }
        var key = line[..(int)i];
        var val = strings.TrimSpace(line[(int)i + 1..]);
        switch (key) {
            case "revno": 
                {
                    var j__prev1 = j;

                    var j = strings.Index(val, " ");

                    if (j >= 0) {
                        val = val[..(int)j];
                    }

                    j = j__prev1;

                }
                var (i, err) = strconv.ParseInt(val, 10, 64);
                if (err != null) {
                    return (_addr_null!, error.As(vcsErrorf("unexpected revno from bzr log: %q", line))!);
                }
                revno = i;
                break;
            case "timestamp": 
                j = strings.Index(val, " ");
                if (j < 0) {
                    return (_addr_null!, error.As(vcsErrorf("unexpected timestamp from bzr log: %q", line))!);
                }
                var (t, err) = time.Parse("2006-01-02 15:04:05 -0700", val[(int)j + 1..]);
                if (err != null) {
                    return (_addr_null!, error.As(vcsErrorf("unexpected timestamp from bzr log: %q", line))!);
                }
                tm = t.UTC();
                break;
        }
    }    if (revno == 0 || tm.IsZero()) {
        return (_addr_null!, error.As(vcsErrorf("unexpected response from bzr log: %q", out))!);
    }
    ptr<RevInfo> info = addr(new RevInfo(Name:fmt.Sprintf("%d",revno),Short:fmt.Sprintf("%012d",revno),Time:tm,Version:rev,));
    return (_addr_info!, error.As(null!)!);
}

private static (ptr<RevInfo>, error) fossilParseStat(@string rev, @string @out) {
    ptr<RevInfo> _p0 = default!;
    error _p0 = default!;

    foreach (var (_, line) in strings.Split(out, "\n")) {
        if (strings.HasPrefix(line, "uuid:") || strings.HasPrefix(line, "hash:")) {
            var f = strings.Fields(line);
            if (len(f) != 5 || len(f[1]) != 40 || f[4] != "UTC") {
                return (_addr_null!, error.As(vcsErrorf("unexpected response from fossil info: %q", line))!);
            }
            var (t, err) = time.Parse("2006-01-02 15:04:05", f[2] + " " + f[3]);
            if (err != null) {
                return (_addr_null!, error.As(vcsErrorf("unexpected response from fossil info: %q", line))!);
            }
            var hash = f[1];
            var version = rev;
            if (strings.HasPrefix(hash, version)) {
                version = hash; // extend to full hash
            }
            ptr<RevInfo> info = addr(new RevInfo(Name:hash,Short:ShortenSHA1(hash),Time:t,Version:version,));
            return (_addr_info!, error.As(null!)!);
        }
    }    return (_addr_null!, error.As(vcsErrorf("unexpected response from fossil info: %q", out))!);
}

private partial struct limitedWriter {
    public io.Writer W;
    public long N;
    public error ErrLimitReached;
}

private static (nint, error) Write(this ptr<limitedWriter> _addr_l, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref limitedWriter l = ref _addr_l.val;

    if (l.N > 0) {
        var max = len(p);
        if (l.N < int64(max)) {
            max = int(l.N);
        }
        n, err = l.W.Write(p[..(int)max]);
        l.N -= int64(n);
        if (err != null || n >= len(p)) {
            return (n, error.As(err)!);
        }
    }
    return (n, error.As(l.ErrLimitReached)!);
}

} // end codehost_package
