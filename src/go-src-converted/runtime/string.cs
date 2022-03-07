// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\string.go
using bytealg = go.@internal.bytealg_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // The constant is known to the compiler.
    // There is no fundamental theory behind this number.
private static readonly nint tmpStringBufSize = 32;



private partial struct tmpBuf { // : array<byte>
}

// concatstrings implements a Go string concatenation x+y+z+...
// The operands are passed in the slice a.
// If buf != nil, the compiler has determined that the result does not
// escape the calling function, so the string data can be stored in buf
// if small enough.
private static @string concatstrings(ptr<tmpBuf> _addr_buf, slice<@string> a) {
    ref tmpBuf buf = ref _addr_buf.val;

    nint idx = 0;
    nint l = 0;
    nint count = 0;
    {
        var x__prev1 = x;

        foreach (var (__i, __x) in a) {
            i = __i;
            x = __x;
            var n = len(x);
            if (n == 0) {
                continue;
            }
            if (l + n < l) {
                throw("string concatenation too long");
            }
            l += n;
            count++;
            idx = i;
        }
        x = x__prev1;
    }

    if (count == 0) {
        return "";
    }
    if (count == 1 && (buf != null || !stringDataOnStack(a[idx]))) {
        return a[idx];
    }
    var (s, b) = rawstringtmp(_addr_buf, l);
    {
        var x__prev1 = x;

        foreach (var (_, __x) in a) {
            x = __x;
            copy(b, x);
            b = b[(int)len(x)..];
        }
        x = x__prev1;
    }

    return s;

}

private static @string concatstring2(ptr<tmpBuf> _addr_buf, @string a0, @string a1) {
    ref tmpBuf buf = ref _addr_buf.val;

    return concatstrings(_addr_buf, new slice<@string>(new @string[] { a0, a1 }));
}

private static @string concatstring3(ptr<tmpBuf> _addr_buf, @string a0, @string a1, @string a2) {
    ref tmpBuf buf = ref _addr_buf.val;

    return concatstrings(_addr_buf, new slice<@string>(new @string[] { a0, a1, a2 }));
}

private static @string concatstring4(ptr<tmpBuf> _addr_buf, @string a0, @string a1, @string a2, @string a3) {
    ref tmpBuf buf = ref _addr_buf.val;

    return concatstrings(_addr_buf, new slice<@string>(new @string[] { a0, a1, a2, a3 }));
}

private static @string concatstring5(ptr<tmpBuf> _addr_buf, @string a0, @string a1, @string a2, @string a3, @string a4) {
    ref tmpBuf buf = ref _addr_buf.val;

    return concatstrings(_addr_buf, new slice<@string>(new @string[] { a0, a1, a2, a3, a4 }));
}

// slicebytetostring converts a byte slice to a string.
// It is inserted by the compiler into generated code.
// ptr is a pointer to the first element of the slice;
// n is the length of the slice.
// Buf is a fixed-size buffer for the result,
// it is not nil if the result does not escape.
private static @string slicebytetostring(ptr<tmpBuf> _addr_buf, ptr<byte> _addr_ptr, nint n) {
    @string str = default;
    ref tmpBuf buf = ref _addr_buf.val;
    ref byte ptr = ref _addr_ptr.val;

    if (n == 0) { 
        // Turns out to be a relatively common case.
        // Consider that you want to parse out data between parens in "foo()bar",
        // you find the indices and convert the subslice to string.
        return "";

    }
    if (raceenabled) {
        racereadrangepc(@unsafe.Pointer(ptr), uintptr(n), getcallerpc(), funcPC(slicebytetostring));
    }
    if (msanenabled) {
        msanread(@unsafe.Pointer(ptr), uintptr(n));
    }
    if (n == 1) {
        var p = @unsafe.Pointer(_addr_staticuint64s[ptr]);
        if (sys.BigEndian) {
            p = add(p, 7);
        }
        stringStructOf(_addr_str).str;

        p;
        stringStructOf(_addr_str).len;

        1;
        return ;

    }
    p = default;
    if (buf != null && n <= len(buf)) {
        p = @unsafe.Pointer(buf);
    }
    else
 {
        p = mallocgc(uintptr(n), null, false);
    }
    stringStructOf(_addr_str).str;

    p;
    stringStructOf(_addr_str).len;

    n;
    memmove(p, @unsafe.Pointer(ptr), uintptr(n));
    return ;

}

