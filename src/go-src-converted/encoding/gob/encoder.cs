// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using errors = errors_package;
using io = io_package;
using reflect = reflect_package;
using sync = sync_package;

partial class gob_package {

// An Encoder manages the transmission of type and data information to the
// other side of a connection.  It is safe for concurrent use by multiple
// goroutines.
[GoType] partial struct Encoder {
    internal sync_package.Mutex mutex;              // each item must be sent atomically
    internal slice<io.Writer> w;        // where to send the data
    internal map<reflectꓸType, typeId> sent; // which types we've already sent
    internal ж<encoderState> countState;        // stage for writing counts
    internal ж<encoderState> freeList;        // list of free encoderStates; avoids reallocation
    internal encBuffer byteBuf;               // buffer for top-level encoderState
    internal error err;
}

// Before we encode a message, we reserve space at the head of the
// buffer in which to encode its length. This means we can use the
// buffer to assemble the message without another allocation.
internal static readonly UntypedInt maxLength = 9; // Maximum size of an encoded length.

internal static slice<byte> spaceForLength = new slice<byte>(maxLength);

// NewEncoder returns a new encoder that will transmit on the [io.Writer].
public static ж<Encoder> NewEncoder(io.Writer w) {
    var enc = @new<Encoder>();
    enc.val.w = new io.Writer[]{w}.slice();
    enc.val.sent = new map<reflectꓸType, typeId>();
    enc.val.countState = enc.newEncoderState(@new<encBuffer>());
    return enc;
}

// writer returns the innermost writer the encoder is using.
[GoRecv] internal static io.Writer writer(this ref Encoder enc) {
    return enc.w[len(enc.w) - 1];
}

// pushWriter adds a writer to the encoder.
[GoRecv] internal static void pushWriter(this ref Encoder enc, io.Writer w) {
    enc.w = append(enc.w, w);
}

// popWriter pops the innermost writer.
[GoRecv] internal static void popWriter(this ref Encoder enc) {
    enc.w = enc.w[0..(int)(len(enc.w) - 1)];
}

[GoRecv] internal static void setError(this ref Encoder enc, error err) {
    if (enc.err == default!) {
        // remember the first.
        enc.err = err;
    }
}

// writeMessage sends the data item preceded by an unsigned count of its length.
[GoRecv] public static void writeMessage(this ref Encoder enc, io.Writer w, ж<encBuffer> Ꮡb) {
    ref var b = ref Ꮡb.val;

    // Space has been reserved for the length at the head of the message.
    // This is a little dirty: we grab the slice from the bytes.Buffer and massage
    // it by hand.
    var message = b.Bytes();
    nint messageLen = len(message) - maxLength;
    // Length cannot be bigger than the decoder can handle.
    if (messageLen >= tooBig) {
        enc.setError(errors.New("gob: encoder: message too big"u8));
        return;
    }
    // Encode the length.
    enc.countState.b.Reset();
    enc.countState.encodeUint(((uint64)messageLen));
    // Copy the length to be a prefix of the message.
    nint offset = maxLength - enc.countState.b.Len();
    copy(message[(int)(offset)..], enc.countState.b.Bytes());
    // Write the data.
    var (_, err) = w.Write(message[(int)(offset)..]);
    // Drain the buffer and restore the space at the front for the count of the next message.
    b.Reset();
    b.Write(spaceForLength);
    if (err != default!) {
        enc.setError(err);
    }
}

// sendActualType sends the requested type, without further investigation, unless
// it's been sent before.
[GoRecv] public static bool /*sent*/ sendActualType(this ref Encoder enc, io.Writer w, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut, reflectꓸType actual) {
    bool sent = default!;

    ref var state = ref Ꮡstate.val;
    ref var ut = ref Ꮡut.val;
    {
        var (_, alreadySent) = enc.sent[actual]; if (alreadySent) {
            return false;
        }
    }
    (info, err) = getTypeInfo(Ꮡut);
    if (err != default!) {
        enc.setError(err);
        return sent;
    }
    // Send the pair (-id, type)
    // Id:
    state.encodeInt(-((int64)(~info).id));
    // Type:
    enc.encode(state.b, reflect.ValueOf((~info).wire), wireTypeUserInfo);
    enc.writeMessage(w, state.b);
    if (enc.err != default!) {
        return sent;
    }
    // Remember we've sent this type, both what the user gave us and the base type.
    enc.sent[ut.@base] = info.val.id;
    if (!AreEqual(ut.user, ut.@base)) {
        enc.sent[ut.user] = info.val.id;
    }
    // Now send the inner types
    {
        var st = actual;
        var exprᴛ1 = st.Kind();
        if (exprᴛ1 == reflect.Struct) {
            for (nint i = 0; i < st.NumField(); i++) {
                if (isExported(st.Field(i).Name)) {
                    enc.sendType(w, Ꮡstate, st.Field(i).Type);
                }
            }
        }
        else if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) {
            enc.sendType(w, Ꮡstate, st.Elem());
        }
        else if (exprᴛ1 == reflect.Map) {
            enc.sendType(w, Ꮡstate, st.Key());
            enc.sendType(w, Ꮡstate, st.Elem());
        }
    }

