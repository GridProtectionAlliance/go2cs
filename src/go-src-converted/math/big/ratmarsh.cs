// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements encoding/decoding of Rats.

// package big -- go2cs converted at 2020 August 29 08:29:32 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\ratmarsh.go
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // Gob codec version. Permits backward-compatible changes to the encoding.
        private static readonly byte ratGobVersion = 1L;

        // GobEncode implements the gob.GobEncoder interface.


        // GobEncode implements the gob.GobEncoder interface.
        private static (slice<byte>, error) GobEncode(this ref Rat x)
        {
            if (x == null)
            {
                return (null, null);
            }
            var buf = make_slice<byte>(1L + 4L + (len(x.a.abs) + len(x.b.abs)) * _S); // extra bytes for version and sign bit (1), and numerator length (4)
            var i = x.b.abs.bytes(buf);
            var j = x.a.abs.bytes(buf[..i]);
            var n = i - j;
            if (int(uint32(n)) != n)
            { 
                // this should never happen
                return (null, errors.New("Rat.GobEncode: numerator too large"));
            }
            binary.BigEndian.PutUint32(buf[j - 4L..j], uint32(n));
            j -= 1L + 4L;
            var b = ratGobVersion << (int)(1L); // make space for sign bit
            if (x.a.neg)
            {
                b |= 1L;
            }
            buf[j] = b;
            return (buf[j..], null);
        }

        // GobDecode implements the gob.GobDecoder interface.
        private static error GobDecode(this ref Rat z, slice<byte> buf)
        {
            if (len(buf) == 0L)
            { 
                // Other side sent a nil or default value.
                z.Value = new Rat();
                return error.As(null);
            }
            var b = buf[0L];
            if (b >> (int)(1L) != ratGobVersion)
            {
                return error.As(fmt.Errorf("Rat.GobDecode: encoding version %d not supported", b >> (int)(1L)));
            }
            const long j = 1L + 4L;

            var i = j + binary.BigEndian.Uint32(buf[j - 4L..j]);
            z.a.neg = b & 1L != 0L;
            z.a.abs = z.a.abs.setBytes(buf[j..i]);
            z.b.abs = z.b.abs.setBytes(buf[i..]);
            return error.As(null);
        }

        // MarshalText implements the encoding.TextMarshaler interface.
        private static (slice<byte>, error) MarshalText(this ref Rat x)
        {
            if (x.IsInt())
            {
                return x.a.MarshalText();
            }
            return (x.marshal(), null);
        }

        // UnmarshalText implements the encoding.TextUnmarshaler interface.
        private static error UnmarshalText(this ref Rat z, slice<byte> text)
        { 
            // TODO(gri): get rid of the []byte/string conversion
            {
                var (_, ok) = z.SetString(string(text));

                if (!ok)
                {
                    return error.As(fmt.Errorf("math/big: cannot unmarshal %q into a *big.Rat", text));
                }

            }
            return error.As(null);
        }
    }
}}