// stringDataOnStack reports whether the string's data is
// stored on the current goroutine's stack.
private static bool stringDataOnStack(@string s) {
    var ptr = uintptr(stringStructOf(_addr_s).str);
    var stk = getg().stack;
    return stk.lo <= ptr && ptr < stk.hi;
}

private static (@string, slice<byte>) rawstringtmp(ptr<tmpBuf> _addr_buf, nint l) {
    @string s = default;
    slice<byte> b = default;
    ref tmpBuf buf = ref _addr_buf.val;

    if (buf != null && l <= len(buf)) {
        b = buf[..(int)l];
        s = slicebytetostringtmp(_addr_b[0], len(b));
    }
    else
 {
        s, b = rawstring(l);
    }
    return ;

}

// slicebytetostringtmp returns a "string" referring to the actual []byte bytes.
//
// Callers need to ensure that the returned string will not be used after
// the calling goroutine modifies the original slice or synchronizes with
// another goroutine.
//
// The function is only called when instrumenting
// and otherwise intrinsified by the compiler.
//
// Some internal compiler optimizations use this function.
// - Used for m[T1{... Tn{..., string(k), ...} ...}] and m[string(k)]
//   where k is []byte, T1 to Tn is a nesting of struct and array literals.
// - Used for "<"+string(b)+">" concatenation where b is []byte.
// - Used for string(b)=="foo" comparison where b is []byte.
private static @string slicebytetostringtmp(ptr<byte> _addr_ptr, nint n) {
    @string str = default;
    ref byte ptr = ref _addr_ptr.val;

    if (raceenabled && n > 0) {
        racereadrangepc(@unsafe.Pointer(ptr), uintptr(n), getcallerpc(), funcPC(slicebytetostringtmp));
    }
    if (msanenabled && n > 0) {
        msanread(@unsafe.Pointer(ptr), uintptr(n));
    }
    stringStructOf(_addr_str).str;

    @unsafe.Pointer(ptr);
    stringStructOf(_addr_str).len;

    n;
    return ;

}

private static slice<byte> stringtoslicebyte(ptr<tmpBuf> _addr_buf, @string s) {
    ref tmpBuf buf = ref _addr_buf.val;

    slice<byte> b = default;
    if (buf != null && len(s) <= len(buf)) {
        buf = new tmpBuf();
        b = buf[..(int)len(s)];
    }
    else
 {
        b = rawbyteslice(len(s));
    }
    copy(b, s);
    return b;

}

private static slice<int> stringtoslicerune(ptr<array<int>> _addr_buf, @string s) {
    ref array<int> buf = ref _addr_buf.val;
 
    // two passes.
    // unlike slicerunetostring, no race because strings are immutable.
    nint n = 0;
    foreach (var (_, r) in s) {
        n++;
    }    slice<int> a = default;
    if (buf != null && n <= len(buf)) {
        buf = new array<int>(new int[] {  });
        a = buf[..(int)n];
    }
    else
 {
        a = rawruneslice(n);
    }
    n = 0;
    foreach (var (_, r) in s) {
        a[n] = r;
        n++;
    }    return a;

}

private static @string slicerunetostring(ptr<tmpBuf> _addr_buf, slice<int> a) {
    ref tmpBuf buf = ref _addr_buf.val;

    if (raceenabled && len(a) > 0) {
        racereadrangepc(@unsafe.Pointer(_addr_a[0]), uintptr(len(a)) * @unsafe.Sizeof(a[0]), getcallerpc(), funcPC(slicerunetostring));
    }
    if (msanenabled && len(a) > 0) {
        msanread(@unsafe.Pointer(_addr_a[0]), uintptr(len(a)) * @unsafe.Sizeof(a[0]));
    }
    array<byte> dum = new array<byte>(4);
    nint size1 = 0;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in a) {
            r = __r;
            size1 += encoderune(dum[..], r);
        }
        r = r__prev1;
    }

    var (s, b) = rawstringtmp(_addr_buf, size1 + 3);
    nint size2 = 0;
    {
        var r__prev1 = r;

        foreach (var (_, __r) in a) {
            r = __r; 
            // check for race
            if (size2 >= size1) {
                break;
            }

            size2 += encoderune(b[(int)size2..], r);

        }
        r = r__prev1;
    }

    return s[..(int)size2];

}

