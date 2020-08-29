// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements Sizes.

// package types -- go2cs converted at 2020 August 29 08:47:55 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\sizes.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // Sizes defines the sizing functions for package unsafe.
        public partial interface Sizes
        {
            long Alignof(Type T); // Offsetsof returns the offsets of the given struct fields, in bytes.
// Offsetsof must implement the offset guarantees required by the spec.
            long Offsetsof(slice<ref Var> fields); // Sizeof returns the size of a variable of type T.
// Sizeof must implement the size guarantees required by the spec.
            long Sizeof(Type T);
        }

        // StdSizes is a convenience type for creating commonly used Sizes.
        // It makes the following simplifying assumptions:
        //
        //    - The size of explicitly sized basic types (int16, etc.) is the
        //      specified size.
        //    - The size of strings and interfaces is 2*WordSize.
        //    - The size of slices is 3*WordSize.
        //    - The size of an array of n elements corresponds to the size of
        //      a struct of n consecutive fields of the array's element type.
        //      - The size of a struct is the offset of the last field plus that
        //      field's size. As with all element types, if the struct is used
        //      in an array its size must first be aligned to a multiple of the
        //      struct's alignment.
        //    - All other types have size WordSize.
        //    - Arrays and structs are aligned per spec definition; all other
        //      types are naturally aligned with a maximum alignment MaxAlign.
        //
        // *StdSizes implements Sizes.
        //
        public partial struct StdSizes
        {
            public long WordSize; // word size in bytes - must be >= 4 (32bits)
            public long MaxAlign; // maximum alignment in bytes - must be >= 1
        }

        private static long Alignof(this ref StdSizes s, Type T)
        { 
            // For arrays and structs, alignment is defined in terms
            // of alignment of the elements and fields, respectively.
            switch (T.Underlying().type())
            {
                case ref Array t:
                    return s.Alignof(t.elem);
                    break;
                case ref Struct t:
                    var max = int64(1L);
                    foreach (var (_, f) in t.fields)
                    {
                        {
                            var a__prev1 = a;

                            var a = s.Alignof(f.typ);

                            if (a > max)
                            {
                                max = a;
                            }

                            a = a__prev1;

                        }
                    }
                    return max;
                    break;
                case ref Slice t:
                    return s.WordSize;
                    break;
                case ref Interface t:
                    return s.WordSize;
                    break;
                case ref Basic t:
                    if (t.Info() & IsString != 0L)
                    {
                        return s.WordSize;
                    }
                    break;
            }
            a = s.Sizeof(T); // may be 0
            // spec: "For a variable x of any type: unsafe.Alignof(x) is at least 1."
            if (a < 1L)
            {
                return 1L;
            } 
            // complex{64,128} are aligned like [2]float{32,64}.
            if (isComplex(T))
            {
                a /= 2L;
            }
            if (a > s.MaxAlign)
            {
                return s.MaxAlign;
            }
            return a;
        }

        private static slice<long> Offsetsof(this ref StdSizes s, slice<ref Var> fields)
        {
            var offsets = make_slice<long>(len(fields));
            long o = default;
            foreach (var (i, f) in fields)
            {
                var a = s.Alignof(f.typ);
                o = align(o, a);
                offsets[i] = o;
                o += s.Sizeof(f.typ);
            }
            return offsets;
        }

        private static array<byte> basicSizes = new array<byte>(InitKeyedValues<byte>((Bool, 1), (Int8, 1), (Int16, 2), (Int32, 4), (Int64, 8), (Uint8, 1), (Uint16, 2), (Uint32, 4), (Uint64, 8), (Float32, 4), (Float64, 8), (Complex64, 8), (Complex128, 16)));

        private static long Sizeof(this ref StdSizes s, Type T)
        {
            switch (T.Underlying().type())
            {
                case ref Basic t:
                    assert(isTyped(T));
                    var k = t.kind;
                    if (int(k) < len(basicSizes))
                    {
                        {
                            var s = basicSizes[k];

                            if (s > 0L)
                            {
                                return int64(s);
                            }

                        }
                    }
                    if (k == String)
                    {
                        return s.WordSize * 2L;
                    }
                    break;
                case ref Array t:
                    var n = t.len;
                    if (n == 0L)
                    {
                        return 0L;
                    }
                    var a = s.Alignof(t.elem);
                    var z = s.Sizeof(t.elem);
                    return align(z, a) * (n - 1L) + z;
                    break;
                case ref Slice t:
                    return s.WordSize * 3L;
                    break;
                case ref Struct t:
                    n = t.NumFields();
                    if (n == 0L)
                    {
                        return 0L;
                    }
                    var offsets = s.Offsetsof(t.fields);
                    return offsets[n - 1L] + s.Sizeof(t.fields[n - 1L].typ);
                    break;
                case ref Interface t:
                    return s.WordSize * 2L;
                    break;
            }
            return s.WordSize; // catch-all
        }

        // common architecture word sizes and alignments
        private static map gcArchSizes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref StdSizes>{"386":{4,4},"arm":{4,4},"arm64":{8,8},"amd64":{8,8},"amd64p32":{4,8},"mips":{4,4},"mipsle":{4,4},"mips64":{8,8},"mips64le":{8,8},"ppc64":{8,8},"ppc64le":{8,8},"s390x":{8,8},};

        // SizesFor returns the Sizes used by a compiler for an architecture.
        // The result is nil if a compiler/architecture pair is not known.
        //
        // Supported architectures for compiler "gc":
        // "386", "arm", "arm64", "amd64", "amd64p32", "mips", "mipsle",
        // "mips64", "mips64le", "ppc64", "ppc64le", "s390x".
        public static Sizes SizesFor(@string compiler, @string arch)
        {
            if (compiler != "gc")
            {
                return null;
            }
            var (s, ok) = gcArchSizes[arch];
            if (!ok)
            {
                return null;
            }
            return s;
        }

        // stdSizes is used if Config.Sizes == nil.
        private static var stdSizes = SizesFor("gc", "amd64");

        private static long alignof(this ref Config _conf, Type T) => func(_conf, (ref Config conf, Defer _, Panic panic, Recover __) =>
        {
            {
                var s = conf.Sizes;

                if (s != null)
                {
                    {
                        var a = s.Alignof(T);

                        if (a >= 1L)
                        {
                            return a;
                        }

                    }
                    panic("Config.Sizes.Alignof returned an alignment < 1");
                }

            }
            return stdSizes.Alignof(T);
        });

        private static slice<long> offsetsof(this ref Config _conf, ref Struct _T) => func(_conf, _T, (ref Config conf, ref Struct T, Defer _, Panic panic, Recover __) =>
        {
            slice<long> offsets = default;
            if (T.NumFields() > 0L)
            { 
                // compute offsets on demand
                {
                    var s = conf.Sizes;

                    if (s != null)
                    {
                        offsets = s.Offsetsof(T.fields); 
                        // sanity checks
                        if (len(offsets) != T.NumFields())
                        {
                            panic("Config.Sizes.Offsetsof returned the wrong number of offsets");
                        }
                        foreach (var (_, o) in offsets)
                        {
                            if (o < 0L)
                            {
                                panic("Config.Sizes.Offsetsof returned an offset < 0");
                            }
                        }
                    else
                    }                    {
                        offsets = stdSizes.Offsetsof(T.fields);
                    }

                }
            }
            return offsets;
        });

        // offsetof returns the offset of the field specified via
        // the index sequence relative to typ. All embedded fields
        // must be structs (rather than pointer to structs).
        private static long offsetof(this ref Config conf, Type typ, slice<long> index)
        {
            long o = default;
            foreach (var (_, i) in index)
            {
                ref Struct s = typ.Underlying()._<ref Struct>();
                o += conf.offsetsof(s)[i];
                typ = s.fields[i].typ;
            }
            return o;
        }

        private static long @sizeof(this ref Config _conf, Type T) => func(_conf, (ref Config conf, Defer _, Panic panic, Recover __) =>
        {
            {
                var s = conf.Sizes;

                if (s != null)
                {
                    {
                        var z = s.Sizeof(T);

                        if (z >= 0L)
                        {
                            return z;
                        }

                    }
                    panic("Config.Sizes.Sizeof returned a size < 0");
                }

            }
            return stdSizes.Sizeof(T);
        });

        // align returns the smallest y >= x such that y % a == 0.
        private static long align(long x, long a)
        {
            var y = x + a - 1L;
            return y - y % a;
        }
    }
}}
