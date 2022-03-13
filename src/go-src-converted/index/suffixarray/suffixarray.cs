// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package suffixarray implements substring search in logarithmic time using
// an in-memory suffix array.
//
// Example use:
//
//    // create index for some data
//    index := suffixarray.New(data)
//
//    // lookup byte slice s
//    offsets1 := index.Lookup(s, -1) // the list of all indices where s occurs in data
//    offsets2 := index.Lookup(s, 3)  // the list of at most 3 indices where s occurs in data
//

// package suffixarray -- go2cs converted at 2022 March 13 06:44:30 UTC
// import "index/suffixarray" ==> using suffixarray = go.index.suffixarray_package
// Original source: C:\Program Files\Go\src\index\suffixarray\suffixarray.go
namespace go.index;

using bytes = bytes_package;
using binary = encoding.binary_package;
using errors = errors_package;
using io = io_package;
using math = math_package;
using regexp = regexp_package;
using sort = sort_package;


// Can change for testing

using System;
public static partial class suffixarray_package {

private static nint maxData32 = realMaxData32;

private static readonly var realMaxData32 = math.MaxInt32;

// Index implements a suffix array for fast substring search.


// Index implements a suffix array for fast substring search.
public partial struct Index {
    public slice<byte> data;
    public ints sa; // suffix array for data; sa.len() == len(data)
}

// An ints is either an []int32 or an []int64.
// That is, one of them is empty, and one is the real data.
// The int64 form is used when len(data) > maxData32
private partial struct ints {
    public slice<int> int32;
    public slice<long> int64;
}

private static nint len(this ptr<ints> _addr_a) {
    ref ints a = ref _addr_a.val;

    return len(a.int32) + len(a.int64);
}

private static long get(this ptr<ints> _addr_a, nint i) {
    ref ints a = ref _addr_a.val;

    if (a.int32 != null) {
        return int64(a.int32[i]);
    }
    return a.int64[i];
}

private static void set(this ptr<ints> _addr_a, nint i, long v) {
    ref ints a = ref _addr_a.val;

    if (a.int32 != null) {
        a.int32[i] = int32(v);
    }
    else
 {
        a.int64[i] = v;
    }
}

private static ints slice(this ptr<ints> _addr_a, nint i, nint j) {
    ref ints a = ref _addr_a.val;

    if (a.int32 != null) {
        return new ints(a.int32[i:j],nil);
    }
    return new ints(nil,a.int64[i:j]);
}

// New creates a new Index for data.
// Index creation time is O(N) for N = len(data).
public static ptr<Index> New(slice<byte> data) {
    ptr<Index> ix = addr(new Index(data:data));
    if (len(data) <= maxData32) {
        ix.sa.int32 = make_slice<int>(len(data));
        text_32(data, ix.sa.int32);
    }
    else
 {
        ix.sa.int64 = make_slice<long>(len(data));
        text_64(data, ix.sa.int64);
    }
    return _addr_ix!;
}

// writeInt writes an int x to w using buf to buffer the write.
private static error writeInt(io.Writer w, slice<byte> buf, nint x) {
    binary.PutVarint(buf, int64(x));
    var (_, err) = w.Write(buf[(int)0..(int)binary.MaxVarintLen64]);
    return error.As(err)!;
}

// readInt reads an int x from r using buf to buffer the read and returns x.
private static (long, error) readInt(io.Reader r, slice<byte> buf) {
    long _p0 = default;
    error _p0 = default!;

    var (_, err) = io.ReadFull(r, buf[(int)0..(int)binary.MaxVarintLen64]); // ok to continue with error
    var (x, _) = binary.Varint(buf);
    return (x, error.As(err)!);
}

// writeSlice writes data[:n] to w and returns n.
// It uses buf to buffer the write.
private static (nint, error) writeSlice(io.Writer w, slice<byte> buf, ints data) {
    nint n = default;
    error err = default!;
 
    // encode as many elements as fit into buf
    var p = binary.MaxVarintLen64;
    var m = data.len();
    while (n < m && p + binary.MaxVarintLen64 <= len(buf)) {
        p += binary.PutUvarint(buf[(int)p..], uint64(data.get(n)));
        n++;
    } 

    // update buffer size
    binary.PutVarint(buf, int64(p)); 

    // write buffer
    _, err = w.Write(buf[(int)0..(int)p]);
    return ;
}

private static var errTooBig = errors.New("suffixarray: data too large");

// readSlice reads data[:n] from r and returns n.
// It uses buf to buffer the read.
private static (nint, error) readSlice(io.Reader r, slice<byte> buf, ints data) {
    nint n = default;
    error err = default!;
 
    // read buffer size
    long size64 = default;
    size64, err = readInt(r, buf);
    if (err != null) {
        return ;
    }
    if (int64(int(size64)) != size64 || int(size64) < 0) { 
        // We never write chunks this big anyway.
        return (0, error.As(errTooBig)!);
    }
    var size = int(size64); 

    // read buffer w/o the size
    _, err = io.ReadFull(r, buf[(int)binary.MaxVarintLen64..(int)size]);

    if (err != null) {
        return ;
    }
    for (var p = binary.MaxVarintLen64; p < size; n++) {
        var (x, w) = binary.Uvarint(buf[(int)p..]);
        data.set(n, int64(x));
        p += w;
    }

    return ;
}

private static readonly nint bufSize = 16 << 10; // reasonable for BenchmarkSaveRestore

// Read reads the index from r into x; x must not be nil.
 // reasonable for BenchmarkSaveRestore

// Read reads the index from r into x; x must not be nil.
private static error Read(this ptr<Index> _addr_x, io.Reader r) {
    ref Index x = ref _addr_x.val;
 
    // buffer for all reads
    var buf = make_slice<byte>(bufSize); 

    // read length
    var (n64, err) = readInt(r, buf);
    if (err != null) {
        return error.As(err)!;
    }
    if (int64(int(n64)) != n64 || int(n64) < 0) {
        return error.As(errTooBig)!;
    }
    var n = int(n64); 

    // allocate space
    if (2 * n < cap(x.data) || cap(x.data) < n || x.sa.int32 != null && n > maxData32 || x.sa.int64 != null && n <= maxData32) { 
        // new data is significantly smaller or larger than
        // existing buffers - allocate new ones
        x.data = make_slice<byte>(n);
        x.sa.int32 = null;
        x.sa.int64 = null;
        if (n <= maxData32) {
            x.sa.int32 = make_slice<int>(n);
        }
        else
 {
            x.sa.int64 = make_slice<long>(n);
        }
    }
    else
 { 
        // re-use existing buffers
        x.data = x.data[(int)0..(int)n];
        x.sa = x.sa.slice(0, n);
    }
    {
        var (_, err) = io.ReadFull(r, x.data);

        if (err != null) {
            return error.As(err)!;
        }
    } 

    // read index
    var sa = x.sa;
    while (sa.len() > 0) {
        var (n, err) = readSlice(r, buf, sa);
        if (err != null) {
            return error.As(err)!;
        }
        sa = sa.slice(n, sa.len());
    }
    return error.As(null!)!;
}

// Write writes the index x to w.
private static error Write(this ptr<Index> _addr_x, io.Writer w) {
    ref Index x = ref _addr_x.val;
 
    // buffer for all writes
    var buf = make_slice<byte>(bufSize); 

    // write length
    {
        var err = writeInt(w, buf, len(x.data));

        if (err != null) {
            return error.As(err)!;
        }
    } 

    // write data
    {
        var (_, err) = w.Write(x.data);

        if (err != null) {
            return error.As(err)!;
        }
    } 

    // write index
    var sa = x.sa;
    while (sa.len() > 0) {
        var (n, err) = writeSlice(w, buf, sa);
        if (err != null) {
            return error.As(err)!;
        }
        sa = sa.slice(n, sa.len());
    }
    return error.As(null!)!;
}

// Bytes returns the data over which the index was created.
// It must not be modified.
//
private static slice<byte> Bytes(this ptr<Index> _addr_x) {
    ref Index x = ref _addr_x.val;

    return x.data;
}

private static slice<byte> at(this ptr<Index> _addr_x, nint i) {
    ref Index x = ref _addr_x.val;

    return x.data[(int)x.sa.get(i)..];
}

// lookupAll returns a slice into the matching region of the index.
// The runtime is O(log(N)*len(s)).
private static ints lookupAll(this ptr<Index> _addr_x, slice<byte> s) {
    ref Index x = ref _addr_x.val;
 
    // find matching suffix index range [i:j]
    // find the first index where s would be the prefix
    var i = sort.Search(x.sa.len(), i => bytes.Compare(x.at(i), s) >= 0); 
    // starting at i, find the first index at which s is not a prefix
    var j = i + sort.Search(x.sa.len() - i, j => !bytes.HasPrefix(x.at(j + i), s));
    return x.sa.slice(i, j);
}

// Lookup returns an unsorted list of at most n indices where the byte string s
// occurs in the indexed data. If n < 0, all occurrences are returned.
// The result is nil if s is empty, s is not found, or n == 0.
// Lookup time is O(log(N)*len(s) + len(result)) where N is the
// size of the indexed data.
//
private static slice<nint> Lookup(this ptr<Index> _addr_x, slice<byte> s, nint n) {
    slice<nint> result = default;
    ref Index x = ref _addr_x.val;

    if (len(s) > 0 && n != 0) {
        var matches = x.lookupAll(s);
        var count = matches.len();
        if (n < 0 || count < n) {
            n = count;
        }
        if (n > 0) {
            result = make_slice<nint>(n);
            if (matches.int32 != null) {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in result) {
                        i = __i;
                        result[i] = int(matches.int32[i]);
                    }
            else

                    i = i__prev1;
                }
            } {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in result) {
                        i = __i;
                        result[i] = int(matches.int64[i]);
                    }

                    i = i__prev1;
                }
            }
        }
    }
    return ;
}

