// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:46 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\string.go
using bytealg = go.@internal.bytealg_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // The constant is known to the compiler.
        // There is no fundamental theory behind this number.
        private static readonly long tmpStringBufSize = (long)32L;



        private partial struct tmpBuf // : array<byte>
        {
        }

        // concatstrings implements a Go string concatenation x+y+z+...
        // The operands are passed in the slice a.
        // If buf != nil, the compiler has determined that the result does not
        // escape the calling function, so the string data can be stored in buf
        // if small enough.
        private static @string concatstrings(ptr<tmpBuf> _addr_buf, slice<@string> a)
        {
            ref tmpBuf buf = ref _addr_buf.val;

            long idx = 0L;
            long l = 0L;
            long count = 0L;
            {
                var x__prev1 = x;

                foreach (var (__i, __x) in a)
                {
                    i = __i;
                    x = __x;
                    var n = len(x);
                    if (n == 0L)
                    {
                        continue;
                    }

                    if (l + n < l)
                    {
                        throw("string concatenation too long");
                    }

                    l += n;
                    count++;
                    idx = i;

                }

                x = x__prev1;
            }

            if (count == 0L)
            {
                return "";
            } 

            // If there is just one string and either it is not on the stack
            // or our result does not escape the calling frame (buf != nil),
            // then we can return that string directly.
            if (count == 1L && (buf != null || !stringDataOnStack(a[idx])))
            {
                return a[idx];
            }

            var (s, b) = rawstringtmp(_addr_buf, l);
            {
                var x__prev1 = x;

                foreach (var (_, __x) in a)
                {
                    x = __x;
                    copy(b, x);
                    b = b[len(x)..];
                }

                x = x__prev1;
            }

            return s;

        }

        private static @string concatstring2(ptr<tmpBuf> _addr_buf, array<@string> a)
        {
            a = a.Clone();
            ref tmpBuf buf = ref _addr_buf.val;

            return concatstrings(_addr_buf, a[..]);
        }

        private static @string concatstring3(ptr<tmpBuf> _addr_buf, array<@string> a)
        {
            a = a.Clone();
            ref tmpBuf buf = ref _addr_buf.val;

            return concatstrings(_addr_buf, a[..]);
        }

        private static @string concatstring4(ptr<tmpBuf> _addr_buf, array<@string> a)
        {
            a = a.Clone();
            ref tmpBuf buf = ref _addr_buf.val;

            return concatstrings(_addr_buf, a[..]);
        }

        private static @string concatstring5(ptr<tmpBuf> _addr_buf, array<@string> a)
        {
            a = a.Clone();
            ref tmpBuf buf = ref _addr_buf.val;

            return concatstrings(_addr_buf, a[..]);
        }

        // slicebytetostring converts a byte slice to a string.
        // It is inserted by the compiler into generated code.
        // ptr is a pointer to the first element of the slice;
        // n is the length of the slice.
        // Buf is a fixed-size buffer for the result,
        // it is not nil if the result does not escape.
        private static @string slicebytetostring(ptr<tmpBuf> _addr_buf, ptr<byte> _addr_ptr, long n)
        {
            @string str = default;
            ref tmpBuf buf = ref _addr_buf.val;
            ref byte ptr = ref _addr_ptr.val;

            if (n == 0L)
            { 
                // Turns out to be a relatively common case.
                // Consider that you want to parse out data between parens in "foo()bar",
                // you find the indices and convert the subslice to string.
                return "";

            }

            if (raceenabled)
            {
                racereadrangepc(@unsafe.Pointer(ptr), uintptr(n), getcallerpc(), funcPC(slicebytetostring));
            }

            if (msanenabled)
            {
                msanread(@unsafe.Pointer(ptr), uintptr(n));
            }

            if (n == 1L)
            {
                var p = @unsafe.Pointer(_addr_staticuint64s[ptr]);
                if (sys.BigEndian)
                {
                    p = add(p, 7L);
                }

                stringStructOf(_addr_str).str;

                p;
                stringStructOf(_addr_str).len;

                1L;
                return ;

            }

            p = default;
            if (buf != null && n <= len(buf))
            {
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
        private static bool stringDataOnStack(@string s)
        {
            var ptr = uintptr(stringStructOf(_addr_s).str);
            var stk = getg().stack;
            return stk.lo <= ptr && ptr < stk.hi;
        }

        private static (@string, slice<byte>) rawstringtmp(ptr<tmpBuf> _addr_buf, long l)
        {
            @string s = default;
            slice<byte> b = default;
            ref tmpBuf buf = ref _addr_buf.val;

            if (buf != null && l <= len(buf))
            {
                b = buf[..l];
                s = slicebytetostringtmp(_addr_b[0L], len(b));
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
        private static @string slicebytetostringtmp(ptr<byte> _addr_ptr, long n)
        {
            @string str = default;
            ref byte ptr = ref _addr_ptr.val;

            if (raceenabled && n > 0L)
            {
                racereadrangepc(@unsafe.Pointer(ptr), uintptr(n), getcallerpc(), funcPC(slicebytetostringtmp));
            }

            if (msanenabled && n > 0L)
            {
                msanread(@unsafe.Pointer(ptr), uintptr(n));
            }

            stringStructOf(_addr_str).str;

            @unsafe.Pointer(ptr);
            stringStructOf(_addr_str).len;

            n;
            return ;

        }

        private static slice<byte> stringtoslicebyte(ptr<tmpBuf> _addr_buf, @string s)
        {
            ref tmpBuf buf = ref _addr_buf.val;

            slice<byte> b = default;
            if (buf != null && len(s) <= len(buf))
            {
                buf = new tmpBuf();
                b = buf[..len(s)];
            }
            else
            {
                b = rawbyteslice(len(s));
            }

            copy(b, s);
            return b;

        }

        private static slice<int> stringtoslicerune(ptr<array<int>> _addr_buf, @string s)
        {
            ref array<int> buf = ref _addr_buf.val;
 
            // two passes.
            // unlike slicerunetostring, no race because strings are immutable.
            long n = 0L;
            foreach (var (_, r) in s)
            {
                n++;
            }
            slice<int> a = default;
            if (buf != null && n <= len(buf))
            {
                buf = new array<int>(new int[] {  });
                a = buf[..n];
            }
            else
            {
                a = rawruneslice(n);
            }

            n = 0L;
            foreach (var (_, r) in s)
            {
                a[n] = r;
                n++;
            }
            return a;

        }

        private static @string slicerunetostring(ptr<tmpBuf> _addr_buf, slice<int> a)
        {
            ref tmpBuf buf = ref _addr_buf.val;

            if (raceenabled && len(a) > 0L)
            {
                racereadrangepc(@unsafe.Pointer(_addr_a[0L]), uintptr(len(a)) * @unsafe.Sizeof(a[0L]), getcallerpc(), funcPC(slicerunetostring));
            }

            if (msanenabled && len(a) > 0L)
            {
                msanread(@unsafe.Pointer(_addr_a[0L]), uintptr(len(a)) * @unsafe.Sizeof(a[0L]));
            }

            array<byte> dum = new array<byte>(4L);
            long size1 = 0L;
            {
                var r__prev1 = r;

                foreach (var (_, __r) in a)
                {
                    r = __r;
                    size1 += encoderune(dum[..], r);
                }

                r = r__prev1;
            }

            var (s, b) = rawstringtmp(_addr_buf, size1 + 3L);
            long size2 = 0L;
            {
                var r__prev1 = r;

                foreach (var (_, __r) in a)
                {
                    r = __r; 
                    // check for race
                    if (size2 >= size1)
                    {
                        break;
                    }

                    size2 += encoderune(b[size2..], r);

                }

                r = r__prev1;
            }

            return s[..size2];

        }

        private partial struct stringStruct
        {
            public unsafe.Pointer str;
            public long len;
        }

        // Variant with *byte pointer type for DWARF debugging.
        private partial struct stringStructDWARF
        {
            public ptr<byte> str;
            public long len;
        }

        private static ptr<stringStruct> stringStructOf(ptr<@string> _addr_sp)
        {
            ref @string sp = ref _addr_sp.val;

            return _addr_(stringStruct.val)(@unsafe.Pointer(sp))!;
        }

        private static @string intstring(ptr<array<byte>> _addr_buf, long v)
        {
            @string s = default;
            ref array<byte> buf = ref _addr_buf.val;

            slice<byte> b = default;
            if (buf != null)
            {
                b = buf[..];
                s = slicebytetostringtmp(_addr_b[0L], len(b));
            }
            else
            {
                s, b = rawstring(4L);
            }

            if (int64(rune(v)) != v)
            {
                v = runeError;
            }

            var n = encoderune(b, rune(v));
            return s[..n];

        }

        // rawstring allocates storage for a new string. The returned
        // string and byte slice both refer to the same storage.
        // The storage is not zeroed. Callers should use
        // b to set the string contents and then drop b.
        private static (@string, slice<byte>) rawstring(long size)
        {
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
        private static slice<byte> rawbyteslice(long size)
        {
            slice<byte> b = default;

            var cap = roundupsize(uintptr(size));
            var p = mallocgc(cap, null, false);
            if (cap != uintptr(size))
            {
                memclrNoHeapPointers(add(p, uintptr(size)), cap - uintptr(size));
            }

            (slice.val)(@unsafe.Pointer(_addr_b)).val;

            new slice(p,size,int(cap));
            return ;

        }

        // rawruneslice allocates a new rune slice. The rune slice is not zeroed.
        private static slice<int> rawruneslice(long size)
        {
            slice<int> b = default;

            if (uintptr(size) > maxAlloc / 4L)
            {
                throw("out of memory");
            }

            var mem = roundupsize(uintptr(size) * 4L);
            var p = mallocgc(mem, null, false);
            if (mem != uintptr(size) * 4L)
            {
                memclrNoHeapPointers(add(p, uintptr(size) * 4L), mem - uintptr(size) * 4L);
            }

            (slice.val)(@unsafe.Pointer(_addr_b)).val;

            new slice(p,size,int(mem/4));
            return ;

        }

        // used by cmd/cgo
        private static slice<byte> gobytes(ptr<byte> _addr_p, long n) => func((_, panic, __) =>
        {
            slice<byte> b = default;
            ref byte p = ref _addr_p.val;

            if (n == 0L)
            {
                return make_slice<byte>(0L);
            }

            if (n < 0L || uintptr(n) > maxAlloc)
            {
                panic(errorString("gobytes: length out of range"));
            }

            var bp = mallocgc(uintptr(n), null, false);
            memmove(bp, @unsafe.Pointer(p), uintptr(n)) * (slice.val)(@unsafe.Pointer(_addr_b));

            new slice(bp,n,n);
            return ;

        });

        // This is exported via linkname to assembly in syscall (for Plan9).
        //go:linkname gostring
        private static @string gostring(ptr<byte> _addr_p)
        {
            ref byte p = ref _addr_p.val;

            var l = findnull(_addr_p);
            if (l == 0L)
            {
                return "";
            }

            var (s, b) = rawstring(l);
            memmove(@unsafe.Pointer(_addr_b[0L]), @unsafe.Pointer(p), uintptr(l));
            return s;

        }

        private static @string gostringn(ptr<byte> _addr_p, long l)
        {
            ref byte p = ref _addr_p.val;

            if (l == 0L)
            {
                return "";
            }

            var (s, b) = rawstring(l);
            memmove(@unsafe.Pointer(_addr_b[0L]), @unsafe.Pointer(p), uintptr(l));
            return s;

        }

        private static long index(@string s, @string t)
        {
            if (len(t) == 0L)
            {
                return 0L;
            }

            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == t[0L] && hasPrefix(s[i..], t))
                {
                    return i;
                }

            }

            return -1L;

        }

        private static bool contains(@string s, @string t)
        {
            return index(s, t) >= 0L;
        }

        private static bool hasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[..len(prefix)] == prefix;
        }

        private static readonly var maxUint = ~uint(0L);
        private static readonly var maxInt = int(maxUint >> (int)(1L));


        // atoi parses an int from a string s.
        // The bool result reports whether s is a number
        // representable by a value of type int.
        private static (long, bool) atoi(@string s)
        {
            long _p0 = default;
            bool _p0 = default;

            if (s == "")
            {
                return (0L, false);
            }

            var neg = false;
            if (s[0L] == '-')
            {
                neg = true;
                s = s[1L..];
            }

            var un = uint(0L);
            for (long i = 0L; i < len(s); i++)
            {
                var c = s[i];
                if (c < '0' || c > '9')
                {
                    return (0L, false);
                }

                if (un > maxUint / 10L)
                { 
                    // overflow
                    return (0L, false);

                }

                un *= 10L;
                var un1 = un + uint(c) - '0';
                if (un1 < un)
                { 
                    // overflow
                    return (0L, false);

                }

                un = un1;

            }


            if (!neg && un > uint(maxInt))
            {
                return (0L, false);
            }

            if (neg && un > uint(maxInt) + 1L)
            {
                return (0L, false);
            }

            var n = int(un);
            if (neg)
            {
                n = -n;
            }

            return (n, true);

        }

        // atoi32 is like atoi but for integers
        // that fit into an int32.
        private static (int, bool) atoi32(@string s)
        {
            int _p0 = default;
            bool _p0 = default;

            {
                var (n, ok) = atoi(s);

                if (n == int(int32(n)))
                {
                    return (int32(n), ok);
                }

            }

            return (0L, false);

        }

        //go:nosplit
        private static long findnull(ptr<byte> _addr_s)
        {
            ref byte s = ref _addr_s.val;

            if (s == null)
            {
                return 0L;
            } 

            // Avoid IndexByteString on Plan 9 because it uses SSE instructions
            // on x86 machines, and those are classified as floating point instructions,
            // which are illegal in a note handler.
            if (GOOS == "plan9")
            {
                ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(s));
                long l = 0L;
                while (p[l] != 0L)
                {
                    l++;
                }

                return l;

            } 

            // pageSize is the unit we scan at a time looking for NULL.
            // It must be the minimum page size for any architecture Go
            // runs on. It's okay (just a minor performance loss) if the
            // actual system page size is larger than this value.
            const long pageSize = (long)4096L;



            long offset = 0L;
            var ptr = @unsafe.Pointer(s); 
            // IndexByteString uses wide reads, so we need to be careful
            // with page boundaries. Call IndexByteString on
            // [ptr, endOfPage) interval.
            var safeLen = int(pageSize - uintptr(ptr) % pageSize);

            while (true)
            {
                ptr<ptr<@string>> t = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(addr(new stringStruct(ptr,safeLen)))); 
                // Check one page at a time.
                {
                    var i = bytealg.IndexByteString(t, 0L);

                    if (i != -1L)
                    {
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

        private static long findnullw(ptr<ushort> _addr_s)
        {
            ref ushort s = ref _addr_s.val;

            if (s == null)
            {
                return 0L;
            }

            ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(s));
            long l = 0L;
            while (p[l] != 0L)
            {
                l++;
            }

            return l;

        }

        //go:nosplit
        private static @string gostringnocopy(ptr<byte> _addr_str)
        {
            ref byte str = ref _addr_str.val;

            ref stringStruct ss = ref heap(new stringStruct(str:unsafe.Pointer(str),len:findnull(str)), out ptr<stringStruct> _addr_ss);
            ptr<ptr<@string>> s = new ptr<ptr<ptr<@string>>>(@unsafe.Pointer(_addr_ss));
            return s;
        }

        private static @string gostringw(ptr<ushort> _addr_strw)
        {
            ref ushort strw = ref _addr_strw.val;

            array<byte> buf = new array<byte>(8L);
            ptr<array<ushort>> str = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(strw));
            long n1 = 0L;
            {
                long i__prev1 = i;

                for (long i = 0L; str[i] != 0L; i++)
                {
                    n1 += encoderune(buf[..], rune(str[i]));
                }


                i = i__prev1;
            }
            var (s, b) = rawstring(n1 + 4L);
            long n2 = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; str[i] != 0L; i++)
                { 
                    // check for race
                    if (n2 >= n1)
                    {
                        break;
                    }

                    n2 += encoderune(b[n2..], rune(str[i]));

                }


                i = i__prev1;
            }
            b[n2] = 0L; // for luck
            return s[..n2];

        }

        // parseRelease parses a dot-separated version number. It follows the
        // semver syntax, but allows the minor and patch versions to be
        // elided.
        private static (long, long, long, bool) parseRelease(@string rel)
        {
            long major = default;
            long minor = default;
            long patch = default;
            bool ok = default;
 
            // Strip anything after a dash or plus.
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(rel); i++)
                {
                    if (rel[i] == '-' || rel[i] == '+')
                    {
                        rel = rel[..i];
                        break;
                    }

                }


                i = i__prev1;
            }

            Func<(long, bool)> next = () =>
            {
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(rel); i++)
                    {
                        if (rel[i] == '.')
                        {
                            var (ver, ok) = atoi(rel[..i]);
                            rel = rel[i + 1L..];
                            return (ver, ok);
                        }

                    }


                    i = i__prev1;
                }
                (ver, ok) = atoi(rel);
                rel = "";
                return (ver, ok);

            }
;
            major, ok = next();

            if (!ok || rel == "")
            {
                return ;
            }

            minor, ok = next();

            if (!ok || rel == "")
            {
                return ;
            }

            patch, ok = next();
            return ;

        }
    }
}
