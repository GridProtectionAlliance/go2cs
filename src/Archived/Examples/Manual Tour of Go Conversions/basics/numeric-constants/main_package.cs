/*
package main

import "fmt"

const (
    // Create a huge number by shifting a 1 bit left 100 places.
    // In other words, the binary number that is 1 followed by 100 zeroes.
    Big = 1 << 100
    // Shift it right again 99 places, so we end up with 1<<1, or 2.
    Small = Big >> 99
)

func needInt(x int) int { return x*10 + 1 }
func needFloat(x float64) float64 {
    return x * 0.1
}

func main() {
    fmt.Println(needInt(Small))
    fmt.Println(needFloat(Small))
    fmt.Println(needFloat(Big))
}
*/
#region source
using fmt = go.fmt_package;
using System.Numerics;

static partial class main_package
{
    // Create a huge number by shifting a 1 bit left 100 places.
    // In other words, the binary number that is 1 followed by 100 zeroes.
    public static BigInteger Big => /* 1 << 100 */ __init__Big();

    // Shift it right again 99 places, so we end up with 1<<1, or 2.
    public static BigInteger Small => /* Big >> 99 */ __init__Small();

    static int needInt(int x) { return x*10 + 1; }
    static double needFloat(double x)  {
        return x * 0.1;
    }

    static void Main() {
        fmt.Println(needInt(Small__i));
        fmt.Println(needFloat(Small__f64));
        fmt.Println(needFloat(Big__f64));
    }
}
#endregion
// BIG TAKEAWAY: Pre-compiled numeric constants need to provide all possible casts

// Also, providing a BigInteger value for constant allows this runtime operation too:
//fmt.Println(needInt((int)Small));
//fmt.Println(needFloat((double)Small));
//fmt.Println(needFloat((double)Big));
