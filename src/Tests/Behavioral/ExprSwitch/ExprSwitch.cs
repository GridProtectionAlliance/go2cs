// _Switch statements_ express conditionals across many
// branches.

using fmt = go.fmt_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static long x = 1L;

        private static long getNext()
        {
            x++;
            return x;
        }

        private static void Main()
        {
            // Here's a basic `switch`.
            long i = 2L;
            fmt.Print("Write ", i, " as ");
            switch (i)
            { // Intra-switch comment
                case 1L: // Case 1 comment
                    fmt.Println("one"); /* Case 1 eol comment */
                    break;
                case 2L: // Case 2 comment
                    // Comment before
                    fmt.Println("two"); // Case 2 eol comment
                    // Comment after
                    break;
                case 3L: 
                    { // Start of block comment
                        // Before statement comment
                        fmt.Println("three"); // eol comment
                        // After statement comment
                    } // End of block comment
                    break;
                case 4L: 
                    // Comment before
                    fmt.Println("four"); // Case 2 eol comment
                    // Comment after
                    break;
                default: // Default case comment
                    // Comment before
                    fmt.Println("unknown"); // Default case eol comment
                    // Comment after
                    break;
            } // End of switch comment

            // You can use commas to separate multiple expressions
            // in the same `case` statement. We use the optional
            // `default` case in this example as well.
            // Intra-switch comment
            if (time.Now().Weekday() == time.Saturday || time.Now().Weekday() == time.Sunday) // Case Sat/Sun comment
                // Weekend comment
                fmt.Println("It's the weekend");
            else if (time.Now().Weekday() == time.Monday) // Case Mon comment
                // Pre-Monday comment
                fmt.Println("Ugh, it's Monday"); 
                // Post-Monday comment
            else // Case default comment
                // Pre-default comment
                fmt.Println("It's a weekday"); 
                // Post-default comment
            // End of switch comment

            // `switch` without an expression is an alternate way
            // to express if/else logic. Here we also show how the
            // `case` expressions can be non-constants.
            var t = time.Now();
            // Intra-switch comment
            if (t.Hour() < 12L) // Before noon
                fmt.Println("It's before noon");
            else // After noon
                fmt.Println("It's after noon");
            // End of switch comment

            // "i" before should be saved
            fmt.Printf("i before = %d\n", i); 

            // Here is a switch with simple statement and a redeclared identifier plus a fallthrough
            {
                var i__prev1 = i;

                i = 1L;

                // Intra-switch comment
                if (getNext() == -1L)
                {
                    fmt.Println("negative");
                    goto __switch_break0;
                }
                if (getNext() == 0L) // Single-value comment
                {
                    // Before zero comment
                    fmt.Println("zero"); 
                    // After zero comment
                    goto __switch_break0;
                }
                if (getNext() == 1L || getNext() == 2L) // Multi-value comment
                {
                    // Before one-or-two comment
                    fmt.Println("one or two"); // eol comment
                    // After one-or-two comment
                    fallthrough = true; // fallthrough comment
                }
                if (fallthrough || getNext() == 3L)
                {
                    fmt.Printf("three, but x=%d and i now = %d\n", x, i);
                }
                // default: // Default case comment
                    // Pre-default-op comments
                    fmt.Println("plus, always a default here because of fallthrough"); // eol comment
                    // Post-default-op comments

                __switch_break0:;

                i = i__prev1;
            } // end of switch comment

            // "i" after should be restored
            fmt.Printf("i after = %d\n", i);
        }
    }
}
