// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cryptobyte -- go2cs converted at 2020 August 29 10:11:20 UTC
// import "vendor/golang_org/x/crypto/cryptobyte" ==> using cryptobyte = go.vendor.golang_org.x.crypto.cryptobyte_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\cryptobyte\builder.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class cryptobyte_package
    {
        // A Builder builds byte strings from fixed-length and length-prefixed values.
        // Builders either allocate space as needed, or are ‘fixed’, which means that
        // they write into a given buffer and produce an error if it's exhausted.
        //
        // The zero value is a usable Builder that allocates space as needed.
        //
        // Simple values are marshaled and appended to a Builder using methods on the
        // Builder. Length-prefixed values are marshaled by providing a
        // BuilderContinuation, which is a function that writes the inner contents of
        // the value to a given Builder. See the documentation for BuilderContinuation
        // for details.
        public partial struct Builder
        {
            public error err;
            public slice<byte> result;
            public bool fixedSize;
            public ptr<Builder> child;
            public long offset;
            public long pendingLenLen;
            public bool pendingIsASN1;
            public ptr<bool> inContinuation;
        }

        // NewBuilder creates a Builder that appends its output to the given buffer.
        // Like append(), the slice will be reallocated if its capacity is exceeded.
        // Use Bytes to get the final buffer.
        public static ref Builder NewBuilder(slice<byte> buffer)
        {
            return ref new Builder(result:buffer,);
        }

        // NewFixedBuilder creates a Builder that appends its output into the given
        // buffer. This builder does not reallocate the output buffer. Writes that
        // would exceed the buffer's capacity are treated as an error.
        public static ref Builder NewFixedBuilder(slice<byte> buffer)
        {
            return ref new Builder(result:buffer,fixedSize:true,);
        }

        // Bytes returns the bytes written by the builder or an error if one has
        // occurred during during building.
        private static (slice<byte>, error) Bytes(this ref Builder b)
        {
            if (b.err != null)
            {
                return (null, b.err);
            }
            return (b.result[b.offset..], null);
        }

        // BytesOrPanic returns the bytes written by the builder or panics if an error
        // has occurred during building.
        private static slice<byte> BytesOrPanic(this ref Builder _b) => func(_b, (ref Builder b, Defer _, Panic panic, Recover __) =>
        {
            if (b.err != null)
            {
                panic(b.err);
            }
            return b.result[b.offset..];
        });

        // AddUint8 appends an 8-bit value to the byte string.
        private static void AddUint8(this ref Builder b, byte v)
        {
            b.add(byte(v));
        }

        // AddUint16 appends a big-endian, 16-bit value to the byte string.
        private static void AddUint16(this ref Builder b, ushort v)
        {
            b.add(byte(v >> (int)(8L)), byte(v));
        }

        // AddUint24 appends a big-endian, 24-bit value to the byte string. The highest
        // byte of the 32-bit input value is silently truncated.
        private static void AddUint24(this ref Builder b, uint v)
        {
            b.add(byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        // AddUint32 appends a big-endian, 32-bit value to the byte string.
        private static void AddUint32(this ref Builder b, uint v)
        {
            b.add(byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        // AddBytes appends a sequence of bytes to the byte string.
        private static void AddBytes(this ref Builder b, slice<byte> v)
        {
            b.add(v);
        }

        // BuilderContinuation is continuation-passing interface for building
        // length-prefixed byte sequences. Builder methods for length-prefixed
        // sequences (AddUint8LengthPrefixed etc) will invoke the BuilderContinuation
        // supplied to them. The child builder passed to the continuation can be used
        // to build the content of the length-prefixed sequence. For example:
        //
        //   parent := cryptobyte.NewBuilder()
        //   parent.AddUint8LengthPrefixed(func (child *Builder) {
        //     child.AddUint8(42)
        //     child.AddUint8LengthPrefixed(func (grandchild *Builder) {
        //       grandchild.AddUint8(5)
        //     })
        //   })
        //
        // It is an error to write more bytes to the child than allowed by the reserved
        // length prefix. After the continuation returns, the child must be considered
        // invalid, i.e. users must not store any copies or references of the child
        // that outlive the continuation.
        //
        // If the continuation panics with a value of type BuildError then the inner
        // error will be returned as the error from Bytes. If the child panics
        // otherwise then Bytes will repanic with the same value.
        public delegate void BuilderContinuation(ref Builder);

        // BuildError wraps an error. If a BuilderContinuation panics with this value,
        // the panic will be recovered and the inner error will be returned from
        // Builder.Bytes.
        public partial struct BuildError
        {
            public error Err;
        }

        // AddUint8LengthPrefixed adds a 8-bit length-prefixed byte sequence.
        private static void AddUint8LengthPrefixed(this ref Builder b, BuilderContinuation f)
        {
            b.addLengthPrefixed(1L, false, f);
        }

        // AddUint16LengthPrefixed adds a big-endian, 16-bit length-prefixed byte sequence.
        private static void AddUint16LengthPrefixed(this ref Builder b, BuilderContinuation f)
        {
            b.addLengthPrefixed(2L, false, f);
        }

        // AddUint24LengthPrefixed adds a big-endian, 24-bit length-prefixed byte sequence.
        private static void AddUint24LengthPrefixed(this ref Builder b, BuilderContinuation f)
        {
            b.addLengthPrefixed(3L, false, f);
        }

        // AddUint32LengthPrefixed adds a big-endian, 32-bit length-prefixed byte sequence.
        private static void AddUint32LengthPrefixed(this ref Builder b, BuilderContinuation f)
        {
            b.addLengthPrefixed(4L, false, f);
        }

        private static void callContinuation(this ref Builder _b, BuilderContinuation f, ref Builder _arg) => func(_b, _arg, (ref Builder b, ref Builder arg, Defer defer, Panic panic, Recover _) =>
        {
            if (!b.inContinuation.Value)
            {
                b.inContinuation.Value = true;

                defer(() =>
                {
                    b.inContinuation.Value = false;

                    var r = recover();
                    if (r == null)
                    {
                        return;
                    }
                    {
                        BuildError (buildError, ok) = r._<BuildError>();

                        if (ok)
                        {
                            b.err = buildError.Err;
                        }
                        else
                        {
                            panic(r);
                        }

                    }
                }());
            }
            f(arg);
        });

        private static void addLengthPrefixed(this ref Builder _b, long lenLen, bool isASN1, BuilderContinuation f) => func(_b, (ref Builder b, Defer _, Panic panic, Recover __) =>
        { 
            // Subsequent writes can be ignored if the builder has encountered an error.
            if (b.err != null)
            {
                return;
            }
            var offset = len(b.result);
            b.add(make_slice<byte>(lenLen));

            if (b.inContinuation == null)
            {
                b.inContinuation = @new<bool>();
            }
            b.child = ref new Builder(result:b.result,fixedSize:b.fixedSize,offset:offset,pendingLenLen:lenLen,pendingIsASN1:isASN1,inContinuation:b.inContinuation,);

            b.callContinuation(f, b.child);
            b.flushChild();
            if (b.child != null)
            {
                panic("cryptobyte: internal error");
            }
        });

        private static void flushChild(this ref Builder _b) => func(_b, (ref Builder b, Defer _, Panic panic, Recover __) =>
        {
            if (b.child == null)
            {
                return;
            }
            b.child.flushChild();
            var child = b.child;
            b.child = null;

            if (child.err != null)
            {
                b.err = child.err;
                return;
            }
            var length = len(child.result) - child.pendingLenLen - child.offset;

            if (length < 0L)
            {
                panic("cryptobyte: internal error"); // result unexpectedly shrunk
            }
            if (child.pendingIsASN1)
            { 
                // For ASN.1, we reserved a single byte for the length. If that turned out
                // to be incorrect, we have to move the contents along in order to make
                // space.
                if (child.pendingLenLen != 1L)
                {
                    panic("cryptobyte: internal error");
                }
                byte lenLen = default;                byte lenByte = default;

                if (int64(length) > 0xfffffffeUL)
                {
                    b.err = errors.New("pending ASN.1 child too long");
                    return;
                }
                else if (length > 0xffffffUL)
                {
                    lenLen = 5L;
                    lenByte = 0x80UL | 4L;
                }
                else if (length > 0xffffUL)
                {
                    lenLen = 4L;
                    lenByte = 0x80UL | 3L;
                }
                else if (length > 0xffUL)
                {
                    lenLen = 3L;
                    lenByte = 0x80UL | 2L;
                }
                else if (length > 0x7fUL)
                {
                    lenLen = 2L;
                    lenByte = 0x80UL | 1L;
                }
                else
                {
                    lenLen = 1L;
                    lenByte = uint8(length);
                    length = 0L;
                } 

                // Insert the initial length byte, make space for successive length bytes,
                // and adjust the offset.
                child.result[child.offset] = lenByte;
                var extraBytes = int(lenLen - 1L);
                if (extraBytes != 0L)
                {
                    child.add(make_slice<byte>(extraBytes));
                    var childStart = child.offset + child.pendingLenLen;
                    copy(child.result[childStart + extraBytes..], child.result[childStart..]);
                }
                child.offset++;
                child.pendingLenLen = extraBytes;
            }
            var l = length;
            for (var i = child.pendingLenLen - 1L; i >= 0L; i--)
            {
                child.result[child.offset + i] = uint8(l);
                l >>= 8L;
            }

            if (l != 0L)
            {
                b.err = fmt.Errorf("cryptobyte: pending child length %d exceeds %d-byte length prefix", length, child.pendingLenLen);
                return;
            }
            if (!b.fixedSize)
            {
                b.result = child.result; // In case child reallocated result.
            }
        });

        private static void add(this ref Builder _b, params byte[] bytes) => func(_b, (ref Builder b, Defer _, Panic panic, Recover __) =>
        {
            if (b.err != null)
            {
                return;
            }
            if (b.child != null)
            {
                panic("attempted write while child is pending");
            }
            if (len(b.result) + len(bytes) < len(bytes))
            {
                b.err = errors.New("cryptobyte: length overflow");
            }
            if (b.fixedSize && len(b.result) + len(bytes) > cap(b.result))
            {
                b.err = errors.New("cryptobyte: Builder is exceeding its fixed-size buffer");
                return;
            }
            b.result = append(b.result, bytes);
        });

        // A MarshalingValue marshals itself into a Builder.
        public partial interface MarshalingValue
        {
            error Marshal(ref Builder b);
        }

        // AddValue calls Marshal on v, passing a pointer to the builder to append to.
        // If Marshal returns an error, it is set on the Builder so that subsequent
        // appends don't have an effect.
        private static void AddValue(this ref Builder b, MarshalingValue v)
        {
            var err = v.Marshal(b);
            if (err != null)
            {
                b.err = err;
            }
        }
    }
}}}}}
