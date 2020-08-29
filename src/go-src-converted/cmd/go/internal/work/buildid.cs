// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package work -- go2cs converted at 2020 August 29 10:01:15 UTC
// import "cmd/go/internal/work" ==> using work = go.cmd.go.@internal.work_package
// Original source: C:\Go\src\cmd\go\internal\work\buildid.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cache = go.cmd.go.@internal.cache_package;
using cfg = go.cmd.go.@internal.cfg_package;
using load = go.cmd.go.@internal.load_package;
using str = go.cmd.go.@internal.str_package;
using buildid = go.cmd.@internal.buildid_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class work_package
    {
        // Build IDs
        //
        // Go packages and binaries are stamped with build IDs that record both
        // the action ID, which is a hash of the inputs to the action that produced
        // the packages or binary, and the content ID, which is a hash of the action
        // output, namely the archive or binary itself. The hash is the same one
        // used by the build artifact cache (see cmd/go/internal/cache), but
        // truncated when stored in packages and binaries, as the full length is not
        // needed and is a bit unwieldy. The precise form is
        //
        //    actionID/[.../]contentID
        //
        // where the actionID and contentID are prepared by hashToString below.
        // and are found by looking for the first or last slash.
        // Usually the buildID is simply actionID/contentID, but see below for an
        // exception.
        //
        // The build ID serves two primary purposes.
        //
        // 1. The action ID half allows installed packages and binaries to serve as
        // one-element cache entries. If we intend to build math.a with a given
        // set of inputs summarized in the action ID, and the installed math.a already
        // has that action ID, we can reuse the installed math.a instead of rebuilding it.
        //
        // 2. The content ID half allows the easy preparation of action IDs for steps
        // that consume a particular package or binary. The content hash of every
        // input file for a given action must be included in the action ID hash.
        // Storing the content ID in the build ID lets us read it from the file with
        // minimal I/O, instead of reading and hashing the entire file.
        // This is especially effective since packages and binaries are typically
        // the largest inputs to an action.
        //
        // Separating action ID from content ID is important for reproducible builds.
        // The compiler is compiled with itself. If an output were represented by its
        // own action ID (instead of content ID) when computing the action ID of
        // the next step in the build process, then the compiler could never have its
        // own input action ID as its output action ID (short of a miraculous hash collision).
        // Instead we use the content IDs to compute the next action ID, and because
        // the content IDs converge, so too do the action IDs and therefore the
        // build IDs and the overall compiler binary. See cmd/dist's cmdbootstrap
        // for the actual convergence sequence.
        //
        // The “one-element cache” purpose is a bit more complex for installed
        // binaries. For a binary, like cmd/gofmt, there are two steps: compile
        // cmd/gofmt/*.go into main.a, and then link main.a into the gofmt binary.
        // We do not install gofmt's main.a, only the gofmt binary. Being able to
        // decide that the gofmt binary is up-to-date means computing the action ID
        // for the final link of the gofmt binary and comparing it against the
        // already-installed gofmt binary. But computing the action ID for the link
        // means knowing the content ID of main.a, which we did not keep.
        // To sidestep this problem, each binary actually stores an expanded build ID:
        //
        //    actionID(binary)/actionID(main.a)/contentID(main.a)/contentID(binary)
        //
        // (Note that this can be viewed equivalently as:
        //
        //    actionID(binary)/buildID(main.a)/contentID(binary)
        //
        // Storing the buildID(main.a) in the middle lets the computations that care
        // about the prefix or suffix halves ignore the middle and preserves the
        // original build ID as a contiguous string.)
        //
        // During the build, when it's time to build main.a, the gofmt binary has the
        // information needed to decide whether the eventual link would produce
        // the same binary: if the action ID for main.a's inputs matches and then
        // the action ID for the link step matches when assuming the given main.a
        // content ID, then the binary as a whole is up-to-date and need not be rebuilt.
        //
        // This is all a bit complex and may be simplified once we can rely on the
        // main cache, but at least at the start we will be using the content-based
        // staleness determination without a cache beyond the usual installed
        // package and binary locations.
        private static readonly @string buildIDSeparator = "/";

        // actionID returns the action ID half of a build ID.


        // actionID returns the action ID half of a build ID.
        private static @string actionID(@string buildID)
        {
            var i = strings.Index(buildID, buildIDSeparator);
            if (i < 0L)
            {
                return buildID;
            }
            return buildID[..i];
        }

        // contentID returns the content ID half of a build ID.
        private static @string contentID(@string buildID)
        {
            return buildID[strings.LastIndex(buildID, buildIDSeparator) + 1L..];
        }

        // hashToString converts the hash h to a string to be recorded
        // in package archives and binaries as part of the build ID.
        // We use the first 96 bits of the hash and encode it in base64,
        // resulting in a 16-byte string. Because this is only used for
        // detecting the need to rebuild installed files (not for lookups
        // in the object file cache), 96 bits are sufficient to drive the
        // probability of a false "do not need to rebuild" decision to effectively zero.
        // We embed two different hashes in archives and four in binaries,
        // so cutting to 16 bytes is a significant savings when build IDs are displayed.
        // (16*4+3 = 67 bytes compared to 64*4+3 = 259 bytes for the
        // more straightforward option of printing the entire h in hex).
        private static @string hashToString(array<byte> h)
        {
            h = h.Clone();

            const @string b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

            const long chunks = 5L;

            array<byte> dst = new array<byte>(chunks * 4L);
            for (long i = 0L; i < chunks; i++)
            {
                var v = uint32(h[3L * i]) << (int)(16L) | uint32(h[3L * i + 1L]) << (int)(8L) | uint32(h[3L * i + 2L]);
                dst[4L * i + 0L] = b64[(v >> (int)(18L)) & 0x3FUL];
                dst[4L * i + 1L] = b64[(v >> (int)(12L)) & 0x3FUL];
                dst[4L * i + 2L] = b64[(v >> (int)(6L)) & 0x3FUL];
                dst[4L * i + 3L] = b64[v & 0x3FUL];
            }

            return string(dst[..]);
        }

        // toolID returns the unique ID to use for the current copy of the
        // named tool (asm, compile, cover, link).
        //
        // It is important that if the tool changes (for example a compiler bug is fixed
        // and the compiler reinstalled), toolID returns a different string, so that old
        // package archives look stale and are rebuilt (with the fixed compiler).
        // This suggests using a content hash of the tool binary, as stored in the build ID.
        //
        // Unfortunately, we can't just open the tool binary, because the tool might be
        // invoked via a wrapper program specified by -toolexec and we don't know
        // what the wrapper program does. In particular, we want "-toolexec toolstash"
        // to continue working: it does no good if "-toolexec toolstash" is executing a
        // stashed copy of the compiler but the go command is acting as if it will run
        // the standard copy of the compiler. The solution is to ask the tool binary to tell
        // us its own build ID using the "-V=full" flag now supported by all tools.
        // Then we know we're getting the build ID of the compiler that will actually run
        // during the build. (How does the compiler binary know its own content hash?
        // We store it there using updateBuildID after the standard link step.)
        //
        // A final twist is that we'd prefer to have reproducible builds for release toolchains.
        // It should be possible to cross-compile for Windows from either Linux or Mac
        // or Windows itself and produce the same binaries, bit for bit. If the tool ID,
        // which influences the action ID half of the build ID, is based on the content ID,
        // then the Linux compiler binary and Mac compiler binary will have different tool IDs
        // and therefore produce executables with different action IDs.
        // To avoids this problem, for releases we use the release version string instead
        // of the compiler binary's content hash. This assumes that all compilers built
        // on all different systems are semantically equivalent, which is of course only true
        // modulo bugs. (Producing the exact same executables also requires that the different
        // build setups agree on details like $GOROOT and file name paths, but at least the
        // tool IDs do not make it impossible.)
        private static @string toolID(this ref Builder b, @string name)
        {
            b.id.Lock();
            var id = b.toolIDCache[name];
            b.id.Unlock();

            if (id != "")
            {
                return id;
            }
            var cmdline = str.StringList(cfg.BuildToolexec, @base.Tool(name), "-V=full");
            var cmd = exec.Command(cmdline[0L], cmdline[1L..]);
            cmd.Env = @base.EnvForDir(cmd.Dir, os.Environ());
            bytes.Buffer stdout = default;            bytes.Buffer stderr = default;

            cmd.Stdout = ref stdout;
            cmd.Stderr = ref stderr;
            {
                var err = cmd.Run();

                if (err != null)
                {
                    @base.Fatalf("go tool %s: %v\n%s%s", name, err, stdout.Bytes(), stderr.Bytes());
                }

            }

            var line = stdout.String();
            var f = strings.Fields(line);
            if (len(f) < 3L || f[0L] != name || f[1L] != "version" || f[2L] == "devel" && !strings.HasPrefix(f[len(f) - 1L], "buildID="))
            {
                @base.Fatalf("go tool %s -V=full: unexpected output:\n\t%s", name, line);
            }
            if (f[2L] == "devel")
            { 
                // On the development branch, use the content ID part of the build ID.
                id = contentID(f[len(f) - 1L]);
            }
            else
            { 
                // For a release, the output is like: "compile version go1.9.1". Use the whole line.
                id = f[2L];
            }
            b.id.Lock();
            b.toolIDCache[name] = id;
            b.id.Unlock();

            return id;
        }

        // gccToolID returns the unique ID to use for a tool that is invoked
        // by the GCC driver. This is in particular gccgo, but this can also
        // be used for gcc, g++, gfortran, etc.; those tools all use the GCC
        // driver under different names. The approach used here should also
        // work for sufficiently new versions of clang. Unlike toolID, the
        // name argument is the program to run. The language argument is the
        // type of input file as passed to the GCC driver's -x option.
        //
        // For these tools we have no -V=full option to dump the build ID,
        // but we can run the tool with -v -### to reliably get the compiler proper
        // and hash that. That will work in the presence of -toolexec.
        //
        // In order to get reproducible builds for released compilers, we
        // detect a released compiler by the absence of "experimental" in the
        // --version output, and in that case we just use the version string.
        private static (@string, error) gccgoToolID(this ref Builder b, @string name, @string language)
        {
            var key = name + "." + language;
            b.id.Lock();
            var id = b.toolIDCache[key];
            b.id.Unlock();

            if (id != "")
            {
                return (id, null);
            } 

            // Invoke the driver with -### to see the subcommands and the
            // version strings. Use -x to set the language. Pretend to
            // compile an empty file on standard input.
            var cmdline = str.StringList(cfg.BuildToolexec, name, "-###", "-x", language, "-c", "-");
            var cmd = exec.Command(cmdline[0L], cmdline[1L..]);
            cmd.Env = @base.EnvForDir(cmd.Dir, os.Environ());
            var (out, err) = cmd.CombinedOutput();
            if (err != null)
            {
                return ("", fmt.Errorf("%s: %v; output: %q", name, err, out));
            }
            @string version = "";
            var lines = strings.Split(string(out), "\n");
            {
                var line__prev1 = line;

                foreach (var (_, __line) in lines)
                {
                    line = __line;
                    {
                        var fields__prev1 = fields;

                        var fields = strings.Fields(line);

                        if (len(fields) > 1L && fields[1L] == "version")
                        {
                            version = line;
                            break;
                        }

                        fields = fields__prev1;

                    }
                }

                line = line__prev1;
            }

            if (version == "")
            {
                return ("", fmt.Errorf("%s: can not find version number in %q", name, out));
            }
            if (!strings.Contains(version, "experimental"))
            { 
                // This is a release. Use this line as the tool ID.
                id = version;
            }
            else
            { 
                // This is a development version. The first line with
                // a leading space is the compiler proper.
                @string compiler = "";
                {
                    var line__prev1 = line;

                    foreach (var (_, __line) in lines)
                    {
                        line = __line;
                        if (len(line) > 1L && line[0L] == ' ')
                        {
                            compiler = line;
                            break;
                        }
                    }

                    line = line__prev1;
                }

                if (compiler == "")
                {
                    return ("", fmt.Errorf("%s: can not find compilation command in %q", name, out));
                }
                fields = strings.Fields(compiler);
                if (len(fields) == 0L)
                {
                    return ("", fmt.Errorf("%s: compilation command confusion %q", name, out));
                }
                var exe = fields[0L];
                if (!strings.ContainsAny(exe, "/\\"))
                {
                    {
                        var (lp, err) = exec.LookPath(exe);

                        if (err == null)
                        {
                            exe = lp;
                        }

                    }
                }
                {
                    var (_, err) = os.Stat(exe);

                    if (err != null)
                    {
                        return ("", fmt.Errorf("%s: can not find compiler %q: %v; output %q", name, exe, err, out));
                    }

                }
                id = b.fileHash(exe);
            }
            b.id.Lock();
            b.toolIDCache[name] = id;
            b.id.Unlock();

            return (id, null);
        }

        // gccgoBuildIDELFFile creates an assembler file that records the
        // action's build ID in an SHF_EXCLUDE section.
        private static (@string, error) gccgoBuildIDELFFile(this ref Builder b, ref Action a)
        {
            var sfile = a.Objdir + "_buildid.s";

            bytes.Buffer buf = default;
            fmt.Fprintf(ref buf, "\t" + ".section .go.buildid,\"e\"" + "\n");
            fmt.Fprintf(ref buf, "\t.byte ");
            for (long i = 0L; i < len(a.buildID); i++)
            {
                if (i > 0L)
                {
                    if (i % 8L == 0L)
                    {
                        fmt.Fprintf(ref buf, "\n\t.byte ");
                    }
                    else
                    {
                        fmt.Fprintf(ref buf, ",");
                    }
                }
                fmt.Fprintf(ref buf, "%#02x", a.buildID[i]);
            }

            fmt.Fprintf(ref buf, "\n");
            fmt.Fprintf(ref buf, "\t" + ".section .note.GNU-stack,\"\",@progbits" + "\n");
            fmt.Fprintf(ref buf, "\t" + ".section .note.GNU-split-stack,\"\",@progbits" + "\n");

            if (cfg.BuildN || cfg.BuildX)
            {
                foreach (var (_, line) in bytes.Split(buf.Bytes(), (slice<byte>)"\n"))
                {
                    b.Showcmd("", "echo '%s' >> %s", line, sfile);
                }
                if (cfg.BuildN)
                {
                    return (sfile, null);
                }
            }
            {
                var err = ioutil.WriteFile(sfile, buf.Bytes(), 0666L);

                if (err != null)
                {
                    return ("", err);
                }

            }

            return (sfile, null);
        }

        // buildID returns the build ID found in the given file.
        // If no build ID is found, buildID returns the content hash of the file.
        private static @string buildID(this ref Builder b, @string file)
        {
            b.id.Lock();
            var id = b.buildIDCache[file];
            b.id.Unlock();

            if (id != "")
            {
                return id;
            }
            var (id, err) = buildid.ReadFile(file);
            if (err != null)
            {
                id = b.fileHash(file);
            }
            b.id.Lock();
            b.buildIDCache[file] = id;
            b.id.Unlock();

            return id;
        }

        // fileHash returns the content hash of the named file.
        private static @string fileHash(this ref Builder b, @string file)
        {
            var (sum, err) = cache.FileHash(file);
            if (err != null)
            {
                return "";
            }
            return hashToString(sum);
        }

        // useCache tries to satisfy the action a, which has action ID actionHash,
        // by using a cached result from an earlier build. At the moment, the only
        // cached result is the installed package or binary at target.
        // If useCache decides that the cache can be used, it sets a.buildID
        // and a.built for use by parent actions and then returns true.
        // Otherwise it sets a.buildID to a temporary build ID for use in the build
        // and returns false. When useCache returns false the expectation is that
        // the caller will build the target and then call updateBuildID to finish the
        // build ID computation.
        // When useCache returns false, it may have initiated buffering of output
        // during a's work. The caller should defer b.flushOutput(a), to make sure
        // that flushOutput is eventually called regardless of whether the action
        // succeeds. The flushOutput call must happen after updateBuildID.
        private static bool useCache(this ref Builder b, ref Action a, ref load.Package p, cache.ActionID actionHash, @string target)
        { 
            // The second half of the build ID here is a placeholder for the content hash.
            // It's important that the overall buildID be unlikely verging on impossible
            // to appear in the output by chance, but that should be taken care of by
            // the actionID half; if it also appeared in the input that would be like an
            // engineered 96-bit partial SHA256 collision.
            a.actionID = actionHash;
            var actionID = hashToString(actionHash);
            var contentID = actionID; // temporary placeholder, likely unique
            a.buildID = actionID + buildIDSeparator + contentID; 

            // Executable binaries also record the main build ID in the middle.
            // See "Build IDs" comment above.
            if (a.Mode == "link")
            {
                var mainpkg = a.Deps[0L];
                a.buildID = actionID + buildIDSeparator + mainpkg.buildID + buildIDSeparator + contentID;
            } 

            // Check to see if target exists and matches the expected action ID.
            // If so, it's up to date and we can reuse it instead of rebuilding it.
            @string buildID = default;
            if (target != "" && !cfg.BuildA)
            {
                error err = default;
                buildID, err = buildid.ReadFile(target);
                if (err != null && b.ComputeStaleOnly)
                {
                    if (p != null && !p.Stale)
                    {
                        p.Stale = true;
                        p.StaleReason = "target missing";
                    }
                    return true;
                }
                if (strings.HasPrefix(buildID, actionID + buildIDSeparator))
                {
                    a.buildID = buildID;
                    a.built = target; 
                    // Poison a.Target to catch uses later in the build.
                    a.Target = "DO NOT USE - " + a.Mode;
                    return true;
                }
            } 

            // Special case for building a main package: if the only thing we
            // want the package for is to link a binary, and the binary is
            // already up-to-date, then to avoid a rebuild, report the package
            // as up-to-date as well. See "Build IDs" comment above.
            // TODO(rsc): Rewrite this code to use a TryCache func on the link action.
            if (target != "" && !cfg.BuildA && a.Mode == "build" && len(a.triggers) == 1L && a.triggers[0L].Mode == "link")
            {
                var (buildID, err) = buildid.ReadFile(target);
                if (err == null)
                {
                    var id = strings.Split(buildID, buildIDSeparator);
                    if (len(id) == 4L && id[1L] == actionID)
                    { 
                        // Temporarily assume a.buildID is the package build ID
                        // stored in the installed binary, and see if that makes
                        // the upcoming link action ID a match. If so, report that
                        // we built the package, safe in the knowledge that the
                        // link step will not ask us for the actual package file.
                        // Note that (*Builder).LinkAction arranged that all of
                        // a.triggers[0]'s dependencies other than a are also
                        // dependencies of a, so that we can be sure that,
                        // other than a.buildID, b.linkActionID is only accessing
                        // build IDs of completed actions.
                        var oldBuildID = a.buildID;
                        a.buildID = id[1L] + buildIDSeparator + id[2L];
                        var linkID = hashToString(b.linkActionID(a.triggers[0L]));
                        if (id[0L] == linkID)
                        { 
                            // Poison a.Target to catch uses later in the build.
                            a.Target = "DO NOT USE - main build pseudo-cache Target";
                            a.built = "DO NOT USE - main build pseudo-cache built";
                            return true;
                        } 
                        // Otherwise restore old build ID for main build.
                        a.buildID = oldBuildID;
                    }
                }
            } 

            // Special case for linking a test binary: if the only thing we
            // want the binary for is to run the test, and the test result is cached,
            // then to avoid the link step, report the link as up-to-date.
            // We avoid the nested build ID problem in the previous special case
            // by recording the test results in the cache under the action ID half.
            if (!cfg.BuildA && len(a.triggers) == 1L && a.triggers[0L].TryCache != null && a.triggers[0L].TryCache(b, a.triggers[0L]))
            {
                a.Target = "DO NOT USE -  pseudo-cache Target";
                a.built = "DO NOT USE - pseudo-cache built";
                return true;
            }
            if (b.ComputeStaleOnly)
            { 
                // Invoked during go list only to compute and record staleness.
                {
                    var p = a.Package;

                    if (p != null && !p.Stale)
                    {
                        p.Stale = true;
                        if (cfg.BuildA)
                        {
                            p.StaleReason = "build -a flag in use";
                        }
                        else
                        {
                            p.StaleReason = "build ID mismatch";
                            foreach (var (_, p1) in p.Internal.Imports)
                            {
                                if (p1.Stale && p1.StaleReason != "")
                                {
                                    if (strings.HasPrefix(p1.StaleReason, "stale dependency: "))
                                    {
                                        p.StaleReason = p1.StaleReason;
                                        break;
                                    }
                                    if (strings.HasPrefix(p.StaleReason, "build ID mismatch"))
                                    {
                                        p.StaleReason = "stale dependency: " + p1.ImportPath;
                                    }
                                }
                            }
                        }
                    }

                }
                return true;
            } 

            // Check the build artifact cache.
            // We treat hits in this cache as being "stale" for the purposes of go list
            // (in effect, "stale" means whether p.Target is up-to-date),
            // but we're still happy to use results from the build artifact cache.
            {
                var c = cache.Default();

                if (c != null)
                {
                    if (!cfg.BuildA)
                    {
                        var (entry, err) = c.Get(actionHash);
                        if (err == null)
                        {
                            var file = c.OutputFile(entry.OutputID);
                            var (info, err1) = os.Stat(file);
                            var (buildID, err2) = buildid.ReadFile(file);
                            if (err1 == null && err2 == null && info.Size() == entry.Size)
                            {
                                var (stdout, stdoutEntry, err) = c.GetBytes(cache.Subkey(a.actionID, "stdout"));
                                if (err == null)
                                {
                                    if (len(stdout) > 0L)
                                    {
                                        if (cfg.BuildX || cfg.BuildN)
                                        {
                                            b.Showcmd("", "%s  # internal", joinUnambiguously(str.StringList("cat", c.OutputFile(stdoutEntry.OutputID))));
                                        }
                                        if (!cfg.BuildN)
                                        {
                                            b.Print(string(stdout));
                                        }
                                    }
                                    a.built = file;
                                    a.Target = "DO NOT USE - using cache";
                                    a.buildID = buildID;
                                    return true;
                                }
                            }
                        }
                    } 

                    // Begin saving output for later writing to cache.
                    a.output = new slice<byte>(new byte[] {  });
                }

            }

            return false;
        }

        // flushOutput flushes the output being queued in a.
        private static void flushOutput(this ref Builder b, ref Action a)
        {
            b.Print(string(a.output));
            a.output = null;
        }

        // updateBuildID updates the build ID in the target written by action a.
        // It requires that useCache was called for action a and returned false,
        // and that the build was then carried out and given the temporary
        // a.buildID to record as the build ID in the resulting package or binary.
        // updateBuildID computes the final content ID and updates the build IDs
        // in the binary.
        //
        // Keep in sync with src/cmd/buildid/buildid.go
        private static error updateBuildID(this ref Builder _b, ref Action _a, @string target, bool rewrite) => func(_b, _a, (ref Builder b, ref Action a, Defer _, Panic panic, Recover __) =>
        {
            if (cfg.BuildX || cfg.BuildN)
            {
                if (rewrite)
                {
                    b.Showcmd("", "%s # internal", joinUnambiguously(str.StringList(@base.Tool("buildid"), "-w", target)));
                }
                if (cfg.BuildN)
                {
                    return error.As(null);
                }
            } 

            // Find occurrences of old ID and compute new content-based ID.
            var (r, err) = os.Open(target);
            if (err != null)
            {
                return error.As(err);
            }
            var (matches, hash, err) = buildid.FindAndHash(r, a.buildID, 0L);
            r.Close();
            if (err != null)
            {
                return error.As(err);
            }
            var newID = a.buildID[..strings.LastIndex(a.buildID, buildIDSeparator)] + buildIDSeparator + hashToString(hash);
            if (len(newID) != len(a.buildID))
            {
                return error.As(fmt.Errorf("internal error: build ID length mismatch %q vs %q", a.buildID, newID));
            } 

            // Replace with new content-based ID.
            a.buildID = newID;
            if (len(matches) == 0L)
            { 
                // Assume the user specified -buildid= to override what we were going to choose.
                return error.As(null);
            }
            if (rewrite)
            {
                var (w, err) = os.OpenFile(target, os.O_WRONLY, 0L);
                if (err != null)
                {
                    return error.As(err);
                }
                err = buildid.Rewrite(w, matches, newID);
                if (err != null)
                {
                    w.Close();
                    return error.As(err);
                }
                {
                    var err = w.Close();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            } 

            // Cache package builds, but not binaries (link steps).
            // The expectation is that binaries are not reused
            // nearly as often as individual packages, and they're
            // much larger, so the cache-footprint-to-utility ratio
            // of binaries is much lower for binaries.
            // Not caching the link step also makes sure that repeated "go run" at least
            // always rerun the linker, so that they don't get too fast.
            // (We don't want people thinking go is a scripting language.)
            // Note also that if we start caching binaries, then we will
            // copy the binaries out of the cache to run them, and then
            // that will mean the go process is itself writing a binary
            // and then executing it, so we will need to defend against
            // ETXTBSY problems as discussed in exec.go and golang.org/issue/22220.
            {
                var c = cache.Default();

                if (c != null && a.Mode == "build")
                {
                    (r, err) = os.Open(target);
                    if (err == null)
                    {
                        if (a.output == null)
                        {
                            panic("internal error: a.output not set");
                        }
                        var (outputID, _, err) = c.Put(a.actionID, r);
                        if (err == null && cfg.BuildX)
                        {
                            b.Showcmd("", "%s # internal", joinUnambiguously(str.StringList("cp", target, c.OutputFile(outputID))));
                        }
                        c.PutBytes(cache.Subkey(a.actionID, "stdout"), a.output);
                        r.Close();
                    }
                }

            }

            return error.As(null);
        });
    }
}}}}
