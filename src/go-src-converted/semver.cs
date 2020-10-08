// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package semver implements comparison of semantic version strings.
// In this package, semantic version strings must begin with a leading "v",
// as in "v1.0.0".
//
// The general form of a semantic version string accepted by this package is
//
//    vMAJOR[.MINOR[.PATCH[-PRERELEASE][+BUILD]]]
//
// where square brackets indicate optional parts of the syntax;
// MAJOR, MINOR, and PATCH are decimal integers without extra leading zeros;
// PRERELEASE and BUILD are each a series of non-empty dot-separated identifiers
// using only alphanumeric characters and hyphens; and
// all-numeric PRERELEASE identifiers must not have leading zeros.
//
// This package follows Semantic Versioning 2.0.0 (see semver.org)
// with two exceptions. First, it requires the "v" prefix. Second, it recognizes
// vMAJOR and vMAJOR.MINOR (with no prerelease or build suffixes)
// as shorthands for vMAJOR.0.0 and vMAJOR.MINOR.0.
// package semver -- go2cs converted at 2020 October 08 04:33:04 UTC
// import "golang.org/x/mod/semver" ==> using semver = go.golang.org.x.mod.semver_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\semver\semver.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace mod
{
    public static partial class semver_package
    {
        // parsed returns the parsed form of a semantic version string.
        private partial struct parsed
        {
            public @string major;
            public @string minor;
            public @string patch;
            public @string @short;
            public @string prerelease;
            public @string build;
            public @string err;
        }

        // IsValid reports whether v is a valid semantic version string.
        public static bool IsValid(@string v)
        {
            var (_, ok) = parse(v);
            return ok;
        }

        // Canonical returns the canonical formatting of the semantic version v.
        // It fills in any missing .MINOR or .PATCH and discards build metadata.
        // Two semantic versions compare equal only if their canonical formattings
        // are identical strings.
        // The canonical invalid semantic version is the empty string.
        public static @string Canonical(@string v)
        {
            var (p, ok) = parse(v);
            if (!ok)
            {
                return "";
            }
            if (p.build != "")
            {
                return v[..len(v) - len(p.build)];
            }
            if (p.@short != "")
            {
                return v + p.@short;
            }
            return v;
        }

        // Major returns the major version prefix of the semantic version v.
        // For example, Major("v2.1.0") == "v2".
        // If v is an invalid semantic version string, Major returns the empty string.
        public static @string Major(@string v)
        {
            var (pv, ok) = parse(v);
            if (!ok)
            {
                return "";
            }
            return v[..1L + len(pv.major)];
        }

        // MajorMinor returns the major.minor version prefix of the semantic version v.
        // For example, MajorMinor("v2.1.0") == "v2.1".
        // If v is an invalid semantic version string, MajorMinor returns the empty string.
        public static @string MajorMinor(@string v)
        {
            var (pv, ok) = parse(v);
            if (!ok)
            {
                return "";
            }
            long i = 1L + len(pv.major);
            {
                var j = i + 1L + len(pv.minor);

                if (j <= len(v) && v[i] == '.' && v[i + 1L..j] == pv.minor)
                {
                    return v[..j];
                }

            }
            return v[..i] + "." + pv.minor;
        }

        // Prerelease returns the prerelease suffix of the semantic version v.
        // For example, Prerelease("v2.1.0-pre+meta") == "-pre".
        // If v is an invalid semantic version string, Prerelease returns the empty string.
        public static @string Prerelease(@string v)
        {
            var (pv, ok) = parse(v);
            if (!ok)
            {
                return "";
            }
            return pv.prerelease;
        }

        // Build returns the build suffix of the semantic version v.
        // For example, Build("v2.1.0+meta") == "+meta".
        // If v is an invalid semantic version string, Build returns the empty string.
        public static @string Build(@string v)
        {
            var (pv, ok) = parse(v);
            if (!ok)
            {
                return "";
            }
            return pv.build;
        }

        // Compare returns an integer comparing two versions according to
        // semantic version precedence.
        // The result will be 0 if v == w, -1 if v < w, or +1 if v > w.
        //
        // An invalid semantic version string is considered less than a valid one.
        // All invalid semantic version strings compare equal to each other.
        public static long Compare(@string v, @string w)
        {
            var (pv, ok1) = parse(v);
            var (pw, ok2) = parse(w);
            if (!ok1 && !ok2)
            {
                return 0L;
            }
            if (!ok1)
            {
                return -1L;
            }
            if (!ok2)
            {
                return +1L;
            }
            {
                var c__prev1 = c;

                var c = compareInt(pv.major, pw.major);

                if (c != 0L)
                {
                    return c;
                }

                c = c__prev1;

            }
            {
                var c__prev1 = c;

                c = compareInt(pv.minor, pw.minor);

                if (c != 0L)
                {
                    return c;
                }

                c = c__prev1;

            }
            {
                var c__prev1 = c;

                c = compareInt(pv.patch, pw.patch);

                if (c != 0L)
                {
                    return c;
                }

                c = c__prev1;

            }
            return comparePrerelease(pv.prerelease, pw.prerelease);
        }

        // Max canonicalizes its arguments and then returns the version string
        // that compares greater.
        public static @string Max(@string v, @string w)
        {
            v = Canonical(v);
            w = Canonical(w);
            if (Compare(v, w) > 0L)
            {
                return v;
            }
            return w;
        }

        private static (parsed, bool) parse(@string v)
        {
            parsed p = default;
            bool ok = default;

            if (v == "" || v[0L] != 'v')
            {
                p.err = "missing v prefix";
                return ;
            }
            p.major, v, ok = parseInt(v[1L..]);
            if (!ok)
            {
                p.err = "bad major version";
                return ;
            }
            if (v == "")
            {
                p.minor = "0";
                p.patch = "0";
                p.@short = ".0.0";
                return ;
            }
            if (v[0L] != '.')
            {
                p.err = "bad minor prefix";
                ok = false;
                return ;
            }
            p.minor, v, ok = parseInt(v[1L..]);
            if (!ok)
            {
                p.err = "bad minor version";
                return ;
            }
            if (v == "")
            {
                p.patch = "0";
                p.@short = ".0";
                return ;
            }
            if (v[0L] != '.')
            {
                p.err = "bad patch prefix";
                ok = false;
                return ;
            }
            p.patch, v, ok = parseInt(v[1L..]);
            if (!ok)
            {
                p.err = "bad patch version";
                return ;
            }
            if (len(v) > 0L && v[0L] == '-')
            {
                p.prerelease, v, ok = parsePrerelease(v);
                if (!ok)
                {
                    p.err = "bad prerelease";
                    return ;
                }
            }
            if (len(v) > 0L && v[0L] == '+')
            {
                p.build, v, ok = parseBuild(v);
                if (!ok)
                {
                    p.err = "bad build";
                    return ;
                }
            }
            if (v != "")
            {
                p.err = "junk on end";
                ok = false;
                return ;
            }
            ok = true;
            return ;
        }

        private static (@string, @string, bool) parseInt(@string v)
        {
            @string t = default;
            @string rest = default;
            bool ok = default;

            if (v == "")
            {
                return ;
            }
            if (v[0L] < '0' || '9' < v[0L])
            {
                return ;
            }
            long i = 1L;
            while (i < len(v) && '0' <= v[i] && v[i] <= '9')
            {
                i++;
            }

            if (v[0L] == '0' && i != 1L)
            {
                return ;
            }
            return (v[..i], v[i..], true);
        }

        private static (@string, @string, bool) parsePrerelease(@string v)
        {
            @string t = default;
            @string rest = default;
            bool ok = default;
 
            // "A pre-release version MAY be denoted by appending a hyphen and
            // a series of dot separated identifiers immediately following the patch version.
            // Identifiers MUST comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-].
            // Identifiers MUST NOT be empty. Numeric identifiers MUST NOT include leading zeroes."
            if (v == "" || v[0L] != '-')
            {
                return ;
            }
            long i = 1L;
            long start = 1L;
            while (i < len(v) && v[i] != '+')
            {
                if (!isIdentChar(v[i]) && v[i] != '.')
                {
                    return ;
                }
                if (v[i] == '.')
                {
                    if (start == i || isBadNum(v[start..i]))
                    {
                        return ;
                    }
                    start = i + 1L;
                }
                i++;
            }

            if (start == i || isBadNum(v[start..i]))
            {
                return ;
            }
            return (v[..i], v[i..], true);
        }

        private static (@string, @string, bool) parseBuild(@string v)
        {
            @string t = default;
            @string rest = default;
            bool ok = default;

            if (v == "" || v[0L] != '+')
            {
                return ;
            }
            long i = 1L;
            long start = 1L;
            while (i < len(v))
            {
                if (!isIdentChar(v[i]) && v[i] != '.')
                {
                    return ;
                }
                if (v[i] == '.')
                {
                    if (start == i)
                    {
                        return ;
                    }
                    start = i + 1L;
                }
                i++;
            }

            if (start == i)
            {
                return ;
            }
            return (v[..i], v[i..], true);
        }

        private static bool isIdentChar(byte c)
        {
            return 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '-';
        }

        private static bool isBadNum(@string v)
        {
            long i = 0L;
            while (i < len(v) && '0' <= v[i] && v[i] <= '9')
            {
                i++;
            }

            return i == len(v) && i > 1L && v[0L] == '0';
        }

        private static bool isNum(@string v)
        {
            long i = 0L;
            while (i < len(v) && '0' <= v[i] && v[i] <= '9')
            {
                i++;
            }

            return i == len(v);
        }

        private static long compareInt(@string x, @string y)
        {
            if (x == y)
            {
                return 0L;
            }
            if (len(x) < len(y))
            {
                return -1L;
            }
            if (len(x) > len(y))
            {
                return +1L;
            }
            if (x < y)
            {
                return -1L;
            }
            else
            {
                return +1L;
            }
        }

        private static long comparePrerelease(@string x, @string y)
        { 
            // "When major, minor, and patch are equal, a pre-release version has
            // lower precedence than a normal version.
            // Example: 1.0.0-alpha < 1.0.0.
            // Precedence for two pre-release versions with the same major, minor,
            // and patch version MUST be determined by comparing each dot separated
            // identifier from left to right until a difference is found as follows:
            // identifiers consisting of only digits are compared numerically and
            // identifiers with letters or hyphens are compared lexically in ASCII
            // sort order. Numeric identifiers always have lower precedence than
            // non-numeric identifiers. A larger set of pre-release fields has a
            // higher precedence than a smaller set, if all of the preceding
            // identifiers are equal.
            // Example: 1.0.0-alpha < 1.0.0-alpha.1 < 1.0.0-alpha.beta <
            // 1.0.0-beta < 1.0.0-beta.2 < 1.0.0-beta.11 < 1.0.0-rc.1 < 1.0.0."
            if (x == y)
            {
                return 0L;
            }
            if (x == "")
            {
                return +1L;
            }
            if (y == "")
            {
                return -1L;
            }
            while (x != "" && y != "")
            {
                x = x[1L..]; // skip - or .
                y = y[1L..]; // skip - or .
                @string dx = default;                @string dy = default;

                dx, x = nextIdent(x);
                dy, y = nextIdent(y);
                if (dx != dy)
                {
                    var ix = isNum(dx);
                    var iy = isNum(dy);
                    if (ix != iy)
                    {
                        if (ix)
                        {
                            return -1L;
                        }
                        else
                        {
                            return +1L;
                        }
                    }
                    if (ix)
                    {
                        if (len(dx) < len(dy))
                        {
                            return -1L;
                        }
                        if (len(dx) > len(dy))
                        {
                            return +1L;
                        }
                    }
                    if (dx < dy)
                    {
                        return -1L;
                    }
                    else
                    {
                        return +1L;
                    }
                }
            }

            if (x == "")
            {
                return -1L;
            }
            else
            {
                return +1L;
            }
        }

        private static (@string, @string) nextIdent(@string x)
        {
            @string dx = default;
            @string rest = default;

            long i = 0L;
            while (i < len(x) && x[i] != '.')
            {
                i++;
            }

            return (x[..i], x[i..]);
        }
    }
}}}}
