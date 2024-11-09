using System;

namespace go.math;

public static partial class rand_package
{
    // Intn returns, as an int, a non-negative pseudo-random number in the half-open interval [0,n)
    // from the default [Source].
    // It panics if n <= 0.
    public static int Intn(int n)
    {
        if (n <= 0)
            panic("invalid argument to Intn");

        return Random.Shared.Next(n); // Returns a random integer in [0, n)
    }
}
