// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run encgen.go -output enc_helpers.go

// package gob -- go2cs converted at 2022 March 06 22:25:07 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Program Files\Go\src\encoding\gob\encode.go
using encoding = go.encoding_package;
using binary = go.encoding.binary_package;
using math = go.math_package;
using bits = go.math.bits_package;
using reflect = go.reflect_package;
using sync = go.sync_package;
using System;


namespace go.encoding;

public static partial class gob_package {

private static readonly nint uint64Size = 8;



public delegate  bool encHelper(ptr<encoderState>,  reflect.Value);

// encoderState is the global execution state of an instance of the encoder.
// Field numbers are delta encoded and always increase. The field
// number is initialized to -1 so 0 comes out as delta(1). A delta of
// 0 terminates the structure.
private partial struct encoderState {
    public ptr<Encoder> enc;
    public ptr<encBuffer> b;
    public bool sendZero; // encoding an array element or map key/value pair; send zero values
    public nint fieldnum; // the last field number written.
    public array<byte> buf; // buffer used by the encoder; here to avoid allocation.
    public ptr<encoderState> next; // for free list
}

// encBuffer is an extremely simple, fast implementation of a write-only byte buffer.
// It never returns a non-nil error, but Write returns an error value so it matches io.Writer.
private partial struct encBuffer {
    public slice<byte> data;
    public array<byte> scratch;
}

private static sync.Pool encBufferPool = new sync.Pool(New:func()interface{}{e:=new(encBuffer)e.data=e.scratch[0:0]returne},);

private static void writeByte(this ptr<encBuffer> _addr_e, byte c) {
    ref encBuffer e = ref _addr_e.val;

    e.data = append(e.data, c);
}

private static (nint, error) Write(this ptr<encBuffer> _addr_e, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref encBuffer e = ref _addr_e.val;

    e.data = append(e.data, p);
    return (len(p), error.As(null!)!);
}

private static void WriteString(this ptr<encBuffer> _addr_e, @string s) {
    ref encBuffer e = ref _addr_e.val;

    e.data = append(e.data, s);
}

private static nint Len(this ptr<encBuffer> _addr_e) {
    ref encBuffer e = ref _addr_e.val;

    return len(e.data);
}

private static slice<byte> Bytes(this ptr<encBuffer> _addr_e) {
    ref encBuffer e = ref _addr_e.val;

    return e.data;
}

private static void Reset(this ptr<encBuffer> _addr_e) {
    ref encBuffer e = ref _addr_e.val;

    if (len(e.data) >= tooBig) {
        e.data = e.scratch[(int)0..(int)0];
    }
    else
 {
        e.data = e.data[(int)0..(int)0];
    }
}

private static ptr<encoderState> newEncoderState(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b) {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;

    var e = enc.freeList;
    if (e == null) {
        e = @new<encoderState>();
        e.enc = enc;
    }
    else
 {
        enc.freeList = e.next;
    }
    e.sendZero = false;
    e.fieldnum = 0;
    e.b = b;
    if (len(b.data) == 0) {
        b.data = b.scratch[(int)0..(int)0];
    }
    return _addr_e!;

}

private static void freeEncoderState(this ptr<Encoder> _addr_enc, ptr<encoderState> _addr_e) {
    ref Encoder enc = ref _addr_enc.val;
    ref encoderState e = ref _addr_e.val;

    e.next = enc.freeList;
    enc.freeList = e;
}

// Unsigned integers have a two-state encoding. If the number is less
// than 128 (0 through 0x7F), its value is written directly.
// Otherwise the value is written in big-endian byte order preceded
// by the byte length, negated.

// encodeUint writes an encoded unsigned integer to state.b.
private static void encodeUint(this ptr<encoderState> _addr_state, ulong x) {
    ref encoderState state = ref _addr_state.val;

    if (x <= 0x7F) {
        state.b.writeByte(uint8(x));
        return ;
    }
    binary.BigEndian.PutUint64(state.buf[(int)1..], x);
    var bc = bits.LeadingZeros64(x) >> 3; // 8 - bytelen(x)
    state.buf[bc] = uint8(bc - uint64Size); // and then we subtract 8 to get -bytelen(x)

    state.b.Write(state.buf[(int)bc..(int)uint64Size + 1]);

}

// encodeInt writes an encoded signed integer to state.w.
// The low bit of the encoding says whether to bit complement the (other bits of the)
// uint to recover the int.
private static void encodeInt(this ptr<encoderState> _addr_state, long i) {
    ref encoderState state = ref _addr_state.val;

    ulong x = default;
    if (i < 0) {
        x = uint64(~i << 1) | 1;
    }
    else
 {
        x = uint64(i << 1);
    }
    state.encodeUint(x);

}

// encOp is the signature of an encoding operator for a given type.
public delegate void encOp(ptr<encInstr>, ptr<encoderState>, reflect.Value);

// The 'instructions' of the encoding machine
private partial struct encInstr {
    public encOp op;
    public nint field; // field number in input
    public slice<nint> index; // struct index
    public nint indir; // how many pointer indirections to reach the value in the struct
}

// update emits a field number and updates the state to record its value for delta encoding.
// If the instruction pointer is nil, it does nothing
private static void update(this ptr<encoderState> _addr_state, ptr<encInstr> _addr_instr) {
    ref encoderState state = ref _addr_state.val;
    ref encInstr instr = ref _addr_instr.val;

    if (instr != null) {
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
private static reflect.Value encIndirect(reflect.Value pv, nint indir) {
    while (indir > 0) {
        if (pv.IsNil()) {
            break;
        indir--;
        }
        pv = pv.Elem();

    }
    return pv;

}

// encBool encodes the bool referenced by v as an unsigned 0 or 1.
private static void encBool(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var b = v.Bool();
    if (b || state.sendZero) {
        state.update(i);
        if (b) {
            state.encodeUint(1);
        }
        else
 {
            state.encodeUint(0);
        }
    }
}

// encInt encodes the signed integer (int int8 int16 int32 int64) referenced by v.
private static void encInt(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var value = v.Int();
    if (value != 0 || state.sendZero) {
        state.update(i);
        state.encodeInt(value);
    }
}

// encUint encodes the unsigned integer (uint uint8 uint16 uint32 uint64 uintptr) referenced by v.
private static void encUint(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var value = v.Uint();
    if (value != 0 || state.sendZero) {
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
private static ulong floatBits(double f) {
    var u = math.Float64bits(f);
    return bits.ReverseBytes64(u);
}

// encFloat encodes the floating point value (float32 float64) referenced by v.
private static void encFloat(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var f = v.Float();
    if (f != 0 || state.sendZero) {
        var bits = floatBits(f);
        state.update(i);
        state.encodeUint(bits);
    }
}

// encComplex encodes the complex value (complex64 complex128) referenced by v.
// Complex numbers are just a pair of floating-point numbers, real part first.
private static void encComplex(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var c = v.Complex();
    if (c != 0 + 0i || state.sendZero) {
        var rpart = floatBits(real(c));
        var ipart = floatBits(imag(c));
        state.update(i);
        state.encodeUint(rpart);
        state.encodeUint(ipart);
    }
}

// encUint8Array encodes the byte array referenced by v.
// Byte arrays are encoded as an unsigned count followed by the raw bytes.
private static void encUint8Array(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var b = v.Bytes();
    if (len(b) > 0 || state.sendZero) {
        state.update(i);
        state.encodeUint(uint64(len(b)));
        state.b.Write(b);
    }
}

// encString encodes the string referenced by v.
// Strings are encoded as an unsigned count followed by the raw bytes.
private static void encString(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    var s = v.String();
    if (len(s) > 0 || state.sendZero) {
        state.update(i);
        state.encodeUint(uint64(len(s)));
        state.b.WriteString(s);
    }
}

// encStructTerminator encodes the end of an encoded struct
// as delta field number of 0.
private static void encStructTerminator(ptr<encInstr> _addr_i, ptr<encoderState> _addr_state, reflect.Value v) {
    ref encInstr i = ref _addr_i.val;
    ref encoderState state = ref _addr_state.val;

    state.encodeUint(0);
}

// Execution engine

// encEngine an array of instructions indexed by field number of the encoding
// data, typically a struct. It is executed top to bottom, walking the struct.
private partial struct encEngine {
    public slice<encInstr> instr;
}

private static readonly nint singletonField = 0;

// valid reports whether the value is valid and a non-nil pointer.
// (Slices, maps, and chans take care of themselves.)


// valid reports whether the value is valid and a non-nil pointer.
// (Slices, maps, and chans take care of themselves.)
private static bool valid(reflect.Value v) {

    if (v.Kind() == reflect.Invalid) 
        return false;
    else if (v.Kind() == reflect.Ptr) 
        return !v.IsNil();
        return true;

}

// encodeSingle encodes a single top-level non-struct value.
private static void encodeSingle(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, ptr<encEngine> _addr_engine, reflect.Value value) => func((defer, _, _) => {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;
    ref encEngine engine = ref _addr_engine.val;

    var state = enc.newEncoderState(b);
    defer(enc.freeEncoderState(state));
    state.fieldnum = singletonField; 
    // There is no surrounding struct to frame the transmission, so we must
    // generate data even if the item is zero. To do this, set sendZero.
    state.sendZero = true;
    var instr = _addr_engine.instr[singletonField];
    if (instr.indir > 0) {
        value = encIndirect(value, instr.indir);
    }
    if (valid(value)) {
        instr.op(instr, state, value);
    }
});

// encodeStruct encodes a single struct value.
private static void encodeStruct(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, ptr<encEngine> _addr_engine, reflect.Value value) => func((defer, _, _) => {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;
    ref encEngine engine = ref _addr_engine.val;

    if (!valid(value)) {
        return ;
    }
    var state = enc.newEncoderState(b);
    defer(enc.freeEncoderState(state));
    state.fieldnum = -1;
    for (nint i = 0; i < len(engine.instr); i++) {
        var instr = _addr_engine.instr[i];
        if (i >= value.NumField()) { 
            // encStructTerminator
            instr.op(instr, state, new reflect.Value());
            break;

        }
        var field = value.FieldByIndex(instr.index);
        if (instr.indir > 0) {
            field = encIndirect(field, instr.indir); 
            // TODO: Is field guaranteed valid? If so we could avoid this check.
            if (!valid(field)) {
                continue;
            }

        }
        instr.op(instr, state, field);

    }

});

// encodeArray encodes an array.
private static void encodeArray(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, reflect.Value value, encOp op, nint elemIndir, nint length, encHelper helper) => func((defer, _, _) => {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;

    var state = enc.newEncoderState(b);
    defer(enc.freeEncoderState(state));
    state.fieldnum = -1;
    state.sendZero = true;
    state.encodeUint(uint64(length));
    if (helper != null && helper(state, value)) {
        return ;
    }
    for (nint i = 0; i < length; i++) {
        var elem = value.Index(i);
        if (elemIndir > 0) {
            elem = encIndirect(elem, elemIndir); 
            // TODO: Is elem guaranteed valid? If so we could avoid this check.
            if (!valid(elem)) {
                errorf("encodeArray: nil element");
            }

        }
        op(null, state, elem);

    }

});

// encodeReflectValue is a helper for maps. It encodes the value v.
private static void encodeReflectValue(ptr<encoderState> _addr_state, reflect.Value v, encOp op, nint indir) {
    ref encoderState state = ref _addr_state.val;

    for (nint i = 0; i < indir && v.IsValid(); i++) {
        v = reflect.Indirect(v);
    }
    if (!v.IsValid()) {
        errorf("encodeReflectValue: nil element");
    }
    op(null, state, v);

}

// encodeMap encodes a map as unsigned count followed by key:value pairs.
private static void encodeMap(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, reflect.Value mv, encOp keyOp, encOp elemOp, nint keyIndir, nint elemIndir) {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;

    var state = enc.newEncoderState(b);
    state.fieldnum = -1;
    state.sendZero = true;
    var keys = mv.MapKeys();
    state.encodeUint(uint64(len(keys)));
    foreach (var (_, key) in keys) {
        encodeReflectValue(_addr_state, key, keyOp, keyIndir);
        encodeReflectValue(_addr_state, mv.MapIndex(key), elemOp, elemIndir);
    }    enc.freeEncoderState(state);
}

// encodeInterface encodes the interface value iv.
// To send an interface, we send a string identifying the concrete type, followed
// by the type identifier (which might require defining that type right now), followed
// by the concrete value. A nil value gets sent as the empty string for the name,
// followed by no value.
private static void encodeInterface(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, reflect.Value iv) {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;
 
    // Gobs can encode nil interface values but not typed interface
    // values holding nil pointers, since nil pointers point to no value.
    var elem = iv.Elem();
    if (elem.Kind() == reflect.Ptr && elem.IsNil()) {
        errorf("gob: cannot encode nil pointer of type %s inside interface", iv.Elem().Type());
    }
    var state = enc.newEncoderState(b);
    state.fieldnum = -1;
    state.sendZero = true;
    if (iv.IsNil()) {
        state.encodeUint(0);
        return ;
    }
    var ut = userType(iv.Elem().Type());
    var (namei, ok) = concreteTypeToName.Load(ut.@base);
    if (!ok) {
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
    ptr<encBuffer> data = encBufferPool.Get()._<ptr<encBuffer>>();
    data.Write(spaceForLength);
    enc.encode(data, elem, ut);
    if (enc.err != null) {
        error_(enc.err);
    }
    enc.popWriter();
    enc.writeMessage(b, data);
    data.Reset();
    encBufferPool.Put(data);
    if (enc.err != null) {
        error_(enc.err);
    }
    enc.freeEncoderState(state);

}

// isZero reports whether the value is the zero of its type.
private static bool isZero(reflect.Value val) => func((_, panic, _) => {

    if (val.Kind() == reflect.Array) 
        {
            nint i__prev1 = i;

            for (nint i = 0; i < val.Len(); i++) {
                if (!isZero(val.Index(i))) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else if (val.Kind() == reflect.Map || val.Kind() == reflect.Slice || val.Kind() == reflect.String) 
        return val.Len() == 0;
    else if (val.Kind() == reflect.Bool) 
        return !val.Bool();
    else if (val.Kind() == reflect.Complex64 || val.Kind() == reflect.Complex128) 
        return val.Complex() == 0;
    else if (val.Kind() == reflect.Chan || val.Kind() == reflect.Func || val.Kind() == reflect.Interface || val.Kind() == reflect.Ptr) 
        return val.IsNil();
    else if (val.Kind() == reflect.Int || val.Kind() == reflect.Int8 || val.Kind() == reflect.Int16 || val.Kind() == reflect.Int32 || val.Kind() == reflect.Int64) 
        return val.Int() == 0;
    else if (val.Kind() == reflect.Float32 || val.Kind() == reflect.Float64) 
        return val.Float() == 0;
    else if (val.Kind() == reflect.Uint || val.Kind() == reflect.Uint8 || val.Kind() == reflect.Uint16 || val.Kind() == reflect.Uint32 || val.Kind() == reflect.Uint64 || val.Kind() == reflect.Uintptr) 
        return val.Uint() == 0;
    else if (val.Kind() == reflect.Struct) 
        {
            nint i__prev1 = i;

            for (i = 0; i < val.NumField(); i++) {
                if (!isZero(val.Field(i))) {
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
private static void encodeGobEncoder(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, ptr<userTypeInfo> _addr_ut, reflect.Value v) {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;
    ref userTypeInfo ut = ref _addr_ut.val;
 
    // TODO: should we catch panics from the called method?

    slice<byte> data = default;
    error err = default!; 
    // We know it's one of these.

    if (ut.externalEnc == xGob) 
        data, err = v.Interface()._<GobEncoder>().GobEncode();
    else if (ut.externalEnc == xBinary) 
        data, err = v.Interface()._<encoding.BinaryMarshaler>().MarshalBinary();
    else if (ut.externalEnc == xText) 
        data, err = v.Interface()._<encoding.TextMarshaler>().MarshalText();
        if (err != null) {
        error_(err);
    }
    var state = enc.newEncoderState(b);
    state.fieldnum = -1;
    state.encodeUint(uint64(len(data)));
    state.b.Write(data);
    enc.freeEncoderState(state);

}

private static array<encOp> encOpTable = new array<encOp>(InitKeyedValues<encOp>((reflect.Bool, encBool), (reflect.Int, encInt), (reflect.Int8, encInt), (reflect.Int16, encInt), (reflect.Int32, encInt), (reflect.Int64, encInt), (reflect.Uint, encUint), (reflect.Uint8, encUint), (reflect.Uint16, encUint), (reflect.Uint32, encUint), (reflect.Uint64, encUint), (reflect.Uintptr, encUint), (reflect.Float32, encFloat), (reflect.Float64, encFloat), (reflect.Complex64, encComplex), (reflect.Complex128, encComplex), (reflect.String, encString)));

// encOpFor returns (a pointer to) the encoding op for the base type under rt and
// the indirection count to reach it.
private static (ptr<encOp>, nint) encOpFor(reflect.Type rt, map<reflect.Type, ptr<encOp>> inProgress, map<ptr<typeInfo>, bool> building) {
    ptr<encOp> _p0 = default!;
    nint _p0 = default;

    var ut = userType(rt); 
    // If the type implements GobEncoder, we handle it without further processing.
    if (ut.externalEnc != 0) {
        return _addr_gobEncodeOpFor(_addr_ut)!;
    }
    {
        var opPtr = inProgress[rt];

        if (opPtr != null) {
            return (_addr_opPtr!, ut.indir);
        }
    }

    var typ = ut.@base;
    var indir = ut.indir;
    var k = typ.Kind();
    ref encOp op = ref heap(out ptr<encOp> _addr_op);
    if (int(k) < len(encOpTable)) {
        op = encOpTable[k];
    }
    if (op == null) {
        _addr_inProgress[rt] = _addr_op;
        inProgress[rt] = ref _addr_inProgress[rt].val; 
        // Special cases
        {
            var t = typ;


            if (t.Kind() == reflect.Slice) 
                if (t.Elem().Kind() == reflect.Uint8) {
                    op = encUint8Array;
                    break;
                } 
                // Slices have a header; we decode it to find the underlying array.
                var (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                var helper = encSliceHelper[t.Elem().Kind()];
                op = (i, state, slice) => {
                    if (!state.sendZero && slice.Len() == 0) {
                        return ;
                    }
                    state.update(i);
                    state.enc.encodeArray(state.b, slice, elemOp.val, elemIndir, slice.Len(), helper);
                }
;
            else if (t.Kind() == reflect.Array) 
                // True arrays have size in the type.
                (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                helper = encArrayHelper[t.Elem().Kind()];
                op = (i, state, array) => {
                    state.update(i);
                    state.enc.encodeArray(state.b, array, elemOp.val, elemIndir, array.Len(), helper);
                }
;
            else if (t.Kind() == reflect.Map) 
                var (keyOp, keyIndir) = encOpFor(t.Key(), inProgress, building);
                (elemOp, elemIndir) = encOpFor(t.Elem(), inProgress, building);
                op = (i, state, mv) => { 
                    // We send zero-length (but non-nil) maps because the
                    // receiver might want to use the map.  (Maps don't use append.)
                    if (!state.sendZero && mv.IsNil()) {
                        return ;
                    }

                    state.update(i);
                    state.enc.encodeMap(state.b, mv, keyOp.val, elemOp.val, keyIndir, elemIndir);

                }
;
            else if (t.Kind() == reflect.Struct) 
                // Generate a closure that calls out to the engine for the nested type.
                getEncEngine(_addr_userType(typ), building);
                var info = mustGetTypeInfo(typ);
                op = (i, state, sv) => {
                    state.update(i); 
                    // indirect through info to delay evaluation for recursive structs
                    ptr<encEngine> enc = info.encoder.Load()._<ptr<encEngine>>();
                    state.enc.encodeStruct(state.b, enc, sv);

                }
;
            else if (t.Kind() == reflect.Interface) 
                op = (i, state, iv) => {
                    if (!state.sendZero && (!iv.IsValid() || iv.IsNil())) {
                        return ;
                    }
                    state.update(i);
                    state.enc.encodeInterface(state.b, iv);
                }
;

        }

    }
    if (op == null) {
        errorf("can't happen: encode type %s", rt);
    }
    return (_addr__addr_op!, indir);

}

// gobEncodeOpFor returns the op for a type that is known to implement GobEncoder.
private static (ptr<encOp>, nint) gobEncodeOpFor(ptr<userTypeInfo> _addr_ut) {
    ptr<encOp> _p0 = default!;
    nint _p0 = default;
    ref userTypeInfo ut = ref _addr_ut.val;

    var rt = ut.user;
    if (ut.encIndir == -1) {
        rt = reflect.PtrTo(rt);
    }
    else if (ut.encIndir > 0) {
        for (var i = int8(0); i < ut.encIndir; i++) {
            rt = rt.Elem();
        }
    }
    ref encOp op = ref heap(out ptr<encOp> _addr_op);
    op = (i, state, v) => {
        if (ut.encIndir == -1) { 
            // Need to climb up one level to turn value into pointer.
            if (!v.CanAddr()) {
                errorf("unaddressable value of type %s", rt);
            }

            v = v.Addr();

        }
        if (!state.sendZero && isZero(v)) {
            return ;
        }
        state.update(i);
        state.enc.encodeGobEncoder(state.b, ut, v);

    };
    return (_addr__addr_op!, int(ut.encIndir)); // encIndir: op will get called with p == address of receiver.
}

// compileEnc returns the engine to compile the type.
private static ptr<encEngine> compileEnc(ptr<userTypeInfo> _addr_ut, map<ptr<typeInfo>, bool> building) {
    ref userTypeInfo ut = ref _addr_ut.val;

    var srt = ut.@base;
    ptr<encEngine> engine = @new<encEngine>();
    var seen = make_map<reflect.Type, ptr<encOp>>();
    var rt = ut.@base;
    if (ut.externalEnc != 0) {
        rt = ut.user;
    }
    if (ut.externalEnc == 0 && srt.Kind() == reflect.Struct) {
        for (nint fieldNum = 0;
        nint wireFieldNum = 0; fieldNum < srt.NumField(); fieldNum++) {
            ref var f = ref heap(srt.Field(fieldNum), out ptr<var> _addr_f);
            if (!isSent(_addr_f)) {
                continue;
            }
            var (op, indir) = encOpFor(f.Type, seen, building);
            engine.instr = append(engine.instr, new encInstr(*op,wireFieldNum,f.Index,indir));
            wireFieldNum++;
        }
    else

        if (srt.NumField() > 0 && len(engine.instr) == 0) {
            errorf("type %s has no exported fields", rt);
        }
        engine.instr = append(engine.instr, new encInstr(encStructTerminator,0,nil,0));

    } {
        engine.instr = make_slice<encInstr>(1);
        (op, indir) = encOpFor(rt, seen, building);
        engine.instr[0] = new encInstr(*op,singletonField,nil,indir);
    }
    return _addr_engine!;

}

// getEncEngine returns the engine to compile the type.
private static ptr<encEngine> getEncEngine(ptr<userTypeInfo> _addr_ut, map<ptr<typeInfo>, bool> building) {
    ref userTypeInfo ut = ref _addr_ut.val;

    var (info, err) = getTypeInfo(ut);
    if (err != null) {
        error_(err);
    }
    ptr<encEngine> (enc, ok) = info.encoder.Load()._<ptr<encEngine>>();
    if (!ok) {
        enc = buildEncEngine(_addr_info, _addr_ut, building);
    }
    return _addr_enc!;

}

private static ptr<encEngine> buildEncEngine(ptr<typeInfo> _addr_info, ptr<userTypeInfo> _addr_ut, map<ptr<typeInfo>, bool> building) => func((defer, _, _) => {
    ref typeInfo info = ref _addr_info.val;
    ref userTypeInfo ut = ref _addr_ut.val;
 
    // Check for recursive types.
    if (building != null && building[info]) {
        return _addr_null!;
    }
    info.encInit.Lock();
    defer(info.encInit.Unlock());
    ptr<encEngine> (enc, ok) = info.encoder.Load()._<ptr<encEngine>>();
    if (!ok) {
        if (building == null) {
            building = make_map<ptr<typeInfo>, bool>();
        }
        building[info] = true;
        enc = compileEnc(_addr_ut, building);
        info.encoder.Store(enc);

    }
    return _addr_enc!;

});

private static void encode(this ptr<Encoder> _addr_enc, ptr<encBuffer> _addr_b, reflect.Value value, ptr<userTypeInfo> _addr_ut) => func((defer, _, _) => {
    ref Encoder enc = ref _addr_enc.val;
    ref encBuffer b = ref _addr_b.val;
    ref userTypeInfo ut = ref _addr_ut.val;

    defer(catchError(_addr_enc.err));
    var engine = getEncEngine(_addr_ut, null);
    var indir = ut.indir;
    if (ut.externalEnc != 0) {
        indir = int(ut.encIndir);
    }
    for (nint i = 0; i < indir; i++) {
        value = reflect.Indirect(value);
    }
    if (ut.externalEnc == 0 && value.Type().Kind() == reflect.Struct) {
        enc.encodeStruct(b, engine, value);
    }
    else
 {
        enc.encodeSingle(b, engine, value);
    }
});

} // end gob_package