    return true;
}

// sendType sends the type info to the other side, if necessary.
[GoRecv] public static bool /*sent*/ sendType(this ref Encoder enc, io.Writer w, ж<encoderState> Ꮡstate, reflectꓸType origt) {
    bool sent = default!;

    ref var state = ref Ꮡstate.val;
    var ut = userType(origt);
    if ((~ut).externalEnc != 0) {
        // The rules are different: regardless of the underlying type's representation,
        // we need to tell the other side that the base type is a GobEncoder.
        return enc.sendActualType(w, Ꮡstate, ut, (~ut).@base);
    }
    // It's a concrete value, so drill down to the base type.
    {
        var rt = ut.val.@base;
        var exprᴛ1 = rt.Kind();
        { /* default: */
            return sent;
        }
        if (exprᴛ1 == reflect.ΔSlice) {
            if (rt.Elem().Kind() == reflect.Uint8) {
                // Basic types and interfaces do not need to be described.
                // If it's []uint8, don't send; it's considered basic.
                return sent;
            }
            break;
        }
        else if (exprᴛ1 == reflect.Array) {
            break;
        }
        else if (exprᴛ1 == reflect.Map) {
            break;
        }
        else if (exprᴛ1 == reflect.Struct) {
            break;
        }
        else if (exprᴛ1 == reflect.Chan || exprᴛ1 == reflect.Func) {
            return sent;
        }
    }

    // Otherwise we do send.
    // arrays must be sent so we know their lengths and element types.
    // maps must be sent so we know their lengths and key/value types.
    // structs must be sent so we know their fields.
    // If we get here, it's a field of a struct; ignore it.
    return enc.sendActualType(w, Ꮡstate, ut, (~ut).@base);
}

// Encode transmits the data item represented by the empty interface value,
// guaranteeing that all necessary type information has been transmitted first.
// Passing a nil pointer to Encoder will panic, as they cannot be transmitted by gob.
[GoRecv] public static error Encode(this ref Encoder enc, any e) {
    return enc.EncodeValue(reflect.ValueOf(e));
}

// sendTypeDescriptor makes sure the remote side knows about this type.
// It will send a descriptor if this is the first time the type has been
// sent.
[GoRecv] public static void sendTypeDescriptor(this ref Encoder enc, io.Writer w, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut) {
    ref var state = ref Ꮡstate.val;
    ref var ut = ref Ꮡut.val;

    // Make sure the type is known to the other side.
    // First, have we already sent this type?
    var rt = ut.@base;
    if (ut.externalEnc != 0) {
        rt = ut.user;
    }
    {
        var (_, alreadySent) = enc.sent[rt]; if (!alreadySent) {
            // No, so send it.
            var sent = enc.sendType(w, Ꮡstate, rt);
            if (enc.err != default!) {
                return;
            }
            // If the type info has still not been transmitted, it means we have
            // a singleton basic type (int, []byte etc.) at top level. We don't
            // need to send the type info but we do need to update enc.sent.
            if (!sent) {
                (info, err) = getTypeInfo(Ꮡut);
                if (err != default!) {
                    enc.setError(err);
                    return;
                }
                enc.sent[rt] = info.val.id;
            }
        }
    }
}

// sendTypeId sends the id, which must have already been defined.
[GoRecv] public static void sendTypeId(this ref Encoder enc, ж<encoderState> Ꮡstate, ж<userTypeInfo> Ꮡut) {
    ref var state = ref Ꮡstate.val;
    ref var ut = ref Ꮡut.val;

    // Identify the type of this top-level value.
    state.encodeInt(((int64)enc.sent[ut.@base]));
}

// EncodeValue transmits the data item represented by the reflection value,
// guaranteeing that all necessary type information has been transmitted first.
// Passing a nil pointer to EncodeValue will panic, as they cannot be transmitted by gob.
[GoRecv] public static error EncodeValue(this ref Encoder enc, reflectꓸValue value) => func((defer, _) => {
    if (value.Kind() == reflect.Invalid) {
        return errors.New("gob: cannot encode nil value"u8);
    }
    if (value.Kind() == reflect.ΔPointer && value.IsNil()) {
        throw panic("gob: cannot encode nil pointer of type "u8 + value.Type().String());
    }
    // Make sure we're single-threaded through here, so multiple
    // goroutines can share an encoder.
    enc.mutex.Lock();
    defer(enc.mutex.Unlock);
    // Remove any nested writers remaining due to previous errors.
    enc.w = enc.w[0..1];
    (ut, err) = validUserType(value.Type());
    if (err != default!) {
        return err;
    }
    enc.err = default!;
    enc.byteBuf.Reset();
    enc.byteBuf.Write(spaceForLength);
    var state = enc.newEncoderState(Ꮡ(enc.byteBuf));
    enc.sendTypeDescriptor(enc.writer(), state, ut);
    enc.sendTypeId(state, ut);
    if (enc.err != default!) {
        return enc.err;
    }
    // Encode the object.
    enc.encode((~state).b, value, ut);
    if (enc.err == default!) {
        enc.writeMessage(enc.writer(), (~state).b);
    }
    enc.freeEncoderState(state);
    return enc.err;
});

} // end gob_package
