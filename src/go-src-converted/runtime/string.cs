// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:21:00 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\string.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class runtime_package
    {
        // The constant is known to the compiler.
        // There is no fundamental theory behind this number.
        private static readonly long tmpStringBufSize = 32L;



        private partial struct tmpBuf // : array<byte>
        {
        }

        // concatstrings implements a Go string concatenation x+y+z+...
        // The operands are passed in the slice a.
        // If buf != nil, the compiler has determined that the result does not
        // escape the calling function, so the string data can be stored in buf
        // if small enough.
        private static @string concatstrings(ref tmpBuf buf, slice<@string> a)
        {
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
            var (s, b) = rawstringtmp(buf, l);
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

        private static @string concatstring2(ref tmpBuf buf, array<@string> a)
        {
            a = a.Clone();

            return concatstrings(buf, a[..]);
        }

        private static @string concatstring3(ref tmpBuf buf, array<@string> a)
        {
            a = a.Clone();

            return concatstrings(buf, a[..]);
        }

        private static @string concatstring4(ref tmpBuf buf, array<@string> a)
        {
            a = a.Clone();

            return concatstrings(buf, a[..]);
        }

        private static @string concatstring5(ref tmpBuf buf, array<@string> a)
        {
            a = a.Clone();

            return concatstrings(buf, a[..]);
        }

        // Buf is a fixed-size buffer for the result,
        // it is not nil if the result does not escape.
        private static @string slicebytetostring(ref tmpBuf buf, slice<byte> b)
        {
            var l = len(b);
            if (l == 0L)
            { 
                // Turns out to be a relatively common case.
                // Consider that you want to parse out data between parens in "foo()bar",
                // you find the indices and convert the subslice to string.
                return "";
            }
            if (raceenabled)
            {
                racereadrangepc(@unsafe.Pointer(ref b[0L]), uintptr(l), getcallerpc(), funcPC(slicebytetostring));
            }
            if (msanenabled)
            {
                msanread(@unsafe.Pointer(ref b[0L]), uintptr(l));
            }
            unsafe.Pointer p = default;
            if (buf != null && len(b) <= len(buf))
            {
                p = @unsafe.Pointer(buf);
            }
            else
            {
                p = mallocgc(uintptr(len(b)), null, false);
            }
            stringStructOf(ref str).str;

            p;
            stringStructOf(ref str).len;

            len(b);
            memmove(p, (@unsafe.Pointer(ref b).Value).array, uintptr(len(b)));
            return;
        }

        // stringDataOnStack reports whether the string's data is
        // stored on the current goroutine's stack.
        private static bool stringDataOnStack(@string s)
        {
            var ptr = uintptr(stringStructOf(ref s).str);
            var stk = getg().stack;
            return stk.lo <= ptr && ptr < stk.hi;
        }

        private static (@string, slice<byte>) rawstringtmp(ref tmpBuf buf, long l)
        {
            if (buf != null && l <= len(buf))
            {
                b = buf[..l];
                s = slicebytetostringtmp(b);
            }
            else
            {
                s, b = rawstring(l);
            }
            return;
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
        // - Used for m[string(k)] lookup where m is a string-keyed map and k is a []byte.
        // - Used for "<"+string(b)+">" concatenation where b is []byte.
        // - Used for string(b)=="foo" comparison where b is []byte.
        private static @string slicebytetostringtmp(slice<byte> b)
        {
            if (raceenabled && len(b) > 0L)
            {
                racereadrangepc(@unsafe.Pointer(ref b[0L]), uintptr(len(b)), getcallerpc(), funcPC(slicebytetostringtmp));
            }
            if (msanenabled && len(b) > 0L)
            {
                msanread(@unsafe.Pointer(ref b[0L]), uintptr(len(b)));
            }
            return @unsafe.Pointer(ref b).Value;
        }

        private static slice<byte> stringtoslicebyte(ref tmpBuf buf, @string s)
        {
            slice<byte> b = default;
            if (buf != null && len(s) <= len(buf))
            {
                buf.Value = new tmpBuf();
                b = buf[..len(s)];
            }
            else
            {
                b = rawbyteslice(len(s));
            }
            copy(b, s);
            return b;
        }

        private static slice<int> stringtoslicerune(ref array<int> buf, @string s)
        { 
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
                buf.Value = new array<int>(new int[] {  });
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

        private static @string slicerunetostring(ref tmpBuf buf, slice<int> a)
        {
            if (raceenabled && len(a) > 0L)
            {
                racereadrangepc(@unsafe.Pointer(ref a[0L]), uintptr(len(a)) * @unsafe.Sizeof(a[0L]), getcallerpc(), funcPC(slicerunetostring));
            }
            if (msanenabled && len(a) > 0L)
            {
                msanread(@unsafe.Pointer(ref a[0L]), uintptr(len(a)) * @unsafe.Sizeof(a[0L]));
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

            var (s, b) = rawstringtmp(buf, size1 + 3L);
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

        private static ref stringStruct stringStructOf(ref @string sp)
        {
            return (stringStruct.Value)(@unsafe.Pointer(sp));
        }

        private static @string intstring(ref array<byte> buf, long v)
        {
            @string s = default;
            slice<byte> b = default;
            if (buf != null)
            {
                b = buf[..];
                s = slicebytetostringtmp(b);
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
            var p = mallocgc(uintptr(size), null, false);

            stringStructOf(ref s).str;

            p;
            stringStructOf(ref s).len;

            size * (slice.Value)(@unsafe.Pointer(ref b));

            new slice(p,size,size);

            return;
        }

        // rawbyteslice allocates a new byte slice. The byte slice is not zeroed.
        private static slice<byte> rawbyteslice(long size)
        {
            var cap = roundupsize(uintptr(size));
            var p = mallocgc(cap, null, false);
            if (cap != uintptr(size))
            {
                memclrNoHeapPointers(add(p, uintptr(size)), cap - uintptr(size));
            }
            (slice.Value)(@unsafe.Pointer(ref b)).Value;

            new slice(p,size,int(cap));
            return;
        }

        // rawruneslice allocates a new rune slice. The rune slice is not zeroed.
        private static slice<int> rawruneslice(long size)
        {
            if (uintptr(size) > _MaxMem / 4L)
            {
                throw("out of memory");
            }
            var mem = roundupsize(uintptr(size) * 4L);
            var p = mallocgc(mem, null, false);
            if (mem != uintptr(size) * 4L)
            {
                memclrNoHeapPointers(add(p, uintptr(size) * 4L), mem - uintptr(size) * 4L);
            }
            (slice.Value)(@unsafe.Pointer(ref b)).Value;

            new slice(p,size,int(mem/4));
            return;
        }

        // used by cmd/cgo
        private static slice<byte> gobytes(ref byte p, long n)
        {
            if (n == 0L)
            {
                return make_slice<byte>(0L);
            }
            var x = make_slice<byte>(n);
            memmove(@unsafe.Pointer(ref x[0L]), @unsafe.Pointer(p), uintptr(n));
            return x;
        }

        private static @string gostring(ref byte p)
        {
            var l = findnull(p);
            if (l == 0L)
            {
                return "";
            }
            var (s, b) = rawstring(l);
            memmove(@unsafe.Pointer(ref b[0L]), @unsafe.Pointer(p), uintptr(l));
            return s;
        }

        private static @string gostringn(ref byte p, long l)
        {
            if (l == 0L)
            {
                return "";
            }
            var (s, b) = rawstring(l);
            memmove(@unsafe.Pointer(ref b[0L]), @unsafe.Pointer(p), uintptr(l));
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
                if (s[i] == t[0L] && hasprefix(s[i..], t))
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

        private static bool hasprefix(@string s, @string t)
        {
            return len(s) >= len(t) && s[..len(t)] == t;
        }

        private static readonly var maxUint = ~uint(0L);
        private static readonly var maxInt = int(maxUint >> (int)(1L));

        // atoi parses an int from a string s.
        // The bool result reports whether s is a number
        // representable by a value of type int.
        private static (long, bool) atoi(@string s)
        {
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
        private static long findnull(ref byte s)
        {
            if (s == null)
            {
                return 0L;
            }
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(s));
            long l = 0L;
            while (p[l] != 0L)
            {
                l++;
            }

            return l;
        }

        private static long findnullw(ref ushort s)
        {
            if (s == null)
            {
                return 0L;
            }
            ref array<ushort> p = new ptr<ref array<ushort>>(@unsafe.Pointer(s));
            long l = 0L;
            while (p[l] != 0L)
            {
                l++;
            }

            return l;
        }

        //go:nosplit
        private static @string gostringnocopy(ref byte str)
        {
            stringStruct ss = new stringStruct(str:unsafe.Pointer(str),len:findnull(str));
            *(*@string) s = @unsafe.Pointer(ref ss).Value;
            return s;
        }

        private static @string gostringw(ref ushort strw)
        {
            array<byte> buf = new array<byte>(8L);
            ref array<ushort> str = new ptr<ref array<ushort>>(@unsafe.Pointer(strw));
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
    }
}
