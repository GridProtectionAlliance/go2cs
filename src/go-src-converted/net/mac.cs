// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 13 05:29:57 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\mac.go
namespace go;

public static partial class net_package {

private static readonly @string hexDigit = "0123456789abcdef";

// A HardwareAddr represents a physical hardware address.


// A HardwareAddr represents a physical hardware address.
public partial struct HardwareAddr { // : slice<byte>
}

public static @string String(this HardwareAddr a) {
    if (len(a) == 0) {
        return "";
    }
    var buf = make_slice<byte>(0, len(a) * 3 - 1);
    foreach (var (i, b) in a) {
        if (i > 0) {
            buf = append(buf, ':');
        }
        buf = append(buf, hexDigit[b >> 4]);
        buf = append(buf, hexDigit[b & 0xF]);
    }    return string(buf);
}

// ParseMAC parses s as an IEEE 802 MAC-48, EUI-48, EUI-64, or a 20-octet
// IP over InfiniBand link-layer address using one of the following formats:
//    00:00:5e:00:53:01
//    02:00:5e:10:00:00:00:01
//    00:00:00:00:fe:80:00:00:00:00:00:00:02:00:5e:10:00:00:00:01
//    00-00-5e-00-53-01
//    02-00-5e-10-00-00-00-01
//    00-00-00-00-fe-80-00-00-00-00-00-00-02-00-5e-10-00-00-00-01
//    0000.5e00.5301
//    0200.5e10.0000.0001
//    0000.0000.fe80.0000.0000.0000.0200.5e10.0000.0001
public static (HardwareAddr, error) ParseMAC(@string s) {
    HardwareAddr hw = default;
    error err = default!;

    if (len(s) < 14) {
        goto error;
    }
    if (s[2] == ':' || s[2] == '-') {
        if ((len(s) + 1) % 3 != 0) {
            goto error;
        }
        var n = (len(s) + 1) / 3;
        if (n != 6 && n != 8 && n != 20) {
            goto error;
        }
        hw = make(HardwareAddr, n);
        {
            nint x__prev1 = x;
            nint i__prev1 = i;

            for (nint x = 0;
            nint i = 0; i < n; i++) {
                bool ok = default;
                hw[i], ok = xtoi2(s[(int)x..], s[2]);

                if (!ok) {
                    goto error;
                }
                x += 3;
            }


            x = x__prev1;
            i = i__prev1;
        }
    }
    else if (s[4] == '.') {
        if ((len(s) + 1) % 5 != 0) {
            goto error;
        }
        n = 2 * (len(s) + 1) / 5;
        if (n != 6 && n != 8 && n != 20) {
            goto error;
        }
        hw = make(HardwareAddr, n);
        {
            nint x__prev1 = x;
            nint i__prev1 = i;

            x = 0;
            i = 0;

            while (i < n) {
                ok = default;
                hw[i], ok = xtoi2(s[(int)x..(int)x + 2], 0);

                if (!ok) {
                    goto error;
                i += 2;
                }
                hw[i + 1], ok = xtoi2(s[(int)x + 2..], s[4]);

                if (!ok) {
                    goto error;
                }
                x += 5;
            }
    else


            x = x__prev1;
            i = i__prev1;
        }
    } {
        goto error;
    }
    return (hw, error.As(null!)!);

error:
    return (null, error.As(addr(new AddrError(Err:"invalid MAC address",Addr:s))!)!);
}

} // end net_package
