// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo,!purego

// package poly1305 -- go2cs converted at 2020 October 08 05:00:17 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\poly1305\sum_s390x.go
using cpu = go.golang.org.x.sys.cpu_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        // updateVX is an assembly implementation of Poly1305 that uses vector
        // instructions. It must only be called if the vector facility (vx) is
        // available.
        //go:noescape
        private static void updateVX(ptr<macState> state, slice<byte> msg)
;

        // mac is a replacement for macGeneric that uses a larger buffer and redirects
        // calls that would have gone to updateGeneric to updateVX if the vector
        // facility is installed.
        //
        // A larger buffer is required for good performance because the vector
        // implementation has a higher fixed cost per call than the generic
        // implementation.
        private partial struct mac
        {
            public ref macState macState => ref macState_val;
            public array<byte> buffer; // size must be a multiple of block size (16)
            public long offset;
        }

        private static (long, error) Write(this ptr<mac> _addr_h, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref mac h = ref _addr_h.val;

            var nn = len(p);
            if (h.offset > 0L)
            {>>MARKER:FUNCTION_updateVX_BLOCK_PREFIX<<
                var n = copy(h.buffer[h.offset..], p);
                if (h.offset + n < len(h.buffer))
                {
                    h.offset += n;
                    return (nn, error.As(null!)!);
                }

                p = p[n..];
                h.offset = 0L;
                if (cpu.S390X.HasVX)
                {
                    updateVX(_addr_h.macState, h.buffer[..]);
                }
                else
                {
                    updateGeneric(_addr_h.macState, h.buffer[..]);
                }

            }

            var tail = len(p) % len(h.buffer); // number of bytes to copy into buffer
            var body = len(p) - tail; // number of bytes to process now
            if (body > 0L)
            {
                if (cpu.S390X.HasVX)
                {
                    updateVX(_addr_h.macState, p[..body]);
                }
                else
                {
                    updateGeneric(_addr_h.macState, p[..body]);
                }

            }

            h.offset = copy(h.buffer[..], p[body..]); // copy tail bytes - can be 0
            return (nn, error.As(null!)!);

        }

        private static void Sum(this ptr<mac> _addr_h, ptr<array<byte>> _addr_@out)
        {
            ref mac h = ref _addr_h.val;
            ref array<byte> @out = ref _addr_@out.val;

            ref var state = ref heap(h.macState, out ptr<var> _addr_state);
            var remainder = h.buffer[..h.offset]; 

            // Use the generic implementation if we have 2 or fewer blocks left
            // to sum. The vector implementation has a higher startup time.
            if (cpu.S390X.HasVX && len(remainder) > 2L * TagSize)
            {
                updateVX(_addr_state, remainder);
            }
            else if (len(remainder) > 0L)
            {
                updateGeneric(_addr_state, remainder);
            }

            finalize(out, _addr_state.h, _addr_state.s);

        }
    }
}}}}}
