// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:21 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\hwcap_linux.go
using ioutil = go.io.ioutil_package;

namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static readonly nint _AT_HWCAP = 16;
private static readonly nint _AT_HWCAP2 = 26;

private static readonly @string procAuxv = "/proc/self/auxv";

private static readonly var uintSize = int(32 << (int)((~uint(0) >> 63)));


// For those platforms don't have a 'cpuid' equivalent we use HWCAP/HWCAP2
// These are initialized in cpu_$GOARCH.go
// and should not be changed after they are initialized.
private static nuint hwCap = default;
private static nuint hwCap2 = default;

private static error readHWCAP() {
    var (buf, err) = ioutil.ReadFile(procAuxv);
    if (err != null) { 
        // e.g. on android /proc/self/auxv is not accessible, so silently
        // ignore the error and leave Initialized = false. On some
        // architectures (e.g. arm64) doinit() implements a fallback
        // readout and will set Initialized = true again.
        return error.As(err)!;

    }
    var bo = hostByteOrder();
    while (len(buf) >= 2 * (uintSize / 8)) {
        nuint tag = default;        nuint val = default;

        switch (uintSize) {
            case 32: 
                tag = uint(bo.Uint32(buf[(int)0..]));
                val = uint(bo.Uint32(buf[(int)4..]));
                buf = buf[(int)8..];
                break;
            case 64: 
                tag = uint(bo.Uint64(buf[(int)0..]));
                val = uint(bo.Uint64(buf[(int)8..]));
                buf = buf[(int)16..];
                break;
        }

        if (tag == _AT_HWCAP) 
            hwCap = val;
        else if (tag == _AT_HWCAP2) 
            hwCap2 = val;
        
    }
    return error.As(null!)!;

}

} // end cpu_package
