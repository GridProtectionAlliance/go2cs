// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run decgen.go -output dec_helpers.go

// package gob -- go2cs converted at 2020 October 08 03:42:35 UTC
// import "encoding/gob" ==> using gob = go.encoding.gob_package
// Original source: C:\Go\src\encoding\gob\decode.go
using encoding = go.encoding_package;
using errors = go.errors_package;
using io = go.io_package;
using math = go.math_package;
using bits = go.math.bits_package;
using reflect = go.reflect_package;
using static go.builtin;
using System;

namespace go {
namespace encoding
{
    public static partial class gob_package
    {
        private static var errBadUint = errors.New("gob: encoded unsigned integer out of range");        private static var errBadType = errors.New("gob: unknown type id or corrupted data");        private static var errRange = errors.New("gob: bad data: field numbers out of bounds");

        public delegate  bool decHelper(ptr<decoderState>,  reflect.Value,  long,  error);

        // decoderState is the execution state of an instance of the decoder. A new state
        // is created for nested objects.
        private partial struct decoderState
        {
            public ptr<Decoder> dec; // The buffer is stored with an extra indirection because it may be replaced
// if we load a type during decode (when reading an interface value).
            public ptr<decBuffer> b;
            public long fieldnum; // the last field number read.
            public ptr<decoderState> next; // for free list
        }

        // decBuffer is an extremely simple, fast implementation of a read-only byte buffer.
        // It is initialized by calling Size and then copying the data into the slice returned by Bytes().
        private partial struct decBuffer
        {
            public slice<byte> data;
            public long offset; // Read offset.
        }

        private static (long, error) Read(this ptr<decBuffer> _addr_d, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref decBuffer d = ref _addr_d.val;

            var n = copy(p, d.data[d.offset..]);
            if (n == 0L && len(p) != 0L)
            {
                return (0L, error.As(io.EOF)!);
            }

            d.offset += n;
            return (n, error.As(null!)!);

        }

        private static void Drop(this ptr<decBuffer> _addr_d, long n) => func((_, panic, __) =>
        {
            ref decBuffer d = ref _addr_d.val;

            if (n > d.Len())
            {
                panic("drop");
            }

            d.offset += n;

        });

        // Size grows the buffer to exactly n bytes, so d.Bytes() will
        // return a slice of length n. Existing data is first discarded.
        private static void Size(this ptr<decBuffer> _addr_d, long n)
        {
            ref decBuffer d = ref _addr_d.val;

            d.Reset();
            if (cap(d.data) < n)
            {
                d.data = make_slice<byte>(n);
            }
            else
            {
                d.data = d.data[0L..n];
            }

        }

        private static (byte, error) ReadByte(this ptr<decBuffer> _addr_d)
        {
            byte _p0 = default;
            error _p0 = default!;
            ref decBuffer d = ref _addr_d.val;

            if (d.offset >= len(d.data))
            {
                return (0L, error.As(io.EOF)!);
            }

            var c = d.data[d.offset];
            d.offset++;
            return (c, error.As(null!)!);

        }

        private static long Len(this ptr<decBuffer> _addr_d)
        {
            ref decBuffer d = ref _addr_d.val;

            return len(d.data) - d.offset;
        }

        private static slice<byte> Bytes(this ptr<decBuffer> _addr_d)
        {
            ref decBuffer d = ref _addr_d.val;

            return d.data[d.offset..];
        }

        private static void Reset(this ptr<decBuffer> _addr_d)
        {
            ref decBuffer d = ref _addr_d.val;

            d.data = d.data[0L..0L];
            d.offset = 0L;
        }

        // We pass the bytes.Buffer separately for easier testing of the infrastructure
        // without requiring a full Decoder.
        private static ptr<decoderState> newDecoderState(this ptr<Decoder> _addr_dec, ptr<decBuffer> _addr_buf)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decBuffer buf = ref _addr_buf.val;

            var d = dec.freeList;
            if (d == null)
            {
                d = @new<decoderState>();
                d.dec = dec;
            }
            else
            {
                dec.freeList = d.next;
            }

            d.b = buf;
            return _addr_d!;

        }

        private static void freeDecoderState(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_d)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState d = ref _addr_d.val;

            d.next = dec.freeList;
            dec.freeList = d;
        }

        private static error overflow(@string name)
        {
            return error.As(errors.New("value for \"" + name + "\" out of range"))!;
        }

        // decodeUintReader reads an encoded unsigned integer from an io.Reader.
        // Used only by the Decoder to read the message length.
        private static (ulong, long, error) decodeUintReader(io.Reader r, slice<byte> buf)
        {
            ulong x = default;
            long width = default;
            error err = default!;

            width = 1L;
            var (n, err) = io.ReadFull(r, buf[0L..width]);
            if (n == 0L)
            {
                return ;
            }

            var b = buf[0L];
            if (b <= 0x7fUL)
            {
                return (uint64(b), width, error.As(null!)!);
            }

            n = -int(int8(b));
            if (n > uint64Size)
            {
                err = errBadUint;
                return ;
            }

            width, err = io.ReadFull(r, buf[0L..n]);
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = io.ErrUnexpectedEOF;
                }

