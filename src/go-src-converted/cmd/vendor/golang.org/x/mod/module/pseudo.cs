// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Pseudo-versions
//
// Code authors are expected to tag the revisions they want users to use,
// including prereleases. However, not all authors tag versions at all,
// and not all commits a user might want to try will have tags.
// A pseudo-version is a version with a special form that allows us to
// address an untagged commit and order that version with respect to
// other versions we might encounter.
//
// A pseudo-version takes one of the general forms:
//
//    (1) vX.0.0-yyyymmddhhmmss-abcdef123456
//    (2) vX.Y.(Z+1)-0.yyyymmddhhmmss-abcdef123456
//    (3) vX.Y.(Z+1)-0.yyyymmddhhmmss-abcdef123456+incompatible
//    (4) vX.Y.Z-pre.0.yyyymmddhhmmss-abcdef123456
//    (5) vX.Y.Z-pre.0.yyyymmddhhmmss-abcdef123456+incompatible
//
// If there is no recently tagged version with the right major version vX,
// then form (1) is used, creating a space of pseudo-versions at the bottom
// of the vX version range, less than any tagged version, including the unlikely v0.0.0.
//
// If the most recent tagged version before the target commit is vX.Y.Z or vX.Y.Z+incompatible,
// then the pseudo-version uses form (2) or (3), making it a prerelease for the next
// possible semantic version after vX.Y.Z. The leading 0 segment in the prerelease string
// ensures that the pseudo-version compares less than possible future explicit prereleases
// like vX.Y.(Z+1)-rc1 or vX.Y.(Z+1)-1.
//
// If the most recent tagged version before the target commit is vX.Y.Z-pre or vX.Y.Z-pre+incompatible,
// then the pseudo-version uses form (4) or (5), making it a slightly later prerelease.

// package module -- go2cs converted at 2022 March 06 23:26:08 UTC
// import "cmd/vendor/golang.org/x/mod/module" ==> using module = go.cmd.vendor.golang.org.x.mod.module_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\module\pseudo.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using time = go.time_package;

using lazyregexp = go.golang.org.x.mod.@internal.lazyregexp_package;
using semver = go.golang.org.x.mod.semver_package;

namespace go.cmd.vendor.golang.org.x.mod;

