// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:04 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\mac.go

using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static readonly @string hexDigit = (@string)"0123456789abcdef";

        // A HardwareAddr represents a physical hardware address.


        // A HardwareAddr represents a physical hardware address.
        public partial struct HardwareAddr // : slice<byte>
        {
        }

        public static @string String(this HardwareAddr a)
        {
            if (len(a) == 0L)
            {
                return "";
            }

            var buf = make_slice<byte>(0L, len(a) * 3L - 1L);
            foreach (var (i, b) in a)
            {
                if (i > 0L)
                {
                    buf = append(buf, ':');
                }

                buf = append(buf, hexDigit[b >> (int)(4L)]);
                buf = append(buf, hexDigit[b & 0xFUL]);

            }
            return string(buf);

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
        public static (HardwareAddr, error) ParseMAC(@string s)
        {
            HardwareAddr hw = default;
            error err = default!;

            if (len(s) < 14L)
            {
                goto error;
            }

            if (s[2L] == ':' || s[2L] == '-')
            {
                if ((len(s) + 1L) % 3L != 0L)
                {
                    goto error;
                }

                var n = (len(s) + 1L) / 3L;
                if (n != 6L && n != 8L && n != 20L)
                {
                    goto error;
                }

                hw = make(HardwareAddr, n);
                {
                    long x__prev1 = x;
                    long i__prev1 = i;

                    for (long x = 0L;
                    long i = 0L; i < n; i++)
                    {
                        bool ok = default;
                        hw[i], ok = xtoi2(s[x..], s[2L]);

                        if (!ok)
                        {
                            goto error;
                        }

                        x += 3L;

                    }


                    x = x__prev1;
                    i = i__prev1;
                }

            }
            else if (s[4L] == '.')
            {
                if ((len(s) + 1L) % 5L != 0L)
                {
                    goto error;
                }

                n = 2L * (len(s) + 1L) / 5L;
                if (n != 6L && n != 8L && n != 20L)
                {
                    goto error;
                }

                hw = make(HardwareAddr, n);
                {
                    long x__prev1 = x;
                    long i__prev1 = i;

                    x = 0L;
                    i = 0L;

                    while (i < n)
                    {
                        ok = default;
                        hw[i], ok = xtoi2(s[x..x + 2L], 0L);

                        if (!ok)
                        {
                            goto error;
                        i += 2L;
                        }

                        hw[i + 1L], ok = xtoi2(s[x + 2L..], s[4L]);

                        if (!ok)
                        {
                            goto error;
                        }

                        x += 5L;

                    }
            else


                    x = x__prev1;
                    i = i__prev1;
                }

            }            {
                goto error;
            }

            return (hw, error.As(null!)!);

error:
            return (null, error.As(addr(new AddrError(Err:"invalid MAC address",Addr:s))!)!);

        }
    }
}
