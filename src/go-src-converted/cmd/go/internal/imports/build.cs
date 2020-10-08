// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Copied from Go distribution src/go/build/build.go, syslist.go

// package imports -- go2cs converted at 2020 October 08 04:34:03 UTC
// import "cmd/go/internal/imports" ==> using imports = go.cmd.go.@internal.imports_package
// Original source: C:\Go\src\cmd\go\internal\imports\build.go
using bytes = go.bytes_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class imports_package
    {
        private static slice<byte> slashslash = (slice<byte>)"//";

        // ShouldBuild reports whether it is okay to use this file,
        // The rule is that in the file's leading run of // comments
        // and blank lines, which must be followed by a blank line
        // (to avoid including a Go package clause doc comment),
        // lines beginning with '// +build' are taken as build directives.
        //
        // The file is accepted only if each such line lists something
        // matching the file. For example:
        //
        //    // +build windows linux
        //
        // marks the file as applicable only on Windows and Linux.
        //
        // If tags["*"] is true, then ShouldBuild will consider every
        // build tag except "ignore" to be both true and false for
        // the purpose of satisfying build tags, in order to estimate
        // (conservatively) whether a file could ever possibly be used
        // in any build.
        //
        public static bool ShouldBuild(slice<byte> content, map<@string, bool> tags)
        { 
            // Pass 1. Identify leading run of // comments and blank lines,
            // which must be followed by a blank line.
            long end = 0L;
            var p = content;
            while (len(p) > 0L)
            {
                var line = p;
                {
                    var i__prev1 = i;

                    var i = bytes.IndexByte(line, '\n');

                    if (i >= 0L)
                    {
                        line = line[..i];
                        p = p[i + 1L..];

                    }
                    else
                    {
                        p = p[len(p)..];
                    }

                    i = i__prev1;

                }

                line = bytes.TrimSpace(line);
                if (len(line) == 0L)
                { // Blank line
                    end = len(content) - len(p);
                    continue;

                }

                if (!bytes.HasPrefix(line, slashslash))
                { // Not comment line
                    break;

                }

            }

            content = content[..end]; 

            // Pass 2.  Process each line in the run.
            p = content;
            var allok = true;
            while (len(p) > 0L)
            {
                line = p;
                {
                    var i__prev1 = i;

                    i = bytes.IndexByte(line, '\n');

                    if (i >= 0L)
                    {
                        line = line[..i];
                        p = p[i + 1L..];

                    }
                    else
                    {
                        p = p[len(p)..];
                    }

                    i = i__prev1;

                }

                line = bytes.TrimSpace(line);
                if (!bytes.HasPrefix(line, slashslash))
                {
                    continue;
                }

                line = bytes.TrimSpace(line[len(slashslash)..]);
                if (len(line) > 0L && line[0L] == '+')
                { 
                    // Looks like a comment +line.
                    var f = strings.Fields(string(line));
                    if (f[0L] == "+build")
                    {
                        var ok = false;
                        foreach (var (_, tok) in f[1L..])
                        {
                            if (matchTags(tok, tags))
                            {
                                ok = true;
                            }

                        }
                        if (!ok)
                        {
                            allok = false;
                        }

                    }

                }

            }


            return allok;

        }

        // matchTags reports whether the name is one of:
        //
        //    tag (if tags[tag] is true)
        //    !tag (if tags[tag] is false)
        //    a comma-separated list of any of these
        //
        private static bool matchTags(@string name, map<@string, bool> tags)
        {
            if (name == "")
            {
                return false;
            }

            {
                var i = strings.Index(name, ",");

                if (i >= 0L)
                { 
                    // comma-separated list
                    var ok1 = matchTags(name[..i], tags);
                    var ok2 = matchTags(name[i + 1L..], tags);
                    return ok1 && ok2;

                }

            }

            if (strings.HasPrefix(name, "!!"))
            { // bad syntax, reject always
                return false;

            }

            if (strings.HasPrefix(name, "!"))
            { // negation
                return len(name) > 1L && matchTag(name[1L..], tags, false);

            }

            return matchTag(name, tags, true);

        }

        // matchTag reports whether the tag name is valid and satisfied by tags[name]==want.
        private static bool matchTag(@string name, map<@string, bool> tags, bool want)
        { 
            // Tags must be letters, digits, underscores or dots.
            // Unlike in Go identifiers, all digits are fine (e.g., "386").
            foreach (var (_, c) in name)
            {
                if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.')
                {
                    return false;
                }

            }
            if (tags["*"] && name != "" && name != "ignore")
            { 
                // Special case for gathering all possible imports:
                // if we put * in the tags map then all tags
                // except "ignore" are considered both present and not
                // (so we return true no matter how 'want' is set).
                return true;

            }

            var have = tags[name];
            if (name == "linux")
            {
                have = have || tags["android"];
            }

            if (name == "solaris")
            {
                have = have || tags["illumos"];
            }

            return have == want;

        }

        // MatchFile returns false if the name contains a $GOOS or $GOARCH
        // suffix which does not match the current system.
        // The recognized name formats are:
        //
        //     name_$(GOOS).*
        //     name_$(GOARCH).*
        //     name_$(GOOS)_$(GOARCH).*
        //     name_$(GOOS)_test.*
        //     name_$(GOARCH)_test.*
        //     name_$(GOOS)_$(GOARCH)_test.*
        //
        // Exceptions:
        //     if GOOS=android, then files with GOOS=linux are also matched.
        //     if GOOS=illumos, then files with GOOS=solaris are also matched.
        //
        // If tags["*"] is true, then MatchFile will consider all possible
        // GOOS and GOARCH to be available and will consequently
        // always return true.
        public static bool MatchFile(@string name, map<@string, bool> tags)
        {
            if (tags["*"])
            {
                return true;
            }

            {
                var dot = strings.Index(name, ".");

                if (dot != -1L)
                {
                    name = name[..dot];
                } 

                // Before Go 1.4, a file called "linux.go" would be equivalent to having a
                // build tag "linux" in that file. For Go 1.4 and beyond, we require this
                // auto-tagging to apply only to files with a non-empty prefix, so
                // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
                // systems, such as android, to arrive without breaking existing code with
                // innocuous source code in "android.go". The easiest fix: cut everything
                // in the name before the initial _.

            } 

            // Before Go 1.4, a file called "linux.go" would be equivalent to having a
            // build tag "linux" in that file. For Go 1.4 and beyond, we require this
            // auto-tagging to apply only to files with a non-empty prefix, so
            // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
            // systems, such as android, to arrive without breaking existing code with
            // innocuous source code in "android.go". The easiest fix: cut everything
            // in the name before the initial _.
            var i = strings.Index(name, "_");
            if (i < 0L)
            {
                return true;
            }

            name = name[i..]; // ignore everything before first _

            var l = strings.Split(name, "_");
            {
                var n__prev1 = n;

                var n = len(l);

                if (n > 0L && l[n - 1L] == "test")
                {
                    l = l[..n - 1L];
                }

                n = n__prev1;

            }

            n = len(l);
            if (n >= 2L && KnownOS[l[n - 2L]] && KnownArch[l[n - 1L]])
            {
                return matchTag(l[n - 2L], tags, true) && matchTag(l[n - 1L], tags, true);
            }

            if (n >= 1L && KnownOS[l[n - 1L]])
            {
                return matchTag(l[n - 1L], tags, true);
            }

            if (n >= 1L && KnownArch[l[n - 1L]])
            {
                return matchTag(l[n - 1L], tags, true);
            }

            return true;

        }

        public static map KnownOS = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"aix":true,"android":true,"darwin":true,"dragonfly":true,"freebsd":true,"hurd":true,"illumos":true,"js":true,"linux":true,"nacl":true,"netbsd":true,"openbsd":true,"plan9":true,"solaris":true,"windows":true,"zos":true,};

        public static map KnownArch = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"386":true,"amd64":true,"amd64p32":true,"arm":true,"armbe":true,"arm64":true,"arm64be":true,"ppc64":true,"ppc64le":true,"mips":true,"mipsle":true,"mips64":true,"mips64le":true,"mips64p32":true,"mips64p32le":true,"ppc":true,"riscv":true,"riscv64":true,"s390":true,"s390x":true,"sparc":true,"sparc64":true,"wasm":true,};
    }
}}}}
