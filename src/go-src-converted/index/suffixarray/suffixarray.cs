// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package suffixarray implements substring search in logarithmic time using
// an in-memory suffix array.
//
// Example use:
//
//	// create index for some data
//	index := suffixarray.New(data)
//
//	// lookup byte slice s
//	offsets1 := index.Lookup(s, -1) // the list of all indices where s occurs in data
//	offsets2 := index.Lookup(s, 3)  // the list of at most 3 indices where s occurs in data
namespace go.index;

using bytes = bytes_package;
using binary = encoding.binary_package;
using errors = errors_package;
using io = io_package;
using math = math_package;
using regexp = regexp_package;
using slices = slices_package;
using sort = sort_package;
using encoding;

partial class suffixarray_package {

// Can change for testing
internal static nint maxData32 = realMaxData32;

internal static readonly UntypedInt realMaxData32 = /* math.MaxInt32 */ 2147483647;

// Index implements a suffix array for fast substring search.
[GoType] partial struct Index {
    internal slice<byte> data;
    internal ints sa; // suffix array for data; sa.len() == len(data)
}

// An ints is either an []int32 or an []int64.
// That is, one of them is empty, and one is the real data.
// The int64 form is used when len(data) > maxData32
[GoType] partial struct ints {
    internal slice<int32> int32;
    internal slice<int64> int64;
}

[GoRecv] internal static nint len(this ref ints a) {
    return len(a.int32) + len(a.int64);
}

[GoRecv] internal static int64 get(this ref ints a, nint i) {
    if (a.int32 != default!) {
        return ((int64)a.int32[i]);
    }
    return a.int64[i];
}

[GoRecv] internal static void set(this ref ints a, nint i, int64 v) {
    if (a.int32 != default!){
        a.int32[i] = ((int32)v);
    } else {
        a.int64[i] = v;
    }
}

[GoRecv] internal static ints Δslice(this ref ints a, nint i, nint j) {
    if (a.int32 != default!) {
        return new ints(a.int32[(int)(i)..(int)(j)], default!);
    }
    return new ints(default!, a.int64[(int)(i)..(int)(j)]);
}

// New creates a new [Index] for data.
// [Index] creation time is O(N) for N = len(data).
public static ж<Index> New(slice<byte> data) {
    var ix = Ꮡ(new Index(data: data));
    if (len(data) <= maxData32){
        (~ix).sa.int32 = new slice<int32>(len(data));
        text_32(data, (~ix).sa.int32);
    } else {
        (~ix).sa.int64 = new slice<int64>(len(data));
        text_64(data, (~ix).sa.int64);
    }
    return ix;
}

// writeInt writes an int x to w using buf to buffer the write.
internal static error writeInt(io.Writer w, slice<byte> buf, nint x) {
    binary.PutVarint(buf, ((int64)x));
    var (_, err) = w.Write(buf[0..(int)(binary.MaxVarintLen64)]);
    return err;
}

// readInt reads an int x from r using buf to buffer the read and returns x.
internal static (int64, error) readInt(io.Reader r, slice<byte> buf) {
    var (_, err) = io.ReadFull(r, buf[0..(int)(binary.MaxVarintLen64)]);
    // ok to continue with error
    var (x, _) = binary.Varint(buf);
    return (x, err);
}

// writeSlice writes data[:n] to w and returns n.
// It uses buf to buffer the write.
internal static (nint n, error err) writeSlice(io.Writer w, slice<byte> buf, ints data) {
    nint n = default!;
    error err = default!;

    // encode as many elements as fit into buf
    nint p = binary.MaxVarintLen64;
    nint m = data.len();
    for (; n < m && p + binary.MaxVarintLen64 <= len(buf); n++) {
        p += binary.PutUvarint(buf[(int)(p)..], ((uint64)data.get(n)));
    }
    // update buffer size
    binary.PutVarint(buf, ((int64)p));
    // write buffer
    (_, err) = w.Write(buf[0..(int)(p)]);
    return (n, err);
}

internal static error errTooBig = errors.New("suffixarray: data too large"u8);

// readSlice reads data[:n] from r and returns n.
// It uses buf to buffer the read.
internal static (nint n, error err) readSlice(io.Reader r, slice<byte> buf, ints data) {
    nint n = default!;
    error err = default!;

    // read buffer size
    int64 size64 = default!;
    (size64, err) = readInt(r, buf);
    if (err != default!) {
        return (n, err);
    }
    if (((int64)((nint)size64)) != size64 || ((nint)size64) < 0) {
        // We never write chunks this big anyway.
        return (0, errTooBig);
    }
    nint size = ((nint)size64);
    // read buffer w/o the size
    {
        (_, err) = io.ReadFull(r, buf[(int)(binary.MaxVarintLen64)..(int)(size)]); if (err != default!) {
            return (n, err);
        }
    }
    // decode as many elements as present in buf
    for (nint p = binary.MaxVarintLen64; p < size; n++) {
        var (x, w) = binary.Uvarint(buf[(int)(p)..]);
        data.set(n, ((int64)x));
        p += w;
    }
    return (n, err);
}

internal static readonly UntypedInt bufSize = /* 16 << 10 */ 16384; // reasonable for BenchmarkSaveRestore

// Read reads the index from r into x; x must not be nil.
[GoRecv] public static error Read(this ref Index x, io.Reader r) {
    // buffer for all reads
    var buf = new slice<byte>(bufSize);
    // read length
    var (n64, err) = readInt(r, buf);
    if (err != default!) {
        return err;
    }
    if (((int64)((nint)n64)) != n64 || ((nint)n64) < 0) {
        return errTooBig;
    }
    nint n = ((nint)n64);
    // allocate space
    if (2 * n < cap(x.data) || cap(x.data) < n || x.sa.int32 != default! && n > maxData32 || x.sa.int64 != default! && n <= maxData32){
        // new data is significantly smaller or larger than
        // existing buffers - allocate new ones
        x.data = new slice<byte>(n);
        x.sa.int32 = default!;
        x.sa.int64 = default!;
        if (n <= maxData32){
            x.sa.int32 = new slice<int32>(n);
        } else {
            x.sa.int64 = new slice<int64>(n);
        }
    } else {
        // re-use existing buffers
        x.data = x.data[0..(int)(n)];
        x.sa = x.sa.Δslice(0, n);
    }
    // read data
    {
        var (_, errΔ1) = io.ReadFull(r, x.data); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    // read index
    var sa = x.sa;
    while (sa.len() > 0) {
        var (nΔ1, errΔ2) = readSlice(r, buf, sa);
        if (errΔ2 != default!) {
            return errΔ2;
        }
        sa = sa.Δslice(nΔ1, sa.len());
    }
    return default!;
}

// Write writes the index x to w.
[GoRecv] public static error Write(this ref Index x, io.Writer w) {
    // buffer for all writes
    var buf = new slice<byte>(bufSize);
    // write length
    {
        var err = writeInt(w, buf, len(x.data)); if (err != default!) {
            return err;
        }
    }
    // write data
    {
        var (_, err) = w.Write(x.data); if (err != default!) {
            return err;
        }
    }
    // write index
    var sa = x.sa;
    while (sa.len() > 0) {
        var (n, err) = writeSlice(w, buf, sa);
        if (err != default!) {
            return err;
        }
        sa = sa.Δslice(n, sa.len());
    }
    return default!;
}

// Bytes returns the data over which the index was created.
// It must not be modified.
[GoRecv] public static slice<byte> Bytes(this ref Index x) {
    return x.data;
}

[GoRecv] internal static slice<byte> at(this ref Index x, nint i) {
    return x.data[(int)(x.sa.get(i))..];
}

// lookupAll returns a slice into the matching region of the index.
// The runtime is O(log(N)*len(s)).
[GoRecv] internal static ints lookupAll(this ref Index x, slice<byte> s) {
    // find matching suffix index range [i:j]
    // find the first index where s would be the prefix
    nint i = sort.Search(x.sa.len(), 
    var sʗ1 = s;
    (nint i) => bytes.Compare(x.at(iΔ1), sʗ1) >= 0);
    // starting at i, find the first index at which s is not a prefix
    nint j = i + sort.Search(x.sa.len() - i, 
    var sʗ3 = s;
    (nint j) => !bytes.HasPrefix(x.at(jΔ1 + i), sʗ3));
    return x.sa.Δslice(i, j);
}

// Lookup returns an unsorted list of at most n indices where the byte string s
// occurs in the indexed data. If n < 0, all occurrences are returned.
// The result is nil if s is empty, s is not found, or n == 0.
// Lookup time is O(log(N)*len(s) + len(result)) where N is the
// size of the indexed data.
[GoRecv] public static slice<nint> /*result*/ Lookup(this ref Index x, slice<byte> s, nint n) {
    slice<nint> result = default!;

    if (len(s) > 0 && n != 0) {
        var matches = x.lookupAll(s);
        nint count = matches.len();
        if (n < 0 || count < n) {
            n = count;
        }
        // 0 <= n <= count
        if (n > 0) {
            result = new slice<nint>(n);
            if (matches.int32 != default!){
                foreach (var (i, _) in result) {
                    result[i] = ((nint)matches.int32[i]);
                }
            } else {
                foreach (var (i, _) in result) {
                    result[i] = ((nint)matches.int64[i]);
                }
            }
        }
    }
    return result;
}

// FindAllIndex returns a sorted list of non-overlapping matches of the
// regular expression r, where a match is a pair of indices specifying
// the matched slice of x.Bytes(). If n < 0, all matches are returned
// in successive order. Otherwise, at most n matches are returned and
// they may not be successive. The result is nil if there are no matches,
// or if n == 0.
[GoRecv] public static slice<slice<nint>> /*result*/ FindAllIndex(this ref Index x, ж<regexp.Regexp> Ꮡr, nint n) {
    slice<slice<nint>> result = default!;

    ref var r = ref Ꮡr.val;
    // a non-empty literal prefix is used to determine possible
    // match start indices with Lookup
    var (prefix, complete) = r.LiteralPrefix();
    var lit = slice<byte>(prefix);
    // worst-case scenario: no literal prefix
    if (prefix == ""u8) {
        return r.FindAllIndex(x.data, n);
    }
    // if regexp is a literal just use Lookup and convert its
    // result into match pairs
    if (complete) {
        // Lookup returns indices that may belong to overlapping matches.
        // After eliminating them, we may end up with fewer than n matches.
        // If we don't have enough at the end, redo the search with an
        // increased value n1, but only if Lookup returned all the requested
        // indices in the first place (if it returned fewer than that then
        // there cannot be more).
        for (nint n1Δ1 = n; ᐧ ;  += 2 * (n - len(result))) {
            /* overflow ok */
            var indices = x.Lookup(lit, n1Δ1);
            if (len(indices) == 0) {
                return result;
            }
            slices.Sort(indices);
            var pairs = new slice<nint>(2 * len(indices));
            result = new slice<slice<nint>>(len(indices));
            nint count = 0;
            nint prev = 0;
            foreach (var (_, i) in indices) {
                if (count == n) {
                    break;
                }
                // ignore indices leading to overlapping matches
                if (prev <= i) {
                    nint j = 2 * count;
                    pairs[j + 0] = i;
                    pairs[j + 1] = i + len(lit);
                    result[count] = pairs[(int)(j)..(int)(j + 2)];
                    count++;
                    prev = i + len(lit);
                }
            }
            result = result[0..(int)(count)];
            if (len(result) >= n || len(indices) != n1Δ1) {
                // found all matches or there's no chance to find more
                // (n and n1 can be negative)
                break;
            }
        }
        if (len(result) == 0) {
            result = default!;
        }
        return result;
    }
    // regexp has a non-empty literal prefix; Lookup(lit) computes
    // the indices of possible complete matches; use these as starting
    // points for anchored searches
    // (regexp "^" matches beginning of input, not beginning of line)
    r = regexp.MustCompile("^"u8 + r.String());
    // compiles because r compiled
    // same comment about Lookup applies here as in the loop above
    for (nint n1 = n; ᐧ ; n1 += 2 * (n - len(result))) {
        /* overflow ok */
        var indices = x.Lookup(lit, n1);
        if (len(indices) == 0) {
            return result;
        }
        slices.Sort(indices);
        result = result[0..0];
        nint prev = 0;
        foreach (var (_, i) in indices) {
            if (len(result) == n) {
                break;
            }
            var m = r.FindIndex(x.data[(int)(i)..]);
            // anchored search - will not run off
            // ignore indices leading to overlapping matches
            if (m != default! && prev <= i) {
                m[0] = i;
                // correct m
                m[1] += i;
                result = append(result, m);
                prev = m[1];
            }
        }
        if (len(result) >= n || len(indices) != n1) {
            // found all matches or there's no chance to find more
            // (n and n1 can be negative)
            break;
        }
    }
    if (len(result) == 0) {
        result = default!;
    }
    return result;
}

} // end suffixarray_package
