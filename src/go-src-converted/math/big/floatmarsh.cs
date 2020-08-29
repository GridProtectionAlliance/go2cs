// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements encoding/decoding of Floats.

// package big -- go2cs converted at 2020 August 29 08:29:09 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\floatmarsh.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // Gob codec version. Permits backward-compatible changes to the encoding.
        private static readonly byte floatGobVersion = 1L;

        // GobEncode implements the gob.GobEncoder interface.
        // The Float value and all its attributes (precision,
        // rounding mode, accuracy) are marshaled.


        // GobEncode implements the gob.GobEncoder interface.
        // The Float value and all its attributes (precision,
        // rounding mode, accuracy) are marshaled.
        private static (slice<byte>, error) GobEncode(this ref Float x)
        {
            if (x == null)
            {
                return (null, null);
            } 

            // determine max. space (bytes) required for encoding
            long sz = 1L + 1L + 4L; // version + mode|acc|form|neg (3+2+2+1bit) + prec
            long n = 0L; // number of mantissa words
            if (x.form == finite)
            { 
                // add space for mantissa and exponent
                n = int((x.prec + (_W - 1L)) / _W); // required mantissa length in words for given precision
                // actual mantissa slice could be shorter (trailing 0's) or longer (unused bits):
                // - if shorter, only encode the words present
                // - if longer, cut off unused words when encoding in bytes
                //   (in practice, this should never happen since rounding
                //   takes care of it, but be safe and do it always)
                if (len(x.mant) < n)
                {
                    n = len(x.mant);
                } 
                // len(x.mant) >= n
                sz += 4L + n * _S; // exp + mant
            }
            var buf = make_slice<byte>(sz);

            buf[0L] = floatGobVersion;
            var b = byte(x.mode & 7L) << (int)(5L) | byte((x.acc + 1L) & 3L) << (int)(3L) | byte(x.form & 3L) << (int)(1L);
            if (x.neg)
            {
                b |= 1L;
            }
            buf[1L] = b;
            binary.BigEndian.PutUint32(buf[2L..], x.prec);

            if (x.form == finite)
            {
                binary.BigEndian.PutUint32(buf[6L..], uint32(x.exp));
                x.mant[len(x.mant) - n..].bytes(buf[10L..]); // cut off unused trailing words
            }
            return (buf, null);
        }

        // GobDecode implements the gob.GobDecoder interface.
        // The result is rounded per the precision and rounding mode of
        // z unless z's precision is 0, in which case z is set exactly
        // to the decoded value.
        private static error GobDecode(this ref Float z, slice<byte> buf)
        {
            if (len(buf) == 0L)
            { 
                // Other side sent a nil or default value.
                z.Value = new Float();
                return error.As(null);
            }
            if (buf[0L] != floatGobVersion)
            {
                return error.As(fmt.Errorf("Float.GobDecode: encoding version %d not supported", buf[0L]));
            }
            var oldPrec = z.prec;
            var oldMode = z.mode;

            var b = buf[1L];
            z.mode = RoundingMode((b >> (int)(5L)) & 7L);
            z.acc = Accuracy((b >> (int)(3L)) & 3L) - 1L;
            z.form = form((b >> (int)(1L)) & 3L);
            z.neg = b & 1L != 0L;
            z.prec = binary.BigEndian.Uint32(buf[2L..]);

            if (z.form == finite)
            {
                z.exp = int32(binary.BigEndian.Uint32(buf[6L..]));
                z.mant = z.mant.setBytes(buf[10L..]);
            }
            if (oldPrec != 0L)
            {
                z.mode = oldMode;
                z.SetPrec(uint(oldPrec));
            }
            return error.As(null);
        }

        // MarshalText implements the encoding.TextMarshaler interface.
        // Only the Float value is marshaled (in full precision), other
        // attributes such as precision or accuracy are ignored.
        private static (slice<byte>, error) MarshalText(this ref Float x)
        {
            if (x == null)
            {
                return ((slice<byte>)"<nil>", null);
            }
            slice<byte> buf = default;
            return (x.Append(buf, 'g', -1L), null);
        }

        // UnmarshalText implements the encoding.TextUnmarshaler interface.
        // The result is rounded per the precision and rounding mode of z.
        // If z's precision is 0, it is changed to 64 before rounding takes
        // effect.
        private static error UnmarshalText(this ref Float z, slice<byte> text)
        { 
            // TODO(gri): get rid of the []byte/string conversion
            var (_, _, err) = z.Parse(string(text), 0L);
            if (err != null)
            {
                err = fmt.Errorf("math/big: cannot unmarshal %q into a *big.Float (%v)", text, err);
            }
            return error.As(err);
        }
    }
}}
