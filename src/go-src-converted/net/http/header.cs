// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using io = io_package;
using httptrace = net.http.httptrace_package;
using ascii = net.http.@internal.ascii_package;
using textproto = net.textproto_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using httpguts = golang.org.x.net.http.httpguts_package;
using golang.org.x.net.http;
using net.http;
using net.http.@internal;

partial class http_package {
/* visitMapType: map[string][]string */

// Add adds the key, value pair to the header.
// It appends to any existing values associated with key.
// The key is case insensitive; it is canonicalized by
// [CanonicalHeaderKey].
public static void Add(this ΔHeader h, @string key, @string value) {
    ((textproto.MIMEHeader)h).Add(key, value);
}

// Set sets the header entries associated with key to the
// single element value. It replaces any existing values
// associated with key. The key is case insensitive; it is
// canonicalized by [textproto.CanonicalMIMEHeaderKey].
// To use non-canonical keys, assign to the map directly.
public static void Set(this ΔHeader h, @string key, @string value) {
    ((textproto.MIMEHeader)h).Set(key, value);
}

// Get gets the first value associated with the given key. If
// there are no values associated with the key, Get returns "".
// It is case insensitive; [textproto.CanonicalMIMEHeaderKey] is
// used to canonicalize the provided key. Get assumes that all
// keys are stored in canonical form. To use non-canonical keys,
// access the map directly.
public static @string Get(this ΔHeader h, @string key) {
    return ((textproto.MIMEHeader)h).Get(key);
}

// Values returns all values associated with the given key.
// It is case insensitive; [textproto.CanonicalMIMEHeaderKey] is
// used to canonicalize the provided key. To use non-canonical
// keys, access the map directly.
// The returned slice is not a copy.
public static slice<@string> Values(this ΔHeader h, @string key) {
    return ((textproto.MIMEHeader)h).Values(key);
}

// get is like Get, but key must already be in CanonicalHeaderKey form.
internal static @string get(this ΔHeader h, @string key) {
    {
        var v = h[key]; if (len(v) > 0) {
            return v[0];
        }
    }
    return ""u8;
}

// has reports whether h has the provided key defined, even if it's
// set to 0-length slice.
internal static bool has(this ΔHeader h, @string key) {
    var _ = h[key];
    var ok = h[key];
    return ok;
}

// Del deletes the values associated with key.
// The key is case insensitive; it is canonicalized by
// [CanonicalHeaderKey].
public static void Del(this ΔHeader h, @string key) {
    ((textproto.MIMEHeader)h).Del(key);
}

// Write writes a header in wire format.
public static error Write(this ΔHeader h, io.Writer w) {
    return h.write(w, nil);
}

public static error write(this ΔHeader h, io.Writer w, ж<httptrace.ClientTrace> Ꮡtrace) {
    ref var trace = ref Ꮡtrace.val;

    return h.writeSubset(w, default!, Ꮡtrace);
}

// Clone returns a copy of h or nil if h is nil.
public static ΔHeader Clone(this ΔHeader h) {
    if (h == default!) {
        return default!;
    }
    // Find total number of values.
    nint nv = 0;
    foreach (var (_, vv) in h) {
        nv += len(vv);
    }
    var sv = new slice<@string>(nv);
    // shared backing array for headers' values
    var h2 = new ΔHeader(len(h));
    foreach (var (k, vv) in h) {
        if (vv == default!) {
            // Preserve nil values. ReverseProxy distinguishes
            // between nil and zero-length header values.
            h2[k] = default!;
            continue;
        }
        nint n = copy(sv, vv);
        h2[k] = sv.slice(-1, n, n);
        sv = sv[(int)(n)..];
    }
    return h2;
}

internal static slice<@string> timeFormats = new @string[]{
    TimeFormat,
    time.RFC850,
    time.ANSIC
}.slice();

// ParseTime parses a time header (such as the Date: header),
// trying each of the three formats allowed by HTTP/1.1:
// [TimeFormat], [time.RFC850], and [time.ANSIC].
public static (time.Time t, error err) ParseTime(@string text) {
    time.Time t = default!;
    error err = default!;

    foreach (var (_, layout) in timeFormats) {
        (t, err) = time.Parse(layout, text);
        if (err == default!) {
            return (t, err);
        }
    }
    return (t, err);
}

internal static ж<strings.Replacer> headerNewlineToSpace = strings.NewReplacer("\n"u8, " ", "\r", " ");

// stringWriter implements WriteString on a Writer.
[GoType] partial struct stringWriter {
    internal io_package.Writer w;
}

internal static (nint n, error err) WriteString(this stringWriter w, @string s) {
    nint n = default!;
    error err = default!;

    return w.w.Write(slice<byte>(s));
}

[GoType] partial struct keyValues {
    internal @string key;
    internal slice<@string> values;
}

// headerSorter contains a slice of keyValues sorted by keyValues.key.
[GoType] partial struct headerSorter {
    internal slice<keyValues> kvs;
}

internal static sync.Pool headerSorterPool = new sync.Pool(
    New: () => @new<headerSorter>()
);

// sortedKeyValues returns h's keys sorted in the returned kvs
// slice. The headerSorter used to sort is also returned, for possible
// return to headerSorterCache.
internal static (slice<keyValues> kvs, ж<headerSorter> hs) sortedKeyValues(this ΔHeader h, map<@string, bool> exclude) {
    slice<keyValues> kvs = default!;
    ж<headerSorter> hs = default!;

    hs = headerSorterPool.Get()._<headerSorter.val>();
    if (cap((~hs).kvs) < len(h)) {
        hs.val.kvs = new slice<keyValues>(0, len(h));
    }
    kvs = (~hs).kvs[..0];
    foreach (var (k, vv) in h) {
        if (!exclude[k]) {
            kvs = append(kvs, new keyValues(k, vv));
        }
    }
    hs.val.kvs = kvs;
    slices.SortFunc((~hs).kvs, (keyValues a, keyValues b) => strings.Compare(a.key, b.key));
    return (kvs, hs);
}

// WriteSubset writes a header in wire format.
// If exclude is not nil, keys where exclude[key] == true are not written.
// Keys are not canonicalized before checking the exclude map.
public static error WriteSubset(this ΔHeader h, io.Writer w, map<@string, bool> exclude) {
    return h.writeSubset(w, exclude, nil);
}

public static error writeSubset(this ΔHeader h, io.Writer w, map<@string, bool> exclude, ж<httptrace.ClientTrace> Ꮡtrace) {
    ref var trace = ref Ꮡtrace.val;

    var (ws, ok) = w._<io.StringWriter>(ᐧ);
    if (!ok) {
        ws = new stringWriter(w);
    }
    (kvs, sorter) = h.sortedKeyValues(exclude);
    slice<@string> formattedVals = default!;
    foreach (var (_, kv) in kvs) {
        if (!httpguts.ValidHeaderFieldName(kv.key)) {
            // This could be an error. In the common case of
            // writing response headers, however, we have no good
            // way to provide the error back to the server
            // handler, so just drop invalid headers instead.
            continue;
        }
        foreach (var (_, v) in kv.values) {
            v = headerNewlineToSpace.Replace(v);
            v = textproto.TrimString(v);
            foreach (var (_, s) in new @string[]{kv.key, ": ", v, "\r\n"}.slice()) {
                {
                    var (_, err) = ws.WriteString(s); if (err != default!) {
                        headerSorterPool.Put(sorter);
                        return err;
                    }
                }
            }
            if (trace != nil && trace.WroteHeaderField != default!) {
                formattedVals = append(formattedVals, v);
            }
        }
        if (trace != nil && trace.WroteHeaderField != default!) {
            trace.WroteHeaderField(kv.key, formattedVals);
            formattedVals = default!;
        }
    }
    headerSorterPool.Put(sorter);
    return default!;
}

// CanonicalHeaderKey returns the canonical format of the
// header key s. The canonicalization converts the first
// letter and any letter following a hyphen to upper case;
// the rest are converted to lowercase. For example, the
// canonical key for "accept-encoding" is "Accept-Encoding".
// If s contains a space or invalid header field bytes, it is
// returned without modifications.
public static @string CanonicalHeaderKey(@string s) {
    return textproto.CanonicalMIMEHeaderKey(s);
}

// hasToken reports whether token appears with v, ASCII
// case-insensitive, with space or comma boundaries.
// token must be all lowercase.
// v may contain mixed cased.
internal static bool hasToken(@string v, @string token) {
    if (len(token) > len(v) || token == ""u8) {
        return false;
    }
    if (v == token) {
        return true;
    }
    for (nint sp = 0; sp <= len(v) - len(token); sp++) {
        // Check that first character is good.
        // The token is ASCII, so checking only a single byte
        // is sufficient. We skip this potential starting
        // position if both the first byte and its potential
        // ASCII uppercase equivalent (b|0x20) don't match.
        // False positives ('^' => '~') are caught by EqualFold.
        {
            var b = v[sp]; if (b != token[0] && (byte)(b | 32) != token[0]) {
                continue;
            }
        }
        // Check that start pos is on a valid token boundary.
        if (sp > 0 && !isTokenBoundary(v[sp - 1])) {
            continue;
        }
        // Check that end pos is on a valid token boundary.
        {
            nint endPos = sp + len(token); if (endPos != len(v) && !isTokenBoundary(v[endPos])) {
                continue;
            }
        }
        if (ascii.EqualFold(v[(int)(sp)..(int)(sp + len(token))], token)) {
            return true;
        }
    }
    return false;
}

internal static bool isTokenBoundary(byte b) {
    return b == (rune)' ' || b == (rune)',' || b == (rune)'\t';
}

} // end http_package
