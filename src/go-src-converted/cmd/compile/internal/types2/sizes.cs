// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements Sizes.

// package types2 -- go2cs converted at 2022 March 06 23:12:52 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\sizes.go


namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // Sizes defines the sizing functions for package unsafe.
public partial interface Sizes {
    long Alignof(Type T); // Offsetsof returns the offsets of the given struct fields, in bytes.
// Offsetsof must implement the offset guarantees required by the spec.
    long Offsetsof(slice<ptr<Var>> fields); // Sizeof returns the size of a variable of type T.
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
public partial struct StdSizes {
    public long WordSize; // word size in bytes - must be >= 4 (32bits)
    public long MaxAlign; // maximum alignment in bytes - must be >= 1
}

private static long Alignof(this ptr<StdSizes> _addr_s, Type T) {
    ref StdSizes s = ref _addr_s.val;
 
    // For arrays and structs, alignment is defined in terms
    // of alignment of the elements and fields, respectively.
    switch (optype(T).type()) {
        case ptr<Array> t:
            return s.Alignof(t.elem);
            break;
        case ptr<Struct> t:
            var max = int64(1);
            foreach (var (_, f) in t.fields) {
                {
                    var a__prev1 = a;

                    var a = s.Alignof(f.typ);

                    if (a > max) {
                        max = a;
                    }

                    a = a__prev1;

                }

            }
            return max;
            break;
        case ptr<Slice> t:
            return s.WordSize;
            break;
        case ptr<Interface> t:
            return s.WordSize;
            break;
        case ptr<Basic> t:
            if (t.Info() & IsString != 0) {
                return s.WordSize;
            }
            break;
    }
    a = s.Sizeof(T); // may be 0
    // spec: "For a variable x of any type: unsafe.Alignof(x) is at least 1."
    if (a < 1) {
        return 1;
    }
    if (isComplex(T)) {
        a /= 2;
    }
    if (a > s.MaxAlign) {
        return s.MaxAlign;
    }
    return a;

}

private static slice<long> Offsetsof(this ptr<StdSizes> _addr_s, slice<ptr<Var>> fields) {
    ref StdSizes s = ref _addr_s.val;

    var offsets = make_slice<long>(len(fields));
    long o = default;
    foreach (var (i, f) in fields) {
        var a = s.Alignof(f.typ);
        o = align(o, a);
        offsets[i] = o;
        o += s.Sizeof(f.typ);
    }    return offsets;
}

private static array<byte> basicSizes = new array<byte>(InitKeyedValues<byte>((Bool, 1), (Int8, 1), (Int16, 2), (Int32, 4), (Int64, 8), (Uint8, 1), (Uint16, 2), (Uint32, 4), (Uint64, 8), (Float32, 4), (Float64, 8), (Complex64, 8), (Complex128, 16)));

private static long Sizeof(this ptr<StdSizes> _addr_s, Type T) => func((_, panic, _) => {
    ref StdSizes s = ref _addr_s.val;

    switch (optype(T).type()) {
        case ptr<Basic> t:
            assert(isTyped(T));
            var k = t.kind;
            if (int(k) < len(basicSizes)) {
                {
                    var s = basicSizes[k];

                    if (s > 0) {
                        return int64(s);
                    }

                }

            }

            if (k == String) {
                return s.WordSize * 2;
            }

            break;
        case ptr<Array> t:
            var n = t.len;
            if (n <= 0) {
                return 0;
            } 
            // n > 0
            var a = s.Alignof(t.elem);
            var z = s.Sizeof(t.elem);
            return align(z, a) * (n - 1) + z;
            break;
        case ptr<Slice> t:
            return s.WordSize * 3;
            break;
        case ptr<Struct> t:
            n = t.NumFields();
            if (n == 0) {
                return 0;
            }
            var offsets = s.Offsetsof(t.fields);
            return offsets[n - 1] + s.Sizeof(t.fields[n - 1].typ);
            break;
        case ptr<Sum> t:
            panic("Sizeof unimplemented for type sum");
            break;
        case ptr<Interface> t:
            return s.WordSize * 2;
            break;
    }
    return s.WordSize; // catch-all
});

// common architecture word sizes and alignments
private static map gcArchSizes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<StdSizes>>{"386":{4,4},"arm":{4,4},"arm64":{8,8},"amd64":{8,8},"amd64p32":{4,8},"mips":{4,4},"mipsle":{4,4},"mips64":{8,8},"mips64le":{8,8},"ppc64":{8,8},"ppc64le":{8,8},"riscv64":{8,8},"s390x":{8,8},"sparc64":{8,8},"wasm":{8,8},};

// SizesFor returns the Sizes used by a compiler for an architecture.
// The result is nil if a compiler/architecture pair is not known.
//
// Supported architectures for compiler "gc":
// "386", "arm", "arm64", "amd64", "amd64p32", "mips", "mipsle",
// "mips64", "mips64le", "ppc64", "ppc64le", "riscv64", "s390x", "sparc64", "wasm".
public static Sizes SizesFor(@string compiler, @string arch) {
    map<@string, ptr<StdSizes>> m = default;
    switch (compiler) {
        case "gc": 
            m = gcArchSizes;
            break;
        case "gccgo": 
            m = gccgoArchSizes;
            break;
        default: 
            return null;
            break;
    }
    var (s, ok) = m[arch];
    if (!ok) {
        return null;
    }
    return s;

}

// stdSizes is used if Config.Sizes == nil.
private static var stdSizes = SizesFor("gc", "amd64");

private static long alignof(this ptr<Config> _addr_conf, Type T) => func((_, panic, _) => {
    ref Config conf = ref _addr_conf.val;

    {
        var s = conf.Sizes;

        if (s != null) {
            {
                var a = s.Alignof(T);

                if (a >= 1) {
                    return a;
                }

            }

            panic("Config.Sizes.Alignof returned an alignment < 1");

        }
    }

    return stdSizes.Alignof(T);

});

private static slice<long> offsetsof(this ptr<Config> _addr_conf, ptr<Struct> _addr_T) => func((_, panic, _) => {
    ref Config conf = ref _addr_conf.val;
    ref Struct T = ref _addr_T.val;

    slice<long> offsets = default;
    if (T.NumFields() > 0) { 
        // compute offsets on demand
        {
            var s = conf.Sizes;

            if (s != null) {
                offsets = s.Offsetsof(T.fields); 
                // sanity checks
                if (len(offsets) != T.NumFields()) {
                    panic("Config.Sizes.Offsetsof returned the wrong number of offsets");
                }

                foreach (var (_, o) in offsets) {
                    if (o < 0) {
                        panic("Config.Sizes.Offsetsof returned an offset < 0");
                    }
                }
            else
            } {
                offsets = stdSizes.Offsetsof(T.fields);
            }

        }

    }
    return offsets;

});

// offsetof returns the offset of the field specified via
// the index sequence relative to typ. All embedded fields
// must be structs (rather than pointer to structs).
private static long offsetof(this ptr<Config> _addr_conf, Type typ, slice<nint> index) {
    ref Config conf = ref _addr_conf.val;

    long o = default;
    foreach (var (_, i) in index) {
        var s = asStruct(typ);
        o += conf.offsetsof(s)[i];
        typ = s.fields[i].typ;
    }    return o;
}

private static long @sizeof(this ptr<Config> _addr_conf, Type T) => func((_, panic, _) => {
    ref Config conf = ref _addr_conf.val;

    {
        var s = conf.Sizes;

        if (s != null) {
            {
                var z = s.Sizeof(T);

                if (z >= 0) {
                    return z;
                }

            }

            panic("Config.Sizes.Sizeof returned a size < 0");

        }
    }

    return stdSizes.Sizeof(T);

});

// align returns the smallest y >= x such that y % a == 0.
private static long align(long x, long a) {
    var y = x + a - 1;
    return y - y % a;
}

} // end types2_package
