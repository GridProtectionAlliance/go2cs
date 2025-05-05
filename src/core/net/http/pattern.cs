// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Patterns for ServeMux routing.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using url = net.url_package;
using strings = strings_package;
using unicode = unicode_package;

partial class http_package {

// A pattern is something that can be matched against an HTTP request.
// It has an optional method, an optional host, and a path.
[GoType] partial struct pattern {
    internal @string str; // original string
    internal @string method;
    internal @string host;
    // The representation of a path differs from the surface syntax, which
    // simplifies most algorithms.
    //
    // Paths ending in '/' are represented with an anonymous "..." wildcard.
    // For example, the path "a/" is represented as a literal segment "a" followed
    // by a segment with multi==true.
    //
    // Paths ending in "{$}" are represented with the literal segment "/".
    // For example, the path "a/{$}" is represented as a literal segment "a" followed
    // by a literal segment "/".
    internal slice<segment> segments;
    internal @string loc; // source location of registering call, for helpful messages
}

[GoRecv] internal static @string String(this ref pattern p) {
    return p.str;
}

[GoRecv] internal static segment lastSegment(this ref pattern p) {
    return p.segments[len(p.segments) - 1];
}

// A segment is a pattern piece that matches one or more path segments, or
// a trailing slash.
//
// If wild is false, it matches a literal segment, or, if s == "/", a trailing slash.
// Examples:
//
//	"a" => segment{s: "a"}
//	"/{$}" => segment{s: "/"}
//
// If wild is true and multi is false, it matches a single path segment.
// Example:
//
//	"{x}" => segment{s: "x", wild: true}
//
// If both wild and multi are true, it matches all remaining path segments.
// Example:
//
//	"{rest...}" => segment{s: "rest", wild: true, multi: true}
[GoType] partial struct segment {
    internal @string s; // literal or wildcard name or "/" for "/{$}".
    internal bool wild;
    internal bool multi; // "..." wildcard
}

// parsePattern parses a string into a Pattern.
// The string's syntax is
//
//	[METHOD] [HOST]/[PATH]
//
// where:
//   - METHOD is an HTTP method
//   - HOST is a hostname
//   - PATH consists of slash-separated segments, where each segment is either
//     a literal or a wildcard of the form "{name}", "{name...}", or "{$}".
//
// METHOD, HOST and PATH are all optional; that is, the string can be "/".
// If METHOD is present, it must be followed by at least one space or tab.
// Wildcard names must be valid Go identifiers.
// The "{$}" and "{name...}" wildcard must occur at the end of PATH.
// PATH may end with a '/'.
// Wildcard names in a path must be distinct.
internal static (ж<pattern> _, error err) parsePattern(@string s) => func((defer, _) => {
    error err = default!;

    if (len(s) == 0) {
        return (default!, errors.New("empty pattern"u8));
    }
    nint off = 0;
    // offset into string
    defer(() => {
        if (err != default!) {
            err = fmt.Errorf("at offset %d: %w"u8, off, err);
        }
    });
    @string method = s;
    @string rest = ""u8;
    var found = false;
    {
        nint iΔ1 = strings.IndexAny(s, " \t"u8); if (iΔ1 >= 0) {
            (method, rest, found) = (s[..(int)(iΔ1)], strings.TrimLeft(s[(int)(iΔ1 + 1)..], " \t"u8), true);
        }
    }
    if (!found) {
        rest = method;
        method = ""u8;
    }
    if (method != ""u8 && !validMethod(method)) {
        return (default!, fmt.Errorf("invalid method %q"u8, method));
    }
    var p = Ꮡ(new pattern(str: s, method: method));
    if (found) {
        off = len(method) + 1;
    }
    nint i = strings.IndexByte(rest, (rune)'/');
    if (i < 0) {
        return (default!, errors.New("host/path missing /"u8));
    }
    p.val.host = rest[..(int)(i)];
    rest = rest[(int)(i)..];
    {
        nint j = strings.IndexByte((~p).host, (rune)'{'); if (j >= 0) {
            off += j;
            return (default!, errors.New("host contains '{' (missing initial '/'?)"u8));
        }
    }
    // At this point, rest is the path.
    off += i;
    // An unclean path with a method that is not CONNECT can never match,
    // because paths are cleaned before matching.
    if (method != ""u8 && method != "CONNECT"u8 && rest != cleanPath(rest)) {
        return (default!, errors.New("non-CONNECT pattern with unclean path can never match"u8));
    }
    var seenNames = new map<@string, bool>{};
    // remember wildcard names to catch dups
    while (len(rest) > 0) {
        // Invariant: rest[0] == '/'.
        rest = rest[1..];
        off = len(s) - len(rest);
        if (len(rest) == 0) {
            // Trailing slash.
            p.val.segments = append((~p).segments, new segment(wild: true, multi: true));
            break;
        }
        nint i = strings.IndexByte(rest, (rune)'/');
        if (i < 0) {
            i = len(rest);
        }
        @string seg = default!;
        (seg, rest) = (rest[..(int)(i)], rest[(int)(i)..]);
        {
            nint iΔ2 = strings.IndexByte(seg, (rune)'{'); if (iΔ2 < 0){
                // Literal.
                seg = pathUnescape(seg);
                p.val.segments = append((~p).segments, new segment(s: seg));
            } else {
                // Wildcard.
                if (iΔ2 != 0) {
                    return (default!, errors.New("bad wildcard segment (must start with '{')"u8));
                }
                if (seg[len(seg) - 1] != (rune)'}') {
                    return (default!, errors.New("bad wildcard segment (must end with '}')"u8));
                }
                @string name = seg[1..(int)(len(seg) - 1)];
                if (name == "$"u8) {
                    if (len(rest) != 0) {
                        return (default!, errors.New("{$} not at end"u8));
                    }
                    p.val.segments = append((~p).segments, new segment(s: "/"u8));
                    break;
                }
                var (name, multi) = strings.CutSuffix(name, "..."u8);
                if (multi && len(rest) != 0) {
                    return (default!, errors.New("{...} wildcard not at end"u8));
                }
                if (name == ""u8) {
                    return (default!, errors.New("empty wildcard"u8));
                }
                if (!isValidWildcardName(name)) {
                    return (default!, fmt.Errorf("bad wildcard name %q"u8, name));
                }
                if (seenNames[name]) {
                    return (default!, fmt.Errorf("duplicate wildcard name %q"u8, name));
                }
                seenNames[name] = true;
                p.val.segments = append((~p).segments, new segment(s: name, wild: true, multi: multi));
            }
        }
    }
    return (p, default!);
});

internal static bool isValidWildcardName(@string s) {
    if (s == ""u8) {
        return false;
    }
    // Valid Go identifier.
    foreach (var (i, c) in s) {
        if (!unicode.IsLetter(c) && c != (rune)'_' && (i == 0 || !unicode.IsDigit(c))) {
            return false;
        }
    }
    return true;
}

internal static @string pathUnescape(@string path) {
    var (u, err) = url.PathUnescape(path);
    if (err != default!) {
        // Invalidly escaped path; use the original
        return path;
    }
    return u;
}

[GoType("@string")] partial struct relationship;

internal static readonly @string equivalent = "equivalent"u8;              // both match the same requests
internal static readonly @string moreGeneral = "moreGeneral"u8;             // p1 matches everything p2 does & more
internal static readonly @string moreSpecific = "moreSpecific"u8;            // p2 matches everything p1 does & more
internal static readonly @string disjoint = "disjoint"u8;                // there is no request that both match
internal static readonly @string overlaps = "overlaps"u8;                // there is a request that both match, but neither is more specific

// conflictsWith reports whether p1 conflicts with p2, that is, whether
// there is a request that both match but where neither is higher precedence
// than the other.
//
//	Precedence is defined by two rules:
//	1. Patterns with a host win over patterns without a host.
//	2. Patterns whose method and path is more specific win. One pattern is more
//	   specific than another if the second matches all the (method, path) pairs
//	   of the first and more.
//
// If rule 1 doesn't apply, then two patterns conflict if their relationship
// is either equivalence (they match the same set of requests) or overlap
// (they both match some requests, but neither is more specific than the other).
[GoRecv] internal static bool conflictsWith(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.val;

    if (p1.host != p2.host) {
        // Either one host is empty and the other isn't, in which case the
        // one with the host wins by rule 1, or neither host is empty
        // and they differ, so they won't match the same paths.
        return false;
    }
    @string rel = p1.comparePathsAndMethods(Ꮡp2);
    return rel == equivalent || rel == overlaps;
}

[GoRecv] internal static relationship comparePathsAndMethods(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.val;

    @string mrel = p1.compareMethods(Ꮡp2);
    // Optimization: avoid a call to comparePaths.
    if (mrel == disjoint) {
        return disjoint;
    }
    @string prel = p1.comparePaths(Ꮡp2);
    return combineRelationships(mrel, prel);
}

// compareMethods determines the relationship between the method
// part of patterns p1 and p2.
//
// A method can either be empty, "GET", or something else.
// The empty string matches any method, so it is the most general.
// "GET" matches both GET and HEAD.
// Anything else matches only itself.
[GoRecv] internal static relationship compareMethods(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.val;

    if (p1.method == p2.method) {
        return equivalent;
    }
    if (p1.method == ""u8) {
        // p1 matches any method, but p2 does not, so p1 is more general.
        return moreGeneral;
    }
    if (p2.method == ""u8) {
        return moreSpecific;
    }
    if (p1.method == "GET"u8 && p2.method == "HEAD"u8) {
        // p1 matches GET and HEAD; p2 matches only HEAD.
        return moreGeneral;
    }
    if (p2.method == "GET"u8 && p1.method == "HEAD"u8) {
        return moreSpecific;
    }
    return disjoint;
}

// comparePaths determines the relationship between the path
// part of two patterns.
[GoRecv] internal static relationship comparePaths(this ref pattern p1, ж<pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.val;

    // Optimization: if a path pattern doesn't end in a multi ("...") wildcard, then it
    // can only match paths with the same number of segments.
    if (len(p1.segments) != len(p2.segments) && !p1.lastSegment().multi && !p2.lastSegment().multi) {
        return disjoint;
    }
    // Consider corresponding segments in the two path patterns.
    slice<segment> segs1 = default!;
    slice<segment> segs2 = default!;
    @string rel = equivalent;
    for ((segs1, segs2) = (p1.segments, p2.segments); len(segs1) > 0 && len(segs2) > 0; (segs1, segs2) = (segs1[1..], segs2[1..])) {
        rel = combineRelationships(rel, compareSegments(segs1[0], segs2[0]));
        if (rel == disjoint) {
            return rel;
        }
    }
    // We've reached the end of the corresponding segments of the patterns.
    // If they have the same number of segments, then we've already determined
    // their relationship.
    if (len(segs1) == 0 && len(segs2) == 0) {
        return rel;
    }
    // Otherwise, the only way they could fail to be disjoint is if the shorter
    // pattern ends in a multi. In that case, that multi is more general
    // than the remainder of the longer pattern, so combine those two relationships.
    if (len(segs1) < len(segs2) && p1.lastSegment().multi) {
        return combineRelationships(rel, moreGeneral);
    }
    if (len(segs2) < len(segs1) && p2.lastSegment().multi) {
        return combineRelationships(rel, moreSpecific);
    }
    return disjoint;
}

// compareSegments determines the relationship between two segments.
internal static relationship compareSegments(segment s1, segment s2) {
    if (s1.multi && s2.multi) {
        return equivalent;
    }
    if (s1.multi) {
        return moreGeneral;
    }
    if (s2.multi) {
        return moreSpecific;
    }
    if (s1.wild && s2.wild) {
        return equivalent;
    }
    if (s1.wild) {
        if (s2.s == "/"u8) {
            // A single wildcard doesn't match a trailing slash.
            return disjoint;
        }
        return moreGeneral;
    }
    if (s2.wild) {
        if (s1.s == "/"u8) {
            return disjoint;
        }
        return moreSpecific;
    }
    // Both literals.
    if (s1.s == s2.s) {
        return equivalent;
    }
    return disjoint;
}

// combineRelationships determines the overall relationship of two patterns
// given the relationships of a partition of the patterns into two parts.
//
// For example, if p1 is more general than p2 in one way but equivalent
// in the other, then it is more general overall.
//
// Or if p1 is more general in one way and more specific in the other, then
// they overlap.
internal static relationship combineRelationships(relationship r1, relationship r2) {
    var exprᴛ1 = r1;
    if (exprᴛ1 == equivalent) {
        return r2;
    }
    if (exprᴛ1 == disjoint) {
        return disjoint;
    }
    if (exprᴛ1 == overlaps) {
        if (r2 == disjoint) {
            return disjoint;
        }
        return overlaps;
    }
    if (exprᴛ1 == moreGeneral || exprᴛ1 == moreSpecific) {
        var exprᴛ2 = r2;
        if (exprᴛ2 == equivalent) {
            return r1;
        }
        if (exprᴛ2 == inverseRelationship(r1)) {
            return overlaps;
        }
        { /* default: */
            return r2;
        }

    }
    { /* default: */
        throw panic(fmt.Sprintf("unknown relationship %q"u8, r1));
    }

}

// If p1 has relationship `r` to p2, then
// p2 has inverseRelationship(r) to p1.
internal static relationship inverseRelationship(relationship r) {
    var exprᴛ1 = r;
    if (exprᴛ1 == moreSpecific) {
        return moreGeneral;
    }
    if (exprᴛ1 == moreGeneral) {
        return moreSpecific;
    }
    { /* default: */
        return r;
    }

}

// isLitOrSingle reports whether the segment is a non-dollar literal or a single wildcard.
internal static bool isLitOrSingle(segment seg) {
    if (seg.wild) {
        return !seg.multi;
    }
    return seg.s != "/"u8;
}

// describeConflict returns an explanation of why two patterns conflict.
internal static @string describeConflict(ж<pattern> Ꮡp1, ж<pattern> Ꮡp2) {
    ref var p1 = ref Ꮡp1.val;
    ref var p2 = ref Ꮡp2.val;

    @string mrel = p1.compareMethods(Ꮡp2);
    @string prel = p1.comparePaths(Ꮡp2);
    @string rel = combineRelationships(mrel, prel);
    if (rel == equivalent) {
        return fmt.Sprintf("%s matches the same requests as %s"u8, p1, p2);
    }
    if (rel != overlaps) {
        throw panic("describeConflict called with non-conflicting patterns");
    }
    if (prel == overlaps) {
        return fmt.Sprintf("""
%[1]s and %[2]s both match some paths, like %[3]q.
But neither is more specific than the other.
%[1]s matches %[4]q, but %[2]s doesn't.
%[2]s matches %[5]q, but %[1]s doesn't.
"""u8,
            p1, p2, commonPath(Ꮡp1, Ꮡp2), differencePath(Ꮡp1, Ꮡp2), differencePath(Ꮡp2, Ꮡp1));
    }
    if (mrel == moreGeneral && prel == moreSpecific) {
        return fmt.Sprintf("%s matches more methods than %s, but has a more specific path pattern"u8, p1, p2);
    }
    if (mrel == moreSpecific && prel == moreGeneral) {
        return fmt.Sprintf("%s matches fewer methods than %s, but has a more general path pattern"u8, p1, p2);
    }
    return fmt.Sprintf("bug: unexpected way for two patterns %s and %s to conflict: methods %s, paths %s"u8, p1, p2, mrel, prel);
}

// writeMatchingPath writes to b a path that matches the segments.
internal static void writeMatchingPath(ж<strings.Builder> Ꮡb, slice<segment> segs) {
    ref var b = ref Ꮡb.val;

    foreach (var (_, s) in segs) {
        writeSegment(Ꮡb, s);
    }
}

internal static void writeSegment(ж<strings.Builder> Ꮡb, segment s) {
    ref var b = ref Ꮡb.val;

    b.WriteByte((rune)'/');
    if (!s.multi && s.s != "/"u8) {
        b.WriteString(s.s);
    }
}

// commonPath returns a path that both p1 and p2 match.
// It assumes there is such a path.
internal static @string commonPath(ж<pattern> Ꮡp1, ж<pattern> Ꮡp2) {
    ref var p1 = ref Ꮡp1.val;
    ref var p2 = ref Ꮡp2.val;

    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    slice<segment> segs1 = default!;
    slice<segment> segs2 = default!;
    for ((segs1, segs2) = (p1.segments, p2.segments); len(segs1) > 0 && len(segs2) > 0; (segs1, segs2) = (segs1[1..], segs2[1..])) {
        {
            var s1 = segs1[0]; if (s1.wild){
                writeSegment(Ꮡb, segs2[0]);
            } else {
                writeSegment(Ꮡb, s1);
            }
        }
    }
    if (len(segs1) > 0){
        writeMatchingPath(Ꮡb, segs1);
    } else 
    if (len(segs2) > 0) {
        writeMatchingPath(Ꮡb, segs2);
    }
    return b.String();
}

// differencePath returns a path that p1 matches and p2 doesn't.
// It assumes there is such a path.
internal static @string differencePath(ж<pattern> Ꮡp1, ж<pattern> Ꮡp2) {
    ref var p1 = ref Ꮡp1.val;
    ref var p2 = ref Ꮡp2.val;

    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    slice<segment> segs1 = default!;
    slice<segment> segs2 = default!;
    for ((segs1, segs2) = (p1.segments, p2.segments); len(segs1) > 0 && len(segs2) > 0; (segs1, segs2) = (segs1[1..], segs2[1..])) {
        var s1 = segs1[0];
        var s2 = segs2[0];
        if (s1.multi && s2.multi) {
            // From here the patterns match the same paths, so we must have found a difference earlier.
            b.WriteByte((rune)'/');
            return b.String();
        }
        if (s1.multi && !s2.multi) {
            // s1 ends in a "..." wildcard but s2 does not.
            // A trailing slash will distinguish them, unless s2 ends in "{$}",
            // in which case any segment will do; prefer the wildcard name if
            // it has one.
            b.WriteByte((rune)'/');
            if (s2.s == "/"u8) {
                if (s1.s != ""u8){
                    b.WriteString(s1.s);
                } else {
                    b.WriteString("x"u8);
                }
            }
            return b.String();
        }
        if (!s1.multi && s2.multi){
            writeSegment(Ꮡb, s1);
        } else 
        if (s1.wild && s2.wild){
            // Both patterns will match whatever we put here; use
            // the first wildcard name.
            writeSegment(Ꮡb, s1);
        } else 
        if (s1.wild && !s2.wild){
            // s1 is a wildcard, s2 is a literal.
            // Any segment other than s2.s will work.
            // Prefer the wildcard name, but if it's the same as the literal,
            // tweak the literal.
            if (s1.s != s2.s){
                writeSegment(Ꮡb, s1);
            } else {
                b.WriteByte((rune)'/');
                b.WriteString(s2.s + "x"u8);
            }
        } else 
        if (!s1.wild && s2.wild){
            writeSegment(Ꮡb, s1);
        } else {
            // Both are literals. A precondition of this function is that the
            // patterns overlap, so they must be the same literal. Use it.
            if (s1.s != s2.s) {
                throw panic(fmt.Sprintf("literals differ: %q and %q"u8, s1.s, s2.s));
            }
            writeSegment(Ꮡb, s1);
        }
    }
    if (len(segs1) > 0){
        // p1 is longer than p2, and p2 does not end in a multi.
        // Anything that matches the rest of p1 will do.
        writeMatchingPath(Ꮡb, segs1);
    } else 
    if (len(segs2) > 0) {
        writeMatchingPath(Ꮡb, segs2);
    }
    return b.String();
}

} // end http_package
