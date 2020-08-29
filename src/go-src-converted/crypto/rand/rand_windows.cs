// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows cryptographically secure pseudorandom number
// generator.

// package rand -- go2cs converted at 2020 August 29 08:30:52 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\rand_windows.go
using os = go.os_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
        // Implemented by using Windows CryptoAPI 2.0.
        private static void init()
        {
            Reader = ref new rngReader();

        }

        // A rngReader satisfies reads by reading from the Windows CryptGenRandom API.
        private partial struct rngReader
        {
            public syscall.Handle prov;
            public sync.Mutex mu;
        }

        private static (long, error) Read(this ref rngReader r, slice<byte> b)
        {
            r.mu.Lock();
            if (r.prov == 0L)
            {
                const var provType = syscall.PROV_RSA_FULL;

                const var flags = syscall.CRYPT_VERIFYCONTEXT | syscall.CRYPT_SILENT;

                var err = syscall.CryptAcquireContext(ref r.prov, null, null, provType, flags);
                if (err != null)
                {
                    r.mu.Unlock();
                    return (0L, os.NewSyscallError("CryptAcquireContext", err));
                }
            }
            r.mu.Unlock();

            if (len(b) == 0L)
            {
                return (0L, null);
            }
            err = syscall.CryptGenRandom(r.prov, uint32(len(b)), ref b[0L]);
            if (err != null)
            {
                return (0L, os.NewSyscallError("CryptGenRandom", err));
            }
            return (len(b), null);
        }
    }
}}
