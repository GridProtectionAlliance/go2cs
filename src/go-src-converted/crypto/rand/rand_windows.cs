// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows cryptographically secure pseudorandom number
// generator.

// package rand -- go2cs converted at 2020 October 09 04:53:06 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Go\src\crypto\rand\rand_windows.go
using os = go.os_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class rand_package
    {
        // Implemented by using Windows CryptoAPI 2.0.
        private static void init()
        {
            Reader = addr(new rngReader());
        }

        // A rngReader satisfies reads by reading from the Windows CryptGenRandom API.
        private partial struct rngReader
        {
            public int used; // atomic; whether this rngReader has been used
            public syscall.Handle prov;
            public sync.Mutex mu;
        }

        private static (long, error) Read(this ptr<rngReader> _addr_r, slice<byte> b) => func((defer, _, __) =>
        {
            long n = default;
            error err = default!;
            ref rngReader r = ref _addr_r.val;

            if (atomic.CompareAndSwapInt32(_addr_r.used, 0L, 1L))
            { 
                // First use of randomness. Start timer to warn about
                // being blocked on entropy not being available.
                var t = time.AfterFunc(60L * time.Second, warnBlocked);
                defer(t.Stop());

            }

            r.mu.Lock();
            if (r.prov == 0L)
            {
                const var provType = syscall.PROV_RSA_FULL;

                const var flags = syscall.CRYPT_VERIFYCONTEXT | syscall.CRYPT_SILENT;

                var err = syscall.CryptAcquireContext(_addr_r.prov, null, null, provType, flags);
                if (err != null)
                {
                    r.mu.Unlock();
                    return (0L, error.As(os.NewSyscallError("CryptAcquireContext", err))!);
                }

            }

            r.mu.Unlock();

            if (len(b) == 0L)
            {
                return (0L, error.As(null!)!);
            }

            err = syscall.CryptGenRandom(r.prov, uint32(len(b)), _addr_b[0L]);
            if (err != null)
            {
                return (0L, error.As(os.NewSyscallError("CryptGenRandom", err))!);
            }

            return (len(b), error.As(null!)!);

        });
    }
}}
