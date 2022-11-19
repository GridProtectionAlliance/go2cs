// _Switch statements_ express conditionals across many
// branches.

namespace go;

using fmt = fmt_package;
using time = time_package;

public static partial class main_package {

private static nint x = 1;

private static nint getNext() {
    x++;
    return x;
}

private static void Main() {
    // Here's a basic `switch`.
    nint i = 2;
    fmt.Print("Write "u8, i, " as "u8);
    switch (i) { // Intra-switch comment
        case 1: // Case 1 comment
            fmt.Println("one"u8); /* Case 1 eol comment */
            break;
        case 2: // Case 2 comment
            // Comment before
            fmt.Println("two"u8); // Case 2 eol comment
            // Comment after
            break;
        case 3: 
            { // Start of block comment
                // Before statement comment
                fmt.Println("three"u8); // eol comment
                // After statement comment
            }
            break;
        case 4: 
            // Comment before
            fmt.Println("four"u8); // Case 2 eol comment
            // Comment after
            break;
        default: // Default case comment
            // Comment before
            fmt.Println("unknown"u8); // Default case eol comment
            // Comment after
            break;
    } // End of switch comment

    // You can use commas to separate multiple expressions
    // in the same `case` statement. We use the optional
    // `default` case in this example as well.
    // Intra-switch comment
    if (time.Now().Weekday() == time.Saturday || time.Now().Weekday() == time.Sunday) // Case Sat/Sun comment
        // Weekend comment
        fmt.Println("It's the weekend"u8);
    else if (time.Now().Weekday() == time.Monday) // Case Mon comment
        // Pre-Monday comment
        fmt.Println("Ugh, it's Monday"u8); 
        // Post-Monday comment
    else // Case default comment
        // Pre-default comment
        fmt.Println("It's a weekday"u8); 
        // Post-default comment
    // End of switch comment

    // `switch` without an expression is an alternate way
    // to express if/else logic. Here we also show how the
    // `case` expressions can be non-constants.
    var t = time.Now();
    // Intra-switch comment
    if (t.Hour() < 12) // Before noon
        fmt.Println("It's before noon"u8);
    else // After noon
        fmt.Println("It's after noon"u8);
    // End of switch comment

    // "i" before should be saved
    fmt.Printf("i before = %d\n"u8, i); 

    // Here is a switch with simple statement and a redeclared identifier plus a fallthrough
    {
        nint i__prev1 = i;

        i = 1;

        // Intra-switch comment
        if (getNext() == -1)
        {
            fmt.Println("negative"u8);
            goto __switch_break0;
        }
        if (getNext() == 0) // Single-value comment
        {
            // Before zero comment
            fmt.Println("zero"u8); 
            // After zero comment
            goto __switch_break0;
        }
        if (getNext() == 1 || getNext() == 2) // Multi-value comment
        {
            // Before one-or-two comment
            fmt.Println("one or two"u8); // eol comment
            // After one-or-two comment
            fallthrough = true; // fallthrough comment
        }
        if (fallthrough || getNext() == 3)
        {
            fmt.Printf("three, but x=%d and i now = %d\n"u8, x, i);
        }
        // default: // Default case comment
            // Pre-default-op comments
            fmt.Println("plus, always a default here because of fallthrough"u8); // eol comment
            // Post-default-op comments

        __switch_break0:;

        i = i__prev1;
    } // end of switch comment

    // "i" after should be restored
    fmt.Printf("i after = %d\n"u8, i);
}

} // end main_package