private partial struct stringStruct {
    public unsafe.Pointer str;
    public nint len;
}

// Variant with *byte pointer type for DWARF debugging.
private partial struct stringStructDWARF {
    public ptr<byte> str;
    public nint len;
}

private static ptr<stringStruct> stringStructOf(ptr<@string> _addr_sp) {
    ref @string sp = ref _addr_sp.val;

    return _addr_(stringStruct.val)(@unsafe.Pointer(sp))!;
}

private static @string intstring(ptr<array<byte>> _addr_buf, long v) {
    @string s = default;
    ref array<byte> buf = ref _addr_buf.val;

    slice<byte> b = default;
    if (buf != null) {
        b = buf[..];
        s = slicebytetostringtmp(_addr_b[0], len(b));
    }
    else
 {
        s, b = rawstring(4);
    }
    if (int64(rune(v)) != v) {
        v = runeError;
    }
    var n = encoderune(b, rune(v));
    return s[..(int)n];

}

// rawstring allocates storage for a new string. The returned
// string and byte slice both refer to the same storage.
// The storage is not zeroed. Callers should use
// b to set the string contents and then drop b.
private static (@string, slice<byte>) rawstring(nint size) {
    @string s = default;
    slice<byte> b = default;

    var p = mallocgc(uintptr(size), null, false);

    stringStructOf(_addr_s).str;

    p;
    stringStructOf(_addr_s).len;

    size * (slice.val)(@unsafe.Pointer(_addr_b));

    new slice(p,size,size);

    return ;

}

// rawbyteslice allocates a new byte slice. The byte slice is not zeroed.
private static slice<byte> rawbyteslice(nint size) {
    slice<byte> b = default;

    var cap = roundupsize(uintptr(size));
    var p = mallocgc(cap, null, false);
    if (cap != uintptr(size)) {
        memclrNoHeapPointers(add(p, uintptr(size)), cap - uintptr(size));
    }
    (slice.val)(@unsafe.Pointer(_addr_b)).val;

    new slice(p,size,int(cap));
    return ;

}

// rawruneslice allocates a new rune slice. The rune slice is not zeroed.
private static slice<int> rawruneslice(nint size) {
    slice<int> b = default;

    if (uintptr(size) > maxAlloc / 4) {
        throw("out of memory");
    }
    var mem = roundupsize(uintptr(size) * 4);
    var p = mallocgc(mem, null, false);
    if (mem != uintptr(size) * 4) {
        memclrNoHeapPointers(add(p, uintptr(size) * 4), mem - uintptr(size) * 4);
    }
    (slice.val)(@unsafe.Pointer(_addr_b)).val;

    new slice(p,size,int(mem/4));
    return ;

}

// used by cmd/cgo
private static slice<byte> gobytes(ptr<byte> _addr_p, nint n) => func((_, panic, _) => {
    slice<byte> b = default;
    ref byte p = ref _addr_p.val;

    if (n == 0) {
        return make_slice<byte>(0);
    }
    if (n < 0 || uintptr(n) > maxAlloc) {
        panic(errorString("gobytes: length out of range"));
    }
    var bp = mallocgc(uintptr(n), null, false);
    memmove(bp, @unsafe.Pointer(p), uintptr(n)) * (slice.val)(@unsafe.Pointer(_addr_b));

    new slice(bp,n,n);
    return ;

});

// This is exported via linkname to assembly in syscall (for Plan9).
//go:linkname gostring
private static @string gostring(ptr<byte> _addr_p) {
    ref byte p = ref _addr_p.val;

    var l = findnull(_addr_p);
    if (l == 0) {
        return "";
    }
    var (s, b) = rawstring(l);
    memmove(@unsafe.Pointer(_addr_b[0]), @unsafe.Pointer(p), uintptr(l));
    return s;

}

private static @string gostringn(ptr<byte> _addr_p, nint l) {
    ref byte p = ref _addr_p.val;

    if (l == 0) {
        return "";
    }
    var (s, b) = rawstring(l);
    memmove(@unsafe.Pointer(_addr_b[0]), @unsafe.Pointer(p), uintptr(l));
    return s;

}

