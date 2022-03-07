// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || plan9 || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd plan9 solaris

// Unix cryptographically secure pseudorandom number
// generator.

// package rand -- go2cs converted at 2022 March 06 22:17:21 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\rand_unix.go
using bufio = go.bufio_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using binary = go.encoding.binary_package;
using io = go.io_package;
using os = go.os_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using System;


namespace go.crypto;

public static partial class rand_package {

private static readonly @string urandomDevice = "/dev/urandom";

// Easy implementation: read from /dev/urandom.
// This is sufficient on Linux, OS X, and FreeBSD.



// Easy implementation: read from /dev/urandom.
// This is sufficient on Linux, OS X, and FreeBSD.

private static void init() {
    if (runtime.GOOS == "plan9") {
        Reader = newReader(null);
    }
    else
 {
        Reader = addr(new devReader(name:urandomDevice));
    }
}

// A devReader satisfies reads by reading the file named name.
private partial struct devReader {
    public @string name;
    public io.Reader f;
    public sync.Mutex mu;
    public int used; // atomic; whether this devReader has been used
}

// altGetRandom if non-nil specifies an OS-specific function to get
// urandom-style randomness.
private static Func<slice<byte>, bool> altGetRandom = default;

private static void warnBlocked() {
    println("crypto/rand: blocked for 60 seconds waiting to read random data from the kernel");
}

private static (nint, error) Read(this ptr<devReader> _addr_r, slice<byte> b) => func((defer, _, _) => {
    nint n = default;
    error err = default!;
    ref devReader r = ref _addr_r.val;

    if (atomic.CompareAndSwapInt32(_addr_r.used, 0, 1)) { 
        // First use of randomness. Start timer to warn about
        // being blocked on entropy not being available.
        var t = time.AfterFunc(60 * time.Second, warnBlocked);
        defer(t.Stop());

    }
    if (altGetRandom != null && r.name == urandomDevice && altGetRandom(b)) {
        return (len(b), error.As(null!)!);
    }
    r.mu.Lock();
    defer(r.mu.Unlock());
    if (r.f == null) {
        var (f, err) = os.Open(r.name);
        if (f == null) {
            return (0, error.As(err)!);
        }
        if (runtime.GOOS == "plan9") {
            r.f = f;
        }
        else
 {
            r.f = bufio.NewReader(new hideAgainReader(f));
        }
    }
    return r.f.Read(b);

});

private static Func<error, bool> isEAGAIN = default; // set by eagain.go on unix systems

// hideAgainReader masks EAGAIN reads from /dev/urandom.
// See golang.org/issue/9205
private partial struct hideAgainReader {
    public io.Reader r;
}

private static (nint, error) Read(this hideAgainReader hr, slice<byte> p) {
    nint n = default;
    error err = default!;

    n, err = hr.r.Read(p);
    if (err != null && isEAGAIN != null && isEAGAIN(err)) {
        err = null;
    }
    return ;

}

// Alternate pseudo-random implementation for use on
// systems without a reliable /dev/urandom.

// newReader returns a new pseudorandom generator that
// seeds itself by reading from entropy. If entropy == nil,
// the generator seeds itself by reading from the system's
// random number generator, typically /dev/random.
// The Read method on the returned reader always returns
// the full amount asked for, or else it returns an error.
//
// The generator uses the X9.31 algorithm with AES-128,
// reseeding after every 1 MB of generated data.
private static io.Reader newReader(io.Reader entropy) {
    if (entropy == null) {
        entropy = addr(new devReader(name:"/dev/random"));
    }
    return addr(new reader(entropy:entropy));

}

private partial struct reader {
    public sync.Mutex mu;
    public nint budget; // number of bytes that can be generated
    public cipher.Block cipher;
    public io.Reader entropy;
    public array<byte> time;
    public array<byte> seed;
    public array<byte> dst;
    public array<byte> key;
}

private static (nint, error) Read(this ptr<reader> _addr_r, slice<byte> b) => func((defer, _, _) => {
    nint n = default;
    error err = default!;
    ref reader r = ref _addr_r.val;

    r.mu.Lock();
    defer(r.mu.Unlock());
    n = len(b);

    while (len(b) > 0) {
        if (r.budget == 0) {
            var (_, err) = io.ReadFull(r.entropy, r.seed[(int)0..]);
            if (err != null) {
                return (n - len(b), error.As(err)!);
            }
            _, err = io.ReadFull(r.entropy, r.key[(int)0..]);
            if (err != null) {
                return (n - len(b), error.As(err)!);
            }
            r.cipher, err = aes.NewCipher(r.key[(int)0..]);
            if (err != null) {
                return (n - len(b), error.As(err)!);
            }
            r.budget = 1 << 20; // reseed after generating 1MB
        }
        r.budget -= aes.BlockSize; 

        // ANSI X9.31 (== X9.17) algorithm, but using AES in place of 3DES.
        //
        // single block:
        // t = encrypt(time)
        // dst = encrypt(t^seed)
        // seed = encrypt(t^dst)
        var ns = time.Now().UnixNano();
        binary.BigEndian.PutUint64(r.time[..], uint64(ns));
        r.cipher.Encrypt(r.time[(int)0..], r.time[(int)0..]);
        {
            nint i__prev2 = i;

            for (nint i = 0; i < aes.BlockSize; i++) {
                r.dst[i] = r.time[i] ^ r.seed[i];
            }


            i = i__prev2;
        }
        r.cipher.Encrypt(r.dst[(int)0..], r.dst[(int)0..]);
        {
            nint i__prev2 = i;

            for (i = 0; i < aes.BlockSize; i++) {
                r.seed[i] = r.time[i] ^ r.dst[i];
            }


            i = i__prev2;
        }
        r.cipher.Encrypt(r.seed[(int)0..], r.seed[(int)0..]);

        var m = copy(b, r.dst[(int)0..]);
        b = b[(int)m..];

    }

    return (n, error.As(null!)!);

});

} // end rand_package
