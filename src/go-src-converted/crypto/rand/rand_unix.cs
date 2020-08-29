// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd plan9 solaris

// Unix cryptographically secure pseudorandom number
// generator.

// package rand -- go2cs converted at 2020 August 29 08:30:52 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\rand_unix.go
using bufio = go.bufio_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using io = go.io_package;
using os = go.os_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
        private static readonly @string urandomDevice = "/dev/urandom";

        // Easy implementation: read from /dev/urandom.
        // This is sufficient on Linux, OS X, and FreeBSD.



        // Easy implementation: read from /dev/urandom.
        // This is sufficient on Linux, OS X, and FreeBSD.

        private static void init()
        {
            if (runtime.GOOS == "plan9")
            {
                Reader = newReader(null);
            }
            else
            {
                Reader = ref new devReader(name:urandomDevice);
            }
        }

        // A devReader satisfies reads by reading the file named name.
        private partial struct devReader
        {
            public @string name;
            public io.Reader f;
            public sync.Mutex mu;
        }

        // altGetRandom if non-nil specifies an OS-specific function to get
        // urandom-style randomness.
        private static Func<slice<byte>, bool> altGetRandom = default;

        private static (long, error) Read(this ref devReader _r, slice<byte> b) => func(_r, (ref devReader r, Defer defer, Panic _, Recover __) =>
        {
            if (altGetRandom != null && r.name == urandomDevice && altGetRandom(b))
            {
                return (len(b), null);
            }
            r.mu.Lock();
            defer(r.mu.Unlock());
            if (r.f == null)
            {
                var (f, err) = os.Open(r.name);
                if (f == null)
                {
                    return (0L, err);
                }
                if (runtime.GOOS == "plan9")
                {
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
        private partial struct hideAgainReader
        {
            public io.Reader r;
        }

        private static (long, error) Read(this hideAgainReader hr, slice<byte> p)
        {
            n, err = hr.r.Read(p);
            if (err != null && isEAGAIN != null && isEAGAIN(err))
            {
                err = null;
            }
            return;
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
        private static io.Reader newReader(io.Reader entropy)
        {
            if (entropy == null)
            {
                entropy = ref new devReader(name:"/dev/random");
            }
            return ref new reader(entropy:entropy);
        }

        private partial struct reader
        {
            public sync.Mutex mu;
            public long budget; // number of bytes that can be generated
            public cipher.Block cipher;
            public io.Reader entropy;
            public array<byte> time;
            public array<byte> seed;
            public array<byte> dst;
            public array<byte> key;
        }

        private static (long, error) Read(this ref reader _r, slice<byte> b) => func(_r, (ref reader r, Defer defer, Panic _, Recover __) =>
        {
            r.mu.Lock();
            defer(r.mu.Unlock());
            n = len(b);

            while (len(b) > 0L)
            {
                if (r.budget == 0L)
                {
                    var (_, err) = io.ReadFull(r.entropy, r.seed[0L..]);
                    if (err != null)
                    {
                        return (n - len(b), err);
                    }
                    _, err = io.ReadFull(r.entropy, r.key[0L..]);
                    if (err != null)
                    {
                        return (n - len(b), err);
                    }
                    r.cipher, err = aes.NewCipher(r.key[0L..]);
                    if (err != null)
                    {
                        return (n - len(b), err);
                    }
                    r.budget = 1L << (int)(20L); // reseed after generating 1MB
                }
                r.budget -= aes.BlockSize; 

                // ANSI X9.31 (== X9.17) algorithm, but using AES in place of 3DES.
                //
                // single block:
                // t = encrypt(time)
                // dst = encrypt(t^seed)
                // seed = encrypt(t^dst)
                var ns = time.Now().UnixNano();
                r.time[0L] = byte(ns >> (int)(56L));
                r.time[1L] = byte(ns >> (int)(48L));
                r.time[2L] = byte(ns >> (int)(40L));
                r.time[3L] = byte(ns >> (int)(32L));
                r.time[4L] = byte(ns >> (int)(24L));
                r.time[5L] = byte(ns >> (int)(16L));
                r.time[6L] = byte(ns >> (int)(8L));
                r.time[7L] = byte(ns);
                r.cipher.Encrypt(r.time[0L..], r.time[0L..]);
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < aes.BlockSize; i++)
                    {
                        r.dst[i] = r.time[i] ^ r.seed[i];
                    }


                    i = i__prev2;
                }
                r.cipher.Encrypt(r.dst[0L..], r.dst[0L..]);
                {
                    long i__prev2 = i;

                    for (i = 0L; i < aes.BlockSize; i++)
                    {
                        r.seed[i] = r.time[i] ^ r.dst[i];
                    }


                    i = i__prev2;
                }
                r.cipher.Encrypt(r.seed[0L..], r.seed[0L..]);

                var m = copy(b, r.dst[0L..]);
                b = b[m..];
            }


            return (n, null);
        });
    }
}}