                return ;

            } 
            // Could check that the high byte is zero but it's not worth it.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in buf[0L..width])
                {
                    b = __b;
                    x = x << (int)(8L) | uint64(b);
                }

                b = b__prev1;
            }

            width++; // +1 for length byte
            return ;

        }

        // decodeUint reads an encoded unsigned integer from state.r.
        // Does not check for overflow.
        private static ulong decodeUint(this ptr<decoderState> _addr_state)
        {
            ulong x = default;
            ref decoderState state = ref _addr_state.val;

            var (b, err) = state.b.ReadByte();
            if (err != null)
            {
                error_(err);
            }

            if (b <= 0x7fUL)
            {
                return uint64(b);
            }

            var n = -int(int8(b));
            if (n > uint64Size)
            {
                error_(errBadUint);
            }

            var buf = state.b.Bytes();
            if (len(buf) < n)
            {
                errorf("invalid uint data length %d: exceeds input size %d", n, len(buf));
            } 
            // Don't need to check error; it's safe to loop regardless.
            // Could check that the high byte is zero but it's not worth it.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in buf[0L..n])
                {
                    b = __b;
                    x = x << (int)(8L) | uint64(b);
                }

                b = b__prev1;
            }

            state.b.Drop(n);
            return x;

        }

        // decodeInt reads an encoded signed integer from state.r.
        // Does not check for overflow.
        private static long decodeInt(this ptr<decoderState> _addr_state)
        {
            ref decoderState state = ref _addr_state.val;

            var x = state.decodeUint();
            if (x & 1L != 0L)
            {
                return ~int64(x >> (int)(1L));
            }

            return int64(x >> (int)(1L));

        }

        // getLength decodes the next uint and makes sure it is a possible
        // size for a data item that follows, which means it must fit in a
        // non-negative int and fit in the buffer.
        private static (long, bool) getLength(this ptr<decoderState> _addr_state)
        {
            long _p0 = default;
            bool _p0 = default;
            ref decoderState state = ref _addr_state.val;

            var n = int(state.decodeUint());
            if (n < 0L || state.b.Len() < n || tooBig <= n)
            {
                return (0L, false);
            }

            return (n, true);

        }

        // decOp is the signature of a decoding operator for a given type.
        public delegate void decOp(ptr<decInstr>, ptr<decoderState>, reflect.Value);

        // The 'instructions' of the decoding machine
        private partial struct decInstr
        {
            public decOp op;
            public long field; // field number of the wire type
            public slice<long> index; // field access indices for destination type
            public error ovfl; // error message for overflow/underflow (for arrays, of the elements)
        }

        // ignoreUint discards a uint value with no destination.
        private static void ignoreUint(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value v)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            state.decodeUint();
        }

        // ignoreTwoUints discards a uint value with no destination. It's used to skip
        // complex values.
        private static void ignoreTwoUints(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value v)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            state.decodeUint();
            state.decodeUint();
        }

        // Since the encoder writes no zeros, if we arrive at a decoder we have
        // a value to extract and store. The field number has already been read
        // (it's how we knew to call this decoder).
        // Each decoder is responsible for handling any indirections associated
        // with the data structure. If any pointer so reached is nil, allocation must
        // be done.

        // decAlloc takes a value and returns a settable value that can
        // be assigned to. If the value is a pointer, decAlloc guarantees it points to storage.
        // The callers to the individual decoders are expected to have used decAlloc.
        // The individual decoders don't need to it.
        private static reflect.Value decAlloc(reflect.Value v)
        {
            while (v.Kind() == reflect.Ptr)
            {
                if (v.IsNil())
                {
                    v.Set(reflect.New(v.Type().Elem()));
                }

                v = v.Elem();

            }

            return v;

        }

        // decBool decodes a uint and stores it as a boolean in value.
        private static void decBool(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            value.SetBool(state.decodeUint() != 0L);
        }

        // decInt8 decodes an integer and stores it as an int8 in value.
        private static void decInt8(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeInt();
            if (v < math.MinInt8 || math.MaxInt8 < v)
            {
                error_(i.ovfl);
            }

            value.SetInt(v);

        }

        // decUint8 decodes an unsigned integer and stores it as a uint8 in value.
        private static void decUint8(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeUint();
            if (math.MaxUint8 < v)
            {
                error_(i.ovfl);
            }

            value.SetUint(v);

        }

        // decInt16 decodes an integer and stores it as an int16 in value.
        private static void decInt16(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeInt();
            if (v < math.MinInt16 || math.MaxInt16 < v)
            {
                error_(i.ovfl);
            }

            value.SetInt(v);

        }

        // decUint16 decodes an unsigned integer and stores it as a uint16 in value.
        private static void decUint16(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeUint();
            if (math.MaxUint16 < v)
            {
                error_(i.ovfl);
            }

            value.SetUint(v);

        }

        // decInt32 decodes an integer and stores it as an int32 in value.
        private static void decInt32(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeInt();
            if (v < math.MinInt32 || math.MaxInt32 < v)
            {
                error_(i.ovfl);
            }

            value.SetInt(v);

        }

        // decUint32 decodes an unsigned integer and stores it as a uint32 in value.
        private static void decUint32(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeUint();
            if (math.MaxUint32 < v)
            {
                error_(i.ovfl);
            }

            value.SetUint(v);

        }

        // decInt64 decodes an integer and stores it as an int64 in value.
        private static void decInt64(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeInt();
            value.SetInt(v);
        }

        // decUint64 decodes an unsigned integer and stores it as a uint64 in value.
        private static void decUint64(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var v = state.decodeUint();
            value.SetUint(v);
        }

        // Floating-point numbers are transmitted as uint64s holding the bits
        // of the underlying representation. They are sent byte-reversed, with
        // the exponent end coming out first, so integer floating point numbers
        // (for example) transmit more compactly. This routine does the
        // unswizzling.
        private static double float64FromBits(ulong u)
        {
            var v = bits.ReverseBytes64(u);
            return math.Float64frombits(v);
        }

        // float32FromBits decodes an unsigned integer, treats it as a 32-bit floating-point
        // number, and returns it. It's a helper function for float32 and complex64.
        // It returns a float64 because that's what reflection needs, but its return
        // value is known to be accurately representable in a float32.
        private static double float32FromBits(ulong u, error ovfl)
        {
            var v = float64FromBits(u);
            var av = v;
            if (av < 0L)
            {
                av = -av;
            } 
            // +Inf is OK in both 32- and 64-bit floats. Underflow is always OK.
            if (math.MaxFloat32 < av && av <= math.MaxFloat64)
            {
                error_(ovfl);
            }

            return v;

        }

        // decFloat32 decodes an unsigned integer, treats it as a 32-bit floating-point
        // number, and stores it in value.
        private static void decFloat32(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            value.SetFloat(float32FromBits(state.decodeUint(), i.ovfl));
        }

        // decFloat64 decodes an unsigned integer, treats it as a 64-bit floating-point
        // number, and stores it in value.
        private static void decFloat64(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            value.SetFloat(float64FromBits(state.decodeUint()));
        }

        // decComplex64 decodes a pair of unsigned integers, treats them as a
        // pair of floating point numbers, and stores them as a complex64 in value.
        // The real part comes first.
        private static void decComplex64(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var real = float32FromBits(state.decodeUint(), i.ovfl);
            var imag = float32FromBits(state.decodeUint(), i.ovfl);
            value.SetComplex(complex(real, imag));
        }

        // decComplex128 decodes a pair of unsigned integers, treats them as a
        // pair of floating point numbers, and stores them as a complex128 in value.
        // The real part comes first.
        private static void decComplex128(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var real = float64FromBits(state.decodeUint());
            var imag = float64FromBits(state.decodeUint());
            value.SetComplex(complex(real, imag));
        }

        // decUint8Slice decodes a byte slice and stores in value a slice header
        // describing the data.
        // uint8 slices are encoded as an unsigned count followed by the raw bytes.
        private static void decUint8Slice(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var (n, ok) = state.getLength();
            if (!ok)
            {
                errorf("bad %s slice length: %d", value.Type(), n);
            }

            if (value.Cap() < n)
            {
                value.Set(reflect.MakeSlice(value.Type(), n, n));
            }
            else
            {
                value.Set(value.Slice(0L, n));
            }

            {
                var (_, err) = state.b.Read(value.Bytes());

                if (err != null)
                {
                    errorf("error decoding []byte: %s", err);
                }

            }

        }

        // decString decodes byte array and stores in value a string header
        // describing the data.
        // Strings are encoded as an unsigned count followed by the raw bytes.
        private static void decString(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var (n, ok) = state.getLength();
            if (!ok)
            {
                errorf("bad %s slice length: %d", value.Type(), n);
            } 
            // Read the data.
            var data = state.b.Bytes();
            if (len(data) < n)
            {
                errorf("invalid string length %d: exceeds input size %d", n, len(data));
            }

            var s = string(data[..n]);
            state.b.Drop(n);
            value.SetString(s);

        }

        // ignoreUint8Array skips over the data for a byte slice value with no destination.
        private static void ignoreUint8Array(ptr<decInstr> _addr_i, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref decInstr i = ref _addr_i.val;
            ref decoderState state = ref _addr_state.val;

            var (n, ok) = state.getLength();
            if (!ok)
            {
                errorf("slice length too large");
            }

            var bn = state.b.Len();
            if (bn < n)
            {
                errorf("invalid slice length %d: exceeds input size %d", n, bn);
            }

            state.b.Drop(n);

        }

        // Execution engine

        // The encoder engine is an array of instructions indexed by field number of the incoming
        // decoder. It is executed with random access according to field number.
        private partial struct decEngine
        {
            public slice<decInstr> instr;
            public long numInstr; // the number of active instructions
        }

        // decodeSingle decodes a top-level value that is not a struct and stores it in value.
        // Such values are preceded by a zero, making them have the memory layout of a
        // struct field (although with an illegal field number).
        private static void decodeSingle(this ptr<Decoder> _addr_dec, ptr<decEngine> _addr_engine, reflect.Value value) => func((defer, _, __) =>
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decEngine engine = ref _addr_engine.val;

            var state = dec.newDecoderState(_addr_dec.buf);
            defer(dec.freeDecoderState(state));
            state.fieldnum = singletonField;
            if (state.decodeUint() != 0L)
            {
                errorf("decode: corrupted data: non-zero delta for singleton");
            }

            var instr = _addr_engine.instr[singletonField];
            instr.op(instr, state, value);

        });

        // decodeStruct decodes a top-level struct and stores it in value.
        // Indir is for the value, not the type. At the time of the call it may
        // differ from ut.indir, which was computed when the engine was built.
        // This state cannot arise for decodeSingle, which is called directly
        // from the user's value, not from the innards of an engine.
        private static void decodeStruct(this ptr<Decoder> _addr_dec, ptr<decEngine> _addr_engine, reflect.Value value) => func((defer, _, __) =>
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decEngine engine = ref _addr_engine.val;

            var state = dec.newDecoderState(_addr_dec.buf);
            defer(dec.freeDecoderState(state));
            state.fieldnum = -1L;
            while (state.b.Len() > 0L)
            {
                var delta = int(state.decodeUint());
                if (delta < 0L)
                {
                    errorf("decode: corrupted data: negative delta");
                }

                if (delta == 0L)
                { // struct terminator is zero delta fieldnum
                    break;

                }

                var fieldnum = state.fieldnum + delta;
                if (fieldnum >= len(engine.instr))
                {
                    error_(errRange);
                    break;
                }

                var instr = _addr_engine.instr[fieldnum];
                reflect.Value field = default;
                if (instr.index != null)
                { 
                    // Otherwise the field is unknown to us and instr.op is an ignore op.
                    field = value.FieldByIndex(instr.index);
                    if (field.Kind() == reflect.Ptr)
                    {
                        field = decAlloc(field);
                    }

                }

                instr.op(instr, state, field);
                state.fieldnum = fieldnum;

            }


        });

        private static reflect.Value noValue = default;

        // ignoreStruct discards the data for a struct with no destination.
        private static void ignoreStruct(this ptr<Decoder> _addr_dec, ptr<decEngine> _addr_engine) => func((defer, _, __) =>
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decEngine engine = ref _addr_engine.val;

            var state = dec.newDecoderState(_addr_dec.buf);
            defer(dec.freeDecoderState(state));
            state.fieldnum = -1L;
            while (state.b.Len() > 0L)
            {
                var delta = int(state.decodeUint());
                if (delta < 0L)
                {
                    errorf("ignore decode: corrupted data: negative delta");
                }

                if (delta == 0L)
                { // struct terminator is zero delta fieldnum
                    break;

                }

                var fieldnum = state.fieldnum + delta;
                if (fieldnum >= len(engine.instr))
                {
                    error_(errRange);
                }

                var instr = _addr_engine.instr[fieldnum];
                instr.op(instr, state, noValue);
                state.fieldnum = fieldnum;

            }


        });

        // ignoreSingle discards the data for a top-level non-struct value with no
        // destination. It's used when calling Decode with a nil value.
        private static void ignoreSingle(this ptr<Decoder> _addr_dec, ptr<decEngine> _addr_engine) => func((defer, _, __) =>
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decEngine engine = ref _addr_engine.val;

            var state = dec.newDecoderState(_addr_dec.buf);
            defer(dec.freeDecoderState(state));
            state.fieldnum = singletonField;
            var delta = int(state.decodeUint());
            if (delta != 0L)
            {
                errorf("decode: corrupted data: non-zero delta for singleton");
            }

            var instr = _addr_engine.instr[singletonField];
            instr.op(instr, state, noValue);

        });

        // decodeArrayHelper does the work for decoding arrays and slices.
        private static void decodeArrayHelper(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, reflect.Value value, decOp elemOp, long length, error ovfl, decHelper helper)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            if (helper != null && helper(state, value, length, ovfl))
            {
                return ;
            }

            ptr<decInstr> instr = addr(new decInstr(elemOp,0,nil,ovfl));
            var isPtr = value.Type().Elem().Kind() == reflect.Ptr;
            for (long i = 0L; i < length; i++)
            {
                if (state.b.Len() == 0L)
                {
                    errorf("decoding array or slice: length exceeds input size (%d elements)", length);
                }

                var v = value.Index(i);
                if (isPtr)
                {
                    v = decAlloc(v);
                }

                elemOp(instr, state, v);

            }


        }

        // decodeArray decodes an array and stores it in value.
        // The length is an unsigned integer preceding the elements. Even though the length is redundant
        // (it's part of the type), it's a useful check and is included in the encoding.
        private static void decodeArray(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, reflect.Value value, decOp elemOp, long length, error ovfl, decHelper helper)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            {
                var n = state.decodeUint();

                if (n != uint64(length))
                {
                    errorf("length mismatch in decodeArray");
                }

            }

            dec.decodeArrayHelper(state, value, elemOp, length, ovfl, helper);

        }

        // decodeIntoValue is a helper for map decoding.
        private static reflect.Value decodeIntoValue(ptr<decoderState> _addr_state, decOp op, bool isPtr, reflect.Value value, ptr<decInstr> _addr_instr)
        {
            ref decoderState state = ref _addr_state.val;
            ref decInstr instr = ref _addr_instr.val;

            var v = value;
            if (isPtr)
            {
                v = decAlloc(value);
            }

            op(instr, state, v);
            return value;

        }

        // decodeMap decodes a map and stores it in value.
        // Maps are encoded as a length followed by key:value pairs.
        // Because the internals of maps are not visible to us, we must
        // use reflection rather than pointer magic.
        private static void decodeMap(this ptr<Decoder> _addr_dec, reflect.Type mtyp, ptr<decoderState> _addr_state, reflect.Value value, decOp keyOp, decOp elemOp, error ovfl)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            var n = int(state.decodeUint());
            if (value.IsNil())
            {
                value.Set(reflect.MakeMapWithSize(mtyp, n));
            }

            var keyIsPtr = mtyp.Key().Kind() == reflect.Ptr;
            var elemIsPtr = mtyp.Elem().Kind() == reflect.Ptr;
            ptr<decInstr> keyInstr = addr(new decInstr(keyOp,0,nil,ovfl));
            ptr<decInstr> elemInstr = addr(new decInstr(elemOp,0,nil,ovfl));
            var keyP = reflect.New(mtyp.Key());
            var keyZ = reflect.Zero(mtyp.Key());
            var elemP = reflect.New(mtyp.Elem());
            var elemZ = reflect.Zero(mtyp.Elem());
            for (long i = 0L; i < n; i++)
            {
                var key = decodeIntoValue(_addr_state, keyOp, keyIsPtr, keyP.Elem(), _addr_keyInstr);
                var elem = decodeIntoValue(_addr_state, elemOp, elemIsPtr, elemP.Elem(), _addr_elemInstr);
                value.SetMapIndex(key, elem);
                keyP.Elem().Set(keyZ);
                elemP.Elem().Set(elemZ);
            }


        }

        // ignoreArrayHelper does the work for discarding arrays and slices.
        private static void ignoreArrayHelper(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, decOp elemOp, long length)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            ptr<decInstr> instr = addr(new decInstr(elemOp,0,nil,errors.New("no error")));
            for (long i = 0L; i < length; i++)
            {
                if (state.b.Len() == 0L)
                {
                    errorf("decoding array or slice: length exceeds input size (%d elements)", length);
                }

                elemOp(instr, state, noValue);

            }


        }

        // ignoreArray discards the data for an array value with no destination.
        private static void ignoreArray(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, decOp elemOp, long length)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            {
                var n = state.decodeUint();

                if (n != uint64(length))
                {
                    errorf("length mismatch in ignoreArray");
                }

            }

            dec.ignoreArrayHelper(state, elemOp, length);

        }

        // ignoreMap discards the data for a map value with no destination.
        private static void ignoreMap(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, decOp keyOp, decOp elemOp)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            var n = int(state.decodeUint());
            ptr<decInstr> keyInstr = addr(new decInstr(keyOp,0,nil,errors.New("no error")));
            ptr<decInstr> elemInstr = addr(new decInstr(elemOp,0,nil,errors.New("no error")));
            for (long i = 0L; i < n; i++)
            {
                keyOp(keyInstr, state, noValue);
                elemOp(elemInstr, state, noValue);
            }


        }

        // decodeSlice decodes a slice and stores it in value.
        // Slices are encoded as an unsigned length followed by the elements.
        private static void decodeSlice(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, reflect.Value value, decOp elemOp, error ovfl, decHelper helper)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            var u = state.decodeUint();
            var typ = value.Type();
            var size = uint64(typ.Elem().Size());
            var nBytes = u * size;
            var n = int(u); 
            // Take care with overflow in this calculation.
            if (n < 0L || uint64(n) != u || nBytes > tooBig || (size > 0L && nBytes / size != u))
            { 
                // We don't check n against buffer length here because if it's a slice
                // of interfaces, there will be buffer reloads.
                errorf("%s slice too big: %d elements of %d bytes", typ.Elem(), u, size);

            }

            if (value.Cap() < n)
            {
                value.Set(reflect.MakeSlice(typ, n, n));
            }
            else
            {
                value.Set(value.Slice(0L, n));
            }

            dec.decodeArrayHelper(state, value, elemOp, n, ovfl, helper);

        }

        // ignoreSlice skips over the data for a slice value with no destination.
        private static void ignoreSlice(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state, decOp elemOp)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;

            dec.ignoreArrayHelper(state, elemOp, int(state.decodeUint()));
        }

        // decodeInterface decodes an interface value and stores it in value.
        // Interfaces are encoded as the name of a concrete type followed by a value.
        // If the name is empty, the value is nil and no value is sent.
        private static void decodeInterface(this ptr<Decoder> _addr_dec, reflect.Type ityp, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;
 
            // Read the name of the concrete type.
            var nr = state.decodeUint();
            if (nr > 1L << (int)(31L))
            { // zero is permissible for anonymous types
                errorf("invalid type name length %d", nr);

            }

            if (nr > uint64(state.b.Len()))
            {
                errorf("invalid type name length %d: exceeds input size", nr);
            }

            var n = int(nr);
            var name = state.b.Bytes()[..n];
            state.b.Drop(n); 
            // Allocate the destination interface value.
            if (len(name) == 0L)
            { 
                // Copy the nil interface value to the target.
                value.Set(reflect.Zero(value.Type()));
                return ;

            }

            if (len(name) > 1024L)
            {
                errorf("name too long (%d bytes): %.20q...", len(name), name);
            } 
            // The concrete type must be registered.
            var (typi, ok) = nameToConcreteType.Load(string(name));
            if (!ok)
            {
                errorf("name not registered for interface: %q", name);
            }

            reflect.Type typ = typi._<reflect.Type>(); 

            // Read the type id of the concrete value.
            var concreteId = dec.decodeTypeSequence(true);
            if (concreteId < 0L)
            {
                error_(dec.err);
            } 
            // Byte count of value is next; we don't care what it is (it's there
            // in case we want to ignore the value by skipping it completely).
            state.decodeUint(); 
            // Read the concrete value.
            var v = allocValue(typ);
            dec.decodeValue(concreteId, v);
            if (dec.err != null)
            {
                error_(dec.err);
            } 
            // Assign the concrete value to the interface.
            // Tread carefully; it might not satisfy the interface.
            if (!typ.AssignableTo(ityp))
            {
                errorf("%s is not assignable to type %s", typ, ityp);
            } 
            // Copy the interface value to the target.
            value.Set(v);

        }

        // ignoreInterface discards the data for an interface value with no destination.
        private static void ignoreInterface(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;
 
            // Read the name of the concrete type.
            var (n, ok) = state.getLength();
            if (!ok)
            {
                errorf("bad interface encoding: name too large for buffer");
            }

            var bn = state.b.Len();
            if (bn < n)
            {
                errorf("invalid interface value length %d: exceeds input size %d", n, bn);
            }

            state.b.Drop(n);
            var id = dec.decodeTypeSequence(true);
            if (id < 0L)
            {
                error_(dec.err);
            } 
            // At this point, the decoder buffer contains a delimited value. Just toss it.
            n, ok = state.getLength();
            if (!ok)
            {
                errorf("bad interface encoding: data length too large for buffer");
            }

            state.b.Drop(n);

        }

        // decodeGobDecoder decodes something implementing the GobDecoder interface.
        // The data is encoded as a byte slice.
        private static void decodeGobDecoder(this ptr<Decoder> _addr_dec, ptr<userTypeInfo> _addr_ut, ptr<decoderState> _addr_state, reflect.Value value)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref userTypeInfo ut = ref _addr_ut.val;
            ref decoderState state = ref _addr_state.val;
 
            // Read the bytes for the value.
            var (n, ok) = state.getLength();
            if (!ok)
            {
                errorf("GobDecoder: length too large for buffer");
            }

            var b = state.b.Bytes();
            if (len(b) < n)
            {
                errorf("GobDecoder: invalid data length %d: exceeds input size %d", n, len(b));
            }

            b = b[..n];
            state.b.Drop(n);
            error err = default!; 
            // We know it's one of these.

            if (ut.externalDec == xGob) 
                err = error.As(value.Interface()._<GobDecoder>().GobDecode(b))!;
            else if (ut.externalDec == xBinary) 
                err = error.As(value.Interface()._<encoding.BinaryUnmarshaler>().UnmarshalBinary(b))!;
            else if (ut.externalDec == xText) 
                err = error.As(value.Interface()._<encoding.TextUnmarshaler>().UnmarshalText(b))!;
                        if (err != null)
            {
                error_(err);
            }

        }

        // ignoreGobDecoder discards the data for a GobDecoder value with no destination.
        private static void ignoreGobDecoder(this ptr<Decoder> _addr_dec, ptr<decoderState> _addr_state)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref decoderState state = ref _addr_state.val;
 
            // Read the bytes for the value.
            var (n, ok) = state.getLength();
            if (!ok)
            {
                errorf("GobDecoder: length too large for buffer");
            }

            var bn = state.b.Len();
            if (bn < n)
            {
                errorf("GobDecoder: invalid data length %d: exceeds input size %d", n, bn);
            }

            state.b.Drop(n);

        }

        // Index by Go types.
        private static array<decOp> decOpTable = new array<decOp>(InitKeyedValues<decOp>((reflect.Bool, decBool), (reflect.Int8, decInt8), (reflect.Int16, decInt16), (reflect.Int32, decInt32), (reflect.Int64, decInt64), (reflect.Uint8, decUint8), (reflect.Uint16, decUint16), (reflect.Uint32, decUint32), (reflect.Uint64, decUint64), (reflect.Float32, decFloat32), (reflect.Float64, decFloat64), (reflect.Complex64, decComplex64), (reflect.Complex128, decComplex128), (reflect.String, decString)));

        // Indexed by gob types.  tComplex will be added during type.init().
        private static map decIgnoreOpMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<typeId, decOp>{tBool:ignoreUint,tInt:ignoreUint,tUint:ignoreUint,tFloat:ignoreUint,tBytes:ignoreUint8Array,tString:ignoreUint8Array,tComplex:ignoreTwoUints,};

        // decOpFor returns the decoding op for the base type under rt and
        // the indirection count to reach it.
        private static ptr<decOp> decOpFor(this ptr<Decoder> _addr_dec, typeId wireId, reflect.Type rt, @string name, map<reflect.Type, ptr<decOp>> inProgress)
        {
            ref Decoder dec = ref _addr_dec.val;

            var ut = userType(rt); 
            // If the type implements GobEncoder, we handle it without further processing.
            if (ut.externalDec != 0L)
            {
                return _addr_dec.gobDecodeOpFor(ut)!;
            } 

            // If this type is already in progress, it's a recursive type (e.g. map[string]*T).
            // Return the pointer to the op we're already building.
            {
                var opPtr = inProgress[rt];

                if (opPtr != null)
                {
                    return _addr_opPtr!;
                }

            }

            var typ = ut.@base;
            ref decOp op = ref heap(out ptr<decOp> _addr_op);
            var k = typ.Kind();
            if (int(k) < len(decOpTable))
            {
                op = decOpTable[k];
            }

            if (op == null)
            {
                _addr_inProgress[rt] = _addr_op;
                inProgress[rt] = ref _addr_inProgress[rt].val; 
                // Special cases
                {
                    var t = typ;


                    if (t.Kind() == reflect.Array) 
                        name = "element of " + name;
                        var elemId = dec.wireType[wireId].ArrayT.Elem;
                        var elemOp = dec.decOpFor(elemId, t.Elem(), name, inProgress);
                        var ovfl = overflow(name);
                        var helper = decArrayHelper[t.Elem().Kind()];
                        op = (i, state, value) =>
                        {
                            state.dec.decodeArray(state, value, elemOp.val, t.Len(), ovfl, helper);
                        }
;
                    else if (t.Kind() == reflect.Map) 
                        var keyId = dec.wireType[wireId].MapT.Key;
                        elemId = dec.wireType[wireId].MapT.Elem;
                        var keyOp = dec.decOpFor(keyId, t.Key(), "key of " + name, inProgress);
                        elemOp = dec.decOpFor(elemId, t.Elem(), "element of " + name, inProgress);
                        ovfl = overflow(name);
                        op = (i, state, value) =>
                        {
                            state.dec.decodeMap(t, state, value, keyOp.val, elemOp.val, ovfl);
                        }
;
                    else if (t.Kind() == reflect.Slice) 
                        name = "element of " + name;
                        if (t.Elem().Kind() == reflect.Uint8)
                        {
                            op = decUint8Slice;
                            break;
                        }

                        elemId = default;
                        {
                            var (tt, ok) = builtinIdToType[wireId];

                            if (ok)
                            {
                                elemId = tt._<ptr<sliceType>>().Elem;
                            }
                            else
                            {
                                elemId = dec.wireType[wireId].SliceT.Elem;
                            }

                        }

                        elemOp = dec.decOpFor(elemId, t.Elem(), name, inProgress);
                        ovfl = overflow(name);
                        helper = decSliceHelper[t.Elem().Kind()];
                        op = (i, state, value) =>
                        {
                            state.dec.decodeSlice(state, value, elemOp.val, ovfl, helper);
                        }
;
                    else if (t.Kind() == reflect.Struct) 
                        // Generate a closure that calls out to the engine for the nested type.
                        ut = userType(typ);
                        var (enginePtr, err) = dec.getDecEnginePtr(wireId, ut);
                        if (err != null)
                        {
                            error_(err);
                        }

                        op = (i, state, value) =>
                        { 
                            // indirect through enginePtr to delay evaluation for recursive structs.
                            dec.decodeStruct(enginePtr.val, value);

                        }
;
                    else if (t.Kind() == reflect.Interface) 
                        op = (i, state, value) =>
                        {
                            state.dec.decodeInterface(t, state, value);
                        }
;

                }

            }

            if (op == null)
            {
                errorf("decode can't handle type %s", rt);
            }

            return _addr__addr_op!;

        }

        // decIgnoreOpFor returns the decoding op for a field that has no destination.
        private static ptr<decOp> decIgnoreOpFor(this ptr<Decoder> _addr_dec, typeId wireId, map<typeId, ptr<decOp>> inProgress)
        {
            ref Decoder dec = ref _addr_dec.val;
 
            // If this type is already in progress, it's a recursive type (e.g. map[string]*T).
            // Return the pointer to the op we're already building.
            {
                var opPtr = inProgress[wireId];

                if (opPtr != null)
                {
                    return _addr_opPtr!;
                }

            }

            var (op, ok) = decIgnoreOpMap[wireId];
            if (!ok)
            {
                inProgress[wireId] = _addr_op;
                if (wireId == tInterface)
                { 
                    // Special case because it's a method: the ignored item might
                    // define types and we need to record their state in the decoder.
                    op = (i, state, value) =>
                    {
                        state.dec.ignoreInterface(state);
                    }
;
                    return _addr__addr_op!;

                } 
                // Special cases
                var wire = dec.wireType[wireId];

                if (wire == null) 
                    errorf("bad data: undefined type %s", wireId.@string());
                else if (wire.ArrayT != null) 
                    var elemId = wire.ArrayT.Elem;
                    var elemOp = dec.decIgnoreOpFor(elemId, inProgress);
                    op = (i, state, value) =>
                    {
                        state.dec.ignoreArray(state, elemOp.val, wire.ArrayT.Len);
                    }
;
                else if (wire.MapT != null) 
                    var keyId = dec.wireType[wireId].MapT.Key;
                    elemId = dec.wireType[wireId].MapT.Elem;
                    var keyOp = dec.decIgnoreOpFor(keyId, inProgress);
                    elemOp = dec.decIgnoreOpFor(elemId, inProgress);
                    op = (i, state, value) =>
                    {
                        state.dec.ignoreMap(state, keyOp.val, elemOp.val);
                    }
;
                else if (wire.SliceT != null) 
                    elemId = wire.SliceT.Elem;
                    elemOp = dec.decIgnoreOpFor(elemId, inProgress);
                    op = (i, state, value) =>
                    {
                        state.dec.ignoreSlice(state, elemOp.val);
                    }
;
                else if (wire.StructT != null) 
                    // Generate a closure that calls out to the engine for the nested type.
                    var (enginePtr, err) = dec.getIgnoreEnginePtr(wireId);
                    if (err != null)
                    {
                        error_(err);
                    }

                    op = (i, state, value) =>
                    { 
                        // indirect through enginePtr to delay evaluation for recursive structs
                        state.dec.ignoreStruct(enginePtr.val);

                    }
;
                else if (wire.GobEncoderT != null || wire.BinaryMarshalerT != null || wire.TextMarshalerT != null) 
                    op = (i, state, value) =>
                    {
                        state.dec.ignoreGobDecoder(state);
                    }
;
                
            }

            if (op == null)
            {
                errorf("bad data: ignore can't handle type %s", wireId.@string());
            }

            return _addr__addr_op!;

        }

        // gobDecodeOpFor returns the op for a type that is known to implement
        // GobDecoder.
        private static ptr<decOp> gobDecodeOpFor(this ptr<Decoder> _addr_dec, ptr<userTypeInfo> _addr_ut)
        {
            ref Decoder dec = ref _addr_dec.val;
            ref userTypeInfo ut = ref _addr_ut.val;

            var rcvrType = ut.user;
            if (ut.decIndir == -1L)
            {
                rcvrType = reflect.PtrTo(rcvrType);
            }
            else if (ut.decIndir > 0L)
            {
                for (var i = int8(0L); i < ut.decIndir; i++)
                {
                    rcvrType = rcvrType.Elem();
                }


            }

            ref decOp op = ref heap(out ptr<decOp> _addr_op);
            op = (i, state, value) =>
            { 
                // We now have the base type. We need its address if the receiver is a pointer.
                if (value.Kind() != reflect.Ptr && rcvrType.Kind() == reflect.Ptr)
                {
                    value = value.Addr();
                }

                state.dec.decodeGobDecoder(ut, state, value);

            }
;
            return _addr__addr_op!;

        }

        // compatibleType asks: Are these two gob Types compatible?
        // Answers the question for basic types, arrays, maps and slices, plus
        // GobEncoder/Decoder pairs.
        // Structs are considered ok; fields will be checked later.
        private static bool compatibleType(this ptr<Decoder> _addr_dec, reflect.Type fr, typeId fw, map<reflect.Type, typeId> inProgress)
        {
            ref Decoder dec = ref _addr_dec.val;

            {
                var (rhs, ok) = inProgress[fr];

                if (ok)
                {
                    return rhs == fw;
                }

            }

            inProgress[fr] = fw;
            var ut = userType(fr);
            var (wire, ok) = dec.wireType[fw]; 
            // If wire was encoded with an encoding method, fr must have that method.
            // And if not, it must not.
            // At most one of the booleans in ut is set.
            // We could possibly relax this constraint in the future in order to
            // choose the decoding method using the data in the wireType.
            // The parentheses look odd but are correct.
            if ((ut.externalDec == xGob) != (ok && wire.GobEncoderT != null) || (ut.externalDec == xBinary) != (ok && wire.BinaryMarshalerT != null) || (ut.externalDec == xText) != (ok && wire.TextMarshalerT != null))
            {
                return false;
            }

            if (ut.externalDec != 0L)
            { // This test trumps all others.
                return true;

            }

            {
                var t = ut.@base;


                if (t.Kind() == reflect.Bool) 
                    return fw == tBool;
                else if (t.Kind() == reflect.Int || t.Kind() == reflect.Int8 || t.Kind() == reflect.Int16 || t.Kind() == reflect.Int32 || t.Kind() == reflect.Int64) 
                    return fw == tInt;
                else if (t.Kind() == reflect.Uint || t.Kind() == reflect.Uint8 || t.Kind() == reflect.Uint16 || t.Kind() == reflect.Uint32 || t.Kind() == reflect.Uint64 || t.Kind() == reflect.Uintptr) 
                    return fw == tUint;
                else if (t.Kind() == reflect.Float32 || t.Kind() == reflect.Float64) 
                    return fw == tFloat;
                else if (t.Kind() == reflect.Complex64 || t.Kind() == reflect.Complex128) 
                    return fw == tComplex;
                else if (t.Kind() == reflect.String) 
                    return fw == tString;
                else if (t.Kind() == reflect.Interface) 
                    return fw == tInterface;
                else if (t.Kind() == reflect.Array) 
                    if (!ok || wire.ArrayT == null)
                    {
                        return false;
                    }

                    var array = wire.ArrayT;
                    return t.Len() == array.Len && dec.compatibleType(t.Elem(), array.Elem, inProgress);
                else if (t.Kind() == reflect.Map) 
                    if (!ok || wire.MapT == null)
                    {
                        return false;
                    }

                    var MapType = wire.MapT;
                    return dec.compatibleType(t.Key(), MapType.Key, inProgress) && dec.compatibleType(t.Elem(), MapType.Elem, inProgress);
                else if (t.Kind() == reflect.Slice) 
                    // Is it an array of bytes?
                    if (t.Elem().Kind() == reflect.Uint8)
                    {
                        return fw == tBytes;
                    } 
                    // Extract and compare element types.
                    ptr<sliceType> sw;
                    {
                        var (tt, ok) = builtinIdToType[fw];

                        if (ok)
                        {
                            sw, _ = tt._<ptr<sliceType>>();
                        }
                        else if (wire != null)
                        {
                            sw = wire.SliceT;
                        }


                    }

                    var elem = userType(t.Elem()).@base;
                    return sw != null && dec.compatibleType(elem, sw.Elem, inProgress);
                else if (t.Kind() == reflect.Struct) 
                    return true;
                else 
                    // chan, etc: cannot handle.
                    return false;

            }

        }

        // typeString returns a human-readable description of the type identified by remoteId.
        private static @string typeString(this ptr<Decoder> _addr_dec, typeId remoteId) => func((defer, _, __) =>
        {
            ref Decoder dec = ref _addr_dec.val;

            typeLock.Lock();
            defer(typeLock.Unlock());
            {
                var t = idToType[remoteId];

                if (t != null)
                { 
                    // globally known type.
                    return t.@string();

                }

            }

            return dec.wireType[remoteId].@string();

        });

        // compileSingle compiles the decoder engine for a non-struct top-level value, including
        // GobDecoders.
        private static (ptr<decEngine>, error) compileSingle(this ptr<Decoder> _addr_dec, typeId remoteId, ptr<userTypeInfo> _addr_ut)
        {
            ptr<decEngine> engine = default!;
            error err = default!;
            ref Decoder dec = ref _addr_dec.val;
            ref userTypeInfo ut = ref _addr_ut.val;

            var rt = ut.user;
            engine = @new<decEngine>();
            engine.instr = make_slice<decInstr>(1L); // one item
            var name = rt.String(); // best we can do
            if (!dec.compatibleType(rt, remoteId, make_map<reflect.Type, typeId>()))
            {
                var remoteType = dec.typeString(remoteId); 
                // Common confusing case: local interface type, remote concrete type.
                if (ut.@base.Kind() == reflect.Interface && remoteId != tInterface)
                {
                    return (_addr_null!, error.As(errors.New("gob: local interface type " + name + " can only be decoded from remote interface type; received concrete type " + remoteType))!);
                }

                return (_addr_null!, error.As(errors.New("gob: decoding into local type " + name + ", received remote type " + remoteType))!);

            }

            var op = dec.decOpFor(remoteId, rt, name, make_map<reflect.Type, ptr<decOp>>());
            var ovfl = errors.New("value for \"" + name + "\" out of range");
            engine.instr[singletonField] = new decInstr(*op,singletonField,nil,ovfl);
            engine.numInstr = 1L;
            return ;

        }

        // compileIgnoreSingle compiles the decoder engine for a non-struct top-level value that will be discarded.
        private static ptr<decEngine> compileIgnoreSingle(this ptr<Decoder> _addr_dec, typeId remoteId)
        {
            ref Decoder dec = ref _addr_dec.val;

            ptr<decEngine> engine = @new<decEngine>();
            engine.instr = make_slice<decInstr>(1L); // one item
            var op = dec.decIgnoreOpFor(remoteId, make_map<typeId, ptr<decOp>>());
            var ovfl = overflow(dec.typeString(remoteId));
            engine.instr[0L] = new decInstr(*op,0,nil,ovfl);
            engine.numInstr = 1L;
            return _addr_engine!;

        }

        // compileDec compiles the decoder engine for a value. If the value is not a struct,
        // it calls out to compileSingle.
        private static (ptr<decEngine>, error) compileDec(this ptr<Decoder> _addr_dec, typeId remoteId, ptr<userTypeInfo> _addr_ut) => func((defer, _, __) =>
        {
            ptr<decEngine> engine = default!;
            error err = default!;
            ref Decoder dec = ref _addr_dec.val;
            ref userTypeInfo ut = ref _addr_ut.val;

            defer(catchError(_addr_err));
            var rt = ut.@base;
            var srt = rt;
            if (srt.Kind() != reflect.Struct || ut.externalDec != 0L)
            {
                return _addr_dec.compileSingle(remoteId, ut)!;
            }

            ptr<structType> wireStruct; 
            // Builtin types can come from global pool; the rest must be defined by the decoder.
            // Also we know we're decoding a struct now, so the client must have sent one.
            {
                var (t, ok) = builtinIdToType[remoteId];

                if (ok)
                {
                    wireStruct, _ = t._<ptr<structType>>();
                }
                else
                {
                    var wire = dec.wireType[remoteId];
                    if (wire == null)
                    {
                        error_(errBadType);
                    }

                    wireStruct = wire.StructT;

                }

            }

            if (wireStruct == null)
            {
                errorf("type mismatch in decoder: want struct type %s; got non-struct", rt);
            }

            engine = @new<decEngine>();
            engine.instr = make_slice<decInstr>(len(wireStruct.Field));
            var seen = make_map<reflect.Type, ptr<decOp>>(); 
            // Loop over the fields of the wire type.
            for (long fieldnum = 0L; fieldnum < len(wireStruct.Field); fieldnum++)
            {
                var wireField = wireStruct.Field[fieldnum];
                if (wireField.Name == "")
                {
                    errorf("empty name for remote field of type %s", wireStruct.Name);
                }

                var ovfl = overflow(wireField.Name); 
                // Find the field of the local type with the same name.
                var (localField, present) = srt.FieldByName(wireField.Name); 
                // TODO(r): anonymous names
                if (!present || !isExported(wireField.Name))
                {
                    var op = dec.decIgnoreOpFor(wireField.Id, make_map<typeId, ptr<decOp>>());
                    engine.instr[fieldnum] = new decInstr(*op,fieldnum,nil,ovfl);
                    continue;
                }

                if (!dec.compatibleType(localField.Type, wireField.Id, make_map<reflect.Type, typeId>()))
                {
                    errorf("wrong type (%s) for received field %s.%s", localField.Type, wireStruct.Name, wireField.Name);
                }

                op = dec.decOpFor(wireField.Id, localField.Type, localField.Name, seen);
                engine.instr[fieldnum] = new decInstr(*op,fieldnum,localField.Index,ovfl);
                engine.numInstr++;

            }

            return ;

        });

        // getDecEnginePtr returns the engine for the specified type.
        private static (ptr<ptr<decEngine>>, error) getDecEnginePtr(this ptr<Decoder> _addr_dec, typeId remoteId, ptr<userTypeInfo> _addr_ut)
        {
            ptr<ptr<decEngine>> enginePtr = default!;
            error err = default!;
            ref Decoder dec = ref _addr_dec.val;
            ref userTypeInfo ut = ref _addr_ut.val;

            var rt = ut.user;
            var (decoderMap, ok) = dec.decoderCache[rt];
            if (!ok)
            {
                decoderMap = make_map<typeId, ptr<ptr<decEngine>>>();
                dec.decoderCache[rt] = decoderMap;
            }

            enginePtr, ok = decoderMap[remoteId];

            if (!ok)
            { 
                // To handle recursive types, mark this engine as underway before compiling.
                enginePtr = @new<decEngine.val>();
                decoderMap[remoteId] = enginePtr;
                enginePtr.val, err = dec.compileDec(remoteId, ut);
                if (err != null)
                {
                    delete(decoderMap, remoteId);
                }

            }

            return ;

        }

        // emptyStruct is the type we compile into when ignoring a struct value.
        private partial struct emptyStruct
        {
        }

        private static var emptyStructType = reflect.TypeOf(new emptyStruct());

        // getIgnoreEnginePtr returns the engine for the specified type when the value is to be discarded.
        private static (ptr<ptr<decEngine>>, error) getIgnoreEnginePtr(this ptr<Decoder> _addr_dec, typeId wireId)
        {
            ptr<ptr<decEngine>> enginePtr = default!;
            error err = default!;
            ref Decoder dec = ref _addr_dec.val;

            bool ok = default;
            enginePtr, ok = dec.ignorerCache[wireId];

            if (!ok)
            { 
                // To handle recursive types, mark this engine as underway before compiling.
                enginePtr = @new<decEngine.val>();
                dec.ignorerCache[wireId] = enginePtr;
                var wire = dec.wireType[wireId];
                if (wire != null && wire.StructT != null)
                {
                    enginePtr.val, err = dec.compileDec(wireId, userType(emptyStructType));
                }
                else
                {
                    enginePtr.val = dec.compileIgnoreSingle(wireId);
                }

                if (err != null)
                {
                    delete(dec.ignorerCache, wireId);
                }

            }

            return ;

        }

        // decodeValue decodes the data stream representing a value and stores it in value.
        private static void decodeValue(this ptr<Decoder> _addr_dec, typeId wireId, reflect.Value value) => func((defer, _, __) =>
        {
            ref Decoder dec = ref _addr_dec.val;

            defer(catchError(_addr_dec.err)); 
            // If the value is nil, it means we should just ignore this item.
            if (!value.IsValid())
            {
                dec.decodeIgnoredValue(wireId);
                return ;
            } 
            // Dereference down to the underlying type.
            var ut = userType(value.Type());
            var @base = ut.@base;
            ptr<ptr<decEngine>> enginePtr;
            enginePtr, dec.err = dec.getDecEnginePtr(wireId, ut);
            if (dec.err != null)
            {
                return ;
            }

            value = decAlloc(value);
            var engine = enginePtr.val;
            {
                var st = base;

                if (st.Kind() == reflect.Struct && ut.externalDec == 0L)
                {
                    var wt = dec.wireType[wireId];
                    if (engine.numInstr == 0L && st.NumField() > 0L && wt != null && len(wt.StructT.Field) > 0L)
                    {
                        var name = @base.Name();
                        errorf("type mismatch: no fields matched compiling decoder for %s", name);
                    }

                    dec.decodeStruct(engine, value);

                }
                else
                {
                    dec.decodeSingle(engine, value);
                }

            }

        });

        // decodeIgnoredValue decodes the data stream representing a value of the specified type and discards it.
        private static void decodeIgnoredValue(this ptr<Decoder> _addr_dec, typeId wireId)
        {
            ref Decoder dec = ref _addr_dec.val;

            ptr<ptr<decEngine>> enginePtr;
            enginePtr, dec.err = dec.getIgnoreEnginePtr(wireId);
            if (dec.err != null)
            {
                return ;
            }

            var wire = dec.wireType[wireId];
            if (wire != null && wire.StructT != null)
            {
                dec.ignoreStruct(enginePtr.val);
            }
            else
            {
                dec.ignoreSingle(enginePtr.val);
            }

        }

        private static void init() => func((_, panic, __) =>
        {
            decOp iop = default;            decOp uop = default;

            switch (reflect.TypeOf(int(0L)).Bits())
            {
                case 32L: 
                    iop = decInt32;
                    uop = decUint32;
                    break;
                case 64L: 
                    iop = decInt64;
                    uop = decUint64;
                    break;
                default: 
                    panic("gob: unknown size of int/uint");
                    break;
            }
            decOpTable[reflect.Int] = iop;
            decOpTable[reflect.Uint] = uop; 

            // Finally uintptr
            switch (reflect.TypeOf(uintptr(0L)).Bits())
            {
                case 32L: 
                    uop = decUint32;
                    break;
                case 64L: 
                    uop = decUint64;
                    break;
                default: 
                    panic("gob: unknown size of uintptr");
                    break;
            }
            decOpTable[reflect.Uintptr] = uop;

        });

        // Gob depends on being able to take the address
        // of zeroed Values it creates, so use this wrapper instead
        // of the standard reflect.Zero.
        // Each call allocates once.
        private static reflect.Value allocValue(reflect.Type t)
        {
            return reflect.New(t).Elem();
        }
    }
}}
