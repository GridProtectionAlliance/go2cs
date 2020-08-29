// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cryptobyte contains types that help with parsing and constructing
// length-prefixed, binary messages, including ASN.1 DER. (The asn1 subpackage
// contains useful ASN.1 constants.)
//
// The String type is for parsing. It wraps a []byte slice and provides helper
// functions for consuming structures, value by value.
//
// The Builder type is for constructing messages. It providers helper functions
// for appending values and also for appending length-prefixed submessages –
// without having to worry about calculating the length prefix ahead of time.
//
// See the documentation and examples for the Builder and String types to get
// started.
// package cryptobyte -- go2cs converted at 2020 August 29 10:11:21 UTC
// import "vendor/golang_org/x/crypto/cryptobyte" ==> using cryptobyte = go.vendor.golang_org.x.crypto.cryptobyte_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\cryptobyte\string.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class cryptobyte_package
    { // import "golang.org/x/crypto/cryptobyte"

        // String represents a string of bytes. It provides methods for parsing
        // fixed-length and length-prefixed values from it.
        public partial struct String // : slice<byte>
        {
        }

        // read advances a String by n bytes and returns them. If less than n bytes
        // remain, it returns nil.
        private static slice<byte> read(this ref String s, long n)
        {
            if (len(s.Value) < n)
            {
                return null;
            }
            var v = (s.Value)[..n];
            s.Value = (s.Value)[n..];
            return v;
        }

        // Skip advances the String by n byte and reports whether it was successful.
        private static bool Skip(this ref String s, long n)
        {
            return s.read(n) != null;
        }

        // ReadUint8 decodes an 8-bit value into out and advances over it. It
        // returns true on success and false on error.
        private static bool ReadUint8(this ref String s, ref byte @out)
        {
            var v = s.read(1L);
            if (v == null)
            {
                return false;
            }
            out.Value = uint8(v[0L]);
            return true;
        }

        // ReadUint16 decodes a big-endian, 16-bit value into out and advances over it.
        // It returns true on success and false on error.
        private static bool ReadUint16(this ref String s, ref ushort @out)
        {
            var v = s.read(2L);
            if (v == null)
            {
                return false;
            }
            out.Value = uint16(v[0L]) << (int)(8L) | uint16(v[1L]);
            return true;
        }

        // ReadUint24 decodes a big-endian, 24-bit value into out and advances over it.
        // It returns true on success and false on error.
        private static bool ReadUint24(this ref String s, ref uint @out)
        {
            var v = s.read(3L);
            if (v == null)
            {
                return false;
            }
            out.Value = uint32(v[0L]) << (int)(16L) | uint32(v[1L]) << (int)(8L) | uint32(v[2L]);
            return true;
        }

        // ReadUint32 decodes a big-endian, 32-bit value into out and advances over it.
        // It returns true on success and false on error.
        private static bool ReadUint32(this ref String s, ref uint @out)
        {
            var v = s.read(4L);
            if (v == null)
            {
                return false;
            }
            out.Value = uint32(v[0L]) << (int)(24L) | uint32(v[1L]) << (int)(16L) | uint32(v[2L]) << (int)(8L) | uint32(v[3L]);
            return true;
        }

        private static bool readUnsigned(this ref String s, ref uint @out, long length)
        {
            var v = s.read(length);
            if (v == null)
            {
                return false;
            }
            uint result = default;
            for (long i = 0L; i < length; i++)
            {
                result <<= 8L;
                result |= uint32(v[i]);
            }

            out.Value = result;
            return true;
        }

        private static bool readLengthPrefixed(this ref String s, long lenLen, ref String outChild)
        {
            var lenBytes = s.read(lenLen);
            if (lenBytes == null)
            {
                return false;
            }
            uint length = default;
            foreach (var (_, b) in lenBytes)
            {
                length = length << (int)(8L);
                length = length | uint32(b);
            }
            if (int(length) < 0L)
            { 
                // This currently cannot overflow because we read uint24 at most, but check
                // anyway in case that changes in the future.
                return false;
            }
            var v = s.read(int(length));
            if (v == null)
            {
                return false;
            }
            outChild.Value = v;
            return true;
        }

        // ReadUint8LengthPrefixed reads the content of an 8-bit length-prefixed value
        // into out and advances over it. It returns true on success and false on
        // error.
        private static bool ReadUint8LengthPrefixed(this ref String s, ref String @out)
        {
            return s.readLengthPrefixed(1L, out);
        }

        // ReadUint16LengthPrefixed reads the content of a big-endian, 16-bit
        // length-prefixed value into out and advances over it. It returns true on
        // success and false on error.
        private static bool ReadUint16LengthPrefixed(this ref String s, ref String @out)
        {
            return s.readLengthPrefixed(2L, out);
        }

        // ReadUint24LengthPrefixed reads the content of a big-endian, 24-bit
        // length-prefixed value into out and advances over it. It returns true on
        // success and false on error.
        private static bool ReadUint24LengthPrefixed(this ref String s, ref String @out)
        {
            return s.readLengthPrefixed(3L, out);
        }

        // ReadBytes reads n bytes into out and advances over them. It returns true on
        // success and false and error.
        private static bool ReadBytes(this ref String s, ref slice<byte> @out, long n)
        {
            var v = s.read(n);
            if (v == null)
            {
                return false;
            }
            out.Value = v;
            return true;
        }

        // CopyBytes copies len(out) bytes into out and advances over them. It returns
        // true on success and false on error.
        private static bool CopyBytes(this ref String s, slice<byte> @out)
        {
            var n = len(out);
            var v = s.read(n);
            if (v == null)
            {
                return false;
            }
            return copy(out, v) == n;
        }

        // Empty reports whether the string does not contain any bytes.
        public static bool Empty(this String s)
        {
            return len(s) == 0L;
        }
    }
}}}}}