public static partial class module_package {

private static var pseudoVersionRE = lazyregexp.New("^v[0-9]+\\.(0\\.0-|\\d+\\.\\d+-([^+]*\\.)?0\\.)\\d{14}-[A-Za-z0-9]+(\\+[0-9A-Za-z-]+(\\.[0-" +
    "9A-Za-z-]+)*)?$");

public static readonly @string PseudoVersionTimestampFormat = "20060102150405";

// PseudoVersion returns a pseudo-version for the given major version ("v1")
// preexisting older tagged version ("" or "v1.2.3" or "v1.2.3-pre"), revision time,
// and revision identifier (usually a 12-byte commit hash prefix).


// PseudoVersion returns a pseudo-version for the given major version ("v1")
// preexisting older tagged version ("" or "v1.2.3" or "v1.2.3-pre"), revision time,
// and revision identifier (usually a 12-byte commit hash prefix).
public static @string PseudoVersion(@string major, @string older, time.Time t, @string rev) {
    if (major == "") {
        major = "v0";
    }
    var segment = fmt.Sprintf("%s-%s", t.UTC().Format(PseudoVersionTimestampFormat), rev);
    var build = semver.Build(older);
    older = semver.Canonical(older);
    if (older == "") {
        return major + ".0.0-" + segment; // form (1)
    }
    if (semver.Prerelease(older) != "") {
        return older + ".0." + segment + build; // form (4), (5)
    }
    var i = strings.LastIndex(older, ".") + 1;
    var v = older[..(int)i];
    var patch = older[(int)i..]; 

    // Reassemble.
    return v + incDecimal(patch) + "-0." + segment + build;

}

// ZeroPseudoVersion returns a pseudo-version with a zero timestamp and
// revision, which may be used as a placeholder.
public static @string ZeroPseudoVersion(@string major) {
    return PseudoVersion(major, "", new time.Time(), "000000000000");
}

// incDecimal returns the decimal string incremented by 1.
private static @string incDecimal(@string @decimal) { 
    // Scan right to left turning 9s to 0s until you find a digit to increment.
    slice<byte> digits = (slice<byte>)decimal;
    var i = len(digits) - 1;
    while (i >= 0 && digits[i] == '9') {
        digits[i] = '0';
        i--;
    }
    if (i >= 0) {
        digits[i]++;
    }
    else
 { 
        // digits is all zeros
        digits[0] = '1';
        digits = append(digits, '0');

    }
    return string(digits);

}

// decDecimal returns the decimal string decremented by 1, or the empty string
// if the decimal is all zeroes.
private static @string decDecimal(@string @decimal) { 
    // Scan right to left turning 0s to 9s until you find a digit to decrement.
    slice<byte> digits = (slice<byte>)decimal;
    var i = len(digits) - 1;
    while (i >= 0 && digits[i] == '0') {
        digits[i] = '9';
        i--;
    }
    if (i < 0) { 
        // decimal is all zeros
        return "";

    }
    if (i == 0 && digits[i] == '1' && len(digits) > 1) {
        digits = digits[(int)1..];
    }
    else
 {
        digits[i]--;
    }
    return string(digits);

}

// IsPseudoVersion reports whether v is a pseudo-version.
public static bool IsPseudoVersion(@string v) {
    return strings.Count(v, "-") >= 2 && semver.IsValid(v) && pseudoVersionRE.MatchString(v);
}

// IsZeroPseudoVersion returns whether v is a pseudo-version with a zero base,
// timestamp, and revision, as returned by ZeroPseudoVersion.
public static bool IsZeroPseudoVersion(@string v) {
    return v == ZeroPseudoVersion(semver.Major(v));
}

// PseudoVersionTime returns the time stamp of the pseudo-version v.
// It returns an error if v is not a pseudo-version or if the time stamp
// embedded in the pseudo-version is not a valid time.
public static (time.Time, error) PseudoVersionTime(@string v) {
    time.Time _p0 = default;
    error _p0 = default!;

    var (_, timestamp, _, _, err) = parsePseudoVersion(v);
    if (err != null) {
        return (new time.Time(), error.As(err)!);
    }
    var (t, err) = time.Parse("20060102150405", timestamp);
    if (err != null) {
        return (new time.Time(), error.As(addr(new InvalidVersionError(Version:v,Pseudo:true,Err:fmt.Errorf("malformed time %q",timestamp),))!)!);
    }
    return (t, error.As(null!)!);

}

// PseudoVersionRev returns the revision identifier of the pseudo-version v.
// It returns an error if v is not a pseudo-version.
public static (@string, error) PseudoVersionRev(@string v) {
    @string rev = default;
    error err = default!;

    _, _, rev, _, err = parsePseudoVersion(v);
    return ;
}

// PseudoVersionBase returns the canonical parent version, if any, upon which
// the pseudo-version v is based.
//
// If v has no parent version (that is, if it is "vX.0.0-[…]"),
// PseudoVersionBase returns the empty string and a nil error.
public static (@string, error) PseudoVersionBase(@string v) => func((_, panic, _) => {
    @string _p0 = default;
    error _p0 = default!;

    var (base, _, _, build, err) = parsePseudoVersion(v);
    if (err != null) {
        return ("", error.As(err)!);
    }
    {
        var pre = semver.Prerelease(base);

        switch (pre) {
            case "": 
                // vX.0.0-yyyymmddhhmmss-abcdef123456 → ""
                if (build != "") { 
                    // Pseudo-versions of the form vX.0.0-yyyymmddhhmmss-abcdef123456+incompatible
                    // are nonsensical: the "vX.0.0-" prefix implies that there is no parent tag,
                    // but the "+incompatible" suffix implies that the major version of
                    // the parent tag is not compatible with the module's import path.
                    //
                    // There are a few such entries in the index generated by proxy.golang.org,
                    // but we believe those entries were generated by the proxy itself.
                    return ("", error.As(addr(new InvalidVersionError(Version:v,Pseudo:true,Err:fmt.Errorf("lacks base version, but has build metadata %q",build),))!)!);

                }

                return ("", error.As(null!)!);

                break;
            case "-0": 
                // vX.Y.(Z+1)-0.yyyymmddhhmmss-abcdef123456 → vX.Y.Z
                // vX.Y.(Z+1)-0.yyyymmddhhmmss-abcdef123456+incompatible → vX.Y.Z+incompatible
                base = strings.TrimSuffix(base, pre);
                var i = strings.LastIndexByte(base, '.');
                if (i < 0) {
                    panic("base from parsePseudoVersion missing patch number: " + base);
                }
                var patch = decDecimal(base[(int)i + 1..]);
                if (patch == "") { 
                    // vX.0.0-0 is invalid, but has been observed in the wild in the index
                    // generated by requests to proxy.golang.org.
                    //
                    // NOTE(bcmills): I cannot find a historical bug that accounts for
                    // pseudo-versions of this form, nor have I seen such versions in any
                    // actual go.mod files. If we find actual examples of this form and a
                    // reasonable theory of how they came into existence, it seems fine to
                    // treat them as equivalent to vX.0.0 (especially since the invalid
                    // pseudo-versions have lower precedence than the real ones). For now, we
                    // reject them.
                    return ("", error.As(addr(new InvalidVersionError(Version:v,Pseudo:true,Err:fmt.Errorf("version before %s would have negative patch number",base),))!)!);

                }

                return (base[..(int)i + 1] + patch + build, error.As(null!)!);

                break;
            default: 
                // vX.Y.Z-pre.0.yyyymmddhhmmss-abcdef123456 → vX.Y.Z-pre
                // vX.Y.Z-pre.0.yyyymmddhhmmss-abcdef123456+incompatible → vX.Y.Z-pre+incompatible
                if (!strings.HasSuffix(base, ".0")) {
                    panic("base from parsePseudoVersion missing \".0\" before date: " + base);
                }
                return (strings.TrimSuffix(base, ".0") + build, error.As(null!)!);
                break;
        }
    }

});

private static var errPseudoSyntax = errors.New("syntax error");

private static (@string, @string, @string, @string, error) parsePseudoVersion(@string v) {
    @string @base = default;
    @string timestamp = default;
    @string rev = default;
    @string build = default;
    error err = default!;

    if (!IsPseudoVersion(v)) {
        return ("", "", "", "", error.As(addr(new InvalidVersionError(Version:v,Pseudo:true,Err:errPseudoSyntax,))!)!);
    }
    build = semver.Build(v);
    v = strings.TrimSuffix(v, build);
    var j = strings.LastIndex(v, "-");
    (v, rev) = (v[..(int)j], v[(int)j + 1..]);    var i = strings.LastIndex(v, "-");
    {
        var j__prev1 = j;

        j = strings.LastIndex(v, ".");

        if (j > i) {
            base = v[..(int)j]; // "vX.Y.Z-pre.0" or "vX.Y.(Z+1)-0"
            timestamp = v[(int)j + 1..];

        }
        else
 {
            base = v[..(int)i]; // "vX.0.0"
            timestamp = v[(int)i + 1..];

        }
        j = j__prev1;

    }

    return (base, timestamp, rev, build, error.As(null!)!);

}

} // end module_package
