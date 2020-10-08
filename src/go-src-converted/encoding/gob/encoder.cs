// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gob -- go2cs converted at 2020 October 08 03:42:41 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Go\src\encoding\gob\encoder.go
using errors = go.errors_package;
using io = go.io_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        // An Encoder manages the transmission of type and data information to the
        // other side of a connection.  It is safe for concurrent use by multiple
        // goroutines.
        public partial struct Encoder
        {
            public sync.Mutex mutex; // each item must be sent atomically
            public slice<io.Writer> w; // where to send the data
            public map<reflect.Type, typeId> sent; // which types we've already sent
            public ptr<encoderState> countState; // stage for writing counts
            public ptr<encoderState> freeList; // list of free encoderStates; avoids reallocation
            public encBuffer byteBuf; // buffer for top-level encoderState
            public error err;
        }

        // Before we encode a message, we reserve space at the head of the
        // buffer in which to encode its length. This means we can use the
        // buffer to assemble the message without another allocation.
        private static readonly long maxLength = (long)9L; // Maximum size of an encoded length.
 // Maximum size of an encoded length.
        private static var spaceForLength = make_slice<byte>(maxLength);

        // NewEncoder returns a new encoder that will transmit on the io.Writer.
        public static ptr<Encoder> NewEncoder(io.Writer w)
        {
            ptr<Encoder> enc = @new<Encoder>();
            enc.w = new slice<io.Writer>(new io.Writer[] { w });
            enc.sent = make_map<reflect.Type, typeId>();
            enc.countState = enc.newEncoderState(@new<encBuffer>());
            return _addr_enc!;
        }

        // writer() returns the innermost writer the encoder is using
        private static io.Writer writer(this ptr<Encoder> _addr_enc)
        {
            ref Encoder enc = ref _addr_enc.val;

            return enc.w[len(enc.w) - 1L];
        }

        // pushWriter adds a writer to the encoder.
        private static void pushWriter(this ptr<Encoder> _addr_enc, io.Writer w)
        {
            ref Encoder enc = ref _addr_enc.val;

            enc.w = append(enc.w, w);
        }

        // popWriter pops the innermost writer.
        private static void popWriter(this ptr<Encoder> _addr_enc)
        {
            ref Encoder enc = ref _addr_enc.val;

            enc.w = enc.w[0L..len(enc.w) - 1L];
        }

        private static void setError(this ptr<Encoder> _addr_enc, error err)
        {
            ref Encoder enc = ref _addr_enc.val;

            if (enc.err == null)
            { // remember the first.
                enc.err = err;

            }

        }

        // writeMessage sends the data item preceded by a unsigned count of its length.
        private static void writeMessage(this ptr<Encoder> _addr_enc, io.Writer w, ptr<encBuffer> _addr_b)
        {
            ref Encoder enc = ref _addr_enc.val;
            ref encBuffer b = ref _addr_b.val;
 
            // Space has been reserved for the length at the head of the message.
            // This is a little dirty: we grab the slice from the bytes.Buffer and massage
            // it by hand.
            var message = b.Bytes();
            var messageLen = len(message) - maxLength; 
            // Length cannot be bigger than the decoder can handle.
            if (messageLen >= tooBig)
            {
                enc.setError(errors.New("gob: encoder: message too big"));
                return ;
            } 
            // Encode the length.
            enc.countState.b.Reset();
            enc.countState.encodeUint(uint64(messageLen)); 
            // Copy the length to be a prefix of the message.
            var offset = maxLength - enc.countState.b.Len();
            copy(message[offset..], enc.countState.b.Bytes()); 
            // Write the data.
            var (_, err) = w.Write(message[offset..]); 
            // Drain the buffer and restore the space at the front for the count of the next message.
            b.Reset();
            b.Write(spaceForLength);
            if (err != null)
            {
                enc.setError(err);
            }

        }

        // sendActualType sends the requested type, without further investigation, unless
        // it's been sent before.
        private static bool sendActualType(this ptr<Encoder> _addr_enc, io.Writer w, ptr<encoderState> _addr_state, ptr<userTypeInfo> _addr_ut, reflect.Type actual)
        {
            bool sent = default;
            ref Encoder enc = ref _addr_enc.val;
            ref encoderState state = ref _addr_state.val;
            ref userTypeInfo ut = ref _addr_ut.val;

            {
                var (_, alreadySent) = enc.sent[actual];

                if (alreadySent)
                {
                    return false;
                }

            }

            var (info, err) = getTypeInfo(ut);
            if (err != null)
            {
                enc.setError(err);
                return ;
            } 
            // Send the pair (-id, type)
            // Id:
            state.encodeInt(-int64(info.id)); 
            // Type:
            enc.encode(state.b, reflect.ValueOf(info.wire), wireTypeUserInfo);
            enc.writeMessage(w, state.b);
            if (enc.err != null)
            {
                return ;
            } 

            // Remember we've sent this type, both what the user gave us and the base type.
            enc.sent[ut.@base] = info.id;
            if (ut.user != ut.@base)
            {
                enc.sent[ut.user] = info.id;
            } 
            // Now send the inner types
            {
                var st = actual;


                if (st.Kind() == reflect.Struct) 
                    for (long i = 0L; i < st.NumField(); i++)
                    {
                        if (isExported(st.Field(i).Name))
                        {
                            enc.sendType(w, state, st.Field(i).Type);
                        }

                    }
                else if (st.Kind() == reflect.Array || st.Kind() == reflect.Slice) 
                    enc.sendType(w, state, st.Elem());
                else if (st.Kind() == reflect.Map) 
                    enc.sendType(w, state, st.Key());
                    enc.sendType(w, state, st.Elem());

            }
            return true;

        }

        // sendType sends the type info to the other side, if necessary.
        private static bool sendType(this ptr<Encoder> _addr_enc, io.Writer w, ptr<encoderState> _addr_state, reflect.Type origt)
        {
            bool sent = default;
            ref Encoder enc = ref _addr_enc.val;
            ref encoderState state = ref _addr_state.val;

            var ut = userType(origt);
            if (ut.externalEnc != 0L)
            { 
                // The rules are different: regardless of the underlying type's representation,
                // we need to tell the other side that the base type is a GobEncoder.
                return enc.sendActualType(w, state, ut, ut.@base);

            } 

            // It's a concrete value, so drill down to the base type.
            {
                var rt = ut.@base;


                if (rt.Kind() == reflect.Slice) 
                    // If it's []uint8, don't send; it's considered basic.
                    if (rt.Elem().Kind() == reflect.Uint8)
                    {
                        return ;
                    } 
                    // Otherwise we do send.
                    break;
                else if (rt.Kind() == reflect.Array) 
                    // arrays must be sent so we know their lengths and element types.
                    break;
                else if (rt.Kind() == reflect.Map) 
                    // maps must be sent so we know their lengths and key/value types.
                    break;
                else if (rt.Kind() == reflect.Struct) 
                    // structs must be sent so we know their fields.
                    break;
                else if (rt.Kind() == reflect.Chan || rt.Kind() == reflect.Func) 
                    // If we get here, it's a field of a struct; ignore it.
                    return ;
                else 
                    // Basic types and interfaces do not need to be described.
                    return ;

            }

            return enc.sendActualType(w, state, ut, ut.@base);

        }

        // Encode transmits the data item represented by the empty interface value,
        // guaranteeing that all necessary type information has been transmitted first.
        // Passing a nil pointer to Encoder will panic, as they cannot be transmitted by gob.
        private static error Encode(this ptr<Encoder> _addr_enc, object e)
        {
            ref Encoder enc = ref _addr_enc.val;

            return error.As(enc.EncodeValue(reflect.ValueOf(e)))!;
        }

        // sendTypeDescriptor makes sure the remote side knows about this type.
        // It will send a descriptor if this is the first time the type has been
        // sent.
        private static void sendTypeDescriptor(this ptr<Encoder> _addr_enc, io.Writer w, ptr<encoderState> _addr_state, ptr<userTypeInfo> _addr_ut)
        {
            ref Encoder enc = ref _addr_enc.val;
            ref encoderState state = ref _addr_state.val;
            ref userTypeInfo ut = ref _addr_ut.val;
 
            // Make sure the type is known to the other side.
            // First, have we already sent this type?
            var rt = ut.@base;
            if (ut.externalEnc != 0L)
            {
                rt = ut.user;
            }

            {
                var (_, alreadySent) = enc.sent[rt];

                if (!alreadySent)
                { 
                    // No, so send it.
                    var sent = enc.sendType(w, state, rt);
                    if (enc.err != null)
                    {
                        return ;
                    } 
                    // If the type info has still not been transmitted, it means we have
                    // a singleton basic type (int, []byte etc.) at top level. We don't
                    // need to send the type info but we do need to update enc.sent.
                    if (!sent)
                    {
                        var (info, err) = getTypeInfo(ut);
                        if (err != null)
                        {
                            enc.setError(err);
                            return ;
                        }

                        enc.sent[rt] = info.id;

                    }

                }

            }

        }

        // sendTypeId sends the id, which must have already been defined.
        private static void sendTypeId(this ptr<Encoder> _addr_enc, ptr<encoderState> _addr_state, ptr<userTypeInfo> _addr_ut)
        {
            ref Encoder enc = ref _addr_enc.val;
            ref encoderState state = ref _addr_state.val;
            ref userTypeInfo ut = ref _addr_ut.val;
 
            // Identify the type of this top-level value.
            state.encodeInt(int64(enc.sent[ut.@base]));

        }

        // EncodeValue transmits the data item represented by the reflection value,
        // guaranteeing that all necessary type information has been transmitted first.
        // Passing a nil pointer to EncodeValue will panic, as they cannot be transmitted by gob.
        private static error EncodeValue(this ptr<Encoder> _addr_enc, reflect.Value value) => func((defer, panic, _) =>
        {
            ref Encoder enc = ref _addr_enc.val;

            if (value.Kind() == reflect.Invalid)
            {
                return error.As(errors.New("gob: cannot encode nil value"))!;
            }

            if (value.Kind() == reflect.Ptr && value.IsNil())
            {
                panic("gob: cannot encode nil pointer of type " + value.Type().String());
            } 

            // Make sure we're single-threaded through here, so multiple
            // goroutines can share an encoder.
            enc.mutex.Lock();
            defer(enc.mutex.Unlock()); 

            // Remove any nested writers remaining due to previous errors.
            enc.w = enc.w[0L..1L];

            var (ut, err) = validUserType(value.Type());
            if (err != null)
            {
                return error.As(err)!;
            }

            enc.err = null;
            enc.byteBuf.Reset();
            enc.byteBuf.Write(spaceForLength);
            var state = enc.newEncoderState(_addr_enc.byteBuf);

            enc.sendTypeDescriptor(enc.writer(), state, ut);
            enc.sendTypeId(state, ut);
            if (enc.err != null)
            {
                return error.As(enc.err)!;
            } 

            // Encode the object.
            enc.encode(state.b, value, ut);
            if (enc.err == null)
            {
                enc.writeMessage(enc.writer(), state.b);
            }

            enc.freeEncoderState(state);
            return error.As(enc.err)!;

        });
    }
}}
