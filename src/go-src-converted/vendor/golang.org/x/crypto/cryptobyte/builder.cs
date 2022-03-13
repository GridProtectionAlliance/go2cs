// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cryptobyte -- go2cs converted at 2022 March 13 06:44:41 UTC
// import "vendor/golang.org/x/crypto/cryptobyte" ==> using cryptobyte = go.vendor.golang.org.x.crypto.cryptobyte_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\cryptobyte\builder.go
namespace go.vendor.golang.org.x.crypto;

using errors = errors_package;
using fmt = fmt_package;


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

using System;
public static partial class cryptobyte_package {

public partial struct Builder {
    public error err;
    public slice<byte> result;
    public bool fixedSize;
    public ptr<Builder> child;
    public nint offset;
    public nint pendingLenLen;
    public bool pendingIsASN1;
    public ptr<bool> inContinuation;
}

// NewBuilder creates a Builder that appends its output to the given buffer.
// Like append(), the slice will be reallocated if its capacity is exceeded.
// Use Bytes to get the final buffer.
public static ptr<Builder> NewBuilder(slice<byte> buffer) {
    return addr(new Builder(result:buffer,));
}

// NewFixedBuilder creates a Builder that appends its output into the given
// buffer. This builder does not reallocate the output buffer. Writes that
// would exceed the buffer's capacity are treated as an error.
public static ptr<Builder> NewFixedBuilder(slice<byte> buffer) {
    return addr(new Builder(result:buffer,fixedSize:true,));
}

// SetError sets the value to be returned as the error from Bytes. Writes
// performed after calling SetError are ignored.
private static void SetError(this ptr<Builder> _addr_b, error err) {
    ref Builder b = ref _addr_b.val;

    b.err = err;
}

// Bytes returns the bytes written by the builder or an error if one has
// occurred during building.
private static (slice<byte>, error) Bytes(this ptr<Builder> _addr_b) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref Builder b = ref _addr_b.val;

    if (b.err != null) {
        return (null, error.As(b.err)!);
    }
    return (b.result[(int)b.offset..], error.As(null!)!);
}

// BytesOrPanic returns the bytes written by the builder or panics if an error
// has occurred during building.
private static slice<byte> BytesOrPanic(this ptr<Builder> _addr_b) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;

    if (b.err != null) {
        panic(b.err);
    }
    return b.result[(int)b.offset..];
});

// AddUint8 appends an 8-bit value to the byte string.
private static void AddUint8(this ptr<Builder> _addr_b, byte v) {
    ref Builder b = ref _addr_b.val;

    b.add(byte(v));
}

// AddUint16 appends a big-endian, 16-bit value to the byte string.
private static void AddUint16(this ptr<Builder> _addr_b, ushort v) {
    ref Builder b = ref _addr_b.val;

    b.add(byte(v >> 8), byte(v));
}

// AddUint24 appends a big-endian, 24-bit value to the byte string. The highest
// byte of the 32-bit input value is silently truncated.
private static void AddUint24(this ptr<Builder> _addr_b, uint v) {
    ref Builder b = ref _addr_b.val;

    b.add(byte(v >> 16), byte(v >> 8), byte(v));
}

// AddUint32 appends a big-endian, 32-bit value to the byte string.
private static void AddUint32(this ptr<Builder> _addr_b, uint v) {
    ref Builder b = ref _addr_b.val;

    b.add(byte(v >> 24), byte(v >> 16), byte(v >> 8), byte(v));
}

// AddBytes appends a sequence of bytes to the byte string.
private static void AddBytes(this ptr<Builder> _addr_b, slice<byte> v) {
    ref Builder b = ref _addr_b.val;

    b.add(v);
}

// BuilderContinuation is a continuation-passing interface for building
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
public delegate void BuilderContinuation(ptr<Builder>);

// BuildError wraps an error. If a BuilderContinuation panics with this value,
// the panic will be recovered and the inner error will be returned from
// Builder.Bytes.
public partial struct BuildError {
    public error Err;
}

// AddUint8LengthPrefixed adds a 8-bit length-prefixed byte sequence.
private static void AddUint8LengthPrefixed(this ptr<Builder> _addr_b, BuilderContinuation f) {
    ref Builder b = ref _addr_b.val;

    b.addLengthPrefixed(1, false, f);
}

// AddUint16LengthPrefixed adds a big-endian, 16-bit length-prefixed byte sequence.
private static void AddUint16LengthPrefixed(this ptr<Builder> _addr_b, BuilderContinuation f) {
    ref Builder b = ref _addr_b.val;

    b.addLengthPrefixed(2, false, f);
}

// AddUint24LengthPrefixed adds a big-endian, 24-bit length-prefixed byte sequence.
private static void AddUint24LengthPrefixed(this ptr<Builder> _addr_b, BuilderContinuation f) {
    ref Builder b = ref _addr_b.val;

    b.addLengthPrefixed(3, false, f);
}

// AddUint32LengthPrefixed adds a big-endian, 32-bit length-prefixed byte sequence.
private static void AddUint32LengthPrefixed(this ptr<Builder> _addr_b, BuilderContinuation f) {
    ref Builder b = ref _addr_b.val;

    b.addLengthPrefixed(4, false, f);
}

