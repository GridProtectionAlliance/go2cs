// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Multiprecision decimal numbers.
// For floating-point formatting only; not general purpose.
// Only operations are assign and (binary) left/right shift.
// Can do binary floating point in multiprecision decimal precisely
// because 2 divides 10; cannot do decimal floating point
// in multiprecision binary precisely.

// package strconv -- go2cs converted at 2020 August 29 08:42:51 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\decimal.go

using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        private partial struct @decimal
        {
            public array<byte> d; // digits, big-endian representation
            public long nd; // number of digits used
            public long dp; // decimal point
            public bool neg; // negative flag
            public bool trunc; // discarded nonzero digits beyond d[:nd]
        }

        private static @string String(this ref decimal a)
        {
            long n = 10L + a.nd;
            if (a.dp > 0L)
            {
                n += a.dp;
            }
            if (a.dp < 0L)
            {
                n += -a.dp;
            }
            var buf = make_slice<byte>(n);
            long w = 0L;

            if (a.nd == 0L) 
                return "0";
            else if (a.dp <= 0L) 
                // zeros fill space between decimal point and digits
                buf[w] = '0';
                w++;
                buf[w] = '.';
                w++;
                w += digitZero(buf[w..w + -a.dp]);
                w += copy(buf[w..], a.d[0L..a.nd]);
            else if (a.dp < a.nd) 
                // decimal point in middle of digits
                w += copy(buf[w..], a.d[0L..a.dp]);
                buf[w] = '.';
                w++;
                w += copy(buf[w..], a.d[a.dp..a.nd]);
            else 
                // zeros fill space between digits and decimal point
                w += copy(buf[w..], a.d[0L..a.nd]);
                w += digitZero(buf[w..w + a.dp - a.nd]);
                        return string(buf[0L..w]);
        }

        private static long digitZero(slice<byte> dst)
        {
            foreach (var (i) in dst)
            {
                dst[i] = '0';
            }
            return len(dst);
        }

        // trim trailing zeros from number.
        // (They are meaningless; the decimal point is tracked
        // independent of the number of digits.)
        private static void trim(ref decimal a)
        {
            while (a.nd > 0L && a.d[a.nd - 1L] == '0')
            {
                a.nd--;
            }

            if (a.nd == 0L)
            {
                a.dp = 0L;
            }
        }

        // Assign v to a.
        private static void Assign(this ref decimal a, ulong v)
        {
            array<byte> buf = new array<byte>(24L); 

            // Write reversed decimal in buf.
            long n = 0L;
            while (v > 0L)
            {
                var v1 = v / 10L;
                v -= 10L * v1;
                buf[n] = byte(v + '0');
                n++;
                v = v1;
            } 

            // Reverse again to produce forward decimal in a.d.
 

            // Reverse again to produce forward decimal in a.d.
            a.nd = 0L;
            n--;

            while (n >= 0L)
            {
                a.d[a.nd] = buf[n];
                a.nd++;
                n--;
            }

            a.dp = a.nd;
            trim(a);
        }

        // Maximum shift that we can do in one pass without overflow.
        // A uint has 32 or 64 bits, and we have to be able to accommodate 9<<k.
        private static readonly long uintSize = 32L << (int)((~uint(0L) >> (int)(63L)));

        private static readonly var maxShift = uintSize - 4L;

        // Binary shift right (/ 2) by k bits.  k <= maxShift to avoid overflow.


        // Binary shift right (/ 2) by k bits.  k <= maxShift to avoid overflow.
        private static void rightShift(ref decimal a, ulong k)
        {
            long r = 0L; // read pointer
            long w = 0L; // write pointer

            // Pick up enough leading digits to cover first shift.
            ulong n = default;
            while (n >> (int)(k) == 0L)
            {
                if (r >= a.nd)
                {
                    if (n == 0L)
                    { 
                        // a == 0; shouldn't get here, but handle anyway.
                        a.nd = 0L;
                        return;
                r++;
                    }
                    while (n >> (int)(k) == 0L)
                    {
                        n = n * 10L;
                        r++;
                    }

                    break;
                }
                var c = uint(a.d[r]);
                n = n * 10L + c - '0';
            }

            a.dp -= r - 1L;

            ulong mask = (1L << (int)(k)) - 1L; 

            // Pick up a digit, put down a digit.
            while (r < a.nd)
            {
                c = uint(a.d[r]);
                var dig = n >> (int)(k);
                n &= mask;
                a.d[w] = byte(dig + '0');
                w++;
                n = n * 10L + c - '0';
                r++;
            } 

            // Put down extra digits.
 

            // Put down extra digits.
            while (n > 0L)
            {
                dig = n >> (int)(k);
                n &= mask;
                if (w < len(a.d))
                {
                    a.d[w] = byte(dig + '0');
                    w++;
                }
                else if (dig > 0L)
                {
                    a.trunc = true;
                }
                n = n * 10L;
            }


            a.nd = w;
            trim(a);
        }

        // Cheat sheet for left shift: table indexed by shift count giving
        // number of new digits that will be introduced by that shift.
        //
        // For example, leftcheats[4] = {2, "625"}.  That means that
        // if we are shifting by 4 (multiplying by 16), it will add 2 digits
        // when the string prefix is "625" through "999", and one fewer digit
        // if the string prefix is "000" through "624".
        //
        // Credit for this trick goes to Ken.

        private partial struct leftCheat
        {
            public long delta; // number of new digits
            public @string cutoff; // minus one digit if original < a.
        }

        private static leftCheat leftcheats = new slice<leftCheat>(new leftCheat[] { {0,""}, {1,"5"}, {1,"25"}, {1,"125"}, {2,"625"}, {2,"3125"}, {2,"15625"}, {3,"78125"}, {3,"390625"}, {3,"1953125"}, {4,"9765625"}, {4,"48828125"}, {4,"244140625"}, {4,"1220703125"}, {5,"6103515625"}, {5,"30517578125"}, {5,"152587890625"}, {6,"762939453125"}, {6,"3814697265625"}, {6,"19073486328125"}, {7,"95367431640625"}, {7,"476837158203125"}, {7,"2384185791015625"}, {7,"11920928955078125"}, {8,"59604644775390625"}, {8,"298023223876953125"}, {8,"1490116119384765625"}, {9,"7450580596923828125"}, {9,"37252902984619140625"}, {9,"186264514923095703125"}, {10,"931322574615478515625"}, {10,"4656612873077392578125"}, {10,"23283064365386962890625"}, {10,"116415321826934814453125"}, {11,"582076609134674072265625"}, {11,"2910383045673370361328125"}, {11,"14551915228366851806640625"}, {12,"72759576141834259033203125"}, {12,"363797880709171295166015625"}, {12,"1818989403545856475830078125"}, {13,"9094947017729282379150390625"}, {13,"45474735088646411895751953125"}, {13,"227373675443232059478759765625"}, {13,"1136868377216160297393798828125"}, {14,"5684341886080801486968994140625"}, {14,"28421709430404007434844970703125"}, {14,"142108547152020037174224853515625"}, {15,"710542735760100185871124267578125"}, {15,"3552713678800500929355621337890625"}, {15,"17763568394002504646778106689453125"}, {16,"88817841970012523233890533447265625"}, {16,"444089209850062616169452667236328125"}, {16,"2220446049250313080847263336181640625"}, {16,"11102230246251565404236316680908203125"}, {17,"55511151231257827021181583404541015625"}, {17,"277555756156289135105907917022705078125"}, {17,"1387778780781445675529539585113525390625"}, {18,"6938893903907228377647697925567626953125"}, {18,"34694469519536141888238489627838134765625"}, {18,"173472347597680709441192448139190673828125"}, {19,"867361737988403547205962240695953369140625"} });

        // Is the leading prefix of b lexicographically less than s?
        private static bool prefixIsLessThan(slice<byte> b, @string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (i >= len(b))
                {
                    return true;
                }
                if (b[i] != s[i])
                {
                    return b[i] < s[i];
                }
            }

            return false;
        }

        // Binary shift left (* 2) by k bits.  k <= maxShift to avoid overflow.
        private static void leftShift(ref decimal a, ulong k)
        {
            var delta = leftcheats[k].delta;
            if (prefixIsLessThan(a.d[0L..a.nd], leftcheats[k].cutoff))
            {
                delta--;
            }
            var r = a.nd; // read index
            var w = a.nd + delta; // write index

            // Pick up a digit, put down a digit.
            ulong n = default;
            r--;

            while (r >= 0L)
            {
                n += (uint(a.d[r]) - '0') << (int)(k);
                var quo = n / 10L;
                var rem = n - 10L * quo;
                w--;
                if (w < len(a.d))
                {
                    a.d[w] = byte(rem + '0');
                r--;
                }
                else if (rem != 0L)
                {
                    a.trunc = true;
                }
                n = quo;
            } 

            // Put down extra digits.
 

            // Put down extra digits.
            while (n > 0L)
            {
                quo = n / 10L;
                rem = n - 10L * quo;
                w--;
                if (w < len(a.d))
                {
                    a.d[w] = byte(rem + '0');
                }
                else if (rem != 0L)
                {
                    a.trunc = true;
                }
                n = quo;
            }


            a.nd += delta;
            if (a.nd >= len(a.d))
            {
                a.nd = len(a.d);
            }
            a.dp += delta;
            trim(a);
        }

        // Binary shift left (k > 0) or right (k < 0).
        private static void Shift(this ref decimal a, long k)
        {

            if (a.nd == 0L)             else if (k > 0L) 
                while (k > maxShift)
                {
                    leftShift(a, maxShift);
                    k -= maxShift;
                }

                leftShift(a, uint(k));
            else if (k < 0L) 
                while (k < -maxShift)
                {
                    rightShift(a, maxShift);
                    k += maxShift;
                }

                rightShift(a, uint(-k));
                    }

        // If we chop a at nd digits, should we round up?
        private static bool shouldRoundUp(ref decimal a, long nd)
        {
            if (nd < 0L || nd >= a.nd)
            {
                return false;
            }
            if (a.d[nd] == '5' && nd + 1L == a.nd)
            { // exactly halfway - round to even
                // if we truncated, a little higher than what's recorded - always round up
                if (a.trunc)
                {
                    return true;
                }
                return nd > 0L && (a.d[nd - 1L] - '0') % 2L != 0L;
            } 
            // not halfway - digit tells all
            return a.d[nd] >= '5';
        }

        // Round a to nd digits (or fewer).
        // If nd is zero, it means we're rounding
        // just to the left of the digits, as in
        // 0.09 -> 0.1.
        private static void Round(this ref decimal a, long nd)
        {
            if (nd < 0L || nd >= a.nd)
            {
                return;
            }
            if (shouldRoundUp(a, nd))
            {
                a.RoundUp(nd);
            }
            else
            {
                a.RoundDown(nd);
            }
        }

        // Round a down to nd digits (or fewer).
        private static void RoundDown(this ref decimal a, long nd)
        {
            if (nd < 0L || nd >= a.nd)
            {
                return;
            }
            a.nd = nd;
            trim(a);
        }

        // Round a up to nd digits (or fewer).
        private static void RoundUp(this ref decimal a, long nd)
        {
            if (nd < 0L || nd >= a.nd)
            {
                return;
            } 

            // round up
            for (var i = nd - 1L; i >= 0L; i--)
            {
                var c = a.d[i];
                if (c < '9')
                { // can stop after this digit
                    a.d[i]++;
                    a.nd = i + 1L;
                    return;
                }
            } 

            // Number is all 9s.
            // Change to single 1 with adjusted decimal point.
 

            // Number is all 9s.
            // Change to single 1 with adjusted decimal point.
            a.d[0L] = '1';
            a.nd = 1L;
            a.dp++;
        }

        // Extract integer part, rounded appropriately.
        // No guarantees about overflow.
        private static ulong RoundedInteger(this ref decimal a)
        {
            if (a.dp > 20L)
            {
                return 0xFFFFFFFFFFFFFFFFUL;
            }
            long i = default;
            var n = uint64(0L);
            for (i = 0L; i < a.dp && i < a.nd; i++)
            {
                n = n * 10L + uint64(a.d[i] - '0');
            }

            while (i < a.dp)
            {
                n *= 10L;
                i++;
            }

            if (shouldRoundUp(a, a.dp))
            {
                n++;
            }
            return n;
        }
    }
}
