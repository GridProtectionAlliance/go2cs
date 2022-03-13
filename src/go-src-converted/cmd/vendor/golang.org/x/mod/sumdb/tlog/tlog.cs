// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package tlog implements a tamper-evident log
// used in the Go module go.sum database server.
//
// This package follows the design of Certificate Transparency (RFC 6962)
// and its proofs are compatible with that system.
// See TestCertificateTransparency.
//

// package tlog -- go2cs converted at 2022 March 13 06:41:12 UTC
// import "cmd/vendor/golang.org/x/mod/sumdb/tlog" ==> using tlog = go.cmd.vendor.golang.org.x.mod.sumdb.tlog_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\sumdb\tlog\tlog.go
namespace go.cmd.vendor.golang.org.x.mod.sumdb;

using sha256 = crypto.sha256_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using bits = math.bits_package;


// A Hash is a hash identifying a log record or tree root.

public static partial class tlog_package {

public partial struct Hash { // : array<byte>
}

// HashSize is the size of a Hash in bytes.
public static readonly nint HashSize = 32;

// String returns a base64 representation of the hash for printing.


// String returns a base64 representation of the hash for printing.
public static @string String(this Hash h) {
    return base64.StdEncoding.EncodeToString(h[..]);
}

// MarshalJSON marshals the hash as a JSON string containing the base64-encoded hash.
public static (slice<byte>, error) MarshalJSON(this Hash h) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return ((slice<byte>)"\"" + h.String() + "\"", error.As(null!)!);
}

// UnmarshalJSON unmarshals a hash from JSON string containing the a base64-encoded hash.
private static error UnmarshalJSON(this ptr<Hash> _addr_h, slice<byte> data) {
    ref Hash h = ref _addr_h.val;

    if (len(data) != 1 + 44 + 1 || data[0] != '"' || data[len(data) - 2] != '=' || data[len(data) - 1] != '"') {
        return error.As(errors.New("cannot decode hash"))!;
    }
    Hash tmp = default;
    var (n, err) = base64.RawStdEncoding.Decode(tmp[..], data[(int)1..(int)len(data) - 2]);
    if (err != null || n != HashSize) {
        return error.As(errors.New("cannot decode hash"))!;
    }
    h.val = tmp;
    return error.As(null!)!;
}

// ParseHash parses the base64-encoded string form of a hash.
public static (Hash, error) ParseHash(@string s) {
    Hash _p0 = default;
    error _p0 = default!;

    var (data, err) = base64.StdEncoding.DecodeString(s);
    if (err != null || len(data) != HashSize) {
        return (new Hash(), error.As(fmt.Errorf("malformed hash"))!);
    }
    Hash h = default;
    copy(h[..], data);
    return (h, error.As(null!)!);
}

// maxpow2 returns k, the maximum power of 2 smaller than n,
// as well as l = log₂ k (so k = 1<<l).
private static (long, nint) maxpow2(long n) {
    long k = default;
    nint l = default;

    l = 0;
    while (1 << (int)(uint(l + 1)) < n) {
        l++;
    }
    return (1 << (int)(uint(l)), l);
}

private static byte zeroPrefix = new slice<byte>(new byte[] { 0x00 });

// RecordHash returns the content hash for the given record data.
public static Hash RecordHash(slice<byte> data) { 
    // SHA256(0x00 || data)
    // https://tools.ietf.org/html/rfc6962#section-2.1
    var h = sha256.New();
    h.Write(zeroPrefix);
    h.Write(data);
    Hash h1 = default;
    h.Sum(h1[..(int)0]);
    return h1;
}

// NodeHash returns the hash for an interior tree node with the given left and right hashes.
public static Hash NodeHash(Hash left, Hash right) { 
    // SHA256(0x01 || left || right)
    // https://tools.ietf.org/html/rfc6962#section-2.1
    // We use a stack buffer to assemble the hash input
    // to avoid allocating a hash struct with sha256.New.
    array<byte> buf = new array<byte>(1 + HashSize + HashSize);
    buf[0] = 0x01;
    copy(buf[(int)1..], left[..]);
    copy(buf[(int)1 + HashSize..], right[..]);
    return sha256.Sum256(buf[..]);
}

