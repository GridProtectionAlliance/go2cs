// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 09 04:57:45 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\header.go
using io = go.io_package;
using httptrace = go.net.http.httptrace_package;
using textproto = go.net.textproto_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // A Header represents the key-value pairs in an HTTP header.
        //
        // The keys should be in canonical form, as returned by
        // CanonicalHeaderKey.
        public partial struct Header // : map<@string, slice<@string>>
        {
        }

        // Add adds the key, value pair to the header.
        // It appends to any existing values associated with key.
        // The key is case insensitive; it is canonicalized by
        // CanonicalHeaderKey.
        public static void Add(this Header h, @string key, @string value)
        {
            textproto.MIMEHeader(h).Add(key, value);
        }

        // Set sets the header entries associated with key to the
        // single element value. It replaces any existing values
        // associated with key. The key is case insensitive; it is
        // canonicalized by textproto.CanonicalMIMEHeaderKey.
        // To use non-canonical keys, assign to the map directly.
        public static void Set(this Header h, @string key, @string value)
        {
            textproto.MIMEHeader(h).Set(key, value);
        }

        // Get gets the first value associated with the given key. If
        // there are no values associated with the key, Get returns "".
        // It is case insensitive; textproto.CanonicalMIMEHeaderKey is
        // used to canonicalize the provided key. To use non-canonical keys,
        // access the map directly.
        public static @string Get(this Header h, @string key)
        {
            return textproto.MIMEHeader(h).Get(key);
        }

        // Values returns all values associated with the given key.
        // It is case insensitive; textproto.CanonicalMIMEHeaderKey is
        // used to canonicalize the provided key. To use non-canonical
        // keys, access the map directly.
        // The returned slice is not a copy.
        public static slice<@string> Values(this Header h, @string key)
        {
            return textproto.MIMEHeader(h).Values(key);
        }

        // get is like Get, but key must already be in CanonicalHeaderKey form.
        public static @string get(this Header h, @string key)
        {
            {
                var v = h[key];

                if (len(v) > 0L)
                {
                    return v[0L];
                }

            }

            return "";

        }

        // has reports whether h has the provided key defined, even if it's
        // set to 0-length slice.
        public static bool has(this Header h, @string key)
        {
            var (_, ok) = h[key];
            return ok;
        }

        // Del deletes the values associated with key.
        // The key is case insensitive; it is canonicalized by
        // CanonicalHeaderKey.
        public static void Del(this Header h, @string key)
        {
            textproto.MIMEHeader(h).Del(key);
        }

        // Write writes a header in wire format.
        public static error Write(this Header h, io.Writer w)
        {
            return error.As(h.write(w, null))!;
        }

        public static error write(this Header h, io.Writer w, ptr<httptrace.ClientTrace> _addr_trace)
        {
            ref httptrace.ClientTrace trace = ref _addr_trace.val;

            return error.As(h.writeSubset(w, null, trace))!;
        }

        // Clone returns a copy of h or nil if h is nil.
        public static Header Clone(this Header h)
        {
            if (h == null)
            {
                return null;
            } 

            // Find total number of values.
            long nv = 0L;
            {
                var vv__prev1 = vv;

                foreach (var (_, __vv) in h)
                {
                    vv = __vv;
                    nv += len(vv);
                }

                vv = vv__prev1;
            }

            var sv = make_slice<@string>(nv); // shared backing array for headers' values
            var h2 = make(Header, len(h));
            {
                var vv__prev1 = vv;

                foreach (var (__k, __vv) in h)
                {
                    k = __k;
                    vv = __vv;
                    var n = copy(sv, vv);
                    h2[k] = sv.slice(-1, n, n);
                    sv = sv[n..];
                }

                vv = vv__prev1;
            }

            return h2;

        }

        private static @string timeFormats = new slice<@string>(new @string[] { TimeFormat, time.RFC850, time.ANSIC });

        // ParseTime parses a time header (such as the Date: header),
        // trying each of the three formats allowed by HTTP/1.1:
        // TimeFormat, time.RFC850, and time.ANSIC.
        public static (time.Time, error) ParseTime(@string text)
        {
            time.Time t = default;
            error err = default!;

            foreach (var (_, layout) in timeFormats)
            {
                t, err = time.Parse(layout, text);
                if (err == null)
                {
                    return ;
                }

            }
            return ;

        }

        private static var headerNewlineToSpace = strings.NewReplacer("\n", " ", "\r", " ");

        // stringWriter implements WriteString on a Writer.
        private partial struct stringWriter
        {
            public io.Writer w;
        }

        private static (long, error) WriteString(this stringWriter w, @string s)
        {
            long n = default;
            error err = default!;

            return w.w.Write((slice<byte>)s);
        }

        private partial struct keyValues
        {
            public @string key;
            public slice<@string> values;
        }

        // A headerSorter implements sort.Interface by sorting a []keyValues
        // by key. It's used as a pointer, so it can fit in a sort.Interface
        // interface value without allocation.
        private partial struct headerSorter
        {
            public slice<keyValues> kvs;
        }

        private static long Len(this ptr<headerSorter> _addr_s)
        {
            ref headerSorter s = ref _addr_s.val;

            return len(s.kvs);
        }
        private static void Swap(this ptr<headerSorter> _addr_s, long i, long j)
        {
            ref headerSorter s = ref _addr_s.val;

            s.kvs[i] = s.kvs[j];
            s.kvs[j] = s.kvs[i];
        }
        private static bool Less(this ptr<headerSorter> _addr_s, long i, long j)
        {
            ref headerSorter s = ref _addr_s.val;

            return s.kvs[i].key < s.kvs[j].key;
        }

        private static sync.Pool headerSorterPool = new sync.Pool(New:func()interface{}{returnnew(headerSorter)},);

        // sortedKeyValues returns h's keys sorted in the returned kvs
        // slice. The headerSorter used to sort is also returned, for possible
        // return to headerSorterCache.
        public static (slice<keyValues>, ptr<headerSorter>) sortedKeyValues(this Header h, map<@string, bool> exclude)
        {
            slice<keyValues> kvs = default;
            ptr<headerSorter> hs = default!;

            hs = headerSorterPool.Get()._<ptr<headerSorter>>();
            if (cap(hs.kvs) < len(h))
            {
                hs.kvs = make_slice<keyValues>(0L, len(h));
            }

            kvs = hs.kvs[..0L];
            foreach (var (k, vv) in h)
            {
                if (!exclude[k])
                {
                    kvs = append(kvs, new keyValues(k,vv));
                }

            }
            hs.kvs = kvs;
            sort.Sort(hs);
            return (kvs, _addr_hs!);

        }

        // WriteSubset writes a header in wire format.
        // If exclude is not nil, keys where exclude[key] == true are not written.
        // Keys are not canonicalized before checking the exclude map.
        public static error WriteSubset(this Header h, io.Writer w, map<@string, bool> exclude)
        {
            return error.As(h.writeSubset(w, exclude, null))!;
        }

        public static error writeSubset(this Header h, io.Writer w, map<@string, bool> exclude, ptr<httptrace.ClientTrace> _addr_trace)
        {
            ref httptrace.ClientTrace trace = ref _addr_trace.val;

            io.StringWriter (ws, ok) = w._<io.StringWriter>();
            if (!ok)
            {
                ws = new stringWriter(w);
            }

            var (kvs, sorter) = h.sortedKeyValues(exclude);
            slice<@string> formattedVals = default;
            foreach (var (_, kv) in kvs)
            {
                foreach (var (_, v) in kv.values)
                {
                    v = headerNewlineToSpace.Replace(v);
                    v = textproto.TrimString(v);
                    foreach (var (_, s) in new slice<@string>(new @string[] { kv.key, ": ", v, "\r\n" }))
                    {
                        {
                            var (_, err) = ws.WriteString(s);

                            if (err != null)
                            {
                                headerSorterPool.Put(sorter);
                                return error.As(err)!;
                            }

                        }

                    }
                    if (trace != null && trace.WroteHeaderField != null)
                    {
                        formattedVals = append(formattedVals, v);
                    }

                }
                if (trace != null && trace.WroteHeaderField != null)
                {
                    trace.WroteHeaderField(kv.key, formattedVals);
                    formattedVals = null;
                }

            }
            headerSorterPool.Put(sorter);
            return error.As(null!)!;

        }

        // CanonicalHeaderKey returns the canonical format of the
        // header key s. The canonicalization converts the first
        // letter and any letter following a hyphen to upper case;
        // the rest are converted to lowercase. For example, the
        // canonical key for "accept-encoding" is "Accept-Encoding".
        // If s contains a space or invalid header field bytes, it is
        // returned without modifications.
        public static @string CanonicalHeaderKey(@string s)
        {
            return textproto.CanonicalMIMEHeaderKey(s);
        }

        // hasToken reports whether token appears with v, ASCII
        // case-insensitive, with space or comma boundaries.
        // token must be all lowercase.
        // v may contain mixed cased.
        private static bool hasToken(@string v, @string token)
        {
            if (len(token) > len(v) || token == "")
            {
                return false;
            }

            if (v == token)
            {
                return true;
            }

            for (long sp = 0L; sp <= len(v) - len(token); sp++)
            { 
                // Check that first character is good.
                // The token is ASCII, so checking only a single byte
                // is sufficient. We skip this potential starting
                // position if both the first byte and its potential
                // ASCII uppercase equivalent (b|0x20) don't match.
                // False positives ('^' => '~') are caught by EqualFold.
                {
                    var b = v[sp];

                    if (b != token[0L] && b | 0x20UL != token[0L])
                    {
                        continue;
                    } 
                    // Check that start pos is on a valid token boundary.

                } 
                // Check that start pos is on a valid token boundary.
                if (sp > 0L && !isTokenBoundary(v[sp - 1L]))
                {
                    continue;
                } 
                // Check that end pos is on a valid token boundary.
                {
                    var endPos = sp + len(token);

                    if (endPos != len(v) && !isTokenBoundary(v[endPos]))
                    {
                        continue;
                    }

                }

                if (strings.EqualFold(v[sp..sp + len(token)], token))
                {
                    return true;
                }

            }

            return false;

        }

        private static bool isTokenBoundary(byte b)
        {
            return b == ' ' || b == ',' || b == '\t';
        }
    }
}}