// FindAllIndex returns a sorted list of non-overlapping matches of the
// regular expression r, where a match is a pair of indices specifying
// the matched slice of x.Bytes(). If n < 0, all matches are returned
// in successive order. Otherwise, at most n matches are returned and
// they may not be successive. The result is nil if there are no matches,
// or if n == 0.
//
private static slice<slice<nint>> FindAllIndex(this ptr<Index> _addr_x, ptr<regexp.Regexp> _addr_r, nint n) {
    slice<slice<nint>> result = default;
    ref Index x = ref _addr_x.val;
    ref regexp.Regexp r = ref _addr_r.val;
 
    // a non-empty literal prefix is used to determine possible
    // match start indices with Lookup
    var (prefix, complete) = r.LiteralPrefix();
    slice<byte> lit = (slice<byte>)prefix; 

    // worst-case scenario: no literal prefix
    if (prefix == "") {
        return r.FindAllIndex(x.data, n);
    }
    if (complete) { 
        // Lookup returns indices that may belong to overlapping matches.
        // After eliminating them, we may end up with fewer than n matches.
        // If we don't have enough at the end, redo the search with an
        // increased value n1, but only if Lookup returned all the requested
        // indices in the first place (if it returned fewer than that then
        // there cannot be more).
        {
            var n1__prev1 = n1;

            var n1 = n;

            while () {
                var indices = x.Lookup(lit, n1);
                if (len(indices) == 0) {
                    return ;
                n1 += 2 * (n - len(result)); /* overflow ok */
                }
                sort.Ints(indices);
                var pairs = make_slice<nint>(2 * len(indices));
                result = make_slice<slice<nint>>(len(indices));
                nint count = 0;
                nint prev = 0;
                {
                    var i__prev2 = i;

                    foreach (var (_, __i) in indices) {
                        i = __i;
                        if (count == n) {
                            break;
                        } 
                        // ignore indices leading to overlapping matches
                        if (prev <= i) {
                            nint j = 2 * count;
                            pairs[j + 0] = i;
                            pairs[j + 1] = i + len(lit);
                            result[count] = pairs[(int)j..(int)j + 2];
                            count++;
                            prev = i + len(lit);
                        }
                    }

                    i = i__prev2;
                }

                result = result[(int)0..(int)count];
                if (len(result) >= n || len(indices) != n1) { 
                    // found all matches or there's no chance to find more
                    // (n and n1 can be negative)
                    break;
                }
            }


            n1 = n1__prev1;
        }
        if (len(result) == 0) {
            result = null;
        }
        return ;
    }
    r = regexp.MustCompile("^" + r.String()); // compiles because r compiled

    // same comment about Lookup applies here as in the loop above
    {
        var n1__prev1 = n1;

        n1 = n;

        while () {
            indices = x.Lookup(lit, n1);
            if (len(indices) == 0) {
                return ;
            n1 += 2 * (n - len(result)); /* overflow ok */
            }
            sort.Ints(indices);
            result = result[(int)0..(int)0];
            prev = 0;
            {
                var i__prev2 = i;

                foreach (var (_, __i) in indices) {
                    i = __i;
                    if (len(result) == n) {
                        break;
                    }
                    var m = r.FindIndex(x.data[(int)i..]); // anchored search - will not run off
                    // ignore indices leading to overlapping matches
                    if (m != null && prev <= i) {
                        m[0] = i; // correct m
                        m[1] += i;
                        result = append(result, m);
                        prev = m[1];
                    }
                }

                i = i__prev2;
            }

            if (len(result) >= n || len(indices) != n1) { 
                // found all matches or there's no chance to find more
                // (n and n1 can be negative)
                break;
            }
        }

        n1 = n1__prev1;
    }
    if (len(result) == 0) {
        result = null;
    }
    return ;
}

} // end suffixarray_package