// For information about the stored hash index ordering,
// see section 3.3 of Crosby and Wallach's paper
// "Efficient Data Structures for Tamper-Evident Logging".
// https://www.usenix.org/legacy/event/sec09/tech/full_papers/crosby.pdf

// StoredHashIndex maps the tree coordinates (level, n)
// to a dense linear ordering that can be used for hash storage.
// Hash storage implementations that store hashes in sequential
// storage can use this function to compute where to read or write
// a given hash.
public static long StoredHashIndex(nint level, long n) { 
    // Level L's n'th hash is written right after level L+1's 2n+1'th hash.
    // Work our way down to the level 0 ordering.
    // We'll add back the original level count at the end.
    for (var l = level; l > 0; l--) {
        n = 2 * n + 1;
    } 

    // Level 0's n'th hash is written at n+n/2+n/4+... (eventually n/2ⁱ hits zero).
    var i = int64(0);
    while (n > 0) {
        i += n;
        n>>=1;
    }

    return i + int64(level);
}

// SplitStoredHashIndex is the inverse of StoredHashIndex.
// That is, SplitStoredHashIndex(StoredHashIndex(level, n)) == level, n.
public static (nint, long) SplitStoredHashIndex(long index) => func((_, panic, _) => {
    nint level = default;
    long n = default;
 
    // Determine level 0 record before index.
    // StoredHashIndex(0, n) < 2*n,
    // so the n we want is in [index/2, index/2+log₂(index)].
    n = index / 2;
    var indexN = StoredHashIndex(0, n);
    if (indexN > index) {
        panic("bad math");
    }
    while (true) { 
        // Each new record n adds 1 + trailingZeros(n) hashes.
        var x = indexN + 1 + int64(bits.TrailingZeros64(uint64(n + 1)));
        if (x > index) {
            break;
        }
        n++;
        indexN = x;
    } 
    // The hash we want was committed with record n,
    // meaning it is one of (0, n), (1, n/2), (2, n/4), ...
    level = int(index - indexN);
    return (level, n >> (int)(uint(level)));
});

// StoredHashCount returns the number of stored hashes
// that are expected for a tree with n records.
public static long StoredHashCount(long n) {
    if (n == 0) {
        return 0;
    }
    var numHash = StoredHashIndex(0, n - 1) + 1; 
    // And it will have any hashes for subtrees completed by that leaf.
    {
        var i = uint64(n - 1);

        while (i & 1 != 0) {
            numHash++;
            i>>=1;
        }
    }
    return numHash;
}

// StoredHashes returns the hashes that must be stored when writing
// record n with the given data. The hashes should be stored starting
// at StoredHashIndex(0, n). The result will have at most 1 + log₂ n hashes,
// but it will average just under two per call for a sequence of calls for n=1..k.
//
// StoredHashes may read up to log n earlier hashes from r
// in order to compute hashes for completed subtrees.
public static (slice<Hash>, error) StoredHashes(long n, slice<byte> data, HashReader r) {
    slice<Hash> _p0 = default;
    error _p0 = default!;

    return StoredHashesForRecordHash(n, RecordHash(data), r);
}

// StoredHashesForRecordHash is like StoredHashes but takes
// as its second argument RecordHash(data) instead of data itself.
public static (slice<Hash>, error) StoredHashesForRecordHash(long n, Hash h, HashReader r) {
    slice<Hash> _p0 = default;
    error _p0 = default!;
 
    // Start with the record hash.
    Hash hashes = new slice<Hash>(new Hash[] { h }); 

    // Build list of indexes needed for hashes for completed subtrees.
    // Each trailing 1 bit in the binary representation of n completes a subtree
    // and consumes a hash from an adjacent subtree.
    var m = int(bits.TrailingZeros64(uint64(n + 1)));
    var indexes = make_slice<long>(m);
    {
        nint i__prev1 = i;

        for (nint i = 0; i < m; i++) { 
            // We arrange indexes in sorted order.
            // Note that n>>i is always odd.
            indexes[m - 1 - i] = StoredHashIndex(i, n >> (int)(uint(i)) - 1);
        }

        i = i__prev1;
    } 

    // Fetch hashes.
    var (old, err) = r.ReadHashes(indexes);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (len(old) != len(indexes)) {
        return (null, error.As(fmt.Errorf("tlog: ReadHashes(%d indexes) = %d hashes", len(indexes), len(old)))!);
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < m; i++) {
            h = NodeHash(old[m - 1 - i], h);
            hashes = append(hashes, h);
        }

        i = i__prev1;
    }
    return (hashes, error.As(null!)!);
}

