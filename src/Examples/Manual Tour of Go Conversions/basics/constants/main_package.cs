/*
package main

import "fmt"

const Pi = 3.14

func main() {
    const World = "世界"
    fmt.Println("Hello", World)
    fmt.Println("Happy", Pi, "Day")

    const Truth = true
    fmt.Println("Go rules?", Truth)
}
*/
#region source
using fmt = go.fmt_package;

static class main_package
{
    // Conversion will need to determine "inferred" type for members
    const double Pi = 3.14;

    static void Main() {
        const string World = "世界";
        fmt.Println("Hello ", World);
        fmt.Println("Happy", Pi, "Day");

        const bool Truth = true;
        fmt.Println("Go rules?", Truth);
    }
}
#endregion
// Note that getting Chinese characters on console output requires a font capable
// of displaying Unicode characters, e.g., MS Gothic, plus UTF8 encoding:
//System.Console.OutputEncoding = Encoding.UTF8;
