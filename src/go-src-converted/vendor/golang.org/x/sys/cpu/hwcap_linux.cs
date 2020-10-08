// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 08 05:01:50 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\hwcap_linux.go
using ioutil = go.io.ioutil_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        private static readonly long _AT_HWCAP = (long)16L;
        private static readonly long _AT_HWCAP2 = (long)26L;

        private static readonly @string procAuxv = (@string)"/proc/self/auxv";

        private static readonly var uintSize = (var)int(32L << (int)((~uint(0L) >> (int)(63L))));


        // For those platforms don't have a 'cpuid' equivalent we use HWCAP/HWCAP2
        // These are initialized in cpu_$GOARCH.go
        // and should not be changed after they are initialized.
        private static ulong hwCap = default;
        private static ulong hwCap2 = default;

        private static error readHWCAP()
        {
            var (buf, err) = ioutil.ReadFile(procAuxv);
            if (err != null)
            { 
                // e.g. on android /proc/self/auxv is not accessible, so silently
                // ignore the error and leave Initialized = false. On some
                // architectures (e.g. arm64) doinit() implements a fallback
                // readout and will set Initialized = true again.
                return error.As(err)!;

            }

            var bo = hostByteOrder();
            while (len(buf) >= 2L * (uintSize / 8L))
            {
                ulong tag = default;                ulong val = default;

                switch (uintSize)
                {
                    case 32L: 
                        tag = uint(bo.Uint32(buf[0L..]));
                        val = uint(bo.Uint32(buf[4L..]));
                        buf = buf[8L..];
                        break;
                    case 64L: 
                        tag = uint(bo.Uint64(buf[0L..]));
                        val = uint(bo.Uint64(buf[8L..]));
                        buf = buf[16L..];
                        break;
                }

                if (tag == _AT_HWCAP) 
                    hwCap = val;
                else if (tag == _AT_HWCAP2) 
                    hwCap2 = val;
                
            }

            return error.As(null!)!;

        }
    }
}}}}}