// A HashReader can read hashes for nodes in the log's tree structure.
public partial interface HashReader {
    (slice<Hash>, error) ReadHashes(slice<long> indexes);
}

// A HashReaderFunc is a function implementing HashReader.
public delegate  error) HashReaderFunc(slice<long>,  (slice<Hash>);

public static (slice<Hash>, error) ReadHashes(this HashReaderFunc f, slice<long> indexes) {
    slice<Hash> _p0 = default;
    error _p0 = default!;

    return f(indexes);
}

// TreeHash computes the hash for the root of the tree with n records,
// using the HashReader to obtain previously stored hashes
// (those returned by StoredHashes during the writes of those n records).
// TreeHash makes a single call to ReadHash requesting at most 1 + log₂ n hashes.
// The tree of size zero is defined to have an all-zero Hash.
public static (Hash, error) TreeHash(long n, HashReader r) => func((_, panic, _) => {
    Hash _p0 = default;
    error _p0 = default!;

    if (n == 0) {
        return (new Hash(), error.As(null!)!);
    }
    var indexes = subTreeIndex(0, n, null);
    var (hashes, err) = r.ReadHashes(indexes);
    if (err != null) {
        return (new Hash(), error.As(err)!);
    }
    if (len(hashes) != len(indexes)) {
        return (new Hash(), error.As(fmt.Errorf("tlog: ReadHashes(%d indexes) = %d hashes", len(indexes), len(hashes)))!);
    }
    var (hash, hashes) = subTreeHash(0, n, hashes);
    if (len(hashes) != 0) {
        panic("tlog: bad index math in TreeHash");
    }
    return (hash, error.As(null!)!);
});

// subTreeIndex returns the storage indexes needed to compute
// the hash for the subtree containing records [lo, hi),
// appending them to need and returning the result.
// See https://tools.ietf.org/html/rfc6962#section-2.1
private static slice<long> subTreeIndex(long lo, long hi, slice<long> need) => func((_, panic, _) => { 
    // See subTreeHash below for commentary.
    while (lo < hi) {
        var (k, level) = maxpow2(hi - lo + 1);
        if (lo & (k - 1) != 0) {
            panic("tlog: bad math in subTreeIndex");
        }
        need = append(need, StoredHashIndex(level, lo >> (int)(uint(level))));
        lo += k;
    }
    return need;
});

// subTreeHash computes the hash for the subtree containing records [lo, hi),
// assuming that hashes are the hashes corresponding to the indexes
// returned by subTreeIndex(lo, hi).
// It returns any leftover hashes.
private static (Hash, slice<Hash>) subTreeHash(long lo, long hi, slice<Hash> hashes) => func((_, panic, _) => {
    Hash _p0 = default;
    slice<Hash> _p0 = default;
 
    // Repeatedly partition the tree into a left side with 2^level nodes,
    // for as large a level as possible, and a right side with the fringe.
    // The left hash is stored directly and can be read from storage.
    // The right side needs further computation.
    nint numTree = 0;
    while (lo < hi) {
        var (k, _) = maxpow2(hi - lo + 1);
        if (lo & (k - 1) != 0 || lo >= hi) {
            panic("tlog: bad math in subTreeHash");
        }
        numTree++;
        lo += k;
    }

    if (len(hashes) < numTree) {
        panic("tlog: bad index math in subTreeHash");
    }
    var h = hashes[numTree - 1];
    for (var i = numTree - 2; i >= 0; i--) {
        h = NodeHash(hashes[i], h);
    }
    return (h, hashes[(int)numTree..]);
});

// A RecordProof is a verifiable proof that a particular log root contains a particular record.
// RFC 6962 calls this a “Merkle audit path.”
public partial struct RecordProof { // : slice<Hash>
}

// ProveRecord returns the proof that the tree of size t contains the record with index n.
public static (RecordProof, error) ProveRecord(long t, long n, HashReader r) => func((_, panic, _) => {
    RecordProof _p0 = default;
    error _p0 = default!;

    if (t < 0 || n < 0 || n >= t) {
        return (null, error.As(fmt.Errorf("tlog: invalid inputs in ProveRecord"))!);
    }
    var indexes = leafProofIndex(0, t, n, null);
    if (len(indexes) == 0) {
        return (new RecordProof(), error.As(null!)!);
    }
    var (hashes, err) = r.ReadHashes(indexes);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (len(hashes) != len(indexes)) {
        return (null, error.As(fmt.Errorf("tlog: ReadHashes(%d indexes) = %d hashes", len(indexes), len(hashes)))!);
    }
    var (p, hashes) = leafProof(0, t, n, hashes);
    if (len(hashes) != 0) {
        panic("tlog: bad index math in ProveRecord");
    }
    return (p, error.As(null!)!);
});

// leafProofIndex builds the list of indexes needed to construct the proof
// that leaf n is contained in the subtree with leaves [lo, hi).
// It appends those indexes to need and returns the result.
// See https://tools.ietf.org/html/rfc6962#section-2.1.1
private static slice<long> leafProofIndex(long lo, long hi, long n, slice<long> need) => func((_, panic, _) => { 
    // See leafProof below for commentary.
    if (!(lo <= n && n < hi)) {
        panic("tlog: bad math in leafProofIndex");
    }
    if (lo + 1 == hi) {
        return need;
    }
    {
        var (k, _) = maxpow2(hi - lo);

        if (n < lo + k) {
            need = leafProofIndex(lo, lo + k, n, need);
            need = subTreeIndex(lo + k, hi, need);
        }
        else
 {
            need = subTreeIndex(lo, lo + k, need);
            need = leafProofIndex(lo + k, hi, n, need);
        }
    }
    return need;
});

// leafProof constructs the proof that leaf n is contained in the subtree with leaves [lo, hi).
// It returns any leftover hashes as well.
// See https://tools.ietf.org/html/rfc6962#section-2.1.1
private static (RecordProof, slice<Hash>) leafProof(long lo, long hi, long n, slice<Hash> hashes) => func((_, panic, _) => {
    RecordProof _p0 = default;
    slice<Hash> _p0 = default;
 
    // We must have lo <= n < hi or else the code here has a bug.
    if (!(lo <= n && n < hi)) {
        panic("tlog: bad math in leafProof");
    }
    if (lo + 1 == hi) { // n == lo
        // Reached the leaf node.
        // The verifier knows what the leaf hash is, so we don't need to send it.
        return (new RecordProof(), hashes);
    }
    RecordProof p = default;
    Hash th = default;
    {
        var (k, _) = maxpow2(hi - lo);

        if (n < lo + k) { 
            // n is on left side
            p, hashes = leafProof(lo, lo + k, n, hashes);
            th, hashes = subTreeHash(lo + k, hi, hashes);
        }
        else
 { 
            // n is on right side
            th, hashes = subTreeHash(lo, lo + k, hashes);
            p, hashes = leafProof(lo + k, hi, n, hashes);
        }
    }
    return (append(p, th), hashes);
});

private static var errProofFailed = errors.New("invalid transparency proof");

// CheckRecord verifies that p is a valid proof that the tree of size t
// with hash th has an n'th record with hash h.
public static error CheckRecord(RecordProof p, long t, Hash th, long n, Hash h) {
    if (t < 0 || n < 0 || n >= t) {
        return error.As(fmt.Errorf("tlog: invalid inputs in CheckRecord"))!;
    }
    var (th2, err) = runRecordProof(p, 0, t, n, h);
    if (err != null) {
        return error.As(err)!;
    }
    if (th2 == th) {
        return error.As(null!)!;
    }
    return error.As(errProofFailed)!;
}

// runRecordProof runs the proof p that leaf n is contained in the subtree with leaves [lo, hi).
// Running the proof means constructing and returning the implied hash of that
// subtree.
private static (Hash, error) runRecordProof(RecordProof p, long lo, long hi, long n, Hash leafHash) => func((_, panic, _) => {
    Hash _p0 = default;
    error _p0 = default!;
 
    // We must have lo <= n < hi or else the code here has a bug.
    if (!(lo <= n && n < hi)) {
        panic("tlog: bad math in runRecordProof");
    }
    if (lo + 1 == hi) { // m == lo
        // Reached the leaf node.
        // The proof must not have any unnecessary hashes.
        if (len(p) != 0) {
            return (new Hash(), error.As(errProofFailed)!);
        }
        return (leafHash, error.As(null!)!);
    }
    if (len(p) == 0) {
        return (new Hash(), error.As(errProofFailed)!);
    }
    var (k, _) = maxpow2(hi - lo);
    if (n < lo + k) {
        var (th, err) = runRecordProof(p[..(int)len(p) - 1], lo, lo + k, n, leafHash);
        if (err != null) {
            return (new Hash(), error.As(err)!);
        }
        return (NodeHash(th, p[len(p) - 1]), error.As(null!)!);
    }
    else
 {
        (th, err) = runRecordProof(p[..(int)len(p) - 1], lo + k, hi, n, leafHash);
        if (err != null) {
            return (new Hash(), error.As(err)!);
        }
        return (NodeHash(p[len(p) - 1], th), error.As(null!)!);
    }
});

// A TreeProof is a verifiable proof that a particular log tree contains
// as a prefix all records present in an earlier tree.
// RFC 6962 calls this a “Merkle consistency proof.”
public partial struct TreeProof { // : slice<Hash>
}

// ProveTree returns the proof that the tree of size t contains
// as a prefix all the records from the tree of smaller size n.
public static (TreeProof, error) ProveTree(long t, long n, HashReader h) => func((_, panic, _) => {
    TreeProof _p0 = default;
    error _p0 = default!;

    if (t < 1 || n < 1 || n > t) {
        return (null, error.As(fmt.Errorf("tlog: invalid inputs in ProveTree"))!);
    }
    var indexes = treeProofIndex(0, t, n, null);
    if (len(indexes) == 0) {
        return (new TreeProof(), error.As(null!)!);
    }
    var (hashes, err) = h.ReadHashes(indexes);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (len(hashes) != len(indexes)) {
        return (null, error.As(fmt.Errorf("tlog: ReadHashes(%d indexes) = %d hashes", len(indexes), len(hashes)))!);
    }
    var (p, hashes) = treeProof(0, t, n, hashes);
    if (len(hashes) != 0) {
        panic("tlog: bad index math in ProveTree");
    }
    return (p, error.As(null!)!);
});

// treeProofIndex builds the list of indexes needed to construct
// the sub-proof related to the subtree containing records [lo, hi).
// See https://tools.ietf.org/html/rfc6962#section-2.1.2.
private static slice<long> treeProofIndex(long lo, long hi, long n, slice<long> need) => func((_, panic, _) => { 
    // See treeProof below for commentary.
    if (!(lo < n && n <= hi)) {
        panic("tlog: bad math in treeProofIndex");
    }
    if (n == hi) {
        if (lo == 0) {
            return need;
        }
        return subTreeIndex(lo, hi, need);
    }
    {
        var (k, _) = maxpow2(hi - lo);

        if (n <= lo + k) {
            need = treeProofIndex(lo, lo + k, n, need);
            need = subTreeIndex(lo + k, hi, need);
        }
        else
 {
            need = subTreeIndex(lo, lo + k, need);
            need = treeProofIndex(lo + k, hi, n, need);
        }
    }
    return need;
});

// treeProof constructs the sub-proof related to the subtree containing records [lo, hi).
// It returns any leftover hashes as well.
// See https://tools.ietf.org/html/rfc6962#section-2.1.2.
private static (TreeProof, slice<Hash>) treeProof(long lo, long hi, long n, slice<Hash> hashes) => func((_, panic, _) => {
    TreeProof _p0 = default;
    slice<Hash> _p0 = default;
 
    // We must have lo < n <= hi or else the code here has a bug.
    if (!(lo < n && n <= hi)) {
        panic("tlog: bad math in treeProof");
    }
    if (n == hi) {
        if (lo == 0) { 
            // This subtree corresponds exactly to the old tree.
            // The verifier knows that hash, so we don't need to send it.
            return (new TreeProof(), hashes);
        }
        var (th, hashes) = subTreeHash(lo, hi, hashes);
        return (new TreeProof(th), hashes);
    }
    TreeProof p = default;
    Hash th = default;
    {
        var (k, _) = maxpow2(hi - lo);

        if (n <= lo + k) { 
            // m is on left side
            p, hashes = treeProof(lo, lo + k, n, hashes);
            th, hashes = subTreeHash(lo + k, hi, hashes);
        }
        else
 { 
            // m is on right side
            th, hashes = subTreeHash(lo, lo + k, hashes);
            p, hashes = treeProof(lo + k, hi, n, hashes);
        }
    }
    return (append(p, th), hashes);
});

// CheckTree verifies that p is a valid proof that the tree of size t with hash th
// contains as a prefix the tree of size n with hash h.
public static error CheckTree(TreeProof p, long t, Hash th, long n, Hash h) {
    if (t < 1 || n < 1 || n > t) {
        return error.As(fmt.Errorf("tlog: invalid inputs in CheckTree"))!;
    }
    var (h2, th2, err) = runTreeProof(p, 0, t, n, h);
    if (err != null) {
        return error.As(err)!;
    }
    if (th2 == th && h2 == h) {
        return error.As(null!)!;
    }
    return error.As(errProofFailed)!;
}

// runTreeProof runs the sub-proof p related to the subtree containing records [lo, hi),
// where old is the hash of the old tree with n records.
// Running the proof means constructing and returning the implied hashes of that
// subtree in both the old and new tree.
private static (Hash, Hash, error) runTreeProof(TreeProof p, long lo, long hi, long n, Hash old) => func((_, panic, _) => {
    Hash _p0 = default;
    Hash _p0 = default;
    error _p0 = default!;
 
    // We must have lo < n <= hi or else the code here has a bug.
    if (!(lo < n && n <= hi)) {
        panic("tlog: bad math in runTreeProof");
    }
    if (n == hi) {
        if (lo == 0) {
            if (len(p) != 0) {
                return (new Hash(), new Hash(), error.As(errProofFailed)!);
            }
            return (old, old, error.As(null!)!);
        }
        if (len(p) != 1) {
            return (new Hash(), new Hash(), error.As(errProofFailed)!);
        }
        return (p[0], p[0], error.As(null!)!);
    }
    if (len(p) == 0) {
        return (new Hash(), new Hash(), error.As(errProofFailed)!);
    }
    var (k, _) = maxpow2(hi - lo);
    if (n <= lo + k) {
        var (oh, th, err) = runTreeProof(p[..(int)len(p) - 1], lo, lo + k, n, old);
        if (err != null) {
            return (new Hash(), new Hash(), error.As(err)!);
        }
        return (oh, NodeHash(th, p[len(p) - 1]), error.As(null!)!);
    }
    else
 {
        (oh, th, err) = runTreeProof(p[..(int)len(p) - 1], lo + k, hi, n, old);
        if (err != null) {
            return (new Hash(), new Hash(), error.As(err)!);
        }
        return (NodeHash(p[len(p) - 1], oh), NodeHash(p[len(p) - 1], th), error.As(null!)!);
    }
});

} // end tlog_package