private static bool hasPrefix(@string s, @string prefix) {
    return len(s) >= len(prefix) && s[..(int)len(prefix)] == prefix;
}

private static readonly var maxUint = ~uint(0);
private static readonly var maxInt = int(maxUint >> 1);


// atoi parses an int from a string s.
// The bool result reports whether s is a number
// representable by a value of type int.
private static (nint, bool) atoi(@string s) {
    nint _p0 = default;
    bool _p0 = default;

    if (s == "") {
        return (0, false);
    }
    var neg = false;
    if (s[0] == '-') {
        neg = true;
        s = s[(int)1..];
    }
    var un = uint(0);
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c < '0' || c > '9') {
            return (0, false);
        }
        if (un > maxUint / 10) { 
            // overflow
            return (0, false);

        }
        un *= 10;
        var un1 = un + uint(c) - '0';
        if (un1 < un) { 
            // overflow
            return (0, false);

        }
        un = un1;

    }

    if (!neg && un > uint(maxInt)) {
        return (0, false);
    }
    if (neg && un > uint(maxInt) + 1) {
        return (0, false);
    }
    var n = int(un);
    if (neg) {
        n = -n;
    }
    return (n, true);

}

// atoi32 is like atoi but for integers
// that fit into an int32.
private static (int, bool) atoi32(@string s) {
    int _p0 = default;
    bool _p0 = default;

    {
        var (n, ok) = atoi(s);

        if (n == int(int32(n))) {
            return (int32(n), ok);
        }
    }

    return (0, false);

}

//go:nosplit
private static nint findnull(ptr<byte> _addr_s) {
    ref byte s = ref _addr_s.val;

    if (s == null) {
        return 0;
    }
    if (GOOS == "plan9") {
        ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(s));
        nint l = 0;
        while (p[l] != 0) {
            l++;
        }
        return l;
    }
    const nint pageSize = 4096;



    nint offset = 0;
    var ptr = @unsafe.Pointer(s); 
    // IndexByteString uses wide reads, so we need to be careful
    // with page boundaries. Call IndexByteString on
    // [ptr, endOfPage) interval.
    var safeLen = int(pageSize - uintptr(ptr) % pageSize);

    while (true) {
        ptr<ptr<@string>> t = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(addr(new stringStruct(ptr,safeLen)))); 
        // Check one page at a time.
        {
            var i = bytealg.IndexByteString(t, 0);

            if (i != -1) {
                return offset + i;
            } 
            // Move to next page

        } 
        // Move to next page
        ptr = @unsafe.Pointer(uintptr(ptr) + uintptr(safeLen));
        offset += safeLen;
        safeLen = pageSize;

    }

}

private static nint findnullw(ptr<ushort> _addr_s) {
    ref ushort s = ref _addr_s.val;

    if (s == null) {
        return 0;
    }
    ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(s));
    nint l = 0;
    while (p[l] != 0) {
        l++;
    }
    return l;

}

//go:nosplit
private static @string gostringnocopy(ptr<byte> _addr_str) {
    ref byte str = ref _addr_str.val;

    ref stringStruct ss = ref heap(new stringStruct(str:unsafe.Pointer(str),len:findnull(str)), out ptr<stringStruct> _addr_ss);
    ptr<ptr<@string>> s = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(_addr_ss));
    return s;
}

private static @string gostringw(ptr<ushort> _addr_strw) {
    ref ushort strw = ref _addr_strw.val;

    array<byte> buf = new array<byte>(8);
    ptr<array<ushort>> str = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(strw));
    nint n1 = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; str[i] != 0; i++) {
            n1 += encoderune(buf[..], rune(str[i]));
        }

        i = i__prev1;
    }
    var (s, b) = rawstring(n1 + 4);
    nint n2 = 0;
    {
        nint i__prev1 = i;

        for (i = 0; str[i] != 0; i++) { 
            // check for race
            if (n2 >= n1) {
                break;
            }

            n2 += encoderune(b[(int)n2..], rune(str[i]));

        }

        i = i__prev1;
    }
    b[n2] = 0; // for luck
    return s[..(int)n2];

}

} // end runtime_package
