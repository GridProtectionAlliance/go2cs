// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements encoding/decoding of Ints.

// package big -- go2cs converted at 2020 August 29 08:29:18 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\intmarsh.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // Gob codec version. Permits backward-compatible changes to the encoding.
        private static readonly byte intGobVersion = 1L;

        // GobEncode implements the gob.GobEncoder interface.


        // GobEncode implements the gob.GobEncoder interface.
        private static (slice<byte>, error) GobEncode(this ref Int x)
        {
            if (x == null)
            {
                return (null, null);
            }
            var buf = make_slice<byte>(1L + len(x.abs) * _S); // extra byte for version and sign bit
            var i = x.abs.bytes(buf) - 1L; // i >= 0
            var b = intGobVersion << (int)(1L); // make space for sign bit
            if (x.neg)
            {
                b |= 1L;
            }
            buf[i] = b;
            return (buf[i..], null);
        }

        // GobDecode implements the gob.GobDecoder interface.
        private static error GobDecode(this ref Int z, slice<byte> buf)
        {
            if (len(buf) == 0L)
            { 
                // Other side sent a nil or default value.
                z.Value = new Int();
                return error.As(null);
            }
            var b = buf[0L];
            if (b >> (int)(1L) != intGobVersion)
            {
                return error.As(fmt.Errorf("Int.GobDecode: encoding version %d not supported", b >> (int)(1L)));
            }
            z.neg = b & 1L != 0L;
            z.abs = z.abs.setBytes(buf[1L..]);
            return error.As(null);
        }

        // MarshalText implements the encoding.TextMarshaler interface.
        private static (slice<byte>, error) MarshalText(this ref Int x)
        {
            if (x == null)
            {
                return ((slice<byte>)"<nil>", null);
            }
            return (x.abs.itoa(x.neg, 10L), null);
        }

        // UnmarshalText implements the encoding.TextUnmarshaler interface.
        private static error UnmarshalText(this ref Int z, slice<byte> text)
        {
            {
                var (_, ok) = z.setFromScanner(bytes.NewReader(text), 0L);

                if (!ok)
                {
                    return error.As(fmt.Errorf("math/big: cannot unmarshal %q into a *big.Int", text));
                }

            }
            return error.As(null);
        }

        // The JSON marshalers are only here for API backward compatibility
        // (programs that explicitly look for these two methods). JSON works
        // fine with the TextMarshaler only.

        // MarshalJSON implements the json.Marshaler interface.
        private static (slice<byte>, error) MarshalJSON(this ref Int x)
        {
            return x.MarshalText();
        }

        // UnmarshalJSON implements the json.Unmarshaler interface.
        private static error UnmarshalJSON(this ref Int z, slice<byte> text)
        { 
            // Ignore null, like in the main JSON package.
            if (string(text) == "null")
            {
                return error.As(null);
            }
            return error.As(z.UnmarshalText(text));
        }
    }
}}
