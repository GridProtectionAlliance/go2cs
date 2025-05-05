// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bufio = bufio_package;
using errors = errors_package;
using saferio = @internal.saferio_package;
using io = io_package;
using reflect = reflect_package;
using sync = sync_package;
using @internal;

partial class gob_package {

// tooBig provides a sanity check for sizes; used in several places. Upper limit
// of is 1GB on 32-bit systems, 8GB on 64-bit, allowing room to grow a little
// without overflow.
internal static readonly UntypedInt tooBig = /* (1 << 30) << (^uint(0) >> 62) */ 8589934592;

// A Decoder manages the receipt of type and data information read from the
// remote side of a connection.  It is safe for concurrent use by multiple
// goroutines.
//
// The Decoder does only basic sanity checking on decoded input sizes,
// and its limits are not configurable. Take caution when decoding gob data
// from untrusted sources.
[GoType] partial struct Decoder {
    internal sync_package.Mutex mutex;                              // each item must be received atomically
    internal io_package.Reader r;                               // source of the data
    internal decBuffer buf;                               // buffer for more efficient i/o from r
    internal gob.wireType wireType;                    // map from remote ID to local description
    internal gob.decEngine decoderCache; // cache of compiled engines
    internal gob.decEngine ignorerCache;                  // ditto for ignored objects
    internal ж<decoderState> freeList;                        // list of free decoderStates; avoids reallocation
    internal slice<byte> countBuf;                             // used for decoding integers while parsing messages
    internal error err;
    // ignoreDepth tracks the depth of recursively parsed ignored fields
    internal nint ignoreDepth;
}

// NewDecoder returns a new decoder that reads from the [io.Reader].
// If r does not also implement [io.ByteReader], it will be wrapped in a
// [bufio.Reader].
public static ж<Decoder> NewDecoder(io.Reader r) {
    var dec = @new<Decoder>();
    // We use the ability to read bytes as a plausible surrogate for buffering.
    {
        var (_, ok) = r._<io.ByteReader>(ᐧ); if (!ok) {
            r = ~bufio.NewReader(r);
        }
    }
    dec.val.r = r;
    dec.val.wireType = new gob.wireType();
    dec.val.decoderCache = new gob.decEngine();
    dec.val.ignorerCache = new gob.decEngine();
    dec.val.countBuf = new slice<byte>(9);
    // counts may be uint64s (unlikely!), require 9 bytes
    return dec;
}

// recvType loads the definition of a type.
[GoRecv] internal static void recvType(this ref Decoder dec, typeId id) {
    // Have we already seen this type? That's an error
    if (id < firstUserId || dec.wireType[id] != nil) {
        dec.err = errors.New("gob: duplicate type received"u8);
        return;
    }
    // Type:
    var wire = @new<wireType>();
    dec.decodeValue(tWireType, reflect.ValueOf(wire));
    if (dec.err != default!) {
        return;
    }
    // Remember we've seen this type.
    dec.wireType[id] = wire;
}

internal static error errBadCount = errors.New("invalid message length"u8);

// recvMessage reads the next count-delimited item from the input. It is the converse
// of Encoder.writeMessage. It returns false on EOF or other error reading the message.
[GoRecv] internal static bool recvMessage(this ref Decoder dec) {
    // Read a count.
    var (nbytes, _, err) = decodeUintReader(dec.r, dec.countBuf);
    if (err != default!) {
        dec.err = err;
        return false;
    }
    if (nbytes >= tooBig) {
        dec.err = errBadCount;
        return false;
    }
    dec.readMessage(((nint)nbytes));
    return dec.err == default!;
}

// readMessage reads the next nbytes bytes from the input.
[GoRecv] internal static void readMessage(this ref Decoder dec, nint nbytes) {
    if (dec.buf.Len() != 0) {
        // The buffer should always be empty now.
        throw panic("non-empty decoder buffer");
    }
    // Read the data
    slice<byte> buf = default!;
    (buf, dec.err) = saferio.ReadData(dec.r, ((uint64)nbytes));
    dec.buf.SetBytes(buf);
    if (AreEqual(dec.err, io.EOF)) {
        dec.err = io.ErrUnexpectedEOF;
    }
}

// toInt turns an encoded uint64 into an int, according to the marshaling rules.
internal static int64 toInt(uint64 x) {
    var i = ((int64)(x >> (int)(1)));
    if ((uint64)(x & 1) != 0) {
        i = ^i;
    }
    return i;
}

[GoRecv] internal static int64 nextInt(this ref Decoder dec) {
    var (n, _, err) = decodeUintReader(dec.buf, dec.countBuf);
    if (err != default!) {
        dec.err = err;
    }
    return toInt(n);
}

[GoRecv] internal static uint64 nextUint(this ref Decoder dec) {
    var (n, _, err) = decodeUintReader(dec.buf, dec.countBuf);
    if (err != default!) {
        dec.err = err;
    }
    return n;
}

// decodeTypeSequence parses:
// TypeSequence
//
//	(TypeDefinition DelimitedTypeDefinition*)?
//
// and returns the type id of the next value. It returns -1 at
// EOF.  Upon return, the remainder of dec.buf is the value to be
// decoded. If this is an interface value, it can be ignored by
// resetting that buffer.
[GoRecv] internal static typeId decodeTypeSequence(this ref Decoder dec, bool isInterface) {
    var firstMessage = true;
    while (dec.err == default!) {
        if (dec.buf.Len() == 0) {
            if (!dec.recvMessage()) {
                // We can only return io.EOF if the input was empty.
                // If we read one or more type spec messages,
                // require a data item message to follow.
                // If we hit an EOF before that, then give ErrUnexpectedEOF.
                if (!firstMessage && AreEqual(dec.err, io.EOF)) {
                    dec.err = io.ErrUnexpectedEOF;
                }
                break;
            }
        }
        // Receive a type id.
        var id = ((typeId)dec.nextInt());
        if (id >= 0) {
            // Value follows.
            return id;
        }
        // Type definition for (-id) follows.
        dec.recvType(-id);
        if (dec.err != default!) {
            break;
        }
        // When decoding an interface, after a type there may be a
        // DelimitedValue still in the buffer. Skip its count.
        // (Alternatively, the buffer is empty and the byte count
        // will be absorbed by recvMessage.)
        if (dec.buf.Len() > 0) {
            if (!isInterface) {
                dec.err = errors.New("extra data in buffer"u8);
                break;
            }
            dec.nextUint();
        }
        firstMessage = false;
    }
    return -1;
}

// Decode reads the next value from the input stream and stores
// it in the data represented by the empty interface value.
// If e is nil, the value will be discarded. Otherwise,
// the value underlying e must be a pointer to the
// correct type for the next data item received.
// If the input is at EOF, Decode returns [io.EOF] and
// does not modify e.
[GoRecv] public static error Decode(this ref Decoder dec, any e) {
    if (e == default!) {
        return dec.DecodeValue(new reflectꓸValue(nil));
    }
    var value = reflect.ValueOf(e);
    // If e represents a value as opposed to a pointer, the answer won't
    // get back to the caller. Make sure it's a pointer.
    if (value.Type().Kind() != reflect.ΔPointer) {
        dec.err = errors.New("gob: attempt to decode into a non-pointer"u8);
        return dec.err;
    }
    return dec.DecodeValue(value);
}

// DecodeValue reads the next value from the input stream.
// If v is the zero reflect.Value (v.Kind() == Invalid), DecodeValue discards the value.
// Otherwise, it stores the value into v. In that case, v must represent
// a non-nil pointer to data or be an assignable reflect.Value (v.CanSet())
// If the input is at EOF, DecodeValue returns [io.EOF] and
// does not modify v.
[GoRecv] public static error DecodeValue(this ref Decoder dec, reflectꓸValue v) => func((defer, _) => {
    if (v.IsValid()) {
        if (v.Kind() == reflect.ΔPointer && !v.IsNil()){
        } else 
        if (!v.CanSet()) {
            // That's okay, we'll store through the pointer.
            return errors.New("gob: DecodeValue of unassignable value"u8);
        }
    }
    // Make sure we're single-threaded through here.
    dec.mutex.Lock();
    defer(dec.mutex.Unlock);
    dec.buf.Reset();
    // In case data lingers from previous invocation.
    dec.err = default!;
    var id = dec.decodeTypeSequence(false);
    if (dec.err == default!) {
        dec.decodeValue(id, v);
    }
    return dec.err;
});

// If debug.go is compiled into the program, debugFunc prints a human-readable
// representation of the gob data read from r by calling that file's Debug function.
// Otherwise it is nil.
internal static Action<io.Reader> debugFunc;

} // end gob_package
