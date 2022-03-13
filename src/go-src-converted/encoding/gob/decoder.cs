// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gob -- go2cs converted at 2022 March 13 05:39:34 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Program Files\Go\src\encoding\gob\decoder.go
namespace go.encoding;

using bufio = bufio_package;
using errors = errors_package;
using io = io_package;
using reflect = reflect_package;
using sync = sync_package;


// tooBig provides a sanity check for sizes; used in several places. Upper limit
// of is 1GB on 32-bit systems, 8GB on 64-bit, allowing room to grow a little
// without overflow.

using System;
public static partial class gob_package {

private static readonly nint tooBig = (1 << 30) << (int)((~uint(0) >> 62));

// A Decoder manages the receipt of type and data information read from the
// remote side of a connection.  It is safe for concurrent use by multiple
// goroutines.
//
// The Decoder does only basic sanity checking on decoded input sizes,
// and its limits are not configurable. Take caution when decoding gob data
// from untrusted sources.


// A Decoder manages the receipt of type and data information read from the
// remote side of a connection.  It is safe for concurrent use by multiple
// goroutines.
//
// The Decoder does only basic sanity checking on decoded input sizes,
// and its limits are not configurable. Take caution when decoding gob data
// from untrusted sources.
public partial struct Decoder {
    public sync.Mutex mutex; // each item must be received atomically
    public io.Reader r; // source of the data
    public decBuffer buf; // buffer for more efficient i/o from r
    public map<typeId, ptr<wireType>> wireType; // map from remote ID to local description
    public map<reflect.Type, map<typeId, ptr<ptr<decEngine>>>> decoderCache; // cache of compiled engines
    public map<typeId, ptr<ptr<decEngine>>> ignorerCache; // ditto for ignored objects
    public ptr<decoderState> freeList; // list of free decoderStates; avoids reallocation
    public slice<byte> countBuf; // used for decoding integers while parsing messages
    public error err;
}

// NewDecoder returns a new decoder that reads from the io.Reader.
// If r does not also implement io.ByteReader, it will be wrapped in a
// bufio.Reader.
public static ptr<Decoder> NewDecoder(io.Reader r) {
    ptr<Decoder> dec = @new<Decoder>(); 
    // We use the ability to read bytes as a plausible surrogate for buffering.
    {
        io.ByteReader (_, ok) = r._<io.ByteReader>();

        if (!ok) {
            r = bufio.NewReader(r);
        }
    }
    dec.r = r;
    dec.wireType = make_map<typeId, ptr<wireType>>();
    dec.decoderCache = make_map<reflect.Type, map<typeId, ptr<ptr<decEngine>>>>();
    dec.ignorerCache = make_map<typeId, ptr<ptr<decEngine>>>();
    dec.countBuf = make_slice<byte>(9); // counts may be uint64s (unlikely!), require 9 bytes

    return _addr_dec!;
}

// recvType loads the definition of a type.
private static void recvType(this ptr<Decoder> _addr_dec, typeId id) {
    ref Decoder dec = ref _addr_dec.val;
 
    // Have we already seen this type? That's an error
    if (id < firstUserId || dec.wireType[id] != null) {
        dec.err = errors.New("gob: duplicate type received");
        return ;
    }
    ptr<wireType> wire = @new<wireType>();
    dec.decodeValue(tWireType, reflect.ValueOf(wire));
    if (dec.err != null) {
        return ;
    }
    dec.wireType[id] = wire;
}

private static var errBadCount = errors.New("invalid message length");

// recvMessage reads the next count-delimited item from the input. It is the converse
// of Encoder.writeMessage. It returns false on EOF or other error reading the message.
private static bool recvMessage(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;
 
    // Read a count.
    var (nbytes, _, err) = decodeUintReader(dec.r, dec.countBuf);
    if (err != null) {
        dec.err = err;
        return false;
    }
    if (nbytes >= tooBig) {
        dec.err = errBadCount;
        return false;
    }
    dec.readMessage(int(nbytes));
    return dec.err == null;
}

// readMessage reads the next nbytes bytes from the input.
private static void readMessage(this ptr<Decoder> _addr_dec, nint nbytes) => func((_, panic, _) => {
    ref Decoder dec = ref _addr_dec.val;

    if (dec.buf.Len() != 0) { 
        // The buffer should always be empty now.
        panic("non-empty decoder buffer");
    }
    dec.buf.Size(nbytes);
    _, dec.err = io.ReadFull(dec.r, dec.buf.Bytes());
    if (dec.err == io.EOF) {
        dec.err = io.ErrUnexpectedEOF;
    }
});

// toInt turns an encoded uint64 into an int, according to the marshaling rules.
private static long toInt(ulong x) {
    var i = int64(x >> 1);
    if (x & 1 != 0) {
        i = ~i;
    }
    return i;
}

private static long nextInt(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    var (n, _, err) = decodeUintReader(_addr_dec.buf, dec.countBuf);
    if (err != null) {
        dec.err = err;
    }
    return toInt(n);
}

private static ulong nextUint(this ptr<Decoder> _addr_dec) {
    ref Decoder dec = ref _addr_dec.val;

    var (n, _, err) = decodeUintReader(_addr_dec.buf, dec.countBuf);
    if (err != null) {
        dec.err = err;
    }
    return n;
}

// decodeTypeSequence parses:
// TypeSequence
//    (TypeDefinition DelimitedTypeDefinition*)?
// and returns the type id of the next value. It returns -1 at
// EOF.  Upon return, the remainder of dec.buf is the value to be
// decoded. If this is an interface value, it can be ignored by
// resetting that buffer.
private static typeId decodeTypeSequence(this ptr<Decoder> _addr_dec, bool isInterface) {
    ref Decoder dec = ref _addr_dec.val;

    while (dec.err == null) {
        if (dec.buf.Len() == 0) {
            if (!dec.recvMessage()) {
                break;
            }
        }
        var id = typeId(dec.nextInt());
        if (id >= 0) { 
            // Value follows.
            return id;
        }
        dec.recvType(-id);
        if (dec.err != null) {
            break;
        }
        if (dec.buf.Len() > 0) {
            if (!isInterface) {
                dec.err = errors.New("extra data in buffer");
                break;
            }
            dec.nextUint();
        }
    }
    return -1;
}

// Decode reads the next value from the input stream and stores
// it in the data represented by the empty interface value.
// If e is nil, the value will be discarded. Otherwise,
// the value underlying e must be a pointer to the
// correct type for the next data item received.
// If the input is at EOF, Decode returns io.EOF and
// does not modify e.
private static error Decode(this ptr<Decoder> _addr_dec, object e) {
    ref Decoder dec = ref _addr_dec.val;

    if (e == null) {
        return error.As(dec.DecodeValue(new reflect.Value()))!;
    }
    var value = reflect.ValueOf(e); 
    // If e represents a value as opposed to a pointer, the answer won't
    // get back to the caller. Make sure it's a pointer.
    if (value.Type().Kind() != reflect.Ptr) {
        dec.err = errors.New("gob: attempt to decode into a non-pointer");
        return error.As(dec.err)!;
    }
    return error.As(dec.DecodeValue(value))!;
}

// DecodeValue reads the next value from the input stream.
// If v is the zero reflect.Value (v.Kind() == Invalid), DecodeValue discards the value.
// Otherwise, it stores the value into v. In that case, v must represent
// a non-nil pointer to data or be an assignable reflect.Value (v.CanSet())
// If the input is at EOF, DecodeValue returns io.EOF and
// does not modify v.
private static error DecodeValue(this ptr<Decoder> _addr_dec, reflect.Value v) => func((defer, _, _) => {
    ref Decoder dec = ref _addr_dec.val;

    if (v.IsValid()) {
        if (v.Kind() == reflect.Ptr && !v.IsNil()) { 
            // That's okay, we'll store through the pointer.
        }
        else if (!v.CanSet()) {
            return error.As(errors.New("gob: DecodeValue of unassignable value"))!;
        }
    }
    dec.mutex.Lock();
    defer(dec.mutex.Unlock());

    dec.buf.Reset(); // In case data lingers from previous invocation.
    dec.err = null;
    var id = dec.decodeTypeSequence(false);
    if (dec.err == null) {
        dec.decodeValue(id, v);
    }
    return error.As(dec.err)!;
});

// If debug.go is compiled into the program, debugFunc prints a human-readable
// representation of the gob data read from r by calling that file's Debug function.
// Otherwise it is nil.
private static Action<io.Reader> debugFunc = default;

} // end gob_package
