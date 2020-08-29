// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run encgen.go -output enc_helpers.go

// package gob -- go2cs converted at 2020 August 29 08:35:33 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Go\src\encoding\gob\encode.go
using encoding = go.encoding_package;
using binary = go.encoding.binary_package;
using math = go.math_package;
using bits = go.math.bits_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        private static readonly long uint64Size = 8L;



        public delegate  bool encHelper(ref encoderState,  reflect.Value);

        // encoderState is the global execution state of an instance of the encoder.
        // Field numbers are delta encoded and always increase. The field
        // number is initialized to -1 so 0 comes out as delta(1). A delta of
        // 0 terminates the structure.
        private partial struct encoderState
        {
            public ptr<Encoder> enc;
            public ptr<encBuffer> b;
            public bool sendZero; // encoding an array element or map key/value pair; send zero values
            public long fieldnum; // the last field number written.
            public array<byte> buf; // buffer used by the encoder; here to avoid allocation.
            public ptr<encoderState> next; // for free list
        }

        // encBuffer is an extremely simple, fast implementation of a write-only byte buffer.
        // It never returns a non-nil error, but Write returns an error value so it matches io.Writer.
        private partial struct encBuffer
        {
            public slice<byte> data;
            public array<byte> scratch;
        }

        private static sync.Pool encBufferPool = new sync.Pool(New:func()interface{}{e:=new(encBuffer)e.data=e.scratch[0:0]returne},);

        private static void WriteByte(this ref encBuffer e, byte c)
        {
            e.data = append(e.data, c);
        }

        private static (long, error) Write(this ref encBuffer e, slice<byte> p)
        {
            e.data = append(e.data, p);
            return (len(p), null);
        }

        private static void WriteString(this ref encBuffer e, @string s)
        {
            e.data = append(e.data, s);
        }

        private static long Len(this ref encBuffer e)
        {
            return len(e.data);
        }

        private static slice<byte> Bytes(this ref encBuffer e)
        {
            return e.data;
        }

        private static void Reset(this ref encBuffer e)
        {
            if (len(e.data) >= tooBig)
            {
                e.data = e.scratch[0L..0L];
            }
            else
            {
                e.data = e.data[0L..0L];
            }
        }

        private static ref encoderState newEncoderState(this ref Encoder enc, ref encBuffer b)
        {
            var e = enc.freeList;
            if (e == null)
            {
                e = @new<encoderState>();
                e.enc = enc;
            }
            else
            {
                enc.freeList = e.next;
            }
            e.sendZero = false;
            e.fieldnum = 0L;
            e.b = b;
            if (len(b.data) == 0L)
            {
                b.data = b.scratch[0L..0L];
            }
            return e;
        }

        private static void freeEncoderState(this ref Encoder enc, ref encoderState e)
        {
            e.next = enc.freeList;
            enc.freeList = e;
        }

        // Unsigned integers have a two-state encoding. If the number is less
        // than 128 (0 through 0x7F), its value is written directly.
        // Otherwise the value is written in big-endian byte order preceded
        // by the byte length, negated.

        // encodeUint writes an encoded unsigned integer to state.b.
        private static void encodeUint(this ref encoderState state, ulong x)
        {
            if (x <= 0x7FUL)
            {
                state.b.WriteByte(uint8(x));
                return;
            }
            binary.BigEndian.PutUint64(state.buf[1L..], x);
            var bc = bits.LeadingZeros64(x) >> (int)(3L); // 8 - bytelen(x)
            state.buf[bc] = uint8(bc - uint64Size); // and then we subtract 8 to get -bytelen(x)

            state.b.Write(state.buf[bc..uint64Size + 1L]);
        }

        // encodeInt writes an encoded signed integer to state.w.
        // The low bit of the encoding says whether to bit complement the (other bits of the)
        // uint to recover the int.
        private static void encodeInt(this ref encoderState state, long i)
        {
            ulong x = default;
            if (i < 0L)
            {
                x = uint64(~i << (int)(1L)) | 1L;
            }
            else
            {
                x = uint64(i << (int)(1L));
            }
            state.encodeUint(x);
        }

        // encOp is the signature of an encoding operator for a given type.
        public delegate void encOp(ref encInstr, ref encoderState, reflect.Value);

        // The 'instructions' of the encoding machine
        private partial struct encInstr
        {
            public encOp op;
            public long field; // field number in input
            public slice<long> index; // struct index
            public long indir; // how many pointer indirections to reach the value in the struct
        }

        // update emits a field number and updates the state to record its value for delta encoding.
        // If the instruction pointer is nil, it does nothing
        private static void update(this ref encoderState state, ref encInstr instr)
        {
            if (instr != null)
            {
                state.encodeUint(uint64(instr.field - state.fieldnum));
                state.fieldnum = instr.field;
            }
        }

        // Each encoder for a composite is responsible for handling any
        // indirections associated with the elements of the data structure.
        // If any pointer so reached is nil, no bytes are written. If the
        // data item is zero, no bytes are written. Single values - ints,
        // strings etc. - are indirected before calling their encoders.
        // Otherwise, the output (for a scalar) is the field number, as an
        // encoded integer, followed by the field data in its appropriate
        // format.

        // encIndirect dereferences pv indir times and returns the result.
        private static reflect.Value encIndirect(reflect.Value pv, long indir)
        {
            while (indir > 0L)
            {
                if (pv.IsNil())
                {
                    break;
                indir--;
                }
                pv = pv.Elem();
            }

            return pv;
        }

        // encBool encodes the bool referenced by v as an unsigned 0 or 1.
        private static void encBool(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var b = v.Bool();
            if (b || state.sendZero)
            {
                state.update(i);
                if (b)
                {
                    state.encodeUint(1L);
                }
                else
                {
                    state.encodeUint(0L);
                }
            }
        }

        // encInt encodes the signed integer (int int8 int16 int32 int64) referenced by v.
        private static void encInt(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var value = v.Int();
            if (value != 0L || state.sendZero)
            {
                state.update(i);
                state.encodeInt(value);
            }
        }

        // encUint encodes the unsigned integer (uint uint8 uint16 uint32 uint64 uintptr) referenced by v.
        private static void encUint(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var value = v.Uint();
            if (value != 0L || state.sendZero)
            {
                state.update(i);
                state.encodeUint(value);
            }
        }

        // floatBits returns a uint64 holding the bits of a floating-point number.
        // Floating-point numbers are transmitted as uint64s holding the bits
        // of the underlying representation. They are sent byte-reversed, with
        // the exponent end coming out first, so integer floating point numbers
        // (for example) transmit more compactly. This routine does the
        // swizzling.
        private static ulong floatBits(double f)
        {
            var u = math.Float64bits(f);
            return bits.ReverseBytes64(u);
        }

        // encFloat encodes the floating point value (float32 float64) referenced by v.
        private static void encFloat(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var f = v.Float();
            if (f != 0L || state.sendZero)
            {
                var bits = floatBits(f);
                state.update(i);
                state.encodeUint(bits);
            }
        }

        // encComplex encodes the complex value (complex64 complex128) referenced by v.
        // Complex numbers are just a pair of floating-point numbers, real part first.
        private static void encComplex(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var c = v.Complex();
            if (c != 0L + 0iUL || state.sendZero)
            {
                var rpart = floatBits(real(c));
                var ipart = floatBits(imag(c));
                state.update(i);
                state.encodeUint(rpart);
                state.encodeUint(ipart);
            }
        }

        // encUint8Array encodes the byte array referenced by v.
        // Byte arrays are encoded as an unsigned count followed by the raw bytes.
        private static void encUint8Array(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var b = v.Bytes();
            if (len(b) > 0L || state.sendZero)
            {
                state.update(i);
                state.encodeUint(uint64(len(b)));
                state.b.Write(b);
            }
        }

        // encString encodes the string referenced by v.
        // Strings are encoded as an unsigned count followed by the raw bytes.
        private static void encString(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            var s = v.String();
            if (len(s) > 0L || state.sendZero)
            {
                state.update(i);
                state.encodeUint(uint64(len(s)));
                state.b.WriteString(s);
            }
        }

        // encStructTerminator encodes the end of an encoded struct
        // as delta field number of 0.
        private static void encStructTerminator(ref encInstr i, ref encoderState state, reflect.Value v)
        {
            state.encodeUint(0L);
        }

        // Execution engine

        // encEngine an array of instructions indexed by field number of the encoding
        // data, typically a struct. It is executed top to bottom, walking the struct.
        private partial struct encEngine
        {
            public slice<encInstr> instr;
        }

        private static readonly long singletonField = 0L;

        // valid reports whether the value is valid and a non-nil pointer.
        // (Slices, maps, and chans take care of themselves.)


        // valid reports whether the value is valid and a non-nil pointer.
        // (Slices, maps, and chans take care of themselves.)
        private static bool valid(reflect.Value v)
        {

            if (v.Kind() == reflect.Invalid) 
                return false;
            else if (v.Kind() == reflect.Ptr) 
                return !v.IsNil();
                        return true;
        }

        // encodeSingle encodes a single top-level non-struct value.
        private static void encodeSingle(this ref Encoder _enc, ref encBuffer _b, ref encEngine _engine, reflect.Value value) => func(_enc, _b, _engine, (ref Encoder enc, ref encBuffer b, ref encEngine engine, Defer defer, Panic _, Recover __) =>
        {
            var state = enc.newEncoderState(b);
            defer(enc.freeEncoderState(state));
            state.fieldnum = singletonField; 
            // There is no surrounding struct to frame the transmission, so we must
            // generate data even if the item is zero. To do this, set sendZero.
            state.sendZero = true;
            var instr = ref engine.instr[singletonField];
            if (instr.indir > 0L)
            {
                value = encIndirect(value, instr.indir);
            }
            if (valid(value))
            {
                instr.op(instr, state, value);
            }
        });

        // encodeStruct encodes a single struct value.
        private static void encodeStruct(this ref Encoder _enc, ref encBuffer _b, ref encEngine _engine, reflect.Value value) => func(_enc, _b, _engine, (ref Encoder enc, ref encBuffer b, ref encEngine engine, Defer defer, Panic _, Recover __) =>
        {
            if (!valid(value))
            {
                return;
            }
            var state = enc.newEncoderState(b);
            defer(enc.freeEncoderState(state));
            state.fieldnum = -1L;
            for (long i = 0L; i < len(engine.instr); i++)
            {
                var instr = ref engine.instr[i];
                if (i >= value.NumField())
                { 
                    // encStructTerminator
                    instr.op(instr, state, new reflect.Value());
                    break;
                }
                var field = value.FieldByIndex(instr.index);
                if (instr.indir > 0L)
                {
                    field = encIndirect(field, instr.indir); 
                    // TODO: Is field guaranteed valid? If so we could avoid this check.
                    if (!valid(field))
                    {
                        continue;
                    }
                }
                instr.op(instr, state, field);
            }

        });

        // encodeArray encodes an array.
        private static void encodeArray(this ref Encoder _enc, ref encBuffer _b, reflect.Value value, encOp op, long elemIndir, long length, encHelper helper) => func(_enc, _b, (ref Encoder enc, ref encBuffer b, Defer defer, Panic _, Recover __) =>
        {
            var state = enc.newEncoderState(b);
            defer(enc.freeEncoderState(state));
            state.fieldnum = -1L;
            state.sendZero = true;
            state.encodeUint(uint64(length));
            if (helper != null && helper(state, value))
            {
                return;
            }
            for (long i = 0L; i < length; i++)
            {
                var elem = value.Index(i);
                if (elemIndir > 0L)
                {
                    elem = encIndirect(elem, elemIndir); 
                    // TODO: Is elem guaranteed valid? If so we could avoid this check.
                    if (!valid(elem))
                    {
                        errorf("encodeArray: nil element");
                    }
                }
                op(null, state, elem);
            }

        });

        // encodeReflectValue is a helper for maps. It encodes the value v.
        private static void encodeReflectValue(ref encoderState state, reflect.Value v, encOp op, long indir)
        {
            for (long i = 0L; i < indir && v.IsValid(); i++)
            {
                v = reflect.Indirect(v);
            }

            if (!v.IsValid())
            {
                errorf("encodeReflectValue: nil element");
            }
            op(null, state, v);
        }

        // encodeMap encodes a map as unsigned count followed by key:value pairs.
        private static void encodeMap(this ref Encoder enc, ref encBuffer b, reflect.Value mv, encOp keyOp, encOp elemOp, long keyIndir, long elemIndir)
        {
            var state = enc.newEncoderState(b);
            state.fieldnum = -1L;
            state.sendZero = true;
            var keys = mv.MapKeys();
            state.encodeUint(uint64(len(keys)));
            foreach (var (_, key) in keys)
            {
                encodeReflectValue(state, key, keyOp, keyIndir);
                encodeReflectValue(state, mv.MapIndex(key), elemOp, elemIndir);
            }
            enc.freeEncoderState(state);
        }

        // encodeInterface encodes the interface value iv.
        // To send an interface, we send a string identifying the concrete type, followed
        // by the type identifier (which might require defining that type right now), followed
        // by the concrete value. A nil value gets sent as the empty string for the name,
        // followed by no value.
        private static void encodeInterface(this ref Encoder enc, ref encBuffer b, reflect.Value iv)
        { 
            // Gobs can encode nil interface values but not typed interface
            // values holding nil pointers, since nil pointers point to no value.
            var elem = iv.Elem();
            if (elem.Kind() == reflect.Ptr && elem.IsNil())
            {
                errorf("gob: cannot encode nil pointer of type %s inside interface", iv.Elem().Type());
            }
            var state = enc.newEncoderState(b);
            state.fieldnum = -1L;
            state.sendZero = true;
            if (iv.IsNil())
            {
                state.encodeUint(0L);
                return;
            }
            var ut = userType(iv.Elem().Type());
            var (namei, ok) = concreteTypeToName.Load(ut.@base);
            if (!ok)
            {
                errorf("type not registered for interface: %s", ut.@base);
            }
            @string name = namei._<@string>(); 

            // Send the name.
            state.encodeUint(uint64(len(name)));
            state.b.WriteString(name); 
            // Define the type id if necessary.
            enc.sendTypeDescriptor(enc.writer(), state, ut); 
            // Send the type id.
            enc.sendTypeId(state, ut); 
            // Encode the value into a new buffer. Any nested type definitions
            // should be written to b, before the encoded value.
            enc.pushWriter(b);
            ref encBuffer data = encBufferPool.Get()._<ref encBuffer>();
            data.Write(spaceForLength);
            enc.encode(data, elem, ut);
            if (enc.err != null)
            {
                error_(enc.err);
            }
            enc.popWriter();
            enc.writeMessage(b, data);
            data.Reset();
            encBufferPool.Put(data);
            if (enc.err != null)
            {
                error_(enc.err);
            }
            enc.freeEncoderState(state);
        }

        // isZero reports whether the value is the zero of its type.
        private static bool isZero(reflect.Value val) => func((_, panic, __) =>
        {

            if (val.Kind() == reflect.Array) 
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < val.Len(); i++)
                    {
                        if (!isZero(val.Index(i)))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
            else if (val.Kind() == reflect.Map || val.Kind() == reflect.Slice || val.Kind() == reflect.String) 
                return val.Len() == 0L;
            else if (val.Kind() == reflect.Bool) 
                return !val.Bool();
            else if (val.Kind() == reflect.Complex64 || val.Kind() == reflect.Complex128) 
                return val.Complex() == 0L;
            else if (val.Kind() == reflect.Chan || val.Kind() == reflect.Func || val.Kind() == reflect.Interface || val.Kind() == reflect.Ptr) 
                return val.IsNil();
            else if (val.Kind() == reflect.Int || val.Kind() == reflect.Int8 || val.Kind() == reflect.Int16 || val.Kind() == reflect.Int32 || val.Kind() == reflect.Int64) 
                return val.Int() == 0L;
            else if (val.Kind() == reflect.Float32 || val.Kind() == reflect.Float64) 
                return val.Float() == 0L;
            else if (val.Kind() == reflect.Uint || val.Kind() == reflect.Uint8 || val.Kind() == reflect.Uint16 || val.Kind() == reflect.Uint32 || val.Kind() == reflect.Uint64 || val.Kind() == reflect.Uintptr) 
                return val.Uint() == 0L;
            else if (val.Kind() == reflect.Struct) 
                {
                    long i__prev1 = i;

                    for (i = 0L; i < val.NumField(); i++)
                    {
                        if (!isZero(val.Field(i)))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
                        panic("unknown type in isZero " + val.Type().String());
        });

        // encGobEncoder encodes a value that implements the GobEncoder interface.
        // The data is sent as a byte array.
        private static void encodeGobEncoder(this ref Encoder enc, ref encBuffer b, ref userTypeInfo ut, reflect.Value v)
        { 
            // TODO: should we catch panics from the called method?

            slice<byte> data = default;
            error err = default; 
            // We know it's one of these.

            if (ut.externalEnc == xGob) 
                data, err = v.Interface()._<GobEncoder>().GobEncode();
            else if (ut.externalEnc == xBinary) 
                data, err = v.Interface()._<encoding.BinaryMarshaler>().MarshalBinary();
            else if (ut.externalEnc == xText) 
                data, err = v.Interface()._<encoding.TextMarshaler>().MarshalText();
                        if (err != null)
            {
                error_(err);
            }
            var state = enc.newEncoderState(b);
            state.fieldnum = -1L;
            state.encodeUint(uint64(len(data)));
            state.b.Write(data);
            enc.freeEncoderState(state);
        }

        private static array<encOp> encOpTable = new array<encOp>(InitKeyedValues<encOp>((reflect.Bool, encBool), (reflect.Int, encInt), (reflect.Int8, encInt), (reflect.Int16, encInt), (reflect.Int32, encInt), (reflect.Int64, encInt), (reflect.Uint, encUint), (reflect.Uint8, encUint), (reflect.Uint16, encUint), (reflect.Uint32, encUint), (reflect.Uint64, encUint), (reflect.Uintptr, encUint), (reflect.Float32, encFloat), (reflect.Float64, encFloat), (reflect.Complex64, encComplex), (reflect.Complex128, encComplex), (reflect.String, encString)));

        // encOpFor returns (a pointer to) the encoding op for the base type under rt and
        // the indirection count to reach it.
        private static (ref encOp, long) encOpFor(reflect.Type rt, map<reflect.Type, ref encOp> inProgress, map<ref typeInfo, bool> building)
        {
            var ut = userType(rt); 
            // If the type implements GobEncoder, we handle it without further processing.
            if (ut.externalEnc != 0L)
            {
                return gobEncodeOpFor(ut);
            } 
            // If this type is already in progress, it's a recursive type (e.g. map[string]*T).
            // Return the pointer to the op we're already building.
            {
                var opPtr = inProgress[rt];

                if (opPtr != null)
                {
                    return (opPtr, ut.indir);
                }

            }
            var typ = ut.@base;
            var indir = ut.indir;
            var k = typ.Kind();
            encOp op = default;
            if (int(k) < len(encOpTable))
            {
                op = encOpTable[k];
            }
            if (op == null)
            {
                inProgress[rt] = ref op; 
                // Special cases
                {
                    var t = typ;


                    if (t.Kind() == reflect.Slice) 
                        if (t.Elem().Kind() == reflect.Uint8)
                        {
                            op = encUint8Array;
                            break;
                        } 
                        // Slices have a header; we decode it to find the underlying array.
                        var (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                        var helper = encSliceHelper[t.Elem().Kind()];
                        op = (i, state, slice) =>
                        {
                            if (!state.sendZero && slice.Len() == 0L)
                            {
                                return;
                            }
                            state.update(i);
                            state.enc.encodeArray(state.b, slice, elemOp.Value, elemIndir, slice.Len(), helper);
                        }
;
                    else if (t.Kind() == reflect.Array) 
                        // True arrays have size in the type.
                        (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                        helper = encArrayHelper[t.Elem().Kind()];
                        op = (i, state, array) =>
                        {
                            state.update(i);
                            state.enc.encodeArray(state.b, array, elemOp.Value, elemIndir, array.Len(), helper);
                        }
;
                    else if (t.Kind() == reflect.Map) 
                        var (keyOp, keyIndir) = encOpFor(t.Key(), inProgress, building);
                        (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                        op = (i, state, mv) =>
                        { 
                            // We send zero-length (but non-nil) maps because the
                            // receiver might want to use the map.  (Maps don't use append.)
                            if (!state.sendZero && mv.IsNil())
                            {
                                return;
                            }
                            state.update(i);
                            state.enc.encodeMap(state.b, mv, keyOp.Value, elemOp.Value, keyIndir, elemIndir);
                        }
;
                    else if (t.Kind() == reflect.Struct) 
                        // Generate a closure that calls out to the engine for the nested type.
                        getEncEngine(userType(typ), building);
                        var info = mustGetTypeInfo(typ);
                        op = (i, state, sv) =>
                        {
                            state.update(i); 
                            // indirect through info to delay evaluation for recursive structs
                            ref encEngine enc = info.encoder.Load()._<ref encEngine>();
                            state.enc.encodeStruct(state.b, enc, sv);
                        }
;
                    else if (t.Kind() == reflect.Interface) 
                        op = (i, state, iv) =>
                        {
                            if (!state.sendZero && (!iv.IsValid() || iv.IsNil()))
                            {
                                return;
                            }
                            state.update(i);
                            state.enc.encodeInterface(state.b, iv);
                        }
;

                }
            }
            if (op == null)
            {
                errorf("can't happen: encode type %s", rt);
            }
            return (ref op, indir);
        }

        // gobEncodeOpFor returns the op for a type that is known to implement GobEncoder.
        private static (ref encOp, long) gobEncodeOpFor(ref userTypeInfo ut)
        {
            var rt = ut.user;
            if (ut.encIndir == -1L)
            {
                rt = reflect.PtrTo(rt);
            }
            else if (ut.encIndir > 0L)
            {
                for (var i = int8(0L); i < ut.encIndir; i++)
                {
                    rt = rt.Elem();
                }

            }
            encOp op = default;
            op = (i, state, v) =>
            {
                if (ut.encIndir == -1L)
                { 
                    // Need to climb up one level to turn value into pointer.
                    if (!v.CanAddr())
                    {
                        errorf("unaddressable value of type %s", rt);
                    }
                    v = v.Addr();
                }
                if (!state.sendZero && isZero(v))
                {
                    return;
                }
                state.update(i);
                state.enc.encodeGobEncoder(state.b, ut, v);
            }
;
            return (ref op, int(ut.encIndir)); // encIndir: op will get called with p == address of receiver.
        }

        // compileEnc returns the engine to compile the type.
        private static ref encEngine compileEnc(ref userTypeInfo ut, map<ref typeInfo, bool> building)
        {
            var srt = ut.@base;
            ptr<encEngine> engine = @new<encEngine>();
            var seen = make_map<reflect.Type, ref encOp>();
            var rt = ut.@base;
            if (ut.externalEnc != 0L)
            {
                rt = ut.user;
            }
            if (ut.externalEnc == 0L && srt.Kind() == reflect.Struct)
            {
                for (long fieldNum = 0L;
                long wireFieldNum = 0L; fieldNum < srt.NumField(); fieldNum++)
                {
                    var f = srt.Field(fieldNum);
                    if (!isSent(ref f))
                    {
                        continue;
                    }
                    var (op, indir) = encOpFor(f.Type, seen, building);
                    engine.instr = append(engine.instr, new encInstr(*op,wireFieldNum,f.Index,indir));
                    wireFieldNum++;
                }
            else

                if (srt.NumField() > 0L && len(engine.instr) == 0L)
                {
                    errorf("type %s has no exported fields", rt);
                }
                engine.instr = append(engine.instr, new encInstr(encStructTerminator,0,nil,0));
            }            {
                engine.instr = make_slice<encInstr>(1L);
                (op, indir) = encOpFor(rt, seen, building);
                engine.instr[0L] = new encInstr(*op,singletonField,nil,indir);
            }
            return engine;
        }

        // getEncEngine returns the engine to compile the type.
        private static ref encEngine getEncEngine(ref userTypeInfo ut, map<ref typeInfo, bool> building)
        {
            var (info, err) = getTypeInfo(ut);
            if (err != null)
            {
                error_(err);
            }
            ref encEngine (enc, ok) = info.encoder.Load()._<ref encEngine>();
            if (!ok)
            {
                enc = buildEncEngine(info, ut, building);
            }
            return enc;
        }

        private static ref encEngine buildEncEngine(ref typeInfo _info, ref userTypeInfo _ut, map<ref typeInfo, bool> building) => func(_info, _ut, (ref typeInfo info, ref userTypeInfo ut, Defer defer, Panic _, Recover __) =>
        { 
            // Check for recursive types.
            if (building != null && building[info])
            {
                return null;
            }
            info.encInit.Lock();
            defer(info.encInit.Unlock());
            ref encEngine (enc, ok) = info.encoder.Load()._<ref encEngine>();
            if (!ok)
            {
                if (building == null)
                {
                    building = make_map<ref typeInfo, bool>();
                }
                building[info] = true;
                enc = compileEnc(ut, building);
                info.encoder.Store(enc);
            }
            return enc;
        });

        private static void encode(this ref Encoder _enc, ref encBuffer _b, reflect.Value value, ref userTypeInfo _ut) => func(_enc, _b, _ut, (ref Encoder enc, ref encBuffer b, ref userTypeInfo ut, Defer defer, Panic _, Recover __) =>
        {
            defer(catchError(ref enc.err));
            var engine = getEncEngine(ut, null);
            var indir = ut.indir;
            if (ut.externalEnc != 0L)
            {
                indir = int(ut.encIndir);
            }
            for (long i = 0L; i < indir; i++)
            {
                value = reflect.Indirect(value);
            }

            if (ut.externalEnc == 0L && value.Type().Kind() == reflect.Struct)
            {
                enc.encodeStruct(b, engine, value);
            }
            else
            {
                enc.encodeSingle(b, engine, value);
            }
        });
    }
}}
