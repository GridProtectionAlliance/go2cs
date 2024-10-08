// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cache -- go2cs converted at 2022 March 13 06:30:35 UTC
// import "cmd/go/internal/cache" ==> using cache = go.cmd.go.@internal.cache_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\cache\hash.go
namespace go.cmd.go.@internal;

using bytes = bytes_package;
using sha256 = crypto.sha256_package;
using fmt = fmt_package;
using hash = hash_package;
using io = io_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;

public static partial class cache_package {

private static var debugHash = false; // set when GODEBUG=gocachehash=1

// HashSize is the number of bytes in a hash.
public static readonly nint HashSize = 32;

// A Hash provides access to the canonical hash function used to index the cache.
// The current implementation uses salted SHA256, but clients must not assume this.


// A Hash provides access to the canonical hash function used to index the cache.
// The current implementation uses salted SHA256, but clients must not assume this.
public partial struct Hash {
    public hash.Hash h;
    public @string name; // for debugging
    public ptr<bytes.Buffer> buf; // for verify
}

// hashSalt is a salt string added to the beginning of every hash
// created by NewHash. Using the Go version makes sure that different
// versions of the go command (or even different Git commits during
// work on the development branch) do not address the same cache
// entries, so that a bug in one version does not affect the execution
// of other versions. This salt will result in additional ActionID files
// in the cache, but not additional copies of the large output files,
// which are still addressed by unsalted SHA256.
//
// We strip any GOEXPERIMENTs the go tool was built with from this
// version string on the assumption that they shouldn't affect go tool
// execution. This allows bootstrapping to converge faster: dist builds
// go_bootstrap without any experiments, so by stripping experiments
// go_bootstrap and the final go binary will use the same salt.
private static slice<byte> hashSalt = (slice<byte>)stripExperiment(runtime.Version());

// stripExperiment strips any GOEXPERIMENT configuration from the Go
// version string.
private static @string stripExperiment(@string version) {
    {
        var i = strings.Index(version, " X:");

        if (i >= 0) {
            return version[..(int)i];
        }
    }
    return version;
}

// Subkey returns an action ID corresponding to mixing a parent
// action ID with a string description of the subkey.
public static ActionID Subkey(ActionID parent, @string desc) {
    var h = sha256.New();
    h.Write((slice<byte>)"subkey:");
    h.Write(parent[..]);
    h.Write((slice<byte>)desc);
    ActionID @out = default;
    h.Sum(out[..(int)0]);
    if (debugHash) {
        fmt.Fprintf(os.Stderr, "HASH subkey %x %q = %x\n", parent, desc, out);
    }
    if (verify) {
        hashDebug.Lock();
        hashDebug.m[out] = fmt.Sprintf("subkey %x %q", parent, desc);
        hashDebug.Unlock();
    }
    return out;
}

// NewHash returns a new Hash.
// The caller is expected to Write data to it and then call Sum.
public static ptr<Hash> NewHash(@string name) {
    ptr<Hash> h = addr(new Hash(h:sha256.New(),name:name));
    if (debugHash) {
        fmt.Fprintf(os.Stderr, "HASH[%s]\n", h.name);
    }
    h.Write(hashSalt);
    if (verify) {
        h.buf = @new<bytes.Buffer>();
    }
    return _addr_h!;
}

// Write writes data to the running hash.
private static (nint, error) Write(this ptr<Hash> _addr_h, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Hash h = ref _addr_h.val;

    if (debugHash) {
        fmt.Fprintf(os.Stderr, "HASH[%s]: %q\n", h.name, b);
    }
    if (h.buf != null) {
        h.buf.Write(b);
    }
    return h.h.Write(b);
}

// Sum returns the hash of the data written previously.
private static array<byte> Sum(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    array<byte> @out = new array<byte>(HashSize);
    h.h.Sum(out[..(int)0]);
    if (debugHash) {
        fmt.Fprintf(os.Stderr, "HASH[%s]: %x\n", h.name, out);
    }
    if (h.buf != null) {
        hashDebug.Lock();
        if (hashDebug.m == null) {
            hashDebug.m = make_map<array<byte>, @string>();
        }
        hashDebug.m[out] = h.buf.String();
        hashDebug.Unlock();
    }
    return out;
}

// In GODEBUG=gocacheverify=1 mode,
// hashDebug holds the input to every computed hash ID,
// so that we can work backward from the ID involved in a
// cache entry mismatch to a description of what should be there.
private static var hashDebug = default;

// reverseHash returns the input used to compute the hash id.
private static @string reverseHash(array<byte> id) {
    id = id.Clone();

    hashDebug.Lock();
    var s = hashDebug.m[id];
    hashDebug.Unlock();
    return s;
}

private static var hashFileCache = default;

// FileHash returns the hash of the named file.
// It caches repeated lookups for a given file,
// and the cache entry for a file can be initialized
// using SetFileHash.
// The hash used by FileHash is not the same as
// the hash used by NewHash.
public static (array<byte>, error) FileHash(@string file) {
    array<byte> _p0 = default;
    error _p0 = default!;

    hashFileCache.Lock();
    var (out, ok) = hashFileCache.m[file];
    hashFileCache.Unlock();

    if (ok) {
        return (out, error.As(null!)!);
    }
    var h = sha256.New();
    var (f, err) = os.Open(file);
    if (err != null) {
        if (debugHash) {
            fmt.Fprintf(os.Stderr, "HASH %s: %v\n", file, err);
        }
        return (new array<byte>(new byte[] {  }), error.As(err)!);
    }
    _, err = io.Copy(h, f);
    f.Close();
    if (err != null) {
        if (debugHash) {
            fmt.Fprintf(os.Stderr, "HASH %s: %v\n", file, err);
        }
        return (new array<byte>(new byte[] {  }), error.As(err)!);
    }
    h.Sum(out[..(int)0]);
    if (debugHash) {
        fmt.Fprintf(os.Stderr, "HASH %s: %x\n", file, out);
    }
    SetFileHash(file, out);
    return (out, error.As(null!)!);
}

// SetFileHash sets the hash returned by FileHash for file.
public static void SetFileHash(@string file, array<byte> sum) {
    sum = sum.Clone();

    hashFileCache.Lock();
    if (hashFileCache.m == null) {
        hashFileCache.m = make_map<@string, array<byte>>();
    }
    hashFileCache.m[file] = sum;
    hashFileCache.Unlock();
}

} // end cache_package
