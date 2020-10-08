// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package big -- go2cs converted at 2020 October 08 03:25:49 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\prime.go
using rand = go.math.rand_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        // ProbablyPrime reports whether x is probably prime,
        // applying the Miller-Rabin test with n pseudorandomly chosen bases
        // as well as a Baillie-PSW test.
        //
        // If x is prime, ProbablyPrime returns true.
        // If x is chosen randomly and not prime, ProbablyPrime probably returns false.
        // The probability of returning true for a randomly chosen non-prime is at most ¼ⁿ.
        //
        // ProbablyPrime is 100% accurate for inputs less than 2⁶⁴.
        // See Menezes et al., Handbook of Applied Cryptography, 1997, pp. 145-149,
        // and FIPS 186-4 Appendix F for further discussion of the error probabilities.
        //
        // ProbablyPrime is not suitable for judging primes that an adversary may
        // have crafted to fool the test.
        //
        // As of Go 1.8, ProbablyPrime(0) is allowed and applies only a Baillie-PSW test.
        // Before Go 1.8, ProbablyPrime applied only the Miller-Rabin tests, and ProbablyPrime(0) panicked.
        private static bool ProbablyPrime(this ptr<Int> _addr_x, long n) => func((_, panic, __) =>
        {
            ref Int x = ref _addr_x.val;
 
            // Note regarding the doc comment above:
            // It would be more precise to say that the Baillie-PSW test uses the
            // extra strong Lucas test as its Lucas test, but since no one knows
            // how to tell any of the Lucas tests apart inside a Baillie-PSW test
            // (they all work equally well empirically), that detail need not be
            // documented or implicitly guaranteed.
            // The comment does avoid saying "the" Baillie-PSW test
            // because of this general ambiguity.

            if (n < 0L)
            {
                panic("negative n for ProbablyPrime");
            }
            if (x.neg || len(x.abs) == 0L)
            {
                return false;
            }
            const ulong primeBitMask = (ulong)1L << (int)(2L) | 1L << (int)(3L) | 1L << (int)(5L) | 1L << (int)(7L) | 1L << (int)(11L) | 1L << (int)(13L) | 1L << (int)(17L) | 1L << (int)(19L) | 1L << (int)(23L) | 1L << (int)(29L) | 1L << (int)(31L) | 1L << (int)(37L) | 1L << (int)(41L) | 1L << (int)(43L) | 1L << (int)(47L) | 1L << (int)(53L) | 1L << (int)(59L) | 1L << (int)(61L);



            var w = x.abs[0L];
            if (len(x.abs) == 1L && w < 64L)
            {
                return primeBitMask & (1L << (int)(w)) != 0L;
            }
            if (w & 1L == 0L)
            {
                return false; // x is even
            }
            const long primesA = (long)3L * 5L * 7L * 11L * 13L * 17L * 19L * 23L * 37L;

            const long primesB = (long)29L * 31L * 41L * 43L * 47L * 53L;



            uint rA = default;            uint rB = default;

            switch (_W)
            {
                case 32L: 
                    rA = uint32(x.abs.modW(primesA));
                    rB = uint32(x.abs.modW(primesB));
                    break;
                case 64L: 
                    var r = x.abs.modW((primesA * primesB) & _M);
                    rA = uint32(r % primesA);
                    rB = uint32(r % primesB);
                    break;
                default: 
                    panic("math/big: invalid word size");
                    break;
            }

            if (rA % 3L == 0L || rA % 5L == 0L || rA % 7L == 0L || rA % 11L == 0L || rA % 13L == 0L || rA % 17L == 0L || rA % 19L == 0L || rA % 23L == 0L || rA % 37L == 0L || rB % 29L == 0L || rB % 31L == 0L || rB % 41L == 0L || rB % 43L == 0L || rB % 47L == 0L || rB % 53L == 0L)
            {
                return false;
            }
            return x.abs.probablyPrimeMillerRabin(n + 1L, true) && x.abs.probablyPrimeLucas();

        });

        // probablyPrimeMillerRabin reports whether n passes reps rounds of the
        // Miller-Rabin primality test, using pseudo-randomly chosen bases.
        // If force2 is true, one of the rounds is forced to use base 2.
        // See Handbook of Applied Cryptography, p. 139, Algorithm 4.24.
        // The number n is known to be non-zero.
        private static bool probablyPrimeMillerRabin(this nat n, long reps, bool force2)
        {
            var nm1 = nat(null).sub(n, natOne); 
            // determine q, k such that nm1 = q << k
            var k = nm1.trailingZeroBits();
            var q = nat(null).shr(nm1, k);

            var nm3 = nat(null).sub(nm1, natTwo);
            var rand = rand.New(rand.NewSource(int64(n[0L])));

            nat x = default;            nat y = default;            nat quotient = default;

            var nm3Len = nm3.bitLen();

NextRandom:

            for (long i = 0L; i < reps; i++)
            {
                if (i == reps - 1L && force2)
                {
                    x = x.set(natTwo);
                }
                else
                {
                    x = x.random(rand, nm3, nm3Len);
                    x = x.add(x, natTwo);
                }

                y = y.expNN(x, q, n);
                if (y.cmp(natOne) == 0L || y.cmp(nm1) == 0L)
                {
                    continue;
                }

                for (var j = uint(1L); j < k; j++)
                {
                    y = y.sqr(y);
                    quotient, y = quotient.div(y, y, n);
                    if (y.cmp(nm1) == 0L)
                    {
                        _continueNextRandom = true;
                        break;
                    }

                    if (y.cmp(natOne) == 0L)
                    {
                        return false;
                    }

                }

                return false;

            }

            return true;

        }

        // probablyPrimeLucas reports whether n passes the "almost extra strong" Lucas probable prime test,
        // using Baillie-OEIS parameter selection. This corresponds to "AESLPSP" on Jacobsen's tables (link below).
        // The combination of this test and a Miller-Rabin/Fermat test with base 2 gives a Baillie-PSW test.
        //
        // References:
        //
        // Baillie and Wagstaff, "Lucas Pseudoprimes", Mathematics of Computation 35(152),
        // October 1980, pp. 1391-1417, especially page 1401.
        // https://www.ams.org/journals/mcom/1980-35-152/S0025-5718-1980-0583518-6/S0025-5718-1980-0583518-6.pdf
        //
        // Grantham, "Frobenius Pseudoprimes", Mathematics of Computation 70(234),
        // March 2000, pp. 873-891.
        // https://www.ams.org/journals/mcom/2001-70-234/S0025-5718-00-01197-2/S0025-5718-00-01197-2.pdf
        //
        // Baillie, "Extra strong Lucas pseudoprimes", OEIS A217719, https://oeis.org/A217719.
        //
        // Jacobsen, "Pseudoprime Statistics, Tables, and Data", http://ntheory.org/pseudoprimes.html.
        //
        // Nicely, "The Baillie-PSW Primality Test", http://www.trnicely.net/misc/bpsw.html.
        // (Note that Nicely's definition of the "extra strong" test gives the wrong Jacobi condition,
        // as pointed out by Jacobsen.)
        //
        // Crandall and Pomerance, Prime Numbers: A Computational Perspective, 2nd ed.
        // Springer, 2005.
        private static bool probablyPrimeLucas(this nat n) => func((_, panic, __) =>
        { 
            // Discard 0, 1.
            if (len(n) == 0L || n.cmp(natOne) == 0L)
            {
                return false;
            } 
            // Two is the only even prime.
            // Already checked by caller, but here to allow testing in isolation.
            if (n[0L] & 1L == 0L)
            {
                return n.cmp(natTwo) == 0L;
            } 

            // Baillie-OEIS "method C" for choosing D, P, Q,
            // as in https://oeis.org/A217719/a217719.txt:
            // try increasing P ≥ 3 such that D = P² - 4 (so Q = 1)
            // until Jacobi(D, n) = -1.
            // The search is expected to succeed for non-square n after just a few trials.
            // After more than expected failures, check whether n is square
            // (which would cause Jacobi(D, n) = 1 for all D not dividing n).
            var p = Word(3L);
            nat d = new nat(1);
            var t1 = nat(null); // temp
            ptr<Int> intD = addr(new Int(abs:d));
            ptr<Int> intN = addr(new Int(abs:n));
            while (i >= 0L)
            {
                if (p > 10000L)
                { 
                    // This is widely believed to be impossible.
                    // If we get a report, we'll want the exact number n.
                    panic("math/big: internal error: cannot find (D/n) = -1 for " + intN.String());
                p++;
                }

                d[0L] = p * p - 4L;
                var j = Jacobi(intD, intN);
                if (j == -1L)
                {
                    break;
                }

                if (j == 0L)
                { 
                    // d = p²-4 = (p-2)(p+2).
                    // If (d/n) == 0 then d shares a prime factor with n.
                    // Since the loop proceeds in increasing p and starts with p-2==1,
                    // the shared prime factor must be p+2.
                    // If p+2 == n, then n is prime; otherwise p+2 is a proper factor of n.
                    return len(n) == 1L && n[0L] == p + 2L;

                }

                if (p == 40L)
                { 
                    // We'll never find (d/n) = -1 if n is a square.
                    // If n is a non-square we expect to find a d in just a few attempts on average.
                    // After 40 attempts, take a moment to check if n is indeed a square.
                    t1 = t1.sqrt(n);
                    t1 = t1.sqr(t1);
                    if (t1.cmp(n) == 0L)
                    {
                        return false;
                    }

                }

            } 

            // Grantham definition of "extra strong Lucas pseudoprime", after Thm 2.3 on p. 876
            // (D, P, Q above have become Δ, b, 1):
            //
            // Let U_n = U_n(b, 1), V_n = V_n(b, 1), and Δ = b²-4.
            // An extra strong Lucas pseudoprime to base b is a composite n = 2^r s + Jacobi(Δ, n),
            // where s is odd and gcd(n, 2*Δ) = 1, such that either (i) U_s ≡ 0 mod n and V_s ≡ ±2 mod n,
            // or (ii) V_{2^t s} ≡ 0 mod n for some 0 ≤ t < r-1.
            //
            // We know gcd(n, Δ) = 1 or else we'd have found Jacobi(d, n) == 0 above.
            // We know gcd(n, 2) = 1 because n is odd.
            //
            // Arrange s = (n - Jacobi(Δ, n)) / 2^r = (n+1) / 2^r.
 

            // Grantham definition of "extra strong Lucas pseudoprime", after Thm 2.3 on p. 876
            // (D, P, Q above have become Δ, b, 1):
            //
            // Let U_n = U_n(b, 1), V_n = V_n(b, 1), and Δ = b²-4.
            // An extra strong Lucas pseudoprime to base b is a composite n = 2^r s + Jacobi(Δ, n),
            // where s is odd and gcd(n, 2*Δ) = 1, such that either (i) U_s ≡ 0 mod n and V_s ≡ ±2 mod n,
            // or (ii) V_{2^t s} ≡ 0 mod n for some 0 ≤ t < r-1.
            //
            // We know gcd(n, Δ) = 1 or else we'd have found Jacobi(d, n) == 0 above.
            // We know gcd(n, 2) = 1 because n is odd.
            //
            // Arrange s = (n - Jacobi(Δ, n)) / 2^r = (n+1) / 2^r.
            var s = nat(null).add(n, natOne);
            var r = int(s.trailingZeroBits());
            s = s.shr(s, uint(r));
            var nm2 = nat(null).sub(n, natTwo); // n-2

            // We apply the "almost extra strong" test, which checks the above conditions
            // except for U_s ≡ 0 mod n, which allows us to avoid computing any U_k values.
            // Jacobsen points out that maybe we should just do the full extra strong test:
            // "It is also possible to recover U_n using Crandall and Pomerance equation 3.13:
            // U_n = D^-1 (2V_{n+1} - PV_n) allowing us to run the full extra-strong test
            // at the cost of a single modular inversion. This computation is easy and fast in GMP,
            // so we can get the full extra-strong test at essentially the same performance as the
            // almost extra strong test."

            // Compute Lucas sequence V_s(b, 1), where:
            //
            //    V(0) = 2
            //    V(1) = P
            //    V(k) = P V(k-1) - Q V(k-2).
            //
            // (Remember that due to method C above, P = b, Q = 1.)
            //
            // In general V(k) = α^k + β^k, where α and β are roots of x² - Px + Q.
            // Crandall and Pomerance (p.147) observe that for 0 ≤ j ≤ k,
            //
            //    V(j+k) = V(j)V(k) - V(k-j).
            //
            // So in particular, to quickly double the subscript:
            //
            //    V(2k) = V(k)² - 2
            //    V(2k+1) = V(k) V(k+1) - P
            //
            // We can therefore start with k=0 and build up to k=s in log₂(s) steps.
            var natP = nat(null).setWord(p);
            var vk = nat(null).setWord(2L);
            var vk1 = nat(null).setWord(p);
            var t2 = nat(null); // temp
            for (var i = int(s.bitLen()); i >= 0L; i--)
            {
                if (s.bit(uint(i)) != 0L)
                { 
                    // k' = 2k+1
                    // V(k') = V(2k+1) = V(k) V(k+1) - P.
                    t1 = t1.mul(vk, vk1);
                    t1 = t1.add(t1, n);
                    t1 = t1.sub(t1, natP);
                    t2, vk = t2.div(vk, t1, n); 
                    // V(k'+1) = V(2k+2) = V(k+1)² - 2.
                    t1 = t1.sqr(vk1);
                    t1 = t1.add(t1, nm2);
                    t2, vk1 = t2.div(vk1, t1, n);

                }
                else
                { 
                    // k' = 2k
                    // V(k'+1) = V(2k+1) = V(k) V(k+1) - P.
                    t1 = t1.mul(vk, vk1);
                    t1 = t1.add(t1, n);
                    t1 = t1.sub(t1, natP);
                    t2, vk1 = t2.div(vk1, t1, n); 
                    // V(k') = V(2k) = V(k)² - 2
                    t1 = t1.sqr(vk);
                    t1 = t1.add(t1, nm2);
                    t2, vk = t2.div(vk, t1, n);

                }

            } 

            // Now k=s, so vk = V(s). Check V(s) ≡ ±2 (mod n).
 

            // Now k=s, so vk = V(s). Check V(s) ≡ ±2 (mod n).
            if (vk.cmp(natTwo) == 0L || vk.cmp(nm2) == 0L)
            { 
                // Check U(s) ≡ 0.
                // As suggested by Jacobsen, apply Crandall and Pomerance equation 3.13:
                //
                //    U(k) = D⁻¹ (2 V(k+1) - P V(k))
                //
                // Since we are checking for U(k) == 0 it suffices to check 2 V(k+1) == P V(k) mod n,
                // or P V(k) - 2 V(k+1) == 0 mod n.
                t1 = t1.mul(vk, natP);
                t2 = t2.shl(vk1, 1L);
                if (t1.cmp(t2) < 0L)
                {
                    t1 = t2;
                    t2 = t1;

                }

                t1 = t1.sub(t1, t2);
                var t3 = vk1; // steal vk1, no longer needed below
                vk1 = null;
                _ = vk1;
                t2, t3 = t2.div(t3, t1, n);
                if (len(t3) == 0L)
                {
                    return true;
                }

            } 

            // Check V(2^t s) ≡ 0 mod n for some 0 ≤ t < r-1.
            for (long t = 0L; t < r - 1L; t++)
            {
                if (len(vk) == 0L)
                { // vk == 0
                    return true;

                } 
                // Optimization: V(k) = 2 is a fixed point for V(k') = V(k)² - 2,
                // so if V(k) = 2, we can stop: we will never find a future V(k) == 0.
                if (len(vk) == 1L && vk[0L] == 2L)
                { // vk == 2
                    return false;

                } 
                // k' = 2k
                // V(k') = V(2k) = V(k)² - 2
                t1 = t1.sqr(vk);
                t1 = t1.sub(t1, natTwo);
                t2, vk = t2.div(vk, t1, n);

            }

            return false;

        });
    }
}}