private static void callContinuation(this ptr<Builder> _addr_b, BuilderContinuation f, ptr<Builder> _addr_arg) => func((defer, panic, _) => {
    ref Builder b = ref _addr_b.val;
    ref Builder arg = ref _addr_arg.val;

    if (!b.inContinuation.val) {
        b.inContinuation.val = true;

        defer(() => {
            b.inContinuation.val = false;

            var r = recover();
            if (r == null) {
                return ;
            }
            {
                BuildError (buildError, ok) = r._<BuildError>();

                if (ok) {
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

private static void addLengthPrefixed(this ptr<Builder> _addr_b, nint lenLen, bool isASN1, BuilderContinuation f) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;
 
    // Subsequent writes can be ignored if the builder has encountered an error.
    if (b.err != null) {
        return ;
    }
    var offset = len(b.result);
    b.add(make_slice<byte>(lenLen));

    if (b.inContinuation == null) {
        b.inContinuation = @new<bool>();
    }
    b.child = addr(new Builder(result:b.result,fixedSize:b.fixedSize,offset:offset,pendingLenLen:lenLen,pendingIsASN1:isASN1,inContinuation:b.inContinuation,));

    b.callContinuation(f, b.child);
    b.flushChild();
    if (b.child != null) {
        panic("cryptobyte: internal error");
    }
});

private static void flushChild(this ptr<Builder> _addr_b) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;

    if (b.child == null) {
        return ;
    }
    b.child.flushChild();
    var child = b.child;
    b.child = null;

    if (child.err != null) {
        b.err = child.err;
        return ;
    }
    var length = len(child.result) - child.pendingLenLen - child.offset;

    if (length < 0) {
        panic("cryptobyte: internal error"); // result unexpectedly shrunk
    }
    if (child.pendingIsASN1) { 
        // For ASN.1, we reserved a single byte for the length. If that turned out
        // to be incorrect, we have to move the contents along in order to make
        // space.
        if (child.pendingLenLen != 1) {
            panic("cryptobyte: internal error");
        }
        byte lenLen = default;        byte lenByte = default;

        if (int64(length) > 0xfffffffe) {
            b.err = errors.New("pending ASN.1 child too long");
            return ;
        }
        else if (length > 0xffffff) {
            lenLen = 5;
            lenByte = 0x80 | 4;
        }
        else if (length > 0xffff) {
            lenLen = 4;
            lenByte = 0x80 | 3;
        }
        else if (length > 0xff) {
            lenLen = 3;
            lenByte = 0x80 | 2;
        }
        else if (length > 0x7f) {
            lenLen = 2;
            lenByte = 0x80 | 1;
        }
        else
 {
            lenLen = 1;
            lenByte = uint8(length);
            length = 0;
        }
        child.result[child.offset] = lenByte;
        var extraBytes = int(lenLen - 1);
        if (extraBytes != 0) {
            child.add(make_slice<byte>(extraBytes));
            var childStart = child.offset + child.pendingLenLen;
            copy(child.result[(int)childStart + extraBytes..], child.result[(int)childStart..]);
        }
        child.offset++;
        child.pendingLenLen = extraBytes;
    }
    var l = length;
    for (var i = child.pendingLenLen - 1; i >= 0; i--) {
        child.result[child.offset + i] = uint8(l);
        l>>=8;
    }
    if (l != 0) {
        b.err = fmt.Errorf("cryptobyte: pending child length %d exceeds %d-byte length prefix", length, child.pendingLenLen);
        return ;
    }
    if (b.fixedSize && _addr_b.result[0] != _addr_child.result[0]) {
        panic("cryptobyte: BuilderContinuation reallocated a fixed-size buffer");
    }
    b.result = child.result;
});

private static void add(this ptr<Builder> _addr_b, params byte[] bytes) => func((_, panic, _) => {
    bytes = bytes.Clone();
    ref Builder b = ref _addr_b.val;

    if (b.err != null) {
        return ;
    }
    if (b.child != null) {
        panic("cryptobyte: attempted write while child is pending");
    }
    if (len(b.result) + len(bytes) < len(bytes)) {
        b.err = errors.New("cryptobyte: length overflow");
    }
    if (b.fixedSize && len(b.result) + len(bytes) > cap(b.result)) {
        b.err = errors.New("cryptobyte: Builder is exceeding its fixed-size buffer");
        return ;
    }
    b.result = append(b.result, bytes);
});

// Unwrite rolls back n bytes written directly to the Builder. An attempt by a
// child builder passed to a continuation to unwrite bytes from its parent will
// panic.
private static void Unwrite(this ptr<Builder> _addr_b, nint n) => func((_, panic, _) => {
    ref Builder b = ref _addr_b.val;

    if (b.err != null) {
        return ;
    }
    if (b.child != null) {
        panic("cryptobyte: attempted unwrite while child is pending");
    }
    var length = len(b.result) - b.pendingLenLen - b.offset;
    if (length < 0) {
        panic("cryptobyte: internal error");
    }
    if (n > length) {
        panic("cryptobyte: attempted to unwrite more than was written");
    }
    b.result = b.result[..(int)len(b.result) - n];
});

// A MarshalingValue marshals itself into a Builder.
public partial interface MarshalingValue {
    error Marshal(ptr<Builder> b);
}

// AddValue calls Marshal on v, passing a pointer to the builder to append to.
// If Marshal returns an error, it is set on the Builder so that subsequent
// appends don't have an effect.
private static void AddValue(this ptr<Builder> _addr_b, MarshalingValue v) {
    ref Builder b = ref _addr_b.val;

    var err = v.Marshal(b);
    if (err != null) {
        b.err = err;
    }
}

} // end cryptobyte_package
