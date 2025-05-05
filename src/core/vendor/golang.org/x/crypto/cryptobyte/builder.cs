// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

using errors = errors_package;
using fmt = fmt_package;
using ꓸꓸꓸbyte = Span<byte>;

partial class cryptobyte_package {

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
[GoType] partial struct Builder {
    internal error err;
    internal slice<byte> result;
    internal bool fixedSize;
    internal ж<Builder> child;
    internal nint offset;
    internal nint pendingLenLen;
    internal bool pendingIsASN1;
    internal ж<bool> inContinuation;
}

// NewBuilder creates a Builder that appends its output to the given buffer.
// Like append(), the slice will be reallocated if its capacity is exceeded.
// Use Bytes to get the final buffer.
public static ж<Builder> NewBuilder(slice<byte> buffer) {
    return Ꮡ(new Builder(
        result: buffer
    ));
}

// NewFixedBuilder creates a Builder that appends its output into the given
// buffer. This builder does not reallocate the output buffer. Writes that
// would exceed the buffer's capacity are treated as an error.
public static ж<Builder> NewFixedBuilder(slice<byte> buffer) {
    return Ꮡ(new Builder(
        result: buffer,
        fixedSize: true
    ));
}

// SetError sets the value to be returned as the error from Bytes. Writes
// performed after calling SetError are ignored.
[GoRecv] public static void SetError(this ref Builder b, error err) {
    b.err = err;
}

// Bytes returns the bytes written by the builder or an error if one has
// occurred during building.
[GoRecv] public static (slice<byte>, error) Bytes(this ref Builder b) {
    if (b.err != default!) {
        return (default!, b.err);
    }
    return (b.result[(int)(b.offset)..], default!);
}

// BytesOrPanic returns the bytes written by the builder or panics if an error
// has occurred during building.
[GoRecv] public static slice<byte> BytesOrPanic(this ref Builder b) {
    if (b.err != default!) {
        throw panic(b.err);
    }
    return b.result[(int)(b.offset)..];
}

// AddUint8 appends an 8-bit value to the byte string.
[GoRecv] public static void AddUint8(this ref Builder b, uint8 v) {
    b.add(((byte)v));
}

// AddUint16 appends a big-endian, 16-bit value to the byte string.
[GoRecv] public static void AddUint16(this ref Builder b, uint16 v) {
    b.add(((byte)(v >> (int)(8))), ((byte)v));
}

// AddUint24 appends a big-endian, 24-bit value to the byte string. The highest
// byte of the 32-bit input value is silently truncated.
[GoRecv] public static void AddUint24(this ref Builder b, uint32 v) {
    b.add(((byte)(v >> (int)(16))), ((byte)(v >> (int)(8))), ((byte)v));
}

// AddUint32 appends a big-endian, 32-bit value to the byte string.
[GoRecv] public static void AddUint32(this ref Builder b, uint32 v) {
    b.add(((byte)(v >> (int)(24))), ((byte)(v >> (int)(16))), ((byte)(v >> (int)(8))), ((byte)v));
}

// AddUint48 appends a big-endian, 48-bit value to the byte string.
[GoRecv] public static void AddUint48(this ref Builder b, uint64 v) {
    b.add(((byte)(v >> (int)(40))), ((byte)(v >> (int)(32))), ((byte)(v >> (int)(24))), ((byte)(v >> (int)(16))), ((byte)(v >> (int)(8))), ((byte)v));
}

// AddUint64 appends a big-endian, 64-bit value to the byte string.
[GoRecv] public static void AddUint64(this ref Builder b, uint64 v) {
    b.add(((byte)(v >> (int)(56))), ((byte)(v >> (int)(48))), ((byte)(v >> (int)(40))), ((byte)(v >> (int)(32))), ((byte)(v >> (int)(24))), ((byte)(v >> (int)(16))), ((byte)(v >> (int)(8))), ((byte)v));
}

// AddBytes appends a sequence of bytes to the byte string.
[GoRecv] public static void AddBytes(this ref Builder b, slice<byte> v) {
    b.add(v.ꓸꓸꓸ);
}

public delegate void BuilderContinuation(ж<Builder> child);

// BuildError wraps an error. If a BuilderContinuation panics with this value,
// the panic will be recovered and the inner error will be returned from
// Builder.Bytes.
[GoType] partial struct BuildError {
    public error Err;
}

// AddUint8LengthPrefixed adds a 8-bit length-prefixed byte sequence.
[GoRecv] public static void AddUint8LengthPrefixed(this ref Builder b, BuilderContinuation f) {
    b.addLengthPrefixed(1, false, f);
}

// AddUint16LengthPrefixed adds a big-endian, 16-bit length-prefixed byte sequence.
[GoRecv] public static void AddUint16LengthPrefixed(this ref Builder b, BuilderContinuation f) {
    b.addLengthPrefixed(2, false, f);
}

// AddUint24LengthPrefixed adds a big-endian, 24-bit length-prefixed byte sequence.
[GoRecv] public static void AddUint24LengthPrefixed(this ref Builder b, BuilderContinuation f) {
    b.addLengthPrefixed(3, false, f);
}

// AddUint32LengthPrefixed adds a big-endian, 32-bit length-prefixed byte sequence.
[GoRecv] public static void AddUint32LengthPrefixed(this ref Builder b, BuilderContinuation f) {
    b.addLengthPrefixed(4, false, f);
}

[GoRecv] public static void callContinuation(this ref Builder b, BuilderContinuation f, ж<Builder> Ꮡarg) => func((defer, recover) => {
    ref var arg = ref Ꮡarg.val;

    if (!b.inContinuation.val) {
        b.inContinuation.val = true;
        defer(() => {
            b.inContinuation.val = false;
            var r = recover();
            if (r == default!) {
                return;
            }
            {
                var (buildError, ok) = r._<BuildError>(ᐧ); if (ok){
                    b.err = buildError.Err;
                } else {
                    throw panic(r);
                }
            }
        });
    }
    f(Ꮡarg);
});

[GoRecv] internal static void addLengthPrefixed(this ref Builder b, nint lenLen, bool isASN1, BuilderContinuation f) {
    // Subsequent writes can be ignored if the builder has encountered an error.
    if (b.err != default!) {
        return;
    }
    ref var offset = ref heap<nint>(out var Ꮡoffset);
    offset = len(b.result);
    b.add(new slice<byte>(lenLen).ꓸꓸꓸ);
    if (b.inContinuation == nil) {
        b.inContinuation = @new<bool>();
    }
    b.child = Ꮡ(new Builder(
        result: b.result,
        fixedSize: b.fixedSize,
        offset: offset,
        pendingLenLen: lenLen,
        pendingIsASN1: isASN1,
        inContinuation: b.inContinuation
    ));
    b.callContinuation(f, b.child);
    b.flushChild();
    if (b.child != nil) {
        throw panic("cryptobyte: internal error");
    }
}

[GoRecv] internal static void flushChild(this ref Builder b) {
    if (b.child == nil) {
        return;
    }
    b.child.flushChild();
    var child = b.child;
    b.child = default!;
    if ((~child).err != default!) {
        b.err = child.val.err;
        return;
    }
    nint length = len((~child).result) - (~child).pendingLenLen - (~child).offset;
    if (length < 0) {
        throw panic("cryptobyte: internal error");
    }
    // result unexpectedly shrunk
    if ((~child).pendingIsASN1) {
        // For ASN.1, we reserved a single byte for the length. If that turned out
        // to be incorrect, we have to move the contents along in order to make
        // space.
        if ((~child).pendingLenLen != 1) {
            throw panic("cryptobyte: internal error");
        }
        uint8 lenLen = default!;
        uint8 lenByte = default!;
        if (((int64)length) > (nint)4294967294L){
            b.err = errors.New("pending ASN.1 child too long"u8);
            return;
        } else 
        if (length > 16777215){
            lenLen = 5;
            lenByte = (uint8)(128 | 4);
        } else 
        if (length > 65535){
            lenLen = 4;
            lenByte = (uint8)(128 | 3);
        } else 
        if (length > 255){
            lenLen = 3;
            lenByte = (uint8)(128 | 2);
        } else 
        if (length > 127){
            lenLen = 2;
            lenByte = (uint8)(128 | 1);
        } else {
            lenLen = 1;
            lenByte = ((uint8)length);
            length = 0;
        }
        // Insert the initial length byte, make space for successive length bytes,
        // and adjust the offset.
        (~child).result[(~child).offset] = lenByte;
        nint extraBytes = ((nint)(lenLen - 1));
        if (extraBytes != 0) {
            child.add(new slice<byte>(extraBytes).ꓸꓸꓸ);
            nint childStart = (~child).offset + (~child).pendingLenLen;
            copy((~child).result[(int)(childStart + extraBytes)..], (~child).result[(int)(childStart)..]);
        }
        (~child).offset++;
        child.val.pendingLenLen = extraBytes;
    }
    nint l = length;
    for (nint i = (~child).pendingLenLen - 1; i >= 0; i--) {
        (~child).result[(~child).offset + i] = ((uint8)l);
        l >>= (UntypedInt)(8);
    }
    if (l != 0) {
        b.err = fmt.Errorf("cryptobyte: pending child length %d exceeds %d-byte length prefix"u8, length, (~child).pendingLenLen);
        return;
    }
    if (b.fixedSize && Ꮡ(b.result[0]) != Ꮡ((~child).result, 0)) {
        throw panic("cryptobyte: BuilderContinuation reallocated a fixed-size buffer");
    }
    b.result = child.val.result;
}

[GoRecv] internal static void add(this ref Builder b, params ꓸꓸꓸbyte bytesʗp) {
    var bytes = bytesʗp.slice();

    if (b.err != default!) {
        return;
    }
    if (b.child != nil) {
        throw panic("cryptobyte: attempted write while child is pending");
    }
    if (len(b.result) + len(bytes) < len(bytes)) {
        b.err = errors.New("cryptobyte: length overflow"u8);
    }
    if (b.fixedSize && len(b.result) + len(bytes) > cap(b.result)) {
        b.err = errors.New("cryptobyte: Builder is exceeding its fixed-size buffer"u8);
        return;
    }
    b.result = append(b.result, bytes.ꓸꓸꓸ);
}

// Unwrite rolls back non-negative n bytes written directly to the Builder.
// An attempt by a child builder passed to a continuation to unwrite bytes
// from its parent will panic.
[GoRecv] public static void Unwrite(this ref Builder b, nint n) {
    if (b.err != default!) {
        return;
    }
    if (b.child != nil) {
        throw panic("cryptobyte: attempted unwrite while child is pending");
    }
    nint length = len(b.result) - b.pendingLenLen - b.offset;
    if (length < 0) {
        throw panic("cryptobyte: internal error");
    }
    if (n < 0) {
        throw panic("cryptobyte: attempted to unwrite negative number of bytes");
    }
    if (n > length) {
        throw panic("cryptobyte: attempted to unwrite more than was written");
    }
    b.result = b.result[..(int)(len(b.result) - n)];
}

// A MarshalingValue marshals itself into a Builder.
[GoType] partial interface MarshalingValue {
    // Marshal is called by Builder.AddValue. It receives a pointer to a builder
    // to marshal itself into. It may return an error that occurred during
    // marshaling, such as unset or invalid values.
    error Marshal(ж<Builder> b);
}

// AddValue calls Marshal on v, passing a pointer to the builder to append to.
// If Marshal returns an error, it is set on the Builder so that subsequent
// appends don't have an effect.
[GoRecv] public static void AddValue(this ref Builder b, MarshalingValue v) {
    var err = v.Marshal(b);
    if (err != default!) {
        b.err = err;
    }
}

} // end cryptobyte_package
